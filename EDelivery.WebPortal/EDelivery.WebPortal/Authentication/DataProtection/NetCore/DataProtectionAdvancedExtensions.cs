using Microsoft.Owin.Security.DataProtection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EDelivery.WebPortal
{
    public static class DataProtectionAdvancedExtensions
    {
        // adapted from https://github.com/dotnet/aspnetcore/blob/main/src/DataProtection/Extensions/src/DataProtectionAdvancedExtensions.cs

        /// <summary>
        /// Cryptographically protects a piece of plaintext data, expiring the data after
        /// the specified amount of time has elapsed.
        /// </summary>
        /// <param name="protector">The protector to use.</param>
        /// <param name="plaintext">The plaintext data to protect.</param>
        /// <param name="lifetime">The amount of time after which the payload should no longer be unprotectable.</param>
        /// <returns>The protected form of the plaintext data.</returns>
        public static byte[] Protect(this ITimeLimitedDataProtector protector, byte[] plaintext, TimeSpan lifetime)
        {
            if (protector == null)
            {
                throw new ArgumentNullException(nameof(protector));
            }

            if (plaintext == null)
            {
                throw new ArgumentNullException(nameof(plaintext));
            }

            return protector.Protect(plaintext, DateTimeOffset.UtcNow + lifetime);
        }

        /// <summary>
        /// Cryptographically unprotects a piece of protected data.
        /// </summary>
        /// <param name="protector">The protector to use.</param>
        /// <param name="protectedData">The protected data to unprotect.</param>
        /// <param name="expiration">An 'out' parameter which upon a successful unprotect
        /// operation receives the expiration date of the payload.</param>
        /// <returns>The plaintext form of the protected data.</returns>
        /// <exception cref="System.Security.Cryptography.CryptographicException">
        /// Thrown if <paramref name="protectedData"/> is invalid, malformed, or expired.
        /// </exception>
        public static byte[] Unprotect(this ITimeLimitedDataProtector protector, byte[] protectedData, out DateTimeOffset expiration)
        {
            if (protector == null)
            {
                throw new ArgumentNullException(nameof(protector));
            }

            if (protectedData == null)
            {
                throw new ArgumentNullException(nameof(protectedData));
            }

            var wrappingProtector = new TimeLimitedWrappingProtector(protector);
            byte[] retVal = wrappingProtector.Unprotect(protectedData);
            expiration = wrappingProtector.Expiration;
            return retVal;
        }

        /// <summary>
        /// Create a <see cref="ITimeLimitedDataProtector"/>
        /// so that payloads can be protected with a finite lifetime.
        /// </summary>
        /// <param name="provider">The <see cref="IDataProtectionProvider"/> to convert to a time-limited protector.</param>
        /// <param name="purposes">The purposes to use for the <see cref="IDataProtectionProvider"/></param>
        /// <returns>An <see cref="ITimeLimitedDataProtector"/>.</returns>
        public static ITimeLimitedDataProtector CreateTimeLimitedDataProtector(this IDataProtectionProvider provider, params string[] purposes)
        {
            if (provider == null)
            {
                throw new ArgumentNullException(nameof(provider));
            }

            return new TimeLimitedDataProtector(provider, purposes);
        }

        private sealed class TimeLimitedWrappingProtector : IDataProtector
        {
            public DateTimeOffset Expiration;
            private readonly ITimeLimitedDataProtector _innerProtector;

            public TimeLimitedWrappingProtector(ITimeLimitedDataProtector innerProtector)
            {
                _innerProtector = innerProtector;
            }

            public byte[] Protect(byte[] plaintext)
            {
                if (plaintext == null)
                {
                    throw new ArgumentNullException(nameof(plaintext));
                }

                return _innerProtector.Protect(plaintext, Expiration);
            }

            public byte[] Unprotect(byte[] protectedData)
            {
                if (protectedData == null)
                {
                    throw new ArgumentNullException(nameof(protectedData));
                }

                return _innerProtector.Unprotect(protectedData, out Expiration);
            }
        }
    }
}
