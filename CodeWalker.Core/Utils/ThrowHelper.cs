//using System;
//using System.Collections.Generic;
//using System.Diagnostics.CodeAnalysis;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace CodeWalker.Core.Utils
//{
//    internal class ThrowHelper
//    {
//        internal static Exception CreateEndOfFileException() =>
//            new EndOfStreamException("Tried to read stream beyond end");

//        [DoesNotReturn]
//        internal static void ThrowEndOfFileException()
//        {
//            throw CreateEndOfFileException();
//        }
//    }
//}
