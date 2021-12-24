namespace StandardDatabaseLibrary.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("PicturePerson")]
    public partial class PicturePerson
    {
        public int Id { get; set; }

        public int PictureFileId { get; set; }

        [Required]
        [StringLength(50)]
        public string LargePersonGroupId { get; set; }

        public Guid? PersonId { get; set; }

        public DateTime DateAdded { get; set; }

        public string FaceJSON { get; set; }

        public bool IsConfirmed { get; set; }

        public virtual PictureFile PictureFile { get; set; }
        public virtual Person Person { get; set; }
    }
}
