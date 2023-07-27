// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Net.Mime;
using System.Xml;

namespace TeamManiacs.Core.Models
{
    public partial class VideoUpload
    {
        public float VideoLength { get; set; }
        public IFormFile File { get; set; }

    }

}
