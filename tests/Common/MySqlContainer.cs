using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

using Docker.DotNet;
using Docker.DotNet.Models;

using NUnit.Framework;

[assembly: Parallelizable(ParallelScope.Fixtures)]
[assembly: LevelOfParallelism(8)]

[SetUpFixture]
public class MySqlContainer
{
    public const string DbName = "identity";
    public const string DbUser = "identity";
    public const string DbPassword = "password";

    private static string? ContainerId { get; set; }

    private static string? ServerIp { get; set; }

    private static DockerClientConfiguration? Configuration { get; set; }

    public static Uri LocalDockerUri
    {
        get
        {
            var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            return isWindows ? new Uri("npipe://./pipe/docker_engine") : new Uri("unix:/var/run/docker.sock");
        }
    }

    [OneTimeTearDown]
    public async Task TearDown()
    {
        if (ContainerId != null && Configuration != null)
        {
            var client = Configuration.CreateClient();
            await client.Containers.StopContainerAsync(ContainerId, new ContainerStopParameters());
            await client.Containers.RemoveContainerAsync(ContainerId, new ContainerRemoveParameters());
        }
    }

    public static async Task<string> GetMysqlServerAddress()
    {
        if (ServerIp == null)
        {
            Configuration = new DockerClientConfiguration(LocalDockerUri);
            var (containerId, serverIp) = await StartMysqlContainer();

            ServerIp = serverIp;
            ContainerId = containerId;
        }

        return ServerIp;
    }

    private static async Task<(string, string)> StartMysqlContainer()
    {
        if (Configuration == null)
        {
            throw new NotSupportedException();
        }

        var client = Configuration.CreateClient();
        var createContainerResponse = await client.Containers.CreateContainerAsync(new CreateContainerParameters
        {
            Image = "public.ecr.aws/bitnami/mariadb:10.4",
            Env = new List<string>
            {
                "MARIADB_RANDOM_ROOT_PASSWORD=1",
                "ALLOW_EMPTY_PASSWORD=yes",
                $"MARIADB_DATABASE={DbName}",
                $"MARIADB_USER={DbUser}",
                $"MARIADB_PASSWORD={DbPassword}",
            },
            HostConfig = new HostConfig
            {
                PortBindings = new Dictionary<string, IList<PortBinding>>
                {
                    ["3306"] = new List<PortBinding> { new PortBinding { HostPort = "3306" } }
                }
            },
            ExposedPorts = new Dictionary<string, EmptyStruct>
            {
                ["3306"] = new EmptyStruct()
            }
        });

        await client.Containers.StartContainerAsync(createContainerResponse.ID, null);
        await Task.Delay(2000);
        await WaitUntilMysqlIsUp(client, createContainerResponse.ID);
        return (createContainerResponse.ID, "localhost");
    }

    private static async Task WaitUntilMysqlIsUp(DockerClient client, string containerId)
    {
        var up = false;

        while (!up)
        {
            var cmd = new List<string> { "mysqladmin", "ping", "-u", DbUser, $"-p{DbPassword}" };
            var execResponse = await client.Containers.ExecCreateContainerAsync(containerId, new ContainerExecCreateParameters
            {
                AttachStdout = true,
                AttachStderr = true,
                Cmd = cmd
            });

            var config = new ContainerExecStartParameters
            {
                AttachStdout = true,
                AttachStderr = true,
                Cmd = cmd
            };

            using var multiplexedStream = await client.Containers.StartWithConfigContainerExecAsync(execResponse.ID, config);
            var (stdout, stderr) = await multiplexedStream.ReadOutputToEndAsync(default);
            up = stdout.Split('\n')[0].Trim() == "mysqld is alive";
        }
    }
}
