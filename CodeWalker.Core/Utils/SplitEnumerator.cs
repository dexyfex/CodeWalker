using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeWalker.Core.Utils
{
    /// <summary>
    /// Enumerates the lines of a <see cref="ReadOnlySpan{Char}"/>.
    /// </summary>
    /// <remarks>
    /// To get an instance of this type, use <see cref="MemoryExtensions.EnumerateLines(ReadOnlySpan{char})"/>.
    /// </remarks>
    public ref struct SpanSplitEnumerator<T> where T : IEquatable<T>?
    {
        private ReadOnlySpan<T> _remaining;
        private ReadOnlySpan<T> _current;
        private bool _isEnumeratorActive;
        private readonly T _splitBy;
        private readonly ReadOnlySpan<T> _splitBySpan;

        internal SpanSplitEnumerator(ReadOnlySpan<T> buffer, T splitBy)
        {
            _remaining = buffer;
            _current = default;
            _isEnumeratorActive = true;
            _splitBy = splitBy;
        }

        /// <summary>
        /// Gets the line at the current position of the enumerator.
        /// </summary>
        public ReadOnlySpan<T> Current => _current;

        /// <summary>
        /// Returns this instance as an enumerator.
        /// </summary>
        public readonly SpanSplitEnumerator<T> GetEnumerator() => this;

        /// <summary>
        /// Advances the enumerator to the next line of the span.
        /// </summary>
        /// <returns>
        /// True if the enumerator successfully advanced to the next line; false if
        /// the enumerator has advanced past the end of the span.
        /// </returns>
        public bool MoveNext()
        {
            if (!_isEnumeratorActive)
            {
                return false; // EOF previously reached or enumerator was never initialized
            }

            ReadOnlySpan<T> remaining = _remaining;

            int idx = remaining.IndexOf(_splitBy);

            if ((uint)idx < (uint)remaining.Length)
            {
                _current = remaining.Slice(0, idx);
                _remaining = remaining.Slice(idx + 1);
            }
            else
            {
                // We've reached EOF, but we still need to return 'true' for this final
                // iteration so that the caller can query the Current property once more.

                _current = remaining;
                _remaining = default;
                _isEnumeratorActive = false;
            }

            return true;
        }
    }

    public ref struct SpanSplitEnumeratorAny<T> where T : IEquatable<T>?
    {
        private ReadOnlySpan<T> _remaining;
        private ReadOnlySpan<T> _current;
        private bool _isEnumeratorActive;
        private readonly ReadOnlySpan<T> _splitBy;

        internal SpanSplitEnumeratorAny(ReadOnlySpan<T> buffer, ReadOnlySpan<T> splitBy)
        {
            _remaining = buffer;
            _current = default;
            _isEnumeratorActive = true;
            _splitBy = splitBy;
        }

        /// <summary>
        /// Gets the line at the current position of the enumerator.
        /// </summary>
        public readonly ReadOnlySpan<T> Current => _current;

        /// <summary>
        /// Returns this instance as an enumerator.
        /// </summary>
        public readonly SpanSplitEnumeratorAny<T> GetEnumerator() => this;

        /// <summary>
        /// Advances the enumerator to the next line of the span.
        /// </summary>
        /// <returns>
        /// True if the enumerator successfully advanced to the next line; false if
        /// the enumerator has advanced past the end of the span.
        /// </returns>
        public bool MoveNext()
        {
            if (!_isEnumeratorActive)
            {
                return false; // EOF previously reached or enumerator was never initialized
            }

            ReadOnlySpan<T> remaining = _remaining;

            int idx = remaining.IndexOfAny(_splitBy);

            if ((uint)idx < (uint)remaining.Length)
            {
                _current = remaining.Slice(0, idx);
                _remaining = remaining.Slice(idx + 1);
            }
            else
            {
                // We've reached EOF, but we still need to return 'true' for this final
                // iteration so that the caller can query the Current property once more.

                _current = remaining;
                _remaining = default;
                _isEnumeratorActive = false;
            }

            return true;
        }
    }

    public static class EnumerateSplitExtensions
    {
        public static SpanSplitEnumerator<T> EnumerateSplit<T>(this ReadOnlySpan<T> span, T splitBy) where T : IEquatable<T>
        {
            return new SpanSplitEnumerator<T>(span, splitBy);
        }

        public static SpanSplitEnumeratorAny<T> EnumerateSplitAny<T>(this ReadOnlySpan<T> span, ReadOnlySpan<T> splitBy) where T : IEquatable<T>
        {
            return new SpanSplitEnumeratorAny<T>(span, splitBy);
        }

        public static SpanSplitEnumerator<T> EnumerateSplit<T>(this Span<T> span, T splitBy) where T : IEquatable<T>
        {
            return new SpanSplitEnumerator<T>(span, splitBy);
        }

        public static SpanSplitEnumeratorAny<T> EnumerateSplitAny<T>(this Span<T> span, ReadOnlySpan<T> splitBy) where T : IEquatable<T>
        {
            return new SpanSplitEnumeratorAny<T>(span, splitBy);
        }

        public static SpanSplitEnumerator<char> EnumerateSplit(this string str, char splitBy)
        {
            return EnumerateSplit(str.AsSpan(), splitBy);
        }

        public static SpanSplitEnumeratorAny<char> EnumerateSplitAny(this string str, ReadOnlySpan<char> splitBy)
        {
            return EnumerateSplitAny(str.AsSpan(), splitBy);
        }

        public static ReverseSpanSplitEnumerator<T> ReverseEnumerateSplit<T>(this ReadOnlySpan<T> span, T splitBy) where T : IEquatable<T>
        {
            return new ReverseSpanSplitEnumerator<T>(span, splitBy);
        }

        public static ReverseSpanSplitEnumerator<T> ReverseEnumerateSplit<T>(this Span<T> span, T splitBy) where T : IEquatable<T>
        {
            return new ReverseSpanSplitEnumerator<T>(span, splitBy);
        }

        public static ReverseSpanSplitEnumerator<char> ReverseEnumerateSplit(this string str, char splitBy)
        {
            return ReverseEnumerateSplit(str.AsSpan(), splitBy);
        }
    }

    public ref struct ReverseSpanSplitEnumerator<T> where T : IEquatable<T>?
    {
        private ReadOnlySpan<T> _remaining;
        private ReadOnlySpan<T> _current;
        private bool _isEnumeratorActive;
        private T _splitBy;

        internal ReverseSpanSplitEnumerator(ReadOnlySpan<T> buffer, T splitBy)
        {
            _remaining = buffer;
            _current = default;
            _isEnumeratorActive = true;
            _splitBy = splitBy;
        }

        /// <summary>
        /// Gets the line at the current position of the enumerator.
        /// </summary>
        public readonly ReadOnlySpan<T> Current => _current;

        /// <summary>
        /// Returns this instance as an enumerator.
        /// </summary>
        public readonly ReverseSpanSplitEnumerator<T> GetEnumerator() => this;

        /// <summary>
        /// Advances the enumerator to the next line of the span.
        /// </summary>
        /// <returns>
        /// True if the enumerator successfully advanced to the next line; false if
        /// the enumerator has advanced past the end of the span.
        /// </returns>
        public bool MoveNext()
        {
            if (!_isEnumeratorActive)
            {
                return false; // EOF previously reached or enumerator was never initialized
            }

            ReadOnlySpan<T> remaining = _remaining;

            int idx = remaining.LastIndexOf(_splitBy);

            if ((uint)idx < (uint)remaining.Length)
            {
                _current = remaining.Slice(idx + 1);
                _remaining = remaining.Slice(0, idx);
            }
            else
            {
                // We've reached EOF, but we still need to return 'true' for this final
                // iteration so that the caller can query the Current property once more.

                _current = remaining;
                _remaining = default;
                _isEnumeratorActive = false;
            }

            return true;
        }
    }
}
