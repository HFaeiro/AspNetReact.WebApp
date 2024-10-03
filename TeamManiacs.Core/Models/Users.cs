using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using TeamManiacs.Core.Enums;

namespace TeamManiacs.Core.Models;


[Table("Users")]
public partial class Users
{
   
    public Users replace(Users obj)
    {

        this.UserId = obj.UserId;
        this.Username= obj.Username;
        this.Privileges = obj.Privileges;
        if (obj.Password != null)
        {
            this.Password = obj.Password;
        }
        return this;
    }
    public Users()
    {

    }
    public Users (string email, byte[] password, string username = " ")
    {
        this.Password= password;

        if(string.IsNullOrEmpty(username))
        {
            if (!string.IsNullOrEmpty(email))
            {
                this.Username = email;
            }
        }
        else 
        {
            this.Username = username;
        }
        this.Email = email;
    }



    [Required]
    [Key]
    public int UserId { get; set; }

    [StringLength(200)]
    [Unicode(false)]
    public string Username { get; set; }

    [Required]
    [StringLength(200)]
    [Unicode(false)]
    public string Email { get; set; }

    [Required]
    [StringLength(200)]
    [Unicode(false)]
    public byte[] Password { get; set; }

    [JsonConverter(typeof(JsonStringEnumMemberConverter))]
    public PrivTypes Privileges { get; set; }

    
    [JsonConverter(typeof(JsonStringEnumMemberConverter))]
    public UserStatus Status { get; set; }
    
    public List<int>? Videos {get; set;}
}
