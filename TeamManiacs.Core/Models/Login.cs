using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using TeamManiacs.Core;

namespace TeamManiacs.Core.Models;


[Keyless]
public partial class Login
{
    [StringLength(200)]
    [Unicode(false)]
    public string Email { get; set; }

    [StringLength(200)]
    [Unicode(false)]
    public string Username { get; set; }

    [StringLength(200)]
    [Unicode(false)]
    public string Password { get; set; }

}
