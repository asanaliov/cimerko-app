using cimerko_app.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Moq;

namespace Tests;

public class LocalImageStorageTests {
    [Fact]
    public async Task SaveAsync_uses_detected_extension_and_a_unique_file_name() {
        var webRootPath = Path.Combine(Path.GetTempPath(), $"cimerko-images-{Guid.NewGuid():N}");
        var environment = new Mock<IWebHostEnvironment>();
        environment.Setup(item => item.WebRootPath).Returns(webRootPath);
        var storage = new LocalImageStorage(environment.Object);
        await using var stream = new MemoryStream([
            0x89, 0x50, 0x4E, 0x47,
            0x0D, 0x0A, 0x1A, 0x0A,
            0x00, 0x00, 0x00, 0x00
        ]);
        var image = new FormFile(stream, 0, stream.Length, "image", "../../unsafe.jpg");

        try {
            var extension = await storage.DetectExtensionAsync(image);
            var savedImage = await storage.SaveAsync(
                image,
                Assert.IsType<string>(extension),
                "uploads/listings",
                "/uploads/listings/",
                CancellationToken.None);

            Assert.EndsWith(".png", savedImage.ImageUrl);
            Assert.DoesNotContain("unsafe", savedImage.ImageUrl);
            Assert.True(File.Exists(savedImage.FilePath));

            storage.DeleteLocalImage(
                savedImage.ImageUrl,
                ("/uploads/listings/", "uploads/listings"));

            Assert.False(File.Exists(savedImage.FilePath));
        }
        finally {
            if (Directory.Exists(webRootPath)) {
                Directory.Delete(webRootPath, recursive: true);
            }
        }
    }

    [Fact]
    public async Task DetectExtensionAsync_rejects_content_that_is_not_an_allowed_image() {
        var storage = new LocalImageStorage(Mock.Of<IWebHostEnvironment>());
        await using var stream = new MemoryStream("not an image"u8.ToArray());
        var image = new FormFile(stream, 0, stream.Length, "image", "photo.jpg");

        var extension = await storage.DetectExtensionAsync(image);

        Assert.Null(extension);
    }
}
