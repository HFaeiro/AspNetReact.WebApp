using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using TeamManiacs.Core;

namespace TeamManiacs.Core.Models;


[Table("Users")]
public partial class Users
{
    [Key]
    public int UserId { get; set; }

    [StringLength(200)]
    [Unicode(false)]
    public string Username { get; set; }

    [StringLength(200)]
    [Unicode(false)]
    public string Password { get; set; }
    [JsonConverter(typeof(JsonStringEnumMemberConverter))]
    public PrivTypes Privileges { get; set; }

    public List<int>? videos {get; set;}
}
