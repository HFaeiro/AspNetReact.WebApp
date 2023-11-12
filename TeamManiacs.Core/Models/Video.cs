// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace TeamManiacs.Core.Models
{
    [Table("Videos")]
    public partial class Video
    {
        public Video()
        {

        }
        public Video(VideoUpload videoIn, int uploader, string fileName = "")
        {

            FileName = fileName == "" ? videoIn.File.FileName : fileName;
            Title = videoIn.File.FileName.Split('_')[0];
            if (Title?.Length < 0)
                Title = videoIn.File.FileName;
            ContentSize = (int)videoIn.File.OpenReadStream().Length;
            Description = videoIn.File.ContentDisposition != null ? videoIn.File.ContentDisposition : " ";
            ContentType = videoIn.File.ContentType != null ? videoIn.File.ContentType : " ";
            ContentDisposition = videoIn.File.ContentDisposition != null ? videoIn.File.ContentDisposition : " ";
            Uploader = uploader;
            isPrivate = true;
        }
        [NotMapped]
        public string VideoName
        {
            get
            {
                return Path.GetFileNameWithoutExtension(FileName);
            }
        }

        [Key]
        public int ID { get; set; }
        public string GUID { get; set; }
        public int ContentSize { get; set; }
        public bool isPrivate { get; set; }
        public string FileName { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ContentType { get; set; }
        public string ContentDisposition { get; set; }
        public int Uploader { get; set; }
        public int VideoLength { get; set; }
        public ICollection<VideoRating>? Ratings { get; set; }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented);
        }
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class Video {\n");
            sb.Append("  Id: ").Append(ID).Append("\n");
            sb.Append("  ContentSize: ").Append(ContentSize).Append("\n");
            sb.Append("  IsPrivate: ").Append(isPrivate).Append("\n");
            sb.Append("  FileName: ").Append(FileName).Append("\n");
            sb.Append("  Title: ").Append(Title).Append("\n");
            sb.Append("  Description: ").Append(Description).Append("\n");
            sb.Append("  ContentType: ").Append(ContentType).Append("\n");
            sb.Append("  ContentDisposition: ").Append(ContentDisposition).Append("\n");
            sb.Append("  Uploader: ").Append(Uploader).Append("\n");
            sb.Append("  Ratings: ").Append(Ratings).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }
        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                var hashCode = 41;
                // Suitable nullity checks etc, of course :)

                    hashCode = hashCode * 59 + ID.GetHashCode();

                    hashCode = hashCode * 59 + ContentSize.GetHashCode();

                    hashCode = hashCode * 59 + isPrivate.GetHashCode();
                if (FileName != null)
                    hashCode = hashCode * 59 + FileName.GetHashCode();
                if (Title != null)
                    hashCode = hashCode * 59 + Title.GetHashCode();
                if (Description != null)
                    hashCode = hashCode * 59 + Description.GetHashCode();
                if (ContentType != null)
                    hashCode = hashCode * 59 + ContentType.GetHashCode();
                if (ContentDisposition != null)
                    hashCode = hashCode * 59 + ContentDisposition.GetHashCode();

                    hashCode = hashCode * 59 + Uploader.GetHashCode();
                if (Ratings != null)
                    hashCode = hashCode * 59 + Ratings.GetHashCode();
                return hashCode;
            }
        }
        /// <summary>
        /// Returns true if objects are equal
        /// </summary>
        /// <param name="obj">Object to be compared</param>
        /// <returns>Boolean</returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((Video)obj);
        }

        /// <summary>
        /// Returns true if Video instances are equal
        /// </summary>
        /// <param name="other">Instance of Video to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(Video other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return
                (
                    ID == other.ID ||
                    ID.Equals(other.ID)
                ) &&
                (
                    ContentSize == other.ContentSize ||
                    ContentSize.Equals(other.ContentSize)
                ) &&
                (
                    isPrivate == other.isPrivate ||
                    isPrivate.Equals(other.isPrivate)
                ) &&
                (
                    FileName == other.FileName ||
                    FileName != null &&
                    FileName.Equals(other.FileName)
                ) &&
                (
                    Title == other.Title ||
                    Title != null &&
                    Title.Equals(other.Title)
                ) &&
                (
                    Description == other.Description ||
                    Description != null &&
                    Description.Equals(other.Description)
                ) &&
                (
                    ContentType == other.ContentType ||
                    ContentType != null &&
                    ContentType.Equals(other.ContentType)
                ) &&
                (
                    ContentDisposition == other.ContentDisposition ||
                    ContentDisposition != null &&
                    ContentDisposition.Equals(other.ContentDisposition)
                ) &&
                (
                    Uploader == other.Uploader ||
                    Uploader.Equals(other.Uploader)
                ) &&
                (
                    Ratings == other.Ratings ||
                    Ratings != null && other.Ratings != null &&
                    Ratings.SequenceEqual(other.Ratings)
                );
        }
        public static bool operator ==(Video? left, Video? right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Video? left, Video? right)
        {
            return !Equals(left, right);
        }
    }



}
