using Microsoft.AspNetCore.Components.Forms;

namespace PetMap.Services;

/// <summary>Validates image signatures and stores uploads under generated names.</summary>
public static class SafeImageStorage
{
    public static async Task<string> SaveAsync(
        IBrowserFile file,
        string webRootPath,
        string folderName,
        long maxBytes,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(file);
        if (file.Size <= 0 || file.Size > maxBytes)
            throw new InvalidDataException("Image exceeds the allowed size.");

        await using var input = file.OpenReadStream(maxBytes, cancellationToken);
        var signature = new byte[12];
        var signatureLength = 0;
        while (signatureLength < signature.Length)
        {
            var read = await input.ReadAsync(signature.AsMemory(signatureLength), cancellationToken);
            if (read == 0) break;
            signatureLength += read;
        }

        var extension = GetImageExtension(signature.AsSpan(0, signatureLength));
        if (extension is null)
            throw new InvalidDataException("Unsupported image content.");

        var relativeFolder = folderName.Trim('/', '\\');
        var destinationDirectory = Path.GetFullPath(Path.Combine(webRootPath, relativeFolder));
        var allowedRoot = Path.GetFullPath(webRootPath) + Path.DirectorySeparatorChar;
        if (!destinationDirectory.StartsWith(allowedRoot, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Upload destination is outside the web root.");

        Directory.CreateDirectory(destinationDirectory);
        var fileName = $"{Guid.NewGuid():N}{extension}";
        var destinationPath = Path.Combine(destinationDirectory, fileName);
        try
        {
            await using var output = new FileStream(destinationPath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
            await output.WriteAsync(signature.AsMemory(0, signatureLength), cancellationToken);
            await input.CopyToAsync(output, cancellationToken);
        }
        catch
        {
            if (File.Exists(destinationPath))
                File.Delete(destinationPath);
            throw;
        }

        return $"/{relativeFolder.Replace(Path.DirectorySeparatorChar, '/')}/{fileName}";
    }

    public static void Delete(string webRootPath, string webPath)
    {
        var relativePath = webPath.TrimStart('/', '\\').Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.GetFullPath(Path.Combine(webRootPath, relativePath));
        var allowedRoot = Path.GetFullPath(webRootPath) + Path.DirectorySeparatorChar;
        if (fullPath.StartsWith(allowedRoot, StringComparison.OrdinalIgnoreCase) && File.Exists(fullPath))
            File.Delete(fullPath);
    }

    private static string? GetImageExtension(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length >= 8 && bytes[..8].SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }))
            return ".png";
        if (bytes.Length >= 3 && bytes[..3].SequenceEqual(new byte[] { 0xFF, 0xD8, 0xFF }))
            return ".jpg";
        if (bytes.Length >= 6 &&
            (bytes[..6].SequenceEqual("GIF87a"u8) || bytes[..6].SequenceEqual("GIF89a"u8)))
            return ".gif";
        if (bytes.Length >= 12 && bytes[..4].SequenceEqual("RIFF"u8) && bytes[8..12].SequenceEqual("WEBP"u8))
            return ".webp";
        return null;
    }
}
