using log4net;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EDelivery.SEOS.Jobs
{
    public abstract class Job
    {
        protected static ILog logger = LogManager.GetLogger("SEOSJobLogger");
        private CancellationTokenSource source;
        private object _lockObj;

        private bool isStarted;
        public bool IsStarted
        {
            get { return this.isStarted; }
        }

        public string Name { get; set; }

        public Job(string name)
        {
            this._lockObj = new object();
            this.Name = name;
            this.isStarted = false;
            this.source = new CancellationTokenSource();

            log4net.Config.XmlConfigurator.Configure();
        }


        public void Start(TimeSpan interval)
        {
            if (isStarted)
            {
                logger.ErrorFormat("Job {0} is already started!", this.Name);
                return;
            }

            try
            {
                var token = source.Token;
                //start the job
                if (Monitor.TryEnter(_lockObj))
                {
                    //start the job
                    Task.Factory.StartNew(() =>
                    {
                        while (true)
                        {
                            try
                            {

                                if (token.IsCancellationRequested)
                                {
                                    logger.ErrorFormat("Cancelation of taks {0} has been requested. Exiting...", this.Name);
                                    return;
                                }
                                //run the method
                                logger.InfoFormat("Starting execution of {0}", this.Name);
                                Execute();
                                logger.InfoFormat("Finished execution of {0}. Delay for {1} minutes. Next execution should be at: {2}", this.Name, interval.TotalMinutes, DateTime.Now.Add(interval).ToString());
                                Task.Delay(interval, token).Wait();
                            }
                            catch (AggregateException exx)
                            {
                                logger.Error("Task cancellation occured " + this.Name, exx.Flatten());
                                return;
                            }
                            catch (Exception ex)
                            {
                                logger.Error("Exception in execute method of Job " + this.Name, ex);
                            }
                        };

                    }, token,
                    TaskCreationOptions.AttachedToParent | TaskCreationOptions.LongRunning,
                    TaskScheduler.Default);
                }

                this.isStarted = true;
                logger.InfoFormat("Job {0} is successfully started!", Name);
            }
            catch (Exception ex)
            {
                logger.Error("Exception while starting job " + Name, ex);
            }
            finally
            {
                Monitor.Exit(_lockObj);
            }
        }

        public void Stop()
        {
            if (!isStarted)
            {
                logger.ErrorFormat("Job {0} is not started!", this.Name);
                return;
            }

            this.source.Cancel(false);
        }

        protected abstract void Execute();

        public void Dispose()
        {
            try
            {
                if (isStarted)
                {
                    this.Stop();
                }
            }
            finally
            {
                this.source.Dispose();
            }
        }

    }
}
