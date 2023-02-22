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
        public Video(VideoMetaData metaData)
        {
           
            MetaData = metaData;
        }
        public Video()
        {
            MetaData = new VideoMetaData();
        }

        [Key]
        public int ID { get; set; }
        public VideoMetaData MetaData { get; set; }
        public VideoRating[]? Ratings { get; set; }

    }
    public partial class VideoMetaData
    {

            public VideoMetaData(VideoUpload videoIn)
        {
            Filename = videoIn.File.FileName;
            Title = videoIn.File.Name;
            Description = videoIn.File.ContentDisposition;
            ContentType = videoIn.File.ContentType;
            ContentDisposition = videoIn.File.ContentDisposition;
        }
        public VideoMetaData(string filename = "", string title = "", string description = "", string contentType = "", string contentDisposition = "")
        {
            Filename = filename;
            Title = title;
            Description = description;
            ContentType = contentType;
            ContentDisposition = contentDisposition;
        }

        [Key]
        public string Filename { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ContentType { get; set; }
        public string ContentDisposition { get; set; }


    }

}
