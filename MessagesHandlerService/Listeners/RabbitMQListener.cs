using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Text;

namespace MessagesHandlerService.Listeners
{
    public class RabbitMQListener
    {
        public void ListenToRabbitMQ(string queueName)
        {
            var factory = new ConnectionFactory() { HostName = "localhost", UserName = "guest", Password = "guest" };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();
            var outputFile = "output.txt";

            channel.QueueDeclare(queue: queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

            var consumer = new EventingBasicConsumer(channel);

            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine($"Received from RabbitMQ: {message}");
                File.AppendAllText(outputFile, message);
            };

            channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);

            Console.WriteLine("Listening for messages...");
            Console.ReadLine();
        }

    }
}
