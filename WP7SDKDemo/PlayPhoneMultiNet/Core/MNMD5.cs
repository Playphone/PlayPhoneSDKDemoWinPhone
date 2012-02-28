//
//  MNMD5.cs
//  MultiNet client
//
//  Copyright 2012 PlayPhone. All rights reserved.
//

namespace PlayPhone.MultiNet.Core
 {
  public class MNMD5
   {
    public const int HashSizeInBytes = 16;

    public MNMD5 ()
     {
      InitState();
     }

    public void Update (byte[] data)
     {
      int dataSize = data.Length;

      for (int dataOffset = 0; dataOffset < dataSize; dataOffset++)
       {
        Buffer[BufferSize++] = data[dataOffset];

        if (BufferSize == BlockSizeInBytes)
         {
          ProcessBlock(Buffer);

          BufferSize = 0;
         }
       }

      Size += ((ulong)dataSize) * 8;
     }

    public void Finish (byte[] Result)
     {
      Buffer[BufferSize++] = 0x80;

      if (BufferSize == BlockSizeInBytes)
       {
        ProcessBlock(Buffer);

        BufferSize = 0;
       }

      if (BufferSize >= BlockSizeInBytes - MessageSizeBlockSizeInBytes)
       {
        while (BufferSize < BlockSizeInBytes)
         {
          Buffer[BufferSize++] = 0x00;
         }

        ProcessBlock(Buffer);

        BufferSize = 0;
       }

      while (BufferSize < BlockSizeInBytes - MessageSizeBlockSizeInBytes)
       {
        Buffer[BufferSize++] = 0x00;
       }

      for (int i = 0; i < MessageSizeBlockSizeInBytes; i++)
       {
        Buffer[BufferSize++] = (byte)(Size & 0xFF);

        Size >>= 8;
       }

      ProcessBlock(Buffer);

      PutUIntToByteBuffer(Result,0,A);
      PutUIntToByteBuffer(Result,4,B);
      PutUIntToByteBuffer(Result,8,C);
      PutUIntToByteBuffer(Result,12,D);

      InitState();
     }

    public static byte[] CalculateHash (byte[] data)
     {
      MNMD5  md5    = new MNMD5();
      byte[] result = new byte[HashSizeInBytes];

      md5.Update(data);
      md5.Finish(result);

      return result;
     }

    private void InitState ()
     {
      A = 0x67452301;
      B = 0xEFCDAB89;
      C = 0x98BADCFE;
      D = 0x10325476;

      Size = 0;

      if (X == null)
       {
        X = new uint[BlockSizeInWords];
       }

      if (Buffer == null)
       {
        Buffer = new byte[BlockSizeInBytes];
       }

      BufferSize = 0;
     }

    private void ProcessBlock (byte[] data)
     {
      uint offset = 0;

      for (uint i = 0; i < BlockSizeInWords; i++)
       {
        X[i] = data[offset] + (uint)(data[offset + 1] << 8) +
                (uint)(data[offset + 2] << 16) + (uint)(data[offset + 3] << 24);

        offset += 4;
       }

      uint AA = A;
      uint BB = B;
      uint CC = C;
      uint DD = D;

      uint F,G,Temp;

      for (uint i = 0; i < 16; i++)
       {
        F = (B & C) | (~B & D); G = i;
        Temp = D; D = C; C = B; B = B + RotateLeft(A + F + K[i] + X[G],R[i]); A = Temp;
       }

      for (uint i = 16; i < 32; i++)
       {
        F = (D & B) | (~D & C); G = (5 * i + 1) % 16;
        Temp = D; D = C; C = B; B = B + RotateLeft(A + F + K[i] + X[G],R[i]); A = Temp;
       }

      for (uint i = 32; i < 48; i++)
       {
        F = B ^ C ^ D; G = (3 * i + 5) % 16;
        Temp = D; D = C; C = B; B = B + RotateLeft(A + F + K[i] + X[G],R[i]); A = Temp;
       }

      for (uint i = 48; i < 64; i++)
       {
        F = C ^ (B | ~D); G = (7 * i) % 16;
        Temp = D; D = C; C = B; B = B + RotateLeft(A + F + K[i] + X[G],R[i]); A = Temp;
       }

      A = AA + A;
      B = BB + B;
      C = CC + C;
      D = DD + D;
     }

    private static uint RotateLeft (uint V, int N)
     {
      return (V << N) | (V >> (32 - N));
     }

    private static void PutUIntToByteBuffer (byte[] Buffer, int Offset, uint Value)
     {
      for (int i = 0; i < 4; i++)
       {
        Buffer[Offset++] = (byte)(Value & 0xFF);

        Value >>= 8;
       }
     }

    private uint   A,B,C,D;
    private byte[] Buffer;
    private uint   BufferSize;
    private ulong  Size;
    private uint[] X;

    private const int BlockSizeInWords            = 16;
    private const int BlockSizeInBytes            = BlockSizeInWords * 4;
    private const int MessageSizeBlockSizeInBytes = 8;
    private static uint[] K = new uint[] { 0xd76aa478, 0xe8c7b756, 0x242070db, 0xc1bdceee,
                               0xf57c0faf, 0x4787c62a, 0xa8304613, 0xfd469501,
                               0x698098d8, 0x8b44f7af, 0xffff5bb1, 0x895cd7be, 
                               0x6b901122, 0xfd987193, 0xa679438e, 0x49b40821,
                               0xf61e2562, 0xc040b340, 0x265e5a51, 0xe9b6c7aa,
                               0xd62f105d, 0x02441453, 0xd8a1e681, 0xe7d3fbc8,
                               0x21e1cde6, 0xc33707d6, 0xf4d50d87, 0x455a14ed,
                               0xa9e3e905, 0xfcefa3f8, 0x676f02d9, 0x8d2a4c8a,
                               0xfffa3942, 0x8771f681, 0x6d9d6122, 0xfde5380c,
                               0xa4beea44, 0x4bdecfa9, 0xf6bb4b60, 0xbebfbc70,
                               0x289b7ec6, 0xeaa127fa, 0xd4ef3085, 0x04881d05,
                               0xd9d4d039, 0xe6db99e5, 0x1fa27cf8, 0xc4ac5665,
                               0xf4292244, 0x432aff97, 0xab9423a7, 0xfc93a039,
                               0x655b59c3, 0x8f0ccc92, 0xffeff47d, 0x85845dd1,
                               0x6fa87e4f, 0xfe2ce6e0, 0xa3014314, 0x4e0811a1,
                               0xf7537e82, 0xbd3af235, 0x2ad7d2bb, 0xeb86d391 };
    private static int[] R = new int[] { 7, 12, 17, 22, 7, 12, 17, 22, 7, 12, 17, 22, 7, 12, 17, 22,
                              5,  9, 14, 20, 5,  9, 14, 20, 5,  9, 14, 20, 5,  9, 14, 20,
                              4, 11, 16, 23, 4, 11, 16, 23, 4, 11, 16, 23, 4, 11, 16, 23,
                              6, 10, 15, 21, 6, 10, 15, 21, 6, 10, 15, 21, 6, 10, 15, 21 };
   }
 }
