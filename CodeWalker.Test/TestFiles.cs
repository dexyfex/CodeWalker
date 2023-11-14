using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CodeWalker.Test
{
    internal class TestFiles
    {
        public static string GetFilePath(string filename)
        {
            // Directory we're looking for.
            var dirToFind = Path.Combine(@"CodeWalker.Test", "Files");

            // Search up directory tree starting at assembly path looking for 'Images' dir.
            var searchPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            while (true)
            {
                var testPath = Path.Combine(searchPath, dirToFind);
                if (Directory.Exists(testPath))
                {
                    // Found it!
                    return Path.Combine(testPath, filename);
                }

                // Move up one directory.
                var newSearchPath = Path.GetFullPath(Path.Combine(searchPath, ".."));
                if (newSearchPath == searchPath)
                {
                    // Didn't move up, so we're at the root.
                    throw new FileNotFoundException($"Could not find '{dirToFind}' directory.");
                }
                searchPath = newSearchPath;
            }
        }
    }
}
