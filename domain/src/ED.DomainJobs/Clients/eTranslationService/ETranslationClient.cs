using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;

namespace ED.DomainJobs
{
    public class ETranslationClient
    {
        private readonly HttpClient httpClient;
        private readonly ETranslationOptions options;

        public ETranslationClient(
            HttpClient httpClient,
            IOptions<ETranslationOptions> options)
        {
            this.httpClient = httpClient;
            this.options = options.Value;
        }

        private readonly string method = "POST";

        private string? realm;
        private string? nonce;
        private string? qop;
        private string? cnonce;
        private Algorithm md5;
        // private DateTime cnonceDate;

        private int _nc;

        public async Task<string> SubmitAuthorizationRequestAsync(CancellationToken ct)
        {
            var data = new { };
            string jsonData = JsonSerializer.Serialize(data);

            using HttpRequestMessage req = new(HttpMethod.Post, new Uri(this.options.ApiUrl!));
            using StringContent content = new(jsonData, Encoding.UTF8, "application/json");
            req.Content = content;

            HttpResponseMessage resp = await this.httpClient.SendAsync(req, ct);

            if (resp.StatusCode != HttpStatusCode.Unauthorized)
            {
                string? contentString = null;

                try
                {
                    // get the first 1000 bytes as string
                    await resp.Content.LoadIntoBufferAsync(1000);
                    contentString = await resp.Content.ReadAsStringAsync(ct);
                }
#pragma warning disable CA1031 // Do not catch general exception types
                catch
#pragma warning restore CA1031 // Do not catch general exception types
                {
                    // this a best effort attempt, ignore all errors and continue
                }

                throw new ETranslationException($"Received http error response. StatusCode: {resp.StatusCode}.\nContent: {contentString}");
            }

            string wwwAuthenticateHeader = resp.Headers
                .GetValues("WWW-Authenticate")
                .ToList<string>()
                .First();

            this.realm = this.GetDigestHeaderAttribute("realm", wwwAuthenticateHeader);
            this.nonce = this.GetDigestHeaderAttribute("nonce", wwwAuthenticateHeader);
            this.qop = this.GetDigestHeaderAttribute("qop", wwwAuthenticateHeader);
            this.md5 = this.GetMD5Algorithm(wwwAuthenticateHeader);

            // Generate a new nonce
            this._nc = 0;
#pragma warning disable CA5394 // Do not use insecure randomness
            this.cnonce = new Random().Next(123400, 9999999).ToString();
#pragma warning restore CA5394 // Do not use insecure randomness
            // this.cnonceDate = DateTime.Now;

            string authorizationHeader = this.ComputeDigestHeader(new Uri(this.options.ApiUrl!));
            return authorizationHeader;
        }

        public class TranslateDocumentRequest
        {
            [JsonPropertyName("content")]
            public string Content { get; set; } = null!;

            [JsonPropertyName("format")]
            public string Format { get; set; } = null!;

            [JsonPropertyName("file-name")]
            public string Filename { get; set; } = null!;
        }

        public async Task<string> SubmitDocumentAsync(
            byte[] content,
            string format,
            string filename,
            string sourceLanguage,
            string[] targetLanguages,
            string authorizationHeader,
            string blobAuthenticationToken,
            CancellationToken ct)
        {
            var data = new
            {
                callerInformation = new
                {
                    application = this.options.Username!,
                    username = "System"
                },
                documentToTranslateBase64 = new TranslateDocumentRequest
                {
                    Content = Convert.ToBase64String(content),
                    Format = format,
                    Filename = filename,
                },
                sourceLanguage,
                targetLanguages,
                destinations = new
                {
                    httpDestinations = new string[]
                    {
                        $"{this.options.DocumentCallbackUrl!}?t={blobAuthenticationToken}"
                    },
                },
            };

            this.httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Digest", authorizationHeader);

            string jsonData = JsonSerializer.Serialize(data);

            using HttpRequestMessage req = new(HttpMethod.Post, new Uri(this.options.ApiUrl!));
            using StringContent stringContent = new(jsonData, Encoding.UTF8, "application/json");
            req.Content = stringContent;

            HttpResponseMessage resp = await this.httpClient.SendAsync(req, ct);

            string requestId = await resp.Content.ReadAsStringAsync(ct);

            return requestId;
        }

