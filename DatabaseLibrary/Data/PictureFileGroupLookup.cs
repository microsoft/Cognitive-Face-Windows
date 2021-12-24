namespace DatabaseLibrary.Data
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("PictureFileGroupLookup")]
    public partial class PictureFileGroupLookup
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [JsonProperty(PropertyName = "I")]
        public int PictureFileId { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(50)]
        [JsonProperty(PropertyName = "G")]
        public string LargePersonGroupId { get; set; }

        [JsonProperty(PropertyName = "S")]
        public int ProcessingState { get; set; }
        
        [JsonIgnore]
        public virtual PictureFile PictureFile { get; set; }
    }
}
