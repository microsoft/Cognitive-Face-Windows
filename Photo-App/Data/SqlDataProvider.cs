//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
//
// Microsoft Cognitive Services (formerly Project Oxford): https://www.microsoft.com/cognitive-services
//
// Microsoft Cognitive Services (formerly Project Oxford) GitHub:
// https://github.com/Microsoft/Cognitive-Face-Windows
//
// Copyright (c) Microsoft Corporation
// All rights reserved.
//
// MIT License:
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using Microsoft.ProjectOxford.Face.Contract;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Photo_Detect_Catalogue_Search_WPF_App.Data
{
    public class SqlDataProvider : IDataProvider
    {
        PhotosDatabase db = new PhotosDatabase();

        public void AddFile(PictureFile file, string groupId)
        {
            var lookup = new PictureFileGroupLookup
            {
                LargePersonGroupId = groupId,
                ProcessingState = 2,
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
