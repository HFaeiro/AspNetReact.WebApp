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
                       
                        if (user.videos == null)
                        {
                            var uniqueFileName = GetUniqueFileName(videoIn.File.FileName);
                            var uploads = Path.Combine(hostEnvironment.WebRootPath, "uploads");
                            Directory.CreateDirectory(uploads);
                            var filePath = Path.Combine(uploads, uniqueFileName);
                            FileStream fileStream = new FileStream(filePath, FileMode.Create);
                            await videoIn.File.CopyToAsync(fileStream);
                            fileStream.Close();
                            Video video = new Video(new VideoMetaData(videoIn));
                            video.MetaData.Filename = uniqueFileName;
                            // videoMetaData.Username = video.Username;
                            _context.Videos.Add(video);
                            var id = await _context.SaveChangesAsync();
                            user.videos = new List<int>();
                            user.videos.Add(id);
                            await _context.SaveChangesAsync();
                            return Ok(GetVideoByFileName(uniqueFileName, true));
                        }
                        else if(user.videos.Count < 4)
                        {
                            var uniqueFileName = GetUniqueFileName(videoIn.File.FileName);
                            var uploads = Path.Combine(hostEnvironment.WebRootPath, "uploads");
                            Directory.CreateDirectory(uploads);
                            var filePath = Path.Combine(uploads, uniqueFileName);
                            FileStream fileStream = new FileStream(filePath, FileMode.Create);
                            await videoIn.File.CopyToAsync(fileStream);
                            fileStream.Close();
                            Video video = new Video(new VideoMetaData(videoIn));
                            video.MetaData.Filename = uniqueFileName;
                            // videoMetaData.Username = video.Username;
                            _context.Videos.Add(video);
                            var id = await _context.SaveChangesAsync();
                            user.videos.Add(id);
                            await _context.SaveChangesAsync();
                        }
                        var ID = GetVideosByIDs(user.videos);
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
        private List<ActionResult<IFormFile>?> GetVideosByIDs(List<int> IDs)
        {
            
            List<ActionResult<IFormFile>?> videos = new List<ActionResult<IFormFile>?>();

            foreach (int ID in IDs) {
                var video = _context.Videos.FirstOrDefault(x =>
                                                   x.ID == ID);
                if (video != null) {
                    if(video.MetaData.Filename != "")
                     videos.Add(GetVideoByFileName(video.MetaData.Filename, check: false));
                }
                
            }
            return videos;
        }
        private Users? GetUserByUsername(string username)
        {
            return _context.UserModels.FirstOrDefault(x =>
                                                   x.Username.ToLower() == username.ToLower());
        }


        private int[]? GetVideoIDsByUsername(string username)
        {
            var user = GetUserByUsername(username);
            if (user != null)
            {
                if (user.videos != null)
                {
                    return user.videos.ToArray();
                }
                else return null;
            }
            else
                return null;

        }
        
        private Microsoft.AspNetCore.Mvc.ActionResult<IFormFile> GetVideoByFileName(string fileName, bool check)
        {
            try
            {
                Video? video = null;
                if (check) {
                    video = _context.Videos.FirstOrDefault(x =>
                                              x.MetaData.Filename.ToLower() == fileName.ToLower());
                }
                if (video != null || !check)
                {
                    var uploads = Path.Combine(hostEnvironment.WebRootPath, "uploads");
                    var filePath = Path.Combine(uploads, fileName);
                    FileStream fileStream = new FileStream(filePath, FileMode.Open);
                    if (fileStream.Length > 0)
                    {
                        IFormFile iVideo = new FormFile(fileStream, 0, fileStream.Length, "File", video.MetaData.Filename);
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
