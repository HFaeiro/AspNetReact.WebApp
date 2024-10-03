using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamManiacs.Core.Models
{
    public class AuthCode
    {
        [Required]
        [Key]
        public int Uid { get; set; }
        [StringLength(200)]
        [Required]
        public string Code { get; set; }

        [Column(TypeName = "Date")]
        public DateTime CreatedDate { get; set; }
    }
}
