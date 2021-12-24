namespace DatabaseLibrary.Data
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("PictureFile")]
    public partial class PictureFile
    {
        [Key]
        [JsonProperty(PropertyName = "I")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [JsonProperty(PropertyName = "F")]
        public string FilePath { get; set; }

        [JsonProperty(PropertyName = "A")]
        public DateTime DateAdded { get; set; }

        [JsonProperty(PropertyName = "T")]
        public DateTime? DateTaken { get; set; }

        [JsonProperty(PropertyName = "C")]
        public bool IsConfirmed { get; set; }
        
        //[JsonIgnore]
        //public virtual ICollection<PicturePerson> PicturePersons { get; set; }
        
  //      public virtual ICollection<PictureFileGroupLookup> PictureFileGroupLookups { get; set; }

    }
}
