using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace CoAEditor.Models {
    public class CoaComponentsLoader : INotifyPropertyChanged {
        public string ck3RootDir;

        public event PropertyChangedEventHandler PropertyChanged;

        public CoaComponentsLoader() {
            ck3RootDir = null;
            ReadCk3DirFromFile();
        }

        public string Ck3RootDir {
            get { return ck3RootDir; }
            set {
                ck3RootDir = value;
            }
        }

        public static bool ImportCoaComponents(string inputImagesUrl, string outputImagesUrl, string inputFormat, string outputFormat) {
            try{
                if (!Directory.Exists(outputImagesUrl))
                    Directory.CreateDirectory(outputImagesUrl);
            } catch (Exception e) {
                Console.WriteLine(e.Message.ToString());
            }

            System.Diagnostics.Process importProc = new System.Diagnostics.Process();
            importProc.StartInfo.FileName = "CMD.exe";
            importProc.StartInfo.Arguments = "/C FOR %A in (\"" + inputImagesUrl + "\\*" + inputFormat + "\") DO ffmpeg.exe -i \"%A\" \"" + outputImagesUrl + "/%~nA" + outputFormat + "\"";
            importProc.StartInfo.UseShellExecute = false;
            importProc.StartInfo.CreateNoWindow = false;

            importProc.Start();
            importProc.WaitForExit();

            return importProc.ExitCode == 0;
        }

        public static string[] GetFileNames(string path, string filter) {
            //var files = new DirectoryInfo(ck3RootDir + "\\game\\gfx\\coat_of_arms\\patterns").GetFiles("*.dds");
            string[] files = Directory.GetFiles(path, filter);
            for (int i = 0; i < files.Length; i++)
                files[i] = Path.GetFileName(files[i]);
            return files;
        }

        public async Task WriteCk3DirToFile() {
            string[] lines = {
                "[CK3 ROOT DIRECTORY]",
                ck3RootDir
            };
            await File.WriteAllLinesAsync("CoA_Editor.cfg", lines);
        }

        public bool ReadCk3DirFromFile() {
            bool rVal = false;
            try {
                string[] lines = File.ReadAllLines("CoA_Editor.cfg");
                int i = 0;
                while (!lines[i].Equals("[CK3 ROOT DIRECTORY]")) {
                    i++;
                }

                if(lines[i].Equals("[CK3 ROOT DIRECTORY]") && !lines[i + 1].Equals("")) {
                    ck3RootDir = lines[i + 1];
                    rVal = true;
                }

            } catch(IOException ioe) {
                Console.WriteLine(ioe.ToString());
                rVal = false;
            }

            return rVal;
        }

        public static bool GeneratePngFromUrl(string img_url) {
            System.Diagnostics.Process ffmpegProc = new System.Diagnostics.Process();
            ffmpegProc.StartInfo.FileName = "ffmpeg.exe";
            ffmpegProc.StartInfo.Arguments = "-y -i \"" + img_url + "\" -vf scale=256:-1 temp.png";
            ffmpegProc.StartInfo.UseShellExecute = false;
            ffmpegProc.StartInfo.RedirectStandardOutput = true;
            ffmpegProc.StartInfo.RedirectStandardError = true;
            ffmpegProc.StartInfo.CreateNoWindow = true;

            ffmpegProc.Start();
            ffmpegProc.WaitForExit();

            while (!ffmpegProc.StandardOutput.EndOfStream) {
                string line = ffmpegProc.StandardOutput.ReadLine();
                System.Diagnostics.Debug.WriteLine(line);
            }

            return ffmpegProc.ExitCode == 0;
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
