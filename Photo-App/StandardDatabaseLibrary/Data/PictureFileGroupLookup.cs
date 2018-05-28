namespace StandardDatabaseLibrary.Data
{
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
        public int PictureFileId { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(50)]
        public string LargePersonGroupId { get; set; }

        public int ProcessingState { get; set; }

        public virtual PictureFile PictureFile { get; set; }
    }
}
