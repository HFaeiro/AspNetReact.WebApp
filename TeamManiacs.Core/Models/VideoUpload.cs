// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeamManiacs.Core.Models
{
    public partial class VideoUpload
    {
        [Key]
        public Guid? uploadId       { get; set; }
        //public int videoDuration  { get; set; }
        //public int videoHeight    { get; set; }
        //public int videoWidth     { get; set; }
        public int chunkCount     { get; set; }
        public int chunkNumber    { get; set; }
        [NotMapped]
        public IFormFile file       { get; set; }
        public byte[] fileBytes  { get; set; }

    }

}
