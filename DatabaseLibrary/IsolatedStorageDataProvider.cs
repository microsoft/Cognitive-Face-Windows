using DatabaseLibrary.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseLibrary
{
    public class IsolatedStorageDataProvider : IDataProvider
    {
        private static IsolatedStorageDatabase _db;

        public IsolatedStorageDataProvider()
        {
            _db = IsolatedStorageDatabase.GetInstance();
        }

        public void AddFile(PictureFile file, string groupId, int processingState)
        {
            var lastFile = _db.PictureFiles.LastOrDefault();
            if (lastFile != null)
            {
                file.Id = lastFile.Id + 1; // Yuk, manual indexing rush
            }
            else
            {
                file.Id = 1; // index starting from 1
            }
            
            _db.PictureFiles.Add(file);
            _db.SaveChanges();

            _db.PictureFileGroupLookups.Add(new PictureFileGroupLookup { LargePersonGroupId = groupId, PictureFileId = file.Id, ProcessingState = processingState });
            _db.SaveChanges();
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
            var dbFile = _db.PictureFiles.SingleOrDefault(a=>a.Id == fileId);
            _db.PictureFiles.Remove(dbFile);
            _db.SaveChanges();
        }

        public void RemovePerson(int personId, int fileId)
        {
            var dbPerson = _db.PicturePersons.SingleOrDefault(a => a.Id == fileId);
            _db.PicturePersons.Remove(dbPerson);
            _db.SaveChanges();
        }

        public int GetFileCountForPersonId(Guid personId)
        {
            var peops = _db.PicturePersons.Where(a => a.PersonId == personId);
            return peops.Count();
        }

        public List<PicturePerson> GetFilesForPersonId(Guid personId)
        {
            var peops = _db.PicturePersons.Where(a => a.PersonId == personId).ToList();
            foreach(var person in peops)
            {
                person.PictureFile = _db.PictureFiles.Find(a => a.Id == person.PictureFileId);
                person.Person = _db.People.Find(a => a.PersonId == person.PersonId);
            }

            return peops;
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
            var persons = _db.PicturePersons.Where(a => a.LargePersonGroupId == groupId);
            foreach(var p in persons)
            {
                _db.PicturePersons.Remove(p);
            }
            _db.SaveChanges();
        }

    }
}