using Microsoft.Azure.KeyVault.Models;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.CompilerServices;

namespace ASP.Back.Libraries
{
    public class FFMPEG
    {
        private enum FFTYPE
        {
            FFPROBE,
            FFMPEG
        }
        protected struct FFPipe
        {
            public NamedPipeServerStream Npss { get; set; }
            public string PipeName { get; set; }
            public Stream? Stream { get; set; }

        }
        public struct FFVideo
        {
            public FFVideo()
            {
                fileName = string.Empty;
                videoName = string.Empty;
                folder = string.Empty;
                extention = string.Empty;
                GUID = string.Empty;
                stream = null;
                master = new List<string>();
                resolutions = new List<string>();
                _codecs = new List<string>();

            }
            public string fileName { get; set; }
            public string videoName { get; set; }
            public string folder { get; set; }
            public string extention { get; set; }
            public List<string> resolutions { get; set; }
            private List<string> _codecs;
            public List<string> codecs
            {
                get
                {
                    return _codecs;
                }
                set
                {
                    _codecs = new List<string>();
                    foreach (var codec in value)
                    {
                        _codecs.Add(codec.Split('=')[1]);
                    }

                }
            }
            public string GUID { get; set; }
            public Stream? stream { get; set; } 
            public List<string> master {  get; set; }
        }
        private string masterPath
        {
            get
            {
                return currentDirectory + this.Video.fileName + "_master.m3u8";
            }
        }
        private FFVideo _video { get; set; }
        private string currentDirectory { get; set; }
        public FFVideo Video 
        {
            get 
            { 
                return _video;
            }
        }
        public bool success { get; set; }

