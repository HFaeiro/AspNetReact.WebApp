// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

using Microsoft.AspNetCore.Http;

namespace TeamManiacs.Core.Models
{
    public partial class VideoUpload
    {

        public string Username { get; set; }

        public IFormFile File { get; set; }


    }
}
