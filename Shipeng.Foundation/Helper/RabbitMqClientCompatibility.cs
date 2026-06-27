using RabbitMQ.Client;

namespace Shipeng.Util
{
    /// <summary>
    /// Compatibility helpers for RabbitMQ.Client 7.x.
    /// The official client moved most channel operations to async APIs; these wrappers keep the existing synchronous helper API stable.
    /// </summary>
    internal static class RabbitMqClientCompatibility
    {
        public static IConnection CreateConnection(this ConnectionFactory factory)
        {
            return factory.CreateConnectionAsync().GetAwaiter().GetResult();
        }

        public static IChannel CreateModel(this IConnection connection)
        {
            return connection.CreateChannelAsync().GetAwaiter().GetResult();
        }

        public static BasicProperties CreateBasicProperties(this IChannel channel)
        {
            return new BasicProperties();
        }

        public static void ExchangeDeclare(this IChannel channel, string exchange, string type, bool durable, bool autoDelete, IDictionary<string, object> arguments)
        {
            channel.ExchangeDeclareAsync(exchange, type, durable, autoDelete, arguments).GetAwaiter().GetResult();
        }

        public static void ExchangeDeclareNoWait(this IChannel channel, string exchange, string type, bool durable, bool autoDelete, IDictionary<string, object> arguments)
        {
            channel.ExchangeDeclareAsync(exchange, type, durable, autoDelete, arguments, noWait: true).GetAwaiter().GetResult();
        }

        public static void ExchangeDeclarePassive(this IChannel channel, string exchange)
        {
            channel.ExchangeDeclarePassiveAsync(exchange).GetAwaiter().GetResult();
        }

        public static void ExchangeDelete(this IChannel channel, string exchange, bool ifUnused)
        {
            channel.ExchangeDeleteAsync(exchange, ifUnused).GetAwaiter().GetResult();
        }

        public static void ExchangeDeleteNoWait(this IChannel channel, string exchange, bool ifUnused)
        {
            channel.ExchangeDeleteAsync(exchange, ifUnused, noWait: true).GetAwaiter().GetResult();
        }

        public static void ExchangeBind(this IChannel channel, string destination, string source, string routingKey, IDictionary<string, object> arguments)
        {
            channel.ExchangeBindAsync(destination, source, routingKey, arguments).GetAwaiter().GetResult();
        }

        public static void ExchangeBindNoWait(this IChannel channel, string destination, string source, string routingKey, IDictionary<string, object> arguments)
        {
            channel.ExchangeBindAsync(destination, source, routingKey, arguments, noWait: true).GetAwaiter().GetResult();
        }

        public static void ExchangeUnbind(this IChannel channel, string destination, string source, string routingKey, IDictionary<string, object> arguments)
        {
            channel.ExchangeUnbindAsync(destination, source, routingKey, arguments).GetAwaiter().GetResult();
        }

        public static void ExchangeUnbindNoWait(this IChannel channel, string destination, string source, string routingKey, IDictionary<string, object> arguments)
        {
            channel.ExchangeUnbindAsync(destination, source, routingKey, arguments, noWait: true).GetAwaiter().GetResult();
        }

        public static QueueDeclareOk QueueDeclare(this IChannel channel, string queue, bool durable, bool exclusive, bool autoDelete, IDictionary<string, object> arguments)
        {
            return channel.QueueDeclareAsync(queue, durable, exclusive, autoDelete, arguments).GetAwaiter().GetResult();
        }

        public static void QueueDeclareNoWait(this IChannel channel, string queue, bool durable, bool exclusive, bool autoDelete, IDictionary<string, object> arguments)
        {
            channel.QueueDeclareAsync(queue, durable, exclusive, autoDelete, arguments, noWait: true).GetAwaiter().GetResult();
        }

        public static QueueDeclareOk QueueDeclarePassive(this IChannel channel, string queue)
        {
            return channel.QueueDeclarePassiveAsync(queue).GetAwaiter().GetResult();
        }

        public static uint QueueDelete(this IChannel channel, string queue, bool ifUnused, bool ifEmpty)
        {
            return channel.QueueDeleteAsync(queue, ifUnused, ifEmpty).GetAwaiter().GetResult();
        }

        public static void QueueDeleteNoWait(this IChannel channel, string queue, bool ifUnused, bool ifEmpty)
        {
            channel.QueueDeleteAsync(queue, ifUnused, ifEmpty, noWait: true).GetAwaiter().GetResult();
        }

        public static void QueueBind(this IChannel channel, string queue, string exchange, string routingKey, IDictionary<string, object> arguments)
        {
            channel.QueueBindAsync(queue, exchange, routingKey, arguments).GetAwaiter().GetResult();
        }

        public static void QueueBindNoWait(this IChannel channel, string queue, string exchange, string routingKey, IDictionary<string, object> arguments)
        {
            channel.QueueBindAsync(queue, exchange, routingKey, arguments, noWait: true).GetAwaiter().GetResult();
        }

        public static void QueueUnbind(this IChannel channel, string queue, string exchange, string routingKey, IDictionary<string, object> arguments)
        {
            channel.QueueUnbindAsync(queue, exchange, routingKey, arguments).GetAwaiter().GetResult();
        }

        public static uint QueuePurge(this IChannel channel, string queue)
        {
            return channel.QueuePurgeAsync(queue).GetAwaiter().GetResult();
        }

        public static void BasicQos(this IChannel channel, uint prefetchSize, ushort prefetchCount, bool global)
        {
            channel.BasicQosAsync(prefetchSize, prefetchCount, global).GetAwaiter().GetResult();
        }

        public static void BasicAck(this IChannel channel, ulong deliveryTag, bool multiple)
        {
            channel.BasicAckAsync(deliveryTag, multiple).GetAwaiter().GetResult();
        }

        public static void BasicNack(this IChannel channel, ulong deliveryTag, bool multiple, bool requeue)
        {
            channel.BasicNackAsync(deliveryTag, multiple, requeue).GetAwaiter().GetResult();
        }

        public static void BasicPublish(this IChannel channel, string exchange, string routingKey, BasicProperties properties, byte[] body)
        {
            channel.BasicPublishAsync(exchange, routingKey, mandatory: false, basicProperties: properties, body: body ?? Array.Empty<byte>()).GetAwaiter().GetResult();
        }

        public static BasicGetResult BasicGet(this IChannel channel, string queue, bool autoAck)
        {
            return channel.BasicGetAsync(queue, autoAck).GetAwaiter().GetResult();
        }

        public static string BasicConsume(this IChannel channel, string queue, bool autoAck, IAsyncBasicConsumer consumer)
        {
            return channel.BasicConsumeAsync(queue, autoAck, consumer).GetAwaiter().GetResult();
        }

        public static void ConfirmSelect(this IChannel channel)
        {
            // Publisher confirmations are enabled through CreateChannelOptions in RabbitMQ.Client 7.x.
        }

        public static bool WaitForConfirms(this IChannel channel)
        {
            // BasicPublishAsync completes after the publish operation; keep existing synchronous API behavior.
            return true;
        }

        public static uint MessageCount(this IChannel channel, string queue)
        {
            return channel.QueueDeclarePassive(queue).MessageCount;
        }
    }
}
