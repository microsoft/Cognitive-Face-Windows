using StandardDatabaseLibrary.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StandardDatabaseLibrary
{
    public class SqlDataProvider : IDataProvider
    {
        PhotosDatabase db = new PhotosDatabase();

        public void AddFile(PictureFile file, string groupId, int processingState)
        {
            var lookup = new PictureFileGroupLookup
            {
                LargePersonGroupId = groupId,
                ProcessingState = processingState,
            };

            lookup.PictureFile = file;
            db.PictureFileGroupLookups.Add(lookup);
            db.SaveChanges();


            //db.PictureFiles.Add(file);
            //db.SaveChanges();
            //db.PictureFileGroupLookups.Add(new PictureFileGroupLookup { LargePersonGroupId = groupId, PictureFileId =  })
        }

        public PictureFile GetFile(string path)
        {
            return db.PictureFiles.Where(a => a.FilePath == path).SingleOrDefault();
        }

        public void AddPerson(PicturePerson person)
        {
            db.PicturePersons.Add(person);
            db.SaveChanges();
        }

        public void RemoveFile(int fileId)
        {
            var dbFile = db.PictureFiles.Find(fileId);
            db.PictureFiles.Remove(dbFile);
            db.SaveChanges();
        }

        public void RemovePerson(int personId, int fileId)
        {
            var dbPerson = db.PicturePersons.Find(fileId);
            db.PicturePersons.Remove(dbPerson);
            db.SaveChanges();
        }

        public int GetFileCountForPersonId(Guid personId)
        {
            return db.PicturePersons.Where(a => a.PersonId == personId).Count();
        }

        public List<PicturePerson> GetFilesForPersonId(Guid personId)
        {
            return db.PicturePersons.Where(a => a.PersonId == personId).ToList();
        }

        public Person GetPerson(Guid personId)
        {
            return db.People.Where(a => a.PersonId == personId).SingleOrDefault();
        }

        public void AddPerson(Guid personId, string name, string userData)
        {
            db.People.Add(new Person { Name = name, PersonId = personId, UserData = userData });
            db.SaveChanges();
        }

        public void RemovePersonsForGroup(string groupId)
        {
            db.Database.ExecuteSqlCommand($"DELETE FROM PicturePerson WHERE LargePersonGroupId = '{groupId}'");
        }
    }
}
