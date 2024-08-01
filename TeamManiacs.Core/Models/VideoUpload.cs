// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net.Mime;
using System.Runtime.InteropServices;

namespace TeamManiacs.Core.Models
{
    public partial class VideoUpload
    {

        public string uploadId { get; set; }
        public string videoDuration { get; set; }
        public string videoHeight { get; set; }
        public string videoWidth { get; set; }
        public string chunkCount { get; set; }
        public string chunkNumber { get; set; }
        public string contentType { get; set; }
        public IFormFile file { get; set; }

    }
    public partial class VideoBlob
    {
        public VideoBlob() { }
        public VideoBlob(VideoUpload videoUpload)
        {
            int outChunkCount = 0;
            int outChunkNumber = 0;
            float outDuration = 0;
            int.TryParse(videoUpload.chunkCount, out outChunkCount);
            int.TryParse(videoUpload.chunkNumber, out outChunkNumber);
            float.TryParse(videoUpload.videoDuration, out outDuration);
            if (!string.IsNullOrEmpty(videoUpload.uploadId) && videoUpload.uploadId.Length == 36)
            {
                uploadId = new Guid(videoUpload.uploadId);
            }

            chunkCount = outChunkCount;
            chunkNumber = outChunkNumber;
            videoDuration = outDuration;
            videoHeight = videoUpload.videoHeight;
            videoWidth = videoUpload.videoWidth;
            videoName = videoUpload.file.FileName;
            ContentType = videoUpload.contentType;
            ContentDisposition = videoUpload.file.ContentDisposition;

            using (MemoryStream stream = new System.IO.MemoryStream())
            {
                videoUpload.file.CopyTo(stream);
                file = stream.ToArray();
            };

           

        }


        [Key]
        public Guid uploadId { get; set; }
        public int chunkCount { get; set; }
        public int chunkNumber { get; set; }
        public float videoDuration { get; set; }
        public string videoHeight { get; set; }
        public string videoWidth { get; set; }
        public string videoName { get; set; }
        public string ContentType { get; set; }
        public string ContentDisposition { get; set; }
        public byte[] file { get; set; }
        

    }

}
