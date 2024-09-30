using System;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using static ED.DomainServices.Esb.Esb;

namespace ED.Blobs
{
    public class EsbAuthHandler : AuthenticationHandler<EsbAuthenticationOptions>
    {
        private readonly EsbClient esbClient;

        public EsbAuthHandler(
            EsbClient esbClient,
            IOptionsMonitor<EsbAuthenticationOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
            this.esbClient = esbClient;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // https://github.com/referbruv/CustomSchemeNinja/blob/main/CustomSchemeNinjaApi/Providers/AuthHandlers/MyNinjaAuthHandler.cs
            if (!this.Request.Headers.ContainsKey(EsbAuthSchemeConstants.DpMiscinfoHeader))
            {
                return await Task.FromResult(AuthenticateResult.Fail("Header Not Found."));
            }

            try
            {
                HeaderInfo headerInfo = this.ParseDpMiscinfo(
                    this.Request.Headers[EsbAuthSchemeConstants.DpMiscinfoHeader].ToString());

                DomainServices.Esb.GetEsbUserResponse resp =
                    await this.esbClient.GetEsbUserAsync(
                        new DomainServices.Esb.GetEsbUserRequest
                        {
                            OId = headerInfo.OId,
                            ClientId = headerInfo.ClientId,
                            OperatorIdentifier = headerInfo.OperatorId,
                            RepresentedProfileIdentifier = headerInfo.RepresentedProfileIdentifier,
                        });

                if (resp.Result == null)
                {
                    return await Task.FromResult(AuthenticateResult.Fail("No esb user found"));
                }

                Claim[] claims = new Claim[]
                {
                    new(ClaimTypes.NameIdentifier, resp.Result.ProfileId.ToString()),
                    new(EsbAuthClaimTypes.LoginId, resp.Result.LoginId.ToString()),
                    new(EsbAuthClaimTypes.OperatorLoginId, resp.Result.OperatorLoginId?.ToString() ?? string.Empty),
                    new(EsbAuthClaimTypes.RepresentedProfileId, resp.Result.RepresentedProfileId?.ToString() ?? string.Empty),

                    new(EsbAuthClaimTypes.OId, headerInfo.OId),
                    new(EsbAuthClaimTypes.ClientId, headerInfo.ClientId),
                    new(EsbAuthClaimTypes.OperatorId, headerInfo.OperatorId),
                    new(EsbAuthClaimTypes.RepresentedProfileIdentifier, headerInfo.RepresentedProfileIdentifier),
                };

                ClaimsIdentity claimsIdentity = new(claims, nameof(EsbAuthHandler));

                AuthenticationTicket ticket = new(
                    new ClaimsPrincipal(claimsIdentity),
                    this.Scheme.Name);

                return await Task.FromResult(AuthenticateResult.Success(ticket));
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                return await Task.FromResult(AuthenticateResult.Fail("IdentityException"));
            }
        }

        private record HeaderInfo(
            string OId,
            string ClientId,
            string RepresentedProfileIdentifier,
            string OperatorId);
        private HeaderInfo ParseDpMiscinfo(string header)
        {
            // example for Dp-Miscinfo header
            // dn:/C=BG/ST=OID:2.16.100.1.1.22.1.3/CN=test.client.morska|representedPersonID:8507270464|correspondentOID:2222|operatorID=12345
            // or
            // dn:/C=BG/ST=OID:2.16.100.1.1.1.1.13/L=BG/CN=ciela.com|representedPersonID:|correspondentOID:|operatorID:

            string[] values = header
                .Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

            string[] identity = values[0]
            .Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries)
            .ToArray();

            string oidValue = identity.First(e => e.StartsWith("ST=OID", StringComparison.InvariantCultureIgnoreCase));
            string clientIdValue = identity.First(e => e.StartsWith("CN=", StringComparison.InvariantCultureIgnoreCase));

            string oId = oidValue
                .Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(e => e.Trim())
                .Last();

            string clientId = clientIdValue
                .Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(e => e.Trim())
                .Last();

            string representedPersonId = values[1]
                .Split(new char[] { ':' })
                .Select(e => e.Trim())
                .Last();

            string representedProfileIdentifier = string.Empty;

            if (!string.IsNullOrWhiteSpace(representedPersonId))
            {
                Match match = Regex.Match(representedPersonId, @"\d+");
                representedProfileIdentifier = match.Value;
            }

            string operatorId = values[3]
                .Split(new char[] { ':' })
                .Select(e => e.Trim())
                .Last();

            return new HeaderInfo(oId, clientId, representedProfileIdentifier, operatorId);
        }
    }

    public static class EsbExtensions
    {
        public static AuthenticationBuilder AddEsb(this AuthenticationBuilder builder)
            => builder.AddEsb(EsbAuthSchemeConstants.EsbAuthScheme);

        public static AuthenticationBuilder AddEsb(
            this AuthenticationBuilder builder,
            string authenticationScheme)
            => builder.AddEsb(authenticationScheme, configureOptions: null!);

        public static AuthenticationBuilder AddEsb(
            this AuthenticationBuilder builder,
            string authenticationScheme,
            Action<EsbAuthenticationOptions> configureOptions)
            => builder.AddEsb(authenticationScheme, displayName: null, configureOptions: configureOptions);

        public static AuthenticationBuilder AddEsb(
            this AuthenticationBuilder builder,
            string authenticationScheme,
            string? displayName,
            Action<EsbAuthenticationOptions> configureOptions)
        {
            return builder.AddScheme<EsbAuthenticationOptions, EsbAuthHandler>(
                authenticationScheme,
                displayName,
                configureOptions);
        }
    }
}
