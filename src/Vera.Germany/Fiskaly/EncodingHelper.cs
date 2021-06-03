using System.Text;

namespace Vera.Germany.Fiskaly
{
    public static class EncodingHelper
    {
        public static string Decode(byte[] bytes) => Encoding.UTF8.GetString(bytes);
        public static byte[] Encode(string data) => Encoding.UTF8.GetBytes(data);
    }
}
