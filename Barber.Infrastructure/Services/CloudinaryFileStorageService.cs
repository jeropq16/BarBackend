using Barber.Application.Interfaces;
using Barber.Infrastructure.Config;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;

namespace Barber.Infrastructure.Services;

public class CloudinaryFileStorageService : IFileStorageService
{
    private readonly Cloudinary _cloudinary;

    public CloudinaryFileStorageService(IOptions<CloudinarySettings> options)
    {
        var settings = options.Value;
        var account = new Account(settings.CloudName, settings.ApiKey, settings.ApiSecret);
        _cloudinary = new Cloudinary(account);
    }

    public async Task<string> UploadAsync(Stream fileStream, string fileName)
    {
        var upload = new ImageUploadParams
        {
            File = new FileDescription(fileName, fileStream),
            Folder = "barber-app" // carpeta opcional
        };

        var result = await _cloudinary.UploadAsync(upload);

        if (result.Error != null)
            throw new Exception(result.Error.Message);

        return result.SecureUrl.ToString();
    }

    public Task DeleteAsync(string fileUrl)
    {
        // (Opcional) Cloudinary usa public_id para borrar la imagen
        return Task.CompletedTask;
    }
}