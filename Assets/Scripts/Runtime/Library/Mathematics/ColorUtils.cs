#nullable enable

using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Deenote.Library.Mathematics
{
    public static class ColorUtils
    {
        /// <param name="code">RGBA, hex, exclude # sign</param>
        /// <param name="color"></param>
        /// <returns></returns>
        public static bool TryParse(ReadOnlySpan<char> code, out Color color)
        {
            const int HalfByteBitCount = 4;

            if (code.IsEmpty)
                goto Failed;
            Span<byte> bytes = stackalloc byte[4];
            bytes[3] = byte.MaxValue;

            int bidx = 0;
            for (int i = 0; i < code.Length; i += 2) {
                var c = code[i];
                if (!TryParseByte(c, out var bl))
                    goto Failed;
                ref var b = ref bytes[bidx];
                b = bl;

                if (i + 1 >= code.Length)
                    break;
                c = code[i + 1];
                if (!TryParseByte(c, out var br))
                    goto Failed;
                b <<= HalfByteBitCount;
                b |= br;
                bidx++;
            }

            color = new Color32(bytes[0], bytes[1], bytes[2], bytes[3]);
            return true;

        Failed:
            color = default;
            return false;

            static bool TryParseByte(char c, out byte b)
            {
                if (c is >= '0' and <= '9') {
                    b = (byte)(c - '0');
                    return true;
                }
                if (c is >= 'a' and <= 'f') {
                    b = (byte)(c - ('a' - 10));
                    return true;
                }
                if (c is >= 'A' and <= 'F') {
                    b = (byte)(c - ('A' - 10));
                    return true;
                }

                b = 0;
                return false;
            }
        }

        public static string ToRGBAString(this Color color)
            => ToRGBAString((Color32)color);

        public static string ToRGBAString(this Color32 color)
        {
            const int HalfByteBitCount = 4;

            var chars = (stackalloc char[8]);

            ToStr(color.r, ref chars[0]);
            ToStr(color.g, ref chars[2]);
            ToStr(color.g, ref chars[4]);
            ToStr(color.a, ref chars[6]);
            return chars.ToString();

            void ToStr(byte b, ref char c)
            {
                var bl = b >> HalfByteBitCount;
                c = ToChar4Bits(bl);

                var br = b & 0xF;
                Unsafe.Add(ref c, 1) = ToChar4Bits(br);
            }

            char ToChar4Bits(int b)
            {
                Debug.Assert(b <= 0xF);
                if (b < 10)
                    return (char)(b + '0');
                else
                    return (char)(b - 10 + 'A');
            }
        }
    }
}