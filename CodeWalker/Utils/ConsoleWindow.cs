using CodeWalker.World;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CodeWalker.Utils
{
    public static class ConsoleWindow
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool AllocConsole();

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("kernel32.dll")]
        static extern bool FreeConsole();

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;

        private static ConsoleStream consoleStream = new ConsoleStream();

        static ConsoleWindow()
        {
            try
            {
                var logFile = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "log.log");
                //var streamWriter = File.OpenWrite(logFile);
                var streamWriter = new StreamWriter(logFile, false, Encoding.UTF8, 1024);

                consoleStream.AddStream(streamWriter);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }


            //var textWriter = new StreamWriter(multiStream);

            consoleStream.AddStream(Console.Out);

            Console.SetOut(consoleStream);
        }

        public static IntPtr GetOrCreateConsoleWindow()
        {
            var handle = GetConsoleWindow();
            if (handle == IntPtr.Zero)
            {
                AllocConsole();
                Application.ApplicationExit += (sender, args) =>
                {
                    ConsoleWindow.Close();
                };
                handle = GetConsoleWindow();
                var originalOut = new StreamWriter(Console.OpenStandardOutput());
                originalOut.AutoFlush = true;
                consoleStream.AddStream(originalOut);
            }
            return handle;
        }

        public static void Close()
        {
            consoleStream.Flush();
            consoleStream.Close();
            FreeConsole();
        }

        public static void Show()
        {
            var handle = GetOrCreateConsoleWindow();
            ShowWindow(handle, SW_SHOW);
        }

        public static void Hide()
        {
            var handle = GetOrCreateConsoleWindow();
            ShowWindow(handle, SW_HIDE);
        }
    }

    public class ConsoleStream : TextWriter
    {
        private readonly List<TextWriter> _writers;

        public ConsoleStream(params TextWriter[] streams)
        {
            _writers = streams.ToList();
        }

        public void AddStream(TextWriter textWriter)
        {
            _writers.Add(textWriter);
        }

        public override void Flush()
        {
            foreach (var stream in _writers)
            {
                stream.Flush();
            }
        }

        public override void Write(char ch)
        {
            for (int i = 0; i < _writers.Count; i++)
            {
                try
                {
                    _writers[i].Write(ch);
                }
                catch (ObjectDisposedException)
                {
                    _writers.Remove(_writers[i]);
                    // handle exception here
                }
                catch (IOException)
                {
                    // handle exception here
                }
            }
        }

        public override void Write(string value)
        {
            foreach (var writer in _writers)
            {
                writer.Write(value);
            }
        }

        public override void WriteLine()
        {
            base.WriteLine();
            Flush();
        }

        public override void WriteLine(string value)
        {
            for (int i = 0; i < _writers.Count; i++)
            {
                try
                {
                    _writers[i].WriteLine(value);
                    _writers[i].Flush();
                }
                catch (ObjectDisposedException)
                {
                    _writers.Remove(_writers[i]);
                    // handle exception here
                }
                catch (IOException)
                {
                    // handle exception here
                }
            }
        }

        public override void WriteLine(object value)
        {
            for (int i = 0; i < _writers.Count; i++)
            {
                try
                {
                    _writers[i].WriteLine(value);
                    _writers[i].Flush();
                }
                catch (ObjectDisposedException)
                {
                    _writers.Remove(_writers[i]);
                    // handle exception here
                }
                catch (IOException)
                {
                    // handle exception here
                }
            }
        }

        public override void Close()
        {
            base.Close();
            foreach(var writer in _writers)
            {
                writer.Close();
            }
            _writers.Clear();
        }

        public override Encoding Encoding => throw new NotImplementedException();
    }
}
