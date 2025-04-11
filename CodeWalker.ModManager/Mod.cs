using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeWalker.ModManager
{
    public class Mod
    {
        public string Name;
        public string IconFile;//optional, relative to the local path
        public string LocalPath;//in the mod manager folder
        public string SourcePath;//original file that was imported/loaded
        public ModType Type;
        public ModStatus Status;
        public int LoadOrder = -1;
        public List<ModFile> Files = new List<ModFile>();
        public List<string> LogItems = new List<string>();
        public object IconObject;//loaded Image object for use by the form
        public string TypeStatusString => $"{Type} : {Status}";//, {(Enabled ? "Enabled" : "Disabled")}";


        public static Mod Load(string localDir)
        {
            //create Mod object for an already installed mod

            if (Directory.Exists(localDir) == false) return null;
            var mod = new Mod();
            mod.LocalPath = localDir;
            mod.LoadCWMMFile();

            foreach (var file in mod.Files)
            {
                file?.UpdateLocalPath(localDir);
            }

            if ((mod.IconObject == null) && (string.IsNullOrEmpty(mod.IconFile) == false))
            {
                try
                {
                    mod.IconObject = Image.FromFile(mod.IconFile);
                }
                catch
                { }
            }

            return mod;
        }

        public static Mod BeginInstall(string file)
        {
            //create Mod object for an imported file, determine name and type
            //TODO: check legal path characters in name

            if (string.IsNullOrEmpty(file)) return null;
            var isdir = Directory.Exists(file);
            if ((isdir == false) && (File.Exists(file) == false)) return null;
            var dp = Path.GetDirectoryName(file);
            var fn = Path.GetFileNameWithoutExtension(file);
            var dn = Path.GetFileName(dp);
            var mod = new Mod();
            mod.SourcePath = file;
            mod.Log($"BeginInstall: {DateTime.Now}");
            if (isdir)//this is a folder not a file..
            {
                //TODO?
                //handle import folder as new loose mod?
                //search for dlc.rpf, .oiv, .asi etc files first?
                mod.Name = fn;
                mod.Type = ModType.Loose;
                mod.AddModFilesFromDir(dp);
            }
            else
            {
                mod.Type = GetModType(file);
                if (mod.Type == ModType.DLC)
                {
                    mod.Name = dn;//use the containing folder for the mod name
                    mod.AddModFile(file);
                }
                else if (mod.Type == ModType.OIV)
                {
                    if (fn.StartsWith("uninstall", StringComparison.InvariantCultureIgnoreCase))
                    {
                        return null;//don't try and install OIV "uninstaller" packages...
                    }

                    mod.Name = fn;
                    mod.AddModFile(file);
                    //will need to unzip oiv file to the local cache dir...
                }
                else if (mod.Type == ModType.ASI)
                {
                    mod.Name = fn;
                    mod.AddModFilesFromDir(dp);
                }
                else
                {
                    mod.Name = dn;//use the containing folder for the mod name
                    mod.AddModFile(file);
                    //for random files, add to default loose files mod?
                }
            }
            return mod;
        }
        public void CompleteInstall(string localDir)
        {
            //copy the source mod files to the local path, and save the cwmm file

            LocalPath = localDir;


            //TODO: unpack .oiv etc here....
            //TODO: set LoadOrder using what is specified in DLCs etc
            if (Type == ModType.OIV)
            {
                Log($"Extracting {SourcePath}");
                System.IO.Compression.ZipFile.ExtractToDirectory(SourcePath, localDir);

                Files.Clear();
                var oivfiles = Directory.GetFiles(localDir, "*", SearchOption.AllDirectories);
                foreach (var oivfile in oivfiles)
                {
                    var fe = Path.GetExtension(oivfile)?.ToLowerInvariant();
                    if (fe == ".cwmm") continue;
                    var file = new ModFile(oivfile);
                    file.LocalPath = file.SourcePath;
                    Files.Add(file);
                }
            }
            else
            {


                foreach (var file in Files)
                {
                    file.UpdateLocalPath(localDir);

                    Log($"Installing {file.LocalPath}");
                    try
                    {
                        if (File.Exists(file.SourcePath) == false)
                        {
                            Log($"File {file.SourcePath} does not exist, skipping.");
                            continue;
                        }
                        if (File.Exists(file.LocalPath))
                        {
                            Log($"File {file.LocalPath} already exists, replacing.");
                            File.Delete(file.LocalPath);
                        }

                        File.Copy(file.SourcePath, file.LocalPath);
                    }
                    catch (Exception ex)
                    {
                        Log($"Error: {ex}");
                    }
                }
            }

            SaveCWMMFile();

            Log($"CompleteInstall: {DateTime.Now}");

            SaveLogFile();
        }

        public static ModType GetModType(string file)
        {
            if (string.IsNullOrEmpty(file)) return ModType.Loose;
            if (file.EndsWith("dlc.rpf", StringComparison.OrdinalIgnoreCase))
            {
                return ModType.DLC;
            }
            if (file.EndsWith(".oiv", StringComparison.OrdinalIgnoreCase))
            {
                return ModType.OIV;
            }
            if (file.EndsWith(".asi", StringComparison.OrdinalIgnoreCase))
            {
                return ModType.ASI;
            }
            return ModType.Loose;
        }
        public static bool CanInstallFile(string file)
        {
            if (string.IsNullOrEmpty(file)) return false;
            if (File.Exists(file) == false) return false;
            //var isdir = Directory.Exists(file);
            //if ((isdir == false) && (File.Exists(file) == false)) return false;

            var t = GetModType(file);
            switch (t)
            {
                case ModType.DLC:
                case ModType.OIV:
                case ModType.ASI:
                    return true;
                default: 
                    return false;
            }
        }


        private void AddModFile(string sourceFile)
        {
            Files.Add(new ModFile(sourceFile));
        }
        private void AddModFilesFromDir(string sourceDir)
        {
            var allfiles = Directory.GetFiles(sourceDir, "*", SearchOption.TopDirectoryOnly);
            foreach (var afile in allfiles)
            {
                if (File.Exists(afile) == false)
                {
                    Log($"Couldn't add file {afile}");
                    continue;
                }
                Files.Add(new ModFile(afile));
            }
        }




        private void LoadCWMMFile()
        {
            Files.Clear();
            var fp = GetCWMMFilePath();
            if (string.IsNullOrEmpty(fp)) return;
            if (File.Exists(fp) == false) return;
            Log($"LoadCWMMFile: {fp}");
            var f = new SimpleKvpFile(fp, true);
            if (f.FileError != null)
            {
                Log($"Error: {f.FileError}");
            }
            Name = f.GetItem("Name");
            IconFile = f.GetItem("Icon");
            SourcePath = f.GetItem("Source");
            Enum.TryParse(f.GetItem("Type"), out Type);
            Enum.TryParse(f.GetItem("Status"), out Status);
            int.TryParse(f.GetItem("Order"), out LoadOrder);
            int.TryParse(f.GetItem("Files"), out var count);
            for (var i = 0; i < count; i++)
            {
                var mf = f.GetItem($"File{i}");
                AddModFile(mf);
            }
        }
        private void SaveCWMMFile()
        {
            var fp = GetCWMMFilePath();
            if (string.IsNullOrEmpty(fp)) return;
            Log($"SaveCWMMFile: {fp}");
            var f = new SimpleKvpFile(fp);
            f.SetItem("Name", Name);
            f.SetItem("Icon", IconFile);
            f.SetItem("Source", SourcePath);
            f.SetItem("Type", Type.ToString());
            f.SetItem("Status", Status.ToString());
            f.SetItem("Order", LoadOrder.ToString());
            f.SetItem("Files", Files.Count.ToString());
            for (var i = 0; i < Files.Count; i++)
            {
                f.SetItem($"File{i}", Files[i].SourcePath);
            }
            f.Save();
            if (f.FileError != null)
            {
                Log($"Error: {f.FileError}");
            }
        }
        private void SaveLogFile()
        {
            try
            {
                var fp = GetLogFilePath();
                File.AppendAllLines(fp, LogItems);
            }
            catch { }
        }

        private string GetCWMMFilePath()
        {
            return $"{LocalPath}\\mod.cwmm";
        }
        private string GetLogFilePath()
        {
            return $"{LocalPath}\\log.cwmm";
        }


        private void Log(string s)
        {
            LogItems.Add(s);
        }


        public override string ToString()
        {
            return $"{Type}: {Name}";
        }
    }

    public class ModFile
    {
        public string Name;
        public string LocalPath;//in the mod manager folder
        public string SourcePath;//original file that was imported/loaded

        public ModFile()
        {
        }
        public ModFile(string sourcePath)
        {
            Name = Path.GetFileName(sourcePath);
            SourcePath = sourcePath;
        }

        public void UpdateLocalPath(string modPath)
        {
            LocalPath = modPath + "\\" + Name;
        }

        public override string ToString()
        {
            return $"ModFile: {Name}";
        }
    }

    public enum ModType
    {
        Loose,
        DLC,
        OIV,
        ASI,
        ASILoader,
        ModLoader,
    }

    public enum ModStatus
    {
        Pending,
        Ready,
    }


}
