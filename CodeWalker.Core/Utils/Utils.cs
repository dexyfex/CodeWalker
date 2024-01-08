using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CodeWalker.Core.Utils;
using SharpDX;
using Color = SharpDX.Color;
using Half = SharpDX.Half;

namespace CodeWalker
{


    public static class TextUtil
    {
        static string[] sizeSuffixes = ["B", "KB", "MB", "GB", "TB", "PB", "EB"];

        public static string GetBytesReadable(long size)
        {
            //shamelessly stolen from stackoverflow, and a bit mangled

            // Returns the human-readable file size for an arbitrary, 64-bit file size 
            // The default format is "0.### XB", e.g. "4.2 KB" or "1.434 GB"
            // Get absolute value
            Debug.Assert(sizeSuffixes.Length > 0);

            if (size == 0)
            {
                return "0 B";
            }

            var absSize = Math.Abs(size);
            var fpPower = Math.Log(absSize, 1024);
            var intPower = (int)fpPower;
            var normSize = absSize / Math.Pow(1024, intPower);

            return $"{normSize:G4} {sizeSuffixes[intPower]}";
        }

        public static string GetBytesReadableOld(long i)
        {
            //shamelessly stolen from stackoverflow, and a bit mangled

            // Returns the human-readable file size for an arbitrary, 64-bit file size 
            // The default format is "0.### XB", e.g. "4.2 KB" or "1.434 GB"
            // Get absolute value
            long absolute_i = Math.Abs(i);
            // Determine the suffix and readable value
            string suffix;
            double readable;
            if (absolute_i >= 0x1000000000000000) // Exabyte
            {
                suffix = "EB";
                readable = (i >> 50);
            }
            else if (absolute_i >= 0x4000000000000) // Petabyte
            {
                suffix = "PB";
                readable = (i >> 40);
            }
            else if (absolute_i >= 0x10000000000) // Terabyte
            {
                suffix = "TB";
                readable = (i >> 30);
            }
            else if (absolute_i >= 0x40000000) // Gigabyte
            {
                suffix = "GB";
                readable = (i >> 20);
            }
            else if (absolute_i >= 0x100000) // Megabyte
            {
                suffix = "MB";
                readable = (i >> 10);
            }
            else if (absolute_i >= 0x400) // Kilobyte
            {
                suffix = "KB";
                readable = i;
            }
            else
            {
                return i.ToString("0 bytes"); // Byte
            }
            // Divide by 1024 to get fractional value
            readable = (readable / 1024);

            string fmt = "0.### ";
            if (readable > 1000)
            {
                fmt = "0";
            }
            else if (readable > 100)
            {
                fmt = "0.#";
            }
            else if (readable > 10)
            {
                fmt = "0.##";
            }

            // Return formatted number with suffix
            return $"{readable.ToString(fmt)}{suffix}";
        }



        public static string GetUTF8Text(Span<byte> bytes)
        {
            if (bytes == null || bytes.Length == 0)
            {
                return string.Empty;
            } //file not found..
            if ((bytes.Length > 3) && (bytes[0] == 0xEF) && (bytes[1] == 0xBB) && (bytes[2] == 0xBF))
            {
                bytes = bytes.Slice(3);
            }

            if (bytes.Length == 0)
            {
                return string.Empty;
            }

            return Encoding.UTF8.GetString(bytes);
        }
        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            return source?.IndexOf(toCheck, comp) >= 0;
        }

        public static bool EndsWithAny(this string str, string searchString)
        {
            return str.EndsWith(searchString, StringComparison.OrdinalIgnoreCase);
        }

        public static bool EndsWithAny(this string str, string searchString, string searchString2)
        {
            return str.EndsWith(searchString, StringComparison.OrdinalIgnoreCase) || str.EndsWith(searchString2, StringComparison.OrdinalIgnoreCase);
        }

