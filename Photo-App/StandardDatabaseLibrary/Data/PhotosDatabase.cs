namespace StandardDatabaseLibrary.Data
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using System.Runtime.Remoting.Contexts;

    public partial class PhotosDatabase : Context
    {
        public PhotosDatabase()
            : base("name=PhotosDatabase")
        {
        }

        public virtual DbSet<Person> People { get; set; }
        public virtual DbSet<PictureFile> PictureFiles { get; set; }
        public virtual DbSet<PictureFileGroupLookup> PictureFileGroupLookups { get; set; }
        public virtual DbSet<PicturePerson> PicturePersons { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PictureFileGroupLookup>()
                .Property(e => e.LargePersonGroupId)
                .IsUnicode(false);
        }
    }
}
