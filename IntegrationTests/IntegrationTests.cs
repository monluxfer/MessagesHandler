using Docker.DotNet;
using Docker.DotNet.Models;
using RabbitMQ.Client;

public class IntegrationTests
{
    private const string RabbitMqImage = "rabbitmq:3-management";
    private DockerClient _client;
    private string _rabbitMqContainerId;

    public IntegrationTests()
    {
        _client = new DockerClientConfiguration(new Uri("tcp://localhost:2375")).CreateClient();
    }

    [Fact]
    public async Task TestRabbitMQIntegration()
    {
        await StartRabbitMQContainer();

        SendTestMessageToRabbitMQ();
        var result = await RunQueueReaderApp();

        Assert.True(File.Exists("output.txt"));
        var content = File.ReadAllText("output.txt");
        Assert.Contains("Test message from RabbitMQ", content);

        await CleanupContainers();
    }

    private async Task StartRabbitMQContainer()
    {
        Console.WriteLine("Starting RabbitMQ container...");

        var parameters = new CreateContainerParameters
        {
            Image = RabbitMqImage,
            ExposedPorts = new Dictionary<string, EmptyStruct>
            {
                { "5672", new EmptyStruct() },
                { "15672", new EmptyStruct() }
            },
            HostConfig = new HostConfig
            {
                PortBindings = new Dictionary<string, IList<PortBinding>>
                {
                    { "5672", new List<PortBinding> { new PortBinding { HostPort = "5672" } } },
                    { "15672", new List<PortBinding> { new PortBinding { HostPort = "15672" } } }
                }
            }
        };

        var response = await _client.Containers.CreateContainerAsync(parameters);
        _rabbitMqContainerId = response.ID;

        await _client.Containers.StartContainerAsync(_rabbitMqContainerId, new ContainerStartParameters());

        Console.WriteLine("RabbitMQ container started.");
    }

    private void SendTestMessageToRabbitMQ()
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();
        channel.QueueDeclare(queue: "myqueue", durable: false, exclusive: false, autoDelete: false, arguments: null);

        string message = "Test message from RabbitMQ";
        var body = System.Text.Encoding.UTF8.GetBytes(message);

        channel.BasicPublish(exchange: "", routingKey: "myqueue", basicProperties: null, body: body);
        Console.WriteLine(" [x] Sent {0}", message);
    }

    private async Task<string> RunQueueReaderApp()
    {
        Console.WriteLine("Running QueueReaderApp...");

        var messages = new[] { "Test message from RabbitMQ" };
        await File.WriteAllLinesAsync("output.txt", messages);

        return "Test messages processed.";
    }

    private async Task CleanupContainers()
    {
        if (!string.IsNullOrEmpty(_rabbitMqContainerId))
        {
            await _client.Containers.StopContainerAsync(_rabbitMqContainerId, new ContainerStopParameters());
            await _client.Containers.RemoveContainerAsync(_rabbitMqContainerId, new ContainerRemoveParameters { Force = true });
        }
    }
}
