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

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using ClientContract = Microsoft.ProjectOxford.Face.Contract;

namespace Microsoft.ProjectOxford.Face.Controls
{
    /// <summary>
    /// Interaction logic for FaceVerification.xaml
    /// </summary>
    public partial class FaceVerificationPage : Page, INotifyPropertyChanged
    {
        #region Fields

        /// <summary>
        /// Description dependency property
        /// </summary>
        public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register("Description", typeof(string), typeof(FaceVerificationPage));

        /// <summary>
        /// Temporary group id for create person database.
        /// </summary>
        private static string sampleGroupId = Guid.NewGuid().ToString();

        /// <summary>
        /// Face detection result container for image on the left
        /// </summary>
        private ObservableCollection<Face> _leftResultCollection = new ObservableCollection<Face>();

        /// <summary>
        /// Face detection result container for image on the right
        /// </summary>
        private ObservableCollection<Face> _rightResultCollection = new ObservableCollection<Face>();

        /// <summary>
        /// Face detected for face to person verification
        /// </summary>
        private ObservableCollection<Face> _rightFaceResultCollection = new ObservableCollection<Face>();
        
        /// <summary>
        /// Faces collection which contains all faces of the person
        /// </summary>
        private ObservableCollection<Face> _facesCollection = new ObservableCollection<Face>();

        /// <summary>
        /// Face to face verification result
        /// </summary>
        private string _faceVerifyResult;

        /// <summary>
        /// Face to person verification result
        /// </summary>
        private string _personVerifyResult;
        
        /// <summary>
        /// max concurrent process number for client query.
        /// </summary>
        private int _maxConcurrentProcesses;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FaceVerificationPage" /> class
        /// </summary>
        public FaceVerificationPage()
        {
            InitializeComponent();
            _maxConcurrentProcesses = 4;
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
        /// Gets or sets description for UI rendering
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
        /// Gets face detection results for image on the left
        /// </summary>
        public ObservableCollection<Face> LeftResultCollection
        {
            get
            {
                return _leftResultCollection;
            }
        }

        /// <summary>
        /// Gets max image size for UI rendering
        /// </summary>
        public int MaxImageSize
        {
            get
            {
                return 300;
            }
        }

        /// <summary>
        /// Gets face detection results for image on the right
        /// </summary>
        public ObservableCollection<Face> RightResultCollection
        {
            get
            {
                return _rightResultCollection;
            }
        }

        /// <summary>
        /// Gets face detection results for face to person verification
        /// </summary>
        public ObservableCollection<Face> RightFaceResultCollection
        {
            get
            {
                return _rightFaceResultCollection;
            }
        }

        /// <summary>
        /// Gets faces of the person
        /// </summary>
        public ObservableCollection<Face> FacesCollection
        {
            get
            {
                return _facesCollection;
            }
        }

        /// <summary>
        /// Gets or sets selected face verification result
        /// </summary>
        public string FaceVerifyResult
        {
            get
            {
                return _faceVerifyResult;
            }

            set
            {
                _faceVerifyResult = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("FaceVerifyResult"));
                }
            }
        }

        /// <summary>
        /// Gets or sets selected face person verification result
        /// </summary>
        public string PersonVerifyResult
        {
            get
            {
                return _personVerifyResult;
            }

            set
            {
                _personVerifyResult = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("PersonVerifyResult"));
                }
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
        /// Person for verification
        /// </summary>
        public Person Person { get; set; } = new Person();

        #endregion Properties

        #region Methods

