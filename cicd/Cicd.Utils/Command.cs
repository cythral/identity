using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Brighid.Identity.Cicd.Utils
{
    /// <summary>
    /// Represents a command to be run.
    /// </summary>
    public class Command
    {
        private readonly ProcessStartInfo startInfo;

        /// <summary>
        /// Initializes a new instance of the <see cref="Command" /> class.
        /// </summary>
        /// <param name="command">The command to run.</param>
        /// <param name="options">Options to use for the command.</param>
        /// <param name="arguments">Arguments to pass to the command.</param>
        public Command(
            string command,
            IDictionary<string, object>? options = null,
            string[]? arguments = null
        )
        {
            var commandParts = command.Split(' ');
            var args = new List<string>();

            if (commandParts.Length > 1)
            {
                args.AddRange(commandParts[1..]);
            }

            if (options != null)
            {
                foreach (var (key, value) in options)
                {
                    args.Add(key);

                    if (value is string stringValue)
                    {
                        args.Add(stringValue);
                    }
                }
            }

            if (arguments != null)
            {
                args.AddRange(arguments);
            }

            startInfo = new ProcessStartInfo(commandParts[0], string.Join(' ', args))
            {
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                StandardInputEncoding = Encoding.ASCII,
            };
        }

        /// <summary>
        /// Runs the command to completion and throws an exception if the command was not successful.
        /// </summary>
        /// <param name="errorMessage">The error message to include in the exception if the command fails.</param>
        /// <param name="input">Text to write to standard input.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The resulting task.</returns>
        /// <exception cref="Exception">Thrown if the command fails.</exception>
        public async Task<string> RunOrThrowError(string errorMessage, string? input = null, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using var process = Process.Start(startInfo)!;
            if (input != null)
            {
                await process!.StandardInput.WriteAsync(input);
                await process!.StandardInput.FlushAsync();
                process!.StandardInput.Close();
            }

            await process.WaitForExitAsync(cancellationToken);
            var output = await process!.StandardOutput.ReadToEndAsync();
            Console.Write(output);

            return process.ExitCode == 0
                ? output
                : throw new Exception(errorMessage);
        }
    }
}
