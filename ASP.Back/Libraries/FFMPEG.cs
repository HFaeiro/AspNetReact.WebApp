using System.Collections.Specialized;
using System.Diagnostics;
using System.IO.Pipes;
using System.Runtime.InteropServices;



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
            public string PipePath { get; set; }

        }
        private IProgress<(int, int)> progress;
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
                _frames = 0;
                _desiredFps = 23.94f;
                _duration = 0f;
            }
            public string fileName { get; set; }
            public string videoName { get; set; }
            public string folder { get; set; }
            public string extention { get; set; }
            public float _desiredFps { get; set; }
            public float _duration { get; set; }
            public int _frames { get; set; }
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
                        if (codec == null)
                            continue;
                        if (codec.Contains("nb_frames") && _frames == 0)
                        {
                            int totalFrames = 0;
                            bool frames = int.TryParse(codec.Split('=')[1], out totalFrames);
                            if (frames)
                            {
                                if (this._duration > 0)
                                {
                                    this._frames = (int)(this._duration * this._desiredFps);
                                }
                                else
                                {
                                    this._frames = totalFrames;
                                }

                            }
                        }
                        else if (codec.Contains("nb_read_frames") && _frames == 0)
                        {
                            int totalFrames = 0;
                            bool frames = int.TryParse(codec.Split('=')[1], out totalFrames);
                            if (frames)
                            {
                                if (this._duration > 0)
                                {
                                    this._frames = (int)(this._duration * this._desiredFps);
                                }
                                else
                                {
                                    this._frames = totalFrames;
                                }

                            }
                        }
                        else if (codec.Contains("duration"))
                        {
                            float duration = 0;
                            bool tryDuration = float.TryParse(codec.Split('=')[1], out duration);
                            if (tryDuration)
                            {
                                this._duration = duration;
                                if(this._frames > 0)
                                {
                                    this._frames = (int)(duration * this._desiredFps);
                                }
                            }
                        }else if (codec.Contains("pts_time"))
                        {
                            float duration = 0;
                            bool tryDuration = float.TryParse(codec.Split('=')[1], out duration);
                            if (tryDuration)
                            {
                                if (duration > this._duration)
                                {
                                    this._duration = duration;
                                    if (this._frames > 0)
                                    {
                                        this._frames = (int)(duration * this._desiredFps);
                                    }
                                }
                            }
                        }
                        else
                        {
                            var codecType = codec.Split('=');
                            if (codecType.Length == 2)
                            {
                                _codecs.Add(codecType[1]);
                            }
                        }
                    }

                }
            }
            public string GUID { get; set; }
            public Stream? stream { get; set; }
            public List<string> master { get; set; }
        }
        private string masterPath
        {
            get
            {
                return currentDirectory + this._video.GUID + "_master.m3u8";
            }
        }
        public FFVideo _video;
        private string currentDirectory { get; set; }
       
        public bool success { get; set; }

        private FFPipe? CreatePipe(PipeDirection direction, string workingDirectory = "", bool createDirectory = false)
        {
            try
            {



                FFPipe pipe = new FFPipe();
                pipe.PipeName = Guid.NewGuid().ToString("N");
                workingDirectory = Path.Combine(workingDirectory , pipe.PipeName);

                if (createDirectory)
                {                    
                    if (!Directory.Exists(workingDirectory))
                    {
                        Console.WriteLine($"\t\t{nameof(CreatePipe)} - workingDirectory:{Path.Combine(workingDirectory)} Doesn't Exist! Creating it. ");
                        Directory.CreateDirectory(workingDirectory);
                    }

                }

                if (RuntimeInformation.RuntimeIdentifier.StartsWith("win"))
                {
                    pipe.Npss = new NamedPipeServerStream(pipe.PipeName, direction, 1,
                                                           PipeTransmissionMode.Byte, PipeOptions.WriteThrough);
                    pipe.Stream = new System.IO.MemoryStream();

                }
                else
                {
                    //ProcessStartInfo startInfo = new ProcessStartInfo("mkfifo");
                    //startInfo.Arguments = pipe.PipeName;
                    //Process.Start(startInfo);
                    if (workingDirectory != "")
                    {
                        string pipeDirectory = Path.Combine("pipes");
                        string pipePath = Path.Combine(workingDirectory, pipeDirectory);
                        if (!Directory.Exists(pipePath))
                        {
                            Console.WriteLine($"\t\t{nameof(CreatePipe)} - pipeDirectory:{pipePath} Doesn't Exist! Creating it. ");
                            Directory.CreateDirectory(pipePath);
                        }

                        pipe.PipePath = Path.Combine(pipePath, pipe.PipeName);
                        pipe.Stream = File.Open(pipe.PipePath, FileMode.Create);

                    }
                    else
                    {
                        pipe.PipePath = Path.Combine("pipes", pipe.PipeName);
                        pipe.Stream = File.Open(pipe.PipePath, FileMode.Create);
                    }
                }

                return pipe;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + "\n\n" + ex.StackTrace + "\n\n");
                return null;
            }
        }


        private Process? StartFFMpeg(FFTYPE type, List<string> arguments, string workingDirectory = "", bool redirectStandardOutput = true,
                   bool redirectStandardError = true)
        {
            if(workingDirectory == "")
            {
                workingDirectory = Directory.GetCurrentDirectory();
            }
            return new Process
            {
                StartInfo = new ProcessStartInfo
                {

                    FileName = type.ToString().ToLower(),
                    Arguments = String.Join(" ", arguments.ToArray()),
                    UseShellExecute = false,
                    RedirectStandardOutput = redirectStandardOutput,
                    RedirectStandardError = redirectStandardError,
                    WorkingDirectory = workingDirectory
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
                if (masterFile == null)
                {
                    return false;
                }
                if (masterFile.Peek() <= 0)
                {
                    masterFile.Dispose();
                    return false;
                }

                while ((line = masterFile.ReadLine()) != null)
                {
                    this._video.master.Add(line);
                }

                masterFile.Dispose();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + "\n\n" + ex.StackTrace + "\n\n");

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

                foreach (string line in this._video.master)
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
                Console.WriteLine(ex.Message + "\n\n" + ex.StackTrace + "\n\n");
                return false;
            }
        }
        //adds a line to the front or end of the master file
        public bool AppendLineMaster(string newLine, bool atFront = false)
        {

            if (this._video.master == null || this._video.master.Count == 0)
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
            if (atFront)
            {
                this._video.master.Insert(0, newLine);

            }
            else
            {
                this._video.master.Add(newLine);

            }
            return SaveMaster();

        }

        public Stream? GetWebStream()
        {
            try
            {
                bool isWindows = RuntimeInformation.RuntimeIdentifier.StartsWith("win");
                List<string> output = new List<string>();
                List<string> error = new List<string>();

                currentDirectory = _video.folder;

                // We use Guid for PipeNames
                FFPipe? ffPipe = CreatePipe(PipeDirection.InOut);
                if (ffPipe == null)
                {
                    return null;
                }
                if ((ffPipe?.Npss == null && isWindows) || ffPipe?.Stream == null)
                {
                    Console.WriteLine($"\t\t{nameof(GetWebStream)} - Named Pipe Returned Null! ");
                    return null;
                }

                string PipeNamesFFmpeg;
                if (isWindows)
                {
                    PipeNamesFFmpeg = $@"\\.\pipe\{ffPipe.Value.PipeName}";
                }
                else
                {
                    PipeNamesFFmpeg = $@"{ffPipe.Value.PipeName}";
                }

                var argumentBuilder = new List<string>();
                argumentBuilder.Add("-loglevel debug  -y");
                argumentBuilder.Add("-i");
                argumentBuilder.Add('"' + currentDirectory + _video.fileName + _video.extention + '"');
                argumentBuilder.Add($"-bsf:a aac -c copy -f {_video.extention} - movflags frag_keyframe+empty_moov");
                argumentBuilder.Add(PipeNamesFFmpeg);
                //argumentBuilder.Add(currentDirectory + "test.mp4");

                Task task = null;
                using (var proc = StartFFMpeg(FFTYPE.FFMPEG,argumentBuilder,"",true,false))
                {
                   // Console.WriteLine($"FFMpeg path: " + FFTYPE.FFMPEG);
                    Console.WriteLine($"Arguments: {proc.StartInfo.Arguments}");

                    proc.EnableRaisingEvents = false;
                    proc.Start();

                    string processOutput = string.Empty;
                    if (isWindows)
                    {
                        //Console.WriteLine($"\t\t{nameof(GetWebStream)} - Writing Windows Stream! - ");
                        ffPipe.Value.Npss.WaitForConnection();

                        task = ffPipe.Value.Npss.CopyToAsync(ffPipe.Value.Stream)
                             .ContinueWith(x =>
                             {
                                 ffPipe.Value.Npss.Disconnect();
                             });
                       // Console.WriteLine($"\t\t{nameof(GetWebStream)} - Wrote Windows Stream! - Wrote: {ffPipe.Value.Stream.Length} Bytes");
                    }

                    try
                    {
                        //while ((processOutput = proc.StandardError.ReadLine()) != null)
                        //{
                        //    error.Add(processOutput);
                        //}
                        while ((processOutput = proc.StandardOutput.ReadLine()) != null)
                        {
                            output.Add(processOutput);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message + "\n\n" + ex.StackTrace + "\n\n");
                        return null;
                    }
                    if (!proc.WaitForExit(9999))
                    {
                        Console.WriteLine($"\t\t{nameof(GetWebStream)} - Proc Timed Out!!! - inStream.Position : {ffPipe.Value.Stream.Position} - Wrote: {ffPipe.Value.Stream.Length} Bytes");
                    }
                    else
                    {
                        //Console.WriteLine($"\t\t{nameof(GetWebStream)} - Proc Finished Successfully!!! - inStream.Position : {ffPipe.Value.Stream.Position} - Wrote: {ffPipe.Value.Stream.Length} Bytes");
                    }

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
                else if (ffPipe.Value.Stream.Length > 0)
                {
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
                Console.WriteLine(ex.Message + "\n\n" + ex.StackTrace + "\n\n");
                return null;
            }
        }

        private FFVideo fillFileStrings(string fullFilePath)
        {
            FFVideo video = new FFVideo();

            int fileExtIndex = fullFilePath.LastIndexOf('.');
            int folderIndex = fullFilePath.LastIndexOf(Path.DirectorySeparatorChar);
            video.extention = fullFilePath.Substring(fileExtIndex);
            video.folder = fullFilePath.Substring(0, folderIndex + 1);
            string pathWithFileName = fullFilePath.Substring(0, fileExtIndex);

            video.fileName = pathWithFileName.Substring(folderIndex + 1);
            return video;
        }

        public List<string> probeForCodecsAndFrames(Stream inStream)
        {
            Stopwatch sw = Stopwatch.StartNew();
            bool isWindows = RuntimeInformation.RuntimeIdentifier.StartsWith("win");
            try
            {
               // Console.WriteLine($"\t\t{nameof(probeForCodecs)} - inStream length: {inStream.Length} - inStream.Position : {inStream.Position}");
                List<string> args = new List<string>();
                FFPipe? ffPipe = CreatePipe(PipeDirection.Out);
                List<string> output = new List<string>();
                if ((ffPipe?.Npss == null && isWindows) || ffPipe?.Stream == null)
                {
                    sw.Stop();
                    //Console.WriteLine($"\t\t{nameof(probeForCodecs)} - Named Pipe Returned Null! , Exiting after {sw.Elapsed.ToString("mm\\:ss\\.ff")}");

                    return output;
                }

                //string 
                string PipeNamesFFmpeg;

                if (isWindows)
                {
                    PipeNamesFFmpeg = $@"\\.\pipe\{ffPipe.Value.PipeName}";
                }
                else
                {

                    PipeNamesFFmpeg = $@"{ffPipe.Value.PipePath + ".pipe"}";
                }
                string ffProbeInstructionsPath = Path.Combine(Directory.GetCurrentDirectory(), "ffProbeCmds.txt");
                if (File.Exists(ffProbeInstructionsPath))
                {
                    string ffProbeArgs = File.ReadAllText(ffProbeInstructionsPath);
                    args.Add($"{ffProbeArgs} " + PipeNamesFFmpeg);
                }
                else
                {
                    args.Add("-loglevel fatal -count_frames -show_entries format_tags -of default=nw=1 " + PipeNamesFFmpeg);
                }
                StringCollection values = new StringCollection();                
                StringCollection genOutput = new StringCollection();
                using (var proc = StartFFMpeg(FFTYPE.FFPROBE, args))
                {
                   // Console.WriteLine($"FFMpeg path: " + FFTYPE.FFPROBE.ToString().ToLower());
                    //Console.WriteLine($"Arguments: {proc.StartInfo.Arguments}");

                    proc.EnableRaisingEvents = false;
                    proc.Start();
                    if (isWindows)
                    {
                        //Console.WriteLine($"\t\t{nameof(probeForCodecs)} - Writing Windows Stream! - ");
                        ffPipe.Value.Npss.WaitForConnection();
                        inStream.CopyToAsync(ffPipe.Value.Npss)
                          .ContinueWith(x =>
                          {
                             
                              ffPipe.Value.Npss.Disconnect();
                              ffPipe.Value.Npss.WaitForPipeDrain();
                              
                          });
                        //Console.WriteLine($"\t\t{nameof(probeForCodecs)} - Wrote Windows Stream! - inStream.Position : {inStream.Position} - ");

                    }
                    else
                    {

                       // Console.WriteLine($"\t\t{nameof(probeForCodecs)} - Writing Linux Stream! - ");
                        inStream.CopyTo(ffPipe.Value.Stream);

                       // Console.WriteLine($"\t\t{nameof(probeForCodecs)} - Wrote Linux Stream! - inStream.Position : {inStream.Position} - Wrote: {ffPipe.Value.Stream.Length} Bytes");

                    }
                    proc.OutputDataReceived += (s, e) =>
                    {
                        lock (genOutput)
                        {
                            genOutput.Add(e.Data);
                        }
                    };
                    proc.ErrorDataReceived += (s, e) =>
                    {
                        lock (values)
                        {
                            values.Add("! >" + e.Data);
                        }
                    };

                    proc.BeginErrorReadLine();
                    proc.BeginOutputReadLine();


                    proc.WaitForExit();

                    lock (values)
                    {
                        foreach (string sline in values)
                        {
                            if (sline != null)
                            {
                                Console.WriteLine(sline);
                                output.Add(sline);
                            }
                        }
                    }

                    ffPipe.Value.Npss?.Dispose();

                }
                sw.Stop();
                Console.WriteLine($"\t\t{nameof(probeForCodecsAndFrames)} - Exiting after {sw.Elapsed.ToString("mm\\:ss\\.ff")}");
                
                return genOutput.Cast<string>().ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + "\n\n" + ex.StackTrace + "\n\n");
                return null;
            }
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
                Console.WriteLine(ex.Message + "\n\n" + ex.StackTrace + "\n\n");
            }
        }
        public bool hasAudio(Stream inStream, FFVideo? video = null)
        {
            List<string> codecs = new List<string>();
            long streamStartPos = inStream.Position;
            if ((_video.codecs == null || _video.codecs.Count == 0) && video == null)
            {
               codecs = probeForCodecsAndFrames(inStream);
            }
            else
            {
                codecs = video?.codecs;
            }

            foreach (var codec in codecs)
            {
                if (codec.Contains("audio"))
                {
                    inStream.Position = streamStartPos;
                    return true;
                }
               
            }
            inStream.Position = streamStartPos;
            return false;
        }
        public bool BuildHLS(Stream inStream, string fileOut, List<string> resolutions, bool recursed = false)
        {
            if(inStream == null)
            {
                return false;
            }
            Stopwatch sw = Stopwatch.StartNew();
            try
            {
                success = false;
                bool isWindows = RuntimeInformation.RuntimeIdentifier.StartsWith("win");
                if(inStream.Position == inStream.Length)
                {
                    inStream.Position = 0;
                }
                long streamStartPos = inStream.Position;
                inStream.Flush();
                this._video = fillFileStrings(fileOut);
                _video.codecs = probeForCodecsAndFrames(inStream);
                inStream.Position = streamStartPos;
                bool containsAudio = hasAudio(inStream, _video);

                inStream.Position = streamStartPos;



                currentDirectory = _video.folder;
                FFPipe? ffPipe = CreatePipe(PipeDirection.InOut, currentDirectory, true);
                if ((ffPipe?.Npss == null && isWindows) || ffPipe?.Stream == null)
                {
                    Console.WriteLine($"\t\t{nameof(BuildHLS)} - Named Pipe Returned Null! ");
                    return false;
                }
                
                this._video.GUID = ffPipe.Value.PipeName;
                currentDirectory += _video.GUID;
                currentDirectory += Path.DirectorySeparatorChar;

                string PipeNamesFFmpeg;
                if (isWindows)
                {
                    PipeNamesFFmpeg = $@"\\.\pipe\{ffPipe.Value.PipeName}";
                }
                else
                {
                    PipeNamesFFmpeg = $@"{ffPipe.Value.PipePath}";
                    Console.WriteLine($"\t\t{nameof(BuildHLS)} - Named Pipe Path! {PipeNamesFFmpeg} ");
                }
                var pipeBuilder = new List<string>();
                var argumentBuilder = new List<string>();
                var filterBuilder = new List<string>();
                var resolutionBuilder = new List<string>();
                var audioMapper = new List<string>();

                filterBuilder.Add("-filter_complex " + '"' + "[v:0]split=" + resolutions.Count);

                pipeBuilder.Add("-y -f " + _video.extention.Split('.')[1] + " -i");
                pipeBuilder.Add(PipeNamesFFmpeg);
                pipeBuilder.Add($"-pix_fmt yuv420p -vcodec libx264 -r {this._video._desiredFps} -crf 30 -b:v 3625k -threads 0 -sc_threshold 0");
               // pipeBuilder.Add("-preset fast -sc_threshold 0");
               // pipeBuilder.Add("-strict -2 -preset:v veryfast -profile:v baseline -level 3.0");
                pipeBuilder.Add("-preset medium -profile:v high -tune film -g 48 -x264opts no-scenecut");

                int index = 0;
                foreach (string resolution in resolutions)
                {
                    int resSplitIndex = resolution.IndexOf('x');
                    string resShortName = resolution.Substring(resSplitIndex + 1);
                    string[] resSplit = { resolution.Substring(0, resSplitIndex), resShortName };
                    argumentBuilder.Add("-map " + '[' + resShortName + "out]");
                    //argumentBuilder.Add("-c:v:" + index + " libx264");
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

                argumentBuilder.Add("-f hls -hls_time 1 -hls_segment_type fmp4 -hls_playlist_type vod");

                argumentBuilder.Add("-master_pl_name " + _video.GUID + "_master.m3u8");

                argumentBuilder.Add("-var_stream_map " + '"');
                for (int i = 0; i < resolutions.Count; i++)
                {
                    argumentBuilder.Add("v:" + i + (containsAudio ? ",a:" + i : ""));
                }
                argumentBuilder.Add('"'.ToString());

                argumentBuilder.Add("-hls_segment_filename " + Path.Combine("stream_%v", "data%06d.m4s"));

                argumentBuilder.Add('"' + _video.GUID + "_index_%v.m3u8" + '"');

                List<string> completeArgs = pipeBuilder.Concat(filterBuilder.Concat(resolutionBuilder.Concat(audioMapper.Concat(argumentBuilder)))).ToList();

                StringCollection values = new StringCollection();
                StringCollection genOutput = new StringCollection();
                using (var proc = StartFFMpeg(FFTYPE.FFMPEG, completeArgs, currentDirectory))
                {
                    //Console.WriteLine($"FFMpeg path: " + FFTYPE.FFMPEG);
                    Console.WriteLine($"Arguments: {proc.StartInfo.Arguments}");

                    proc.EnableRaisingEvents = false;
                    proc.Start();

                    if (isWindows)
                    {

                        IAsyncResult connectionResult = ffPipe.Value.Npss.BeginWaitForConnection(c =>
                        {
                           // Console.WriteLine($"\t\t{nameof(BuildHLS)} - Writing Windows Stream! - ");
                            inStream.CopyToAsync(ffPipe.Value.Npss)
                             .ContinueWith(x =>
                             {
                                 ffPipe.Value.Npss.WaitForPipeDrain();
                                 ffPipe.Value.Npss.Disconnect();
                             });
                        }, ffPipe.Value.Npss);
                        if (!connectionResult.AsyncWaitHandle.WaitOne(999999, false))
                        {
                            Console.WriteLine("..Operation Timeout...");
                        }
                       // Console.WriteLine($"\t\t{nameof(BuildHLS)} - Wrote Windows Stream! - Wrote: {inStream.Position} Bytes");
                    }
                    else
                    {

                        //Console.WriteLine($"\t\t{nameof(BuildHLS)} - Writing Linux Stream! - ");
                        inStream.CopyTo(ffPipe.Value.Stream);

                        //Console.WriteLine($"\t\t{nameof(BuildHLS)} - Wrote Linux Stream! - inStream.Position : {inStream.Position} - Wrote: {ffPipe.Value.Stream.Length} Bytes");

                    }

                    proc.OutputDataReceived += (s, e) =>
                    {
                        lock (genOutput)
                        {
                            genOutput.Add(e.Data);
                            Console.Out.WriteLine($"\t\t{nameof(BuildHLS)}" +
                                $".{FFTYPE.FFMPEG.ToString()} -     {e.Data}!-");
                        }
                    };
                    proc.ErrorDataReceived += (s, e) =>
                    {
                        if (e.Data != null)
                        {
                            lock (values)
                            {

                                values.Add("! >" + e.Data);

                            }
                            (int, int) Eta = getProgressFromString(e.Data);

                            if (this.progress != null)
                            {
                                if (Eta.Item1 + Eta.Item2 > 0)
                                {
                                    this.progress.Report(Eta);
                                }
                            }
                            else
                            {
                                Console.WriteLine(Eta);
                            }

                        }
                    };

                    proc.BeginErrorReadLine();
                    proc.BeginOutputReadLine();

                    if (!proc.WaitForExit(999999))
                    {
                        Console.WriteLine($"\t\t{nameof(BuildHLS)} - Proc Timed Out!!! - - Wrote: {ffPipe.Value.Stream.Length} Bytes");
                    }

                    ffPipe.Value.Npss?.Dispose();
                    ffPipe?.Stream?.Dispose();
                    //taskIndex++;
                }
                int actualErr = 0;
                lock (values)
                {
                    
                    foreach (string sline in values)
                    {
                        if (sline != null)
                        {
                            //Console.WriteLine(sline);
                            if (sline.StartsWith("! >Conversion failed!") || sline.Contains("Cannot determine format"))
                            {
                                Console.WriteLine(sline);
                                actualErr++;
                            }
                        }
                    }
                }
                if (actualErr > 0)
                {
                    //if (Directory.Exists(currentDirectory))
                    //{
                    //    Directory.Delete(currentDirectory,true);
                    //}
                    Console.WriteLine($"\t\t{nameof(BuildHLS)} - Directory.Delete({currentDirectory},true);");
                    success = false;
                    //Console.WriteLine($"\t\t{nameof(BuildHLS)} - FFMPEG Failed to Convert media To HLS format - Time Elapsed {sw.Elapsed.ToString("mm\\:ss\\.ff")}");
                    if (recursed)
                    {
                        Console.WriteLine($"\t\t{nameof(BuildHLS)} - We've already attempted this twice. lets not push things futher.");

                        return false;
                    }
                    else
                    {
                        Stream? recoveryStream = MoveFlags(inStream);
                        if (recoveryStream?.Length > 0)
                        {
                            Console.WriteLine($"\t\t{nameof(BuildHLS)} - \n\nFFMPEG Recovered and Moved the File FLags to the start! Attempting to Re-encode");
                            int fileExtIndex = fileOut.LastIndexOf('.');
                            if (fileExtIndex != -1)
                            {
                                fileOut = fileOut[..fileExtIndex] + ".flv";
                            }
                            recoveryStream.Position = 0;
                            recoveryStream.Flush();
                            return BuildHLS(recoveryStream, fileOut, resolutions, true);
                        }
                    }
                }
                else
                {
                    success = true;
                    Console.WriteLine("");
                    sw.Stop();
                    Console.WriteLine($"\t\t{nameof(BuildHLS)} - FFMPEG Successfully Converted media To HLS format ");


                }
                Console.WriteLine($"\t\t{nameof(BuildHLS)} - Total Time Taken to do job - {sw.Elapsed.ToString("mm\\:ss\\.ff")}");
                
                return success;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\t\t{nameof(BuildHLS)} - Time Elapsed till Exception {sw.Elapsed.ToString("mm\\:ss\\.ff")} {ex.Message} \n\n {ex.StackTrace} \n\n");


                return false;
            }
        }
        private (int, int) getProgressFromString(string sLine)
        {
            if (sLine == null)
                return (0,0);
            try
            {
                if (sLine.StartsWith("frame="))
                {
                    string[] framesInfoToArray = sLine.Split('=');
                    if (framesInfoToArray.Length < 2) { return (0, 0); }
                    int currentFrame = 0;
                    string[] framesCountSplitBySpace = framesInfoToArray[1].Split(' ');
                    int spacingIndex = 4;
                    if (framesCountSplitBySpace.Length <= 5)
                    {
                        int decrementer = 2;
                        spacingIndex = framesCountSplitBySpace.Length - decrementer;
                        if (spacingIndex < 0)
                        {
                            return (0,0);
                        }
                    }
                    string strCurrentFrame = framesCountSplitBySpace[spacingIndex];
                    bool isInt = int.TryParse(strCurrentFrame, out currentFrame);
                    if (!isInt)
                    {
                        return (0, 0);
                    }


                    string[] aSFPS = framesInfoToArray[2].Split(' ');
                    if (aSFPS.Length > 1)
                    {
                        spacingIndex = aSFPS.Length - 2;
                        string sFPS = aSFPS[spacingIndex];
                        float currentProccessingFPS = 0;
                        isInt = float.TryParse(sFPS, out currentProccessingFPS);
                        if (!isInt)
                        {

                            return (0, 0);
                        }


                        int framesLeft = this._video._frames - currentFrame;
                        float eta = 9999999;
                        string strEta = eta.ToString();
                        if (currentProccessingFPS > 0)
                        {
                            eta = (float)framesLeft / (float)currentProccessingFPS;
                            TimeSpan t = TimeSpan.FromSeconds(eta);

                            strEta = t.ToString(@"hh\:mm\:ss\:fff");

                        }
                        double progress = 1;

                        progress = Math.Round(((double)currentFrame / (double)this._video._frames) * 100d, 1);

                        // return $"\t\t{nameof(BuildHLS)}\t eta :{strEta}\t\tprogress :{progress}%";
                        return ((int)eta,(int)progress);
                    }
                }
                return (0, 0);
            }
            catch (Exception e)
            {

                Console.WriteLine($"{e.Message} \n\n {e.StackTrace}");
                return (0,100);
            }
        }

        public FFMPEG(Stream inStream, string fileOut, List<string> resolutions, IProgress<(int, int)> progress)
        {
            try
            {
                this.progress = progress;
                this.success = BuildHLS(inStream, fileOut, resolutions);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + "\n\n" + ex.StackTrace + "\n\n");
            }

        }
        private Stream? MoveFlags(Stream videoIn)
        {
            Stopwatch sw = Stopwatch.StartNew();

            try
            {
                //Console.WriteLine($"\t\t{nameof(MoveFlags)} - Attempting To Move mp4 Flags to the front");
                FFPipe? outFFPipe = CreatePipe(PipeDirection.InOut);

                if (outFFPipe == null)
                {
                    sw.Stop();
                    Console.WriteLine($"\t\t{nameof(MoveFlags)} - Pipe Was Null, Exiting after {sw.Elapsed.ToString("mm\\:ss\\.ff")}");
                    return null;
                }
                bool isWindows = RuntimeInformation.RuntimeIdentifier.StartsWith("win");
                videoIn.Position = 0;
                string applicationTmpPath = Path.Combine(System.IO.Path.GetTempPath(), "Aeirosoft", "video");

                string videoPath = Path.Combine(applicationTmpPath , outFFPipe.Value.PipeName);
                if (!Directory.Exists(applicationTmpPath))
                {
                    Directory.CreateDirectory(applicationTmpPath);
                }

                using (FileStream fs = new FileStream(videoPath, System.IO.FileMode.Create))
                {
                    videoIn.CopyTo(fs);
                    fs.Flush();
                }

                string outPipeNameFFmpeg;

                if (isWindows)
                {
                    outPipeNameFFmpeg = $@"\\.\pipe\{outFFPipe.Value.PipeName}";
                }
                else
                {
                    outPipeNameFFmpeg = $@"{outFFPipe.Value.PipeName}";

                }
                var argumentBuilder = new List<string>();

                argumentBuilder.Add("-loglevel error -y -i");
                argumentBuilder.Add(videoPath + " -f flv -c:v copy -map 0 -movflags faststart " + outPipeNameFFmpeg);



                using (var proc = StartFFMpeg(FFTYPE.FFMPEG, argumentBuilder))
                {
                    //Console.WriteLine($"FFMpeg path: " + FFTYPE.FFMPEG);
                    //Console.WriteLine($"Arguments: {proc.StartInfo.Arguments}");

                    proc.EnableRaisingEvents = true;
                    proc.Start();
                    Task task = null;

                    if (isWindows)
                    {
                        //Console.WriteLine($"\t\t{nameof(MoveFlags)} - Writing Windows Stream! - ");
                        outFFPipe.Value.Npss.WaitForConnection();
                        task = outFFPipe.Value.Npss.CopyToAsync(outFFPipe.Value.Stream)
                             .ContinueWith(x =>
                             {

                                 outFFPipe.Value.Npss.Disconnect();
                             });
                      //  Console.WriteLine($"\t\t{nameof(MoveFlags)} - Wrote Windows Stream! - inStream.Position : {outFFPipe.Value.Stream.Position} - Wrote: {outFFPipe.Value.Stream.Length} Bytes");
                    }

                    StringCollection values = new StringCollection();
                    proc.OutputDataReceived += (s, e) =>
                    {
                        lock (values)
                        {
                            values.Add(e.Data);
                        }
                    };
                    proc.ErrorDataReceived += (s, e) =>
                    {
                        lock (values)
                        {
                            values.Add("! >" + e.Data);
                        }
                    };

                    proc.BeginErrorReadLine();
                    proc.BeginOutputReadLine();

                    if (!proc.WaitForExit(/*99*/9999))
                    {
                        Console.WriteLine($"\n\n\t\t{nameof(MoveFlags)} - Proc Timed Out!!! - videoIn.Length : {videoIn.Length} - Wrote: {outFFPipe.Value.Stream.Length} Bytes\n\n");
                    }
                    else
                    {
                       // Console.WriteLine($"\t\t{nameof(MoveFlags)} - Proc Finished !!! - videoIn.Length : {videoIn.Length} - Wrote: { outFFPipe.Value.Stream.Length} Bytes");
                    }

                    if (task != null)
                    {
                        Task.WaitAll(task);
                        
                    }
                    else
                    {
                      //  Console.WriteLine($"\t\t{nameof(MoveFlags)} - Wrote Linux Stream! -  Wrote: {outFFPipe.Value.Stream.Length} Bytes");
                    }

                    try
                    {
                        lock (values)
                        {
                            foreach (string sline in values)
                            {
                                Console.WriteLine(sline);
                            }
                        }
                        
                    }
                    catch {


                    }
                    if (File.Exists(videoPath))
                    {
                        File.Delete(videoPath);
                    }
                    
                    outFFPipe.Value.Npss?.Dispose();
                    sw.Stop();
                    Console.WriteLine($"\t\t{nameof(MoveFlags)} - Total Time Taken to do job - {sw.Elapsed.ToString("hh\\mm\\:ss\\.ff")}");
                    if (videoIn.Length > outFFPipe.Value.Stream.Length * outFFPipe.Value.Stream.Length)
                    {
                        Console.WriteLine($"\t\t{nameof(MoveFlags)} - (videoIn.Length > outFFPipe.Value.Stream.Length * outFFPipe.Value.Stream.Length) - DOH! - Disposing Pipe Stream");
                        outFFPipe.Value.Stream.Dispose();
                        return null;
                    }

                    return outFFPipe?.Stream;
                }
            }

            catch (Exception ex)
            {
                Console.WriteLine($"\t\t{nameof(MoveFlags)} - Time Elapsed till Exception {sw.Elapsed.ToString("mm\\:ss\\.ff")} {ex.Message} \n\n {ex.StackTrace} \n\n");

                return null;
            }
        }
    }
}




