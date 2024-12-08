using app.Services;
using Microsoft.AspNetCore.Mvc;

namespace app.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FilesController(MQTTClientService mqttService) : ControllerBase
{
    private readonly MQTTClientService _mqttService = mqttService;

    [HttpPost]
    public async Task<IActionResult> UploadFiles(IFormFile file, double? version = null)
    {
        double finalVersion = version ?? GetNextVersion();

        (string msg, bool isSuccess, string? fileName, byte[]? fileBytes) = new UploadHandler().Upload(file, finalVersion);

        if (!isSuccess || fileName is null || fileBytes is null) return BadRequest($"Failed to upload file: {msg}");

        // Remove if you gonna use the mqtt method
        string MQTTNotifyMessage = await PublishMessage("iot/updates", fileName); // Temp

        // mqtt method
        string MQTTUpdateMessage = await PublishMessage("iot/files/latest", Convert.ToBase64String(fileBytes));

        return Ok($"{msg}, File name: {fileName}. MQTT: {MQTTNotifyMessage}.");
    }

    // http method
    [HttpGet("latest")]
    public IActionResult GetLatestCode(string? fileName)
    {
        string path = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");

        if (!Directory.Exists(path))
            return NotFound("Uploads directory not found.");

        string[] files = Directory.GetFiles(path, "*.c"); // All code files

        if (files.Length == 0)
            return NotFound("No code file available.");

        string? fileToRead;

        if (!string.IsNullOrEmpty(fileName))
        {
            fileToRead = files.FirstOrDefault(file =>
                Path.GetFileName(file) == fileName);
        }
        else
        {
            // Find the latest version file (based on file name version parsing)
            fileToRead = files
                .OrderByDescending(file => double.Parse(Path.GetFileNameWithoutExtension(file).Replace('_', '.')))
                .First();
        }

        if (fileToRead is null)
            return NotFound($"Code file '{fileName}' not found.");

        byte[] fileBytes = System.IO.File.ReadAllBytes(fileToRead);

        return File(fileBytes, "application/octet-stream", Path.GetFileName(fileToRead)); // binary response

        //return Ok(fileBytes); // bytes response

        //string base64String = Convert.ToBase64String(fileBytes);
        //return Ok(base64String); // base64 response
    }

    private static double GetNextVersion()
    {
        string path = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
            return 1.00; // base version
        }

        var files = Directory.GetFiles(path, "*.c");

        if (files.Length == 0)
            return 1.00;

        // Find the latest version
        double latestVersion = files
            .Select(file => Path.GetFileNameWithoutExtension(file))
            .Where(fileName => double.TryParse(fileName.Replace('_', '.'), out _)) // Replace underscores with periods
            .Select(fileName => double.Parse(fileName.Replace('_', '.')))          // Convert to double after replacing underscores
            .DefaultIfEmpty(1.00) // Default if no valid files
            .Max();

        return Math.Round(latestVersion + 0.01, 2); // Increment by 0.01 and return
    }

    private async Task<string> PublishMessage(string topic, string message)
    {
        if (!_mqttService.IsConnected)
        {
            return "Failed to publish message: The MQTT client is not connected.";
        }

        try
        {
            await _mqttService.PublishAsync(topic, message);
            return "Notification broadcasted";
        }
        catch (Exception ex)
        {
            return $"Failed to broadcast notification: {ex.Message}";
        }
    }
}
