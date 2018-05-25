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
// This is rewritten from the same named page in the other sample project.
// This page loads the datasets of images for each person into the web service.
// You should have at least 3-5 images of each person to begin.
// Create a parent folder for the group, which has subfolders inside,
// for each person in the group/family. The parent folder name should be the
// group name "Jones Family", the sub folders should be the people names, 
// like "Jones-John" and "Jones-Jane", then the file names should be the 
// person name with 1,2,3, etc after. See the demo data folder, exampled below:
//
//   ../Laker/Laker-Peter/Laker-Peter1.jpg
//   ../Laker/Laker-Peter/Laker-Peter2.jpg
//   ../Laker/Laker-Peter/Laker-Peter3.jpg
//   ../Laker/Laker-Sally/Laker-Sally1.jpg
//   ../Laker/Laker-Sally/Laker-Sally2.jpg


namespace Photo_Detect_Catalogue_Search_WPF_App.Controls
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using ClientContract = Microsoft.ProjectOxford.Face.Contract;
    using System.Windows.Media;
    using Microsoft.ProjectOxford.Face.Contract;
    using Microsoft.ProjectOxford.Face;
    using Photo_Detect_Catalogue_Search_WPF_App.Helpers;
    using Newtonsoft.Json.Serialization;

    /// <summary>
    /// Interaction logic for FaceDetection.xaml
    /// </summary>
    public partial class FaceIdentificationPage : Page, INotifyPropertyChanged
    {
        #region Fields

        /// <summary>
        /// Description dependency property
        /// </summary>
        public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register("Description", typeof(string), typeof(FaceIdentificationPage));

        /// <summary>
        /// Temporary group id for create person database.
        /// </summary>
        private static string sampleGroupId = Guid.NewGuid().ToString();

        /// <summary>
        /// Faces to identify
        /// </summary>
        private ObservableCollection<Models.Face> _faces = new ObservableCollection<Models.Face>();

        /// <summary>
        /// Person database
        /// </summary>
        private ObservableCollection<Person> _persons = new ObservableCollection<Person>();

        /// <summary>
        /// User picked image file
        /// </summary>
        private ImageSource _selectedFile;

        /// <summary>
        /// max concurrent process number for client query.
        /// </summary>
        private int _maxConcurrentProcesses;

        private MainWindowLogTraceWriter _mainWindowLogTraceWriter;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FaceIdentificationPage" /> class
        /// </summary>
        public FaceIdentificationPage()
        {
            InitializeComponent();
            _maxConcurrentProcesses = 4;

            _mainWindowLogTraceWriter = new MainWindowLogTraceWriter();
        }

        #endregion Constructors

        #region Events

        /// <summary>
        /// Implement INotifyPropertyChanged interface
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Events

        #region Properties

        /// <summary>
        /// Gets or sets description
        /// </summary>
        public string Description
        {
            get
            {
                return (string)GetValue(DescriptionProperty);
            }

            set
            {
                SetValue(DescriptionProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets group id.
        /// </summary>
        public string GroupId
        {
            get
            {
                return sampleGroupId;
            }

            set
            {
                sampleGroupId = value;
            }
        }

        /// <summary>
        /// Gets constant maximum image size for rendering detection result
        /// </summary>
        public int MaxImageSize
        {
            get
            {
                return 300;
            }
        }

        /// <summary>
        /// Gets person database
        /// </summary>
        public ObservableCollection<Person> Persons
        {
            get
            {
                return _persons;
            }
        }

        /// <summary>
        /// Gets or sets user picked image file
        /// </summary>
        public ImageSource SelectedFile
        {
            get
            {
                return _selectedFile;
            }

            set
            {
                _selectedFile = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("SelectedFile"));
                }
            }
        }

        /// <summary>
        /// Gets faces to identify
        /// </summary>
        public ObservableCollection<Models.Face> TargetFaces
        {
            get
            {
                return _faces;
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Pick the root person database folder, to minimum the data preparation logic, the folder should be under following construction
        /// Each person's image should be put into one folder named as the person's name
        /// All person's image folder should be put directly under the root person database folder
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event argument</param>
        private async void FolderPicker_Click(object sender, RoutedEventArgs e)
        {
            bool groupExists = false;

            MainWindow mainWindow = Window.GetWindow(this) as MainWindow;
            string subscriptionKey = mainWindow._scenariosControl.SubscriptionKey;
            string endpoint= mainWindow._scenariosControl.SubscriptionEndpoint;

            var faceServiceClient = new FaceServiceClient(subscriptionKey,endpoint);

            // Test whether the group already exists
            try
            {
                MainWindow.Log("Request: Group {0} will be used to build a person database. Checking whether the group exists.", this.GroupId);

                await faceServiceClient.GetLargePersonGroupAsync(this.GroupId);
                groupExists = true;
                MainWindow.Log("Response: Group {0} exists.", this.GroupId);
            }
            catch (FaceAPIException ex)
            {
                if (ex.ErrorCode != "LargePersonGroupNotFound")
                {
                    MainWindow.Log("Response: {0}. {1}", ex.ErrorCode, ex.ErrorMessage);
                    return;
                }
                else
                {
                    MainWindow.Log("Response: Group {0} did not exist previously.", this.GroupId);
                }
            }

            if (groupExists)
            {
                var cleanGroup = System.Windows.MessageBox.Show(string.Format("Requires a clean up for group \"{0}\" before setting up a new person database. Click OK to proceed, group \"{0}\" will be cleared.", this.GroupId), "Warning", MessageBoxButton.OKCancel);
                if (cleanGroup == MessageBoxResult.OK)
                {
                    await faceServiceClient.DeleteLargePersonGroupAsync(this.GroupId);
                    this.GroupId = Guid.NewGuid().ToString();
                }
                else
                {
                    return;
                }
            }

            // Show folder picker
            System.Windows.Forms.FolderBrowserDialog dlg = new System.Windows.Forms.FolderBrowserDialog();
            var result = dlg.ShowDialog();

            // Set the suggestion count is intent to minimum the data preparation step only,
            // it's not corresponding to service side constraint
            const int SuggestionCount = 15;

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                // User picked a root person database folder
                // Clear person database
                Persons.Clear();
                TargetFaces.Clear();
                SelectedFile = null;
                IdentifyButton.IsEnabled = false;
                
                // Call create large person group REST API
                // Create large person group API call will failed if group with the same name already exists
                MainWindow.Log("Request: Creating group \"{0}\"", this.GroupId);
                try
                {
                    await faceServiceClient.CreateLargePersonGroupAsync(this.GroupId, this.GroupId, dlg.SelectedPath);
                    MainWindow.Log("Response: Success. Group \"{0}\" created", this.GroupId);
                }
                catch (FaceAPIException ex)
                {
                    MainWindow.Log("Response: {0}. {1}", ex.ErrorCode, ex.ErrorMessage);
                    return;
                }

                int processCount = 0;
                bool forceContinue = false;

                MainWindow.Log("Request: Preparing faces for identification, detecting faces in chosen folder.");

                // Enumerate top level directories, each directory contains one person's images
                int invalidImageCount = 0;
                foreach (var dir in System.IO.Directory.EnumerateDirectories(dlg.SelectedPath))
                {
                    var tasks = new List<Task>();
                    var tag = System.IO.Path.GetFileName(dir);
                    Person p = new Person();
                    p.PersonName = tag;

                    var faces = new ObservableCollection<Models.Face>();
                    p.Faces = faces;

                    // Call create person REST API, the new create person id will be returned
                    MainWindow.Log("Request: Creating person \"{0}\"", p.PersonName);
                    
                    p.PersonId = (await RetryHelper.OperationWithBasicRetryAsync(async () => await 
                        faceServiceClient.CreatePersonInLargePersonGroupAsync(this.GroupId, p.PersonName, dir), 
                        new[] { typeof(FaceAPIException) },
                        traceWriter:_mainWindowLogTraceWriter
                        )).PersonId.ToString();

                    MainWindow.Log("Response: Success. Person \"{0}\" (PersonID:{1}) created", p.PersonName, p.PersonId);

                    string img;
                    // Enumerate images under the person folder, call detection
                    var imageList =
                    new ConcurrentBag<string>(
                        Directory.EnumerateFiles(dir, "*.*", SearchOption.AllDirectories)
                            .Where(s => s.ToLower().EndsWith(".jpg") || s.ToLower().EndsWith(".png") || s.ToLower().EndsWith(".bmp") || s.ToLower().EndsWith(".gif")));
                    
                    while (imageList.TryTake(out img))
                    {
                        tasks.Add(Task.Factory.StartNew(
                            async (obj) =>
                            {
                                var imgPath = obj as string;

                                using (var fStream = File.OpenRead(imgPath))
                                {
                                    try
                                    {
                                        // Update person faces on server side
                                        var persistFace = await faceServiceClient.AddPersonFaceInLargePersonGroupAsync(this.GroupId, Guid.Parse(p.PersonId), fStream, imgPath);
                                        return new Tuple<string, ClientContract.AddPersistedFaceResult>(imgPath, persistFace);
                                    }
                                    catch (FaceAPIException ex)
                                    {
                                        // if operation conflict, retry.
                                        if (ex.ErrorCode.Equals("ConcurrentOperationConflict"))
                                        {
                                            imageList.Add(imgPath);
                                            return null;
                                        }
                                        // if operation cause rate limit exceed, retry.
                                        else if (ex.ErrorCode.Equals("RateLimitExceeded"))
                                        {
                                            imageList.Add(imgPath);
                                            return null;
                                        }
                                        else if (ex.ErrorMessage.Contains("more than 1 face in the image."))
                                        {
                                            Interlocked.Increment(ref invalidImageCount);
                                        }
                                        // Here we simply ignore all detection failure in this sample
                                        // You may handle these exceptions by check the Error.Error.Code and Error.Message property for ClientException object
                                        return new Tuple<string, ClientContract.AddPersistedFaceResult>(imgPath, null);
                                    }
                                }
                            },
                            img).Unwrap().ContinueWith((detectTask) =>
                            {
                                // Update detected faces for rendering
                                var detectionResult = detectTask?.Result;
                                if (detectionResult == null || detectionResult.Item2 == null)
                                {
                                    return;
                                }

                                this.Dispatcher.Invoke(
                                    new Action<ObservableCollection<Models.Face>, string, ClientContract.AddPersistedFaceResult>(UIHelper.UpdateFace),
                                    faces,
                                    detectionResult.Item1,
                                    detectionResult.Item2);
                            }));
                        if (processCount >= SuggestionCount && !forceContinue)
                        {
                            var continueProcess = System.Windows.Forms.MessageBox.Show("The images loaded have reached the recommended count, may take long time if proceed. Would you like to continue to load images?", "Warning", System.Windows.Forms.MessageBoxButtons.YesNo);
                            if (continueProcess == System.Windows.Forms.DialogResult.Yes)
                            {
                                forceContinue = true;
                            }
                            else
                            {
                                break;
                            }
                        }

                        if (tasks.Count >= _maxConcurrentProcesses || imageList.IsEmpty)
                        {
                            await Task.WhenAll(tasks);
                            tasks.Clear();
                        }
                    }

                    Persons.Add(p);
                }
                if (invalidImageCount > 0)
                {
                    MainWindow.Log("Warning: more or less than one face is detected in {0} images, can not add to face list.", invalidImageCount);
                }
                MainWindow.Log("Response: Success. Total {0} faces are detected.", Persons.Sum(p => p.Faces.Count));

                try
                {
                    // Start train large person group
                    MainWindow.Log("Request: Training group \"{0}\"", this.GroupId);

                    await RetryHelper.VoidOperationWithBasicRetryAsync(() =>
                        faceServiceClient.TrainLargePersonGroupAsync(this.GroupId),
                        new[] { typeof(FaceAPIException) },
                        traceWriter: _mainWindowLogTraceWriter);

                    //await faceServiceClient.TrainLargePersonGroupAsync(this.GroupId);

                    // Wait until train completed
                    while (true)
                    {
                        await Task.Delay(1000);
                        var status = await faceServiceClient.GetLargePersonGroupTrainingStatusAsync(this.GroupId);
                        MainWindow.Log("Response: {0}. Group \"{1}\" training process is {2}", "Success", this.GroupId, status.Status);
                        if (status.Status != Status.Running)
                        {
                            break;
                        }
                    }
                    IdentifyButton.IsEnabled = true;
                }
                catch (FaceAPIException ex)
                {
                    MainWindow.Log("Response: {0}. {1}", ex.ErrorCode, ex.ErrorMessage);
                }
            }
            GC.Collect();
        }

        /// <summary>
        /// Pick image, detect and identify all faces detected
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event arguments</param>
        private async void Identify_Click(object sender, RoutedEventArgs e)
        {
            // Show file picker
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".jpg";
            dlg.Filter = "Image files(*.jpg, *.png, *.bmp, *.gif) | *.jpg; *.png; *.bmp; *.gif";
            var result = dlg.ShowDialog();

            if (result.HasValue && result.Value)
            {
                // User picked one image
                // Clear previous detection and identification results
                TargetFaces.Clear();
                var pickedImagePath = dlg.FileName;
                var renderingImage = UIHelper.LoadImageAppliedOrientation(pickedImagePath);
                var imageInfo = UIHelper.GetImageInfoForRendering(renderingImage);
                SelectedFile = renderingImage;

                var sw = Stopwatch.StartNew();

                MainWindow mainWindow = Window.GetWindow(this) as MainWindow;
                string subscriptionKey = mainWindow._scenariosControl.SubscriptionKey;
                string subscriptionEndpoint = mainWindow._scenariosControl.SubscriptionEndpoint;
                var faceServiceClient = new FaceServiceClient(subscriptionKey, subscriptionEndpoint);

                // Call detection REST API
                using (var fStream = File.OpenRead(pickedImagePath))
                {
                    try
                    {
                        var faces = await RetryHelper.OperationWithBasicRetryAsync(async () => await
                            faceServiceClient.DetectAsync(fStream),
                            new[] { typeof(FaceAPIException) },
                            traceWriter: _mainWindowLogTraceWriter);

                        //var faces = await faceServiceClient.DetectAsync(fStream);

                        // Convert detection result into UI binding object for rendering
                        foreach (var face in UIHelper.CalculateFaceRectangleForRendering(faces, MaxImageSize, imageInfo))
                        {
                            TargetFaces.Add(face);
                        }

                        MainWindow.Log("Request: Identifying {0} face(s) in group \"{1}\"", faces.Length, this.GroupId);

                        // Identify each face
                        // Call identify REST API, the result contains identified person information

                        var identifyResult = await RetryHelper.OperationWithBasicRetryAsync(async () => await
                            faceServiceClient.IdentifyAsync(faces.Select(ff => ff.FaceId).ToArray(), largePersonGroupId: this.GroupId),
                            new[] { typeof(FaceAPIException) },
                            traceWriter: _mainWindowLogTraceWriter);

                        //var identifyResult = await faceServiceClient.IdentifyAsync(faces.Select(ff => ff.FaceId).ToArray(), largePersonGroupId: this.GroupId);

                        for (int idx = 0; idx < faces.Length; idx++)
                        {
                            // Update identification result for rendering
                            var face = TargetFaces[idx];
                            var res = identifyResult[idx];
                            if (res.Candidates.Length > 0 && Persons.Any(p => p.PersonId == res.Candidates[0].PersonId.ToString()))
                            {
                                face.PersonName = Persons.Where(p => p.PersonId == res.Candidates[0].PersonId.ToString()).First().PersonName;
                            }
                            else
                            {
                                face.PersonName = "Unknown";
                            }
                        }

                        var outString = new StringBuilder();
                        foreach (var face in TargetFaces)
                        {
                            outString.AppendFormat("Face {0} is identified as {1}. ", face.FaceId, face.PersonName);
                        }

                        MainWindow.Log("Response: Success. {0}", outString);
                    }
                    catch (FaceAPIException ex)
                    {
                        MainWindow.Log("Response: {0}. {1}", ex.ErrorCode, ex.ErrorMessage);
                    }
                }
            }
            GC.Collect();
        }

        #endregion Methods

        #region Nested Types

        /// <summary>
        /// Identification result for UI binding
        /// </summary>
        public class IdentificationResult : INotifyPropertyChanged
        {
            #region Fields

            /// <summary>
            /// Face to identify
            /// </summary>
            private Face _faceToIdentify;

            /// <summary>
            /// Identified person's name
            /// </summary>
            private string _name;

            #endregion Fields

            #region Events

            /// <summary>
            /// Implement INotifyPropertyChanged interface
            /// </summary>
            public event PropertyChangedEventHandler PropertyChanged;

            #endregion Events

            #region Properties

            /// <summary>
            /// Gets or sets face to identify
            /// </summary>
            public Face FaceToIdentify
            {
                get
                {
                    return _faceToIdentify;
                }

                set
                {
                    _faceToIdentify = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("FaceToIdentify"));
                    }
                }
            }

            /// <summary>
            /// Gets or sets identified person's name
            /// </summary>
            public string Name
            {
                get
                {
                    return _name;
                }

                set
                {
                    _name = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("Name"));
                    }
                }
            }

            #endregion Properties
        }

        /// <summary>
        /// Person structure for UI binding
        /// </summary>
        public class Person : INotifyPropertyChanged
        {
            #region Fields

            /// <summary>
            /// Person's faces from database
            /// </summary>
            private ObservableCollection<Models.Face> _faces = new ObservableCollection<Models.Face>();

            /// <summary>
            /// Person's id
            /// </summary>
            private string _personId;

            /// <summary>
            /// Person's name
            /// </summary>
            private string _personName;

            #endregion Fields

            #region Events

            /// <summary>
            /// Implement INotifyPropertyChanged interface
            /// </summary>
            public event PropertyChangedEventHandler PropertyChanged;

            #endregion Events

            #region Properties

            /// <summary>
            /// Gets or sets person's faces from database
            /// </summary>
            public ObservableCollection<Models.Face> Faces
            {
                get
                {
                    return _faces;
                }

                set
                {
                    _faces = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("Faces"));
                    }
                }
            }

            /// <summary>
            /// Gets or sets person's id
            /// </summary>
            public string PersonId
            {
                get
                {
                    return _personId;
                }

                set
                {
                    _personId = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("PersonId"));
                    }
                }
            }

            /// <summary>
            /// Gets or sets person's name
            /// </summary>
            public string PersonName
            {
                get
                {
                    return _personName;
                }

                set
                {
                    _personName = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("PersonName"));
                    }
                }
            }

            #endregion Properties
        }

        #endregion Nested Types
    }
}