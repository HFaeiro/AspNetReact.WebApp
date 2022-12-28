using TeamManiacs.Core;
using Microsoft.EntityFrameworkCore;
using TeamManiacs.Data;
using ASP.Back.Controllers;
using System.Text.Json.Serialization;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

internal class Program
{
    private static void Main(string[] args)
    {
        var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddCors(options =>
        {
            options.AddPolicy(name: MyAllowSpecificOrigins,
                              policy =>
                              {
                                  policy.WithOrigins("localhost").AllowAnyHeader().AllowAnyMethod(); 
                              });
        });

        //Add services to the container.
        builder.Services.AddDbContext<TeamManiacsDbContext>(options =>
        {
            IConfiguration config = new ConfigurationBuilder()
                                        .AddJsonFile("appsettings.json", optional: false)
                                        .Build();
            //options.UseSqlServer(config.GetConnectionString("ASPBackContext"));
            var connectString = config.GetConnectionString("CleverCloudSQL");
            options.UseMySql(connectString, ServerVersion.AutoDetect(connectString));

        });
        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddControllers()
                        .AddJsonOptions(o =>
                        {
                            //o.JsonSerializerOptions.NumberHandling = JsonNumberHandling.AllowReadingFromString;
                            o.JsonSerializerOptions.Converters.Add(new JsonStringEnumMemberConverter());

                        });

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                        .AddJwtBearer(options =>
                        {
                            options.TokenValidationParameters =
                            new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                            {
                                ValidateIssuer = true,
                                ValidateAudience = true,
                                ValidateIssuerSigningKey = true,
                                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                                ValidAudience = builder.Configuration["Jwt:Audience"],
                                IssuerSigningKey = new SymmetricSecurityKey(
                                    Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
                            };
                        });
        //builder.Services.AddSpaStaticFiles(configuration =>
        //{
        //    configuration.RootPath = "../React.Front/build";



        //});


        var app = builder.Build();
      //  app.UseSpaStaticFiles(new StaticFileOptions { RequestPath = "/build" });
        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        
        
        app.UseStaticFiles();
        app.UseRouting();
        app.UseDefaultFiles();
        app.UseCors(MyAllowSpecificOrigins);

        app.UseAuthentication();
        app.UseAuthorization();



        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapFallbackToController("Index", "Fallback");
        });
  
 
       app.MapControllers();

        app.UseHttpsRedirection();
 





        app.Run();
    }
}