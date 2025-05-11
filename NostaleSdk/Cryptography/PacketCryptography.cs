using NostaleSdk.Cryptography.Interfaces;

namespace NostaleSdk.Cryptography
{
    public class PacketCryptography : IPacketCryptography
    {
        private static readonly char[] Keys = { ' ', '-', '.', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'n' };

        private int _EncryptionKey { get; set; }

        public string DecryptLoginPacket(byte[] bytes, int size)
        {
            var output = "";
            for (var i = 0; i < size; i++)
            {
                output += Convert.ToChar(bytes[i] - 0xF);
            }
            return output;
        }

        public List<string> DecryptWorldPacket(byte[] bytes, int size)
        {
            var output = new List<string>();

            var currentPacket = "";
            var index = 0;

            while (index < size)
            {
                byte currentByte = bytes[index];
                index++;

                if (currentByte == 0xFF)
                {
                    output.Add(currentPacket);
                    currentPacket = "";
                    continue;
                }

                var length = (byte)(currentByte & 0x7F);

                if ((currentByte & 0x80) != 0)
                {
                    while (length != 0)
                    {
                        if (index <= size)
                        {
                            currentByte = bytes[index];
                            index++;

                            var firstIndex = (byte)(((currentByte & 0xF0u) >> 4) - 1);
                            var first = (byte)(firstIndex != 255 ? firstIndex != 14 ? Keys[firstIndex] : '\u0000' : '?');
                            if (first != 0x6E)
                                currentPacket += Convert.ToChar(first);

                            if (length <= 1)
                                break;

                            var secondIndex = (byte)((currentByte & 0xF) - 1);
                            var second = (byte)(secondIndex != 255 ? secondIndex != 14 ? Keys[secondIndex] : '\u0000' : '?');
                            if (second != 0x6E)
                                currentPacket += Convert.ToChar(second);

                            length -= 2;
                        }
                        else
                        {
                            length--;
                        }
                    }
                }
                else
                {
                    while (length != 0)
                    {
                        if (index < size)
                        {
                            currentPacket += Convert.ToChar(bytes[index] ^ 0xFF);
                            index++;
                        }
                        else if (index == size)
                        {
                            currentPacket += Convert.ToChar(0xFF);
                            index++;
                        }

                        length--;
                    }
                }
            }
            return output;
        }

        public byte[] EncryptWorldPacket(string value, bool isSessionPacket = false)
        {
            var output = new List<byte>();

            var mask = new string(value.Select(c =>
            {
                var b = (sbyte)c;
                if (c == '#' || c == '/' || c == '%')
                    return '0';
                if ((b -= 0x20) == 0 || (b += unchecked((sbyte)0xF1)) < 0 || (b -= 0xB) < 0 ||
                    b - unchecked((sbyte)0xC5) == 0)
                    return '1';
                return '0';
            }).ToArray());

            int packetLength = value.Length;

            var sequenceCounter = 0;
            var currentPosition = 0;

            while (currentPosition <= packetLength)
            {
                int lastPosition = currentPosition;
                while (currentPosition < packetLength && mask[currentPosition] == '0')
                    currentPosition++;

                int sequences;
                int length;

                if (currentPosition != 0)
                {
                    length = currentPosition - lastPosition;
                    sequences = length / 0x7E;
                    for (var i = 0; i < length; i++, lastPosition++)
                    {
                        if (i == sequenceCounter * 0x7E)
                        {
                            if (sequences == 0)
                            {
                                output.Add((byte)(length - i));
                            }
                            else
                            {
                                output.Add(0x7E);
                                sequences--;
                                sequenceCounter++;
                            }
                        }

                        output.Add((byte)((byte)value[lastPosition] ^ 0xFF));
                    }
                }

                if (currentPosition >= packetLength)
                    break;

                lastPosition = currentPosition;
                while (currentPosition < packetLength && mask[currentPosition] == '1')
                    currentPosition++;

                if (currentPosition == 0) continue;

                length = currentPosition - lastPosition;
                sequences = length / 0x7E;
                for (var i = 0; i < length; i++, lastPosition++)
                {
                    if (i == sequenceCounter * 0x7E)
                    {
                        if (sequences == 0)
                        {
                            output.Add((byte)((length - i) | 0x80));
                        }
                        else
                        {
                            output.Add(0x7E | 0x80);
                            sequences--;
                            sequenceCounter++;
                        }
                    }

                    var currentByte = (byte)value[lastPosition];
                    switch (currentByte)
                    {
                        case 0x20:
                            currentByte = 1;
                            break;

                        case 0x2D:
                            currentByte = 2;
                            break;

                        case 0xFF:
                            currentByte = 0xE;
                            break;

                        default:
                            currentByte -= 0x2C;
                            break;
                    }

                    if (currentByte == 0x00) continue;

                    if (i % 2 == 0)
                        output.Add((byte)(currentByte << 4));
                    else
                        output[output.Count - 1] = (byte)(output.Last() | currentByte);
                }
            }

            output.Add(0xFF);

            var sessionNumber = (sbyte)((_EncryptionKey >> 6) & 0xFF & 0x80000003);

            if (sessionNumber < 0)
                sessionNumber = (sbyte)(((sessionNumber - 1) | 0xFFFFFFFC) + 1);

            var sessionKey = (byte)(_EncryptionKey & 0xFF);

            if (isSessionPacket)
                sessionNumber = -1;

            switch (sessionNumber)
            {
                case 0:
                    for (var i = 0; i < output.Count; i++)
                        output[i] = (byte)(output[i] + sessionKey + 0x40);
                    break;

                case 1:
                    for (var i = 0; i < output.Count; i++)
                        output[i] = (byte)(output[i] - (sessionKey + 0x40));
                    break;

                case 2:
                    for (var i = 0; i < output.Count; i++)
                        output[i] = (byte)((output[i] ^ 0xC3) + sessionKey + 0x40);
                    break;

                case 3:
                    for (var i = 0; i < output.Count; i++)
                        output[i] = (byte)((output[i] ^ 0xC3) - (sessionKey + 0x40));
                    break;

                default:
                    for (var i = 0; i < output.Count; i++)
                        output[i] = (byte)(output[i] + 0x0F);
                    break;
            }

            return output.ToArray();
        }

        public byte[] EncryptLoginPacket(string value)
        {
            var output = new byte[value.Length + 1];
            for (var i = 0; i < value.Length; i++)
            {
                output[i] = (byte)((value[i] ^ 0xC3) + 0xF);
            }
            output[^1] = 0xD8;
            return output;
        }

        public void SetEncryptionKey(int key)
        {
            _EncryptionKey = key;
        }

        public int GetEncryptionKey()
        {
            return _EncryptionKey;
        }


    }

}
