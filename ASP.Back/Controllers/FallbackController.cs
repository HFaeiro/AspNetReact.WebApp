using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using System.Text.RegularExpressions;
using TeamManiacs.Core.Models;
using TeamManiacs.Data;

namespace ASP.Back.Controllers
{
    public class FallbackController : Controller
    {
        private readonly IWebHostEnvironment hostEnvironment;
        private readonly TeamManiacsDbContext _context;
        public FallbackController(IWebHostEnvironment hostEnvironment, TeamManiacsDbContext context)
        {
            this.hostEnvironment = hostEnvironment;
            this._context = context;
        }

        public IActionResult Index()
        {
            return PhysicalFile(Path.Combine(Directory.GetCurrentDirectory(),
                "wwwroot", "index.html"),
                MediaTypeNames.Text.Html);
        }
        [Route("/play/{id}"), Route("/videoapp/play/{id}")]
        public IActionResult Play(int id)
        {

            Video? video = _context.Videos.Find(id);
            if (video != null)
            {
                if (video.isPrivate)
                    return PhysicalFile(Path.Combine(Directory.GetCurrentDirectory(),
                    "wwwroot", "index.html"),
                    MediaTypeNames.Text.Html);

                string cssFile =  GetNewestFileOfTypeWithPatternInDirectory("*.css", "main" ,Path.Combine(Directory.GetCurrentDirectory(),
                        "wwwroot/static/css/"));
                string jsFile = GetNewestFileOfTypeWithPatternInDirectory("*.js","main",Path.Combine(Directory.GetCurrentDirectory(),
                        "wwwroot/static/js/"));



                string strIndex = $"<!DOCTYPE html>\r\n<html lang=\"en\">\r\n<head>\r\n    " +


                $"<meta name=\"theme-color\"                        content=\"rgba(33, 33, 33, 0.98)\">\r\n\r\n" +
                $"<meta name=\"title\"                              content=\"{video.Title}\">\r\n" +
                $"<meta name=\"description\"                        content=\"{video.Description}\">\r\n" +
                $"<meta property=\"og:site_name\"                   content=\"Aeirosoft\">\r\n" +
                $"<meta property=\"og:url\"                         content=\"https://aeirosoft.com/api/video/play/{id}\">\r\n" +
                $"<meta property=\"og:title\"                       content=\"{video.Title}\">\r\n" +
                $"<meta property=\"og:image\"                       content=\"https://aeirosoft.com/AeiroSoftLogoInitials.png\">\r\n" +
                $"<meta property=\"og:image:width\"                 content=\"1280\">\r\n" +
                $"<meta property=\"og:image:height\"                content=\"720\">\r\n" +
                $"<meta property=\"og:description\"                 content=\"{video.Description}\">\r\n" +
                $"<meta property=\"al:web:url\"                     content=\"https://aeirosoft.com/api/video/play/{id}\">\r\n" +
                $"<meta property=\"og:type\"                        content=\"video.other\">\r\n" +
                $"<meta property=\"og:video:url\"                   content=\"https://aeirosoft.com/api/video/play/{id}\">\r\n" +
                $"<meta property=\"og:video:secure_url\"            content=\"https://aeirosoft.com/api/video/play/{id}\">\r\n" +
                $"<meta property=\"og:video:type\"                  content=\"video/mp4\">\r\n" +
                $"<meta property=\"og:video:width\"                 content=\"1280\">\r\n" +
                $"<meta property=\"og:video:height\"                content=\"720\">\r\n" +
                $"<meta property=\"al:android:app_name\"            content=\"Aeirosoft\">\r\n" +
                $"<meta name=\"twitter:card\"                       content=\"player\">\r\n" +
                $"<meta name=\"twitter:site\"                       content=\"@Aeirosoft\">\r\n" +
                $"<meta name=\"twitter:url\"                        content=\"https://aeirosoft.com/\">\r\n" +
                $"<meta name=\"twitter:title\"                      content=\"{video.Title}\">\r\n" +
                $"<meta name=\"twitter:description\"                content=\"{video.Description}\">\r\n" +
                $"<meta name=\"twitter:image\"                      content=\"https://aeirosoft.com/AeiroSoftLogoInitials.png\">\r\n" +
                $"<meta name=\"twitter:player\"                     content=\"https://aeirosoft.com/api/video/play/{id}\">\r\n" +
                $"<meta name=\"twitter:player:width\"               content=\"1280\">\r\n" +
                $"<meta name=\"twitter:player:height\"              content=\"720\">" +
                $"<meta name=\"twitter:player:stream\"              content=\"https://aeirosoft.com/api/video/play/{id}\"\r\n" +
                $"<meta name=\"twitter:player:stream:content_type\" content=\"video/mp4\" />" +


                $"<link rel=\"manifest\" href=\"/manifest.json\" />\r\n\r\n    " +

                $"<title>{video.Title}</title>\r\n    " +

                $"<link rel=\"stylesheet\" href=\"https://cdn.jsdelivr.net/npm/bootstrap@5.2.3/dist/css/bootstrap.min.css\" integrity=\"sha384-rbsA2VBKQhggwzxH7pPCaAqO46MgnOM80zW1RWuH61DGLwZJEdK2Kadq2F9CUG65\" crossorigin=\"anonymous\">" +

                $"<script defer=\"defer\" src=\"/static/js/{jsFile}\"></script>" +

                $"<link href=\"/static/css/{cssFile}\" rel=\"stylesheet\">" +
                
                $"\r\n</head>\r\n " +

                $"<body>\r\n    " +

                $"<noscript>You need to enable JavaScript to run this app.</noscript>\r\n    " +

                $"<div id=\"root\"></div>\r\n\r\n  " +

                $"</body>\r\n" +

                $"</html>";

                Console.WriteLine($"Writing Video Meta Data For Video {video.Title}");

                return Content(strIndex, "text/html");








            }
            return PhysicalFile(Path.Combine(Directory.GetCurrentDirectory(),
                "wwwroot", "index.html"),
                MediaTypeNames.Text.Html);
        }
        private string GetNewestFileOfTypeWithPatternInDirectory(string type, string pattern, string path) {
            var d = new DirectoryInfo(path);//Directory
            var files = d.GetFiles(type, SearchOption.AllDirectories);//Get .txt files
            var matching = files.OrderByDescending(x => x.CreationTimeUtc)
                                   .Where(f => f.FullName.Contains(pattern));

            var latest = matching.First();
            return latest.Name;

        }
    }
}
