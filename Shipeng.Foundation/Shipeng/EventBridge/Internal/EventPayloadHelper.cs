using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using Shipeng.Extensions;
using Shipeng.FriendlyException;
using Shipeng.JsonSerialization;

namespace Shipeng.EventBridge
{
    /// <summary>
    /// 事件负载处理辅助类。
    ///
    /// 放置位置：
    /// src/Infrastructure/Trace/EventBridge/Internal/EventPayloadHelper.cs
    ///
    /// 作用：
    /// 1. 统一处理事件 payload 的序列化。
    /// 2. 统一处理事件 payload 的反序列化。
    /// 3. 统一兼容 Tuple 和 ValueTuple。
    /// 4. 统一兼容发送端 payload 类型和接收端 EventMessage&lt;TPayload&gt; 类型不一致的情况。
    ///
    /// 为什么要单独封装：
    /// 1. Event.cs 只应该负责事件发布流程，不应该写 Tuple/JSON 细节。
    /// 2. EventDispatcher.cs 只应该负责分发，不应该写复杂类型转换细节。
    /// 3. 以后事件负载规则变化，只改这一个类。
    ///
    /// 支持的 Payload 格式：
    /// 1. 简单值：string、int、decimal、bool、DateTime、Guid、枚举。
    /// 2. 普通对象：Entity、DTO、匿名对象。
    /// 3. Tuple：Tuple.Create(entity, null, null)。
    /// 4. ValueTuple：(organize: entity, adopter: null, contractor: null)。
    ///
    /// 注意：
    /// 1. 该类只服务于事件总线内部，所以使用 internal。
    /// 2. 业务模块不要直接调用这个类。
    /// </summary>
    internal static class EventPayloadHelper
    {
        /// <summary>
        /// 序列化事件负载。
        ///
        /// 规则：
        /// 1. 简单类型直接 ToString，减少 JSON 序列化开销。
        /// 2. 复杂类型统一 JSON 序列化。
        /// 3. Tuple / ValueTuple 必须走 JSON，否则 ToString 后无法稳定还原。
        /// </summary>
        /// <param name="payload">事件负载对象</param>
        /// <param name="payloadType">事件负载运行时类型</param>
        /// <returns>序列化后的字符串</returns>
        public static string SerializePayload(object payload, Type payloadType)
        {
            if (payload == null)
                return null;

            if (IsSimplePayloadType(payloadType))
                return payload.ToString();

            return JSON.Serialize(payload);
        }

        /// <summary>
        /// 反序列化事件负载。
        ///
        /// 说明：
        /// 1. payloadType 是事件发布时记录的真实发送类型。
        /// 2. 如果是简单类型，直接转换。
        /// 3. 如果是 Tuple / ValueTuple，按 Item1、Item2、Item3... 手动还原。
        /// 4. 如果是普通对象，使用 JSON 反序列化。
        /// </summary>
        /// <param name="payload">序列化后的 payload 字符串</param>
        /// <param name="payloadType">事件发布时记录的 payload 类型</param>
        /// <returns>反序列化后的 payload 对象</returns>
        public static object DeserializePayload(string payload, Type payloadType)
        {
            if (payloadType == null)
                throw Tracexception.Oh("事件负载目标类型不能为空。");

            if (payload == null)
                return null;

            if (IsTupleLikeType(payloadType))
                return DeserializeTupleLikePayload(payload, payloadType);

            if (IsSimplePayloadType(payloadType))
                return ConvertValue(payload, payloadType);

            return typeof(JSON)
                .GetMethod("Deserialize")
                .MakeGenericMethod(payloadType)
                .Invoke(null, new object[] { payload, null, null });
        }

        /// <summary>
        /// 将事件实际负载转换为事件处理方法声明的目标负载类型。
        ///
        /// 使用场景：
        /// EventDispatcher 调用事件处理方法前，需要构造 EventMessage&lt;TPayload&gt;。
        /// 如果发送端 payload 类型和接收端 TPayload 类型不完全一致，就在这里兼容。
        ///
        /// 兼容范围：
        /// 1. 类型一致：直接返回。
        /// 2. Tuple 转 ValueTuple。
        /// 3. ValueTuple 转 Tuple。
        /// 4. Tuple 转 Tuple。
        /// 5. ValueTuple 转 ValueTuple。
        /// 6. 普通对象转普通对象：通过 JSON 二次转换。
        ///
        /// 示例：
        /// 发送端：
        /// await Event.EmitAsync("Merchant:SyncSaveMerchant",
        ///     (organize: entity, adopter: (AdopterEntity)null, contractor: (ContractorEntity)null));
        ///
        /// 接收端：
        /// public async Task SyncSaveMerchant(EventMessage&lt;(OrganizeEntity organize, AdopterEntity adopter, ContractorEntity contractor)&gt; eventPayload)
        ///
        /// 这种写法可以正常转换。
        /// </summary>
        /// <param name="sourcePayload">事件实际负载对象</param>
        /// <param name="targetPayloadType">事件处理方法声明的 TPayload 类型</param>
        /// <returns>转换后的 payload 对象</returns>
        public static object ConvertPayloadToTargetType(object sourcePayload, Type targetPayloadType)
        {
            if (targetPayloadType == null)
                return sourcePayload;

