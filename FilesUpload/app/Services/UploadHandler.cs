namespace app.Services;

public class UploadHandler
{
    readonly List<string> allowedExtentions = [".c"];
    const int MAX_SIZE = 5; // mgb

    public (string Message, bool IsSuccess, string? FileName, byte[]? FileBytes) Upload(IFormFile file, double version)
    {
        // extension validation
        string fileExt = Path.GetExtension(file.FileName);

        if (!allowedExtentions.Contains(fileExt))
            return ($"Extention {fileExt} is not valid", false, null, null);

        // file size validation
        long fileSize = file.Length;
        if (fileSize > (MAX_SIZE * 1024 * 1024))
            return ("File reached maximum size", false, null, null);

        // name procesessing
        string versionString = version.ToString("0.00").Replace('.', '_');

        string fileName = versionString + fileExt;
        string filePath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", fileName);

        byte[]? fileBytes = null;

        // saving the file to the uploads folder, if there is an existing version it will be overwritten
        try
        {
            using FileStream fileStream = new(Path.Combine(filePath, fileName), FileMode.Create);
            file.CopyTo(fileStream);

            fileBytes = System.IO.File.ReadAllBytes(filePath);
        }
        catch (Exception ex)
        {
            return ($"Error saving file: {ex.Message}", false, null, null);
        }

        return ("File uploaded successfully", true, fileName, fileBytes);
    }
}
