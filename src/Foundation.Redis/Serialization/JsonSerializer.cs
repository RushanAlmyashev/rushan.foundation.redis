using Foundation.Redis.Serialization.Settings;
using System.IO;
using System.IO.Compression;

namespace Foundation.Redis.Serialization
{
    public class JsonSerializer: ISerializer
    {
        private static readonly System.Text.Encoding encoding = System.Text.Encoding.UTF8;        

#if NET5_0_OR_GREATER

        private static readonly System.Text.Json.JsonSerializerOptions _textJsonSettings = JsonSerializerSettings.GetTextJsonSerializerSettings();

        public byte[] Serialize<T>(T payload)
        {
            var byteArray = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(payload, _textJsonSettings);

            byte[] compressedJsonBytes = Compress(byteArray);

            return compressedJsonBytes;
        }

        public T Deserialize<T>(byte[] serializedPayload)
        {
            var decompressedByteArray = Decompress(serializedPayload);

            var payload = System.Text.Json.JsonSerializer.Deserialize<T>(decompressedByteArray, _textJsonSettings);

            return payload;
        }

#else
        private static readonly Newtonsoft.Json.JsonSerializerSettings _newtonSettings = JsonSerializerSettings.GetJsonSerializerSettings();

        public byte[] Serialize<T>(T payload)
        {
            var jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(payload, _newtonSettings);

            var byteArray = encoding.GetBytes(jsonString);

            byte[] compressedJsonBytes = Compress(byteArray);

            return compressedJsonBytes;
        }

        public T Deserialize<T>(byte[] serializedPayload)
        {
            var decompressedByteArray = Decompress(serializedPayload);

            var jsonString = encoding.GetString(decompressedByteArray);

            var payload = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(jsonString, _newtonSettings);

            return payload;
        }
#endif
        private static byte[] Compress(byte[] data)
        {
            using (var compressedStream = new MemoryStream())
            using (var zipStream = new GZipStream(compressedStream, CompressionMode.Compress))
            {
                zipStream.Write(data, 0, data.Length);
                zipStream.Close();
                return compressedStream.ToArray();
            }
        }

        private static byte[] Decompress(byte[] data)
        {
            using (var compressedStream = new MemoryStream(data))
            using (var zipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
            using (var resultStream = new MemoryStream())
            {
                zipStream.CopyTo(resultStream);
                return resultStream.ToArray();
            }
        }

    }
}
