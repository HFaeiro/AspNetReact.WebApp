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
using TeamManiacs.Core.Convertors;
using Microsoft.OpenApi.Models;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.ResponseCompression;
using System.Net.WebSockets;
using TeamManiacs.Core.Models;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using ASP.Back.Libraries;

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
        builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(x =>
        {
            x.ValueLengthLimit = int.MaxValue;
            x.MultipartBodyLengthLimit = int.MaxValue; // In case of multipart
        });
        //Add services to the container.
#if !DEBUG
                       string appsettings = "appsettings.Production.json";
                
#else
        string appsettings = "appsettings.Development.json";
#endif
        builder.Services.AddDbContext<TeamManiacsDbContext>(options =>
        {
             IConfiguration config = new ConfigurationBuilder()
                                        .AddJsonFile(appsettings, optional: false)
                                        .Build();
            //options.UseSqlServer(config.GetConnectionString("ASPBackContext"));
#if !DEBUG
                var connectString = config.GetConnectionString("SQLDB");
#else
            var connectString = config.GetConnectionString("DEVSQLDB");
#endif
            options.UseMySql(connectString, ServerVersion.AutoDetect(connectString));

        }, ServiceLifetime.Transient);
        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Mikes Video Api", Version = "v1" });
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = @"JWT Authorization header using the Bearer scheme. \r\n\r\n 
                      Enter 'Bearer' [space] and then your token in the text input below.
                      \r\n\r\nExample: 'Bearer 12345abcdef'",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement()
      {
        {
          new OpenApiSecurityScheme
          {
            Reference = new OpenApiReference
              {
                Type = ReferenceType.SecurityScheme,
                Id = "Bearer"
              },
              Scheme = "oauth2",
              Name = "Bearer",
              In = ParameterLocation.Header,

            },
            new List<string>()
          }
        });
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            c.IncludeXmlComments(xmlPath);
        });
        builder.Services.AddResponseCompression(options =>
        {
            options.Providers.Add<GzipCompressionProvider>();
            options.EnableForHttps = true;
        });
        builder.Services.AddControllers()
                        .AddJsonOptions(o =>
                        {
                            //o.JsonSerializerOptions.NumberHandling = JsonNumberHandling.AllowReadingFromString;
                            o.JsonSerializerOptions.Converters.Add(new JsonStringEnumMemberConverter());
                            o.JsonSerializerOptions.Converters.Add(new BoolConvertor());
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
        
        builder.Services.AddSingleton(x =>
            new Emailer(new ConfigurationBuilder()
                                        .AddJsonFile(appsettings, optional: false)
                                        .Build(), builder.Environment));

        builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);
        var app = builder.Build();
        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
            
         }
        else
        {
            app.UseHsts();
        }
        app.UseHttpsRedirection();
        app.UseResponseCompression();
        

        app.UseStaticFiles();
        app.UseRouting();
        app.UseDefaultFiles();
        app.UseCors(MyAllowSpecificOrigins);

        app.UseAuthentication();
        app.UseAuthorization();



        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Fallback}/{action=Index}/{id?}")
                    .WithDisplayName("Default Controller Route")
                    .WithMetadata("Custom.. but default metadata"); ;

            endpoints.MapFallbackToController("Index", "Fallback")
                .WithDisplayName("Fallback route")
                .WithMetadata("Custom metadata");
        });


        app.MapControllers();


        //app.MapWhen(x => !x.Request.Path.Value.StartsWith("/api"), builder =>
        //{
        //    builder.Run(async context => {
        //       var index =  Path.Combine(Directory.GetCurrentDirectory(),
        //        "wwwroot", "index.html");

        //        await context.Response.WriteAsync("Not API call");
        //    }
        //    );

       
        //});




        app.Run();
    }
}