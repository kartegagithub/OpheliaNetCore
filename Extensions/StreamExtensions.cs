using System;
using System.IO;

namespace Ophelia
{
    public static class StreamExtensions
    {
        public static byte[] ReadFully(this Stream stream, long initialLength)
        {
            stream.Seek(0, System.IO.SeekOrigin.Begin);
            if (initialLength < 1)
                initialLength = 32768;

            byte[] buffer = new byte[initialLength];
            int read = 0;

            int chunk;
            while ((chunk = stream.Read(buffer, read, buffer.Length - read)) > 0)
            {
                read += chunk;

                if (read == buffer.Length)
                {
                    int nextByte = stream.ReadByte();

                    if (nextByte == -1) return buffer;

                    byte[] newBuffer = new byte[buffer.Length * 2];
                    Array.Copy(buffer, newBuffer, buffer.Length);
                    newBuffer[read] = (byte)nextByte;
                    buffer = newBuffer;
                    read++;
                }
            }

            byte[] ret = new byte[read];
            Array.Copy(buffer, ret, read);
            return ret;
        }
    }
}
