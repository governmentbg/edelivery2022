using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Mapster;
using static ED.Domain.IMessageOpenHORepository;
using static ED.Domain.IProfilesService;

namespace ED.Domain
{
    partial class MessageOpenHORepository : IMessageOpenHORepository
    {
        public async Task<GetForwardedMessageAsRecipientVO> GetForwardedMessageAsRecipientAsync(
            int messageId,
            int profileId,
            CancellationToken ct)
        {
            IMessageOpenQueryRepository.GetForwardedMessageAsRecipientVO messageVO =
                await this.messageOpenQueryRepository.GetForwardedMessageAsRecipientAsync(
                    messageId,
                    profileId,
                    ct);

            ProfileKeyVO profileKey =
              await this.profilesService.GetProfileKeyAsync(
                  messageVO.RecipientProfileKeyId,
                  ct);

            Keystore.DecryptWithRsaKeyResponse decryptedKeyResp =
              await this.keystoreClient.DecryptWithRsaKeyAsync(
                  request: new Keystore.DecryptWithRsaKeyRequest
                  {
                      Key = new Keystore.RsaKey
                      {
                          Provider = profileKey.Provider,
                          KeyName = profileKey.KeyName,
                          OaepPadding = profileKey.OaepPadding,
                      },
                      EncryptedData = ByteString.CopyFrom(messageVO.RecipientEncryptedKey)
                  },
                  cancellationToken: ct);

            IEncryptor encryptor = this.encryptorFactory.CreateEncryptor(
                decryptedKeyResp.Plaintext.ToByteArray(),
                messageVO.IV);

            string decryptedBody =
                Encoding.UTF8.GetString(encryptor.Decrypt(messageVO.Body));
            
            GetForwardedMessageAsRecipientVO vo =
                messageVO.Adapt<GetForwardedMessageAsRecipientVO>() with
                {
                    Body = decryptedBody
                };

            return vo;
        }
    }
}