        /// <summary>
        /// Computes the digest header that should be attached to a request.
        /// </summary>
        /// <returns>The digest header.</returns>
        /// <param name="uri">URI.</param>
        private string ComputeDigestHeader(Uri uri)
        {
            this._nc++;

            string ha1, ha2;

            switch (this.md5)
            {
                // IIS-Specific
                case Algorithm.MD5sess:
                    {
                        string secret = this.ComputeMd5Hash(
                            string.Format(
                                CultureInfo.InvariantCulture,
                                "{0}:{1}:{2}",
                                this.options.Username,
                                this.realm,
                                this.options.Password));

                        ha1 = this.ComputeMd5Hash(
                            string.Format(
                                CultureInfo.InvariantCulture,
                                "{0}:{1}:{2}",
                                secret,
                                this.nonce,

                                this.cnonce));

                        ha2 = this.ComputeMd5Hash(
                            string.Format(
                                CultureInfo.InvariantCulture,
                                "{0}:{1}",
                                this.method,
                                uri.PathAndQuery));

                        string data = string.Format(
                            CultureInfo.InvariantCulture,
                            "{0}:{1:00000000}:{2}:{3}:{4}",
                            this.nonce,
                            this._nc,
                            this.cnonce,
                            this.qop,
                            ha2);

                        string kd = this.ComputeMd5Hash(
                            string.Format(
                                CultureInfo.InvariantCulture,
                                "{0}:{1}",
                                ha1,
                                data));

                        return string.Format(
                            "username=\"{0}\", realm=\"{1}\", nonce=\"{2}\", uri=\"{3}\", " +
                            "algorithm=MD5-sess, response=\"{4}\", qop={5}, nc={6:00000000}, cnonce=\"{7}\"",
                            this.options.Username,
                            this.realm,
                            this.nonce,
                            uri.PathAndQuery,
                            kd,
                            this.qop,
                            this._nc,
                            this.cnonce);
                    }
                // Standard (Apache etc)
                case Algorithm.MD5:
                    {
                        ha1 = this.ComputeMd5Hash(
                            string.Format(
                                "{0}:{1}:{2}",
                                this.options.Username,
                                this.realm,
                                this.options.Password));

                        ha2 = this.ComputeMd5Hash(
                            string.Format("{0}:{1}",
                            this.method,
                            uri.PathAndQuery));

                        string digestResponse = this.ComputeMd5Hash(
                            string.Format(
                                "{0}:{1}:{2:00000000}:{3}:{4}:{5}",
                                ha1,
                                this.nonce,
                                this._nc,
                                this.cnonce,
                                this.qop,
                                ha2));

                        return string.Format(
                            "username=\"{0}\", realm=\"{1}\", nonce=\"{2}\", uri=\"{3}\", " +
                            "algorithm=MD5, response=\"{4}\", qop={5}, nc={6:00000000}, cnonce=\"{7}\"",
                            this.options.Username,
                            this.realm,
                            this.nonce,
                            uri.PathAndQuery,
                            digestResponse,
                            this.qop,
                            this._nc,
                            this.cnonce);
                    }
            }

            throw new Exception("The digest header could not be generated");
        }

        /// <summary>
        /// Computes the md5 hash of a string.
        /// </summary>
        /// <returns>The md5 hash.</returns>
        /// <param name="input">Input.</param>
        private string ComputeMd5Hash(string input)
        {
            byte[] inputBytes = Encoding.ASCII.GetBytes(input);

#pragma warning disable CA5351 // Do Not Use Broken Cryptographic Algorithms
            using MD5 md5 = MD5.Create();
#pragma warning restore CA5351 // Do Not Use Broken Cryptographic Algorithms

            byte[] hash = md5.ComputeHash(inputBytes);
            StringBuilder sb = new();

            foreach (byte b in hash)
            {
                sb.Append(b.ToString("x2"));
            }

            return sb.ToString();
        }

        /// <summary>
        /// Gets the MD 5 algorithm. This is either straight MD5 or MD5-sess
        /// TODO: find out what the diffference is.
        /// </summary>
        /// <returns>The MD 5 algorithm.</returns>
        /// <param name="digestAuthHeader">Digest auth header.</param>
        private Algorithm GetMD5Algorithm(string digestAuthHeader)
        {
            var md5Regex = new Regex(@"algorithm=(?<algo>.*)[,]", RegexOptions.IgnoreCase);
            var md5Attribute = md5Regex.Match(digestAuthHeader);

            if (md5Attribute.Success)
            {

                char[] charSeparator = new char[] { ',' };

                string algorithm = md5Attribute.Result("${algo}").ToLower().Split(charSeparator)[0];

                return algorithm switch
                {
                    "md5-sess" or "\"md5-sess\"" => Algorithm.MD5sess,
                    _ => Algorithm.MD5,
                };
            }

            return Algorithm.MD5;
        }

        /// <summary>
        /// Gets the digest header attribute from the authentication header which 
        ///  appears to be in attribute="value" format
        /// </summary>
        /// <returns>The digest header attribute.</returns>
        /// <param name="attributeName">Attribute name.</param>
        /// <param name="digestAuthHeader">Digest auth header.</param>
        private string GetDigestHeaderAttribute(
            string attributeName,
            string digestAuthHeader)
        {
            var regHeader = new Regex(string.Format(@"{0}=""([^""]*)""", attributeName));
            var matchHeader = regHeader.Match(digestAuthHeader);

            if (matchHeader.Success)
            {
                return matchHeader.Groups[1].Value;
            }

            throw new Exception("Header ${attributeName} not found");
        }

        public enum Algorithm
        {
            MD5 = 0, // Apache Default
            MD5sess = 1 //IIS Default
        }
    }
}
