using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.Extensions.FileProviders;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Runtime.CompilerServices;
using System.IO;
using TeamManiacs.Core.Models;
using TeamManiacs.Data;
using System.Security.Claims;
using NuGet.Protocol;

namespace ASP.Back.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class VideoController : ControllerBase
    {
        private readonly IWebHostEnvironment hostEnvironment;
        private readonly TeamManiacsDbContext _context;

        public VideoController(IWebHostEnvironment hostEnvironment, TeamManiacsDbContext context)
        {
            this.hostEnvironment = hostEnvironment;
            this._context = context;
        }
        private async Task<Users?> GetUserById(int id)
        {
            return await _context.UserModels.FindAsync(id);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<IAsyncEnumerable<Video>>> Get(int id)
        {
           
            try
            {
                var user = await GetUserById(id);
                if (user != null)
                {
                    var videos = await GetVideosByUser(user);
                    if(videos != null)
                    {
                       
                        return Ok(videos);
                    }
                    
                }
            }
            catch (Exception ex) {
                return BadRequest($"Video.GET: " + ex.Message);
            }
            return BadRequest($"Video.GET: No Videos");
        }
        /// <response code="200">Returns the Requested Video</response>
        /// <response code="400">If the item is null</response>
        [HttpGet("play/{id}")]
        [Produces("application/json")]
        [Authorize]
        public async Task<ActionResult<string?>> GetPlay(int id)
        {
            try
            {
                var videoIn = await _context.Videos.FindAsync(id);
                if (videoIn != null)
                {
                    var userId = GetUserIdFromToken();
                    if (userId != null)
                    {
                        if(videoIn.isPrivate)
                        {
                            if (userId != videoIn.Uploader)
                                return BadRequest($"Video.GET: Video is Private");
                        }
                        using (FileStream video = GetVideoFromMediaFolder(videoIn))
                        {
                            if (video != null)
                            {
                                using (MemoryStream ms = new MemoryStream())
                                {

                                    await video.CopyToAsync(ms);
                                    string base64Video = Convert.ToBase64String(ms.ToArray(), 0, (int)ms.Length, Base64FormattingOptions.None);
                                    return Ok(base64Video);
                                }
                            }
                            else
                                return BadRequest($"Video.GET: Video was Null ");
                        }
                    }
                        

                }
            }
            catch (Exception ex)
            {
                return BadRequest($"Video.GET: " + ex.Message);
            }
            return BadRequest($"Video.GET: ");
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

        [HttpPost]
        [Authorize]
        [DisableRequestSizeLimit]
        public async Task<ActionResult<int>> Post([FromForm] VideoUpload videoIn)
        {
            try
            {
                if (videoIn.File.Length / 1024 / 1024 <= 100)
                {

                    var userId = GetUserIdFromToken();
                    if (userId != null)
                    {
                        var user = await GetUserById((int)userId);
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
        private DateTime? GetExpirationFromToken()
        {
            var claimsPrinciple = this.User.Identity as ClaimsPrincipal;
            Claim? exp = claimsPrinciple?.FindFirst("exp");
            if (exp != null)
            {
                var expiration = new DateTime(long.Parse(exp.Value));
                return expiration;
            }
            else return null;
        }
        private int? GetUserIdFromToken()
        {
            var claimsIdentity = this.User.Identity as ClaimsIdentity;
            string? claimId = null;
            try
            {
               claimId =  claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if(claimId != null)
                    return Int32.Parse(claimId);
            }
            catch (FormatException)
            {
                return null;
            }

            return null;
        }
        private async Task<int?> AddVideoToDB(VideoUpload videoIn)
        {

            var userId = GetUserIdFromToken();
            if (userId != null)
            {
                    var uniqueFileName = GetUniqueFileName(videoIn.File.FileName);
                    Video video = new Video(videoIn, (int)userId, uniqueFileName);
                    try
                    {
                        _context.Videos.Add(video);
                        await _context.SaveChangesAsync();
                        SaveVideoToMediaFolder(videoIn, uniqueFileName);

                    }
                    catch (Exception ex)
                    {

                    }
                    return video.ID;
            }
            return null;
        }
        private FileStream GetVideoFromMediaFolder(Video videoIn)
        {
           
            FileStream fileStream = new FileStream(GetUploadsFolder(videoIn.FileName), FileMode.Open);
            return fileStream;
        }
        private void SaveVideoToMediaFolder(VideoUpload videoIn, string filePath = "")
        {
            if(filePath == "")
            {
                filePath = GetUniqueFileName(videoIn.File.FileName);
            }
            FileStream fileStream = new FileStream(GetUploadsFolder(filePath), FileMode.Create);
           videoIn.File.CopyTo(fileStream);
            fileStream.Close();
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
            var userId = GetUserIdFromToken();
            foreach (int ID in IDs) {
                var video = _context.Videos.FirstOrDefault(x =>
                                                   x.ID == ID);
                if (video != null) {

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
            foreach(int ID in badIds)
            {
                IDs.Remove(ID);
            }
            
            return videos;
        }
        private Users? GetUserByUsername(string username)
        {
            return _context.UserModels.FirstOrDefault(x =>
                                                   x.Username.ToLower() == username.ToLower());
        }


        private List<int>? GetVideoIDsByUsername(string username)
        {
            var user = GetUserByUsername(username);
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
                    var fullFilePath = GetUploadsFolder(video.FileName);
                    if (System.IO.File.Exists(fullFilePath))
                    {
                        return File(video.FileName, video.ContentType, video.FileName);
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
            catch(Exception ex) {
                return null;
                // return BadRequest($"Video.GetVideoByFileName: " + ex);
            }
           
        }

        [HttpPut]
        [Authorize]
        public async Task<IActionResult> Put([FromBody] VideoEdit videoIn)
        {

            var userId = GetUserIdFromToken();
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

        private Video? GetVideoById(int id)
        {

            return _context.Videos.Find(id);
            
        }

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

        private string GetUniqueFileName(string fileName)
        {
            fileName = Path.GetFileName(fileName);
            return Path.GetFileNameWithoutExtension(fileName)
                + "_"
                + Guid.NewGuid().ToString().Substring(0, 4)
                + Path.GetExtension(fileName);
        }
    }
}
