// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

using System.ComponentModel.DataAnnotations;

namespace TeamManiacs.Core.Models
{
    public partial class VideoRating
    {
        [Key]
        public string Category { get; set; }
        public string Judge { get; set; }
        public Ratings Rating { get; set; }
    }



}
