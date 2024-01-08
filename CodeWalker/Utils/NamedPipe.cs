using CodeWalker.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CodeWalker.Core.Utils
{
    public class NamedPipe
    {
        private readonly Form form;

        public NamedPipe(Form form)
        {
            this.form = form;
        }

        public void Init()
        {
            try
            {
                using var client = new NamedPipeClientStream("codewalker");
                client.Connect(0);
                client.Dispose();
                return;
            }
            catch (Exception ex)
            {
                if (ex is not TimeoutException)
                {
                    throw;
                }
                else
                {
                    StartServer();
                }
            }
        }

        public async void StartServer()
        {
            try
            {
                await Task.Run(async () =>
                {
                    var server = new NamedPipeServerStream("codewalker", PipeDirection.InOut, NamedPipeServerStream.MaxAllowedServerInstances);

                    var buffer = new byte[1024];

                    while (true)
                    {
                        try
                        {
                            await Task.Delay(50);
                            await server.WaitForConnectionAsync();
                            var read = await server.ReadAsync(buffer.AsMemory());
                            if (read > 0)
                            {
                                var message = Encoding.UTF8.GetString(buffer, 0, read);

                                Console.WriteLine(message);

                                var args = message.Split(' ');

                                switch (args[0])
                                {
                                    case "explorer":
                                        form.BeginInvoke(() =>
                                        {
                                            var f = new ExploreForm();
                                            f.Load += (sender, args) =>
                                            {
                                                f.WindowState = FormWindowState.Normal;
                                                f.Activate();
                                            };

                                            f.Show();
                                        });
                                        break;
                                    case "peds-mode":
                                        form.BeginInvoke(() =>
                                        {
                                            var f = new PedsForm();
                                            f.Load += (sender, args) =>
                                            {
                                                f.WindowState = FormWindowState.Normal;
                                                f.Activate();
                                            };

                                            f.Show();
                                        });
                                        break;
                                    case "open-file":
                                        try
                                        {
                                            var form = OpenAnyFile.OpenFilePath(string.Join(" ", args.AsSpan(1).ToArray()));
                                            form.Show();
                                            form.Activate();
                                        }
                                        catch (NotImplementedException ex)
                                        {
                                            Console.WriteLine(ex);
                                            MessageBox.Show("This file type is not yet supported!", ex.ToString());
                                        }
                                        break;
                                }
                            }

                            server.Disconnect();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                            if (ex is not IOException)
                            {
                                throw;
                            }
                            server.Disconnect();
                        }

                    }
                });
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public async void SendMessage(string message)
        {
            var client = new NamedPipeClientStream("codewalker");
        }

        public static bool TrySendMessageToOtherProcess(string message)
        {
            try
            {
                using var client = new NamedPipeClientStream("codewalker");
                client.Connect(0);

                var _buffer = Encoding.UTF8.GetBytes(message);

                client.Write(_buffer, 0, _buffer.Length);
                client.WaitForPipeDrain();

                return true;
            }
            catch (Exception ex)
            {
                if (ex is TimeoutException)
                {
                    return false;
                }

                throw;
            }

        }
    }
}
