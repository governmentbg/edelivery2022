using System.Threading;

namespace ED.DomainJobs
{
    public class BatchCounter
    {
        private int successes;
        private int failures;

        public int Successes => successes;
        public int Failures => failures;

        public void CountSuccess()
        {
            Interlocked.Increment(ref this.successes);
        }

        public void CountFailure()
        {
            Interlocked.Increment(ref this.failures);
        }
    }
}
