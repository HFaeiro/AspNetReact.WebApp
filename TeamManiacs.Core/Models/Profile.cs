using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TeamManiacs.Core.Models
{
    public class Profile
    {
        private PrivTypes privileges;

        public Profile(int userId, string username, string token, PrivTypes privileges)
        {
            UserId = userId;
            Username = username;
            Token = token;
            Privileges = privileges;
        }

        [Key]
        public int UserId { get; set; }

        [StringLength(200)]
        [Unicode(false)]
        public string Username { get; set; }

        [StringLength(200)]
        [Unicode(false)]
        public string Token { get; set; }

        [JsonConverter(typeof(JsonStringEnumMemberConverter))]
        public PrivTypes Privileges { get; set; }
    }
}
