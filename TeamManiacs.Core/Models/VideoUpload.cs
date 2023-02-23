// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net.Mime;

namespace TeamManiacs.Core.Models
{
    
    public partial class VideoUpload
    {

        public string Username { get; set; }

        public IFormFile File { get; set; }


    }
   
    public partial class VideoRating
    {
        [Key]
        public string Category { get; set; }
        public string Judge { get; set; }
        public Ratings Rating { get; set; }
    }
 
    public enum Ratings
    {
        Novice,
        NeedsImprovement,
        Competent,
        ExceedsExpectations,
        Expert

    }
    [Table("Videos")]
    public partial class Video
    {
        public Video()
        {

        }
        public Video(VideoUpload videoIn, string fileName = "")
        {

            FileName = fileName == "" ? videoIn.File.FileName: fileName;
            Title = videoIn.File.Name;
            Description = videoIn.File.ContentDisposition;
            ContentType = videoIn.File.ContentType;
            ContentDisposition = videoIn.File.ContentDisposition;
        }

        [Key]
        public int ID { get; set; }
        public string FileName { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ContentType { get; set; }
        public string ContentDisposition { get; set; }
        public ICollection<VideoRating>? Ratings { get; set; }

    }



}
