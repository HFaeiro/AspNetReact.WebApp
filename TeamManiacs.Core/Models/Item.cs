using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamManiacs.Core.Models
{
    [Table("Item")]
    public class Item
    {
        [Key]
        public int Id { get; set; }
        [StringLength(200)]
        [Unicode(false)]
        public string Name { get; set; }
        [StringLength(200)]
        [Unicode(false)]
        public string Description { get; set; }
        [StringLength(200)]
        [Unicode(false)]
        public string Type { get; set; }

    }

}

