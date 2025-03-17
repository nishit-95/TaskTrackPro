using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace MyApp.Core.Services
{
    public class RabbitMQService
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public RabbitMQService()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" }; // Update with your RabbitMQ host
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: "admin_notifications_queue", durable: true, exclusive: false, autoDelete: false);
        }

        public void PublishAdminMessage(string message)
        {
            var body = Encoding.UTF8.GetBytes(message);
            _channel.BasicPublish(exchange: "", routingKey: "admin_notifications_queue", basicProperties: null, body: body);
        }

        public void ConsumeAdminMessages(Action<string> onMessageReceived)
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                onMessageReceived(message);
            };
            _channel.BasicConsume(queue: "admin_notifications_queue", autoAck: true, consumer: consumer);
        }

        public void Dispose()
        {
            _channel.Close();
            _connection.Close();
        }
    }
}