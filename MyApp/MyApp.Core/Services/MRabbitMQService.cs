using System;
using System;
using RabbitMQ.Client;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using Newtonsoft.Json;

namespace MyApp.Core.Services
{
    public class MRabbitMQService
    {
        private readonly IConnection _connection;
        private IModel _channel;

    public MRabbitMQService(string hostname)
    {
        var factory = new ConnectionFactory() { HostName = hostname };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
    }

    public void PublishMessageTask(int userId, object message)
    {

        string queueName = $"notifications_for_user_{userId}";
        if (!_channel.IsOpen)
        {
            Console.WriteLine("[RabbitMQ] Channel closed, reconnecting...");
            _channel = _connection.CreateModel();
        }

        _channel.QueueDeclare(queue: queueName,
                                  durable: true,
                                  exclusive: false,
                                  autoDelete: false,
                                  arguments: null);
        var messageJson = JsonConvert.SerializeObject(message);
        var body = Encoding.UTF8.GetBytes(messageJson);

        var properties = _channel.CreateBasicProperties();
        properties.Persistent = true;

        _channel.BasicPublish(exchange: "",
                             routingKey: queueName,
                             basicProperties: properties,
                             body: body);
        Console.WriteLine($"[RabbitMQ] Sent notification to user {userId}: {messageJson}");
        Console.WriteLine($"[RabbitMQ] Queue '{queueName}' declared and message published.");
    }

    public void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
    }
    }
}