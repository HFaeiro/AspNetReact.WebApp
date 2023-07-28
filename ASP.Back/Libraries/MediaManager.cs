using ASP.Back.Controllers;
using Microsoft.AspNetCore.Mvc;
using System.Security.Principal;
using TeamManiacs.Core.Models;
using TeamManiacs.Data;
using static ASP.Back.Libraries.FFMPEG;

namespace ASP.Back.Libraries
{
    public class MediaManager
    {
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;
        private readonly ControllerBase _controller;
        private readonly string RootPath;
        public readonly string uploadsPath;
        public readonly string videosPath;
        private readonly TeamManiacsDbContext _context;

        public MediaManager(IWebHostEnvironment hostEnvironment, TeamManiacsDbContext context, IConfiguration config , ControllerBase controller)
        {
            _hostEnvironment = hostEnvironment;
            _context = context;
            _configuration = config;
            _controller = controller;
            RootPath = _hostEnvironment.WebRootPath;
            uploadsPath = Path.Combine(RootPath, "uploads");
            videosPath = Path.Combine(uploadsPath, "videos");
            Directory.CreateDirectory(uploadsPath);
            Directory.CreateDirectory(videosPath);

        }

        public string IndexPath(string fileName, int index) {
            return Path.Combine(RootPath, videosPath,fileName, "stream_" +  index);
        }

        public bool SaveVideoToMediaFolder(VideoUpload videoIn, out FFVideo? videoOut,
                     List<string> resolutions, string filePath = "")
        {
            try
            {
                if (filePath == "")
                {
                    filePath = ControllerHelpers.GetUniqueFileName(videoIn.File.FileName);
                }
                Stream videoStream = videoIn.File.OpenReadStream();

                FFMPEG ffmpeg = new FFMPEG(videoStream, Path.Combine(videosPath, filePath), resolutions);
                videoStream?.Dispose();
                videoOut = ffmpeg.Video;
                if (!ffmpeg.success)
                {
                    return false;
                }
                return ffmpeg.AppendLineMaster("#GUID=" + ffmpeg.Video.GUID, true);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                videoOut = null;
                return false;
            }
        }

        private Stream? GetVideoChunk(string fileName)
        {
            fileName = Path.GetFileNameWithoutExtension(fileName);
            var fullFilePath = Path.Combine(IndexPath(fileName, 0), "data000000.ts");
            FFMPEG video = new FFMPEG(fullFilePath);
            if (video.success)
            {
                return video.GetWebStream();
            }
            else
                return null;
        }
        private string GetMasterPath(string fileName)
        {
            return Path.Combine(videosPath, fileName, fileName + "_master.m3u8");
        }
        
        private FileResult? GetVideoByFileName(string fileName)
        {
            try
            {
                Video? video = null;

                video = _context.Videos.FirstOrDefault(x =>
                                          x.FileName.ToLower() == fileName.ToLower());

                if (video != null)
                {
                    var videoFilePath = Path.Combine(videosPath,fileName);
                    if (System.IO.File.Exists(videoFilePath))
                    {
                        
                        return _controller.File(fileName, video.ContentType, video.FileName);
                    }
                    //return BadRequest($"Video.GetVideoByFileName:  Failed to Open File");
                }
                return null;
                // return BadRequest($"Video.GetVideoByFileName:  Failed to Find Video in DB");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
                // return BadRequest($"Video.GetVideoByFileName: " + ex);
            }

        }


        public Stream? GetMasterFile(string fileName)
        {
            fileName = Path.GetFileNameWithoutExtension(fileName);
            var masterPath = GetMasterPath(fileName);
            if (!System.IO.File.Exists(masterPath))
            {
                return null;
            }
            FileStream? fileStream = new FileStream(masterPath, FileMode.Open, FileAccess.Read);
            if (fileStream != null)
            {
                return fileStream;
            }
            else
            {
                return null;
            }
        }

        public async Task<int?> AddVideoToDB(VideoUpload videoIn, IIdentity claimsIdentity)
        {

            var userId = ControllerHelpers.GetUserIdFromToken(claimsIdentity);
            if (userId != null)
            {
                var uniqueFileName = ControllerHelpers.GetUniqueFileName(videoIn.File.FileName);
                Video video = new Video(videoIn, (int)userId, uniqueFileName);
                try
                {

                    FFVideo? videoOut = new FFVideo();
                    List<string> resolutions = new List<string> { "1920x1080", "1280x720", "720x480" };
                    if (SaveVideoToMediaFolder(videoIn, out videoOut, resolutions, uniqueFileName))
                    {
                        if (videoOut.HasValue)
                        {
                            video.GUID = videoOut.Value.GUID;
                        }
                        video.VideoLength = (int)videoIn.VideoLength;
                        _context.Videos.Add(video);
                        await _context.SaveChangesAsync();
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                return video.ID;
            }
            return null;
        }
        public List<Video>? GetVideosByIDs(List<int> IDs, System.Security.Principal.IIdentity claimsIdentity)
        {

            List<Video>? videos = new List<Video>();
            List<int> badIds = new List<int>();
            var userId = ControllerHelpers.GetUserIdFromToken(claimsIdentity);
            foreach (int ID in IDs)
            {
                var video = _context.Videos.FirstOrDefault(x =>
                                                   x.ID == ID);
                if (video != null)
                {

                    if (video.isPrivate && userId != null && userId != video.Uploader)
                    {
                        //we can't verify if this is users own private video so we will skip this private video.
                        continue;
                    }

                    if (video.FileName != "")
                    {
                        var vid = GetMasterFile(video.FileName);
                        if (vid != null)
                            videos.Add(video);
#if !DEBUG //we don't want to delete not found on disk videos if we are in dev environment
                        else
                            badIds.Add(ID);
#endif
                    }
                    else
                        badIds.Add(ID);

                }
                else
                    badIds.Add(ID);

            }
            foreach (int ID in badIds)
            {
                IDs.Remove(ID);
            }

            return videos;
        }



        private List<int>? GetVideoIDsByUsername(string username)
        {
            var user = ControllerHelpers.GetUserByUsername(username, _context);
            if (user != null)
            {
                if (user.Videos != null)
                {
                    return user.Videos;
                }
                else return null;
            }
            else
                return null;
        }

    }

}
