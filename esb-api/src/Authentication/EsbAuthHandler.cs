using System;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using static ED.DomainServices.Esb.Esb;

namespace ED.EsbApi;

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
                        RepresentedProfileIdentifier = headerInfo.RepresentedPersonId,
                    });

            Claim[] claims = new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, resp.ProfileId.ToString()),
                new Claim(EsbAuthClaimTypes.LoginId, resp.LoginId.ToString()),
                new Claim(EsbAuthClaimTypes.OperatorLoginId, resp.OperatorLoginId?.ToString() ?? string.Empty),
                new Claim(EsbAuthClaimTypes.RepresentedProfileId, resp.RepresentedProfileId?.ToString() ?? string.Empty),

                new Claim(EsbAuthClaimTypes.OId, headerInfo.OId),
                new Claim(EsbAuthClaimTypes.ClientId, headerInfo.ClientId),
                new Claim(EsbAuthClaimTypes.OperatorId, headerInfo.OperatorId),
                new Claim(EsbAuthClaimTypes.RepresentedPersonId, headerInfo.RepresentedPersonId),
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
        string RepresentedPersonId,
        string OperatorId);
    private HeaderInfo ParseDpMiscinfo(string header)
    {
        // example for Dp-Miscinfo header
        // dn:/C=BG/ST=OID:2.16.100.1.1.22.1.3/CN=test.client.morska|representedPersonID:8507270464|correspondentOID:2222|operatorID=12345

        string[] values = header
            .Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

        string[] identity = values[0]
            .Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries)
            .ToArray()[^2..^0];

        string oId = identity[0]
            .Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(e => e.Trim())
            .Last();

        string clientId = identity[1]
            .Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(e => e.Trim())
            .Last();

        string representedPersonId = values[1]
            .Split(new char[] { ':' })
            .Select(e => e.Trim())
            .Last();

        string operatorId = values[3]
            .Split(new char[] { '=' })
            .Select(e => e.Trim())
            .Last();

        return new HeaderInfo(oId, clientId, representedPersonId, operatorId);
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
