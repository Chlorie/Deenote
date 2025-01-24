#nullable enable

using CommunityToolkit.HighPerformance;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Deenote.Library
{
    public static class IOUtils
    {
        public static void WriteArrayWithLengthPrefix(this BinaryWriter bw, byte[] bytes)
        {
            bw.Write(bytes.Length);
            bw.Write(bytes);
        }

        public static byte[] ReadArrayWithLengthPrefix(this BinaryReader br)
        {
            var len = br.ReadInt32();
            var result = new byte[len];
            br.Read(result);
            return result;
        }

        public static unsafe void Write<T>(this BinaryWriter bw, in T value) where T : unmanaged
        {
            ref byte reference = ref Unsafe.As<T, byte>(ref Unsafe.AsRef(in value));
            var len = sizeof(T);
            ReadOnlySpan<byte> buffer = MemoryMarshal.CreateReadOnlySpan(ref reference, len);
            bw.Write(buffer);
        }

        public static unsafe T Read<T>(this BinaryReader br) where T : unmanaged
        {
            Unsafe.SkipInit(out T value);
            Span<byte> buffer = MemoryMarshal.CreateSpan(ref Unsafe.As<T, byte>(ref value), sizeof(T));
            var read = br.Read(buffer);
            if (read != sizeof(T))
                throw new EndOfStreamException();
            return value;
        }
    }
}