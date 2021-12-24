using DatabaseLibrary;
using DatabaseLibrary.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseLibrary
{
    public class DataProviderManager
    {
        private readonly string _isolatedStorageDatabaseTypeFileName = "DatabaseType.txt";
        private readonly string _isolatedStorageConnectionStringFileName = "ConnectionString.txt";
        private readonly string _defaultConnectionString = @"data source=.\sqlexpress;initial catalog=PhotosDatabase;integrated security=True;MultipleActiveResultSets=True;App=EntityFramework";
        
        private static IDataProvider _currentSingleton = null;
        private static DataSourceType? _dataSourceType;
        private static string _connectionString;
        
        private DataSourceType GetDatabaseTypeFromIsolatedStorage()
        {
            DataSourceType databaseType;

            using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null))
            {
                try
                {
                    using (var iStreamForEndpoint = new IsolatedStorageFileStream(_isolatedStorageDatabaseTypeFileName, FileMode.Open, isoStore))
                    {
                        using (var readerForEndpoint = new StreamReader(iStreamForEndpoint))
                        {
                            databaseType = (DataSourceType)Enum.Parse(typeof(DataSourceType),readerForEndpoint.ReadLine());
                        }
                    }
                }
                catch (FileNotFoundException)
                {
                    databaseType = DataSourceType.LocalIsolatedStorage;
                }
            }
            return databaseType;
        }
        
        private void SaveDatabaseTypeToIsolatedStorage(DataSourceType databaseType)
        {
            using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null))
            {
                using (var oStream = new IsolatedStorageFileStream(_isolatedStorageDatabaseTypeFileName, FileMode.Create, isoStore))
                {
                    using (var writer = new StreamWriter(oStream))
                    {
                        writer.WriteLine(databaseType);
                    }
                }
            }
        }

        private void SaveConnectionStringToIsolatedStorage(string connectionString)
        {
            using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null))
            {
                using (var oStream = new IsolatedStorageFileStream(_isolatedStorageConnectionStringFileName, FileMode.Create, isoStore))
                {
                    using (var writer = new StreamWriter(oStream))
                    {
                        writer.WriteLine(connectionString);
                    }
                }
            }
        }

        private string GetConnectionStringToIsolatedStorage()
        {
            string connectionString = null;

            using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null))
            {
                try
                {
                    using (var iStreamForEndpoint = new IsolatedStorageFileStream(_isolatedStorageConnectionStringFileName, FileMode.Open, isoStore))
                    {
                        using (var readerForEndpoint = new StreamReader(iStreamForEndpoint))
                        {
                            connectionString = readerForEndpoint.ReadLine();
                        }
                    }
                }
                catch (FileNotFoundException)
                {
                    connectionString = null;
                }
            }
            if (string.IsNullOrEmpty(connectionString))
            {
                connectionString = _defaultConnectionString;
            }
            return connectionString;
        }
        
        public DataSourceType DataSourceType
        {
            get
            {
                if (_dataSourceType == null)
                {
                    _dataSourceType = GetDatabaseTypeFromIsolatedStorage();
                }
                return _dataSourceType.Value;
            }
            set
            {
                _dataSourceType = value;
                SaveDatabaseTypeToIsolatedStorage(value);
            }
        }

        public string ConnectionString
        {
            get
            {
                if (_connectionString == null)
                {
                    _connectionString = GetConnectionStringToIsolatedStorage();
                }
                return _connectionString;
            }
            set
            {
                _connectionString = value;
                SaveConnectionStringToIsolatedStorage(value);
            }
        }

        public static IDataProvider Current
        {
            get
            {
                if (_currentSingleton == null)
                {
                    switch(_dataSourceType)
                    {
                        case DataSourceType.SqlConnectionString:
                            _currentSingleton = new SqlDataProvider("name=PhotosDatabase");
                            break;
                        case DataSourceType.LocalIsolatedStorage:
                            _currentSingleton = new IsolatedStorageDataProvider();
                            break;
                    }
                }

                return _currentSingleton;
            }
        }

        public void ReinitialiseDatabase()
        {
            _currentSingleton = null;
        }
    }
}
