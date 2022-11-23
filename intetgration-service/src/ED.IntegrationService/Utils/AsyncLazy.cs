using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace ED.IntegrationService
{
    // https://devblogs.microsoft.com/pfxteam/asynclazyt/
    public class AsyncLazy<T> : Lazy<Task<T>>
    {
        public AsyncLazy(Func<T> valueFactory) :
            base(() => Task.Factory.StartNew(valueFactory))
        { }

        public AsyncLazy(Func<Task<T>> taskFactory) :
            base(() => Task.Factory.StartNew(() => taskFactory()).Unwrap())
        { }

        public TaskAwaiter<T> GetAwaiter() { return this.Value.GetAwaiter(); }
    }
}
