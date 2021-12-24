namespace DatabaseLibrary.Data
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Person")]
    public partial class Person
    {
        [JsonProperty(PropertyName = "I")]
        public Guid PersonId { get; set; }

        [Required]
        [StringLength(300)]
        [JsonProperty(PropertyName = "N")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "D")]
        public string UserData { get; set; }

   //     public virtual ICollection<PicturePerson> PicturePersons { get; set; }
    }
}