        public static bool EndsWithAny(this string str, params string[] strings)
        {
            foreach(var searchString in strings)
            {
                if (str.EndsWith(searchString, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

    }



    public static class FloatUtil
    {
        //public static bool TryParse(string? s, out float f)
        //{
        //    return float.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out f);
        //}

        public static bool TryParse(ReadOnlySpan<char> s, out float f)
        {
            return float.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out f);
        }

        public static float Parse(ReadOnlySpan<char> s)
        {
            TryParse(s, out float f);
            return f;
        }
        public static string ToString(float value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }


        public static string GetVector2String(Vector2 v, string d = ", ")
        {
            return $"{ToString(v.X)}{d}{ToString(v.Y)}";
        }
        public static string GetVector2XmlString(Vector2 v)
        {
            return $"x=\"{ToString(v.X)}\" y=\"{ToString(v.Y)}\"";
        }
        public static string GetVector3String(in Vector3 v, string d = ", ")
        {
            return $"{ToString(v.X)}{d}{ToString(v.Y)}{d}{ToString(v.Z)}";
        }
        public static string GetVector3StringFormat(in Vector3 v, string format)
        {
            var c = CultureInfo.InvariantCulture;
            return $"{v.X.ToString(format, c)}, {v.Y.ToString(format, c)}, {v.Z.ToString(format, c)}";
        }
        public static string GetVector3XmlString(in Vector3 v)
        {
            return $"x=\"{ToString(v.X)}\" y=\"{ToString(v.Y)}\" z=\"{ToString(v.Z)}\"";
        }

        public static string GetVector4String(in Vector4 v)
        {
            return GetVector4String(in v, ", ");
        }
        public static string GetVector4String(in Vector4 v, string d)
        {
            return $"{ToString(v.X)}{d}{ToString(v.Y)}{d}{ToString(v.Z)}{d}{ToString(v.W)}";
        }
        public static string GetVector4XmlString(in Vector4 v)
        {
            return $"x=\"{ToString(v.X)}\" y=\"{ToString(v.Y)}\" z=\"{ToString(v.Z)}\" w=\"{ToString(v.W)}\"";
        }
        public static string GetQuaternionXmlString(in Quaternion q)
        {
            return $"x=\"{ToString(q.X)}\" y=\"{ToString(q.Y)}\" z=\"{ToString(q.Z)}\" w=\"{ToString(q.W)}\"";
        }

        public static Span<float> ConvertToFloat(Span<Half> values)
        {
            Span<float> array = new float[values.Length];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = values[i];
            }

            return array;
        }

        public static string GetHalf2String(Half2 v, string d = ", ")
        {
            var f = ConvertToFloat([v.X, v.Y]);
            return $"{ToString(f[0])}{d}{ToString(f[1])}";
        }
        public static string GetHalf4String(Half4 v, string d = ", ")
        {
            var f = ConvertToFloat([v.X, v.Y, v.Z, v.W]);
            return $"{ToString(f[0])}{d}{ToString(f[1])}{d}{ToString(f[2])}{d}{ToString(f[3])}";
        }
        public static string GetColourString(Color v, string d = ", ")
        {
            var c = CultureInfo.InvariantCulture;
            return $"{v.R.ToString(c)}{d}{v.G.ToString(c)}{d}{v.B.ToString(c)}{d}{v.A.ToString(c)}";
        }


        public static Vector2 ParseVector2String(string s)
        {
            Vector2 p = new Vector2(0.0f);

            var enumerator = s.EnumerateSplit(',');
            if (enumerator.MoveNext())
            {
                _ = TryParse(enumerator.Current.Trim(), out p.X);
            }
            if (enumerator.MoveNext())
            {
                _ = TryParse(enumerator.Current.Trim(), out p.Y);
            }
            return p;
        }
        public static Vector3 ParseVector3String(string s)
        {
            Vector3 p = new Vector3(0.0f);

            var enumerator = s.EnumerateSplit(',');

            if (enumerator.MoveNext())
            {
                _ = TryParse(enumerator.Current.Trim(), out p.X);
            }
            if (enumerator.MoveNext())
            {
                _ = TryParse(enumerator.Current.Trim(), out p.Y);
            }
            if (enumerator.MoveNext())
            {
                _ = TryParse(enumerator.Current.Trim(), out p.Z);
            }
            return p;
        }
        public static Vector4 ParseVector4String(string s)
        {
            Vector4 p = new Vector4(0.0f);
            var enumerator = s.EnumerateSplit(',');
            if (enumerator.MoveNext())
            {
                _ = TryParse(enumerator.Current.Trim(), out p.X);
            }
            if (enumerator.MoveNext())
            {
                TryParse(enumerator.Current.Trim(), out p.Y);
            }
            if (enumerator.MoveNext())
            {
                _ = TryParse(enumerator.Current.Trim(), out p.Z);
            }
            if (enumerator.MoveNext())
            {
                _ = TryParse(enumerator.Current.Trim(), out p.W);
            }
            return p;
        }


    }





    public static class BitUtil
    {
        public static bool IsBitSet(uint value, int bit)
        {
            return (((value >> bit) & 1) > 0);
        }
        public static uint SetBit(uint value, int bit)
        {
            return (value | (1u << bit));
        }
        public static uint ClearBit(uint value, int bit)
        {
            return (value & (~(1u << bit)));
        }
        public static uint UpdateBit(uint value, int bit, bool flag)
        {
            if (flag) return SetBit(value, bit);






            else return ClearBit(value, bit);
        }
        public static uint RotateLeft(uint value, int count)
        {
            return (value << count) | (value >> (32 - count));
        }
        public static uint RotateRight(uint value, int count)
        {
            return (value >> count) | (value << (32 - count));
        }
    }

    public static class SemaphoreSlimExtension
    {
        public struct SemaphoreLock : IDisposable
        {
            private bool _isDisposed = false;
            private readonly SemaphoreSlim? _semaphore;
            private readonly string _callerName;
            private readonly string _callerFilePath;
            public readonly bool LockTaken => _semaphore is not null;

            public SemaphoreLock(SemaphoreSlim? semaphore, string callerName = "", string callerFilePath = "")
            {
                _callerFilePath = callerFilePath;
                _callerName = callerName;
                _semaphore = semaphore;

                Console.WriteLine($"Lock taken from {callerFilePath} -> {callerName}");
            }

            public void Dispose()
            {
                if (_isDisposed)
                    return;

                Console.WriteLine($"Lock for {_callerFilePath} -> {_callerName} released");
                _semaphore?.Release();
                _isDisposed = true;
            }
        }

        public static async ValueTask<SemaphoreLock> WaitAsyncDisposable(this SemaphoreSlim semaphore)
        {
            await semaphore.WaitAsync();
            return new SemaphoreLock(semaphore);
        }

        public static SemaphoreLock WaitDisposable(this SemaphoreSlim semaphore, [CallerMemberName] string callerName = "", [CallerFilePath] string callerFilePath = "")
        {
            semaphore.Wait();
            return new SemaphoreLock(semaphore, callerName, callerFilePath);
        }

        public static SemaphoreLock WaitDisposable(this SemaphoreSlim semaphore, TimeSpan timeout, CancellationToken cancellationToken, [CallerMemberName] string callerName = "", [CallerFilePath] string callerFilePath = "")
        {
            if (semaphore.Wait(timeout, cancellationToken))
            {
                return new SemaphoreLock(semaphore, callerName, callerFilePath);
            }

            return new SemaphoreLock(null);
        }

        public static SemaphoreLock WaitDisposable(this SemaphoreSlim semaphore, int timeout, [CallerMemberName] string callerName = "", [CallerFilePath] string callerFilePath = "")
        {
            if (semaphore.Wait(timeout))
            {
                return new SemaphoreLock(semaphore, callerName, callerFilePath);
            }

            return new SemaphoreLock(null);
        }
    }
}
