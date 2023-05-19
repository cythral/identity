using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Amazon.S3;
using Amazon.S3.Model;

using Brighid.Identity.Cicd.Utils;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Brighid.Identity.Cicd.ClientUpdateDriver
{
    /// <inheritdoc />
    public class Host : IHost
    {
        private readonly CommandLineOptions options;
        private readonly IHostApplicationLifetime lifetime;

        /// <summary>
        /// Initializes a new instance of the <see cref="Host" /> class.
        /// </summary>
        /// <param name="options">Command line options.</param>
        /// <param name="lifetime">Service that controls the application lifetime.</param>
        /// <param name="serviceProvider">Object that provides access to the program's services.</param>
        public Host(
            IOptions<CommandLineOptions> options,
            IHostApplicationLifetime lifetime,
            IServiceProvider serviceProvider
        )
        {
            this.options = options.Value;
            this.lifetime = lifetime;
            Services = serviceProvider;
        }

        /// <inheritdoc />
        public IServiceProvider Services { get; }

        /// <inheritdoc />
        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var outputDirectory = ProjectRootDirectoryAttribute.ThisAssemblyProjectRootDirectory + "bin/Client";
            var username = "Brighid";
            var email = "52382196+brighid-bot@users.noreply.github.com";
            var branch = $"swagger-updates/{options.Version}";

            await Step($"Display GitHub Auth Status", async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                var command = new Command(
                    command: "gh auth status"
                );

                await command.RunOrThrowError(
                    errorMessage: "Could not retrieve github CLI authentication status.",
                    cancellationToken: cancellationToken
                );
            });

            await Step($"Clone Client Repository", async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (Directory.Exists(outputDirectory))
                {
                    Directory.Delete(outputDirectory, true);
                }

                var command = new Command(
                    command: "gh repo clone",
                    arguments: new[]
                    {
                        "cythral/identity-client",
                        outputDirectory,
                    }
                );

                await command.RunOrThrowError(
                    errorMessage: "Could not clone client repository.",
                    cancellationToken: cancellationToken
                );

                Directory.SetCurrentDirectory(outputDirectory);
            });

            await Step($"Setup Git Credential Helper", async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                var command = new Command(
                    command: "git config",
                    arguments: new[]
                    {
                        "credential.helper",
                        "\"!gh auth git-credential\"",
                    }
                );

                await command.RunOrThrowError(
                    errorMessage: "Could not set git username.",
                    cancellationToken: cancellationToken
                );

                Console.WriteLine($"Set git username to: {username}");
            });

            await Step($"Setup Git Username", async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                var command = new Command(
                    command: "git config",
                    arguments: new[]
                    {
                        "user.name",
                        username,
                    }
                );

                await command.RunOrThrowError(
                    errorMessage: "Could not set git username.",
                    cancellationToken: cancellationToken
                );

                Console.WriteLine($"Set git username to: {username}");
            });

            await Step($"Setup Git Email", async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                var command = new Command(
                    command: "git config",
                    arguments: new[]
                    {
                        "user.email",
                        email,
                    }
                );

                await command.RunOrThrowError(
                    errorMessage: "Could not set git email.",
                    cancellationToken: cancellationToken
                );

                Console.WriteLine($"Set git email to: {email}");
            });

            await Step($"Pull Updated Swagger", async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                var key = $"{options.ArtifactsLocation!.AbsolutePath.TrimStart('/').TrimEnd('/')}/swagger.json";

                var s3 = new AmazonS3Client();
                var request = new GetObjectRequest
                {
                    BucketName = options.ArtifactsLocation!.Host,
                    Key = key,
                };

                Console.WriteLine($"Pulling s3://{options.ArtifactsLocation.Host}/{key}");
                var response = await s3.GetObjectAsync(request, cancellationToken);
                await response.WriteResponseStreamToFileAsync($"{outputDirectory}/swagger.json", false, cancellationToken);
                Console.WriteLine("Pulled updated swagger from S3.");
            });

            var hasNoChangesToSwagger = await Step($"Check For Changes", async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                var command = new Command(
                    command: "git status",
                    options: new Dictionary<string, object>
                    {
                        ["--porcelain"] = true,
                    }
                );

                var output = await command.RunOrThrowError(
                    errorMessage: "Could not set retrieve git status",
                    cancellationToken: cancellationToken
                );

                return string.IsNullOrWhiteSpace(output);
            });

            if (hasNoChangesToSwagger)
            {
                Console.WriteLine("No changes, exiting.");
                lifetime.StopApplication();
                return;
            }

            await Step($"Checkout Branch", async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                var command = new Command(
                    command: "git checkout",
                    options: new Dictionary<string, object>
                    {
                        ["-b"] = branch,
                    }
                );

                await command.RunOrThrowError(
                    errorMessage: "Could not set git email.",
                    cancellationToken: cancellationToken
                );
            });

            await Step($"Add File", async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                var command = new Command(
                    command: "git add swagger.json"
                );

                await command.RunOrThrowError(
                    errorMessage: "Could not add file.",
                    cancellationToken: cancellationToken
                );
            });

            await Step($"Commit Changes", async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                var command = new Command(
                    command: "git commit",
                    options: new Dictionary<string, object>
                    {
                        ["--all"] = true,
                        ["--message"] = $"\"Update swagger to spec version {options.Version}\"",
                    }
                );

                await command.RunOrThrowError(
                    errorMessage: "Could not commit changes.",
                    cancellationToken: cancellationToken
                );
            });

            await Step($"Push Changes", async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                var command = new Command(
                    command: "git push",
                    options: new Dictionary<string, object>
                    {
                        ["--verbose"] = true,
                    },
                    arguments: new[]
                    {
                        "origin",
                        branch,
                    }
                );

                await command.RunOrThrowError(
                    errorMessage: "Could not push changes to remote repository.",
                    cancellationToken: cancellationToken
                );
            });

            await Step($"Open PR", async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                var command = new Command(
                    command: "gh pr create",
                    options: new Dictionary<string, object>
                    {
                        ["--title"] = $"\"Update Swagger to spec version {options.Version}\"",
                        ["--body"] = $"\"This is an automated update of the swagger spec to spec version {options.Version}.\"",
                    }
                );

                await command.RunOrThrowError(
                    errorMessage: "Could not open PR.",
                    cancellationToken: cancellationToken
                );
            });

            await Step($"Set PR Merge Options", async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                var command = new Command(
                    command: "gh pr merge",
                    options: new Dictionary<string, object>
                    {
                        ["--squash"] = true,
                        ["--auto"] = true,
                    }
                );

                await command.RunOrThrowError(
                    errorMessage: "Could set PR merge options.",
                    cancellationToken: cancellationToken
                );

                Console.WriteLine("Setup PR to squash-merge automatically once commit checks pass.");
            });

            lifetime.StopApplication();
        }

        /// <inheritdoc />
        public Task StopAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        private static void PrintHeading(string title)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n{title} ==========\n");
            Console.ResetColor();
        }

        private static async Task Step(string title, Func<Task> action)
        {
            PrintHeading(title);
            await action();
        }

        private static async Task<TOutput> Step<TOutput>(string title, Func<Task<TOutput>> action)
        {
            PrintHeading(title);

            return await action();
        }
    }
}
