using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Npgsql;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;


namespace MyApp.Core.Services
{
    public class RabbitMQService
    {
        private readonly string _hostName;
        private readonly string _userName;
        private readonly string _password;
        private readonly string _queueName;

        public RabbitMQService(IConfiguration configuration)
        {
            _hostName = configuration["RabbitMQ:HostName"];
            _userName = configuration["RabbitMQ:UserName"];
            _password = configuration["RabbitMQ:Password"];
            _queueName = configuration["RabbitMQ:QueueName"];
        }
        public void PublishMessage(string message)
        {
            var factory = new ConnectionFactory() { HostName = _hostName, UserName = _userName, Password = _password };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.QueueDeclare(queue: _queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

            var body = Encoding.UTF8.GetBytes(message);

            channel.BasicPublish(exchange: "", routingKey: _queueName, basicProperties: null, body: body);
        }

    }
}