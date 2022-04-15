using Google.Protobuf;
using Grpc.Core;

namespace ED.Keystore
{
    public static class GrpcUtils
    {
        public static void AssertNotTypeDefault(string fieldName, int fieldValue)
        {
            if (fieldValue == 0)
            {
                throw new RpcException(
                    new Status(
                        StatusCode.FailedPrecondition,
                        $"'{fieldName}' should have a value other than its default."));
            }
        }

        public static void AssertNotTypeDefault(string fieldName, bool fieldValue)
        {
            if (fieldValue == false)
            {
                throw new RpcException(
                    new Status(
                        StatusCode.FailedPrecondition,
                        $"'{fieldName}' should have a value other than its default."));
            }
        }

        public static void AssertNotTypeDefault(string fieldName, string fieldValue)
        {
            if (fieldValue.Length == 0)
            {
                throw new RpcException(
                    new Status(
                        StatusCode.FailedPrecondition,
                        $"'{fieldName}' should have a value other than its default."));
            }
        }

        public static void AssertNotTypeDefault(string fieldName, ByteString fieldValue)
        {
            if (fieldValue.Length == 0)
            {
                throw new RpcException(
                    new Status(
                        StatusCode.FailedPrecondition,
                        $"'{fieldName}' should have a value other than its default."));
            }
        }
    }
}
