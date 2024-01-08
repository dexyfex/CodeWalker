using CodeWalker.GameFiles;
using Collections.Pooled;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.HighPerformance.Buffers;
using Microsoft.Extensions.ObjectPool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CodeWalker.Core.Utils
{
    public class SharedObjectPool<T> where T : class, new()
    {
        private static readonly ObjectPool<T> s_shared = ObjectPool.Create<T>();

        public static ObjectPool<T> Shared => s_shared;
    }

    public class PooledListObjectPolicy<T> : PooledObjectPolicy<PooledList<T>>
    {
        private readonly ClearMode clearMode;
        public PooledListObjectPolicy(ClearMode _clearMode = ClearMode.Auto)
        {
            clearMode = _clearMode;
        }
        public PooledList<T> Get()
        {
            return new PooledList<T>(clearMode);
        }

        public override PooledList<T> Create()
        {
            return new PooledList<T>(clearMode);
        }

        public override bool Return(PooledList<T> list)
        {
            list.Clear();
            return true;
        }
    }

    public static class PooledListPool<T>
    {
        private static readonly ObjectPool<PooledList<T>> s_shared = ObjectPool.Create(new PooledListObjectPolicy<T>(ClearMode.Never));
        public static ObjectPool<PooledList<T>> Shared => s_shared;
    }

    public static class PooledListExtensions
    {
        public static int EnsureCapacity<T>(this PooledList<T> list, int capacity)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(capacity, 0, nameof(capacity));

            if (list.Capacity < capacity)
            {
                list.Capacity = capacity;
            }

            return list.Capacity;
        }
    }

    public static class StringPoolExtension
    {
        [SkipLocalsInit]
        public static string GetStringPooled(this Encoding encoding, ReadOnlySpan<byte> bytes)
        {
            Span<char> buffer = stackalloc char[bytes.Length];

            var charsWritten = encoding.GetChars(bytes, buffer);

            return StringPool.Shared.GetOrAdd(buffer.Slice(0, charsWritten));
        }
    }
}
