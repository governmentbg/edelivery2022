using System;
using System.Collections.Generic;

namespace ED.Keystore
{
    public class KeystoreDeployment
    {
        public string? Name { get; set; }

        public string? Url { get; set; }

        public bool UseGrpcWeb { get; set; }
    }

    public class KeystoreOptions
    {
        public string? CngProvider { get; set; }

        public string[]? GrpcServiceHosts { get; set; }

        public bool EnableGrpcWeb { get; set; }

        public string? DeploymentName { get; set; }

        public KeystoreDeployment[]? Deployments { get; set; }

        public (string deploymentName, string deploymentUrl, bool deploymentUseGrpcWeb)[] GetOtherDeployments()
        {
            List<(string deploymentName, string deploymentUrl, bool deploymentUseGrpcWeb)> res = new();

            if (!string.IsNullOrEmpty(this.DeploymentName) &&
                this.Deployments?.Length > 1)
            {
                foreach (var deployment in this.Deployments)
                {
                    var dName = deployment.Name
                        ?? throw new Exception($"Missing setting {nameof(KeystoreDeployment)}.{nameof(KeystoreDeployment.Name)}");
                    var dUrl = deployment.Url
                        ?? throw new Exception($"Missing setting {nameof(KeystoreDeployment)}.{nameof(KeystoreDeployment.Url)}");

                    if (dName == this.DeploymentName)
                    {
                        // this is us
                        continue;
                    }

                    res.Add((dName, dUrl, deployment.UseGrpcWeb));
                }
            }

            return res.ToArray();
        }
    }
}
