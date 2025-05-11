
namespace NostaleSdk.Cryptography.Interfaces
{
    public interface IPacketCryptography
    {
        byte[] EncryptLoginPacket(string value);

        byte[] EncryptWorldPacket(string value, bool isSessionPacket = false);

        string DecryptLoginPacket(byte[] bytes, int size);

        List<string> DecryptWorldPacket(byte[] bytes, int size);

        int GetEncryptionKey();

        void SetEncryptionKey(int key);
    }

}
