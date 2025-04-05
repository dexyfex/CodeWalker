using CodeWalker.GameFiles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace CodeWalker.Core.Utils
{
    public class Gen9Converter
    {
        //converts files from legacy to enhanced format.

        public string InputFolder;//source of files to convert
        public string OutputFolder;//destination for converted files
        public bool ProcessSubfolders = true;//recurse all the subfolders?
        public bool OverwriteExisting = true;//replace existing files in the output folder? (otherwise ignore)
        public bool CopyUnconverted = true;//also copy files that don't need converting?
        public Func<string, string, bool> QuestionFunc;//(message, title, result) called from the calling thread only
        public Action<string> ErrorAction;//this will be called from the calling thread only, for prechecks.
        public Action<string> LogAction;//this will be called from the conversion task thread during processing.
        public Action<float> ProgressAction;//will be called for each file being converted
        public Action<bool> StartStopAction;//(bool start) called when actual conversion process begins/ends


        public void Convert()
        {
            var inputFolder = InputFolder?.Replace('/', '\\');
            var outputFolder = OutputFolder?.Replace('/', '\\');
            var subfolders = ProcessSubfolders;
            var overwrite = OverwriteExisting;
            var copyunconverted = CopyUnconverted;
            if (string.IsNullOrEmpty(inputFolder))
            {
                Error("Please select an input folder.");
                return;
            }
            if (string.IsNullOrEmpty(outputFolder))
            {
                Error("Please select an output folder.");
                return;
            }
            if (inputFolder.EndsWith("\\") == false)
            {
                inputFolder = inputFolder + "\\";
            }
            if (outputFolder.EndsWith("\\") == false)
            {
                outputFolder = outputFolder + "\\";
            }
            if (inputFolder.Equals(outputFolder, StringComparison.InvariantCultureIgnoreCase))
            {
                Error("Input folder and Output folder must be different.");
                return;
            }
            if (Directory.Exists(inputFolder) == false)
            {
                Error($"Input folder {inputFolder} does not exist.");
                return;
            }
            if (Directory.Exists(outputFolder) == false)
            {
                if (Question($"Output folder {outputFolder} does not exist.\nWould you like to create it?", "Create output folder?", true) == true)
                {
                    try
                    {
                        Directory.CreateDirectory(outputFolder);
                    }
                    catch (Exception ex)
                    {
                        Error($"Error creating Output folder:\n{ex.Message}");
                        return;
                    }
                    if (Directory.Exists(outputFolder) == false)
                    {
                        return;
                    }
                }
                else
                {
                    return;
                }
            }

            string[] allpaths = null;
            try
            {
                var soption = subfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                allpaths = Directory.GetFileSystemEntries(inputFolder, "*", soption);
                if ((allpaths == null) || (allpaths.Length == 0))
                {
                    Error($"Input folder {inputFolder} is empty.");
                    return;
                }
            }
            catch (Exception ex)
            {
                Error($"Error listing Input folder contents:\n{ex.Message}");
                return;
            }

            var singleFileProgress = 1.0f / allpaths.Length;
            var curFile = 0;

            Task.Run(new Action(() =>
            {
                StartStop(true);
                Log($"Conversion process started at {DateTime.Now}");
                Log($"Input folder: {inputFolder}\r\nOutput folder: {outputFolder}");

                var exgen9 = RpfManager.IsGen9;
                RpfManager.IsGen9 = true;

                foreach (var path in allpaths)
                {
                    try
                    {
                        curFile++;
                        Progress(curFile * singleFileProgress);
                        var pathl = path.ToLowerInvariant();
                        var relpath = path.Substring(inputFolder.Length);
                        var outpath = outputFolder + relpath;
                        if ((overwrite == false) && File.Exists(outpath))
                        {
                            Log($"{relpath} - output file already exists, skipping.");
                            continue;
                        }
                        if (File.Exists(path) == false)
                        {
                            //Log($"{relpath} - input file does not exist, skipping.");
                            continue;
                        }
                        var outdir = Path.GetDirectoryName(outpath);
                        if (Directory.Exists(outdir) == false)
                        {
                            Directory.CreateDirectory(outdir);
                        }

                        //Log($"{relpath}...");
                        var ext = Path.GetExtension(pathl);
                        if (ext == ".rpf")
                        {
                            Log($"{relpath} - Cannot convert RPF files! Extract the contents of the RPF and convert that instead.");

                        }
                        else
                        {
                            var datain = File.ReadAllBytes(path);
                            var dataout = TryConvert(datain, ext, Log, relpath, copyunconverted, out var converted);
                            if (dataout != null)
                            {
                                File.WriteAllBytes(outpath, dataout);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log($"Error processing {path}:\r\n{ex}");
                    }
                }

                RpfManager.IsGen9 = exgen9;

                Log($"Conversion process completed at {DateTime.Now}");
                StartStop(false);
            }));

        }

        public static byte[] TryConvert(byte[] data, string fileType, bool copyunconverted = false)
        {
            var log = new Action<string>((str) => { });
            var relpath = fileType;
            var dataout = TryConvert(data, fileType, log, relpath, copyunconverted, out var converted);
            if (converted) return dataout;
            if (copyunconverted) return dataout;
            return null;
        }
        public static byte[] TryConvert(byte[] data, string fileType, Action<string> log, string relpath, bool copyunconverted, out bool converted)
        {
            converted = false;
            var exmsg = " - already gen9 format";
            if (copyunconverted) exmsg += ", directly copying file.";
            var rfe = (RpfResourceFileEntry)null;
            switch (fileType)
            {
                case ".ytd":
                    var ytd = new YtdFile();
                    ytd.Load(data);
                    rfe = ytd.RpfFileEntry as RpfResourceFileEntry;
                    if (rfe?.Version == 5)
                    {
                        log($"{relpath}{exmsg}");
                        return data;
                    }
                    else
                    {
                        log($"{relpath} - converting...");
                        converted = true;
                        return ytd.Save();
                    }
                case ".ydr":
                    var ydr = new YdrFile();
                    ydr.Load(data);
                    rfe = ydr.RpfFileEntry as RpfResourceFileEntry;
                    if (rfe?.Version == 159)
                    {
                        log($"{relpath}{exmsg}");
                        return data;
                    }
                    else
                    {
                        log($"{relpath} - converting...");
                        converted = true;
                        return ydr.Save();
                    }
                case ".ydd":
                    var ydd = new YddFile();
                    ydd.Load(data);
                    rfe = ydd.RpfFileEntry as RpfResourceFileEntry;
                    if (rfe?.Version == 159)
                    {
                        log($"{relpath}{exmsg}");
                        return data;
                    }
                    else
                    {
                        log($"{relpath} - converting...");
                        converted = true;
                        return ydd.Save();
                    }
                case ".yft":
                    var yft = new YftFile();
                    yft.Load(data);
                    rfe = yft.RpfFileEntry as RpfResourceFileEntry;
                    if (rfe?.Version == 171)
                    {
                        log($"{relpath}{exmsg}");
                        return data;
                    }
                    else
                    {
                        log($"{relpath} - converting...");
                        converted = true;
                        return yft.Save();
                    }
                case ".ypt":
                    var ypt = new YptFile();
                    ypt.Load(data);
                    rfe = ypt.RpfFileEntry as RpfResourceFileEntry;
                    if (rfe?.Version == 71)
                    {
                        log($"{relpath}{exmsg}");
                        return data;
                    }
                    else
                    {
                        log($"{relpath} - converting...");
                        converted = true;
                        return ypt.Save();
                    }
                case ".rpf":
                    log($"{relpath} - Cannot convert RPF files! Extract the contents of the RPF and convert that instead.");
                    return null;
                default:
                    if (copyunconverted)
                    {
                        log($"{relpath} - conversion not required, directly copying file.");
                        return data;
                    }
                    else
                    {
                        log($"{relpath} - conversion not required, skipping.");
                        return null;
                    }
            }

        }


        private bool Question(string msg, string title, bool nullChoice)
        {
            if (QuestionFunc != null) return QuestionFunc(msg, title);
            return nullChoice;
        }
        private void Error(string msg)
        {
            if (ErrorAction != null) ErrorAction(msg);
        }
        private void Log(string msg)
        {
            if (LogAction != null) LogAction(msg);
        }
        private void Progress(float prog)
        {
            if (ProgressAction != null) ProgressAction(prog);
        }
        private void StartStop(bool start)
        {
            if (StartStopAction != null) StartStopAction(start);
        }



    }
}
