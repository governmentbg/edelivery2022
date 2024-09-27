using EDelivery.WebPortal.Controllers;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security.DataProtection;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Routing;

namespace EDelivery.WebPortal
{
    public static class BlobUrlCreator
    {
        private const string MessageBlobTokenPurpose = "ED.Blobs.MessageBlobToken";
        private const string ProfileBlobTokenPurpose = "ED.Blobs.ProfileBlobToken";
        private const string MessageBlobWebPortalTokenPurpose = "EDelivery.WebPortal.MessageBlobWebPortalToken";
        private const string ProfileBlobWebPortalTokenPurpose = "EDelivery.WebPortal.ProfileBlobWebPortalToken";
        private const string MessageBlobAccessCodeTokenPurpose = "EDelivery.WebPortal.MessageBlobAccessCodeToken";

        private static readonly Lazy<ITimeLimitedDataProtector> MessageBlobTokenDataProtector =
            new Lazy<ITimeLimitedDataProtector>(
                () => AuthConfig.DataProtectionProvider.CreateTimeLimitedDataProtector(MessageBlobTokenPurpose),
                LazyThreadSafetyMode.ExecutionAndPublication);
        private static readonly Lazy<ITimeLimitedDataProtector> ProfileBlobTokenDataProtector =
            new Lazy<ITimeLimitedDataProtector>(
                () => AuthConfig.DataProtectionProvider.CreateTimeLimitedDataProtector(ProfileBlobTokenPurpose),
                LazyThreadSafetyMode.ExecutionAndPublication);
        private static readonly Lazy<IDataProtector> MessageBlobWebPortalTokenDataProtector =
            new Lazy<IDataProtector>(
                () => AuthConfig.DataProtectionProvider.Create(MessageBlobWebPortalTokenPurpose),
                LazyThreadSafetyMode.ExecutionAndPublication);
        private static readonly Lazy<IDataProtector> ProfileBlobWebPortalTokenDataProtector =
            new Lazy<IDataProtector>(
                () => AuthConfig.DataProtectionProvider.Create(ProfileBlobWebPortalTokenPurpose),
                LazyThreadSafetyMode.ExecutionAndPublication);
        private static readonly Lazy<IDataProtector> MessageBlobAccessCodeTokenDataProtector =
            new Lazy<IDataProtector>(
                () => AuthConfig.DataProtectionProvider.Create(MessageBlobAccessCodeTokenPurpose),
                LazyThreadSafetyMode.ExecutionAndPublication);

        public static string CreateMessageBlobUrl(int profileId, int messageId, int blobId)
        {
            return CreateBlobUrl(
                MessageBlobTokenDataProtector.Value,
                "message",
                profileId,
                messageId,
                blobId);
        }

        public static string CreateProfileBlobUrl(int profileId, int blobId)
        {
            return CreateBlobUrl(
                ProfileBlobTokenDataProtector.Value,
                "profile",
                profileId,
                blobId);
        }

        public static string CreateMessageBlobWebPortalToken(
            this UrlHelper urlHelper,
            int profileId,
            int messageId,
            int blobId)
        {
            return CreateBlobWebPortalToken(
                urlHelper,
                "Storage",
                nameof(StorageController.DownloadMessageBlob),
                StorageController.DownloadActionQueryStringKey,
                MessageBlobWebPortalTokenDataProtector.Value,
                profileId,
                messageId,
                blobId);
        }

        public static string CreateMessageBlobWebPortalToken(
            this HttpContextBase httpContext,
            int profileId,
            int messageId,
            int blobId)
        {
            return CreateMessageBlobWebPortalToken(
                new UrlHelper(httpContext.Request.RequestContext),
                profileId,
                messageId,
                blobId);
        }

        public static string CreateProfileBlobWebPortalPath(
            this UrlHelper urlHelper,
            int profileId,
            int blobId)
        {
            return CreateBlobWebPortalToken(
                urlHelper,
                "Storage",
                nameof(StorageController.DownloadProfileBlob),
                StorageController.DownloadActionQueryStringKey,
                ProfileBlobWebPortalTokenDataProtector.Value,
                profileId,
                blobId);
        }

        public static string CreateProfileBlobWebPortalPath(
            this HttpContextBase httpContext,
            int profileId,
            int blobId)
        {
            return CreateProfileBlobWebPortalPath(
                new UrlHelper(httpContext.Request.RequestContext),
                profileId,
                blobId);
        }

        private static readonly Type[] MessageBlobTokenValueTypes =
            new[] { typeof(int), typeof(int), typeof(int) };
        public static bool TryParseMessageBlobWebPortalToken(
            HttpContextBase httpContext,
            string token,
            out int profileId,
            out int messageId,
            out int blobId)
        {
            bool result = TryParseBlobWebPortalToken(
                httpContext,
                MessageBlobWebPortalTokenDataProtector.Value,
                token,
                MessageBlobTokenValueTypes,
                out var values);

            if (!result)
            {
                profileId = 0;
                messageId = 0;
                blobId = 0;
                return false;
            }

            profileId = (int)values[0];
            messageId = (int)values[1];
            blobId = (int)values[2];
            return true;
        }

        private static readonly Type[] ProfileBlobTokenValueTypes =
            new[] { typeof(int), typeof(int) };
        public static bool TryParseProfileBlobWebPortalToken(
            HttpContextBase httpContext,
            string token,
            out int profileId,
            out int blobId)
        {
            bool result = TryParseBlobWebPortalToken(
                httpContext,
                ProfileBlobWebPortalTokenDataProtector.Value,
                token,
                ProfileBlobTokenValueTypes,
                out var values);

            if (!result)
            {
                profileId = 0;
                blobId = 0;
                return false;
            }

            profileId = (int)values[0];
            blobId = (int)values[1];
            return true;
        }