        /// <summary>
        /// Pick image for detection, get detection result and put detection results into LeftResultCollection 
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event argument</param>
        private async void LeftImagePicker_Click(object sender, RoutedEventArgs e)
        {
            // Show image picker, show jpg type files only
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".jpg";
            dlg.Filter = "Image files(*.jpg, *png, *.bmp, *.gif) | *.jpg; *.png; *.bmp; *.gif";
            var result = dlg.ShowDialog();

            if (result.HasValue && result.Value)
            {
                FaceVerifyResult = string.Empty;

                // User already picked one image
                var pickedImagePath = dlg.FileName;
                var renderingImage = UIHelper.LoadImageAppliedOrientation(pickedImagePath);
                var imageInfo = UIHelper.GetImageInfoForRendering(renderingImage);
                LeftImageDisplay.Source = renderingImage;

                // Clear last time detection results
                LeftResultCollection.Clear();
                FaceVerifyButton.IsEnabled = (LeftResultCollection.Count!=0 && RightResultCollection.Count!=0);
                MainWindow.Log("Request: Detecting in {0}", pickedImagePath);
                var sw = Stopwatch.StartNew();

                // Call detection REST API, detect faces inside the image
                using (var fileStream = File.OpenRead(pickedImagePath))
                {
                    try
                    {
                        MainWindow mainWindow = Window.GetWindow(this) as MainWindow;
                        string subscriptionKey = mainWindow._scenariosControl.SubscriptionKey;
                        string endpoint = mainWindow._scenariosControl.SubscriptionEndpoint;
                        var faceServiceClient = new FaceServiceClient(subscriptionKey,endpoint);
                        var faces = await faceServiceClient.DetectAsync(fileStream);

                        // Handle REST API calling error
                        if (faces == null)
                        {
                            return;
                        }

                        MainWindow.Log("Response: Success. Detected {0} face(s) in {1}", faces.Length, pickedImagePath);

                        // Convert detection results into UI binding object for rendering
                        foreach (var face in UIHelper.CalculateFaceRectangleForRendering(faces, MaxImageSize, imageInfo))
                        {
                            // Detected faces are hosted in result container, will be used in the verification later
                            LeftResultCollection.Add(face);
                        }

                        FaceVerifyButton.IsEnabled = (LeftResultCollection.Count != 0 && RightResultCollection.Count != 0);
                    }
                    catch (FaceAPIException ex)
                    {
                        MainWindow.Log("Response: {0}. {1}", ex.ErrorCode, ex.ErrorMessage);
                        return;
                    }
                }
            }
            GC.Collect();
        }

        /// <summary>
        /// Pick image for detection, get detection result and put detection results into RightResultCollection 
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event argument</param>
        private async void RightImagePicker_Click(object sender, RoutedEventArgs e)
        {
            // Show image picker, show jpg type files only
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".jpg";
            dlg.Filter = "Image files(*.jpg, *.png, *.bmp, *.gif) | *.jpg; *.png; *.bmp; *.gif";
            var result = dlg.ShowDialog();

            if (result.HasValue && result.Value)
            {
                FaceVerifyResult = string.Empty;

                // User already picked one image
                var pickedImagePath = dlg.FileName;
                var renderingImage = UIHelper.LoadImageAppliedOrientation(pickedImagePath);
                var imageInfo = UIHelper.GetImageInfoForRendering(renderingImage);
                RightImageDisplay.Source = renderingImage;

                // Clear last time detection results
                RightResultCollection.Clear();
                FaceVerifyButton.IsEnabled = (LeftResultCollection.Count != 0 && RightResultCollection.Count != 0);

                MainWindow.Log("Request: Detecting in {0}", pickedImagePath);
                var sw = Stopwatch.StartNew();

                // Call detection REST API, detect faces inside the image
                using (var fileStream = File.OpenRead(pickedImagePath))
                {
                    try
                    {
                        MainWindow mainWindow = Window.GetWindow(this) as MainWindow;
                        string subscriptionKey = mainWindow._scenariosControl.SubscriptionKey;
                        string endpoint = mainWindow._scenariosControl.SubscriptionEndpoint;
                        var faceServiceClient = new FaceServiceClient(subscriptionKey, endpoint);

                        var faces = await faceServiceClient.DetectAsync(fileStream);

                        // Handle REST API calling error
                        if (faces == null)
                        {
                            return;
                        }

                        MainWindow.Log("Response: Success. Detected {0} face(s) in {1}", faces.Length, pickedImagePath);

                        // Convert detection results into UI binding object for rendering
                        foreach (var face in UIHelper.CalculateFaceRectangleForRendering(faces, MaxImageSize, imageInfo))
                        {
                            // Detected faces are hosted in result container, will be used in the verification later
                            RightResultCollection.Add(face);
                        }
                        FaceVerifyButton.IsEnabled = (LeftResultCollection.Count != 0 && RightResultCollection.Count != 0);
                    }
                    catch (FaceAPIException ex)
                    {
                        MainWindow.Log("Response: {0}. {1}", ex.ErrorCode, ex.ErrorMessage);

                        return;
                    }
                }
            }
            GC.Collect();
        }

