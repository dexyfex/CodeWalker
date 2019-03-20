using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

/*
 Shamelessly stolen and mangled from:
 https://github.com/hamish-milne/FbxWriter
 Under GPL license, for full terms see the above link.

 Copyright (c) 2015 Hamish Milne

 "An FBX library for .NET"
*/


namespace CodeWalker
{

    /// <summary>
    /// Static read and write methods
    /// </summary>
    public static class FbxIO
    {

        /// <summary>
        /// Read binary or ASCII FBX from memory. Decides which based on the header.
        /// (This method added by dexyfex)
        /// </summary>
        /// <param name="data">FBX byte array.</param>
        /// <returns></returns>
        public static FbxDocument Read(byte[] data)
        {
            using (var stream = new MemoryStream(data))
            {
                var isbinary = FbxBinary.IsBinary(stream);
                if (isbinary)
                {
                    var reader = new FbxBinaryReader(stream);
                    return reader.Read();
                }
                else //try ASCII
                {
                    var reader = new FbxAsciiReader(stream);
                    return reader.Read();
                }
            }
        }


        /// <summary>
        /// Reads a binary FBX file
        /// </summary>
        /// <param name="path"></param>
        /// <returns>The top level document node</returns>
        public static FbxDocument ReadBinary(string path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));
            using (var stream = new FileStream(path, FileMode.Open))
            {
                var reader = new FbxBinaryReader(stream);
                return reader.Read();
            }
        }

        /// <summary>
        /// Reads an ASCII FBX file
        /// </summary>
        /// <param name="path"></param>
        /// <returns>The top level document node</returns>
        public static FbxDocument ReadAscii(string path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));
            using (var stream = new FileStream(path, FileMode.Open))
            {
                var reader = new FbxAsciiReader(stream);
                return reader.Read();
            }
        }

        /// <summary>
        /// Writes an FBX document
        /// </summary>
        /// <param name="document">The top level document node</param>
        /// <param name="path"></param>
        public static void WriteBinary(FbxDocument document, string path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));
            using (var stream = new FileStream(path, FileMode.Create))
            {
                var writer = new FbxBinaryWriter(stream);
                writer.Write(document);
            }
        }

        /// <summary>
        /// Writes an FBX document
        /// </summary>
        /// <param name="document">The top level document node</param>
        /// <param name="path"></param>
        public static void WriteAscii(FbxDocument document, string path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));
            using (var stream = new FileStream(path, FileMode.Create))
            {
                var writer = new FbxAsciiWriter(stream);
                writer.Write(document);
            }
        }
    }




    /// <summary>
    /// Reads FBX nodes from a text stream
    /// </summary>
    public class FbxAsciiReader
    {
        private readonly Stream stream;
        private readonly FbxErrorLevel errorLevel;

        private int line = 1;
        private int column = 1;

        /// <summary>
        /// Creates a new reader
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="errorLevel"></param>
        public FbxAsciiReader(Stream stream, FbxErrorLevel errorLevel = FbxErrorLevel.Checked)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));
            this.stream = stream;
            this.errorLevel = errorLevel;
        }

        /// <summary>
        /// The maximum array size that will be allocated
        /// </summary>
        /// <remarks>
        /// If you trust the source, you can expand this value as necessary.
        /// Malformed files could cause large amounts of memory to be allocated
        /// and slow or crash the system as a result.
        /// </remarks>
        public int MaxArrayLength { get; set; } = (1 << 24);

        // We read bytes a lot, so we should make a more efficient method here
        // (The normal one makes a new byte array each time)

        readonly byte[] singleChar = new byte[1];
        private char? prevChar;
        private bool endStream;
        private bool wasCr;

        // Reads a char, allows peeking and checks for end of stream
        char ReadChar()
        {
            if (prevChar != null)
            {
                var c = prevChar.Value;
                prevChar = null;
                return c;
            }
            if (stream.Read(singleChar, 0, 1) < 1)
            {
                endStream = true;
                return '\0';
            }
            var ch = (char)singleChar[0];
            // Handle line and column numbers here;
            // This isn't terribly accurate, but good enough for diagnostics
            if (ch == '\r')
            {
                wasCr = true;
                line++;
                column = 0;
            }
            else
            {
                if (ch == '\n' && !wasCr)
                {
                    line++;
                    column = 0;
                }
                wasCr = false;
            }
            column++;
            return ch;
        }

        // Checks if a character is valid in a real number
        static bool IsDigit(char c, bool first)
        {
            if (char.IsDigit(c))
                return true;
            switch (c)
            {
                case '-':
                case '+':
                    return true;
                case '.':
                case 'e':
                case 'E':
                case 'X':
                case 'x':
                    return !first;
            }
            return false;
        }

        static bool IsLineEnd(char c)
        {
            return c == '\r' || c == '\n';
        }

        // Token to mark the end of the stream
        class EndOfStream
        {
            public override string ToString()
            {
                return "end of stream";
            }
        }

        // Wrapper around a string to mark it as an identifier
        // (as opposed to a string literal)
        class Identifier
        {
            public readonly string String;

            public override bool Equals(object obj)
            {
                var id = obj as Identifier;
                if (id != null)
                    return String == id.String;
                return false;
            }

            public override int GetHashCode()
            {
                return String?.GetHashCode() ?? 0;
            }

            public Identifier(string str)
            {
                String = str;
            }

            public override string ToString()
            {
                return String + ":";
            }
        }

        private object prevTokenSingle;

        // Reads a single token, allows peeking
        // Can return 'null' for a comment or whitespace
        object ReadTokenSingle()
        {
            if (prevTokenSingle != null)
            {
                var ret = prevTokenSingle;
                prevTokenSingle = null;
                return ret;
            }
            var c = ReadChar();
            if (endStream)
                return new EndOfStream();
            switch (c)
            {
                case ';': // Comments
                    while (!IsLineEnd(ReadChar()) && !endStream) { } // Skip a line
                    return null;
                case '{': // Operators
                case '}':
                case '*':
                case ':':
                case ',':
                    return c;
                case '"': // String literal
                    var sb1 = new StringBuilder();
                    while ((c = ReadChar()) != '"')
                    {
                        if (endStream)
                            throw new FbxException(line, column,
                                "Unexpected end of stream; expecting end quote");
                        sb1.Append(c);
                    }
                    return sb1.ToString();
                default:
                    if (char.IsWhiteSpace(c))
                    {
                        // Merge whitespace
                        while (char.IsWhiteSpace(c = ReadChar()) && !endStream) { }
                        if (!endStream)
                            prevChar = c;
                        return null;
                    }
                    if (IsDigit(c, true)) // Number
                    {
                        var sb2 = new StringBuilder();
                        do
                        {
                            sb2.Append(c);
                            c = ReadChar();
                        } while (IsDigit(c, false) && !endStream);
                        if (!endStream)
                            prevChar = c;
                        var str = sb2.ToString();
                        if (str.Contains("."))
                        {
                            if (str.Split('.', 'e', 'E')[1].Length > 6)
                            {
                                double d;
                                if (!double.TryParse(str, out d))
                                    throw new FbxException(line, column,
                                        "Invalid number");
                                return d;
                            }
                            else
                            {
                                float f;
                                if (!float.TryParse(str, out f))
                                    throw new FbxException(line, column,
                                        "Invalid number");
                                return f;
                            }
                        }
                        long l;
                        if (!long.TryParse(str, out l))
                            throw new FbxException(line, column,
                                "Invalid integer");
                        // Check size and return the smallest possible
                        if (l >= byte.MinValue && l <= byte.MaxValue)
                            return (byte)l;
                        if (l >= int.MinValue && l <= int.MaxValue)
                            return (int)l;
                        return l;
                    }
                    if (char.IsLetter(c) || c == '_') // Identifier
                    {
                        var sb3 = new StringBuilder();
                        do
                        {
                            sb3.Append(c);
                            c = ReadChar();
                        } while ((char.IsLetterOrDigit(c) || c == '_') && !endStream);
                        if (!endStream)
                            prevChar = c;
                        return new Identifier(sb3.ToString());
                    }
                    break;
            }
            throw new FbxException(line, column,
                "Unknown character " + c);
        }

        private object prevToken;

        // Use a loop rather than recursion to prevent stack overflow
        // Here we can also merge string+colon into an identifier,
        // returning single-character bare strings (for C-type properties)
        object ReadToken()
        {
            object ret;
            if (prevToken != null)
            {
                ret = prevToken;
                prevToken = null;
                return ret;
            }
            do
            {
                ret = ReadTokenSingle();
            } while (ret == null);
            var id = ret as Identifier;
            if (id != null)
            {
                object colon;
                do
                {
                    colon = ReadTokenSingle();
                } while (colon == null);
                if (!':'.Equals(colon))
                {
                    if (id.String.Length > 1)
                        throw new FbxException(line, column,
                            "Unexpected '" + colon + "', expected ':' or a single-char literal");
                    ret = id.String[0];
                    prevTokenSingle = colon;
                }
            }
            return ret;
        }

        void ExpectToken(object token)
        {
            var t = ReadToken();
            if (!token.Equals(t))
                throw new FbxException(line, column,
                    "Unexpected '" + t + "', expected " + token);
        }

        private enum ArrayType
        {
            Byte = 0,
            Int = 1,
            Long = 2,
            Float = 3,
            Double = 4,
        };

        Array ReadArray()
        {
            // Read array length and header
            var len = ReadToken();
            long l;
            if (len is long)
                l = (long)len;
            else if (len is int)
                l = (int)len;
            else if (len is byte)
                l = (byte)len;
            else
                throw new FbxException(line, column,
                    "Unexpected '" + len + "', expected an integer");
            if (l < 0)
                throw new FbxException(line, column,
                    "Invalid array length " + l);
            if (l > MaxArrayLength)
                throw new FbxException(line, column,
                    "Array length " + l + " higher than permitted maximum " + MaxArrayLength);
            ExpectToken('{');
            ExpectToken(new Identifier("a"));
            var array = new double[l];

            // Read array elements
            bool expectComma = false;
            object token;
            var arrayType = ArrayType.Byte;
            long pos = 0;
            while (!'}'.Equals(token = ReadToken()))
            {
                if (expectComma)
                {
                    if (!','.Equals(token))
                        throw new FbxException(line, column,
                            "Unexpected '" + token + "', expected ','");
                    expectComma = false;
                    continue;
                }
                if (pos >= array.Length)
                {
                    if (errorLevel >= FbxErrorLevel.Checked)
                        throw new FbxException(line, column,
                            "Too many elements in array");
                    continue;
                }

                // Add element to the array, checking for the maximum
                // size of any one element.
                // (I'm not sure if this is the 'correct' way to do it, but it's the only
                // logical one given the nature of the ASCII format)
                double d;
                if (token is byte)
                {
                    d = (byte)token;
                }
                else if (token is int)
                {
                    d = (int)token;
                    if (arrayType < ArrayType.Int)
                        arrayType = ArrayType.Int;
                }
                else if (token is long)
                {
                    d = (long)token;
                    if (arrayType < ArrayType.Long)
                        arrayType = ArrayType.Long;
                }
                else if (token is float)
                {
                    d = (float)token;
                    // A long can't be accurately represented by a float
                    arrayType = arrayType < ArrayType.Long
                        ? ArrayType.Float : ArrayType.Double;
                }
                else if (token is double)
                {
                    d = (double)token;
                    if (arrayType < ArrayType.Double)
                        arrayType = ArrayType.Double;
                }
                else
                    throw new FbxException(line, column,
                            "Unexpected '" + token + "', expected a number");
                array[pos++] = d;
                expectComma = true;
            }
            if (pos < array.Length && errorLevel >= FbxErrorLevel.Checked)
                throw new FbxException(line, column,
                    "Too few elements in array - expected " + (array.Length - pos) + " more");

            // Convert the array to the smallest type we can see
            Array ret;
            switch (arrayType)
            {
                case ArrayType.Byte:
                    var bArray = new byte[array.Length];
                    for (int i = 0; i < bArray.Length; i++)
                        bArray[i] = (byte)array[i];
                    ret = bArray;
                    break;
                case ArrayType.Int:
                    var iArray = new int[array.Length];
                    for (int i = 0; i < iArray.Length; i++)
                        iArray[i] = (int)array[i];
                    ret = iArray;
                    break;
                case ArrayType.Long:
                    var lArray = new long[array.Length];
                    for (int i = 0; i < lArray.Length; i++)
                        lArray[i] = (long)array[i];
                    ret = lArray;
                    break;
                case ArrayType.Float:
                    var fArray = new float[array.Length];
                    for (int i = 0; i < fArray.Length; i++)
                        fArray[i] = (long)array[i];
                    ret = fArray;
                    break;
                default:
                    ret = array;
                    break;
            }
            return ret;
        }

        /// <summary>
        /// Reads the next node from the stream
        /// </summary>
        /// <returns>The read node, or <c>null</c></returns>
        public FbxNode ReadNode()
        {
            var first = ReadToken();
            var id = first as Identifier;
            if (id == null)
            {
                if (first is EndOfStream)
                    return null;
                throw new FbxException(line, column,
                    "Unexpected '" + first + "', expected an identifier");
            }
            var node = new FbxNode { Name = id.String };

            // Read properties
            object token;
            bool expectComma = false;
            while (!'{'.Equals(token = ReadToken()) && !(token is Identifier) && !'}'.Equals(token))
            {
                if (expectComma)
                {
                    if (!','.Equals(token))
                        throw new FbxException(line, column,
                            "Unexpected '" + token + "', expected a ','");
                    expectComma = false;
                    continue;
                }
                if (token is char)
                {
                    var c = (char)token;
                    switch (c)
                    {
                        case '*':
                            token = ReadArray();
                            break;
                        case '}':
                        case ':':
                        case ',':
                            throw new FbxException(line, column,
                                "Unexpected '" + c + "' in property list");
                    }
                }
                node.Properties.Add(token);
                expectComma = true; // The final comma before the open brace isn't required
            }
            // TODO: Merge property list into an array as necessary
            // Now we're either at an open brace, close brace or a new node
            if (token is Identifier || '}'.Equals(token))
            {
                prevToken = token;
                return node;
            }
            // The while loop can't end unless we're at an open brace, so we can continue right on
            object endBrace;
            while (!'}'.Equals(endBrace = ReadToken()))
            {
                prevToken = endBrace; // If it's not an end brace, the next node will need it
                node.Nodes.Add(ReadNode());
            }
            if (node.Nodes.Count < 1) // If there's an open brace, we want that to be preserved
                node.Nodes.Add(null);
            return node;
        }

        /// <summary>
        /// Reads a full document from the stream
        /// </summary>
        /// <returns>The complete document object</returns>
        public FbxDocument Read()
        {
            var ret = new FbxDocument();

            // Read version string
            const string versionString = @"; FBX (\d)\.(\d)\.(\d) project file";
            char c;
            while (char.IsWhiteSpace(c = ReadChar()) && !endStream) { } // Skip whitespace
            bool hasVersionString = false;
            if (c == ';')
            {
                var sb = new StringBuilder();
                do
                {
                    sb.Append(c);
                } while (!IsLineEnd(c = ReadChar()) && !endStream);
                var match = Regex.Match(sb.ToString(), versionString);
                hasVersionString = match.Success;
                if (hasVersionString)
                    ret.Version = (FbxVersion)(
                        int.Parse(match.Groups[1].Value) * 1000 +
                        int.Parse(match.Groups[2].Value) * 100 +
                        int.Parse(match.Groups[3].Value) * 10
                    );
            }
            if (!hasVersionString && errorLevel >= FbxErrorLevel.Strict)
                throw new FbxException(line, column,
                    "Invalid version string; first line must match \"" + versionString + "\"");
            FbxNode node;
            while ((node = ReadNode()) != null)
                ret.Nodes.Add(node);
            return ret;
        }
    }

    /// <summary>
    /// Writes an FBX document in a text format
    /// </summary>
    public class FbxAsciiWriter
    {
        private readonly Stream stream;

        /// <summary>
        /// Creates a new reader
        /// </summary>
        /// <param name="stream"></param>
        public FbxAsciiWriter(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));
            this.stream = stream;
        }

        /// <summary>
        /// The maximum line length in characters when outputting arrays
        /// </summary>
        /// <remarks>
        /// Lines might end up being a few characters longer than this, visibly and otherwise,
        /// so don't rely on it as a hard limit in code!
        /// </remarks>
        public int MaxLineLength { get; set; } = 260;

        readonly Stack<string> nodePath = new Stack<string>();

        // Adds the given node text to the string
        void BuildString(FbxNode node, StringBuilder sb, bool writeArrayLength, int indentLevel = 0)
        {
            nodePath.Push(node.Name ?? "");
            int lineStart = sb.Length;
            // Write identifier
            for (int i = 0; i < indentLevel; i++)
                sb.Append('\t');
            sb.Append(node.Name).Append(':');

            // Write properties
            var first = true;
            for (int j = 0; j < node.Properties.Count; j++)
            {
                var p = node.Properties[j];
                if (p == null)
                    continue;
                if (!first)
                    sb.Append(',');
                sb.Append(' ');
                if (p is string)
                {
                    sb.Append('"').Append(p).Append('"');
                }
                else if (p is Array)
                {
                    var array = (Array)p;
                    var elementType = p.GetType().GetElementType();
                    // ReSharper disable once PossibleNullReferenceException
                    // We know it's an array, so we don't need to check for null
                    if (array.Rank != 1 || !elementType.IsPrimitive)
                        throw new FbxException(nodePath, j,
                            "Invalid array type " + p.GetType());
                    if (writeArrayLength)
                    {
                        sb.Append('*').Append(array.Length).Append(" {\n");
                        lineStart = sb.Length;
                        for (int i = -1; i < indentLevel; i++)
                            sb.Append('\t');
                        sb.Append("a: ");
                    }
                    bool pFirst = true;
                    foreach (var v in (Array)p)
                    {
                        if (!pFirst)
                            sb.Append(',');
                        var vstr = v.ToString();
                        if ((sb.Length - lineStart) + vstr.Length >= MaxLineLength)
                        {
                            sb.Append('\n');
                            lineStart = sb.Length;
                        }
                        sb.Append(vstr);
                        pFirst = false;
                    }
                    if (writeArrayLength)
                    {
                        sb.Append('\n');
                        for (int i = 0; i < indentLevel; i++)
                            sb.Append('\t');
                        sb.Append('}');
                    }
                }
                else if (p is char)
                    sb.Append((char)p);
                else if (p.GetType().IsPrimitive && p is IFormattable)
                    sb.Append(p);
                else
                    throw new FbxException(nodePath, j,
                        "Invalid property type " + p.GetType());
                first = false;
            }

            // Write child nodes
            if (node.Nodes.Count > 0)
            {
                sb.Append(" {\n");
                foreach (var n in node.Nodes)
                {
                    if (n == null)
                        continue;
                    BuildString(n, sb, writeArrayLength, indentLevel + 1);
                }
                for (int i = 0; i < indentLevel; i++)
                    sb.Append('\t');
                sb.Append('}');
            }
            sb.Append('\n');

            nodePath.Pop();
        }

        /// <summary>
        /// Writes an FBX document to the stream
        /// </summary>
        /// <param name="document"></param>
        /// <remarks>
        /// ASCII FBX files have no header or footer, so you can call this multiple times
        /// </remarks>
        public void Write(FbxDocument document)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));
            var sb = new StringBuilder();

            // Write version header (a comment, but required for many importers)
            var vMajor = (int)document.Version / 1000;
            var vMinor = ((int)document.Version % 1000) / 100;
            var vRev = ((int)document.Version % 100) / 10;
            sb.Append($"; FBX {vMajor}.{vMinor}.{vRev} project file\n\n");

            nodePath.Clear();
            foreach (var n in document.Nodes)
            {
                if (n == null)
                    continue;
                BuildString(n, sb, document.Version >= FbxVersion.v7_1);
                sb.Append('\n');
            }
            var b = Encoding.ASCII.GetBytes(sb.ToString());
            stream.Write(b, 0, b.Length);
        }
    }


    /// <summary>
    /// Base class for binary stream wrappers
    /// </summary>
    public abstract class FbxBinary
    {
        // Header string, found at the top of all compliant files
        private static readonly byte[] headerString
            = Encoding.ASCII.GetBytes("Kaydara FBX Binary  \0\x1a\0");

        // This data was entirely calculated by me, honest. Turns out it works, fancy that!
        private static readonly byte[] sourceId =
            { 0x58, 0xAB, 0xA9, 0xF0, 0x6C, 0xA2, 0xD8, 0x3F, 0x4D, 0x47, 0x49, 0xA3, 0xB4, 0xB2, 0xE7, 0x3D };
        private static readonly byte[] key =
            { 0xE2, 0x4F, 0x7B, 0x5F, 0xCD, 0xE4, 0xC8, 0x6D, 0xDB, 0xD8, 0xFB, 0xD7, 0x40, 0x58, 0xC6, 0x78 };
        // This wasn't - it just appears at the end of every compliant file
        private static readonly byte[] extension =
            { 0xF8, 0x5A, 0x8C, 0x6A, 0xDE, 0xF5, 0xD9, 0x7E, 0xEC, 0xE9, 0x0C, 0xE3, 0x75, 0x8F, 0x29, 0x0B };

        // Number of null bytes between the footer code and the version
        private const int footerZeroes1 = 20;
        // Number of null bytes between the footer version and extension code
        private const int footerZeroes2 = 120;

        /// <summary>
        /// The size of the footer code
        /// </summary>
        protected const int footerCodeSize = 16;

        /// <summary>
        /// The namespace separator in the binary format (remember to reverse the identifiers)
        /// </summary>
        protected const string binarySeparator = "\0\x1";

        /// <summary>
        /// The namespace separator in the ASCII format and in object data
        /// </summary>
        protected const string asciiSeparator = "::";

        /// <summary>
        /// Checks if the first part of 'data' matches 'original'
        /// </summary>
        /// <param name="data"></param>
        /// <param name="original"></param>
        /// <returns><c>true</c> if it does, otherwise <c>false</c></returns>
        protected static bool CheckEqual(byte[] data, byte[] original)
        {
            for (int i = 0; i < original.Length; i++)
                if (data[i] != original[i])
                    return false;
            return true;
        }


        public static bool IsBinary(Stream stream)
        {
            var isb = ReadHeader(stream);
            stream.Position = 0;
            return isb;
        }



        /// <summary>
        /// Writes the FBX header string
        /// </summary>
        /// <param name="stream"></param>
        protected static void WriteHeader(Stream stream)
        {
            stream.Write(headerString, 0, headerString.Length);
        }

        /// <summary>
        /// Reads the FBX header string
        /// </summary>
        /// <param name="stream"></param>
        /// <returns><c>true</c> if it's compliant</returns>
        protected static bool ReadHeader(Stream stream)
        {
            var buf = new byte[headerString.Length];
            stream.Read(buf, 0, buf.Length);
            return CheckEqual(buf, headerString);
        }

        // Turns out this is the algorithm they use to generate the footer. Who knew!
        static void Encrypt(byte[] a, byte[] b)
        {
            byte c = 64;
            for (int i = 0; i < footerCodeSize; i++)
            {
                a[i] = (byte)(a[i] ^ (byte)(c ^ b[i]));
                c = a[i];
            }
        }

        const string timePath1 = "FBXHeaderExtension";
        const string timePath2 = "CreationTimeStamp";
        static readonly Stack<string> timePath = new Stack<string>(new[] { timePath1, timePath2 });

        // Gets a single timestamp component
        static int GetTimestampVar(FbxNode timestamp, string element)
        {
            var elementNode = timestamp[element];
            if (elementNode != null && elementNode.Properties.Count > 0)
            {
                var prop = elementNode.Properties[0];
                if (prop is int || prop is long)
                    return (int)prop;
            }
            throw new FbxException(timePath, -1, "Timestamp has no " + element);
        }

        /// <summary>
        /// Generates the unique footer code based on the document's timestamp
        /// </summary>
        /// <param name="document"></param>
        /// <returns>A 16-byte code</returns>
        protected static byte[] GenerateFooterCode(FbxNodeList document)
        {
            var timestamp = document.GetRelative(timePath1 + "/" + timePath2);
            if (timestamp == null)
                throw new FbxException(timePath, -1, "No creation timestamp");
            try
            {
                return GenerateFooterCode(
                    GetTimestampVar(timestamp, "Year"),
                    GetTimestampVar(timestamp, "Month"),
                    GetTimestampVar(timestamp, "Day"),
                    GetTimestampVar(timestamp, "Hour"),
                    GetTimestampVar(timestamp, "Minute"),
                    GetTimestampVar(timestamp, "Second"),
                    GetTimestampVar(timestamp, "Millisecond")
                    );
            }
            catch (ArgumentOutOfRangeException)
            {
                throw new FbxException(timePath, -1, "Invalid timestamp");
            }
        }

        /// <summary>
        /// Generates a unique footer code based on a timestamp
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="day"></param>
        /// <param name="hour"></param>
        /// <param name="minute"></param>
        /// <param name="second"></param>
        /// <param name="millisecond"></param>
        /// <returns>A 16-byte code</returns>
        protected static byte[] GenerateFooterCode(
            int year, int month, int day,
            int hour, int minute, int second, int millisecond)
        {
            if (year < 0 || year > 9999)
                throw new ArgumentOutOfRangeException(nameof(year));
            if (month < 0 || month > 12)
                throw new ArgumentOutOfRangeException(nameof(month));
            if (day < 0 || day > 31)
                throw new ArgumentOutOfRangeException(nameof(day));
            if (hour < 0 || hour >= 24)
                throw new ArgumentOutOfRangeException(nameof(hour));
            if (minute < 0 || minute >= 60)
                throw new ArgumentOutOfRangeException(nameof(minute));
            if (second < 0 || second >= 60)
                throw new ArgumentOutOfRangeException(nameof(second));
            if (millisecond < 0 || millisecond >= 1000)
                throw new ArgumentOutOfRangeException(nameof(millisecond));

            var str = (byte[])sourceId.Clone();
            var mangledTime = $"{second:00}{month:00}{hour:00}{day:00}{(millisecond / 10):00}{year:0000}{minute:00}";
            var mangledBytes = Encoding.ASCII.GetBytes(mangledTime);
            Encrypt(str, mangledBytes);
            Encrypt(str, key);
            Encrypt(str, mangledBytes);
            return str;
        }

        /// <summary>
        /// Writes the FBX footer extension (NB - not the unique footer code)
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="version"></param>
        protected void WriteFooter(BinaryWriter stream, int version)
        {
            var zeroes = new byte[Math.Max(footerZeroes1, footerZeroes2)];
            stream.Write(zeroes, 0, footerZeroes1);
            stream.Write(version);
            stream.Write(zeroes, 0, footerZeroes2);
            stream.Write(extension, 0, extension.Length);
        }

        static bool AllZero(byte[] array)
        {
            foreach (var b in array)
                if (b != 0)
                    return false;
            return true;
        }

        /// <summary>
        /// Reads and checks the FBX footer extension (NB - not the unique footer code)
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="version"></param>
        /// <returns><c>true</c> if it's compliant</returns>
        protected bool CheckFooter(BinaryReader stream, FbxVersion version)
        {
            var buffer = new byte[Math.Max(footerZeroes1, footerZeroes2)];
            stream.Read(buffer, 0, footerZeroes1);
            bool correct = AllZero(buffer);
            var readVersion = stream.ReadInt32();
            correct &= (readVersion == (int)version);
            stream.Read(buffer, 0, footerZeroes2);
            correct &= AllZero(buffer);
            stream.Read(buffer, 0, extension.Length);
            correct &= CheckEqual(buffer, extension);
            return correct;
        }
    }

    /// <summary>
    /// Reads FBX nodes from a binary stream
    /// </summary>
    public class FbxBinaryReader : FbxBinary
    {
        private readonly BinaryReader stream;
        private readonly FbxErrorLevel errorLevel;

        private delegate object ReadPrimitive(BinaryReader reader);

        /// <summary>
        /// Creates a new reader
        /// </summary>
        /// <param name="stream">The stream to read from</param>
        /// <param name="errorLevel">When to throw an <see cref="FbxException"/></param>
        /// <exception cref="ArgumentException"><paramref name="stream"/> does
        /// not support seeking</exception>
        public FbxBinaryReader(Stream stream, FbxErrorLevel errorLevel = FbxErrorLevel.Checked)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));
            if (!stream.CanSeek)
                throw new ArgumentException(
                    "The stream must support seeking. Try reading the data into a buffer first");
            this.stream = new BinaryReader(stream, Encoding.ASCII);
            this.errorLevel = errorLevel;
        }

        // Reads a single property
        object ReadProperty()
        {
            var dataType = (char)stream.ReadByte();
            switch (dataType)
            {
                case 'Y':
                    return stream.ReadInt16();
                case 'C':
                    return (char)stream.ReadByte();
                case 'I':
                    return stream.ReadInt32();
                case 'F':
                    return stream.ReadSingle();
                case 'D':
                    return stream.ReadDouble();
                case 'L':
                    return stream.ReadInt64();
                case 'f':
                    return ReadArray(br => br.ReadSingle(), typeof(float));
                case 'd':
                    return ReadArray(br => br.ReadDouble(), typeof(double));
                case 'l':
                    return ReadArray(br => br.ReadInt64(), typeof(long));
                case 'i':
                    return ReadArray(br => br.ReadInt32(), typeof(int));
                case 'b':
                    return ReadArray(br => br.ReadBoolean(), typeof(bool));
                case 'S':
                    var len = stream.ReadInt32();
                    var str = len == 0 ? "" : Encoding.ASCII.GetString(stream.ReadBytes(len));
                    // Convert \0\1 to '::' and reverse the tokens
                    if (str.Contains(binarySeparator))
                    {
                        var tokens = str.Split(new[] { binarySeparator }, StringSplitOptions.None);
                        var sb = new StringBuilder();
                        bool first = true;
                        for (int i = tokens.Length - 1; i >= 0; i--)
                        {
                            if (!first)
                                sb.Append(asciiSeparator);
                            sb.Append(tokens[i]);
                            first = false;
                        }
                        str = sb.ToString();
                    }
                    return str;
                case 'R':
                    return stream.ReadBytes(stream.ReadInt32());
                default:
                    throw new FbxException(stream.BaseStream.Position - 1,
                        "Invalid property data type `" + dataType + "'");
            }
        }

        // Reads an array, decompressing it if required
        Array ReadArray(ReadPrimitive readPrimitive, Type arrayType)
        {
            var len = stream.ReadInt32();
            var encoding = stream.ReadInt32();
            var compressedLen = stream.ReadInt32();
            var ret = Array.CreateInstance(arrayType, len);
            var s = stream;
            var endPos = stream.BaseStream.Position + compressedLen;
            if (encoding != 0)
            {
                if (errorLevel >= FbxErrorLevel.Checked)
                {
                    if (encoding != 1)
                        throw new FbxException(stream.BaseStream.Position - 1,
                            "Invalid compression encoding (must be 0 or 1)");
                    var cmf = stream.ReadByte();
                    if ((cmf & 0xF) != 8 || (cmf >> 4) > 7)
                        throw new FbxException(stream.BaseStream.Position - 1,
                            "Invalid compression format " + cmf);
                    var flg = stream.ReadByte();
                    if (errorLevel >= FbxErrorLevel.Strict && ((cmf << 8) + flg) % 31 != 0)
                        throw new FbxException(stream.BaseStream.Position - 1,
                            "Invalid compression FCHECK");
                    if ((flg & (1 << 5)) != 0)
                        throw new FbxException(stream.BaseStream.Position - 1,
                            "Invalid compression flags; dictionary not supported");
                }
                else
                {
                    stream.BaseStream.Position += 2;
                }
                var codec = new FbxDeflateWithChecksum(stream.BaseStream, CompressionMode.Decompress);
                s = new BinaryReader(codec);
            }
            try
            {
                for (int i = 0; i < len; i++)
                    ret.SetValue(readPrimitive(s), i);
            }
            catch (InvalidDataException)
            {
                throw new FbxException(stream.BaseStream.Position - 1,
                    "Compressed data was malformed");
            }
            if (encoding != 0)
            {
                if (errorLevel >= FbxErrorLevel.Checked)
                {
                    stream.BaseStream.Position = endPos - sizeof(int);
                    var checksumBytes = new byte[sizeof(int)];
                    stream.BaseStream.Read(checksumBytes, 0, checksumBytes.Length);
                    int checksum = 0;
                    for (int i = 0; i < checksumBytes.Length; i++)
                        checksum = (checksum << 8) + checksumBytes[i];
                    if (checksum != ((FbxDeflateWithChecksum)s.BaseStream).Checksum)
                        throw new FbxException(stream.BaseStream.Position,
                            "Compressed data has invalid checksum");
                }
                else
                {
                    stream.BaseStream.Position = endPos;
                }
            }
            return ret;
        }

        /// <summary>
        /// Reads a single node.
        /// </summary>
        /// <remarks>
        /// This won't read the file header or footer, and as such will fail if the stream is a full FBX file
        /// </remarks>
        /// <returns>The node</returns>
        /// <exception cref="FbxException">The FBX data was malformed
        /// for the reader's error level</exception>
        public FbxNode ReadNode()
        {
            var endOffset = stream.ReadInt32();
            var numProperties = stream.ReadInt32();
            var propertyListLen = stream.ReadInt32();
            var nameLen = stream.ReadByte();
            var name = nameLen == 0 ? "" : Encoding.ASCII.GetString(stream.ReadBytes(nameLen));

            if (endOffset == 0)
            {
                // The end offset should only be 0 in a null node
                if (errorLevel >= FbxErrorLevel.Checked && (numProperties != 0 || propertyListLen != 0 || !string.IsNullOrEmpty(name)))
                    throw new FbxException(stream.BaseStream.Position,
                        "Invalid node; expected NULL record");
                return null;
            }

            var node = new FbxNode { Name = name };

            var propertyEnd = stream.BaseStream.Position + propertyListLen;
            // Read properties
            for (int i = 0; i < numProperties; i++)
                node.Properties.Add(ReadProperty());

            if (errorLevel >= FbxErrorLevel.Checked && stream.BaseStream.Position != propertyEnd)
                throw new FbxException(stream.BaseStream.Position,
                    "Too many bytes in property list, end point is " + propertyEnd);

            // Read nested nodes
            var listLen = endOffset - stream.BaseStream.Position;
            if (errorLevel >= FbxErrorLevel.Checked && listLen < 0)
                throw new FbxException(stream.BaseStream.Position,
                    "Node has invalid end point");
            if (listLen > 0)
            {
                FbxNode nested;
                do
                {
                    nested = ReadNode();
                    node.Nodes.Add(nested);
                } while (nested != null);
                if (errorLevel >= FbxErrorLevel.Checked && stream.BaseStream.Position != endOffset)
                    throw new FbxException(stream.BaseStream.Position,
                        "Too many bytes in node, end point is " + endOffset);
            }
            return node;
        }

        /// <summary>
        /// Reads an FBX document from the stream
        /// </summary>
        /// <returns>The top-level node</returns>
        /// <exception cref="FbxException">The FBX data was malformed
        /// for the reader's error level</exception>
        public FbxDocument Read()
        {
            // Read header
            bool validHeader = ReadHeader(stream.BaseStream);
            if (errorLevel >= FbxErrorLevel.Strict && !validHeader)
                throw new FbxException(stream.BaseStream.Position,
                    "Invalid header string");
            var document = new FbxDocument { Version = (FbxVersion)stream.ReadInt32() };

            // Read nodes
            var dataPos = stream.BaseStream.Position;
            FbxNode nested;
            do
            {
                nested = ReadNode();
                if (nested != null)
                    document.Nodes.Add(nested);
            } while (nested != null);

            // Read footer code
            var footerCode = new byte[footerCodeSize];
            stream.BaseStream.Read(footerCode, 0, footerCode.Length);
            if (errorLevel >= FbxErrorLevel.Strict)
            {
                var validCode = GenerateFooterCode(document);
                if (!CheckEqual(footerCode, validCode))
                    throw new FbxException(stream.BaseStream.Position - footerCodeSize,
                        "Incorrect footer code");
            }

            // Read footer extension
            dataPos = stream.BaseStream.Position;
            var validFooterExtension = CheckFooter(stream, document.Version);
            if (errorLevel >= FbxErrorLevel.Strict && !validFooterExtension)
                throw new FbxException(dataPos, "Invalid footer");
            return document;
        }
    }

    /// <summary>
    /// Writes an FBX document to a binary stream
    /// </summary>
    public class FbxBinaryWriter : FbxBinary
    {
        private readonly Stream output;
        private readonly MemoryStream memory;
        private readonly BinaryWriter stream;

        readonly Stack<string> nodePath = new Stack<string>();

        /// <summary>
        /// The minimum size of an array in bytes before it is compressed
        /// </summary>
        public int CompressionThreshold { get; set; } = 1024;

        /// <summary>
        /// Creates a new writer
        /// </summary>
        /// <param name="stream"></param>
        public FbxBinaryWriter(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));
            output = stream;
            // Wrap in a memory stream to guarantee seeking
            memory = new MemoryStream();
            this.stream = new BinaryWriter(memory, Encoding.ASCII);
        }

        private delegate void PropertyWriter(BinaryWriter sw, object obj);

        struct WriterInfo
        {
            public readonly char id;
            public readonly PropertyWriter writer;

            public WriterInfo(char id, PropertyWriter writer)
            {
                this.id = id;
                this.writer = writer;
            }
        }

        private static readonly Dictionary<Type, WriterInfo> writePropertyActions
            = new Dictionary<Type, WriterInfo>
            {
                { typeof(int),  new WriterInfo('I', (sw, obj) => sw.Write((int)obj)) },
                { typeof(short),  new WriterInfo('Y', (sw, obj) => sw.Write((short)obj)) },
                { typeof(long),   new WriterInfo('L', (sw, obj) => sw.Write((long)obj)) },
                { typeof(float),  new WriterInfo('F', (sw, obj) => sw.Write((float)obj)) },
                { typeof(double), new WriterInfo('D', (sw, obj) => sw.Write((double)obj)) },
                { typeof(bool),   new WriterInfo('C', (sw, obj) => sw.Write((byte)(char)obj)) },
                { typeof(byte[]), new WriterInfo('R', WriteRaw) },
                { typeof(string), new WriterInfo('S', WriteString) },
				// null elements indicate arrays - they are checked again with their element type
				{ typeof(int[]),    new WriterInfo('i', null) },
                { typeof(long[]),   new WriterInfo('l', null) },
                { typeof(float[]),  new WriterInfo('f', null) },
                { typeof(double[]), new WriterInfo('d', null) },
                { typeof(bool[]),   new WriterInfo('b', null) },
            };

        static void WriteRaw(BinaryWriter stream, object obj)
        {
            var bytes = (byte[])obj;
            stream.Write(bytes.Length);
            stream.Write(bytes);
        }

        static void WriteString(BinaryWriter stream, object obj)
        {
            var str = obj.ToString();
            // Replace "::" with \0\1 and reverse the tokens
            if (str.Contains(asciiSeparator))
            {
                var tokens = str.Split(new[] { asciiSeparator }, StringSplitOptions.None);
                var sb = new StringBuilder();
                bool first = true;
                for (int i = tokens.Length - 1; i >= 0; i--)
                {
                    if (!first)
                        sb.Append(binarySeparator);
                    sb.Append(tokens[i]);
                    first = false;
                }
                str = sb.ToString();
            }
            var bytes = Encoding.ASCII.GetBytes(str);
            stream.Write(bytes.Length);
            stream.Write(bytes);
        }

        void WriteArray(Array array, Type elementType, PropertyWriter writer)
        {
            stream.Write(array.Length);

            var size = array.Length * Marshal.SizeOf(elementType);
            bool compress = size >= CompressionThreshold;
            stream.Write(compress ? 1 : 0);

            var sw = stream;
            FbxDeflateWithChecksum codec = null;

            var compressLengthPos = stream.BaseStream.Position;
            stream.Write(0); // Placeholder compressed length
            var dataStart = stream.BaseStream.Position;
            if (compress)
            {
                stream.Write(new byte[] { 0x58, 0x85 }, 0, 2); // Header bytes for DeflateStream settings
                codec = new FbxDeflateWithChecksum(stream.BaseStream, CompressionMode.Compress, true);
                sw = new BinaryWriter(codec);
            }
            foreach (var obj in array)
                writer(sw, obj);
            if (compress)
            {
                codec.Close(); // This is important - otherwise bytes can be incorrect
                var checksum = codec.Checksum;
                byte[] bytes =
                {
                    (byte)((checksum >> 24) & 0xFF),
                    (byte)((checksum >> 16) & 0xFF),
                    (byte)((checksum >> 8) & 0xFF),
                    (byte)(checksum & 0xFF),
                };
                stream.Write(bytes);
            }

            // Now we can write the compressed data length, since we know the size
            if (compress)
            {
                var dataEnd = stream.BaseStream.Position;
                stream.BaseStream.Position = compressLengthPos;
                stream.Write((int)(dataEnd - dataStart));
                stream.BaseStream.Position = dataEnd;
            }
        }

        void WriteProperty(object obj, int id)
        {
            if (obj == null)
                return;
            WriterInfo writerInfo;
            if (!writePropertyActions.TryGetValue(obj.GetType(), out writerInfo))
                throw new FbxException(nodePath, id,
                    "Invalid property type " + obj.GetType());
            stream.Write((byte)writerInfo.id);
            // ReSharper disable once AssignNullToNotNullAttribute
            if (writerInfo.writer == null) // Array type
            {
                var elementType = obj.GetType().GetElementType();
                WriteArray((Array)obj, elementType, writePropertyActions[elementType].writer);
            }
            else
                writerInfo.writer(stream, obj);
        }

        // Data for a null node
        static readonly byte[] nullData = new byte[13];

        // Writes a single document to the buffer
        void WriteNode(FbxNode node)
        {
            if (node == null)
            {
                stream.BaseStream.Write(nullData, 0, nullData.Length);
            }
            else
            {
                nodePath.Push(node.Name ?? "");
                var name = string.IsNullOrEmpty(node.Name) ? null : Encoding.ASCII.GetBytes(node.Name);
                if (name != null && name.Length > byte.MaxValue)
                    throw new FbxException(stream.BaseStream.Position,
                        "Node name is too long");

                // Header
                var endOffsetPos = stream.BaseStream.Position;
                stream.Write(0); // End offset placeholder
                stream.Write(node.Properties.Count);
                var propertyLengthPos = stream.BaseStream.Position;
                stream.Write(0); // Property length placeholder
                stream.Write((byte)(name?.Length ?? 0));
                if (name != null)
                    stream.Write(name);

                // Write properties and length
                var propertyBegin = stream.BaseStream.Position;
                for (int i = 0; i < node.Properties.Count; i++)
                {
                    WriteProperty(node.Properties[i], i);
                }
                var propertyEnd = stream.BaseStream.Position;
                stream.BaseStream.Position = propertyLengthPos;
                stream.Write((int)(propertyEnd - propertyBegin));
                stream.BaseStream.Position = propertyEnd;

                // Write child nodes
                if (node.Nodes.Count > 0)
                {
                    foreach (var n in node.Nodes)
                    {
                        if (n == null)
                            continue;
                        WriteNode(n);
                    }
                    WriteNode(null);
                }

                // Write end offset
                var dataEnd = stream.BaseStream.Position;
                stream.BaseStream.Position = endOffsetPos;
                stream.Write((int)dataEnd);
                stream.BaseStream.Position = dataEnd;

                nodePath.Pop();
            }
        }

        /// <summary>
        /// Writes an FBX file to the output
        /// </summary>
        /// <param name="document"></param>
        public void Write(FbxDocument document)
        {
            stream.BaseStream.Position = 0;
            WriteHeader(stream.BaseStream);
            stream.Write((int)document.Version);
            // TODO: Do we write a top level node or not? Maybe check the version?
            nodePath.Clear();
            foreach (var node in document.Nodes)
                WriteNode(node);
            WriteNode(null);
            stream.Write(GenerateFooterCode(document));
            WriteFooter(stream, (int)document.Version);
            output.Write(memory.GetBuffer(), 0, (int)memory.Position);
        }
    }



    /// <summary>
    /// A top-level FBX node
    /// </summary>
    public class FbxDocument : FbxNodeList
    {
        /// <summary>
        /// Describes the format and data of the document
        /// </summary>
        /// <remarks>
        /// It isn't recommended that you change this value directly, because
        /// it won't change any of the document's data which can be version-specific.
        /// Most FBX importers can cope with any version.
        /// </remarks>
        public FbxVersion Version { get; set; } = FbxVersion.v7_4;


        /// <summary>
        /// Creates connections between objects and returns the root nodes.
        /// (added by dexyfex)
        /// </summary>
        /// <returns></returns>
        public List<FbxNode> GetSceneNodes()
        {
            var fobjs = this["Objects"];
            if (fobjs?.Nodes == null)
                return null;

            var fconns = this["Connections"];
            if (fconns?.Nodes == null)
                return null;

            var fobjdict = new Dictionary<long, FbxNode>();
            var rootnodes = new List<FbxNode>();

            foreach (var node in fobjs.Nodes) //put all the object nodes into a decktionary for the connections
            {
                if (node == null) continue;
                long id = 0;
                if (node.Value is long)
                { id = (long)node.Value; }
                if (id == 0)
                { }//shouldn't happen..
                fobjdict[id] = node;
            }

            foreach (var node in fconns.Nodes) //build the scene hierarchy by adding connections to object nodes
            {
                if (node == null) continue;
                var connType = node.Value as string;
                if ((connType == "OO") || (connType == "OP"))
                {
                    if (node.Properties.Count < 3) { continue; }
                    if (!(node.Properties[1] is long)) { continue; }
                    if (!(node.Properties[2] is long)) { continue; }
                    long cid = (long)node.Properties[1];
                    long pid = (long)node.Properties[2];
                    FbxNode cnode;
                    FbxNode pnode;
                    fobjdict.TryGetValue(cid, out cnode);
                    fobjdict.TryGetValue(pid, out pnode);
                    if (cnode == null) { continue; }
                    if (pnode == null)
                    {
                        rootnodes.Add(cnode);
                    }
                    else
                    {
                        pnode.Connections.Add(cnode);
                    }
                }
                else
                { }
            }

            return rootnodes;
        }

    }

    /// <summary>
    /// An error with the FBX data input
    /// </summary>
    public class FbxException : Exception
    {
        /// <summary>
        /// An error at a binary stream offset
        /// </summary>
        /// <param name="position"></param>
        /// <param name="message"></param>
        public FbxException(long position, string message) :
            base($"{message}, near offset {position}")
        {
        }

        /// <summary>
        /// An error in a text file
        /// </summary>
        /// <param name="line"></param>
        /// <param name="column"></param>
        /// <param name="message"></param>
        public FbxException(int line, int column, string message) :
            base($"{message}, near line {line} column {column}")
        {
        }

        /// <summary>
        /// An error in a node object
        /// </summary>
        /// <param name="nodePath"></param>
        /// <param name="propertyID"></param>
        /// <param name="message"></param>
        public FbxException(Stack<string> nodePath, int propertyID, string message) :
            base(message + ", at " + string.Join("/", nodePath.ToArray()) + (propertyID < 0 ? "" : $"[{propertyID}]"))
        {
        }
    }

    /// <summary>
    /// Represents a node in an FBX file
    /// </summary>
    public class FbxNode : FbxNodeList
    {
        /// <summary>
        /// The node name, which is often a class type
        /// </summary>
        /// <remarks>
        /// The name must be smaller than 256 characters to be written to a binary stream
        /// </remarks>
        public string Name { get; set; }

        /// <summary>
        /// The list of properties associated with the node
        /// </summary>
        /// <remarks>
        /// Supported types are primitives (apart from byte and char),arrays of primitives, and strings
        /// </remarks>
        public List<object> Properties { get; } = new List<object>();

        /// <summary>
        /// List of FbxNodes that are connected to this node via the Connections section.
        /// (Added by dexyfex, used by FbxConverter)
        /// </summary>
        public List<FbxNode> Connections { get; } = new List<FbxNode>();

        /// <summary>
        /// The first property element
        /// </summary>
        public object Value
        {
            get { return Properties.Count < 1 ? null : Properties[0]; }
            set
            {
                if (Properties.Count < 1)
                    Properties.Add(value);
                else
                    Properties[0] = value;
            }
        }

        /// <summary>
        /// Whether the node is empty of data
        /// </summary>
        public bool IsEmpty => string.IsNullOrEmpty(Name) && Properties.Count == 0 && Nodes.Count == 0;

        public override string ToString()
        {
            return Name + ((Value != null) ? (": " + Value.ToString()) : "");
        }
    }

    /// <summary>
    /// Base class for nodes and documents
    /// </summary>
    public abstract class FbxNodeList
    {
        /// <summary>
        /// The list of child/nested nodes
        /// </summary>
        /// <remarks>
        /// A list with one or more null elements is treated differently than an empty list,
        /// and represented differently in all FBX output files.
        /// </remarks>
        public List<FbxNode> Nodes { get; } = new List<FbxNode>();

        /// <summary>
        /// Gets a named child node
        /// </summary>
        /// <param name="name"></param>
        /// <returns>The child node, or null</returns>
        public FbxNode this[string name] { get { return Nodes.Find(n => n != null && n.Name == name); } }

        /// <summary>
        /// Gets a child node, using a '/' separated path
        /// </summary>
        /// <param name="path"></param>
        /// <returns>The child node, or null</returns>
        public FbxNode GetRelative(string path)
        {
            var tokens = path.Split('/');
            FbxNodeList n = this;
            foreach (var t in tokens)
            {
                if (t == "")
                    continue;
                n = n[t];
                if (n == null)
                    break;
            }
            return n as FbxNode;
        }
    }

    /// <summary>
    /// Enumerates the FBX file versions
    /// </summary>
    public enum FbxVersion
    {
        /// <summary>
        /// FBX version 6.0
        /// </summary>
        v6_0 = 6000,

        /// <summary>
        /// FBX version 6.1
        /// </summary>
        v6_1 = 6100,

        /// <summary>
        /// FBX version 7.0
        /// </summary>
        v7_0 = 7000,

        /// <summary>
        /// FBX 2011 version
        /// </summary>
        v7_1 = 7100,

        /// <summary>
        /// FBX 2012 version
        /// </summary>
        v7_2 = 7200,

        /// <summary>
        /// FBX 2013 version
        /// </summary>
        v7_3 = 7300,

        /// <summary>
        /// FBX 2014 version
        /// </summary>
        v7_4 = 7400,
    }



    /// <summary>
    /// Indicates when a reader should throw errors
    /// </summary>
    public enum FbxErrorLevel
    {
        /// <summary>
        /// Ignores inconsistencies unless the parser can no longer continue
        /// </summary>
        Permissive = 0,

        /// <summary>
        /// Checks data integrity, such as checksums and end points
        /// </summary>
        Checked = 1,

        /// <summary>
        /// Checks everything, including magic bytes
        /// </summary>
        Strict = 2,
    }


    /// <summary>
    /// A wrapper for DeflateStream that calculates the Adler32 checksum of the payload
    /// </summary>
    public class FbxDeflateWithChecksum : DeflateStream
    {
        private const int modAdler = 65521;
        private uint checksumA;
        private uint checksumB;

        /// <summary>
        /// Gets the Adler32 checksum at the current point in the stream
        /// </summary>
        public int Checksum
        {
            get
            {
                checksumA %= modAdler;
                checksumB %= modAdler;
                return (int)((checksumB << 16) | checksumA);
            }
        }

        /// <inheritdoc />
        public FbxDeflateWithChecksum(Stream stream, CompressionMode mode) : base(stream, mode)
        {
            ResetChecksum();
        }

        /// <inheritdoc />
        public FbxDeflateWithChecksum(Stream stream, CompressionMode mode, bool leaveOpen) : base(stream, mode, leaveOpen)
        {
            ResetChecksum();
        }

        // Efficiently extends the checksum with the given buffer
        void CalcChecksum(byte[] array, int offset, int count)
        {
            checksumA %= modAdler;
            checksumB %= modAdler;
            for (int i = offset, c = 0; i < (offset + count); i++, c++)
            {
                checksumA += array[i];
                checksumB += checksumA;
                if (c > 4000) // This is about how many iterations it takes for B to reach IntMax
                {
                    checksumA %= modAdler;
                    checksumB %= modAdler;
                    c = 0;
                }
            }
        }

        /// <inheritdoc />
        public override void Write(byte[] array, int offset, int count)
        {
            base.Write(array, offset, count);
            CalcChecksum(array, offset, count);
        }

        /// <inheritdoc />
        public override int Read(byte[] array, int offset, int count)
        {
            var ret = base.Read(array, offset, count);
            CalcChecksum(array, offset, count);
            return ret;
        }

        /// <summary>
        /// Initializes the checksum values
        /// </summary>
        public void ResetChecksum()
        {
            checksumA = 1;
            checksumB = 0;
        }
    }

}
