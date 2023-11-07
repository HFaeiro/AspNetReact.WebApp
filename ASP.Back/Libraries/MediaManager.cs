using ASP.Back.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        public enum MediaType
        {
            Video,
            Master,
            Index,
            Init
        }


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
            return Path.Combine(videosPath,fileName, fileName + "_index_" +  index + ".m3u8");
        }
        public string DataPath(string fileName, int index, int dataIndex)
        {
            return Path.Combine(RootPath, videosPath, fileName, "stream_" + index, "data" + dataIndex.ToString().PadLeft(6, '0') + ".m4s");
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
                Console.WriteLine(ex.Message + "\n\n" + ex.StackTrace + "\n\n");
                videoOut = null;
                return false;
            }
        }

        private Stream? GetVideoChunk(string fileName)
        {
            var fullFilePath = Path.Combine(IndexPath(fileName, 0), "data000000.m4s");
            FFMPEG video = new FFMPEG(fullFilePath);
            if (video.success)
            {
                return video.GetWebStream();
            }
            else
                return null;
        }
        private string GetInitPath(string fileName, int index)
        {
            return Path.Combine(videosPath, fileName, $"init_{index}.mp4");
        }
        private string GetMasterPath(string fileName)
        {
            return Path.Combine(videosPath, fileName, fileName + "_master.m3u8");
        }

        ////deprecated Do not use. 
        //private FileResult? GetVideoByFileName(string fileName)
        //{
        //    try
        //    {
        //        Video? video = null;

        //        video = _context.Videos.FirstOrDefault(x =>
        //                                  x.FileName.ToLower() == fileName.ToLower());

        //        if (video != null)
        //        {
        //            var videoFilePath = Path.Combine(videosPath,fileName);
        //            if (System.IO.File.Exists(videoFilePath))
        //            {

        //                return _controller.File(fileName, video.ContentType, video.FileName);
        //            }
        //            //return BadRequest($"Video.GetVideoByFileName:  Failed to Open File");
        //        }
        //        return null;
        //        // return BadRequest($"Video.GetVideoByFileName:  Failed to Find Video in DB");
        //    }
        //    catch (Exception ex)
        //    {
        //      Console.WriteLine(ex.Message + "\n\n" + ex.StackTrace + "\n\n");
        //        return null;
        //        // return BadRequest($"Video.GetVideoByFileName: " + ex);
        //    }

        //}
        public async Task<IEnumerable<Video>?> GetVideosByUser(Users user, IIdentity claimsIdentity)
        {
            List<Video>? result = null;
            if (user.Videos?.Count > 0)
            {

                int storedVideoCount = user.Videos.Count;
                var ID = GetVideosByIDs(user.Videos, claimsIdentity);
#if !DEBUG //we don't want to delete not found on disk videos if we are in dev environment

                if (storedVideoCount > ID.Count)
                {
                    //if(ID.Count == 0)
                    //{
                    //    user.Videos.Clear();
                    //}

                    _context.Entry(user).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                }
#endif
                if (ID?.Count > 0)
                    return ID;
            }
            return result;
        }
        public Stream? GetMedia(MediaType mediaType, string fileName, int index = 0, int dataIndex = 0)
        {
           
            string path = string.Empty;
            switch (mediaType)
            {
                case MediaType.Video:
                    {
                        path = DataPath(fileName,index, dataIndex);
                        break;
                    }
                case MediaType.Master:
                    {
                        path = GetMasterPath(fileName);
                        break;
                    }
                case MediaType.Init:
                    {
                        path = GetInitPath(fileName, index);
                        break;
                    }
                case MediaType.Index:
                    {
                        path = IndexPath(fileName, index);
                        break;
                    }
                    default:
                    {
                        Console.WriteLine(string.Format("Warning... Using Default Media Path: {0}, Please Check Configuration!"));
                        path = uploadsPath;
                        break;
                    }
            }
            Console.WriteLine($"\t\t{nameof(GetMedia)} - mediaType: {mediaType.ToString()} - video : {fileName} - Index : {index}, dataIndex : {dataIndex} \n ");
            if (!System.IO.File.Exists(path))
            {
                return null;
            }
            Stream? fileStream;

                
            if (mediaType != MediaType.Video)
            {
                fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
            }
            else
            {
               //fileStream = new FFMPEG(path)?.GetWebStream();
               fileStream = new FileStream(path,FileMode.Open, FileAccess.Read);
            }
            if (fileStream != null)
            {
                return fileStream;
            }
            else
            {
                return null;
            }

        }

        public FileStream? GetMasterFile(string fileName)
        {
            return GetMedia(MediaType.Master, fileName) as FileStream;
        }

        public async Task<int?> AddVideoToDB(VideoUpload videoIn, IIdentity claimsIdentity)
        {
            Console.WriteLine($"\t\t{nameof(AddVideoToDB)} - Adding {videoIn.File.FileName}");
            var userId = ControllerHelpers.GetUserIdFromToken(claimsIdentity);
            if (userId != null)
            {
                var uniqueFileName = ControllerHelpers.GetUniqueFileName(videoIn.File.FileName);
                Video video = new Video(videoIn, (int)userId, uniqueFileName);
                try
                {

                    FFVideo? videoOut = new FFVideo();
                    List<string> resolutions = new List<string> { "1920x1080", "1280x720", "720x480", "480x360", "360x240" };
                    if (SaveVideoToMediaFolder(videoIn, out videoOut, resolutions, uniqueFileName))
                    {
                        if (videoOut.HasValue)
                        {
                            video.GUID = videoOut.Value.GUID;
                        }
                        video.VideoLength = (int)videoIn.File.Length;
                        _context.Videos.Add(video);
                        await _context.SaveChangesAsync();
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message + "\n\n" + ex.StackTrace + "\n\n");
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
            Console.WriteLine($"\t\t{nameof(GetVideosByIDs)} - IDs: {IDs} - userID : {userId}");
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
                        var vid = GetMasterFile(video.GUID);
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



        //private List<int>? GetVideoIDsByUsername(string username)
        //{
        //    var user = ControllerHelpers.GetUserByUsername(username, _context);
        //    if (user != null)
        //    {
        //        if (user.Videos != null)
        //        {
        //            return user.Videos;
        //        }
        //        else return null;
        //    }
        //    else
        //        return null;
        //}
        public Video? GetVideoByGuid(string guid)
        {
            return _context.Videos.FirstOrDefault(x =>
                                                   x.GUID == guid);

        }
        public Video? GetVideoByGuid(Guid guid)
        {
            return _context.Videos.FirstOrDefault(x =>
                                                   x.GUID == guid.ToString("N"));
            
        }
    }

}
