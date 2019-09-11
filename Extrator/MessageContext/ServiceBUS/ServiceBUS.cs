using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Extrator.MessageContext.ServiceBUS
{
    public class ServiceBUS : IMessage
    {
        private readonly IConfigurationRoot config;
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly QueueClient queue;

        public ServiceBUS(IConfigurationRoot config)
        {
            this.config = config;
            this.queue = GetQueue();
        }

        private QueueClient GetQueue()
        {
            var sbConnectionString = config.GetSection("Queue").GetSection("ConnectionString").Value;
            if (string.IsNullOrEmpty(sbConnectionString)) throw new NullReferenceException("[Queue]:[ConnectionString]");
            var sbQueueName = config.GetSection("Queue").GetSection("Name").Value;
            if (string.IsNullOrEmpty(sbQueueName)) throw new NullReferenceException("[QueueService]:[Name]");
            return new QueueClient(sbConnectionString, sbQueueName);
        }

        public async void SendMessage(JObject data)
        {
            var message = new Message(Encoding.UTF8.GetBytes(data.ToString()));
            Logger.Info("Sending message...");
            await queue.SendAsync(message);
        }
    }
}
