using System.Threading;

namespace ED.DomainJobs
{
    public class BatchCounter
    {
        private int successes;
        private int failures;
        private int cancels;

        public int Successes => this.successes;
        public int Failures => this.failures;
        public int Cancels => this.cancels;

        public void CountSuccess()
        {
            Interlocked.Increment(ref this.successes);
        }

        public void CountFailure()
        {
            Interlocked.Increment(ref this.failures);
        }

        public void CountCancel()
        {
            Interlocked.Increment(ref this.cancels);
        }
    }
}
