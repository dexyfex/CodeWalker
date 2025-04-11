using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeWalker.ModManager
{
    public class SimpleKvpFile
    {
        public string FileName;
        public string FilePath;
        public bool FileExists;
        public bool OnlySaveIfFileExists;
        public Exception FileError;
        public Dictionary<string, string> Items = new Dictionary<string, string>();


        public SimpleKvpFile() { }
        public SimpleKvpFile(string filePath, bool loadFile = false)
        {
            FilePath = filePath;
            FileName = Path.GetFileName(filePath);
            if (loadFile)
            {
                Load();
            }
        }

        public virtual void Load()
        {
            Items.Clear();
            FileExists = File.Exists(FilePath);
            if (FileExists == false)
            {
                FileError = new Exception($"File not found: {FilePath}");
                return;
            }
            try
            {
                var lines = File.ReadAllLines(FilePath);
                if (lines == null) return;
                foreach (var line in lines)
                {
                    var tline = line?.Trim();
                    if (string.IsNullOrEmpty(tline)) continue;
                    var spi = tline.IndexOf(' ');
                    if (spi < 1) continue;
                    if (spi >= (tline.Length - 1)) continue;
                    var key = tline.Substring(0, spi).Trim().Replace("<SPACE>", " ");
                    var val = tline.Substring(spi + 1).Trim().Replace("<NEWLINE>", "\n");
                    if (string.IsNullOrEmpty(key)) continue;
                    if (string.IsNullOrEmpty(val)) continue;
                    Items[key] = val;
                }
            }
            catch (Exception ex)
            {
                FileError = ex;
                FileExists = false;
            }
        }

        public virtual void Save()
        {
            if ((FileExists == false) && OnlySaveIfFileExists) return;
            try
            {
                var sb = new StringBuilder();
                foreach (var kvp in Items)
                {
                    var key = kvp.Key?.Replace(" ", "<SPACE>");
                    var val = kvp.Value?.Replace("\n", "<NEWLINE>");
                    sb.AppendLine($"{key} {val}");
                }
                var str = sb.ToString();
                File.WriteAllText(FilePath, str);
            }
            catch (Exception ex)
            {
                FileError = ex;
                //FileExists = false;
            }
        }

        public string GetItem(string key)
        {
            if (string.IsNullOrEmpty(key)) return null;
            Items.TryGetValue(key, out var item);
            return item;
        }
        public void SetItem(string key, string val)
        {
            if (string.IsNullOrEmpty(key)) return;
            Items[key] = val;
        }

    }
}
