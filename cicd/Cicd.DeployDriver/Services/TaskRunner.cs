using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Amazon.ECS;
using Amazon.ECS.Model;

using Task = System.Threading.Tasks.Task;

namespace Brighid.Identity.Cicd.DeployDriver
{
    /// <summary>
    /// Utility for running fargate tasks.
    /// </summary>
    public class TaskRunner
    {
        private readonly IAmazonECS ecs = new AmazonECSClient();

        /// <summary>
        /// Runs a task, waits for it to complete, and throws an exception if any of the containers did not exit ok.
        /// </summary>
        /// <param name="request">Request to run a task.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The resulting task.</returns>
        public async Task Run(RunTaskRequest request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var response = await ecs.RunTaskAsync(request, cancellationToken);
            var task = response.Tasks.ElementAt(0).TaskArn;

            await WaitForTaskToStop(task, cancellationToken);
            await EnsureTaskExitedOk(task, cancellationToken);
        }

        private async Task WaitForTaskToStop(string task, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            while ((await GetTaskStatus(task, cancellationToken)) != "STOPPED")
            {
                Console.WriteLine("Waiting for task to complete...");
                await Task.Delay(10000, cancellationToken);
            }
        }

        private async Task EnsureTaskExitedOk(string task, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var request = new DescribeTasksRequest { Cluster = "brighid", Tasks = new List<string> { task } };
            var taskInfo = await ecs.DescribeTasksAsync(request, cancellationToken);

            foreach (var container in taskInfo.Tasks.ElementAt(0).Containers)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (container.ExitCode != 0)
                {
                    throw new Exception(container.Reason);
                }
            }
        }

        private async Task<string> GetTaskStatus(string task, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var request = new DescribeTasksRequest { Cluster = "brighid", Tasks = new List<string> { task } };
            var taskInfo = await ecs.DescribeTasksAsync(request, cancellationToken);
            return taskInfo.Tasks.ElementAt(0).LastStatus;
        }
    }
}
