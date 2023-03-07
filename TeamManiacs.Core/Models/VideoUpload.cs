// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net.Mime;

namespace TeamManiacs.Core.Models
{

    public partial class VideoEdit
    {

        public VideoEdit(int id, string title, string description, bool isPrivate) {
            Id = id;
            Title = title;
            Description = description;
            IsPrivate = isPrivate;
        }

        [Key]
        [Required]
        public int Id { get; set; }
        [Required]
        [MaxLength(255)]
        public string Title { get; set; }
        [Required]
        [MaxLength(255)]
        public string Description { get; set; }
        [Required]
        public bool IsPrivate { get; set; }
    }
    public partial class VideoUpload
    {
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
        public Video(VideoUpload videoIn, int uploader, string fileName = "")
        {

            FileName = fileName == "" ? videoIn.File.FileName: fileName;
            Title = videoIn.File.FileName.Split('_')[0];
            if(Title?.Length < 0)
                Title = videoIn.File.FileName;
            ContentSize = (int)videoIn.File.OpenReadStream().Length;
            Description = videoIn.File.ContentDisposition;
            ContentType = videoIn.File.ContentType;
            ContentDisposition = videoIn.File.ContentDisposition;
            Uploader = uploader;
            isPrivate= true;
        }

        [Key]
        public int ID { get; set; }
        public int ContentSize { get; set; }
        public bool isPrivate { get; set; }
        public string FileName { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ContentType { get; set; }
        public string ContentDisposition { get; set; }
        public int Uploader { get; set; } 
        public ICollection<VideoRating>? Ratings { get; set; }

    }



}
