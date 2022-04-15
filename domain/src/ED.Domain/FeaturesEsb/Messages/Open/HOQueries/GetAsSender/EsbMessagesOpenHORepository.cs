using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Mapster;
using static ED.Domain.IEsbMessagesOpenHORepository;
using static ED.Domain.IProfilesService;

namespace ED.Domain
{
    partial class EsbMessagesOpenHORepository : IEsbMessagesOpenHORepository
    {
        public async Task<GetAsSenderVO> GetAsSenderAsync(
            int messageId,
            CancellationToken ct)
        {
            IEsbMessagesOpenQueryRepository.GetAsSenderVO? messageVO =
                await this.esbMessageOpenQueryRepository.GetAsSenderAsync(
                    messageId,
                    ct);

            ProfileKeyVO profileKey =
              await this.profilesService.GetProfileKeyAsync(
                  messageVO.ProfileKeyId,
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
                      EncryptedData = ByteString.CopyFrom(messageVO.EncryptedKey)
                  },
                  cancellationToken: ct);

            IEncryptor encryptor = this.encryptorFactory.CreateEncryptor(
                decryptedKeyResp.Plaintext.ToByteArray(),
                messageVO.IV);

            string decryptedBody =
                Encoding.UTF8.GetString(encryptor.Decrypt(messageVO.Body));

            GetAsSenderVO vo = messageVO.Adapt<GetAsSenderVO>() with
            {
                Body = decryptedBody
            };

            return vo;
        }
    }
}