        private FFPipe? CreatePipe(PipeDirection direction)
        {
            try
            {
                FFPipe pipe = new FFPipe();
                pipe.PipeName = Guid.NewGuid().ToString("N");
                pipe.Npss = new NamedPipeServerStream(pipe.PipeName, PipeDirection.InOut, 1,
                                                        PipeTransmissionMode.Byte, PipeOptions.WriteThrough);
                pipe.Stream = new System.IO.MemoryStream();
                return pipe;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        private Process? StartFFMpeg(FFTYPE type, List<string> arguments)
        {
            return new Process
            {
                StartInfo = new ProcessStartInfo
                {

                    FileName = type.ToString(),
                    Arguments = String.Join(" ", arguments.ToArray()),
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = false,
                    RedirectStandardError = true
                }
            };
        }
        private bool LoadMaster()
        {
            try
            {
                if (!File.Exists(masterPath))
                {
                    return false;
                }

                StreamReader masterFile = File.OpenText(masterPath);
                string line = string.Empty;
                if (masterFile.Peek() <= 0)
                {
                    masterFile.Dispose();
                    return false;
                }
                
                while ((line = masterFile.ReadLine()) != null)
                {
                    this.Video.master.Add(line);
                }

                masterFile.Dispose();
                return true;
            }
            catch(Exception ex)
            { 
                Console.WriteLine(ex.Message);
               
                return false;
            }
        }
        private bool SaveMaster()
        {
            try
            {
                StreamWriter masterFile = null;
                if (!File.Exists(masterPath))
                {
                    masterFile = File.CreateText(masterPath);
                }
                else
                {
                    masterFile = new StreamWriter(masterPath);
                }
                if (masterFile == null)
                {
                    return false;
                }
                
                foreach (string line in this.Video.master)
                {
                    masterFile.Write(line + '\n');
                }
                masterFile.Flush();
                masterFile.Dispose();
                masterFile.Close();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }
        //adds a line to the front or end of the master file
        public bool AppendLineMaster(string newLine,bool atFront = false)
        {

            if(this.Video.master == null || this.Video.master.Count == 0)
            {
                if (LoadMaster())
                {
                    return AppendLineMaster(newLine, atFront);
                }
                else
                {
                    return false;
                }
                    
            }
            if(atFront)
            {
                this.Video.master.Insert(0, newLine);
                
            }
            else
            {
                this.Video.master.Add(newLine);
                
            }
            return SaveMaster();

        }
        public Stream? GetWebStream()
        {
            try
            {
                List<string> output = new List<string>();
                List<string> error = new List<string>();

                currentDirectory = _video.folder;

                    // We use Guid for PipeNames
                   FFPipe? ffPipe = CreatePipe(PipeDirection.InOut);

                    if (ffPipe.Value.Npss == null)
                        return null;

                    string PipeNamesFFmpeg = $@"\\.\pipe\{ffPipe.Value.PipeName}";


                    var argumentBuilder = new List<string>();
                    argumentBuilder.Add("-loglevel fatal  -y");
                    argumentBuilder.Add("-i");
                    argumentBuilder.Add('"' + currentDirectory  + _video.fileName + _video.extention + '"');
                    argumentBuilder.Add("-bsf:a aac_adtstoasc -c copy -f mp4 -movflags frag_keyframe+empty_moov");
                    argumentBuilder.Add(PipeNamesFFmpeg);
                //argumentBuilder.Add(currentDirectory + "test.mp4");

                Task task = null;
                    using (var proc = StartFFMpeg(FFTYPE.FFMPEG, argumentBuilder))
                    {
                        Console.WriteLine($"FFMpeg path: ffmpeg");
                        Console.WriteLine($"Arguments: {proc.StartInfo.Arguments}");

                        proc.EnableRaisingEvents = false;
                        proc.Start();
                        ffPipe.Value.Npss.WaitForConnection();

                        task = ffPipe.Value.Npss.CopyToAsync(ffPipe.Value.Stream)
                             .ContinueWith(x =>
                             {
                                 ffPipe.Value.Npss.Disconnect();
                             });
                        string processOutput = string.Empty;
                        try
                        {
                            while ((processOutput = proc.StandardError.ReadLine()) != null)
                            {
                                error.Add(processOutput);
                            }
                            while ((processOutput = proc.StandardOutput.ReadLine()) != null)
                            {
                                output.Add(processOutput);
                            }
                        }
                        catch (Exception ex)
                        {
                           Console.WriteLine(ex.Message + ex.StackTrace);
                            return null;
                        }
                        proc.WaitForExit();

                    }

                if (task != null)
                {
                    Task.WaitAll(task);
                    ffPipe.Value.Stream.Position = 0;
                        task = ffPipe.Value.Stream.CopyToAsync(_video.stream)
                            .ContinueWith(x =>
                            {
                                ffPipe.Value.Stream.Dispose();
                            });
                    Task.WaitAll(task);
                }
                if (_video.stream != null && _video.stream.Length > 0)
                {
                    _video.stream.Position = 0;
                    success = true;
                }
                else
                {
                    if (error.Count > 0)
                    {
                        int actualErr = 0;
                        foreach (var err in error)
                        {
                            Console.WriteLine(err);
                            if (err.Contains("Invalid argument"))
                            {
                                continue;
                            }
                            else
                            {
                                actualErr++;
                            }

                        }
                    }
                }
                return _video.stream;

            }
            catch (Exception ex)
            {
               Console.WriteLine(ex.Message + ex.StackTrace);
                return null; 
            }
        }
        private FFVideo fillFileStrings(string fullFilePath)
        {
            FFVideo video = new FFVideo();

            int fileExtIndex = fullFilePath.LastIndexOf('.');
            int folderIndex = fullFilePath.LastIndexOf('\\');
            video.extention = fullFilePath.Substring(fileExtIndex);
            video.folder = fullFilePath.Substring(0, folderIndex + 1);
            string pathWithFileName = fullFilePath.Substring(0, fileExtIndex);

            video.fileName = pathWithFileName.Substring(folderIndex + 1);
            return video;
        }
        
        public List<string> probe(Stream inStream)
        {
            List<string> args = new List<string>();
            FFPipe? ffPipe = CreatePipe(PipeDirection.Out);
            List<string> output = new List<string>();
            if (ffPipe?.Npss == null)
                    return output;
            string PipeNamesFFmpeg = $@"\\.\pipe\{ffPipe.Value.PipeName}";
            args.Add("-loglevel fatal -show_entries stream=codec_type -of default=nw=1 " + PipeNamesFFmpeg);
            
            using (var proc = StartFFMpeg(FFTYPE.FFPROBE, args))
            {
                Console.WriteLine($"FFMpeg path: " + FFTYPE.FFPROBE.ToString());
                Console.WriteLine($"Arguments: {proc.StartInfo.Arguments}");

                proc.EnableRaisingEvents = false;
                proc.Start();
                ffPipe.Value.Npss.WaitForConnection();

                inStream.CopyToAsync(ffPipe.Value.Npss)
                      .ContinueWith(x =>
                      {
                          ffPipe.Value.Npss.WaitForPipeDrain();
                          ffPipe.Value.Npss.Disconnect();
                      });
                string processOutput = string.Empty;
                while ((processOutput = proc.StandardOutput.ReadLine()) != null)
                {
                    output.Add(processOutput);
                }

                proc.WaitForExit();
                
                ffPipe.Value.Npss?.Dispose();

            }
            
            return output;

        }

        public FFMPEG(string fileName)
        {
            try
            {
                success = true;
                FFVideo video = fillFileStrings(fileName);
                video.stream = new System.IO.MemoryStream();
                _video = video;
            }
            catch (Exception ex)
            {
               Console.WriteLine(ex.Message + ex.StackTrace);
            }
        }
        public FFMPEG(Stream inStream, string fileOut, List<string> resolutions)
        {
            try
            {
                List<string> output = new List<string>();
                List<string> error = new List<string>();
                success = false;
                long streamStartPos = inStream.Position;

                FFVideo video = fillFileStrings(fileOut);

                currentDirectory = video.folder + video.fileName;
                if (!Directory.Exists(currentDirectory))
                {
                    Directory.CreateDirectory(currentDirectory);
                }
                currentDirectory += '\\';

                inStream.Position = streamStartPos;
                video.codecs = probe(inStream);
                bool containsAudio = false;
                foreach (var codec in video.codecs)
                {
                    if (codec.Contains("audio"))
                    {
                        containsAudio = true;
                    }
                }
                inStream.Position = streamStartPos;
                FFPipe? ffPipe = CreatePipe(PipeDirection.Out);
                if (ffPipe?.Npss == null)
                    return;
                video.GUID = ffPipe.Value.PipeName;
                string PipeNamesFFmpeg = $@"\\.\pipe\{ffPipe.Value.PipeName}";
                var pipeBuilder = new List<string>();
                var argumentBuilder = new List<string>();
                var filterBuilder = new List<string>();
                var resolutionBuilder = new List<string>();
                var audioMapper = new List<string>();
                filterBuilder.Add("-filter_complex " + '"' + "[v:0]split=" + resolutions.Count);

                pipeBuilder.Add("-loglevel fatal -y -i");
                pipeBuilder.Add(PipeNamesFFmpeg);
                pipeBuilder.Add("-preset veryfast -sc_threshold 0");
                pipeBuilder.Add("-metadata TITLE=\"" + ffPipe.Value.PipeName + "\"");
                int index = 0;
                foreach (string resolution in resolutions)
                {
                    int resSplitIndex = resolution.IndexOf('x');
                    string resShortName = resolution.Substring(resSplitIndex + 1);
                    string[] resSplit = { resolution.Substring(0, resSplitIndex), resShortName };
                    argumentBuilder.Add("-map " + '[' + resShortName + "out]");
                    argumentBuilder.Add("-c:v:" + index + " libx264");
                    filterBuilder.Add('[' + resShortName + "tmp]");
                    resolutionBuilder.Add(";["
                        + resShortName + "tmp] scale=w=" + resSplit[0] + ":h=" + resSplit[1] + '[' + resShortName + "out]");
                    if (containsAudio)
                    {
                        audioMapper.Add("-map a:0 -c:a:" + index + " aac -b:a:" + index + " 128k");
                    }
                    index++;
                }
                resolutionBuilder.Add('"'.ToString());

                argumentBuilder.Add("-f hls -hls_time 4 -hls_playlist_type event");
                
                

                argumentBuilder.Add("-master_pl_name " + video.fileName + "_master.m3u8");

                argumentBuilder.Add("-var_stream_map " + '"');
                for (int i = 0; i < resolutions.Count; i++)
                {
                    argumentBuilder.Add("v:" + i + (containsAudio? ",a:" + i : ""));
                }
                argumentBuilder.Add('"'.ToString());
                
               argumentBuilder.Add("-hls_segment_filename " + currentDirectory + "stream_%v\\data%06d.ts");
                //argumentBuilder.Add("-strftime_mkdir 1 ");
                argumentBuilder.Add('"' + currentDirectory + video.fileName + "_index_%v.m3u8" + '"');
                
                List<string> completeArgs = pipeBuilder.Concat(filterBuilder.Concat(resolutionBuilder.Concat(audioMapper.Concat(argumentBuilder)))).ToList();


                using (var proc = StartFFMpeg(FFTYPE.FFMPEG, completeArgs))
                {
                    Console.WriteLine($"FFMpeg path: " +FFTYPE.FFMPEG);
                    Console.WriteLine($"Arguments: {proc.StartInfo.Arguments}");

                    proc.EnableRaisingEvents = false;
                    proc.Start();
                    ffPipe.Value.Npss.WaitForConnection();

                   inStream.CopyToAsync(ffPipe.Value.Npss)
                         .ContinueWith(x =>
                         {
                             ffPipe.Value.Npss.WaitForPipeDrain();
                             ffPipe.Value.Npss.Disconnect();
                         });
                    string processOutput = string.Empty;

                    while ((processOutput = proc.StandardError.ReadLine()) != null)
                    {
                        error.Add(processOutput);
                    }
                    while ((processOutput = proc.StandardOutput.ReadLine()) != null)
                    {
                        output.Add(processOutput);
                    }


                    proc.WaitForExit();
                    ffPipe.Value.Npss?.Dispose();
                    //taskIndex++;
                }

                //}
                //Task.WaitAll(tasks);
                if(error.Count > 0)
                {
                    int actualErr = 0;
                    foreach(var err in error)
                    {
                        Console.WriteLine(err);
                        if (err.Contains("Invalid argument"))
                        {
                            continue;
                        }
                        else
                        {
                            actualErr++;
                        }

                    }
                    if (actualErr > 0)
                    {
                        success = false;
                        Console.WriteLine("FFMPEG Failed to Convert media To HLS format");
                    }
                    else { success = true; }
                   
                }
                else
                {
                    success = true;
                }
                foreach (var err in output)
                {
                    Console.WriteLine(err);
                }
                _video = video;
                return;
                
            }
            catch (Exception ex)
            {
               Console.WriteLine(ex.Message + ex.StackTrace);
            }

        }
    }
}
