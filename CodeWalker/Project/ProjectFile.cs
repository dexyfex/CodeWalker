using CodeWalker.GameFiles;
using CodeWalker.World;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CodeWalker.Project
{
    public class ProjectFile
    {
        //fields stored in file
        public string Name { get; set; } //friendly name for this project
        public int Version { get; set; }
        public HashSet<string> YmapFilenames { get; set; } = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
        public HashSet<string> YtypFilenames { get; set; } = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
        public HashSet<string> YbnFilenames { get; set; } = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
        public HashSet<string> YndFilenames { get; set; } = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
        public HashSet<string> YnvFilenames { get; set; } = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
        public HashSet<string> TrainsFilenames { get; set; } = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
        public HashSet<string> ScenarioFilenames { get; set; } = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
        public HashSet<string> AudioRelFilenames { get; set; } = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
        public HashSet<string> YdrFilenames { get; set; } = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
        public HashSet<string> YddFilenames { get; set; } = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
        public HashSet<string> YftFilenames { get; set; } = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
        public HashSet<string> YtdFilenames { get; set; } = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

        //fields not stored
        public string Filename { get; set; } //filename without path
        public string Filepath { get; set; } //full path of the current file
        public bool HasChanged { get; set; } //flag for use by the UI

        public List<YmapFile> YmapFiles { get; } = new List<YmapFile>();
        public List<YtypFile> YtypFiles { get; } = new List<YtypFile>();
        public List<YbnFile> YbnFiles { get; } = new List<YbnFile>();
        public List<YndFile> YndFiles { get; } = new List<YndFile>();
        public List<YnvFile> YnvFiles { get; } = new List<YnvFile>();
        public List<TrainTrack> TrainsFiles { get; } = new List<TrainTrack>();
        public List<YmtFile> ScenarioFiles { get; } = new List<YmtFile>();
        public List<RelFile> AudioRelFiles { get; } = new List<RelFile>();
        public List<YdrFile> YdrFiles { get; } = new List<YdrFile>();
        public List<YddFile> YddFiles { get; } = new List<YddFile>();
        public List<YftFile> YftFiles { get; } = new List<YftFile>();
        public List<YtdFile> YtdFiles { get; } = new List<YtdFile>();



        public void Save()
        {
            XmlDocument doc = new XmlDocument();
            var projelem = doc.CreateElement("CodeWalkerProject");
            doc.AppendChild(projelem);

            Xml.AddChildWithInnerText(doc, projelem, "Name", Name);
            Xml.AddChildWithAttribute(doc, projelem, "Version", "value", Version.ToString());

            var ymapselem = Xml.AddChild(doc, projelem, "YmapFilenames");
            foreach (string ymapfilename in YmapFilenames)
            {
                Xml.AddChildWithInnerText(doc, ymapselem, "Item", ymapfilename);
            }

            var ytypselem = Xml.AddChild(doc, projelem, "YtypFilenames");
            foreach (string ytypfilename in YtypFilenames)
            {
                Xml.AddChildWithInnerText(doc, ytypselem, "Item", ytypfilename);
            }

            var ybnselem = Xml.AddChild(doc, projelem, "YbnFilenames");
            foreach (string ybnfilename in YbnFilenames)
            {
                Xml.AddChildWithInnerText(doc, ybnselem, "Item", ybnfilename);
            }

            var yndselem = Xml.AddChild(doc, projelem, "YndFilenames");
            foreach (string yndfilename in YndFilenames)
            {
                Xml.AddChildWithInnerText(doc, yndselem, "Item", yndfilename);
            }

            var ynvselem = Xml.AddChild(doc, projelem, "YnvFilenames");
            foreach (string ynvfilename in YnvFilenames)
            {
                Xml.AddChildWithInnerText(doc, ynvselem, "Item", ynvfilename);
            }

            var trainselem = Xml.AddChild(doc, projelem, "TrainsFilenames");
            foreach (string trainsfile in TrainsFilenames)
            {
                Xml.AddChildWithInnerText(doc, trainselem, "Item", trainsfile);
            }

            var scenarioselem = Xml.AddChild(doc, projelem, "ScenarioFilenames");
            foreach (string scenariofilename in ScenarioFilenames)
            {
                Xml.AddChildWithInnerText(doc, scenarioselem, "Item", scenariofilename);
            }

            var audiorelselem = Xml.AddChild(doc, projelem, "AudioRelFilenames");
            foreach (string audiorelfilename in AudioRelFilenames)
            {
                Xml.AddChildWithInnerText(doc, audiorelselem, "Item", audiorelfilename);
            }

            var ydrselem = Xml.AddChild(doc, projelem, "YdrFilenames");
            foreach (string ydrfilename in YdrFilenames)
            {
                Xml.AddChildWithInnerText(doc, ydrselem, "Item", ydrfilename);
            }

            var yddselem = Xml.AddChild(doc, projelem, "YddFilenames");
            foreach (string yddfilename in YddFilenames)
            {
                Xml.AddChildWithInnerText(doc, yddselem, "Item", yddfilename);
            }

            var yftselem = Xml.AddChild(doc, projelem, "YftFilenames");
            foreach (string yftfilename in YftFilenames)
            {
                Xml.AddChildWithInnerText(doc, yftselem, "Item", yftfilename);
            }

            var ytdselem = Xml.AddChild(doc, projelem, "YtdFilenames");
            foreach (string ytdfilename in YtdFilenames)
            {
                Xml.AddChildWithInnerText(doc, ytdselem, "Item", ytdfilename);
            }

            doc.Save(Filepath);
        }

        public void Load(string filepath)
        {
            FileInfo fi = new FileInfo(filepath);
            Filename = fi.Name;
            Filepath = filepath;

            XmlDocument doc = new XmlDocument();
            doc.Load(filepath);

            var projelem = doc.DocumentElement;

            Name = Xml.GetChildInnerText(projelem, "Name");
            Version = Xml.GetChildIntAttribute(projelem, "Version", "value");

            YmapFilenames.Clear();
            YmapFiles.Clear();
            var ymapselem = Xml.GetChild(projelem, "YmapFilenames");
            var ymapItems = ymapselem?.SelectNodes("Item");
            if (ymapItems is not null)
            {
                foreach (var node in ymapItems)
                {
                    if (node is XmlElement ymapel)
                    {
                        AddYmapFile(ymapel.InnerText);
                    }
                }
            }


            YtypFilenames.Clear();
            YtypFiles.Clear();
            var ytypItems = Xml.GetChild(projelem, "YtypFilenames")
                ?.SelectNodes("Item");
            if (ytypItems is not null)
            {
                foreach (var node in ytypItems)
                {
                    if (node is XmlElement ytypel)
                    {
                        AddYtypFile(ytypel.InnerText);
                    }
                }
            }


            YbnFilenames.Clear();
            YbnFiles.Clear();

            var ybnItems = Xml.GetChild(projelem, "YbnFilenames")
                ?.SelectNodes("Item");

            if (ybnItems is not null)
            {
                foreach (var node in ybnItems)
                {
                    if (node is XmlElement ybnel)
                    {
                        AddYbnFile(ybnel.InnerText);
                    }
                }
            }


            YndFilenames.Clear();
            YndFiles.Clear();
            var yndItems = Xml.GetChild(projelem, "YndFilenames")
                ?.SelectNodes("Item");

            if (yndItems is not null)
            {
                foreach (var node in yndItems)
                {
                    if (node is XmlElement yndel)
                    {
                        AddYndFile(yndel.InnerText);
                    }
                }
            }



            YnvFilenames.Clear();
            YnvFiles.Clear();
            var ynvItems = Xml.GetChild(projelem, "YnvFilenames")
                ?.SelectNodes("Item");
            if (ynvItems is not null)
            {
                foreach (var node in ynvItems)
                {
                    if (node is XmlElement ynvel)
                    {
                        AddYnvFile(ynvel.InnerText);
                    }
                }
            }


            TrainsFilenames.Clear();
            TrainsFiles.Clear();
            var trainItems = Xml.GetChild(projelem, "TrainsFilenames")
                ?.SelectNodes("Item");
            if (trainItems is not null)
            {
                foreach (var node in trainItems)
                {
                    if (node is XmlElement trainel)
                    {
                        AddTrainsFile(trainel.InnerText);
                    }
                }
            }



            ScenarioFilenames.Clear();
            ScenarioFiles.Clear();
            var scenarioItems = Xml.GetChild(projelem, "ScenarioFilenames")
                ?.SelectNodes("Item");
            if (scenarioItems is not null)
            {
                foreach (var node in scenarioItems)
                {
                    if (node is XmlElement scenarioel)
                    {
                        AddScenarioFile(scenarioel.InnerText);
                    }
                }
            }



            AudioRelFilenames.Clear();
            AudioRelFiles.Clear();
            var audiorelItems = Xml.GetChild(projelem, "AudioRelFilenames")
                ?.SelectNodes("Item");
            if (audiorelItems is not null)
            {
                foreach (var node in audiorelItems)
                {
                    if (node is XmlElement audiorelel)
                    {
                        AddAudioRelFile(audiorelel.InnerText);
                    }
                }
            }


            YdrFilenames.Clear();
            YdrFiles.Clear();
            var ydrItems = Xml.GetChild(projelem, "YdrFilenames")
                ?.SelectNodes("Item");
            if (ydrItems is not null)
            {
                foreach (var node in ydrItems)
                {
                    if (node is XmlElement ydrel)
                    {
                        AddYdrFile(ydrel.InnerText);
                    }
                }
            }


            YddFilenames.Clear();
            YddFiles.Clear();
            var yddItems = Xml.GetChild(projelem, "YddFilenames")
                ?.SelectNodes("Item");
            if (yddItems is not null)
            {
                foreach (var node in yddItems)
                {
                    if (node is XmlElement yddel)
                    {
                        AddYddFile(yddel.InnerText);
                    }
                }
            }


            YftFilenames.Clear();
            YftFiles.Clear();
            var yftItems = Xml.GetChild(projelem, "YftFilenames")
                ?.SelectNodes("Item");
            if (yftItems is not null)
            {
                foreach (var node in yftItems)
                {
                    if (node is XmlElement yftel)
                    {
                        AddYftFile(yftel.InnerText);
                    }
                }
            }


            YtdFilenames.Clear();
            YtdFiles.Clear();
            var ytdItems = Xml.GetChild(projelem, "YtdFilenames")
                ?.SelectNodes("Item");
            if (ytdItems is not null)
            {
                foreach (var node in ytdItems)
                {
                    if (node is XmlElement ytdel)
                    {
                        AddYtdFile(ytdel.InnerText);
                    }
                }
            }
        }


        public void UpdateFilenames(string oldprojpath)
        {
            YmapFilenames = YmapFilenames
                .Select(p => GetUpdatedFilePath(p, oldprojpath))
                .ToHashSet();

            YtypFilenames = YtypFilenames
                .Select(p => GetUpdatedFilePath(p, oldprojpath))
                .ToHashSet();

            YbnFilenames = YbnFilenames
                .Select(p => GetUpdatedFilePath(p, oldprojpath))
                .ToHashSet();

            YndFilenames = YndFilenames
                .Select(P => GetUpdatedFilePath(P, oldprojpath))
                .ToHashSet();

            YnvFilenames = YnvFilenames
                .Select(P => GetUpdatedFilePath(P, oldprojpath))
                .ToHashSet();

            TrainsFilenames = TrainsFilenames
                .Select(P => GetUpdatedFilePath(P, oldprojpath))
                .ToHashSet();

            ScenarioFilenames = ScenarioFilenames
                .Select(P => GetUpdatedFilePath(P, oldprojpath))
                .ToHashSet();

            AudioRelFilenames = AudioRelFilenames
                .Select(P => GetUpdatedFilePath(P, oldprojpath))
                .ToHashSet();

            YdrFilenames = YdrFilenames
                .Select(p => GetUpdatedFilePath(p, oldprojpath))
                .ToHashSet();

            YddFilenames = YddFilenames
                .Select(p => GetUpdatedFilePath(p, oldprojpath))
                .ToHashSet();

            YftFilenames = YftFilenames
                .Select(p => GetUpdatedFilePath(p, oldprojpath))
                .ToHashSet();

            YtdFilenames = YtdFilenames
                .Select(p => GetUpdatedFilePath(p, oldprojpath))
                .ToHashSet();
        }

        public string GetUpdatedFilePath(string oldpath, string oldprojpath)
        {
            string fullpath = GetFullFilePath(oldpath, oldprojpath);
            return GetRelativePath(fullpath);
        }
        public string GetRelativePath(string filepath)
        {
            if (filepath == null)
                return string.Empty;
            if (Filepath == null)
                return filepath;

            Uri fromUri;
            if (!Uri.TryCreate(Filepath, UriKind.RelativeOrAbsolute, out fromUri))
            {
                return filepath;
            }

            Uri toUri;
            if (!Uri.TryCreate(filepath, UriKind.RelativeOrAbsolute, out toUri))
            {
                return filepath;
            }
            if (!toUri.IsAbsoluteUri)
            {
                return filepath;//already relative...
            }

            //Uri fromUri = new Uri(Filepath);
            //Uri toUri = new Uri(filepath);
            if (fromUri.Scheme != toUri.Scheme)
            {
                return filepath.ToLowerInvariant();
            }
            Uri relativeUri = fromUri.MakeRelativeUri(toUri);
            string relativePath = Uri.UnescapeDataString(relativeUri.ToString());
            if (string.Equals(toUri.Scheme, Uri.UriSchemeFile, StringComparison.OrdinalIgnoreCase))
            {
                relativePath = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            }
            return relativePath.ToLowerInvariant();
        }
        public string GetFullFilePath(string relpath)
        {
            if (string.IsNullOrEmpty(Filepath))
                return relpath;
            string projfldr = new FileInfo(Filepath).DirectoryName + "\\";
            string cpath = Path.Combine(projfldr, relpath);
            return Path.GetFullPath(cpath);
        }
        public string GetFullFilePath(string relpath, string basepath)
        {
            if (string.IsNullOrEmpty(basepath))
                return relpath;

            string basefldr = new FileInfo(basepath).DirectoryName + "\\";
            string cpath = Path.Combine(basefldr, relpath);
            return Path.GetFullPath(cpath);
        }



        public YmapFile? AddYmapFile(string filename)
        {
            YmapFile ymap = new YmapFile();
            ymap.RpfFileEntry = new RpfResourceFileEntry();
            ymap.RpfFileEntry.Name = Path.GetFileName(filename);
            ymap.FilePath = GetFullFilePath(filename);
            ymap.Name = ymap.RpfFileEntry.Name;
            JenkIndex.Ensure(ymap.Name);
            JenkIndex.Ensure(Path.GetFileNameWithoutExtension(ymap.Name));
            JenkIndex.Ensure(filename);
            if (!AddYmapFile(ymap))
                return null;
            return ymap;
        }
        public bool AddYmapFile(YmapFile ymap)
        {
            string relpath = GetRelativePath(ymap.FilePath);
            if (string.IsNullOrEmpty(relpath))
                relpath = ymap.Name;
            lock(YmapFiles)
            {
                if (YmapFilenames.Contains(relpath))
                    return false;
                YmapFilenames.Add(relpath);
                YmapFiles.Add(ymap);
            }

            return true;
        }
        public void RemoveYmapFile(YmapFile ymap)
        {
            if (ymap == null)
                return;
            var relpath = GetRelativePath(ymap.FilePath);
            if (string.IsNullOrEmpty(relpath))
                relpath = ymap.Name;
            lock(YmapFiles)
            {
                YmapFiles.Remove(ymap);
                YmapFilenames.Remove(relpath);
            }

            HasChanged = true;
        }

        public bool ContainsYmap(string filename)
        {
            lock(YmapFiles)
            {
                return YmapFilenames.Contains(filename);
            }
        }

        public bool ContainsYmap(YmapFile ymap)
        {
            lock(YmapFiles)
            {
                return YmapFiles.Contains(ymap);
            }
        }

        public bool RenameYmap(string oldfilename, string newfilename)
        {
            lock(YmapFiles)
            {
                if (YmapFilenames.Remove(oldfilename))
                {
                    YmapFilenames.Add(newfilename.ToLowerInvariant());
                    HasChanged = true;
                    return true;
                }

                return false;
            }
        }


        public YtypFile? AddYtypFile(string filename)
        {
            YtypFile ytyp = new YtypFile();
            ytyp.RpfFileEntry = new RpfResourceFileEntry();
            ytyp.RpfFileEntry.Name = Path.GetFileName(filename);
            ytyp.FilePath = GetFullFilePath(filename);
            ytyp.Name = ytyp.RpfFileEntry.Name;
            JenkIndex.EnsureBoth(ytyp.Name);
            JenkIndex.EnsureBoth(Path.GetFileNameWithoutExtension(ytyp.Name));
            JenkIndex.EnsureBoth(filename);
            if (!AddYtypFile(ytyp))
                return null;
            return ytyp;
        }
        public bool AddYtypFile(YtypFile ytyp)
        {
            string relpath = GetRelativePath(ytyp.FilePath);
            if (string.IsNullOrEmpty(relpath))
                relpath = ytyp.Name;
            lock(YtypFiles)
            {
                if (YtypFilenames.Contains(relpath))
                    return false;

                YtypFilenames.Add(relpath);
                YtypFiles.Add(ytyp);
            }

            return true;
        }
        public void RemoveYtypFile(YtypFile ytyp)
        {
            if (ytyp == null) return;
            var relpath = GetRelativePath(ytyp.FilePath);
            if (string.IsNullOrEmpty(relpath))
                relpath = ytyp.Name;
            lock(YtypFiles)
            {
                YtypFiles.Remove(ytyp);
                YtypFilenames.Remove(relpath);
            }
            HasChanged = true;
        }

        public bool ContainsYtyp(string filename)
        {
            lock(YtypFiles)
            {
                return YtypFilenames.Contains(filename);
            }
        }

        public bool ContainsYtyp(YtypFile ytyp)
        {
            lock (YtypFiles)
            {
                return YtypFiles.Contains(ytyp);
            }
        }

        public bool RenameYtyp(string oldfilename, string newfilename)
        {
            lock(YtypFiles)
            {
                if (YtypFilenames.Remove(oldfilename))
                {
                    YtypFilenames.Add(newfilename);
                    HasChanged = true;
                    return true;
                }

                return false;
            }
        }


        public YbnFile? AddYbnFile(string filename)
        {
            YbnFile ybn = new YbnFile();
            ybn.RpfFileEntry = new RpfResourceFileEntry();
            ybn.RpfFileEntry.Name = Path.GetFileName(filename);
            ybn.FilePath = GetFullFilePath(filename);
            ybn.Name = ybn.RpfFileEntry.Name;
            if (!AddYbnFile(ybn))
                return null;
            return ybn;
        }

        public bool AddYbnFile(YbnFile ybn)
        {
            string relpath = GetRelativePath(ybn.FilePath);
            if (string.IsNullOrEmpty(relpath)) relpath = ybn.Name;
            lock(YbnFiles)
            {
                if (YndFilenames.Contains(relpath))
                    return false;
                YbnFilenames.Add(relpath);
                YbnFiles.Add(ybn);
                return true;
            }
        }

        public void RemoveYbnFile(YbnFile ybn)
        {
            if (ybn == null) return;
            var relpath = GetRelativePath(ybn.FilePath);
            if (string.IsNullOrEmpty(relpath))
                relpath = ybn.Name;

            lock(YbnFiles)
            {
                YbnFiles.Remove(ybn);
                YbnFilenames.Remove(relpath);
                HasChanged = true;
            }
        }

        public bool ContainsYbn(string filename)
        {
            lock(YbnFiles)
            {
                return YbnFilenames.Contains(filename);
            }
        }

        public bool ContainsYbn(YbnFile ybn)
        {
            lock(YbnFiles)
            {
                return YbnFiles.Contains(ybn);
            }
        }

        public bool RenameYbn(string oldfilename, string newfilename)
        {
            lock(YbnFiles)
            {
                if (YbnFilenames.Remove(oldfilename))
                {
                    YbnFilenames.Add(newfilename.ToLowerInvariant());
                    HasChanged = true;
                    return true;
                }

                return false;
            }
        }


        public YndFile? AddYndFile(string filename)
        {
            YndFile ynd = new YndFile();
            ynd.RpfFileEntry = new RpfResourceFileEntry();
            ynd.RpfFileEntry.Name = Path.GetFileName(filename);
            ynd.FilePath = GetFullFilePath(filename);
            ynd.Name = ynd.RpfFileEntry.Name;
            if (!AddYndFile(ynd))
                return null;
            return ynd;
        }
        public bool AddYndFile(YndFile ynd)
        {
            string relpath = GetRelativePath(ynd.FilePath);
            if (string.IsNullOrEmpty(relpath))
                relpath = ynd.Name;

            lock(YndFiles)
            {
                if (YndFilenames.Contains(relpath))
                    return false;
                YndFilenames.Add(relpath);
                YndFiles.Add(ynd);
            }
            return true;
        }
        public void RemoveYndFile(YndFile? ynd)
        {
            if (ynd is null)
                return;
            var relpath = GetRelativePath(ynd.FilePath);
            if (string.IsNullOrEmpty(relpath))
                relpath = ynd.Name;

            lock(YndFiles)
            {
                YndFiles.Remove(ynd);
                YndFilenames.Remove(relpath);
            }

            HasChanged = true;
        }
        public bool ContainsYnd(string filename)
        {
            lock(YndFiles)
            {
                return YndFilenames.Contains(filename);
            }
        }
        public bool ContainsYnd(YndFile ynd)
        {
            lock(YndFiles)
            {
                return YndFiles.Contains(ynd);
            }
        }
        public bool RenameYnd(string oldfilename, string newfilename)
        {
            lock(YndFiles)
            {
                if (YndFilenames.Remove(oldfilename))
                {
                    YndFilenames.Add(newfilename);
                    HasChanged = true;
                    return true;
                }
            }

            return false;
        }


        public YnvFile AddYnvFile(string filename)
        {
            YnvFile ynv = new YnvFile();
            ynv.RpfFileEntry = new RpfResourceFileEntry();
            ynv.RpfFileEntry.Name = Path.GetFileName(filename);
            ynv.FilePath = GetFullFilePath(filename);
            ynv.Name = ynv.RpfFileEntry.Name;
            if (!AddYnvFile(ynv))
                return null;
            return ynv;
        }

        public bool AddYnvFile(YnvFile ynv)
        {
            string relpath = GetRelativePath(ynv.FilePath);
            if (string.IsNullOrEmpty(relpath))
                relpath = ynv.Name;

            lock(YnvFiles)
            {
                if (YnvFilenames.Contains(relpath))
                    return false;

                YnvFilenames.Add(relpath);
                YnvFiles.Add(ynv);
            }

            return true;
        }

        public void RemoveYnvFile(YnvFile ynv)
        {
            if (ynv == null) return;
            var relpath = GetRelativePath(ynv.FilePath);
            if (string.IsNullOrEmpty(relpath))
                relpath = ynv.Name;

            lock(YnvFiles)
            {
                YnvFiles.Remove(ynv);
                YnvFilenames.Remove(relpath);
            }

            HasChanged = true;
        }

        public bool ContainsYnv(string filename)
        {
            lock(YnvFiles)
            {
                return YnvFilenames.Contains(filename);
            }
        }

        public bool ContainsYnv(YnvFile ynv)
        {
            lock(YnvFiles)
            {
                return YnvFiles.Contains(ynv);
            }
        }

        public bool RenameYnv(string oldfilename, string newfilename)
        {
            lock(YnvFiles)
            {
                if (YnvFilenames.Remove(oldfilename))
                {
                    YnvFilenames.Add(newfilename);
                    HasChanged = true;
                    return true;
                }
            }


            return false;
        }


        public TrainTrack? AddTrainsFile(string filename)
        {
            TrainTrack track = new TrainTrack();
            track.RpfFileEntry = new RpfResourceFileEntry();
            track.RpfFileEntry.Name = Path.GetFileName(filename);
            track.FilePath = GetFullFilePath(filename);
            track.Name = track.RpfFileEntry.Name;
            if (!AddTrainsFile(track))
                return null;
            return track;
        }
        public bool AddTrainsFile(TrainTrack track)
        {
            string relpath = GetRelativePath(track.FilePath);
            if (string.IsNullOrEmpty(relpath))
                relpath = track.Name;
            if (TrainsFilenames.Contains(relpath))
                return false;
            TrainsFilenames.Add(relpath);
            TrainsFiles.Add(track);
            return true;
        }
        public void RemoveTrainsFile(TrainTrack? track)
        {
            if (track is null)
                return;
            var relpath = GetRelativePath(track.FilePath);
            if (string.IsNullOrEmpty(relpath))
                relpath = track.Name;
            TrainsFiles.Remove(track);
            TrainsFilenames.Remove(relpath);
            HasChanged = true;
        }
        public bool ContainsTrainTrack(string filename)
        {
            return TrainsFilenames.Contains(filename);
        }

        public bool ContainsTrainTrack(TrainTrack track)
        {
            return TrainsFiles.Contains(track);
        }

        public bool RenameTrainTrack(string oldfilename, string newfilename)
        {
            if (TrainsFilenames.Remove(oldfilename))
            {
                TrainsFilenames.Add(newfilename);
                HasChanged = true;
                return true;
            }

            return false;
        }


        public YmtFile? AddScenarioFile(string filename)
        {
            YmtFile scenario = new YmtFile();
            scenario.RpfFileEntry = new RpfResourceFileEntry();
            scenario.RpfFileEntry.Name = Path.GetFileName(filename);
            scenario.FilePath = GetFullFilePath(filename);
            scenario.Name = scenario.RpfFileEntry.Name;
            scenario.ContentType = YmtFileContentType.ScenarioPointRegion;
            scenario.FileFormat = YmtFileFormat.RSC;
            if (!AddScenarioFile(scenario))
                return null;
            return scenario;
        }
        public bool AddScenarioFile(YmtFile ymt)
        {
            string relpath = GetRelativePath(ymt.FilePath);
            if (string.IsNullOrEmpty(relpath))
                relpath = ymt.Name;

            lock(ScenarioFiles)
            {
                if (ScenarioFilenames.Contains(relpath))
                    return false;
                ScenarioFilenames.Add(relpath);
                ScenarioFiles.Add(ymt);
            }

            return true;
        }
        public void RemoveScenarioFile(YmtFile? ymt)
        {
            if (ymt is null)
                return;
            var relpath = GetRelativePath(ymt.FilePath);
            if (string.IsNullOrEmpty(relpath))
                relpath = ymt.Name;

            lock(ScenarioFiles)
            {
                ScenarioFiles.Remove(ymt);
                ScenarioFilenames.Remove(relpath);
            }

            HasChanged = true;
        }
        public bool ContainsScenario(string filename)
        {
            lock(ScenarioFiles)
            {
                return ScenarioFilenames.Contains(filename);
            }
        }

        public bool ContainsScenario(YmtFile ymt)
        {
            lock(ScenarioFiles)
            {
                return ScenarioFiles.Contains(ymt);
            }
        }

        public bool RenameScenario(string oldfilename, string newfilename)
        {
            lock(ScenarioFiles)
            {
                if (ScenarioFilenames.Remove(oldfilename))
                {
                    ScenarioFilenames.Add(newfilename);
                    HasChanged = true;
                    return true;
                }
            }

            return false;
        }


        public RelFile? AddAudioRelFile(string filename)
        {
            RelFile relfile = new RelFile();
            relfile.RpfFileEntry = new RpfResourceFileEntry();
            relfile.RpfFileEntry.Name = Path.GetFileName(filename);
            relfile.FilePath = GetFullFilePath(filename);
            relfile.Name = relfile.RpfFileEntry.Name;
            if (!AddAudioRelFile(relfile))
                return null;
            return relfile;
        }

        public bool AddAudioRelFile(RelFile rel)
        {
            string relpath = GetRelativePath(rel.FilePath);
            if (string.IsNullOrEmpty(relpath))
                relpath = rel.Name;

            lock(AudioRelFiles)
            {
                if (AudioRelFilenames.Contains(relpath))
                    return false;
                AudioRelFilenames.Add(relpath);
                AudioRelFiles.Add(rel);
            }

            return true;
        }

        public void RemoveAudioRelFile(RelFile? rel)
        {
            if (rel is null)
                return;
            var relpath = GetRelativePath(rel.FilePath);
            if (string.IsNullOrEmpty(relpath))
                relpath = rel.Name;

            lock(AudioRelFiles)
            {
                AudioRelFiles.Remove(rel);
                AudioRelFilenames.Remove(relpath);
            }

            HasChanged = true;
        }

        public bool ContainsAudioRel(string filename)
        {
            lock(AudioRelFiles)
            {
                return AudioRelFilenames.Contains(filename);
            }
        }

        public bool ContainsAudioRel(RelFile rel)
        {
            lock(AudioRelFiles)
            {
                return AudioRelFiles.Contains(rel);
            }
        }

        public bool RenameAudioRel(string oldfilename, string newfilename)
        {
            lock(AudioRelFiles)
            {
                if (AudioRelFilenames.Remove(oldfilename))
                {
                    AudioRelFilenames.Add(newfilename);
                    HasChanged = true;
                    return true;
                }
                return false;
            }
        }


        public YdrFile? AddYdrFile(string filename)
        {
            YdrFile ydr = new YdrFile();
            ydr.RpfFileEntry = new RpfResourceFileEntry();
            ydr.RpfFileEntry.Name = Path.GetFileName(filename);
            ydr.FilePath = GetFullFilePath(filename);
            ydr.Name = ydr.RpfFileEntry.Name;

            if (!AddYdrFile(ydr))
                return null;

            return ydr;
        }
        public bool AddYdrFile(YdrFile ydr)
        {
            string relpath = GetRelativePath(ydr.FilePath);

            if (string.IsNullOrEmpty(relpath))
                relpath = ydr.Name;

            lock(YdrFiles)
            {
                if (YdrFilenames.Contains(relpath))
                    return false;

                YdrFilenames.Add(relpath);
                YdrFiles.Add(ydr);
                return true;
            }
        }

        public void RemoveYdrFile(YdrFile ydr)
        {
            if (ydr == null)
                return;
            var relpath = GetRelativePath(ydr.FilePath);
            if (string.IsNullOrEmpty(relpath))
                relpath = ydr.Name;

            lock(YdrFiles)
            {
                if (YdrFiles.Remove(ydr) || YdrFilenames.Remove(relpath))
                {
                    HasChanged = true;
                }
            }
        }

        public bool ContainsYdr(string filename)
        {
            lock(YdrFiles)
            {
                return YdrFilenames.Contains(filename);
            }
        }

        public bool ContainsYdr(YdrFile ydr)
        {
            lock (YdrFiles)
            {
                return YdrFiles.Contains(ydr);
            }
        }

        public bool RenameYdr(string oldfilename, string newfilename)
        {
            lock(YdrFiles)
            {
                if (YdrFilenames.Remove(oldfilename))
                {
                    YdrFilenames.Add(newfilename);
                    HasChanged = true;
                    return true;
                }
            }

            return false;
        }


        public YddFile AddYddFile(string filename)
        {
            YddFile ydd = new YddFile();
            ydd.RpfFileEntry = new RpfResourceFileEntry();
            ydd.RpfFileEntry.Name = Path.GetFileName(filename);
            ydd.FilePath = GetFullFilePath(filename);
            ydd.Name = ydd.RpfFileEntry.Name;
            if (!AddYddFile(ydd)) return null;
            return ydd;
        }

        public bool AddYddFile(YddFile ydd)
        {
            string relpath = GetRelativePath(ydd.FilePath);
            if (string.IsNullOrEmpty(relpath))
                relpath = ydd.Name;

            lock(YddFiles)
            {
                if (YddFilenames.Contains(relpath))
                    return false;
                YddFilenames.Add(relpath);
                YddFiles.Add(ydd);
            }

            return true;
        }

        public void RemoveYddFile(YddFile? ydd)
        {
            if (ydd is null)
                return;
            var relpath = GetRelativePath(ydd.FilePath);
            if (string.IsNullOrEmpty(relpath))
                relpath = ydd.Name;
            lock(YddFiles)
            {
                YddFiles.Remove(ydd);
                YddFilenames.Remove(relpath);
            }
            HasChanged = true;
        }

        public bool ContainsYdd(string filename)
        {
            lock(YddFiles)
            {
                return YddFilenames.Contains(filename);
            }
        }

        public bool ContainsYdd(YddFile ydd)
        {
            lock(YddFiles)
            {
                return YddFiles.Contains(ydd);
            }
        }

        public bool RenameYdd(string oldfilename, string newfilename)
        {
            newfilename = newfilename.ToLowerInvariant();
            lock(YddFiles)
            {
                if (YddFilenames.Remove(oldfilename))
                {
                    YddFilenames.Add(newfilename);
                    HasChanged = true;
                    return true;
                }
            }

            return false;
        }


        public YftFile? AddYftFile(string filename)
        {
            YftFile yft = new YftFile();
            yft.RpfFileEntry = new RpfResourceFileEntry();
            yft.RpfFileEntry.Name = Path.GetFileName(filename);
            yft.FilePath = GetFullFilePath(filename);
            yft.Name = yft.RpfFileEntry.Name;
            if (!AddYftFile(yft))
                return null;
            return yft;
        }
        public bool AddYftFile(YftFile yft)
        {
            string relpath = GetRelativePath(yft.FilePath);
            if (string.IsNullOrEmpty(relpath))
                relpath = yft.Name;

            lock(YftFiles)
            {
                if (YftFilenames.Contains(relpath))
                    return false;
                YftFilenames.Add(relpath);
                YftFiles.Add(yft);
                return true;
            }
        }
        public void RemoveYftFile(YftFile yft)
        {
            if (yft == null)
                return;
            var relpath = GetRelativePath(yft.FilePath);
            if (string.IsNullOrEmpty(relpath)) relpath = yft.Name;

            lock(YftFiles)
            {
                YftFiles.Remove(yft);
                YftFilenames.Remove(relpath);
                HasChanged = true;
            }
        }

        public bool ContainsYft(string filename)
        {
            lock(YftFiles)
            {
                return YftFilenames.Contains(filename);
            }
        }

        public bool ContainsYft(YftFile yft)
        {
            lock(YftFiles)
            {
                return YftFiles.Contains(yft);
            }
        }

        public bool RenameYft(string oldfilename, string newfilename)
        {
            lock(YftFiles)
            {
                if (YftFilenames.Remove(oldfilename))
                {
                    YftFilenames.Add(newfilename);
                    HasChanged = true;
                    return true;
                }
                return false;
            }
        }


        public YtdFile? AddYtdFile(string filename)
        {
            YtdFile ytd = new YtdFile();
            ytd.RpfFileEntry = new RpfResourceFileEntry();
            ytd.RpfFileEntry.Name = Path.GetFileName(filename);
            ytd.FilePath = GetFullFilePath(filename);
            ytd.Name = ytd.RpfFileEntry.Name;
            if (!AddYtdFile(ytd))
                return null;
            return ytd;
        }

        public bool AddYtdFile(YtdFile ytd)
        {
            string relpath = GetRelativePath(ytd.FilePath);

            if (string.IsNullOrEmpty(relpath))
                relpath = ytd.Name;

            lock(YtdFiles)
            {
                if (YtdFilenames.Contains(relpath))
                    return false;

                YtdFilenames.Add(relpath);
                YtdFiles.Add(ytd);
                return true;
            }
        }

        public void RemoveYtdFile(YtdFile ytd)
        {
            if (ytd == null)
                return;
            var relpath = GetRelativePath(ytd.FilePath);
            if (string.IsNullOrEmpty(relpath))
                relpath = ytd.Name;

            lock(YtdFiles)
            {
                YtdFiles.Remove(ytd);
                YtdFilenames.Remove(relpath);
                HasChanged = true;
            }
        }

        public bool ContainsYtd(string filename)
        {
            lock(YtdFiles)
            {
                return YtdFilenames.Contains(filename);
            }
        }

        public bool ContainsYtd(YtdFile ytd)
        {
            lock(YtdFiles)
            {
                return YtdFiles.Contains(ytd);
            }
        }

        public bool RenameYtd(string oldfilename, string newfilename)
        {
            lock(YtdFiles)
            {
                if (YtdFilenames.Remove(oldfilename))
                {
                    YtdFilenames.Add(newfilename);
                    HasChanged = true;
                    return true;
                }

                return false;
            }
        }

    }
}
