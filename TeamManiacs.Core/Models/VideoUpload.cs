// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeamManiacs.Core.Models
{
    public partial class VideoUpload
    {
        public string uploadId       { get; set; }
        public string videoDuration  { get; set; }
        public string videoHeight    { get; set; }
        public string videoWidth     { get; set; }
        public string chunkCount     { get; set; }
        public string chunkNumber    { get; set; }
        public IFormFile file        { get; set; }

    }
    public partial class VideoBlob
    {
        public VideoBlob() { }
       public VideoBlob(VideoUpload videoUpload) {
            int outChunkCount = 0;
            int outChunkNumber = 0;
            int.TryParse(videoUpload.chunkCount, out outChunkCount);
            int.TryParse(videoUpload.chunkNumber, out outChunkNumber);

            if(!string.IsNullOrEmpty(videoUpload.uploadId) && videoUpload.uploadId.Length == 36)
            {
                uploadId = new Guid(videoUpload.uploadId);
            }
            chunkCount = outChunkCount;
            chunkNumber = outChunkNumber;

            using (var stream = new System.IO.MemoryStream())
            {
                videoUpload.file.CopyTo(stream);
                file = stream.ToArray();
            }
        }


        [Key]
        public Guid? uploadId       { get; set; }
        public int chunkCount { get; set; }
        public int chunkNumber { get; set; }
        public byte[] file  { get; set; }

    }

}