        /// <summary>
        /// Verify two detected faces, get whether these two faces belong to the same person
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event argument</param>
        private async void Face2FaceVerification_Click(object sender, RoutedEventArgs e)
        {
            // Call face to face verification, verify REST API supports one face to one face verification only
            // Here, we handle single face image only
            if (LeftResultCollection.Count == 1 && RightResultCollection.Count == 1)
            {
                FaceVerifyResult = "Verifying...";
                var faceId1 = LeftResultCollection[0].FaceId;
                var faceId2 = RightResultCollection[0].FaceId;

                MainWindow.Log("Request: Verifying face {0} and {1}", faceId1, faceId2);

                // Call verify REST API with two face id
                try
                {
                    MainWindow mainWindow = Window.GetWindow(this) as MainWindow;
                    string subscriptionKey = mainWindow._scenariosControl.SubscriptionKey;
                    string endpoint = mainWindow._scenariosControl.SubscriptionEndpoint;
                    var faceServiceClient = new FaceServiceClient(subscriptionKey, endpoint);

                    var res = await faceServiceClient.VerifyAsync(Guid.Parse(faceId1), Guid.Parse(faceId2));

                    // Verification result contains IsIdentical (true or false) and Confidence (in range 0.0 ~ 1.0),
                    // here we update verify result on UI by FaceVerifyResult binding
                    FaceVerifyResult = string.Format("Confidence = {0:0.00}, {1}", res.Confidence,  res.IsIdentical ? "two faces belong to same person" : "two faces not belong to same person");
                    MainWindow.Log("Response: Success. Face {0} and {1} {2} to the same person", faceId1, faceId2, res.IsIdentical ? "belong" : "not belong");
                }
                catch (FaceAPIException ex)
                {
                    MainWindow.Log("Response: {0}. {1}", ex.ErrorCode, ex.ErrorMessage);

                    return;
                }
            }
            else
            {
                MessageBox.Show("Verification accepts two faces as input, please pick images with only one detectable face in it.", "Warning", MessageBoxButton.OK);
            }
            GC.Collect();
        }
        
