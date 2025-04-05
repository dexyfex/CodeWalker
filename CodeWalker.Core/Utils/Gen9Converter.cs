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
                        var datain = File.ReadAllBytes(path);
                        var dataout = (byte[])null;
                        var rfe = (RpfResourceFileEntry)null;
                        var ext = Path.GetExtension(pathl);
                        switch (ext)
                        {
                            case ".ytd":
                                var ytd = new YtdFile();
                                ytd.Load(datain);
                                rfe = ytd.RpfFileEntry as RpfResourceFileEntry;
                                if (rfe?.Version == 5)
                                {
                                    Log($"{relpath} - already gen9 format, directly copying file.");
                                    dataout = datain;
                                }
                                else
                                {
                                    Log($"{relpath} - converting...");
                                    dataout = ytd.Save();
                                }
                                break;
                            case ".ydr":
                                var ydr = new YdrFile();
                                ydr.Load(datain);
                                rfe = ydr.RpfFileEntry as RpfResourceFileEntry;
                                if (rfe?.Version == 159)
                                {
                                    Log($"{relpath} - already gen9 format, directly copying file.");
                                    dataout = datain;
                                }
                                else
                                {
                                    Log($"{relpath} - converting...");
                                    dataout = ydr.Save();
                                }
                                break;
                            case ".ydd":
                                var ydd = new YddFile();
                                ydd.Load(datain);
                                rfe = ydd.RpfFileEntry as RpfResourceFileEntry;
                                if (rfe?.Version == 159)
                                {
                                    Log($"{relpath} - already gen9 format, directly copying file.");
                                    dataout = datain;
                                }
                                else
                                {
                                    Log($"{relpath} - converting...");
                                    dataout = ydd.Save();
                                }
                                break;
                            case ".yft":
                                var yft = new YftFile();
                                yft.Load(datain);
                                rfe = yft.RpfFileEntry as RpfResourceFileEntry;
                                if (rfe?.Version == 171)
                                {
                                    Log($"{relpath} - already gen9 format, directly copying file.");
                                    dataout = datain;
                                }
                                else
                                {
                                    Log($"{relpath} - converting...");
                                    dataout = yft.Save();
                                }
                                break;
                            case ".ypt":
                                var ypt = new YptFile();
                                ypt.Load(datain);
                                rfe = ypt.RpfFileEntry as RpfResourceFileEntry;
                                if (rfe?.Version == 71)
                                {
                                    Log($"{relpath} - already gen9 format, directly copying file.");
                                    dataout = datain;
                                }
                                else
                                {
                                    Log($"{relpath} - converting...");
                                    dataout = ypt.Save();
                                }
                                break;
                            case ".rpf":
                                Log($"{relpath} - Cannot convert RPF files! Extract the contents of the RPF and convert that instead.");
                                break;
                            default:
                                if (copyunconverted)
                                {
                                    Log($"{relpath} - conversion not required, directly copying file.");
                                    dataout = datain;
                                }
                                else
                                {
                                    Log($"{relpath} - conversion not required, skipping.");
                                }
                                break;
                        }
                        if (dataout != null)
                        {
                            File.WriteAllBytes(outpath, dataout);
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
