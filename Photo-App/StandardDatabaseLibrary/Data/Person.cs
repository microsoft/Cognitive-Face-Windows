namespace StandardDatabaseLibrary.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Person")]
    public partial class Person
    {
        public Guid PersonId { get; set; }

        [Required]
        [StringLength(300)]
        public string Name { get; set; }

        public string UserData { get; set; }

        public virtual ICollection<PicturePerson> PicturePersons { get; set; }
    }
}
