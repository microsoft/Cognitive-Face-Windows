using DatabaseLibrary.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseLibrary
{
    public class SqlDataProvider : IDataProvider
    {
        PhotosDatabase _db;

        public SqlDataProvider(string connectionString)
        {
            _db = new PhotosDatabase(connectionString);
        }

        public void AddFile(PictureFile file, string groupId, int processingState)
        {
            var lookup = new PictureFileGroupLookup
            {
                LargePersonGroupId = groupId,
                ProcessingState = processingState,
            };

            lookup.PictureFile = file;
            _db.PictureFileGroupLookups.Add(lookup);
            _db.SaveChanges();


            //db.PictureFiles.Add(file);
            //db.SaveChanges();
            //db.PictureFileGroupLookups.Add(new PictureFileGroupLookup { LargePersonGroupId = groupId, PictureFileId =  })
        }

        public PictureFile GetFile(string path)
        {
            return _db.PictureFiles.Where(a => a.FilePath == path).SingleOrDefault();
        }

        public void AddPerson(PicturePerson person)
        {
            _db.PicturePersons.Add(person);
            _db.SaveChanges();
        }

        public void RemoveFile(int fileId)
        {
            var dbFile = _db.PictureFiles.Find(fileId);
            _db.PictureFiles.Remove(dbFile);
            _db.SaveChanges();
        }

        public void RemovePerson(int personId, int fileId)
        {
            var dbPerson = _db.PicturePersons.Find(fileId);
            _db.PicturePersons.Remove(dbPerson);
            _db.SaveChanges();
        }

        public int GetFileCountForPersonId(Guid personId)
        {
            return _db.PicturePersons.Where(a => a.PersonId == personId).Count();
        }

        public List<PicturePerson> GetFilesForPersonId(Guid personId)
        {
            return _db.PicturePersons.Where(a => a.PersonId == personId).ToList();
        }

        public Person GetPerson(Guid personId)
        {
            return _db.People.Where(a => a.PersonId == personId).SingleOrDefault();
        }

        public void AddPerson(Guid personId, string name, string userData)
        {
            _db.People.Add(new Person { Name = name, PersonId = personId, UserData = userData });
            _db.SaveChanges();
        }

        public void RemovePersonsForGroup(string groupId)
        {
            _db.Database.ExecuteSqlCommand($"DELETE FROM PicturePerson WHERE LargePersonGroupId = '{groupId}'");
        }
    }
}
