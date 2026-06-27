namespace Shipeng.Util
{
    /// <summary>
    /// id 字符串辅助类。
    /// </summary>
    /// <remarks>
    /// <para>
    /// 主要用于处理数据库中常见的“多个 id 使用分隔符拼接保存”的字段，例如：
    /// 行政区划路径、主营品类、角色、菜单、权限、附件等。
    /// </para>
    /// <para>
    /// 默认使用英文逗号 <c>,</c> 作为分隔符；所有拆分方法都会自动去除首尾空白、
    /// 忽略空片段，并按需要去重。需要频繁判断是否包含某个 id 时，优先使用
    /// <see cref="ContainsId(string, string)"/>，它会逐段精确比较，避免
    /// <c>"12,13".Contains("1")</c> 这类误判，也避免为了单次判断构造集合。
    /// </para>
    /// </remarks>
    public static class IdStringHelper
    {
        /// <summary>
        /// 默认 id 分隔符。
        /// </summary>
        private const char DefaultSeparator = ',';

        /// <summary>
        /// 拆分英文逗号分隔的 id 字符串，并返回去空、去重后的 <see cref="HashSet{T}"/>。
        /// </summary>
        /// <param name="value">英文逗号分隔的 id 字符串，例如 <c>1001, 1002,1001,,1003</c>。</param>
        /// <returns>去除空白、过滤空值、去重后的 id 集合。</returns>
        public static HashSet<string> SplitToHashSet(string value)
        {
            return SplitToHashSet(value, DefaultSeparator, StringComparer.Ordinal);
        }

        /// <summary>
        /// 按指定分隔符拆分 id 字符串，并返回去空、去重后的 <see cref="HashSet{T}"/>。
        /// </summary>
        /// <param name="value">分隔符拼接的 id 字符串。</param>
        /// <param name="separator">分隔符。</param>
        /// <returns>去除空白、过滤空值、去重后的 id 集合。</returns>
        public static HashSet<string> SplitToHashSet(string value, char separator)
        {
            return SplitToHashSet(value, separator, StringComparer.Ordinal);
        }

        /// <summary>
        /// 按指定分隔符和比较规则拆分 id 字符串，并返回去空、去重后的 <see cref="HashSet{T}"/>。
        /// </summary>
        /// <param name="value">分隔符拼接的 id 字符串。</param>
        /// <param name="separator">分隔符。</param>
        /// <param name="comparer">id 去重和匹配时使用的字符串比较器；传 null 时使用 <see cref="StringComparer.Ordinal"/>。</param>
        /// <returns>去除空白、过滤空值、去重后的 id 集合。</returns>
        public static HashSet<string> SplitToHashSet(string value, char separator, IEqualityComparer<string> comparer)
        {
            var result = new HashSet<string>(comparer ?? StringComparer.Ordinal);
            AddSegments(value, separator, result, null);
            return result;
        }

        /// <summary>
        /// 拆分英文逗号分隔的 id 字符串，并返回去空、去重且保留首次出现顺序的列表。
        /// </summary>
        /// <param name="value">英文逗号分隔的 id 字符串。</param>
        /// <returns>去除空白、过滤空值、去重后的 id 列表。</returns>
        public static List<string> SplitToList(string value)
        {
            return SplitToList(value, DefaultSeparator, StringComparer.Ordinal);
        }

        /// <summary>
        /// 按指定分隔符拆分 id 字符串，并返回去空、去重且保留首次出现顺序的列表。
        /// </summary>
        /// <param name="value">分隔符拼接的 id 字符串。</param>
        /// <param name="separator">分隔符。</param>
        /// <returns>去除空白、过滤空值、去重后的 id 列表。</returns>
        public static List<string> SplitToList(string value, char separator)
        {
            return SplitToList(value, separator, StringComparer.Ordinal);
        }

        /// <summary>
        /// 按指定分隔符和比较规则拆分 id 字符串，并返回去空、去重且保留首次出现顺序的列表。
        /// </summary>
        /// <param name="value">分隔符拼接的 id 字符串。</param>
        /// <param name="separator">分隔符。</param>
        /// <param name="comparer">id 去重和匹配时使用的字符串比较器；传 null 时使用 <see cref="StringComparer.Ordinal"/>。</param>
        /// <returns>去除空白、过滤空值、去重后的 id 列表。</returns>
        public static List<string> SplitToList(string value, char separator, IEqualityComparer<string> comparer)
        {
            var result = new List<string>();
            var set = new HashSet<string>(comparer ?? StringComparer.Ordinal);
            AddSegments(value, separator, set, result);
            return result;
        }

        /// <summary>
        /// 判断英文逗号分隔的 id 字符串中是否包含指定 id。
        /// </summary>
        /// <param name="value">英文逗号分隔的 id 字符串。</param>
        /// <param name="id">要判断的 id。</param>
        /// <returns>包含指定 id 返回 true，否则返回 false。</returns>
        public static bool ContainsId(string value, string id)
        {
            return ContainsId(value, id, DefaultSeparator, StringComparison.Ordinal);
        }

        /// <summary>
        /// 判断指定分隔符拼接的 id 字符串中是否包含指定 id。
        /// </summary>
        /// <param name="value">分隔符拼接的 id 字符串。</param>
        /// <param name="id">要判断的 id。</param>
        /// <param name="separator">分隔符。</param>
        /// <returns>包含指定 id 返回 true，否则返回 false。</returns>
        public static bool ContainsId(string value, string id, char separator)
        {
            return ContainsId(value, id, separator, StringComparison.Ordinal);
        }

        /// <summary>
        /// 判断指定分隔符拼接的 id 字符串中是否包含指定 id，并允许指定字符串比较规则。
        /// </summary>
        /// <param name="value">分隔符拼接的 id 字符串。</param>
        /// <param name="id">要判断的 id。</param>
        /// <param name="separator">分隔符。</param>
        /// <param name="comparison">字符串比较规则。</param>
        /// <returns>包含指定 id 返回 true，否则返回 false。</returns>
        public static bool ContainsId(string value, string id, char separator, StringComparison comparison)
        {
            if (string.IsNullOrWhiteSpace(value) || string.IsNullOrWhiteSpace(id))
            {
                return false;
            }

            string target = id.Trim();
            int start = 0;
            while (start <= value.Length)
            {
                int separatorIndex = value.IndexOf(separator, start);
                int end = separatorIndex < 0 ? value.Length : separatorIndex;
                string current = value.Substring(start, end - start).Trim();
                if (current.Length > 0 && string.Equals(current, target, comparison))
                {
                    return true;
                }

                if (separatorIndex < 0)
                {
                    break;
                }

                start = separatorIndex + 1;
            }

            return false;
        }

        /// <summary>
        /// 将 id 集合拼接成英文逗号分隔字符串。
        /// </summary>
        /// <param name="ids">id 集合。</param>
        /// <returns>去空、去重并保留首次出现顺序后的分隔符字符串。</returns>
        public static string JoinIds(IEnumerable<string> ids)
        {
            return JoinIds(ids, DefaultSeparator, StringComparer.Ordinal);
        }

        /// <summary>
        /// 将 id 集合按指定分隔符拼接成字符串。
        /// </summary>
        /// <param name="ids">id 集合。</param>
        /// <param name="separator">分隔符。</param>
        /// <returns>去空、去重并保留首次出现顺序后的分隔符字符串。</returns>
        public static string JoinIds(IEnumerable<string> ids, char separator)
        {
            return JoinIds(ids, separator, StringComparer.Ordinal);
        }

        /// <summary>
        /// 将 id 集合按指定分隔符和比较规则拼接成字符串。
        /// </summary>
        /// <param name="ids">id 集合。</param>
        /// <param name="separator">分隔符。</param>
        /// <param name="comparer">id 去重时使用的字符串比较器；传 null 时使用 <see cref="StringComparer.Ordinal"/>。</param>
        /// <returns>去空、去重并保留首次出现顺序后的分隔符字符串。</returns>
        public static string JoinIds(IEnumerable<string> ids, char separator, IEqualityComparer<string> comparer)
        {
            if (ids == null)
            {
                return string.Empty;
            }

            var result = new List<string>();
            var set = new HashSet<string>(comparer ?? StringComparer.Ordinal);
            foreach (string item in ids)
            {
                string id = item?.Trim();
                if (string.IsNullOrWhiteSpace(id))
                {
                    continue;
                }

                if (set.Add(id))
                {
                    result.Add(id);
                }
            }

            return string.Join(separator, result);
        }

        /// <summary>
        /// 将分隔符字符串解析为片段，并按需写入集合和有序列表。
        /// </summary>
        /// <param name="value">待解析的字符串。</param>
        /// <param name="separator">分隔符。</param>
        /// <param name="set">用于去重的集合。</param>
        /// <param name="orderedResult">需要保留顺序时传入列表；只需要集合时传 null。</param>
        private static void AddSegments(string value, char separator, HashSet<string> set, List<string> orderedResult)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            int start = 0;
            while (start <= value.Length)
            {
                int separatorIndex = value.IndexOf(separator, start);
                int end = separatorIndex < 0 ? value.Length : separatorIndex;
                string id = value.Substring(start, end - start).Trim();
                if (id.Length > 0 && set.Add(id))
                {
                    orderedResult?.Add(id);
                }

                if (separatorIndex < 0)
                {
                    break;
                }

                start = separatorIndex + 1;
            }
        }
    }
}
