using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace CodeWalker
{

    internal static class EmbeddedAssemblies
    {
        private const string ResourcePrefix = "CodeWalker.Assemblies/";

        private static readonly Assembly Container = typeof(EmbeddedAssemblies).Assembly;
        private static readonly Dictionary<string, string> ResourceNames = BuildIndex();
        private static readonly Dictionary<string, Assembly> LoadedAssemblies = new Dictionary<string, Assembly>(StringComparer.OrdinalIgnoreCase);


        public static void Install()
        {
            AppDomain.CurrentDomain.AssemblyResolve += Resolve;
        }


        private static Assembly Resolve(object sender, ResolveEventArgs args)
        {
            var name = new AssemblyName(args.Name).Name;

            lock (LoadedAssemblies)
            {
                Assembly assembly;
                if (LoadedAssemblies.TryGetValue(name, out assembly)) return assembly;

                string resourceName;
                if (ResourceNames.TryGetValue(name, out resourceName) == false) return null;

                assembly = Load(resourceName);
                LoadedAssemblies[name] = assembly;
                return assembly;
            }
        }

        private static Assembly Load(string resourceName)
        {
            using (var stream = Container.GetManifestResourceStream(resourceName))
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
                return Assembly.Load(data);
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

    }

}