        public static string CreateMessageBlobAccessCodeToken(
            Guid accessCode,
            int profileId,
            int messageId,
            int blobId)
        {
            byte[] protectedData = CompactPositionalSerializer.Serialize(
                accessCode,
                profileId,
                messageId,
                blobId);

            byte[] tokenBytes =
                MessageBlobAccessCodeTokenDataProtector.Value.Protect(
                    protectedData);

            return ToUrlSafeBase64(tokenBytes);
        }

        private static readonly Type[] MessageBlobAccessCodeTokenValueTypes =
            new[] { typeof(Guid), typeof(int), typeof(int), typeof(int) };
        public static (Guid accessCode, int profileId, int messageId, int blobId)
            ParseMessageBlobAccessCodeToken(string token)
        {
            byte[] tokenBytes = FromUrlSafeBase64(token);

            byte[] protectedData =
                MessageBlobAccessCodeTokenDataProtector.Value.Unprotect(tokenBytes);

            object[] tokenValues =
                CompactPositionalSerializer.Deserialize(
                    protectedData,
                    MessageBlobAccessCodeTokenValueTypes);

            return (
                accessCode: (Guid)tokenValues[0],
                profileId: (int)tokenValues[1],
                messageId: (int)tokenValues[2],
                blobId: (int)tokenValues[3]);
        }

        private static string CreateBlobUrl(
            ITimeLimitedDataProtector dataProtector,
            string path,
            params object[] tokenValues)
        {
            string blobServiceWebUrl =
                WebConfigurationManager.AppSettings["BlobServiceWebUrl"];
            string blobTokenLifetimeInMinutesString =
                WebConfigurationManager.AppSettings["BlobTokenLifetimeInMinutes"];

            if (string.IsNullOrEmpty(blobServiceWebUrl))
            {
                throw new Exception("Missing AppSettings.BlobServiceWebUrl");
            }

            if (string.IsNullOrEmpty(blobTokenLifetimeInMinutesString) ||
                !int.TryParse(blobTokenLifetimeInMinutesString, out int blobTokenLifetimeInMinutes))
            {
                throw new Exception("Missing or incorrect AppSettings.BlobTokenLifetimeInMinutes");
            }

            byte[] protectedData = CompactPositionalSerializer.Serialize(tokenValues);

            byte[] tokenBytes =
                dataProtector.Protect(
                    protectedData,
                    TimeSpan.FromMinutes(blobTokenLifetimeInMinutes));

            string token = ToUrlSafeBase64(tokenBytes);

            return $"{blobServiceWebUrl.TrimEnd('/')}/{path.Trim('/')}?t={token}";
        }

        private static string CreateBlobWebPortalToken(
            UrlHelper urlHelper,
            string controllerName,
            string actionName,
            string tokenRouteKey,
            IDataProtector dataProtector,
            params object[] values)
        {
            var user = urlHelper.RequestContext.HttpContext.User;
            if (user?.Identity?.IsAuthenticated != true)
            {
                throw new Exception("CreateBlobWebPortalToken can be used only for authenticated users.");
            }

            int loginId = user.Identity.GetUserId<int>();

            object[] tokenValues = Prepend(values, loginId);

            byte[] protectedData = CompactPositionalSerializer.Serialize(tokenValues);

            byte[] tokenBytes = dataProtector.Protect(protectedData);

            string token = ToUrlSafeBase64(tokenBytes);

            return urlHelper.Action(
                actionName,
                controllerName,
                new RouteValueDictionary()
                {
                    { tokenRouteKey, token }
                });
        }

        private static bool TryParseBlobWebPortalToken(
            HttpContextBase httpContext,
            IDataProtector dataProtector,
            string token,
            Type[] valueTypes,
            out object[] values)
        {
            var user = httpContext.User;
            if (user?.Identity?.IsAuthenticated != true)
            {
                values = Array.Empty<object>();
                return false;
            }

            byte[] tokenBytes = FromUrlSafeBase64(token);

            byte[] protectedData;
            try
            {
                protectedData = dataProtector.Unprotect(tokenBytes);
            }
            catch (CryptographicException)
            {
                values = Array.Empty<object>();
                return false;
            }

            Type[] tokenValueTypes = Prepend(valueTypes, typeof(int));

            object[] tokenValues =
                CompactPositionalSerializer.Deserialize(
                    protectedData,
                    tokenValueTypes);

            int loginId = (int)tokenValues[0];
            if (user.Identity.GetUserId<int>() != loginId)
            {
                values = Array.Empty<object>();
                return false;
            }

            values = tokenValues.Skip(1).ToArray();
            return true;
        }

        private static string ToUrlSafeBase64(byte[] data)
            => Convert.ToBase64String(data).TrimEnd('=').Replace('+', '-').Replace('/', '_');

        private static byte[] FromUrlSafeBase64(string dataString)
        {
            string incoming = dataString.Replace('_', '/').Replace('-', '+');

            switch (dataString.Length % 4)
            {
                case 2:
                    incoming += "==";
                    break;
                case 3:
                    incoming += "=";
                    break;
            }

            return Convert.FromBase64String(incoming);
        }

        private static T[] Prepend<T>(T[] array, T item)
        {
            T[] newArray = new T[array.Length + 1];
            newArray[0] = item;
            Array.Copy(array, 0, newArray, 1, array.Length);

            return newArray;
        }
    }
}
