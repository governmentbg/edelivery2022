using System;
using System.IO;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.DataProtection;

#nullable enable
#pragma warning disable SYSLIB0021 // Type or member is obsolete

namespace ED.EsbApi;

[SupportedOSPlatform("windows")]
public class LocalMachineDpapiDataProtector : IDataProtector
{
    private string[] purposes;
    private DataProtectionScope scope;
    private byte[]? hashedPurpose;

    public LocalMachineDpapiDataProtector(params string[] purposes)
    {
        this.purposes = purposes;
        this.scope = DataProtectionScope.LocalMachine;
    }

    public IDataProtector CreateProtector(string purpose)
    {
        string[] newPurposes;
        if (this.purposes != null)
        {
            // create a new array with the new purpose at the end
            newPurposes = new string[this.purposes.Length + 1];
            Array.Copy(this.purposes, newPurposes, this.purposes.Length);
            newPurposes[this.purposes.Length] = purpose;
        }
        else
        {
            newPurposes = new[] { purpose };
        }

        return new LocalMachineDpapiDataProtector(newPurposes);
    }

    public byte[] Protect(byte[] plaintext)
    {
        return ProtectedData.Protect(
            plaintext,
            this.GetHashedPurpose(),
            this.scope);
    }

    public byte[] Unprotect(byte[] protectedData)
    {
        return ProtectedData.Unprotect(
            protectedData,
            this.GetHashedPurpose(),
            this.scope);
    }

    private byte[] GetHashedPurpose()
    {
        if (this.purposes == null ||
            this.purposes.Length == 0)
        {
            throw new Exception("At least one purpose should be specified");
        }

        string[] owinDPPurposes = new string[this.purposes.Length + 1];

        owinDPPurposes[0] = this.purposes[0];
        owinDPPurposes[1] = "Microsoft.Owin.Security.DataProtection.IDataProtector";
        for (int i = 1; i < this.purposes.Length; i++)
        {
            owinDPPurposes[i + 1] = this.purposes[i];
        }

        // adapted from the System.Security.Cryptography.DataProtector reference source
        // https://referencesource.microsoft.com/#System.Security/system/security/cryptography/dataprotector.cs

        if (this.hashedPurpose == null)
        {
            // Compute hash of the full purpose.
            // The full purpose is a concatination of all the parts -
            // applicationName, primaryPurpose,and specificPurposes[].
            // We prefix each part with the length so we know the process is
            // reversible
            using SHA256Managed sha256 = new();

            using (BinaryWriter stream =
                new(
                    new CryptoStream(
                        new MemoryStream(),
                        sha256,
                        CryptoStreamMode.Write),
                    new UTF8Encoding(false, true)))
            {
                foreach (string purpose in owinDPPurposes)
                {
                    stream.Write(purpose);
                }
            }

            // Now that the CryptoStream is closed,
            // sha256 should have the computed hash
            this.hashedPurpose = sha256.Hash!;
        }

        return this.hashedPurpose;
    }
}
