using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeWalker.GameFiles
{


    public static class StatsNames
    {
        public static Dictionary<uint, string> Index = new Dictionary<uint, string>();
        private static object syncRoot = new object();

        public static volatile bool FullIndexBuilt = false;

        public static void Clear()
        {
            lock (syncRoot)
            {
                Index.Clear();
            }
        }

        public static bool Ensure(string str)
        {
            uint hash = JenkHash.GenHash(str);
            if (hash == 0) return true;
            lock (syncRoot)
            {
                if (!Index.ContainsKey(hash))
                {
                    Index.Add(hash, str);
                    return false;
                }
            }
            return true;
        }

        public static bool Ensure(string str, uint hash)
        {
            if (hash == 0) return true;
            lock (syncRoot)
            {
                if (!Index.ContainsKey(hash))
                {
                    Index.Add(hash, str);
                    return false;
                }
            }
            return true;
        }

        public static string GetString(uint hash)
        {
            string res;
            lock (syncRoot)
            {
                if (!Index.TryGetValue(hash, out res))
                {
                    res = hash.ToString();
                }
            }
            return res;
        }
        public static string TryGetString(uint hash)
        {
            string res;
            lock (syncRoot)
            {
                if (!Index.TryGetValue(hash, out res))
                {
                    res = string.Empty;
                }
            }
            return res;
        }

    }



}
