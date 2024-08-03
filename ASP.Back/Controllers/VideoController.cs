using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeamManiacs.Core.Models;
using TeamManiacs.Data;
using System.Text;
using ASP.Back.Libraries;
using System.Security.Principal;
using System.Reflection.Metadata;
namespace ASP.Back.Controllers
{



    [Route("api/[controller]")]
    [ApiController]
    public class VideoController : ControllerBase
    {

        private readonly IWebHostEnvironment hostEnvironment;
        private readonly IConfiguration _configuration;
        IServiceScopeFactory _serviceScopeFactory;
        private MediaManager mediaManager;
        private StreamOut streamOut;




        ///<Summary>
        /// Constructor For Video Controller
        /// Host Env , DB Context
        ///</Summary>
        public VideoController(IWebHostEnvironment hostEnvironment, IServiceScopeFactory _serviceScopeFactory, IConfiguration configuration)
        {
            this.hostEnvironment = hostEnvironment;
            this._serviceScopeFactory = _serviceScopeFactory;
            mediaManager = new MediaManager(hostEnvironment, _serviceScopeFactory, configuration, this);
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
                Users? user = null;
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    TeamManiacsDbContext db = scope.ServiceProvider.GetService<TeamManiacsDbContext>();
                    if (db == null)
                    {
                        return BadRequest($"Video.GET: No DB");
                    }

                    user = await ControllerHelpers.GetUserById(id, db);
                    if (user != null)
                    {
                        var videos = await mediaManager.GetVideosByUser(user, this.User.Identity, db);
                        if (videos != null)
                        {
                            return Ok(videos);
                        }

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
                Video? videoIn = null;
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    TeamManiacsDbContext db = scope.ServiceProvider.GetService<TeamManiacsDbContext>();
                    if (db == null)
                    {
                        return ;
                    }
                    videoIn = await db.Videos.FindAsync(id);
                }
                Console.WriteLine($"\t\t{nameof(GetMaster)} - video Id: {id} - video : {(videoIn != null ? videoIn.FileName : null)}");
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


       
        [HttpGet("progress/{id}")]
        public async Task<ActionResult<(int,int)>> GetUploadProgress(int id)
        {
            try
            {
               
                if (MediaManager.processingList.ContainsKey(id))
                {
                    MediaManager.MediaTask mediaTask = MediaManager.processingList[id];

                    (int, int) progress = mediaTask.mediaManager.progress;
                    if (progress.Item2 == 100 || mediaTask.task.Status == TaskStatus.RanToCompletion)
                    {
                        //might have multithreading issue with multiple requests
                        MediaManager.processingList.Remove(id);
                        progress.Item2 = 100;
                    }
                    return Ok(progress.ToTuple<int,int>());                    
                }
            }
            catch
            (Exception ex)
            {
                return (0, 0);
            }
            return (0, 0);
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
                if (this.User.Identity == null)
                {
                    return BadRequest();
                }
                var userId = ControllerHelpers.GetUserIdFromToken(this.User.Identity);
                if (userId != null)
                {
                    IIdentity? identity = this.User.Identity;
                    Console.WriteLine($"\t\t{nameof(Post)} - {videoIn.file.FileName} Processing chunk number {videoIn.chunkNumber} for {userId}");
                    if (videoIn.file.Length > 0)
                    {
                        VideoBlob videoBlob = new VideoBlob(videoIn);                       
                        if (videoBlob.chunkCount > 0)
                        {
                            using (var scope = _serviceScopeFactory.CreateScope())
                            {
                                TeamManiacsDbContext? db = scope.ServiceProvider.GetService<TeamManiacsDbContext>();
                                if (db == null)
                                {
                                    return BadRequest("db null 168");
                                }
                                //first chunk
                                if (videoBlob.uploadId == Guid.Empty && videoBlob.chunkCount > 1)
                                {
                                    var dbBlob = db.VideoBlobs.Add(videoBlob);
                                    
                                    //db.Entry(videoUpload).State = EntityState.Modified;
                                    
                                    if (mediaManager.SaveBlobToFolder(videoBlob, userId.Value))
                                    {
                                        dbBlob.Entity.collectedChunks++;
                                        await db.SaveChangesAsync();
                                        return Ok(dbBlob.Entity.uploadId);
                                    }
                                    else
                                    {
                                        mediaManager.CleanUpBlob( userId.Value, videoBlob.uploadId, videoBlob.chunkNumber);                                        
                                    }

                                }
                                //next chunks
                                else
                                {
                                    if (videoBlob.chunkCount > 1)
                                    {

                                        if (mediaManager.SaveBlobToFolder(videoBlob, userId.Value))
                                        {
                                            var dbBlob = db.VideoBlobs.Find(videoBlob.uploadId);
                                            if (dbBlob == null)
                                            {
                                                return BadRequest("db null 169");
                                            }
                                            dbBlob.collectedChunks++;
                                            await db.SaveChangesAsync();
                                            //last chunk was sent 
                                            if (dbBlob.collectedChunks >= dbBlob.chunkCount)
                                            {
                                                //send file to ffmpeg for processing. 
                                                Task? task = mediaManager.ProcessFinishedBlob(videoBlob, userId.Value, identity);
                                                if (task != null)
                                                {
                                                    
                                                    return Ok(task.Id);
                                                }
                                                else
                                                {
                                                    dbBlob.collectedChunks--;
                                                    mediaManager.CleanUpFailedUpload(userId.Value, videoBlob.uploadId);
                                                    return StatusCode(401);
                                                }
                                            }
                                            else
                                            {                                                
                                                return Ok(videoBlob.uploadId);
                                            }
                                        }
                                        else
                                        {
                                            
                                            await db.SaveChangesAsync();
                                            mediaManager.CleanUpBlob(userId.Value, videoBlob.uploadId, videoBlob.chunkNumber);                                            
                                        }
                                    }
                                    else
                                    {
                                        if (videoBlob.uploadId == Guid.Empty)
                                        {
                                            videoBlob.uploadId = Guid.NewGuid();
                                        }
                                        //send file to ffmpeg for processing. 
                                        Task? task = mediaManager.ProcessFinishedBlob(videoIn, userId.Value, identity);
                                        if (task != null)
                                        {
                                            return Ok(task.Id);
                                        }
                                        else
                                        {
                                            return StatusCode(401);
                                        }
                                    }
                                }                               
                            }
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
                            using (var scope = _serviceScopeFactory.CreateScope())
                            {
                                TeamManiacsDbContext db = scope.ServiceProvider.GetService<TeamManiacsDbContext>();
                                if (db == null)
                                {
                                    return BadRequest($"Video.GET: No DB");
                                }
                                db.Entry(video).State = EntityState.Modified;
                                try
                                {
                                    await db.SaveChangesAsync();
                                }
                                catch (DbUpdateConcurrencyException)
                                {

                                    {
                                        throw;
                                    }
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
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                TeamManiacsDbContext db = scope.ServiceProvider.GetService<TeamManiacsDbContext>();
                if (db == null)
                {
                    return null;
                }
                return db.Videos.Find(id);
            }
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
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        TeamManiacsDbContext db = scope.ServiceProvider.GetService<TeamManiacsDbContext>();
                        if (db == null)
                        {
                            return;
                        }
                        db.Videos.Remove(video);
                        db.SaveChanges();
                    }
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