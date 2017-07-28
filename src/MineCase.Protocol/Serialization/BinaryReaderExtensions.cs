﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MineCase.Serialization
{
    internal static class BinaryReaderExtensions
    {
        public static bool ReadAsBoolean(this BinaryReader br) =>
            br.ReadBoolean();

        /// <see cref="http://wiki.vg/Protocol#VarInt_and_VarLong"/>
        public static uint ReadAsVarInt(this BinaryReader br, out int bytesRead)
        {
            int numRead = 0;
            uint result = 0;
            byte read;
            do
            {
                read = br.ReadByte();
                uint value = (uint)(read & 0b01111111);
                result |= (value << (7 * numRead));

                numRead++;
                if (numRead > 5)
                    throw new InvalidDataException("VarInt is too big");
            } while ((read & 0b10000000) != 0);

            bytesRead = numRead;
            return result;
        }

        public static string ReadAsString(this BinaryReader br)
        {
            var len = br.ReadAsVarInt(out _);
            var bytes = br.ReadBytes((int)len);
            return Encoding.UTF8.GetString(bytes);
        }

        public static ushort ReadAsUnsignedShort(this BinaryReader br)
        {
            var value = br.ReadUInt16();
            return value.ToBigEndian();
        }

        public static long ReadAsLong(this BinaryReader br)
        {
            var value = br.ReadUInt64();
            return (long)value.ToBigEndian();
        }
    }

    internal static class StreamExtensions
    {
        public static async Task ReadExactAsync(this Stream stream, byte[] buffer, int offset, int count)
        {
            while (count != 0)
            {
                var numRead = await stream.ReadAsync(buffer, offset, count);
                if (numRead == 0)
                    throw new EndOfStreamException();
                offset += numRead;
                count -= numRead;
            }
        }
    }
}
