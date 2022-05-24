using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SocketClient
{
    public class TrackSegmentationService
    {
        private Process process = null;

        private IPEndPoint remoteEP = null;
        private Socket socket = null;

        private string storageFolder;
        private string scriptName;
        private string graphDirectory;
        private int expectedResultLenght;
        private Size expectedResultSize;
        private int connectionPort;


        public TrackSegmentationService(string mainScriptName, string graphDirectory, string storageFolder, int expectedResultLenght, Size expectedResultSize, int connectionPort)
        {
            this.storageFolder = storageFolder;
            this.scriptName = Path.Combine(storageFolder, mainScriptName);
            this.graphDirectory = Path.Combine(storageFolder, graphDirectory);
            this.expectedResultLenght = expectedResultLenght;
            this.expectedResultSize = expectedResultSize;
            this.connectionPort = connectionPort;

        }

        public void Initialize()
        {
            //_runScript();
                _startClient();
        }

        public void Dispose()
        {
            _stopScript();
            _stopClient();
        }

        public Bitmap Evaluate(Bitmap source)
        {
            //sending data 
            Bitmap resizedSource = source.Resize(expectedResultSize);
            byte[] bytes = source.ToByteArray(ImageFormat.Png); //??
            resizedSource.Dispose();

            string header = bytes.Length.ToString();
            byte[] headerMsg = Encoding.UTF8.GetBytes(header);

            socket.Send(headerMsg); 
            int bytesSent = socket.Send(bytes);

            //receiving data
            Bitmap result = null; 
            int resultLength = 0;
            int msgLen = 7; 
            byte[] backMsg = new byte[msgLen];
            List<byte> partResultBytes = new List<byte>();
            while (result == null)
            {
                int received = socket.Receive(backMsg);
                // if(backMsg.Any())
                //  {
                while(true)
                {
                    if (resultLength == 0)
                    {
                        string s = System.Text.Encoding.UTF8.GetString(backMsg, 0, msgLen);
                        resultLength = Convert.ToInt32(s);
                        backMsg = new byte[resultLength];
                        break;
                    }

                    partResultBytes.AddRange(backMsg.ToList().GetRange(0, received));
                    backMsg = new byte[resultLength];
                    if (partResultBytes.Count < resultLength)
                    {
                        break;
                    }
                    else
                    {
                        resultLength = 0;
                        result = (Bitmap)partResultBytes.ToArray().ImageFromArray(expectedResultSize.Width, expectedResultSize.Height, PixelFormat.Format24bppRgb);
                        break;
                    }
                    
                }

            }
               


            return result;
        }

        private void _runScript()
        {
            var programFilesFolder = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

            ProcessStartInfo start = new ProcessStartInfo();

            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "Python");
            start.FileName = Path.Combine(path, "Python36\\python.exe");
            // start.FileName = Path.Combine(@"C:\Users\Kasia\AppData\Local\Programs\Python\", "Python36\\python.exe");

            string cmd = scriptName; //script path 
            string arg = $" --name={graphDirectory}";
            start.Arguments = string.Format("\"{0}\"{1}", cmd, arg);
            start.UseShellExecute = false;
            start.RedirectStandardOutput = true;
            start.RedirectStandardInput = true;
            start.RedirectStandardError = true;
            start.CreateNoWindow = false;

            process = new Process();
            process.StartInfo = start;

            using (AutoResetEvent outputWaitHandle = new AutoResetEvent(false))
            using (AutoResetEvent errorWaitHandle = new AutoResetEvent(false))
            {
                process.OutputDataReceived += (sender, e) =>
                {
                    if (e.Data == null)
                    {
                        // outputWaitHandle.Set();
                    }
                    else
                    {
                        Console.WriteLine(e.Data);

                        if (e.Data.Contains("Listen"))
                        {

                            _startClient();

                            outputWaitHandle.Set();
                            errorWaitHandle.Set();

                        }


                        //if (process != null && !process.HasExited)
                        //{
                        //    process.Kill();
                        //  //  KillProcessAndChildren(process.Id);
                        //}

                    }
                };
                process.ErrorDataReceived += (sender, e) =>
                {
                    if (e.Data == null)
                    {
                        //  errorWaitHandle.Set();
                    }
                    else
                    {
                        Console.WriteLine("error: " + e.Data);
                        //Error.Add(e.Data);

                        //if (process != null && !process.HasExited)
                        //{
                        //    process.Kill();
                        //}

                    }
                };

                process.Start();

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                outputWaitHandle.WaitOne();
                errorWaitHandle.WaitOne();

            }
        }

        private void _stopScript()
        {
            process?.Close();
            process?.Dispose();
        }

        private void _startClient()
        {
            try
            {
                remoteEP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), connectionPort);
                socket = new Socket(SocketType.Stream, ProtocolType.Tcp);

                try
                {
                    socket.Connect(remoteEP);
                }
                catch (ArgumentNullException ane)
                {
                    Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
                }
                catch (SocketException se)
                {
                    Console.WriteLine("SocketException : {0}", se.ToString());
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unexpected exception : {0}", e.ToString());
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }



        private void _stopClient()
        {
            socket?.Shutdown(SocketShutdown.Both);
            socket?.Close();
        }

    }

}
