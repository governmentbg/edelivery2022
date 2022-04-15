using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize
#pragma warning disable CA1063 // Implement IDisposable Correctly
#pragma warning disable CA2213 // Disposable fields should be disposed

namespace ED.DomainJobs
{
    public abstract class PeriodicBackgroundService : IHostedService, IDisposable
    {
        private Task? executingTask;
        private TimeSpan period;

        private readonly CancellationTokenSource stoppingCts = new();
        private readonly CancellationTokenSource stopCts = new();

        protected PeriodicBackgroundService(TimeSpan period)
        {
            this.period = period;
        }

        protected abstract Task ExecuteAsync(
            CancellationToken stoppingToken,
            CancellationToken stopToken);

        public virtual Task StartAsync(CancellationToken cancellationToken)
        {
            if (this.period == TimeSpan.Zero)
            {
                return Task.CompletedTask;
            }

            this.executingTask =
                Task.Run(
                    async () =>
                    {
                        Stopwatch sw = new Stopwatch();
                        do
                        {
                            sw.Restart();

                            await this.ExecuteAsync(
                                this.stoppingCts.Token,
                                this.stopCts.Token);

                            sw.Stop();

                            if (sw.Elapsed < this.period)
                            {
                                try
                                {
                                    await Task.Delay(
                                        this.period - sw.Elapsed,
                                        this.stoppingCts.Token);
                                }
    #pragma warning disable CA1031 // Do not catch general exception types
                                catch
    #pragma warning restore CA1031 // Do not catch general exception types
                                {
                                }
                            }
                        }
                        while (!this.stoppingCts.Token.IsCancellationRequested);
                    },
                    cancellationToken);

            return Task.CompletedTask;
        }

        public virtual async Task StopAsync(CancellationToken cancellationToken)
        {
            // Stop called without start
            if (this.executingTask == null)
            {
                return;
            }

            cancellationToken.Register(() => this.stopCts.Cancel());

            try
            {
                // Signal cancellation to the executing method
                this.stoppingCts.Cancel();
            }
            finally
            {
                await this.executingTask;
            }
        }

        public virtual void Dispose()
        {
            this.stoppingCts.Cancel();
            this.stopCts.Cancel();
        }
    }
}