        /// <summary>
        /// Pick image folder to detect faces and using these faces to create the person database
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void PersonImageFolderPicker_Click(object sender, RoutedEventArgs e)
        {
            bool groupExists = false;

            MainWindow mainWindow = Window.GetWindow(this) as MainWindow;
            string subscriptionKey = mainWindow._scenariosControl.SubscriptionKey;
            string endpoint = mainWindow._scenariosControl.SubscriptionEndpoint;
            var faceServiceClient = new FaceServiceClient(subscriptionKey, endpoint);

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
                    PersonVerifyResult = string.Empty;
                    Person.Faces.Clear();
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
                FacesCollection.Clear();
                PersonVerifyButton.IsEnabled = (FacesCollection.Count != 0 && RightFaceResultCollection.Count != 0);

                // Call create large person group REST API
                // Create large person group API call will failed if group with the same name already exists
                MainWindow.Log("Request: Creating group \"{0}\"", this.GroupId);
                try
                {
                    await faceServiceClient.CreateLargePersonGroupAsync(this.GroupId, this.GroupId);
                    MainWindow.Log("Response: Success. Group \"{0}\" created", this.GroupId);
                }
                catch (FaceAPIException ex)
                {
                    MainWindow.Log("Response: {0}. {1}", ex.ErrorCode, ex.ErrorMessage);
                    return;
                }

                int processCount = 0;
                bool forceContinue = false;

                MainWindow.Log("Request: Preparing person for verification, detecting faces in chosen folder.");

                // Enumerate top level directories, each directory contains one person's images

                var tasks = new List<Task>();
                var tag = System.IO.Path.GetFileName(dlg.SelectedPath);
                Person = new Person();
                Person.PersonName = tag;

                var faces = new ObservableCollection<Face>();
                Person.Faces = faces;

                // Call create person REST API, the new create person id will be returned
                MainWindow.Log("Request: Creating person \"{0}\"", Person.PersonName);
                Person.PersonId =
                    (await faceServiceClient.CreatePersonInLargePersonGroupAsync(this.GroupId, Person.PersonName)).PersonId.ToString();
                MainWindow.Log("Response: Success. Person \"{0}\" (PersonID:{1}) created", Person.PersonName, Person.PersonId);

                string img;
                var imageList =
                    new ConcurrentBag<string>(
                        Directory.EnumerateFiles(dlg.SelectedPath, "*.*", SearchOption.AllDirectories)
                            .Where(s => s.ToLower().EndsWith(".jpg") || s.ToLower().EndsWith(".png") || s.ToLower().EndsWith(".bmp") || s.ToLower().EndsWith(".gif")));

                // Enumerate images under the person folder, call detection
                int invalidImageCount = 0;
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
                                    var persistFace =
                                        await
                                            faceServiceClient.AddPersonFaceInLargePersonGroupAsync(this.GroupId, Guid.Parse(Person.PersonId),
                                                fStream, imgPath);
                                    return new Tuple<string, ClientContract.AddPersistedFaceResult>(imgPath,
                                        persistFace);
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
                            if (detectionResult?.Item2 == null)
                            {
                                return;
                            }

                            this.Dispatcher.Invoke(
                                new Action
                                    <ObservableCollection<Face>, string, ClientContract.AddPersistedFaceResult>(
                                    UIHelper.UpdateFace),
                                FacesCollection,
                                detectionResult.Item1,
                                detectionResult.Item2);
                        }));
                    processCount++;

                    if (processCount >= SuggestionCount && !forceContinue)
                    {
                        var continueProcess =
                            System.Windows.Forms.MessageBox.Show(
                                "The images loaded have reached the recommended count, may take long time if proceed. Would you like to continue to load images?",
                                "Warning", System.Windows.Forms.MessageBoxButtons.YesNo);
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

                Person.Faces = FacesCollection;

                PersonVerifyButton.IsEnabled = (FacesCollection.Count != 0 && RightFaceResultCollection.Count != 0);

                if (invalidImageCount > 0)
                {
                    MainWindow.Log("Warning: more or less than one face is detected in {0} images, can not add to face list.", invalidImageCount);
                }
                MainWindow.Log("Response: Success. Total {0} faces are detected.", Person.Faces.Count);
            }
            GC.Collect();
        }
        
        /// <summary>
        /// Pick image for detection, and using the detected face as the face to person verify.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void FaceImagePicker_Click(object sender, RoutedEventArgs e)
        {
            // Show image picker, show jpg type files only
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".jpg";
            dlg.Filter = "Image files(*.jpg, *.png, *.bmp, *.gif) | *.jpg; *.png; *.bmp; *.gif";
            var result = dlg.ShowDialog();

            if (result.HasValue && result.Value)
            {
                PersonVerifyResult = string.Empty;

                // User already picked one image
                var pickedImagePath = dlg.FileName;
                var renderingImage = UIHelper.LoadImageAppliedOrientation(pickedImagePath);
                var imageInfo = UIHelper.GetImageInfoForRendering(renderingImage);
                RightImageDisplay2.Source = renderingImage;

                // Clear last time detection results
                RightFaceResultCollection.Clear();
                PersonVerifyButton.IsEnabled = (FacesCollection.Count != 0 && RightFaceResultCollection.Count != 0);

                MainWindow.Log("Request: Detecting in {0}", pickedImagePath);
                var sw = Stopwatch.StartNew();

                // Call detection REST API, detect faces inside the image
                using (var fileStream = File.OpenRead(pickedImagePath))
                {
                    try
                    {
                        MainWindow mainWindow = Window.GetWindow(this) as MainWindow;
                        string subscriptionKey = mainWindow._scenariosControl.SubscriptionKey;
                        string endpoint = mainWindow._scenariosControl.SubscriptionEndpoint;
                        var faceServiceClient = new FaceServiceClient(subscriptionKey, endpoint);

                        var faces = await faceServiceClient.DetectAsync(fileStream);

                        // Handle REST API calling error
                        if (faces == null)
                        {
                            return;
                        }

                        MainWindow.Log("Response: Success. Detected {0} face(s) in {1}", faces.Length, pickedImagePath);

                        // Convert detection results into UI binding object for rendering
                        foreach (var face in UIHelper.CalculateFaceRectangleForRendering(faces, MaxImageSize, imageInfo))
                        {
                            // Detected faces are hosted in result container, will be used in the verification later
                            RightFaceResultCollection.Add(face);
                        }
                        PersonVerifyButton.IsEnabled = (FacesCollection.Count != 0 && RightFaceResultCollection.Count != 0);
                    }
                    catch (FaceAPIException ex)
                    {
                        MainWindow.Log("Response: {0}. {1}", ex.ErrorCode, ex.ErrorMessage);

                        return;
                    }
                }
            }
            GC.Collect();
        }

        /// <summary>
        /// Verify the face with the person, get whether these two faces belong to the same person
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event argument</param>
        private async void Face2PersonVerification_Click(object sender, RoutedEventArgs e)
        {
            // Call face to face verification, verify REST API supports one face to one person verification only
            // Here, we handle single face image only
            if (Person != null && Person.Faces.Count != 0 && RightFaceResultCollection.Count == 1)
            {
                PersonVerifyResult = "Verifying..."; 
                var faceId = RightFaceResultCollection[0].FaceId;

                MainWindow.Log("Request: Verifying face {0} and person {1}", faceId, Person.PersonName);

                // Call verify REST API with two face id
                try
                {
                    MainWindow mainWindow = Window.GetWindow(this) as MainWindow;
                    string subscriptionKey = mainWindow._scenariosControl.SubscriptionKey;
                    string endpoint = mainWindow._scenariosControl.SubscriptionEndpoint;
                    var faceServiceClient = new FaceServiceClient(subscriptionKey, endpoint);

                    var res = await faceServiceClient.VerifyAsync(Guid.Parse(faceId), Guid.Parse(Person.PersonId), largePersonGroupId: this.GroupId);

                    // Verification result contains IsIdentical (true or false) and Confidence (in range 0.0 ~ 1.0),
                    // here we update verify result on UI by PersonVerifyResult binding
                    PersonVerifyResult = string.Format("{0} ({1:0.0})", res.IsIdentical ? "the face belongs to the person" : "the face not belong to the person", res.Confidence);
                    MainWindow.Log("Response: Success. Face {0} {1} person {2}", faceId, res.IsIdentical ? "belong" : "not belong", Person.PersonName);
                }
                catch (FaceAPIException ex)
                {
                    MainWindow.Log("Response: {0}. {1}", ex.ErrorCode, ex.ErrorMessage);

                    return;
                }
            }
            else
            {
                MessageBox.Show("Verification accepts one person containing face(s) and one face as input, please check.", "Warning", MessageBoxButton.OK);
            }
            GC.Collect();
        }

        #endregion Methods
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
        private ObservableCollection<Face> _faces = new ObservableCollection<Face>();

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
        public ObservableCollection<Face> Faces
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
}