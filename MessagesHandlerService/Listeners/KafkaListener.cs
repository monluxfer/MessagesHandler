using Confluent.Kafka;

namespace MessagesHandlerService.Listeners
{
    public class KafkaListener
    {

        public void ListenToKafka(string topic, string groupId, string bootstrapServers)
        {
            var config = new ConsumerConfig
            {
                GroupId = groupId,
                BootstrapServers = bootstrapServers,
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            var outputFile = "output.txt";

            using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
            consumer.Subscribe(topic);

            Console.WriteLine("Listening for messages from Kafka...");

            while (true)
            {
                var consumeResult = consumer.Consume();
                Console.WriteLine($"Received from Kafka: {consumeResult.Message.Value}");
                File.AppendAllText(outputFile, consumeResult.Message.Value);
            }
        }
    }
}
