namespace cimerko_app.Services;

public class LocalImageStorage {
    private readonly IWebHostEnvironment _environment;

    public LocalImageStorage(IWebHostEnvironment environment) {
        _environment = environment;
    }

    public async Task<string?> DetectExtensionAsync(IFormFile image) {
        var header = new byte[12];
        await using var stream = image.OpenReadStream();
        var bytesRead = await stream.ReadAsync(header);

        if (bytesRead >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF) {
            return ".jpg";
        }

        if (bytesRead >= 8 &&
            header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47 &&
            header[4] == 0x0D && header[5] == 0x0A && header[6] == 0x1A && header[7] == 0x0A) {
            return ".png";
        }

        if (bytesRead >= 12 &&
            header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x46 &&
            header[8] == 0x57 && header[9] == 0x45 && header[10] == 0x42 && header[11] == 0x50) {
            return ".webp";
        }

        return null;
    }

    public async Task<StoredImage> SaveAsync(
        IFormFile image,
        string extension,
        string relativeDirectory,
        string urlPrefix,
        CancellationToken cancellationToken) {
        var storageDirectory = Path.Combine(WebRootPath(), relativeDirectory);
        Directory.CreateDirectory(storageDirectory);

        var fileName = $"{Guid.NewGuid():N}{extension}";
        var filePath = Path.Combine(storageDirectory, fileName);

        try {
            await using var output = File.Create(filePath);
            await image.CopyToAsync(output, cancellationToken);
        }
        catch {
            DeleteFile(filePath);
            throw;
        }

        return new StoredImage($"{urlPrefix}{fileName}", filePath);
    }

    public void DeleteLocalImage(
        string? imageUrl,
        params (string UrlPrefix, string RelativeDirectory)[] locations) {
        if (string.IsNullOrWhiteSpace(imageUrl)) {
            return;
        }

        foreach (var location in locations) {
            if (!imageUrl.StartsWith(location.UrlPrefix, StringComparison.Ordinal)) {
                continue;
            }

            var fileName = Path.GetFileName(imageUrl);
            if (imageUrl != $"{location.UrlPrefix}{fileName}") {
                return;
            }

            DeleteFile(Path.Combine(WebRootPath(), location.RelativeDirectory, fileName));
            return;
        }
    }

    public void DeleteFile(string? path) {
        if (path != null && File.Exists(path)) {
            File.Delete(path);
        }
    }

    private string WebRootPath() {
        return _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
    }
}

public record StoredImage(string ImageUrl, string FilePath);
