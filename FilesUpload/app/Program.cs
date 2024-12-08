using app.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Why Use a Singleton?
//Efficiency: The MQTT client connection is maintained across the application's lifetime.
//Consistency: A single connection avoids potential conflicts from multiple instances trying to connect simultaneously.
builder.Services.AddSingleton<MQTTClientService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

var mqttService = app.Services.GetRequiredService<MQTTClientService>();
await mqttService.ConnectAsync();

app.Run();
