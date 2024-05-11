using System.Management;
using System.Text;
using AngleSharp.Io;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;

namespace Duracellko.NET.Deployment;

public class DeployAzureStorage : MultiConfigModule
{
    private const string DefaultContainerName = "$web";
    
    private const string ConnectionString = nameof(ConnectionString);
    private const string ContainerName = nameof(ContainerName);
    private const string SourcePath = nameof(SourcePath);

    private const string ContentType = nameof(ContentType);

    public DeployAzureStorage FromPath(Config<NormalizedPath> path) => (DeployAzureStorage)SetConfig(SourcePath, path);

    public DeployAzureStorage ToContainer(Config<string> container) => (DeployAzureStorage)SetConfig(ContainerName, container);

    public DeployAzureStorage(Config<string> connectionString)
        : base([new KeyValuePair<string, IConfig>(ConnectionString, connectionString)], false)
    {
    }

    protected override async Task<IEnumerable<IDocument>> ExecuteConfigAsync(IDocument input, IExecutionContext context, IMetadata values)
    {
        var connectionString = values.GetString(ConnectionString);
        var containerName = values.GetString(ContainerName, DefaultContainerName);
        var sourcePath = values.GetPath(SourcePath, context.FileSystem.OutputPath);

        try
        {
            var blobServiceClient = new BlobServiceClient(connectionString);
            context.LogDebug(
                "Deploying '{source}' to Azure Storage '{uri}' container '{container}'.",
                sourcePath,
                blobServiceClient.Uri,
                containerName);

            var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);
            await UploadFilesToAzureBlobContainer(sourcePath, blobContainerClient, context);
        }
        catch (Exception ex)
        {
            context.LogError("Exception while deploying Azure Storage: {error}", ex.Message);
            throw;
        }

        return await input.YieldAsync();
    }

    private static async Task UploadFilesToAzureBlobContainer(NormalizedPath sourcePath, BlobContainerClient blobContainerClient, IExecutionContext context)
    {
        var headers = new BlobHttpHeaders();

        var files = Directory.GetFiles(sourcePath.FullPath, "*", SearchOption.AllDirectories);
        foreach (var file in files)
        {
            var relativeFilePath = Path.GetRelativePath(sourcePath.FullPath, file);
            context.LogDebug("Uploading file to Azure Blob: {path}", relativeFilePath);

            var blobName = GetBlobName(relativeFilePath);
            var mimeType = MimeTypeProvider.GetMimeTypeFromFileName(relativeFilePath);

            var blobClient = blobContainerClient.GetBlobClient(blobName);
            headers.ContentType = mimeType;
            await blobClient.UploadAsync(file, headers);
        }
    }

    private static string GetBlobName(string relativeFilePath)
    {
        var blobName = relativeFilePath;
        if (blobName.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
        {
            blobName = blobName.Substring(0, blobName.Length - 5);
        }

        return blobName;
    }
}