            if (sourcePayload == null)
                return CreateDefaultValue(targetPayloadType);

            var sourcePayloadType = sourcePayload.GetType();

            if (targetPayloadType.IsAssignableFrom(sourcePayloadType))
                return sourcePayload;

            if (IsTupleLikeType(sourcePayloadType) && IsTupleLikeType(targetPayloadType))
                return ConvertTupleLikeObject(sourcePayload, targetPayloadType);

            var json = JSON.Serialize(sourcePayload);

            return typeof(JSON)
                .GetMethod("Deserialize")
                .MakeGenericMethod(targetPayloadType)
                .Invoke(null, new object[] { json, null, null });
        }

        /// <summary>
        /// 判断是否为简单 Payload 类型。
        ///
        /// 简单类型可以直接 ToString 保存。
        ///
        /// 注意：
        /// 1. Nullable&lt;T&gt; 会先取出 T 再判断。
        /// 2. 枚举按简单类型处理。
        /// 3. ValueTuple 虽然是值类型，但不属于简单类型。
        /// </summary>
        /// <param name="type">要判断的类型</param>
        /// <returns>true：简单类型；false：复杂类型</returns>
        public static bool IsSimplePayloadType(Type type)
        {
            if (type == null)
                return true;

            type = Nullable.GetUnderlyingType(type) ?? type;

            if (type.IsEnum)
                return true;

            return type == typeof(string)
                || type == typeof(bool)
                || type == typeof(byte)
                || type == typeof(sbyte)
                || type == typeof(short)
                || type == typeof(ushort)
                || type == typeof(int)
                || type == typeof(uint)
                || type == typeof(long)
                || type == typeof(ulong)
                || type == typeof(float)
                || type == typeof(double)
                || type == typeof(decimal)
                || type == typeof(char)
                || type == typeof(DateTime)
                || type == typeof(DateTimeOffset)
                || type == typeof(Guid);
        }

        /// <summary>
        /// 判断是否为 Tuple 或 ValueTuple。
        /// </summary>
        /// <param name="type">要判断的类型</param>
        /// <returns>true：Tuple 或 ValueTuple；false：其它类型</returns>
        public static bool IsTupleLikeType(Type type)
        {
            return IsTupleType(type) || IsValueTupleType(type);
        }

        /// <summary>
        /// 反序列化 Tuple / ValueTuple。
        ///
        /// 说明：
        /// 1. Tuple 和 ValueTuple 序列化后都会有 Item1、Item2、Item3。
        /// 2. 这里按 Item 顺序逐个转换。
        /// 3. 这样可以避免 Newtonsoft 直接反序列化 ValueTuple 时出现转换异常。
        /// </summary>
        /// <param name="json">Tuple / ValueTuple 的 JSON 字符串</param>
        /// <param name="tupleType">目标 Tuple / ValueTuple 类型</param>
        /// <returns>Tuple / ValueTuple 对象</returns>
        public static object DeserializeTupleLikePayload(string json, Type tupleType)
        {
            if (string.IsNullOrWhiteSpace(json))
                return Activator.CreateInstance(tupleType);

            var jsonObject = JObject.Parse(json);
            var genericTypes = tupleType.GetGenericArguments();
            var args = new object[genericTypes.Length];

            for (var i = 0; i < genericTypes.Length; i++)
            {
                var itemName = $"Item{i + 1}";
                var itemType = genericTypes[i];
                var token = jsonObject[itemName];

                if (token == null || token.Type == JTokenType.Null)
                {
                    args[i] = CreateDefaultValue(itemType);
                    continue;
                }

                args[i] = token.ToObject(itemType);
            }

            return Activator.CreateInstance(tupleType, args);
        }

        /// <summary>
        /// Tuple / ValueTuple 之间按 Item 顺序互转。
        ///
        /// 支持：
        /// 1. Tuple 转 ValueTuple。
        /// 2. ValueTuple 转 Tuple。
        /// 3. Tuple 转 Tuple。
        /// 4. ValueTuple 转 ValueTuple。
        /// </summary>
        /// <param name="source">原始 Tuple / ValueTuple 对象</param>
        /// <param name="targetType">目标 Tuple / ValueTuple 类型</param>
        /// <returns>转换后的 Tuple / ValueTuple 对象</returns>
        private static object ConvertTupleLikeObject(object source, Type targetType)
        {
            var sourceValues = GetTupleLikeValues(source);
            var targetGenericTypes = targetType.GetGenericArguments();

