using ASP.Back.Controllers;
using Microsoft.CodeAnalysis;
using System.Diagnostics;
using System.IO.Pipes;
using TeamManiacs.Core.Models;

namespace ASP.Back.Libraries
{
    public class FFMPEG
    {
        public struct Video
        {

            public string fileName { get; set; }
            public string folder { get; set; }
            public string extention { get; set; }
            public List<string> resolutions { get; set; }

        }
        public FFMPEG(Stream inStream, string fileOut, string[] resolutions)
        {
            try
            {
                Video video = new Video();
                long streamStartPos = inStream.Position;

                int fileExtIndex = fileOut.LastIndexOf('.');
                int folderIndex = fileOut.LastIndexOf('\\');
                video.extention = fileOut.Substring(fileExtIndex);
                video.folder = fileOut.Substring(0,folderIndex+1);               
                string pathWithFileName = fileOut.Substring(0, fileExtIndex);
                video.fileName = pathWithFileName.Substring(folderIndex + 1);

                Task[] tasks = new Task[resolutions.Length];
                int taskIndex = 0;
                foreach (string resolution in resolutions)
                {
                    
                    inStream.Position = streamStartPos;
                    // We use Guid for pipeNames
                    var pipeName = Guid.NewGuid().ToString("N");
                    var npss = new NamedPipeServerStream(pipeName, PipeDirection.Out, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);                
                    string pipeNamesFFmpeg = $@"-i \\.\pipe\{pipeName}";

                    int resSplitIndex = resolution.IndexOf(':');


                    var argumentBuilder = new List<string>();

                    argumentBuilder.Add("-y");
                    argumentBuilder.Add(pipeNamesFFmpeg);
                    argumentBuilder.Add("-c:v libx264 -crf 20 -s");
                    argumentBuilder.Add(resolution);
                    argumentBuilder.Add('"' + pathWithFileName + '_' + resolution.Substring(resSplitIndex+1) + video.extention + '"');

                    using (var proc = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "ffmpeg",
                            Arguments = String.Join(" ", argumentBuilder.ToArray()),
                            UseShellExecute = false,
                        }
                    })
                    {
                        Console.WriteLine($"FFMpeg path: ffmpeg");
                        Console.WriteLine($"Arguments: {proc.StartInfo.Arguments}");

                        proc.EnableRaisingEvents = false;
                        proc.Start();
                        npss.WaitForConnection();

                        tasks[taskIndex] = inStream.CopyToAsync(npss)
                             // .ContinueWith(_ => pipe.FlushAsync()) // Flush does nothing on Pipes
                             .ContinueWith(x =>
                             {
                                 npss.WaitForPipeDrain();
                                 npss.Disconnect();
                             });

                        proc.WaitForExit();
                        npss?.Dispose();
                        taskIndex++;
                    }

                }
                Task.WaitAll(tasks);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

        }
    }
}
