using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;

namespace Brighid.Identity.Auth
{
    public class CertificateUpdateTimer : IHostedService
    {
        private readonly Timer timer;

        public CertificateUpdateTimer(ICertificateUpdater certificateUpdater)
        {
            timer = new Timer((_) => certificateUpdater.UpdateCertificates());
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            timer.Change(60000, 60000);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            timer.Change(TimeSpan.MaxValue, TimeSpan.MaxValue);
            return Task.CompletedTask;
        }
    }
}
