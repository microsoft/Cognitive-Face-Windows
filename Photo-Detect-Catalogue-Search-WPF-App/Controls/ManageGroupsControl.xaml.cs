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

namespace Photo_Detect_Catalogue_Search_WPF_App.Controls
{
    using Microsoft.ProjectOxford.Face;
    using Microsoft.ProjectOxford.Face.Contract;
    using Photo_Detect_Catalogue_Search_WPF_App.Helpers;
    using Photo_Detect_Catalogue_Search_WPF_App.Models;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Input;

    /// <summary>
    /// Interaction logic for ManageGroupsControl.xaml
    /// </summary>
    public partial class ManageGroupsControl : UserControl, INotifyPropertyChanged
    {
        /// <summary>
        /// The number of faces to pull
        /// </summary>
        private const int _numberOfFacesToPull = 5;

        /// <summary>
        /// The face service client
        /// </summary>
        private FaceServiceClient _faceServiceClient;

        /// <summary>
        /// The main window
        /// </summary>
        private MainWindow _mainWindow;

        /// <summary>
        /// The database provider layer
        /// </summary>
        private Data.SqlDataProvider _db = new Data.SqlDataProvider();

        /// <summary>
        /// max concurrent process number for client query.
        /// </summary>
        private int _maxConcurrentProcesses;

        /// <summary>
        /// The face groups
        /// </summary>
        private ObservableCollection<LargePersonGroupExtended> _faceGroups = new ObservableCollection<LargePersonGroupExtended>();

        /// <summary>
        /// The selected faces
        /// </summary>
        private ObservableCollection<Models.Face> _selectedFaces 
            = new ObservableCollection<Models.Face>();

        /// <summary>
        /// The selected group
        /// </summary>
        private LargePersonGroupExtended _selectedGroup;

        /// <summary>
        /// The main window log trace writer
        /// </summary>
        private MainWindowLogTraceWriter _mainWindowLogTraceWriter;



        /// <summary>
        /// Gets the face groups.
        /// </summary>
        /// <value>
        /// The face groups.
        /// </value>
        public ObservableCollection<LargePersonGroupExtended> FaceGroups
        {
            get
            {
                return _faceGroups;
            }
        }

