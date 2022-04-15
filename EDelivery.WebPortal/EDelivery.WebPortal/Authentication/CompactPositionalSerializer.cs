using System;
using System.Linq;
using System.Text;

namespace EDelivery.WebPortal
{
    public static class CompactPositionalSerializer
    {
        public static byte[] Serialize(params object[] values)
        {
            int size = GetByteCount(values);
            byte[] buffer = new byte[size];
            _ = Serialize(buffer, 0, values);
            return buffer;
        }

        public static object[] Deserialize(byte[] buffer, params Type[] types)
        {
            _ = Deserialize(buffer, 0, types, out object[] values);
            return values;
        }

        public static int GetByteCount(params object[] values)
        {
            int size = 0;
            foreach (object value in values)
            {
                size += GetByteCount(value);
            }
            return size;
        }

        public static int Serialize(byte[] buffer, int position, object[] values)
        {
            int totalBytesWritten = 0;
            foreach (object value in values)
            {
                int bytesWritten = WriteSerializedValue(buffer, position, value);

                position += bytesWritten;
                totalBytesWritten += bytesWritten;
            }
            return totalBytesWritten;
        }

        public static int Deserialize(byte[] buffer, int position, Type[] valueTypes, out object[] values)
        {
            int totalBytesRead = 0;
            values = new object[valueTypes.Length];
            for (int i = 0; i < valueTypes.Length; i++)
            {
                Type valueType = valueTypes[i];

                int bytesRead = ReadSerializedValue(buffer, position, valueType, out object value);

                position += bytesRead;
                totalBytesRead += bytesRead;
                values[i] = value;
            }
            return totalBytesRead;
        }

        private static readonly Encoding USAsciiStrict = Encoding.GetEncoding(
            "us-ascii",
            new EncoderExceptionFallback(),
            new DecoderExceptionFallback());

        private static int WriteSerializedValue(byte[] buffer, int position, object value)
        {
            if (value is int intValue)
            {
                Array.Copy(BitConverter.GetBytes(intValue), 0, buffer, position, sizeof(int));
                if (!BitConverter.IsLittleEndian)
                {
                    // int bytes are stored as little endian
                    Array.Reverse(buffer, position, sizeof(int));
                }

                return sizeof(int);
            }
            if (value is long longValue)
            {
                Array.Copy(BitConverter.GetBytes(longValue), 0, buffer, position, sizeof(long));
                if (!BitConverter.IsLittleEndian)
                {
                    // long bytes are stored as little endian
                    Array.Reverse(buffer, position, sizeof(int));
                }

                return sizeof(long);
            }
            if (value is Guid guidValue)
            {
                Array.Copy(guidValue.ToByteArray(), 0, buffer, position, SizeOfGuid);

                return SizeOfGuid;
            }
            else if (value is string stringValue)
            {
                if (stringValue.Length > byte.MaxValue)
                {
                    throw new Exception(string.Format("Strings of length more than {0} are not supported.", byte.MaxValue));
                }

                byte[] strBytes = USAsciiStrict.GetBytes(stringValue);

                // write the length as a single byte preceding the string bytes
                buffer[position++] = Convert.ToByte(strBytes.Length);
                Array.Copy(strBytes, 0, buffer, position, strBytes.Length);

                return sizeof(byte) + strBytes.Length;
            }
            else
            {
                throw new Exception("Unsupported value type.");
            }
        }

        private static int ReadSerializedValue(byte[] buffer, int position, Type valueType, out object value)
        {
            if (valueType == typeof(int))
            {
                if (buffer.Length < position + sizeof(int))
                {
                    throw new Exception("Insufficient buffer length");
                }

                if (!BitConverter.IsLittleEndian)
                {
                    // int bytes are stored as little endian
                    Array.Reverse(buffer, position, sizeof(int));
                }

                value = BitConverter.ToInt32(buffer, position);
                return sizeof(int);
            }
            else if (valueType == typeof(long))
            {
                if (buffer.Length < position + sizeof(long))
                {
                    throw new Exception("Insufficient buffer length");
                }

                if (!BitConverter.IsLittleEndian)
                {
                    // long bytes are stored as little endian
                    Array.Reverse(buffer, position, sizeof(long));
                }

                value = BitConverter.ToInt64(buffer, position);
                return sizeof(long);
            }
            else if (valueType == typeof(Guid))
            {
                if (buffer.Length < position + SizeOfGuid)
                {
                    throw new Exception("Insufficient buffer length");
                }

                value = new Guid(buffer.Skip(position).Take(SizeOfGuid).ToArray());
                return SizeOfGuid;
            }
            else if (valueType == typeof(string))
            {
                if (buffer.Length < position + 1 ||
                    buffer.Length < position + buffer[position])
                {
                    throw new Exception("Insufficient buffer length");
                }

                byte length = buffer[position++];

                value = USAsciiStrict.GetString(buffer, position, length);
                return sizeof(byte) + length;
            }
            else
            {
                throw new Exception("Unsupported value type.");
            }
        }

        private const int SizeOfGuid = 16;
        private static int GetByteCount(object value)
        {
            if (value is int)
            {
                return sizeof(int);
            }
            else if (value is long)
            {
                return sizeof(long);
            }
            else if (value is Guid)
            {
                return SizeOfGuid;
            }
            else if (value is string stringValue)
            {
                if (stringValue.Length > byte.MaxValue)
                {
                    throw new Exception(string.Format("Strings of length more than {0} are not supported.", byte.MaxValue));
                }

                return sizeof(byte) + USAsciiStrict.GetByteCount(stringValue);
            }
            else
            {
                throw new Exception("Unsupported value type.");
            }
        }
    }
}
