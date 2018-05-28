namespace DatabaseLibrary.Data
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class PhotosDatabase : DbContext
    {
        public PhotosDatabase(string nameOrConnectionString)
            : base(nameOrConnectionString)
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

            modelBuilder.Entity<PictureFileGroupLookup>()
                .HasKey(l => new { l.PictureFileId, l.LargePersonGroupId });

            modelBuilder.Entity<PicturePerson>()
                .HasKey(l => l.Id);
        }
    }
}