        /// <summary>
        /// Gets or sets the selected group.
        /// </summary>
        /// <value>
        /// The selected group.
        /// </value>
        public LargePersonGroupExtended SelectedGroup
        {
            get
            {
                return _selectedGroup;
            }

            set
            {
                _selectedGroup = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("SelectedGroup"));
                    GetPeopleForSelectedGroup();
                }
            }
        }

        /// <summary>
        /// Checks the people in database.
        /// </summary>
        private void CheckPeopleInDatabase()
        {
            foreach(var person in SelectedGroup.GroupPersons)
            {
                var dbPerson = _db.GetPerson(person.Person.PersonId);
                if (dbPerson == null)
                {
                    _db.AddPerson(person.Person.PersonId, person.Person.Name, person.Person.UserData);
                }
            }
        }

        /// <summary>
        /// Gets the people for selected group.
        /// </summary>
        /// <returns></returns>
        private async Task GetPeopleForSelectedGroup()
        {
            if (SelectedGroup == null)
            {
                return;
            }

            MainWindow.Log("Loading group persons...");
            SelectedGroup.GroupPersons.Clear();

            Microsoft.ProjectOxford.Face.Contract.Person[] peops = null;
            
            while(true)
            {
                try
                {
                    // ListPersonsInLargePersonGroupAsync also has skip/take overrides
                    peops = await _faceServiceClient.ListPersonsInLargePersonGroupAsync(SelectedGroup.Group.LargePersonGroupId);
                    break;
                }
                catch (Exception e)
                {
                    MainWindow.Log($"API rate limit exceeded, retrying");
                    await Task.Delay(1000);
                }
            }

            if (peops == null)
            {
                MainWindow.Log($"failed to get Persons in group");
                return;
            }

            foreach (var p in peops)
            {
                var person = new PersonExtended { Person = p };
                person.PersonFilesDbCount = _db.GetFileCountForPersonId(p.PersonId);
                this.Dispatcher.Invoke(() =>
                {
                    SelectedGroup.GroupPersons.Add(person);
                });

                // Initially loading just one, to save on API calls
                var guidList = new ConcurrentBag<Guid>(person.Person.PersistedFaceIds.Take(1)); 
                
                await GetFacesFromServerAsync(person, guidList);
            }

            MainWindow.Log("Finished loading group persons");
            CheckPeopleInDatabase();
        }

        /// <summary>
        /// Gets trained faces from the API.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="guidList">The unique identifier list.</param>
        /// <returns></returns>
        private async Task GetFacesFromServerAsync(PersonExtended person, ConcurrentBag<Guid> guidList)
        {
            var tasks = new List<Task>();
            Guid guid;
            while (guidList.TryTake(out guid))
            {
                tasks.Add(Task.Factory.StartNew((object inParams) =>
                {
                    var prm = (Tuple<PersonExtended, Guid>)inParams;
                    try
                    {
                        var face = _faceServiceClient.GetPersonFaceInLargePersonGroupAsync(SelectedGroup.Group.LargePersonGroupId, prm.Item1.Person.PersonId, prm.Item2).Result;

                        this.Dispatcher.Invoke(
                            new Action<ObservableCollection<Models.Face>, string, PersistedFace>(UIHelper.UpdateFace),
                            prm.Item1.Faces,
                            face.UserData,
                            face);
                    }
                    catch (FaceAPIException e)
                    {
                        // if operation conflict, retry.
                        if (e.ErrorCode.Equals("ConcurrentOperationConflict"))
                        {
                            guidList.Add(guid);
                        }
                    }
                    catch (Exception ex)
                    {
                        guidList.Add(guid);
                        this.Dispatcher.Invoke(() =>
                        {
                            MainWindow.Log($"Rate limit exceeded, Re-queuing in 1 second");
                            Task.Delay(1000).Wait();
                        });
                    }
                }, new Tuple<PersonExtended, Guid>(person, guid)));

                if (tasks.Count >= _maxConcurrentProcesses || guidList.IsEmpty)
                {
                    await Task.WhenAll(tasks);
                    tasks.Clear();
                }
            }

            await Task.WhenAll(tasks);
            tasks.Clear();

            return;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ManageGroupsControl"/> class.
        /// </summary>
        public ManageGroupsControl()
        {
            InitializeComponent();
            _mainWindowLogTraceWriter = new MainWindowLogTraceWriter();
            _maxConcurrentProcesses = 4;
            Loaded += ManageGroupsControl_Loaded;
        }

        /// <summary>
        /// Handles the Loaded event of the ManageGroupsControl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private async void ManageGroupsControl_Loaded(object sender, RoutedEventArgs e)
        {
            _mainWindow = Window.GetWindow(this) as MainWindow;
            string subscriptionKey = _mainWindow._scenariosControl.SubscriptionKey;
            string endpoint = _mainWindow._scenariosControl.SubscriptionEndpoint;

            _faceServiceClient = new FaceServiceClient(subscriptionKey, endpoint);

            await LoadGroups();
        }

        /// <summary>
        /// Loads the groups.
        /// </summary>
        /// <returns></returns>
        private async Task LoadGroups()
        {
            FaceGroups.Clear();

            var groups = await RetryHelper.OperationWithBasicRetryAsync(async () => await
                _faceServiceClient.ListLargePersonGroupsAsync(),
                new[] { typeof(FaceAPIException) },
                traceWriter: _mainWindowLogTraceWriter);

            foreach (var grp in groups)
            {
                FaceGroups.Add(new LargePersonGroupExtended { Group = grp });
            }

            MainWindow.Log("Found {0} groups.", groups.Length);
        }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Handles the Click event of the btnFolderScan control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnFolderScan_Click(object sender, RoutedEventArgs e)
        {
            var person = SelectedGroup.SelectedPerson;
            var ctrl = new ScanFolderControl(_selectedGroup, _mainWindow);
            var win = new PopupWindow(ctrl, $"Scan folders for matches with {_selectedGroup.Group.Name}");
            win.Show();
        }

        /// <summary>
        /// Handles the Click event of the btnAddGroup control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private async void btnAddGroup_Click(object sender, RoutedEventArgs e)
        {
            var groupId = Guid.NewGuid().ToString();

            //await faceServiceClient.CreateLargePersonGroupAsync(groupId, groupId);
            await RetryHelper.VoidOperationWithBasicRetryAsync(() =>
                _faceServiceClient.CreateLargePersonGroupAsync(groupId, groupId),
                new[] { typeof(FaceAPIException) },
                traceWriter: _mainWindowLogTraceWriter);
            
            await LoadGroups();
            SelectedGroup = FaceGroups.Where(a => a.Group.LargePersonGroupId == groupId).SingleOrDefault();
        }

        /// <summary>
        /// Handles the Click event of the btnUpdateGroup control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private async void btnUpdateGroup_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //await faceServiceClient.UpdateLargePersonGroupAsync(SelectedGroup.Group.LargePersonGroupId, SelectedGroup.Group.Name, SelectedGroup.Group.UserData);
                await RetryHelper.VoidOperationWithBasicRetryAsync(() =>
                    _faceServiceClient.UpdateLargePersonGroupAsync(SelectedGroup.Group.LargePersonGroupId, SelectedGroup.Group.Name, SelectedGroup.Group.UserData),
                    new[] { typeof(FaceAPIException) },
                    traceWriter: _mainWindowLogTraceWriter);

                MainWindow.Log($"Changes to the selected group were saved successfully");
            }
            catch (Exception ex)
            {
                MainWindow.Log($"Error updating group: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles the Click event of the btnDeleteGroup control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private async void btnDeleteGroup_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show("Are you sure you want to delete this group and database matches?", "Delete Confirmation", System.Windows.MessageBoxButton.YesNo);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                try
                {
                    await RetryHelper.VoidOperationWithBasicRetryAsync(() =>
                        _faceServiceClient.DeleteLargePersonGroupAsync(SelectedGroup.Group.LargePersonGroupId),
                        new[] { typeof(FaceAPIException) },
                        traceWriter: _mainWindowLogTraceWriter);

                    _db.RemovePersonsForGroup(SelectedGroup.Group.LargePersonGroupId);
                    
                    FaceGroups.Remove(SelectedGroup);
                    SelectedGroup = null;
                    MainWindow.Log($"Selected group deleted successfully");
                }
                catch (Exception ex)
                {
                    MainWindow.Log($"Error deleting group: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnShowFiles control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnShowFiles_Click(object sender, RoutedEventArgs e)
        {
            var person = SelectedGroup.SelectedPerson;
            var ctrl = new ShowPersonMatchedFilesControl(person);
            var win = new PopupWindow(ctrl, $"Matched files for {person.Person.Name}");
            win.Show();
        }

        private async void btnShowMore_Click(object sender, RoutedEventArgs e)
        {
            var person = SelectedGroup.SelectedPerson;
            var alreadyTaken = person.Faces.Count;
            var maxAvailable = person.Person.PersistedFaceIds.Count();
            if (alreadyTaken < maxAvailable)
            {
                var guidList = new ConcurrentBag<Guid>(person.Person.PersistedFaceIds.Skip(alreadyTaken).Take(_numberOfFacesToPull));
                await GetFacesFromServerAsync(person, guidList);
            }
            else
            {
                MainWindow.Log($"No more images stored in the service for {person.Person.Name}");
            }
        }

        /// <summary>
        /// Handles the MouseDown event of the Grid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            foreach(var p in SelectedGroup.GroupPersons)
            {
                p.IsSelected = false;
            }
            var grid = sender as Grid;
            var person = grid.DataContext as PersonExtended;
            SelectedGroup.SelectedPerson = person;
            person.IsSelected = true;
        }

        /// <summary>
        /// Handles the Click event of the btnImportGroup control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnImportGroup_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.Dispatcher.Invoke(() => 
            {
                var listBox = (ListBox)_mainWindow._scenariosControl.FindName("_scenarioListBox");
                listBox.SelectedIndex = 0;
            });
        }
    }

}

