using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseLibrary.Data
{
    public interface IDataProvider
    {
        void AddFile(PictureFile file, string groupId, int processingState);

        PictureFile GetFile(string path);

        void RemoveFile(int fileId); // Must also remove any persons associated with the file

        void AddPerson(PicturePerson person);

        void RemovePerson(int personId, int fileId);

        int GetFileCountForPersonId(Guid personId);

        Person GetPerson(Guid personId);

        void AddPerson(Guid personId, string name, string userData);

        void RemovePersonsForGroup(string groupId);

        List<PicturePerson> GetFilesForPersonId(Guid personId);
    }
}
