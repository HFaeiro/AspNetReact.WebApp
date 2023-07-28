using ASP.Back.Libraries;
using Microsoft.AspNetCore.Mvc;
using TeamManiacs.Data;
using static ASP.Back.Libraries.FFMPEG;
// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ASP.Back.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StreamController : ControllerBase
    {



        // GET: api/<StreamController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<StreamController>/5
        [HttpGet("{id}")]
        public string Get(Guid guid)
        {

            return "value";
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

        private readonly IWebHostEnvironment hostEnvironment;
        private readonly TeamManiacsDbContext _context;
        private readonly IConfiguration _configuration;
        private MediaManager mediaManager;

        public StreamController(IWebHostEnvironment hostEnvironment, TeamManiacsDbContext context, IConfiguration configuration)
        {
            this.hostEnvironment = hostEnvironment;
            this._context = context;
            mediaManager = new MediaManager(hostEnvironment, context, configuration, this);
        }

       








    }
}
