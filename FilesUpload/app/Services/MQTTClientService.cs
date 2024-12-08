using MQTTnet;
using MQTTnet.Client;
using System.Text;

namespace app.Services;

public class MQTTClientService
{
    private readonly IMqttClient _mqttClient;
    private readonly MqttClientOptions _mqttOptions;

    public bool IsConnected => _mqttClient.IsConnected;

    public MQTTClientService()
    {
        var mqttFactory = new MqttFactory();
        _mqttClient = mqttFactory.CreateMqttClient();

        // Configure the MQTT client options
        _mqttOptions = new MqttClientOptionsBuilder()
            // .WithClientId("dotnet-backend") // if cloud hosted
            .WithTcpServer("localhost", 1883)
            //.WithCredentials("username", "password")
            .WithCleanSession()
            .Build();

        // Set up event handlers (optional)
        _mqttClient.ConnectedAsync += OnConnectedAsync;
        _mqttClient.DisconnectedAsync += OnDisconnectedAsync;
    }

    public async Task ConnectAsync()
    {
        try
        {
            await _mqttClient.ConnectAsync(_mqttOptions);
            Console.WriteLine("Connected to MQTT broker.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to connect to MQTT broker: {ex.Message}");
        }
    }

    public async Task PublishAsync(string topic, string message)
    {
        var mqttMessage = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(Encoding.UTF8.GetBytes(message))
            .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce) // Qos = 1
            .Build();

        try
        {
            await _mqttClient.PublishAsync(mqttMessage);
            Console.WriteLine($"Message published to topic '{topic}': {message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to publish message: {ex.Message}");
        }
    }

    private Task OnConnectedAsync(MqttClientConnectedEventArgs args)
    {
        Console.WriteLine("Successfully connected to the broker.");
        return Task.CompletedTask;
    }

    private Task OnDisconnectedAsync(MqttClientDisconnectedEventArgs args)
    {
        Console.WriteLine($"Disconnected from broker. Reason: {args.Reason}");
        return Task.CompletedTask;
    }
}
