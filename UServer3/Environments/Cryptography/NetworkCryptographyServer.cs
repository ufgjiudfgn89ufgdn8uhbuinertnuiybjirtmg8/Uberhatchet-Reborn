using System.IO;
using Network;
using UServer3.Rust.Network;

namespace UServer3.Environments.Cryptography
{
    public class NetworkCryptographyServer : NetworkCryptography
    {
        // Methods
        protected override void DecryptionHandler(Connection connection, MemoryStream src, int srcOffset, MemoryStream dst, int dstOffset)
        {
            if (connection.encryptionLevel <= 1)
            {
                Cryptography.XOR(VirtualServer.ConnectionInformation.ConnectionProtocol, src, srcOffset, dst, dstOffset);
            }
        }

        protected override void EncryptionHandler(Connection connection, MemoryStream src, int srcOffset, MemoryStream dst, int dstOffset)
        {
            if (connection.encryptionLevel <= 1)
            {
                Cryptography.XOR(VirtualServer.ConnectionInformation.ConnectionProtocol, src, srcOffset, dst, dstOffset);
            }
        }
    }
}