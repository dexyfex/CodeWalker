using CodeWalker.Forms;
using CodeWalker.GameFiles;
using CodeWalker.Properties;
using CodeWalker;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using System.IO;
using System.Windows.Forms;

namespace CodeWalker.Utils
{
    internal class OpenAnyFile
    {
        public static Form OpenFilePath(string path)
        {
            var extension = Path.GetExtension(path);
            if (extension == ".ydr" || extension == ".ydd" || extension == ".yft" || extension == ".ybn" || extension == ".ypt" || extension == ".ynv")
            {
                var modelForm = new ModelForm();
                modelForm.Load += new EventHandler(async (sender, eventArgs) =>
                {
                    modelForm.ViewModel(path);
                    modelForm.Activate();
                });

                return modelForm;
                
            }
            else if (extension == ".ytd")
            {
                var textureForm = new YtdForm();
                var data = File.ReadAllBytes(path);
                var e = RpfFile.CreateFileEntry(Path.GetFileName(path), path, ref data);
                var ytdFile = RpfFile.GetFile<YtdFile>(e, data);
                textureForm.Load += (sender, eventArgs) =>
                {
                    textureForm.LoadYtd(ytdFile);
                    textureForm.Activate();
                };
                return textureForm;
            }
            else if (extension == ".ymf" || extension == ".ymap" || extension == ".ytyp" || extension == ".ymt")
            {
                GTA5Keys.LoadFromPath(GTAFolder.CurrentGTAFolder, Settings.Default.Key);
                var fileCache = GameFileCacheFactory.GetInstance();
                if (!fileCache.IsInited)
                {
                    fileCache.Init();
                }

                var metaForm = new MetaForm();
                var data = File.ReadAllBytes(path);
                var e = RpfFile.CreateFileEntry(Path.GetFileName(path), path, ref data);
                PackedFile packedFile = null;
                if (extension == ".ymt") packedFile = RpfFile.GetFile<YmfFile>(e, data);
                if (extension == ".ymap") packedFile = RpfFile.GetFile<YmapFile>(e, data);
                if (extension == ".ytyp") packedFile = RpfFile.GetFile<YtypFile>(e, data);
                if (extension == ".ymf") packedFile = RpfFile.GetFile<YmfFile>(e, data);

                metaForm.Load += (sender, eventArgs) =>
                {
                    metaForm.Activate();
                    metaForm.LoadMeta(packedFile);
                };
                return metaForm;
            }

            throw new NotImplementedException();
        }
    }
}
