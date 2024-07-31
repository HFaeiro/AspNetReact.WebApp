using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeamManiacs.Core.Models;
using TeamManiacs.Data;
using System.Text;
using ASP.Back.Libraries;
using System.Security.Principal;
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
        static Dictionary<int, MediaTask> processingList = new Dictionary<int, MediaTask>();

        private struct MediaTask
            {
            public IProgress<(int, int)> iProgress;
            public Task task;
            public MediaManager mediaManager;
        }


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


        private async Task UploadVideoAsync(int userId, IIdentity identity, IProgress<(int, int)> progress)
        {
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    TeamManiacsDbContext db = scope.ServiceProvider.GetService<TeamManiacsDbContext>();
                    if (db == null)
                    {
                        return;
                    }
                    var user = await ControllerHelpers.GetUserById((int)userId, db);
                    if (user != null)
                    {
                        int? ID = null;
                        if (user.Videos == null || user.Videos.Count <= 0)
                        {
                            user.Videos = new List<int>();
                        }
                        ID = await mediaManager.AddVideoToDB(identity, db, progress);
                        if (ID != null)
                        {
                            user.Videos.Add((int)ID);
                            db.Entry(user).State = EntityState.Modified;
                            await db.SaveChangesAsync();
                        }
                    }
                }
            }
        }
        [HttpGet("progress/{id}")]
        public async Task<ActionResult<(int,int)>> GetUploadProgress(int id)
        {
            try
            {
                if (processingList.ContainsKey(id))
                {
                    MediaTask mediaTask = processingList[id];

                    (int, int) progress = mediaTask.mediaManager.progress;
                    if (progress.Item2 == 100 || mediaTask.task.Status == TaskStatus.RanToCompletion)
                    {
                        processingList.Remove(id);
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
                    Console.WriteLine($"\t\t{nameof(Post)} - {videoIn.file.FileName} uploading for {userId}");

                    if (videoIn.file.Length / 1024 >= 1024 * 1024)
                    {
                        Stream vod = new System.IO.MemoryStream();
                        videoIn.file.CopyTo(vod);
                        VideoUpload videoUpload = new VideoUpload();
                        videoUpload.file = new FormFile(vod, 0, vod.Length, "streamFile", videoIn.file.FileName)
                        {
                            Headers = new HeaderDictionary(),
                            ContentType = videoIn.file.ContentType,
                            ContentDisposition = videoIn.file.ContentDisposition,
                        };
                        mediaManager.setVideoIn(videoUpload);
                        string uniqueFileName = mediaManager.getUniqueFileName(userId.GetHashCode());
                        IProgress<(int, int)> progress = new Progress<(int, int)>(progress =>
                        {


                            // Console.WriteLine($"\t\tEta {progress.Item1} \t\tProgress:{progress.Item2}%");
                            if (processingList.ContainsKey(this.mediaManager.TaskId))
                            {
                                MediaTask mediaTask = processingList[this.mediaManager.TaskId];
                                mediaTask.mediaManager.progress = progress;
                                processingList[this.mediaManager.TaskId] = mediaTask;
                            }

                        });
                        Task task = UploadVideoAsync(userId.Value, identity, progress);
                        mediaManager.TaskId = task.Id;

                        MediaTask mediaTask = new MediaTask();
                        mediaTask.task = task;
                        mediaTask.mediaManager = mediaManager;
                        mediaTask.iProgress = progress;

                        processingList.Add(task.Id, mediaTask);

                        return Ok(task.Id);
                    }
                    if (videoIn?.chunkCount > 0)
                    {
                        using (var scope = _serviceScopeFactory.CreateScope())
                        {
                            TeamManiacsDbContext db = scope.ServiceProvider.GetService<TeamManiacsDbContext>();
                            if (db == null)
                            {
                                return BadRequest("db null 168");
                            }
                            //first chunk
                            if (videoIn.uploadId == null)
                            {                                
                                using (var stream = new System.IO.MemoryStream())
                                {
                                    await videoIn.file.CopyToAsync(stream);
                                    videoIn.fileBytes = stream.ToArray();
                                }
                                var videoUpload = db.VideoUploads.Add(videoIn);
                                db.Entry(videoUpload).State = EntityState.Modified;
                                await db.SaveChangesAsync();
                                return Ok(videoUpload.Entity.uploadId);
                            }
                            //next chunks
                            else
                            {
                                var videoUpload = db.VideoUploads.Find(videoIn.uploadId);
                                if (videoUpload == null)
                                {
                                    return BadRequest("db null 169");
                                }

                                //dont add to db, write over existing file. we dont want to store a massive video file. Just a tmp buffer so we can recover 
                                videoUpload.file = videoIn.file;                               
                                using (var stream = new System.IO.MemoryStream())
                                {
                                    await videoIn.file.CopyToAsync(stream);
                                    videoUpload.fileBytes = stream.ToArray();
                                }                                
                                videoUpload.chunkNumber = videoIn.chunkNumber;
                                db.Entry(videoUpload).State = EntityState.Modified;
                                await db.SaveChangesAsync();
                                return Ok(videoUpload.chunkNumber);
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