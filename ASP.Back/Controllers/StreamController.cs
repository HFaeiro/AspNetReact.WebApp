using ASP.Back.Libraries;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.Metrics;
using TeamManiacs.Core.Models;
using TeamManiacs.Data;
using static ASP.Back.Libraries.FFMPEG;
// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ASP.Back.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StreamController : ControllerBase
    {
        private readonly IWebHostEnvironment hostEnvironment;
        private readonly TeamManiacsDbContext _context;
        private readonly IConfiguration _configuration;
        private MediaManager mediaManager;
        private StreamOut streamOut;

        public StreamController(IWebHostEnvironment hostEnvironment, TeamManiacsDbContext context, IConfiguration configuration)
        {
            this.hostEnvironment = hostEnvironment;
            this._context = context;
            mediaManager = new MediaManager(hostEnvironment, context, configuration, this);
            streamOut = new StreamOut(this);
        }

        // GET api/<StreamController>/guid/index
        [HttpGet]
        [Route("index")]
        public async Task Get([FromQuery] string guid, [FromQuery] int index)
        {
            try
            {
                Response.StatusCode = 400;
                Video? video = mediaManager.GetVideoByGuid(guid);
                if (video == null)
                {
                    Response.StatusCode = 400;
                    await streamOut.Write($"Stream.GET: Video is Null ");
                    return;
                }
                FileStream indexStream = mediaManager.GetMedia(MediaManager.MediaType.Index, video.GUID, index) as FileStream ;
                if (indexStream != null && indexStream.Length > 0)
                {
                    await streamOut.Write(indexStream, "text:html", StreamOut.StatusCodes.Text);
                    indexStream.Close();
                }
                if (streamOut.StatusCode == 400)
                {
                    await streamOut.Write($"Stream.GET: Video is Null or Video Does not Exist on DB ");
                    return;
                }
                return;
            }
            catch (Exception ex)
            {
                Response.StatusCode = 400;
                await streamOut.Write($"Stream.GET(Index): " + ex.Message);
                return;


            }
        }

        // GET api/<StreamController>/guid/index
        [HttpGet]
        [Route("data")]
        public async Task Get([FromQuery] string guid, [FromQuery] int index, [FromQuery] int dataIndex)
        {
            try
            {

                Video? video = mediaManager.GetVideoByGuid(Guid.Parse(guid));
                if (video == null)
                {
                    Response.StatusCode = 400;
                    await streamOut.Write($"Stream.GET: Video is Null ");
                    return;
                }
                Stream indexStream = mediaManager.GetMedia(MediaManager.MediaType.Video, video.GUID,index, dataIndex);
                if (indexStream != null && indexStream.Length > 0)
                {
                    await streamOut.Write(indexStream, video.ContentType, StreamOut.StatusCodes.Blob);
                    indexStream.Close();
                }
                if (streamOut.StatusCode == 400)
                {
                    await streamOut.Write($"Stream.GET: Video is Null or Video Does not Exist on DB ");
                    return;
                }
            }
            catch (Exception ex)
            {
                Response.StatusCode = 400;
                await streamOut.Write($"Stream.GET(Data): " + ex.Message);
                return;


            }
        }

        // POST api/<StreamController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<StreamController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<StreamController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }



       








    }
}
