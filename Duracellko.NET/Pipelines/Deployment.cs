using Duracellko.NET.Deployment;

namespace Duracellko.NET.Pipelines;

public class Deployment : Pipeline
{
    public Deployment()
    {
        Deployment = true;

        OutputModules = new ModuleList
        {
            new ExecuteIf(Config.ContainsSettings(DeploymentKeys.AzureStorageConnectionString))
            {
                new LogMessage(Config.FromSettings(x => $"Deploying to Azure Blob Storage")),
                new DeployAzureStorage(Config.FromSetting(DeploymentKeys.AzureStorageConnectionString))
            }
        };
    }
}