            if (sourceValues.Count != targetGenericTypes.Length)
                throw Tracexception.Oh($"事件负载参数数量不匹配，当前数量：{sourceValues.Count}，目标数量：{targetGenericTypes.Length}");

            var args = new object[targetGenericTypes.Length];

            for (var i = 0; i < targetGenericTypes.Length; i++)
            {
                args[i] = ConvertValue(sourceValues[i], targetGenericTypes[i]);
            }

            return Activator.CreateInstance(targetType, args);
        }

        /// <summary>
        /// 获取 Tuple / ValueTuple 中 Item1、Item2、Item3... 的值。
        ///
        /// 说明：
        /// 1. System.Tuple 的 Item 是属性。
        /// 2. System.ValueTuple 的 Item 是字段。
        /// 3. 这里同时兼容属性和字段。
        /// </summary>
        /// <param name="source">Tuple / ValueTuple 对象</param>
        /// <returns>按顺序排列的 Item 值</returns>
        private static List<object> GetTupleLikeValues(object source)
        {
            var type = source.GetType();
            var values = new List<object>();

            for (var i = 1; i <= 8; i++)
            {
                var name = $"Item{i}";

                var property = type.GetProperty(name);
                if (property != null)
                {
                    values.Add(property.GetValue(source));
                    continue;
                }

                var field = type.GetField(name);
                if (field != null)
                {
                    values.Add(field.GetValue(source));
                    continue;
                }

                break;
            }

            return values;
        }

        /// <summary>
        /// 将一个值转换成目标类型。
        ///
        /// 这里比直接 ChangeType 更稳：
        /// 1. 支持 null。
        /// 2. 支持 Nullable&lt;T&gt;。
        /// 3. 支持枚举。
        /// 4. 支持复杂对象 JSON 转换。
        /// </summary>
        /// <param name="value">原始值</param>
        /// <param name="targetType">目标类型</param>
        /// <returns>转换后的值</returns>
        private static object ConvertValue(object value, Type targetType)
        {
            if (targetType == null)
                return value;

            if (value == null)
                return CreateDefaultValue(targetType);

            var realTargetType = Nullable.GetUnderlyingType(targetType) ?? targetType;

            if (realTargetType.IsAssignableFrom(value.GetType()))
                return value;

            if (realTargetType.IsEnum)
                return Enum.Parse(realTargetType, value.ToString());

            if (IsSimplePayloadType(realTargetType))
                return value.ChangeType(realTargetType);

            var json = JSON.Serialize(value);

            return typeof(JSON)
                .GetMethod("Deserialize")
                .MakeGenericMethod(realTargetType)
                .Invoke(null, new object[] { json, null, null });
        }

        /// <summary>
        /// 根据类型创建默认值。
        ///
        /// 规则：
        /// 1. 引用类型返回 null。
        /// 2. Nullable&lt;T&gt; 返回 null。
        /// 3. 普通值类型返回默认值，例如 int 返回 0。
        /// </summary>
        /// <param name="type">目标类型</param>
        /// <returns>默认值</returns>
        private static object CreateDefaultValue(Type type)
        {
            if (type == null)
                return null;

            if (!type.IsValueType)
                return null;

            if (Nullable.GetUnderlyingType(type) != null)
                return null;

            return Activator.CreateInstance(type);
        }

        /// <summary>
        /// 判断是否为 System.Tuple 类型。
        /// </summary>
        /// <param name="type">要判断的类型</param>
        /// <returns>true：Tuple；false：其它类型</returns>
        private static bool IsTupleType(Type type)
        {
            if (type == null || !type.IsGenericType)
                return false;

            var genericType = type.GetGenericTypeDefinition();

            return genericType == typeof(Tuple<>)
                || genericType == typeof(Tuple<,>)
                || genericType == typeof(Tuple<,,>)
                || genericType == typeof(Tuple<,,,>)
                || genericType == typeof(Tuple<,,,,>)
                || genericType == typeof(Tuple<,,,,,>)
                || genericType == typeof(Tuple<,,,,,,>)
                || genericType == typeof(Tuple<,,,,,,,>);
        }

        /// <summary>
        /// 判断是否为 System.ValueTuple 类型。
        /// </summary>
        /// <param name="type">要判断的类型</param>
        /// <returns>true：ValueTuple；false：其它类型</returns>
        private static bool IsValueTupleType(Type type)
        {
            if (type == null || !type.IsGenericType)
                return false;

            var genericType = type.GetGenericTypeDefinition();

            return genericType == typeof(ValueTuple<>)
                || genericType == typeof(ValueTuple<,>)
                || genericType == typeof(ValueTuple<,,>)
                || genericType == typeof(ValueTuple<,,,>)
                || genericType == typeof(ValueTuple<,,,,>)
                || genericType == typeof(ValueTuple<,,,,,>)
                || genericType == typeof(ValueTuple<,,,,,,>)
                || genericType == typeof(ValueTuple<,,,,,,,>);
        }
    }
}