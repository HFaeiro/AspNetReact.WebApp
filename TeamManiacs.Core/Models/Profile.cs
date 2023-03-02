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

        public Profile(int userId, string username, string token, PrivTypes privileges, List<int>? videos)
        {
            UserId = userId;
            Username = username;
            Token = token;
            Privileges = privileges;
            Videos = videos?.Count>0 ? videos : new List<int>();
        }
        public Profile(Users user)
        {
            UserId = user.UserId;
            Username = user.Username;
            Privileges= user.Privileges;
            Videos = user.Videos?.Count > 0 ? user.Videos : new List<int>();
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
        public List<int> Videos { get; set; }
    }
}
