using System.IO.Compression;

public sealed class ZipGameLoader
{
    public async Task<string> ExtractAsync(string zipFileName)
    {
        var targetFolder = Path.Combine(
            FileSystem.AppDataDirectory,
            Guid.NewGuid().ToString());

        Directory.CreateDirectory(targetFolder);

        await using var zipStream =
            await FileSystem.OpenAppPackageFileAsync(zipFileName);

        using var archive = new ZipArchive(zipStream);

        foreach (var entry in archive.Entries)
        {
            var filePath = Path.Combine(targetFolder, entry.FullName);

            using var entryStream = entry.Open();
            using var fileStream = File.Create(filePath);

            await entryStream.CopyToAsync(fileStream);
        }

        return targetFolder;
    }
}