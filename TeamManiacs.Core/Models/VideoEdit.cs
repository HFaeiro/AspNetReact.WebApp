// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

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



}
