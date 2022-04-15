using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Mapster;
using static ED.Domain.ICodeMessageOpenHORepository;

namespace ED.Domain
{
    partial class CodeMessageOpenHORepository : ICodeMessageOpenHORepository
    {
        public async Task<GetAsRecipientVO> GetAsRecipientAsync(
            int messageId,
            CancellationToken ct)
        {
            ICodeMessageOpenQueryRepository.GetAsRecipientVO messageVO =
                await this.codeMessageOpenQueryRepository.GetAsRecipientAsync(
                    messageId,
                    ct);

            IProfilesService.ProfileKeyVO profileKey =
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

            GetAsRecipientVO vo = messageVO.Adapt<GetAsRecipientVO>() with
            {
                Body = decryptedBody
            };

            return vo;
        }
    }
}
