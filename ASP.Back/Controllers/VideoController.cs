﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeamManiacs.Core.Models;
using TeamManiacs.Data;
using System.Text;
using ASP.Back.Libraries;
using static ASP.Back.Libraries.FFMPEG;
using TeamManiacs.Core.Convertors;

namespace ASP.Back.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class VideoController : ControllerBase
    {

        private readonly IWebHostEnvironment hostEnvironment;
        private readonly TeamManiacsDbContext _context;



        ///<Summary>
        /// Constructor For Video Controller
        /// Host Env , DB Context
        ///</Summary>
        public VideoController(IWebHostEnvironment hostEnvironment, TeamManiacsDbContext context)
        {
            this.hostEnvironment = hostEnvironment;
            this._context = context;
        }


        ///<Summary>
        /// Gets the list of users videos
        /// [INT]Id = Profile User ID
        ///</Summary>
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<IAsyncEnumerable<Video>>> Get(int id)
        {

            try
            {
                var user = await ControllerHelpers.GetUserById(id, _context);
                if (user != null)
                {
                    var videos = await GetVideosByUser(user);
                    if (videos != null)
                    {
                        return Ok(videos);
                    }

                }
            }
            catch (Exception ex)
            {
                return BadRequest($"Video.GET: " + ex.Message);
            }
            return BadRequest($"Video.GET: No Videos");
        }
        ///<Summary>
        /// Returns the Requested Video
        ///</Summary>
        ///[INT]Id = Video Id
        ///if user sends token we can validate private video ownership
        /// <response code="200">Returns the Requested Video</response>
        /// <response code="400">If the item is null</response>
        [HttpGet("play/{id}")]
        public async Task GetPlay(int id)
        {
            byte[]? str = null;
            try
            {
                var videoIn = await _context.Videos.FindAsync(id);
                if (videoIn != null)
                {
                    var userId = ControllerHelpers.GetUserIdFromToken(this.User.Identity);

                        if(videoIn.isPrivate)
                        {
                            if (userId != videoIn.Uploader)
                            {
                                Response.StatusCode = 400;
                                 str = Encoding.UTF8.GetBytes($"Video.GET: Video is Private");
                                await Response.Body.WriteAsync(str, 0, str.Length);
                                return;
                            }
                               
                        }

                        using (Stream? master = GetMasterFile(videoIn))
                        {
                            if (master != null && master.Length > 0)
                            {

                                Response.StatusCode = 200;
                                Response.ContentType = videoIn.ContentType;
                                byte[] buffer = new byte[1024 * 10];
                                int bytesRead = 0;
                                while ((bytesRead = master.Read(buffer, 0, buffer.Length - 1)) > 0)
                                {
                                    //string base64Video = Convert.ToBase64String(buffer, 0, bytesRead, Base64FormattingOptions.None);
                                    await Response.Body.WriteAsync(buffer, 0, bytesRead);

                                }

                                await Response.Body.FlushAsync();
                                master.Close();
                                return;
                            }
                            else
                            {

                                Response.StatusCode = 400;
                                str = Encoding.UTF8.GetBytes($"Video.GET: Video is Null ");
                                await Response.Body.WriteAsync(str, 0 ,str.Length);

                                return;
                            }
                        }
                }
                Response.StatusCode = 400;
                 str = Encoding.UTF8.GetBytes($"Video.GET: Video Does not Exist on DB ");
                await Response.Body.WriteAsync(str, 0, str.Length);
                return;
            }
            catch (Exception ex)
            {
                Response.StatusCode = 400;
                str = Encoding.UTF8.GetBytes($"Video.GET: " + ex.Message);
                await Response.Body.WriteAsync(str, 0, str.Length);
                return;


            }
        }
        ///<Summary>
        ///upload a video as json form data. 
        /// Returns the Uploaded Video [INT]Id
        /// <response code="200">Returns the Uploaded Video Id</response>
        /// <response code="400">If the item is null</response>
        ///</Summary>
        [HttpPost]
        [Authorize]
        [DisableRequestSizeLimit]
        public async Task<ActionResult<int>> Post([FromForm] VideoUpload videoIn)
        {
            try
            {
                if (videoIn.File.Length / 1024 / 1024 <= 4000)
                {

                    var userId = ControllerHelpers.GetUserIdFromToken(this.User.Identity);
                    if (userId != null)
                    {
                        var user = await ControllerHelpers.GetUserById((int)userId, _context);
                        if (user != null)
                        {
                            int? ID = null;
                            if (user.Videos == null || user.Videos.Count <= 0)
                            {
                                ID = await AddVideoToDB(videoIn);
                                user.Videos = new List<int>();
                                if (ID != null)
                                {
                                    user.Videos.Add((int)ID);
                                    _context.Entry(user).State = EntityState.Modified;
                                    await _context.SaveChangesAsync();
                                }

                            }
                            else
                            {

                                ID = await AddVideoToDB(videoIn);
                                if (ID != null)
                                {
                                    user.Videos.Add((int)ID);
                                    _context.Entry(user).State = EntityState.Modified;
                                    await _context.SaveChangesAsync();
                                }
                            }
                            return Ok(ID);
                        }
                    }
                }
                return BadRequest();
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }

        }
        ///<Summary>
        /// Edits Video Information
        /// send values for every field. 
        /// [Int]
        /// Id
        /// STR[MaxLength(255)]
        /// Title
        /// STR[MaxLength(255)]
        /// Description
        /// STR[MaxLength(255)]
        /// IsPrivate
        ///</Summary>
        // <response code="200">Returns String</response>
        [HttpPut]
        [Authorize]
        public async Task<IActionResult> Put([FromBody] VideoEdit videoIn)
        {

            var userId = ControllerHelpers.GetUserIdFromToken(this.User.Identity);
            if (userId != null)
            {
                if (videoIn.Id > 0)
                {
                    Video? video = GetVideoById(videoIn.Id);
                    if (video != null)
                    {
                        if (video.Uploader == userId)
                        {
                            video.isPrivate = videoIn.IsPrivate;
                            video.Description = videoIn.Description;
                            //video.Ratings = videoIn.Ratings;
                            video.Title = videoIn.Title;

                            _context.Entry(video).State = EntityState.Modified;
                            try
                            {
                                await _context.SaveChangesAsync();
                            }
                            catch (DbUpdateConcurrencyException)
                            {

                                {
                                    throw;
                                }
                            }
                            return Ok($"Video Edited Successfully");
                        }

                    }
                }

            }
            return BadRequest($"Video.PUT");
        }

        protected Video? GetVideoById(int id)
        {

            return _context.Videos.Find(id);

        }
        ///<Summary>
        /// Deletes the Video by [INT]Id
        /// returns void
        ///</Summary>
        [HttpDelete("{id}")]
        [Authorize]
        public void Delete(int id)
        {
            try
            {
                Video? video = GetVideoById(id);
                if (video != null)
                {

                    _context.Videos.Remove(video);
                    _context.SaveChanges();
                    var fullFilePath = GetUploadsFolder(video.FileName);
                    if (System.IO.File.Exists(fullFilePath))
                    {
                        System.IO.File.Delete(fullFilePath);
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        private async Task<IEnumerable<Video>?> GetVideosByUser(Users user)
        {
            List<Video>? result = null;
            if (user.Videos?.Count > 0)
            {
                int storedVideoCount = user.Videos.Count;
                var ID = GetVideosByIDs(user.Videos);
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
        private async Task<int?> AddVideoToDB(VideoUpload videoIn)
        {

            var userId = ControllerHelpers.GetUserIdFromToken(this.User.Identity);
            if (userId != null)
            {
                var uniqueFileName = ControllerHelpers.GetUniqueFileName(videoIn.File.FileName);
                Video video = new Video(videoIn, (int)userId, uniqueFileName);
                try
                {
                    
                    
                    if(SaveVideoToMediaFolder(videoIn, uniqueFileName))
                    {
                        _context.Videos.Add(video);
                        await _context.SaveChangesAsync();
                    }

                }
                catch (Exception ex)
                {

                }
                return video.ID;
            }
            return null;
        }
        private Stream? GetMasterFile(Video videoIn)
        {
            var fileName = Path.GetFileNameWithoutExtension(videoIn.FileName);
            var fullFilePath = GetUploadsFolder(fileName) + '\\' + fileName + "_master.m3u8";
            FileStream? fileStream = new FileStream(fullFilePath, FileMode.Open, FileAccess.Read);
            if (fileStream != null)
            {
                return fileStream;
            }
            else
            {
                return null;
            }
        }
        //private Stream? GetVideoFromMediaFolder(Video videoIn)
        //{

        //    var fileName = Path.GetFileNameWithoutExtension(videoIn.FileName);
        //    var fullFilePath = GetUploadsFolder(fileName) + '\\' + "stream_0\\data000000.ts";
        //    FFMPEG video = new FFMPEG(fullFilePath, new[] { "1920:1080"/*, "1280:720", "720:480"*/ });
        //    if (video.success)
        //    {
        //        //FileStream? fileStream = new FileStream(file, FileMode.Open, FileAccess.Read);
        //        //return fileStream;
        //        // return video.Video.stream;
        //        return video.GetWebmStream(new[] { "1920:1080" });
        //    }
        //    else
        //        return null;
        //}
        
        private bool SaveVideoToMediaFolder(VideoUpload videoIn, string filePath = "")
        {
            try
            {
                if (filePath == "")
                {
                    filePath = ControllerHelpers.GetUniqueFileName(videoIn.File.FileName);
                }
                Stream videoStream = videoIn.File.OpenReadStream();
                
                FFMPEG ffmpeg = new FFMPEG(videoStream, GetUploadsFolder(filePath), new List<string> { "1920x1080" , "1280x720", "720x480"});
                
                
                videoStream?.Dispose();

                return ffmpeg.success;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }


        }
        private string GetUploadsFolder(string fileName)
        {
            var uploads = Path.Combine(hostEnvironment.WebRootPath, "uploads");
            Directory.CreateDirectory(uploads);
            return Path.Combine(uploads, fileName);
        }

        private List<Video>? GetVideosByIDs(List<int> IDs)
        {

            List<Video>? videos = new List<Video>();
            List<int> badIds = new List<int>();
            var userId = ControllerHelpers.GetUserIdFromToken(this.User.Identity);
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
                        var vid = GetFileFromVideo(video);
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
        private FileResult? GetFileFromVideo(Video video)
        {
            try
            {
                var fileName = Path.GetFileNameWithoutExtension(video.FileName);
                var fullFilePath = GetUploadsFolder(fileName) + '\\' + fileName + "_master.m3u8";
                if (System.IO.File.Exists(fullFilePath))
                {
                    return File(fullFilePath, video.ContentType, fullFilePath);
                }
                //return BadRequest($"Video.GetVideoByFileName:  Failed to Open File");

                // return BadRequest($"Video.GetVideoByFileName:  Failed to Find Video in DB");
            }
            catch (Exception ex)
            {
                return null;
                // return BadRequest($"Video.GetVideoByFileName: " + ex);
            }
            return null;
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
                    var fullFilePath = GetUploadsFolder(fileName);
                    if (System.IO.File.Exists(fullFilePath))
                    {
                        return File(fileName, video.ContentType, video.FileName);
                    }
                    //return BadRequest($"Video.GetVideoByFileName:  Failed to Open File");
                }
                return null;
                // return BadRequest($"Video.GetVideoByFileName:  Failed to Find Video in DB");
            }
            catch (Exception ex)
            {
                return null;
                // return BadRequest($"Video.GetVideoByFileName: " + ex);
            }

        }

    }
}
