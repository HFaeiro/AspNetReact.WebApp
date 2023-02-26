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
        public async Task<ActionResult<IEnumerable<Video>>> Get(int id)
        {
            List<Video>? result = null;
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
                return BadRequest($"Video.Get: " + ex.Message);
            }
            return BadRequest($"Video.Get: No Videos");
        }

        [HttpGet("play/{id}")]
        [Authorize]
        public async Task<ActionResult<string?>> GetPlay(int id)
        {
            try
            {
                var videoIn = await _context.Videos.FindAsync(id);
                if (videoIn != null)
                {
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
                            return BadRequest($"Video.Get: Video was Null ");
                    }
                        

                }
            }
            catch (Exception ex)
            {
                return BadRequest($"Video.Get: " + ex.Message);
            }
            return BadRequest($"Video.Get: ");
        }

        private async Task<IEnumerable<Video>?> GetVideosByUser(Users user)
        {
            List<Video>? result = null;
            if (user.Videos?.Count > 0)
            {
                int storedVideoCount = user.Videos.Count;
                var ID = GetVideosByIDs(user.Videos.ToList());
                if (storedVideoCount > ID.Count)
                {
                    if(ID.Count == 0)
                    {
                        user.Videos.Clear();
                    }
                    _context.Entry(user).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                }
                if (ID?.Count > 0)
                    return ID;
            }
            return result;
        }

        [HttpPost]
        [Authorize]
        [DisableRequestSizeLimit]
        public async Task<ActionResult> Post( [FromForm]VideoUpload videoIn)
        {
            try
            {
                if (videoIn.File.Length / 1024 / 1024 <= 100)
                {
                    var user = GetUserByUsername(videoIn.Username);
                    if (user != null)
                    {
                        
                        if (user.Videos == null || user.Videos.Count <= 0)
                        {
                            int ID = await AddVideoToDB(videoIn);
                            user.Videos = new List<int>();
                            user.Videos.Add(ID);
                            _context.Entry(user).State = EntityState.Modified;
                            await _context.SaveChangesAsync();
                            //return Ok(GetVideoByFileName(uniqueFileName, true));
                        }
                        else 
                        {

                            int ID = await AddVideoToDB(videoIn);
                            user.Videos.Add(ID);
                            _context.Entry(user).State = EntityState.Modified;
                            await _context.SaveChangesAsync();
                        }

                            return Ok($"Added Video to User!");
                   }
                }
                return BadRequest();
            }
            catch(Exception ex)
            {
                return BadRequest(ex);
            }

        }
        private async Task<int> AddVideoToDB(VideoUpload videoIn)
        {
            var uniqueFileName = GetUniqueFileName(videoIn.File.FileName);
            SaveVideoToMediaFolder(videoIn, uniqueFileName);
            Video video = new Video(videoIn, uniqueFileName);
            try
            {
                _context.Videos.Add(video);
                await _context.SaveChangesAsync();
            }
            catch(Exception ex)
            {
              
            }
            return video.ID;
        }
        private FileStream GetVideoFromMediaFolder(Video videoIn)
        {
           
            FileStream fileStream = new FileStream(GetUploadsFolder(videoIn.FileName), FileMode.Open);
            return fileStream;
        }
        private async void SaveVideoToMediaFolder(VideoUpload videoIn, string filePath = "")
        {
            if(filePath == "")
            {
                filePath = GetUniqueFileName(videoIn.File.FileName);
            }
            FileStream fileStream = new FileStream(GetUploadsFolder(filePath), FileMode.Create);
            await videoIn.File.CopyToAsync(fileStream);
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
            foreach (int ID in IDs) {
                var video = _context.Videos.FirstOrDefault(x =>
                                                   x.ID == ID);
                if (video != null) {
                    if (video.FileName != "")
                    {
                        //var vid = GetVideoByFileName(video.Filename);
                        //if(vid != null)
                            videos.Add(video);
                        //else
                        //    badIds.Add(ID);

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
        
        private FileResult? GetVideoByFileName(string fileName)
        {
            try
            {
                Video? video = null;
               
                    video = _context.Videos.FirstOrDefault(x =>
                                              x.FileName.ToLower() == fileName.ToLower());

                if (video != null)
                {
                    
                    return File(fileName,video.ContentType, video.FileName);
                    
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

        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async void Delete(int id)
        {
            try
            {
                Video? video = _context.Videos.Find(id);
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
        //private async Task<Video?> GetVideoById(int id)
        //{

        //    var video = 

        //    return video;
        //}
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
