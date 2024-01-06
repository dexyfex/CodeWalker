using Collections.Pooled;
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
        public PooledList<T> Get()
        {
            return new PooledList<T>();
        }

        public override PooledList<T> Create()
        {
            return new PooledList<T>();
        }

        public override bool Return(PooledList<T> list)
        {
            foreach (var entry in list.Span)
            {
                if (entry is IDisposable disposable)
                    disposable.Dispose();
                if (entry is IResettable resettable)
                    resettable.TryReset();
            }

            list.Clear();
            return true;
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
