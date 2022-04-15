using System;
using System.Security.Cryptography;
using Microsoft.Owin.Security.DataProtection;

namespace EDelivery.WebPortal
{
    public class LocalMachineDpapiDataProtector : IDataProtector
    {
        private readonly DpapiDataProtector protector;

        internal LocalMachineDpapiDataProtector(params string[] purposes)
        {
            if (purposes == null ||
                purposes.Length == 0)
            {
                throw new ArgumentException("At least one purpose should be specified", nameof(purposes));
            }

            string[] specificPurpose;
            if (purposes.Length == 1)
            {
                specificPurpose = Array.Empty<string>();
            }
            else
            {
                specificPurpose = new string[purposes.Length - 1];
                Array.Copy(purposes, 1, specificPurpose, 0, purposes.Length - 1);
            }

            protector = new DpapiDataProtector(
                purposes[0],
                "Microsoft.Owin.Security.DataProtection.IDataProtector",
                specificPurpose)
            {
                Scope = DataProtectionScope.LocalMachine
            };
        }

        public byte[] Protect(byte[] userData)
        {
            return protector.Protect(userData);
        }

        public byte[] Unprotect(byte[] protectedData)
        {
            return protector.Unprotect(protectedData);
        }
    }
}