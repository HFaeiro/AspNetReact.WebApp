using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeamManiacs.Core.Models;
using TeamManiacs.Data;
using System.Text;
using ASP.Back.Libraries;
using static ASP.Back.Libraries.FFMPEG;
using TeamManiacs.Core.Convertors;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace ASP.Back.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class VideoController : ControllerBase
    {

        private readonly IWebHostEnvironment hostEnvironment;
        private readonly TeamManiacsDbContext _context;
        private readonly IConfiguration _configuration;
        private MediaManager mediaManager;
        private StreamOut streamOut;


        ///<Summary>
        /// Constructor For Video Controller
        /// Host Env , DB Context
        ///</Summary>
        public VideoController(IWebHostEnvironment hostEnvironment, TeamManiacsDbContext context, IConfiguration configuration)
        {
            this.hostEnvironment = hostEnvironment;
            this._context = context;
            mediaManager = new MediaManager(hostEnvironment, context, configuration, this);
            _configuration = configuration;
            streamOut = new StreamOut(this);
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
                    var videos = await mediaManager.GetVideosByUser(user, this.User.Identity);
                    if (videos != null)
                    {
                        return Ok(videos);
                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + "\n\n" + ex.StackTrace + "\n\n");
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
        [HttpGet("master/{id}")]
        public async Task GetMaster(int id)
        {
            byte[]? str = null;
            Response.StatusCode = 400;
            try
            {
                var videoIn = await _context.Videos.FindAsync(id);
                if (videoIn != null)
                {
                    var userId = ControllerHelpers.GetUserIdFromToken(this.User.Identity);

                    if (videoIn.isPrivate)
                    {
                        if (userId != videoIn.Uploader)
                        {
                            str = Encoding.UTF8.GetBytes($"Video.GET: Video is Private");
                            await Response.Body.WriteAsync(str, 0, str.Length);
                            return;
                        }

                    }
                    Stream? master = mediaManager.GetMedia(MediaManager.MediaType.Master, videoIn.GUID) as FileStream;
                    if (master != null && master.Length > 0)
                    {
                        await streamOut.Write(master, videoIn.ContentType, StreamOut.StatusCodes.Text); 
                        master.Close();
                    }
                    if (streamOut.StatusCode == 400)
                    {
                        await streamOut.Write($"Video.GET: Video is Null ");
                    }
                    return;
                }
                await streamOut.Write($"Video.GET: Video Does not Exist on DB ");
                return;
            }
            catch (Exception ex)
            {
                Response.StatusCode = 400;
               await streamOut.Write($"Video.GET: " + ex.Message);
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
                    if(this.User.Identity == null)
                    {
                        return BadRequest();
                    }
                    var userId = ControllerHelpers.GetUserIdFromToken(this.User.Identity);
                    if (userId != null)
                    {
                        var user = await ControllerHelpers.GetUserById((int)userId, _context);
                        if (user != null)
                        {
                            int? ID = null;
                            if (user.Videos == null || user.Videos.Count <= 0)
                            {
                                ID = await mediaManager.AddVideoToDB(videoIn, this.User.Identity);
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

                                ID = await mediaManager.AddVideoToDB(videoIn, this.User.Identity);
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
              Console.WriteLine(ex.Message + "\n\n" + ex.StackTrace + "\n\n");
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
                    var fullFilePath = Path.Combine(mediaManager.videosPath, video.VideoName);
                    if (System.IO.File.Exists(fullFilePath))
                    {

                        System.IO.File.Delete(fullFilePath);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + "\n\n" + ex.StackTrace + "\n\n");
            }
        }

        


    }
}