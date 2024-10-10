using MessagesHandlerService.Listeners;
using Microsoft.Extensions.Configuration;

public class Program
{
    public static async Task Main(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        var rabbitMQSettings = configuration.GetSection("RabbitMQ");
        var rabbitQueueName = rabbitMQSettings["QueueName"];

        var kafkaSettings = configuration.GetSection("Kafka");
        var kafkaTopic = kafkaSettings["TopicName"];
        var kafkaGroupId = kafkaSettings["GroupId"];
        var kafkaBootstrapServers = kafkaSettings["BootstrapServers"];


        //TO DO: set up skip and take options.

        var rabbitListener = new RabbitMQListener();
        Task.Run(() => rabbitListener.ListenToRabbitMQ(rabbitQueueName));

        var kafkaListener = new KafkaListener();
        kafkaListener.ListenToKafka(kafkaTopic, kafkaGroupId, kafkaBootstrapServers);
    }
}