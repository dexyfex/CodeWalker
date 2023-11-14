﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CodeWalker.GameFiles
{

    public class JenkHash
    {
        public JenkHashInputEncoding Encoding { get; set; }
        public string Text { get; set; }
        public int HashInt { get; set; }
        public uint HashUint { get; set; }
        public string HashHex { get; set; }

        public JenkHash(string text, JenkHashInputEncoding encoding)
        {
            Encoding = encoding;
            Text = text;
            HashUint = GenHash(text, encoding);
            HashInt = (int)HashUint;
            HashHex = "0x" + HashUint.ToString("X");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static char ToLower(char c)
        {
            return (c >= 'A' && c <= 'Z') ? (char)(c - 'A' + 'a') : c;
        }

        public static uint GenHash(string text, JenkHashInputEncoding encoding)
        {
            uint h = 0;
            byte[] chars;

            switch (encoding)
            {
                default:
                case JenkHashInputEncoding.UTF8:
                    chars = UTF8Encoding.UTF8.GetBytes(text);
                    break;
                case JenkHashInputEncoding.ASCII:
                    chars = ASCIIEncoding.ASCII.GetBytes(text);
                    break;
            }

            for (uint i = 0; i < chars.Length; i++)
            {
                h += chars[i];
                h += (h << 10);
                h ^= (h >> 6);
            }
            h += (h << 3);
            h ^= (h >> 11);
            h += (h << 15);

            return h;
        }

        public static uint GenHashLower(string text)
        {
            if (text == null) return 0;
            uint h = 0;
            for (int i = 0; i < text.Length; i++)
            {
                h += (byte)ToLower(text[i]);
                h += (h << 10);
                h ^= (h >> 6);
            }
            h += (h << 3);
            h ^= (h >> 11);
            h += (h << 15);

            return h;
        }

        public static uint GenHashLower(ReadOnlySpan<char> text, ReadOnlySpan<char> str2 = default)
        {
            if (text == null) return 0;
            uint h = 0;
            for (int i = 0; i < text.Length; i++)
            {
                h += (byte)ToLower(text[i]);
                h += (h << 10);
                h ^= (h >> 6);
            }
            for (int i = 0; i < str2.Length; i++)
            {
                h += (byte)ToLower(str2[i]);
                h += (h << 10);
                h ^= (h >> 6);
            }
            h += (h << 3);
            h ^= (h >> 11);
            h += (h << 15);

            return h;
        }

        public static uint GenHash(ReadOnlySpan<char> text)
        {
            if (text == null) return 0;
            uint h = 0;
            for (int i = 0; i < text.Length; i++)
            {
                h += (byte)text[i];
                h += (h << 10);
                h ^= (h >> 6);
            }
            h += (h << 3);
            h ^= (h >> 11);
            h += (h << 15);

            return h;
        }

        public static uint GenHash(string text)
        {
            if (text == null) return 0;
            uint h = 0;
            for (int i = 0; i < text.Length; i++)
            {
                h += (byte)text[i];
                h += (h << 10);
                h ^= (h >> 6);
            }
            h += (h << 3);
            h ^= (h >> 11);
            h += (h << 15);

            return h;
        }

        public static uint GenHash(byte[] data)
        {
            uint h = 0;
            for (uint i = 0; i < data.Length; i++)
            {
                h += data[i];
                h += (h << 10);
                h ^= (h >> 6);
            }
            h += (h << 3);
            h ^= (h >> 11);
            h += (h << 15);
            return h;
        }

    }

    public enum JenkHashInputEncoding
    {
        UTF8 = 0,
        ASCII = 1,
    }


    public class JenkIndMatch
    {
        public string Hash { get; set; }
        public string Value { get; set; }
        public double Score { get; set; }

        public JenkIndMatch(string hash, string val)
        {
            Hash = hash;
            Value = val;
            CalculateScore();
        }

        public void CalculateScore()
        {

            int wordlength = 0;
            int wordrank = 0;

            string okwordsymbs = " _-.";
            string goodwordsymbs = "_";

            for (int i = 0; i < Value.Length; i++)
            {
                char c = Value[i];

                bool wordchar = (char.IsLetter(c) || char.IsDigit(c) || goodwordsymbs.Contains(c));

                if (wordchar)
                {
                    wordlength++;
                }
                else if (okwordsymbs.Contains(c))
                {
                    //wordlength++; //don't add this to the score, but allow it to continue the chain
                }
                else
                {
                    if (wordlength > 2)
                    {
                        wordrank += wordlength; //linear word increment, ignoring 1-2char matches
                    }
                    wordlength = 0;
                }

                //wordrank += wordlength; //each sequential letter in a word contributes more to the rank, ie. 1+2+3+4+...
            }
            if (wordlength > 2)
            {
                wordrank += wordlength; //linear word increment, ignoring 1-2char matches
            }


            if (Value.Length > 0)
            {
                //the max value for a given length when 1+2+3+4+5+..n = n(n+1)/2
                //double n = (double)Value.Length;
                //double maxscore = n * (n + 1.0) * 0.5;

                double n = (double)Value.Length;
                Score = (((double)wordrank) / n);
                //Score = (((double)wordrank));
            }
            else
            {
                Score = 0.0;
            }

        }

        public override string ToString()
        {
            return string.Format("{0} -> {1}   ({2:0.##})", Hash, Value, Score);
        }
    }

    public class JenkIndProblem
    {
        public string Filename { get; set; }
        public string Excuse { get; set; }
        public int Line { get; set; }

        public JenkIndProblem(string filepath, string excuse, int line)
        {
            Filename = Path.GetFileName(filepath);
            Excuse = excuse;
            Line = line;
        }
        public override string ToString()
        {
            return string.Format("{0} : {1} at line {2}", Filename, Excuse, Line);
        }
    }



    public static class DictionaryExtension
    {
        public static bool TryAdd<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue value)
        {
            if (dict.ContainsKey(key))
            {
                return false;
            }

            dict[key] = value;
            return true;
        }
    }



    public static class JenkIndex
    {
        //public static ConcurrentDictionary<uint, string> Index = new ConcurrentDictionary<uint, string>(Environment.ProcessorCount * 2, 2000000);
        public static Dictionary<uint, string> Index = new Dictionary<uint, string>(2000000);

        public static void Ensure(string str)
        {
            uint hash = JenkHash.GenHash(str);
            Ensure(str, hash);
        }

        public static void Ensure(string str, uint hash)
        {
            if (hash == 0) return;

            if (Index.ContainsKey(hash))
            {
                return;
            }

            lock(Index)
            {
                Index[hash] = str;
            }
        }

        public static void Ensure(ReadOnlySpan<char> str, uint hash)
        {
            if (hash == 0) return;

            if (Index.ContainsKey(hash))
            {
                return;
            }

            lock(Index)
            {
                Index[hash] = str.ToString();
            }
        }

        public static void EnsureLower(string str)
        {
            uint hash = JenkHash.GenHashLower(str);
            Ensure(str, hash);
        }

        public static void EnsureLower(ReadOnlySpan<char> str)
        {
            uint hash = JenkHash.GenHashLower(str);
            Ensure(str, hash);
        }

        public static void EnsureBoth(string str)
        {
            uint hash = JenkHash.GenHash(str);
            uint hashLower = JenkHash.GenHashLower(str);
            Ensure(str, hash);
            if (hash != hashLower)
            {
                Ensure(str, hashLower);
            }
        }

        public static void EnsureBoth(ReadOnlySpan<char> str)
        {
            uint hash = JenkHash.GenHash(str);
            uint hashLower = JenkHash.GenHashLower(str);
            Ensure(str, hash);
            if (hash != hashLower)
            {
                Ensure(str, hashLower);
            }
        }

        public static string GetString(uint hash)
        {
            string res;
            if (!Index.TryGetValue(hash, out res))
            {
                res = hash.ToString();
            }
            return res;
        }
        public static string TryGetString(uint hash)
        {
            string res;
            if (!Index.TryGetValue(hash, out res))
            {
                res = string.Empty;
            }
            return res;
        }

        public static ICollection<string> GetAllStrings()
        {
            var res = Index.Values;
            return res;
        }

    }


}
