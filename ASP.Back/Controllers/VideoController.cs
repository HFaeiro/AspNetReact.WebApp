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
using TeamManiacs.Core.Models;
using TeamManiacs.Data;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

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

        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }


        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }


        [HttpPost]
        [DisableRequestSizeLimit]
        public async Task<ActionResult<IFormFile>> Post( [FromForm]VideoUpload videoIn)
        {
            try
            {
                if (videoIn.File.Length / 1024 / 1024 <= 100)
                {
                    var user = GetUserByUsername(videoIn.Username);
                    if (user != null)
                    {
                        
                        if (user.videos == null || user.videos.Count <= 0)
                        {
                            Video video = await AddVideoToDB(videoIn);
                            user.videos = new List<int>();
                            user.videos.Add(video.ID);
                            await _context.SaveChangesAsync();
                            //return Ok(GetVideoByFileName(uniqueFileName, true));
                        }
                        else 
                        {

                            Video video = await AddVideoToDB(videoIn);
                            user.videos.Add(video.ID);
                            await _context.SaveChangesAsync();
                        }
                        int storedVideoCount = user.videos.Count;
                        var ID = GetVideosByIDs(user.videos);
                        if(storedVideoCount > user.videos.Count)
                        {
                            if(user.videos.Count <= 0)
                            { user.videos.Clear();
                                await _context.SaveChangesAsync();
                                return await Post(videoIn);
                                
                            }
                            await _context.SaveChangesAsync();
                        }
                        if (ID.Count > 0)
                            return Ok(ID[0]);
                        else
                            return BadRequest($"Video.POST:  No Videos by ID");
                    }
                }
                return BadRequest();
            }
            catch(Exception ex)
            {
                return BadRequest(ex);
            }

        }
        private async Task<Video> AddVideoToDB(VideoUpload videoIn)
        {
            var uniqueFileName = GetUniqueFileName(videoIn.File.FileName);
            SaveVideoToMediaFolder(videoIn, uniqueFileName);
            Video video = new Video(new VideoMetaData(videoIn));
            video.MetaData.Filename = uniqueFileName;
            _context.Videos.Add(video);
            await _context.SaveChangesAsync();
            return video;
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

        private List<ActionResult<IFormFile>?> GetVideosByIDs(List<int> IDs)
        {
            
            List<ActionResult<IFormFile>?> videos = new List<ActionResult<IFormFile>?>();
            List<int> badIds = new List<int>();
            foreach (int ID in IDs) {
                var video = _context.Videos.FirstOrDefault(x =>
                                                   x.ID == ID);
                if (video != null) {
                    if (video.MetaData.Filename != "")
                    {
                        var vid = GetVideoByFileName(video.MetaData.Filename);
                        if(vid.Result?.ToString()?.Length > 0)
                            videos.Add(vid);
                        else
                            badIds.Add(ID);

                    }
                        
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
                if (user.videos != null)
                {
                    return user.videos;
                }
                else return null;
            }
            else
                return null;

        }
        
        private Microsoft.AspNetCore.Mvc.ActionResult<IFormFile> GetVideoByFileName(string fileName)
        {
            try
            {
                Video? video = null;
               
                    video = _context.Videos.FirstOrDefault(x =>
                                              x.MetaData.Filename.ToLower() == fileName.ToLower());

                if (video != null)
                {
                    
                    FileStream fileStream = new FileStream(GetUploadsFolder(fileName), FileMode.Open);
                    if (fileStream.Length > 0)
                    {
                        IFormFile iVideo = new FormFile(fileStream, 0, fileStream.Length, video.MetaData.Title, video.MetaData.Filename)
                        {
                            Headers = new HeaderDictionary()
                        }; 
                        fileStream.Close();
                        return Ok(iVideo);
                    }
                    return BadRequest($"Video.GetVideoByFileName:  Failed to Open File");
                }
                return BadRequest($"Video.GetVideoByFileName:  Failed to Find Video in DB");
            }
            catch(Exception ex) {
                return BadRequest($"Video.GetVideoByFileName: " + ex);
            }
           
        }

        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        [HttpDelete("{id}")]
        public void Delete(int id)
        {
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
