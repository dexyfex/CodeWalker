using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CodeWalker.Core.Utils
{
    public static class FileUtils
    {
        public static bool TryFindFolder(string dirToFind, out string folder)
        {
            return TryFindFolder(dirToFind, null, out folder);
        }

        public static bool TryFindFolder(string dirToFind, string? basePath, [NotNullWhen(true)]out string folder)
        {
            basePath ??= Assembly.GetExecutingAssembly().Location;
            // Search up directory tree starting at assembly path looking for 'Images' dir.
            var searchPath = Path.GetDirectoryName(basePath);

            ArgumentNullException.ThrowIfNullOrEmpty(basePath, nameof(basePath));
            while (true)
            {
                var testPath = Path.Combine(searchPath, dirToFind);
                if (Directory.Exists(testPath))
                {
                    // Found it!
                    folder = testPath;
                    return true;
                }

                // Move up one directory.
                var newSearchPath = Path.GetFullPath(Path.Combine(searchPath, ".."));
                if (newSearchPath == searchPath)
                {
                    // Didn't move up, so we're at the root.
                    folder = null;
                    return false;
                }
                searchPath = newSearchPath;
            }
        }
    }
}
