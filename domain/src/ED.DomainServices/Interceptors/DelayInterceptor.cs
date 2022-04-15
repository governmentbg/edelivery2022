using System;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Core.Interceptors;

namespace ED.DomainServices
{
    public class DelayInterceptor : Interceptor
    {
        private readonly TimeSpan delay;

        public DelayInterceptor(TimeSpan delay)
        {
            this.delay = delay;
        }

        // delaying only unary methods for simplicity
        public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
            TRequest request,
            ServerCallContext context,
            UnaryServerMethod<TRequest, TResponse> continuation)
        {
            await Task.Delay(this.delay);
            return await continuation(request, context);
        }
    }
}
