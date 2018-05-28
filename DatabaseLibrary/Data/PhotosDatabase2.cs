namespace DatabaseLibrary.Data
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using System.IO.IsolatedStorage;
    using System.IO;
    using Newtonsoft.Json;

    public partial class PhotosDatabase2 : DbContext
    {
        private static readonly string _isolatedStorageDatabaseFileName = "Database2.txt";

        public PhotosDatabase2() : base()
        {
        }

        public override int SaveChanges()
        {
            saveDatabaseToIsolatedStorage(this);
            return 1;
        }

        public static void saveDatabaseToIsolatedStorage(PhotosDatabase2 database)
        {
            using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null))
            {
                using (var oStream = new IsolatedStorageFileStream(_isolatedStorageDatabaseFileName, FileMode.Create, isoStore))
                {
                    using (var writer = new StreamWriter(oStream))
                    {
                        var settings = new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate };
                        writer.Write(JsonConvert.SerializeObject(database, settings));
                    }
                }
            }
        }

        private static PhotosDatabase2 getDatabaseFromIsolatedStorage()
        {
            PhotosDatabase2 database = null;

            using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null))
            {
                try
                {
                    using (var iStreamForEndpoint = new IsolatedStorageFileStream(_isolatedStorageDatabaseFileName, FileMode.Open, isoStore))
                    {
                        using (var readerForEndpoint = new StreamReader(iStreamForEndpoint))
                        {
                            var json = readerForEndpoint.ReadToEnd();
                            try
                            {
                                database = JsonConvert.DeserializeObject<PhotosDatabase2>(json);
                            }
                            catch (Exception e)
                            {

                            }
                        }
                    }
                }
                catch (FileNotFoundException)
                {
                    database = null;
                }
            }
            return database;
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
