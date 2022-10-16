using System;
// ReSharper disable InconsistentNaming

namespace AsepriteRead
{
    public enum BitDepth
    {
        Indexed = 8,  /*bpp*/
        GrayScale = 16, /*bpp*/
        RGBA = 32, /*bpp*/
    }

    public class AseBufferRead
    {
        public static readonly byte BYTE = sizeof(byte);   //  8 位无符号整数值
        public static readonly byte WORD = sizeof(ushort); // 16 位无符号整数值
        public static readonly byte SHORT = WORD;           // 16 位有符号整数值
        public static readonly byte DWORD = sizeof(uint);   // 32 位无符号整数值
        public static readonly byte LONG = DWORD;          // 32 位有符号整数值
        public static readonly byte FIXED = DWORD;          // 32 位定点 （16.16） 值

        public byte[] AseBuffer { get; private set; }
        public int CurrentReadPos { get; set; }
        public BitDepth Depth { get; set; } = BitDepth.RGBA;

        public AseBufferRead(byte[] aseBuffer, int currentReadPos)
        {
            AseBuffer = aseBuffer;
            CurrentReadPos = currentReadPos;
        }

        public UInt32 ReadDword()
        {
            var dword = BitConverter.ToUInt32(AseBuffer, CurrentReadPos);
            CurrentReadPos += 4;
            return dword;
        }

        public UInt16 ReadWord()
        {
            var word = BitConverter.ToUInt16(AseBuffer, CurrentReadPos);
            CurrentReadPos += 2;
            return word;
        }
        public Int16 ReadShort()
        {
            var shortInt16 = BitConverter.ToInt16(AseBuffer, CurrentReadPos);
            CurrentReadPos += 2;
            return shortInt16;
        }

        public byte ReadByte()
        {
            var _byte = AseBuffer[CurrentReadPos];
            CurrentReadPos += 1;
            return _byte;
        }

        public byte[] ReadByteArray(int num)
        {
            var _byte = new byte[num];
            for (var i = 1; i <= num; i++)
            {
                _byte[i - 1] = AseBuffer[CurrentReadPos];
                CurrentReadPos += 1;
            }
            return _byte;
        }

        public void SkipData(int num = 1)
        {
            CurrentReadPos += num;
        }


        public void SkipData(byte basic, int num = 1)
        {
            CurrentReadPos += basic * num;
        }


    }
}