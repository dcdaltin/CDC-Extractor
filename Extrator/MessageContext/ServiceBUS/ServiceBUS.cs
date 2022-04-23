using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Extrator.MessageContext.ServiceBUS
{
    public class ServiceBUS : IMessage
    {
        private readonly IConfiguration config;
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly QueueClient queue;

        public ServiceBUS(IConfiguration config)
        {
            this.config = config;
            this.queue = GetQueue();
        }

        internal QueueClient GetQueue()
        {
            var sbConnectionString = config.GetSection("Queue").GetSection("ConnectionString").Value;
            if (string.IsNullOrEmpty(sbConnectionString)) throw new NullReferenceException("[Queue]:[ConnectionString]");
            var sbQueueName = config.GetSection("Queue").GetSection("Name").Value;
            if (string.IsNullOrEmpty(sbQueueName)) throw new NullReferenceException("[QueueService]:[Name]");
            return new QueueClient(sbConnectionString, sbQueueName);
        }

        internal Message CreateMessage(string section, string data)
        {
            var customerID = config.GetSection("CustomerID").Value;
            if (string.IsNullOrEmpty(customerID)) throw new NullReferenceException("[CustomerID]");
            var message = new JObject();
            message.Add("Timestamp", DateTime.Now);
            message.Add("CustomerID", customerID);
            message.Add("Section", section);
            message.Add("Data", data);
            return new Message(Encoding.UTF8.GetBytes(message.ToString()));
        }

        public Task SendMessage(string section, string data)
        {
            var message = CreateMessage(section, data);
            Logger.Info("Sending message...");
            return queue.SendAsync(message);
        }
    }
}
