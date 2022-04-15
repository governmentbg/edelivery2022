using System;

namespace EDelivery.WebPortal.Utils.Exceptions
{
    public class ModelStateException : Exception
    {
        public string Key { get; set; }

        public ModelStateException(string key, string message)
            : base(message)
        {
            this.Key = key;
        }
    }
}
