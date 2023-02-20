using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Runtime.CompilerServices;
using TeamManiacs.Core.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ASP.Back.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class VideoController : ControllerBase
    {
        private readonly IWebHostEnvironment hostEnvironment;

        public VideoController(IWebHostEnvironment hostEnvironment)
        {
            this.hostEnvironment = hostEnvironment;
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
        public async Task<IActionResult> Post( [FromForm]VideoUpload video)
        {
            try
            {
                if (video.File.Length / 1024 / 1024 <= 100)
                {
                    var uniqueFileName = GetUniqueFileName(video.File.FileName);
                    var uploads = Path.Combine(hostEnvironment.WebRootPath, "uploads");
                    Directory.CreateDirectory(uploads);
                    var filePath = Path.Combine(uploads, uniqueFileName);
                    FileStream fileStream = new FileStream(filePath, FileMode.Create);
                    await video.File.CopyToAsync(fileStream);
                    fileStream.Close();
                    return Ok($"Uploaded Successfully");
                }
                return BadRequest();
            }
            catch(Exception ex)
            {
                return BadRequest();
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
