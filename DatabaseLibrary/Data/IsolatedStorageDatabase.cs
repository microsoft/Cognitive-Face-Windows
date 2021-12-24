using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseLibrary.Data
{
    public class IsolatedStorageDatabase
    {
        private static readonly string _isolatedStorageDatabaseFileName = "Database.json";

        private static IsolatedStorageDatabase _db;

        [JsonProperty(PropertyName = "P")]
        public virtual List<Person> People { get; set; }
        [JsonProperty(PropertyName = "PF")]
        public virtual List<PictureFile> PictureFiles { get; set; }
        [JsonProperty(PropertyName = "PL")]
        public virtual List<PictureFileGroupLookup> PictureFileGroupLookups { get; set; }
        [JsonProperty(PropertyName = "PP")]
        public virtual List<PicturePerson> PicturePersons { get; set; }

        internal void SaveChanges()
        {
            saveDatabaseToIsolatedStorage(_db);
        }

        public IsolatedStorageDatabase()
        {
        }

        public static IsolatedStorageDatabase GetInstance()
        { 
            if (_db == null)
            {
                _db = getDatabaseFromIsolatedStorage();
            }

            if (_db == null)
            {
                var d = new IsolatedStorageDatabase();
                d.People = new List<Person>();
                d.PictureFiles = new List<PictureFile>();
                d.PictureFileGroupLookups = new List<PictureFileGroupLookup>();
                d.PicturePersons = new List<PicturePerson>();

                _db = d;
                saveDatabaseToIsolatedStorage(d);
            }

            return _db;
        }

        public static void saveDatabaseToIsolatedStorage(IsolatedStorageDatabase database)
        {
            using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null))
            {
                using (var oStream = new IsolatedStorageFileStream(_isolatedStorageDatabaseFileName, FileMode.Create, isoStore))
                {
                    using (var writer = new StreamWriter(oStream))
                    {
                        writer.Write(JsonConvert.SerializeObject(database));
                    }
                }
            }
        }

        private static IsolatedStorageDatabase getDatabaseFromIsolatedStorage()
        {
            IsolatedStorageDatabase database = null;

            try
            {
                using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null))
                {
                    using (var iStreamForEndpoint = new IsolatedStorageFileStream(_isolatedStorageDatabaseFileName, FileMode.Open, isoStore))
                    {
                        using (var readerForEndpoint = new StreamReader(iStreamForEndpoint))
                        {
                            var json = readerForEndpoint.ReadToEnd();
                            database = JsonConvert.DeserializeObject<IsolatedStorageDatabase>(json, new JsonSerializerSettings { Error = deserialiseErrorEventHandler });
                        }
                    }
                }
            }
            catch (FileNotFoundException)
            {
                database = null;
            }

            return database;
        }

        private static void deserialiseErrorEventHandler(object sender, Newtonsoft.Json.Serialization.ErrorEventArgs e)
        {
            
        }

        public string GetDatabaseLocation()
        {
            using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null))
            {
                return isoStore.GetType().GetField("m_RootDir", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(isoStore).ToString();
            }
            
        }

        // v2 use Isolated Storage like Table STorage ;)
        //private void AddRow(string tableName, string alphaNumRowKey, object row)
        //{
        //}
    }
}
