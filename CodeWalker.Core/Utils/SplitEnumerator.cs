using System;
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
    public ref struct SpanSplitEnumerator
    {
        private ReadOnlySpan<char> _remaining;
        private ReadOnlySpan<char> _current;
        private bool _isEnumeratorActive;
        private char _splitBy;

        internal SpanSplitEnumerator(ReadOnlySpan<char> buffer, char splitBy)
        {
            _remaining = buffer;
            _current = default;
            _isEnumeratorActive = true;
            _splitBy = splitBy;
        }

        /// <summary>
        /// Gets the line at the current position of the enumerator.
        /// </summary>
        public ReadOnlySpan<char> Current => _current;

        /// <summary>
        /// Returns this instance as an enumerator.
        /// </summary>
        public SpanSplitEnumerator GetEnumerator() => this;

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

            ReadOnlySpan<char> remaining = _remaining;

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

    public static class EnumerateSplitExtensions
    {
        public static SpanSplitEnumerator EnumerateSplit(this ReadOnlySpan<char> span, char splitBy)
        {
            return new SpanSplitEnumerator(span, splitBy);
        }

        public static SpanSplitEnumerator EnumerateSplit(this Span<char> span, char splitBy)
        {
            return new SpanSplitEnumerator(span, splitBy);
        }

        public static SpanSplitEnumerator EnumerateSplit(this string str, char splitBy)
        {
            return new SpanSplitEnumerator(str.AsSpan(), splitBy);
        }
    }
}
