namespace DatabaseLibrary.Data
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("PicturePerson")]
    public partial class PicturePerson
    {
        [Key]
        [JsonProperty(PropertyName = "I")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [JsonProperty(PropertyName = "F")]
        public int PictureFileId { get; set; }

        [Required]
        [StringLength(50)]
        [JsonProperty(PropertyName = "G")]
        public string LargePersonGroupId { get; set; }

        [JsonProperty(PropertyName = "P")]
        public Guid? PersonId { get; set; }

        [JsonProperty(PropertyName = "A")]
        public DateTime DateAdded { get; set; }

        [JsonProperty(PropertyName = "J")]
        public string FaceJSON { get; set; }

        [JsonProperty(PropertyName = "C")]
        public bool IsConfirmed { get; set; }

        [JsonIgnore]
        public virtual PictureFile PictureFile { get; set; }

        [JsonIgnore]
        public virtual Person Person { get; set; }
    }
}
