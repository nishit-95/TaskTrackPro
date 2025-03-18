using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Npgsql;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;


namespace MyApp.Core.Services
{
    public interface IRabbitMQService
    {
        // void SendMessage(string queueName, string message);
        void SendMessage(string queueName, string sender, string message);

        // string ReceiveMessage(string queueName);
        (string Sender, string Message) ReceiveMessage(string queueName);
    }
    public class RabbitMQService : IRabbitMQService
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;

        private readonly string _hostName;
        private readonly string _userName;
        private readonly string _password;
        // private readonly string _queueName;

        public RabbitMQService(IConfiguration configuration)
        {
            _hostName = configuration["RabbitMQ:HostName"];
            _userName = configuration["RabbitMQ:UserName"];
            _password = configuration["RabbitMQ:Password"];
            var factory = new ConnectionFactory { HostName = _hostName };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            // _queueName = configuration["RabbitMQ:QueueName"];
        }

        public void SendMessage(string queueName, string sender, string message)
        {
            // Queue ne banvai
            _channel.QueueDeclare(queue: queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

            // custom message aetel ke sender ane real message ne bhegu karyu
            var fullMessage = $"{sender}:{message}";
            Console.WriteLine("Full Message:" + fullMessage);

            // Convert to byte
            var body = Encoding.UTF8.GetBytes(fullMessage);

            // message mokalyo
            _channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: null, body: body);
        }
        public (string Sender, string Message) ReceiveMessage(string queueName)
        {
            // Queue ne banavi
            _channel.QueueDeclare(queue: queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

            // message ne extract karyo
            var result = _channel.BasicGet(queueName, autoAck: true);
            if (result == null)
            {
                return (null, null); // No message available
            }

            // Extract the message body
            var messageBody = Encoding.UTF8.GetString(result.Body.ToArray());

            // sender nu name nad real message ne separate karya eg sender:message
            var messageParts = messageBody.Split(new[] { ':' }, 2);
            if (messageParts.Length != 2)
            {
                return (null, null);
            }

            var sender = messageParts[0];
            var message = messageParts[1];

            Console.WriteLine("Sender:" + sender);
            Console.WriteLine("Message:" + message);

            return (sender, message);
        }


        public void PublishMessage(string message, int userId)
        {
            try
            {
                var factory = new ConnectionFactory()
                {
                    HostName = _hostName,
                    UserName = _userName,
                    Password = _password
                };
                string _queueName = "UserRegistrationQueue";
                using var connection = factory.CreateConnection();
                using var channel = connection.CreateModel();

                channel.QueueDeclare(queue: _queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
                Console.WriteLine($"[Publishing] Sending message to queue {_queueName}: {message}, UserId: {userId}");

                // 🔹 Create JSON payload
                var notificationData = new { Title = message, UserId = userId };
                var jsonMessage = JsonConvert.SerializeObject(notificationData);
                var body = Encoding.UTF8.GetBytes(jsonMessage);

                channel.BasicPublish(exchange: "", routingKey: _queueName, basicProperties: null, body: body);

                Console.WriteLine($"Message Published: {jsonMessage}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"RabbitMQ Publish Error: {ex.Message}");
            }
        }


    }
}