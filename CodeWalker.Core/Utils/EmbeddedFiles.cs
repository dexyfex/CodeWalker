using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace CodeWalker
{

    public static class EmbeddedFiles
    {
        private const string ResourcePrefix = "CodeWalker.Files/";

        private static readonly Assembly Container = typeof(EmbeddedFiles).Assembly;
        private static readonly Dictionary<string, string> ResourceNames = BuildIndex();


        public static bool Exists(string appRelativePath)
        {
            return ResourceNames.ContainsKey(Normalise(appRelativePath));
        }

        public static Stream Open(string appRelativePath)
        {
            string name;
            if (ResourceNames.TryGetValue(Normalise(appRelativePath), out name) == false) return null;
            return Container.GetManifestResourceStream(name);
        }

        public static byte[] ReadAllBytes(string appRelativePath)
        {
            using (var stream = Open(appRelativePath))
            {
                if (stream == null) return null;
                var data = new byte[stream.Length];
                var offset = 0;
                while (offset < data.Length)
                {
                    var count = stream.Read(data, offset, data.Length - offset);
                    if (count <= 0) break;
                    offset += count;
                }
                return data;
            }
        }

        public static string ReadAllText(string appRelativePath)
        {
            using (var stream = Open(appRelativePath))
            {
                if (stream == null) return null;
                using (var reader = new StreamReader(stream, Encoding.UTF8, true))
                {
                    return reader.ReadToEnd();
                }
            }
        }


        private static Dictionary<string, string> BuildIndex()
        {
            var index = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var name in Container.GetManifestResourceNames())
            {
                if (name.StartsWith(ResourcePrefix, StringComparison.OrdinalIgnoreCase) == false) continue;
                index[name.Substring(ResourcePrefix.Length)] = name;
            }
            return index;
        }

        private static string Normalise(string appRelativePath)
        {
            if (string.IsNullOrEmpty(appRelativePath)) return string.Empty;
            return appRelativePath.Replace('\\', '/').TrimStart('/');
        }

    }

}
