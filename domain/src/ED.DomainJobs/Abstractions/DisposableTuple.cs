using System;

namespace ED.DomainJobs
{
    public sealed class DisposableTuple<T1, T2> : IDisposable
    {
        private bool disposed;

        public T1 Item1 { get; private set; }
        public T2 Item2 { get; private set; }
        public DisposableTuple(T1 item1, T2 item2)
        {
            this.Item1 = item1;
            this.Item2 = item2;
        }

        public void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            try
            {
                using (this.Item1 as IDisposable)
                using (this.Item2 as IDisposable)
                {
                }
            }
            finally
            {
                this.disposed = true;
            }
        }
    }

    public sealed class DisposableTuple<T1, T2, T3> : IDisposable
    {
        private bool disposed;

        public T1 Item1 { get; private set; }
        public T2 Item2 { get; private set; }
        public T3 Item3 { get; private set; }
        public DisposableTuple(T1 item1, T2 item2, T3 item3)
        {
            this.Item1 = item1;
            this.Item2 = item2;
            this.Item3 = item3;
        }

        public void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            try
            {
                using (this.Item1 as IDisposable)
                using (this.Item2 as IDisposable)
                using (this.Item3 as IDisposable)
                {
                }
            }
            finally
            {
                this.disposed = true;
            }
        }
    }

    public sealed class DisposableTuple<T1, T2, T3, T4> : IDisposable
    {
        private bool disposed;

        public T1 Item1 { get; private set; }
        public T2 Item2 { get; private set; }
        public T3 Item3 { get; private set; }
        public T4 Item4 { get; private set; }
        public DisposableTuple(T1 item1, T2 item2, T3 item3, T4 item4)
        {
            this.Item1 = item1;
            this.Item2 = item2;
            this.Item3 = item3;
            this.Item4 = item4;
        }

        public void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            try
            {
                using (this.Item1 as IDisposable)
                using (this.Item2 as IDisposable)
                using (this.Item3 as IDisposable)
                using (this.Item4 as IDisposable)
                {
                }
            }
            finally
            {
                this.disposed = true;
            }
        }
    }

    public sealed class DisposableTuple<T1, T2, T3, T4, T5> : IDisposable
    {
        private bool disposed;

        public T1 Item1 { get; private set; }
        public T2 Item2 { get; private set; }
        public T3 Item3 { get; private set; }
        public T4 Item4 { get; private set; }
        public T5 Item5 { get; private set; }
        public DisposableTuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5)
        {
            this.Item1 = item1;
            this.Item2 = item2;
            this.Item3 = item3;
            this.Item4 = item4;
            this.Item5 = item5;
        }

        public void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            try
            {
                using (this.Item1 as IDisposable)
                using (this.Item2 as IDisposable)
                using (this.Item3 as IDisposable)
                using (this.Item4 as IDisposable)
                using (this.Item5 as IDisposable)
                {
                }
            }
            finally
            {
                this.disposed = true;
            }
        }
    }

    public sealed class DisposableTuple<T1, T2, T3, T4, T5, T6> : IDisposable
    {
        private bool disposed;

        public T1 Item1 { get; private set; }
        public T2 Item2 { get; private set; }
        public T3 Item3 { get; private set; }
        public T4 Item4 { get; private set; }
        public T5 Item5 { get; private set; }
        public T6 Item6 { get; private set; }
        public DisposableTuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6)
        {
            this.Item1 = item1;
            this.Item2 = item2;
            this.Item3 = item3;
            this.Item4 = item4;
            this.Item5 = item5;
            this.Item6 = item6;
        }

        public void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            try
            {
                using (this.Item1 as IDisposable)
                using (this.Item2 as IDisposable)
                using (this.Item3 as IDisposable)
                using (this.Item4 as IDisposable)
                using (this.Item5 as IDisposable)
                using (this.Item6 as IDisposable)
                {
                }
            }
            finally
            {
                this.disposed = true;
            }
        }
    }

    public sealed class DisposableTuple<T1, T2, T3, T4, T5, T6, T7> : IDisposable
    {
        private bool disposed;

        public T1 Item1 { get; private set; }
        public T2 Item2 { get; private set; }
        public T3 Item3 { get; private set; }
        public T4 Item4 { get; private set; }
        public T5 Item5 { get; private set; }
        public T6 Item6 { get; private set; }
        public T7 Item7 { get; private set; }
        public DisposableTuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7)
        {
            this.Item1 = item1;
            this.Item2 = item2;
            this.Item3 = item3;
            this.Item4 = item4;
            this.Item5 = item5;
            this.Item6 = item6;
            this.Item7 = item7;
        }

        public void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            try
            {
                using (this.Item1 as IDisposable)
                using (this.Item2 as IDisposable)
                using (this.Item3 as IDisposable)
                using (this.Item4 as IDisposable)
                using (this.Item5 as IDisposable)
                using (this.Item6 as IDisposable)
                using (this.Item7 as IDisposable)
                {
                }
            }
            finally
            {
                this.disposed = true;
            }
        }
    }

    public sealed class DisposableTuple<T1, T2, T3, T4, T5, T6, T7, T8> : IDisposable
    {
        private bool disposed;

        public T1 Item1 { get; private set; }
        public T2 Item2 { get; private set; }
        public T3 Item3 { get; private set; }
        public T4 Item4 { get; private set; }
        public T5 Item5 { get; private set; }
        public T6 Item6 { get; private set; }
        public T7 Item7 { get; private set; }
        public T8 Item8 { get; private set; }
        public DisposableTuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7, T8 item8)
        {
            this.Item1 = item1;
            this.Item2 = item2;
            this.Item3 = item3;
            this.Item4 = item4;
            this.Item5 = item5;
            this.Item6 = item6;
            this.Item7 = item7;
            this.Item8 = item8;
        }

        public void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            try
            {
                using (this.Item1 as IDisposable)
                using (this.Item2 as IDisposable)
                using (this.Item3 as IDisposable)
                using (this.Item4 as IDisposable)
                using (this.Item5 as IDisposable)
                using (this.Item6 as IDisposable)
                using (this.Item7 as IDisposable)
                using (this.Item8 as IDisposable)
                {
                }
            }
            finally
            {
                this.disposed = true;
            }
        }
    }

    public sealed class DisposableTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9> : IDisposable
    {
        private bool disposed;

        public T1 Item1 { get; private set; }
        public T2 Item2 { get; private set; }
        public T3 Item3 { get; private set; }
        public T4 Item4 { get; private set; }
        public T5 Item5 { get; private set; }
        public T6 Item6 { get; private set; }
        public T7 Item7 { get; private set; }
        public T8 Item8 { get; private set; }
        public T9 Item9 { get; private set; }
        public DisposableTuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7, T8 item8, T9 item9)
        {
            this.Item1 = item1;
            this.Item2 = item2;
            this.Item3 = item3;
            this.Item4 = item4;
            this.Item5 = item5;
            this.Item6 = item6;
            this.Item7 = item7;
            this.Item8 = item8;
            this.Item9 = item9;
        }

        public void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            try
            {
                using (this.Item1 as IDisposable)
                using (this.Item2 as IDisposable)
                using (this.Item3 as IDisposable)
                using (this.Item4 as IDisposable)
                using (this.Item5 as IDisposable)
                using (this.Item6 as IDisposable)
                using (this.Item7 as IDisposable)
                using (this.Item8 as IDisposable)
                using (this.Item9 as IDisposable)
                {
                }
            }
            finally
            {
                this.disposed = true;
            }
        }
    }

    public sealed class DisposableTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> : IDisposable
    {
        private bool disposed;

        public T1 Item1 { get; private set; }
        public T2 Item2 { get; private set; }
        public T3 Item3 { get; private set; }
        public T4 Item4 { get; private set; }
        public T5 Item5 { get; private set; }
        public T6 Item6 { get; private set; }
        public T7 Item7 { get; private set; }
        public T8 Item8 { get; private set; }
        public T9 Item9 { get; private set; }
        public T10 Item10 { get; private set; }
        public DisposableTuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7, T8 item8, T9 item9, T10 item10)
        {
            this.Item1 = item1;
            this.Item2 = item2;
            this.Item3 = item3;
            this.Item4 = item4;
            this.Item5 = item5;
            this.Item6 = item6;
            this.Item7 = item7;
            this.Item8 = item8;
            this.Item9 = item9;
            this.Item10 = item10;
        }

        public void Dispose()
        {
            try
            {
                if (this.disposed)
                {
                    return;
                }

                using (this.Item1 as IDisposable)
                using (this.Item2 as IDisposable)
                using (this.Item3 as IDisposable)
                using (this.Item4 as IDisposable)
                using (this.Item5 as IDisposable)
                using (this.Item6 as IDisposable)
                using (this.Item7 as IDisposable)
                using (this.Item8 as IDisposable)
                using (this.Item9 as IDisposable)
                using (this.Item10 as IDisposable)
                {
                }
            }
            finally
            {
                this.disposed = true;
            }
        }
    }

    public sealed class DisposableTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> : IDisposable
    {
        private bool disposed;

        public T1 Item1 { get; private set; }
        public T2 Item2 { get; private set; }
        public T3 Item3 { get; private set; }
        public T4 Item4 { get; private set; }
        public T5 Item5 { get; private set; }
        public T6 Item6 { get; private set; }
        public T7 Item7 { get; private set; }
        public T8 Item8 { get; private set; }
        public T9 Item9 { get; private set; }
        public T10 Item10 { get; private set; }
        public T11 Item11 { get; private set; }
        public DisposableTuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7, T8 item8, T9 item9, T10 item10, T11 item11)
        {
            this.Item1 = item1;
            this.Item2 = item2;
            this.Item3 = item3;
            this.Item4 = item4;
            this.Item5 = item5;
            this.Item6 = item6;
            this.Item7 = item7;
            this.Item8 = item8;
            this.Item9 = item9;
            this.Item10 = item10;
            this.Item11 = item11;
        }

        public void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            try
            {
                using (this.Item1 as IDisposable)
                using (this.Item2 as IDisposable)
                using (this.Item3 as IDisposable)
                using (this.Item4 as IDisposable)
                using (this.Item5 as IDisposable)
                using (this.Item6 as IDisposable)
                using (this.Item7 as IDisposable)
                using (this.Item8 as IDisposable)
                using (this.Item9 as IDisposable)
                using (this.Item10 as IDisposable)
                using (this.Item11 as IDisposable)
                {
                }
            }
            finally
            {
                this.disposed = true;
            }
        }
    }

    public static class DisposableTuple
    {
        public static DisposableTuple<T1, T2> Create<T1, T2>(T1 item1, T2 item2)
        {
            return new DisposableTuple<T1, T2>(item1, item2);
        }

        public static DisposableTuple<T1, T2, T3> Create<T1, T2, T3>(T1 item1, T2 item2, T3 item3)
        {
            return new DisposableTuple<T1, T2, T3>(item1, item2, item3);
        }

        public static DisposableTuple<T1, T2, T3, T4> Create<T1, T2, T3, T4>(T1 item1, T2 item2, T3 item3, T4 item4)
        {
            return new DisposableTuple<T1, T2, T3, T4>(item1, item2, item3, item4);
        }

        public static DisposableTuple<T1, T2, T3, T4, T5> Create<T1, T2, T3, T4, T5>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5)
        {
            return new DisposableTuple<T1, T2, T3, T4, T5>(item1, item2, item3, item4, item5);
        }

        public static DisposableTuple<T1, T2, T3, T4, T5, T6> Create<T1, T2, T3, T4, T5, T6>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6)
        {
            return new DisposableTuple<T1, T2, T3, T4, T5, T6>(item1, item2, item3, item4, item5, item6);
        }

        public static DisposableTuple<T1, T2, T3, T4, T5, T6, T7> Create<T1, T2, T3, T4, T5, T6, T7>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7)
        {
            return new DisposableTuple<T1, T2, T3, T4, T5, T6, T7>(item1, item2, item3, item4, item5, item6, item7);
        }

        public static DisposableTuple<T1, T2, T3, T4, T5, T6, T7, T8> Create<T1, T2, T3, T4, T5, T6, T7, T8>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7, T8 item8)
        {
            return new DisposableTuple<T1, T2, T3, T4, T5, T6, T7, T8>(item1, item2, item3, item4, item5, item6, item7, item8);
        }

        public static DisposableTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9> Create<T1, T2, T3, T4, T5, T6, T7, T8, T9>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7, T8 item8, T9 item9)
        {
            return new DisposableTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>(item1, item2, item3, item4, item5, item6, item7, item8, item9);
        }

        public static DisposableTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7, T8 item8, T9 item9, T10 item10)
        {
            return new DisposableTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10);
        }

        public static DisposableTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7, T8 item8, T9 item9, T10 item10, T11 item11)
        {
            return new DisposableTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11);
        }
    }
}
