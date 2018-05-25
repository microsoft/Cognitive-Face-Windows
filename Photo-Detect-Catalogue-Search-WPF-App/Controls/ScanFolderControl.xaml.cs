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
    using Photo_Detect_Catalogue_Search_WPF_App.Data;
    using Photo_Detect_Catalogue_Search_WPF_App.Helpers;
    using Photo_Detect_Catalogue_Search_WPF_App.Models;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Shapes;

    /// <summary>
    /// Interaction logic for ScanFolderPage.xaml
    /// </summary>
    public partial class ScanFolderControl : UserControl, INotifyPropertyChanged
    {
        /// <summary>
        /// The selected folder
        /// </summary>
        private string _selectedFolder;

        /// <summary>
        /// The files count
        /// </summary>
        private int _filesCount;

        /// <summary>
        /// The files
        /// </summary>
        private Queue<string> _files;

        /// <summary>
        /// The can scan
        /// </summary>
        private bool _canScan;

        /// <summary>
        /// The scan group
        /// </summary>
        private LargePersonGroupExtended _scanGroup;

        /// <summary>
        /// The detected faces
        /// </summary>
        private ObservableCollection<Models.Face> _detectedFaces = new ObservableCollection<Models.Face>();

        /// <summary>
        /// The result collection
        /// </summary>
        private ObservableCollection<Models.Face> _resultCollection = new ObservableCollection<Models.Face>();

        /// <summary>
        /// The selected file
        /// </summary>
        private ImageSource _selectedFile;

        /// <summary>
        /// The selected file path
        /// </summary>
        private string _selectedFilePath;

        /// <summary>
        /// The is dragging
        /// </summary>
        private bool _isDragging;

        /// <summary>
        /// The select rectangle
        /// </summary>
        private Rectangle _selectRectangle;

        /// <summary>
        /// The select rectangle start point
        /// </summary>
        private Point _selectRectangleStartPoint;

        /// <summary>
        /// The database
        /// </summary>
        private Data.SqlDataProvider db = new Data.SqlDataProvider();

        /// <summary>
        /// The face service client
        /// </summary>
        private FaceServiceClient _faceServiceClient;

        /// <summary>
        /// The main window
        /// </summary>
        private MainWindow _mainWindow;

        /// <summary>
        /// The main window log trace writer
        /// </summary>
        private MainWindowLogTraceWriter _mainWindowLogTraceWriter;

        /// <summary>
        /// Gets or sets the select rectangle.
        /// </summary>
        /// <value>
        /// The select rectangle.
        /// </value>
        public Rectangle SelectRectangle
        {
            get
            {
                return _selectRectangle;
            }

            set
            {
                _selectRectangle = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("SelectRectangle"));
                    CanScan = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets the files count.
        /// </summary>
        /// <value>
        /// The files count.
        /// </value>
        public int FilesCount
        {
            get
            {
                return _filesCount;
            }

            set
            {
                _filesCount = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("FilesCount"));
                    CanScan = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets the files.
        /// </summary>
        /// <value>
        /// The files.
        /// </value>
        public Queue<string> Files
        {
            get
            {
                return _files;
            }

            set
            {
                _files = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("Files"));
                    CanScan = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets the seleceted folder.
        /// </summary>
        /// <value>
        /// The seleceted folder.
        /// </value>
        public string SelecetedFolder
        {
            get
            {
                return _selectedFolder;
            }

            set
            {
                _selectedFolder = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("SelecetedFolder"));
                    CanScan = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance can scan.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance can scan; otherwise, <c>false</c>.
        /// </value>
        public bool CanScan
        {
            get
            {
                return _canScan;
            }

            set
            {
                _canScan = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("CanScan"));
                }
            }
        }

        /// <summary>
        /// Gets or sets the scan group.
        /// </summary>
        /// <value>
        /// The scan group.
        /// </value>
        public LargePersonGroupExtended ScanGroup
        {
            get
            {
                return _scanGroup;
            }

            set
            {
                _scanGroup = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("ScanGroup"));
                }
            }
        }

        /// <summary>
        /// Gets the detected faces.
        /// </summary>
        /// <value>
        /// The detected faces.
        /// </value>
        public ObservableCollection<Models.Face> DetectedFaces
        {
            get
            {
                return _detectedFaces;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScanFolderControl"/> class.
        /// </summary>
        /// <param name="group">The group.</param>
        /// <param name="mainWindow">The main window.</param>
        public ScanFolderControl(LargePersonGroupExtended group, MainWindow mainWindow)
        {
            _scanGroup = group;
            _mainWindow = mainWindow;
            _mainWindowLogTraceWriter = new MainWindowLogTraceWriter();
            InitializeComponent();
            Loaded += ScanFolderControl_Loaded;
        }

        /// <summary>
        /// Handles the Loaded event of the ScanFolderControl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void ScanFolderControl_Loaded(object sender, RoutedEventArgs e)
        {
            string subscriptionKey = _mainWindow._scenariosControl.SubscriptionKey;
            string endpoint = _mainWindow._scenariosControl.SubscriptionEndpoint;

            _faceServiceClient = new FaceServiceClient(subscriptionKey, endpoint);
        }

        /// <summary>
        /// Gets or sets the selected file.
        /// </summary>
        /// <value>
        /// The selected file.
        /// </value>
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
        /// Gets the result collection.
        /// </summary>
        /// <value>
        /// The result collection.
        /// </value>
        public ObservableCollection<Models.Face> ResultCollection
        {
            get
            {
                return _resultCollection;
            }
        }

        /// <summary>
        /// Gets the maximum size of the image.
        /// </summary>
        /// <value>
        /// The maximum size of the image.
        /// </value>
        public int MaxImageSize
        {
            get
            {
                return 300;
            }
        }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Handles the Click event of the BtnFolderSelect control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void BtnFolderSelect_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                dialog.SelectedPath = SelecetedFolder;
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();

                if (dialog.SelectedPath == null)
                {
                    return;
                }

                SelecetedFolder = dialog.SelectedPath;
                Files = new Queue<string>(Directory.GetFiles(_selectedFolder));
                FilesCount = Files.Count;
            }
        }

        /// <summary>
        /// Handles the Click event of the BtnScan control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void BtnScan_Click(object sender, RoutedEventArgs e)
        {
            GetNextFile();
        }

        /// <summary>
        /// Gets the next file.
        /// </summary>
        private void GetNextFile()
        {
            DetectedFaces.Clear();
            btnNext.IsEnabled = false;

            while (Files.Count > 0)
            {
                var file = Files.Dequeue();
                var dbFile = db.GetFile(file);
                if (dbFile == null)
                {
                    ProcessFile(file);
                    break;
                }
            }
            MainWindow.Log("No more files in this folder to process");
        }

        /// <summary>
        /// Processes the file.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        private async void ProcessFile(string filePath)
        {
            _selectedFilePath = filePath;
            
            using (var fStream = File.OpenRead(filePath))
            {
                try
                {
                    var faces = await RetryHelper.OperationWithBasicRetryAsync(async () => await
                        _faceServiceClient.DetectAsync(fStream, false, true, new FaceAttributeType[] { FaceAttributeType.Gender, FaceAttributeType.Age, FaceAttributeType.Smile, FaceAttributeType.Glasses, FaceAttributeType.HeadPose, FaceAttributeType.FacialHair, FaceAttributeType.Emotion, FaceAttributeType.Hair, FaceAttributeType.Makeup, FaceAttributeType.Occlusion, FaceAttributeType.Accessories, FaceAttributeType.Noise, FaceAttributeType.Exposure, FaceAttributeType.Blur }),
                        new[] { typeof(FaceAPIException) },
                        traceWriter: _mainWindowLogTraceWriter);
                    
                    MainWindow.Log("Response: Success. Detected {0} face(s) in {1}", faces.Length, filePath);
                    
                    if (faces.Length == 0)
                    {
                        btnNext.IsEnabled = true;
                        return;
                    }

                    var renderingImage = UIHelper.LoadImageAppliedOrientation(filePath);
                    var imageInfo = UIHelper.GetImageInfoForRendering(renderingImage);
                    SelectedFile = renderingImage;

                    foreach (var face in faces)
                    {
                        DetectedFaces.Add(new Models.Face()
                        {
                            ImageFile = renderingImage,
                            Left = face.FaceRectangle.Left,
                            Top = face.FaceRectangle.Top,
                            Width = face.FaceRectangle.Width,
                            Height = face.FaceRectangle.Height,
                            FaceRectangle = new FaceRectangle { Height = face.FaceRectangle.Height, Width = face.FaceRectangle.Width, Left = face.FaceRectangle.Left, Top = face.FaceRectangle.Top },
                            FaceId = face.FaceId.ToString(),
                            Age = string.Format("{0:#} years old", face.FaceAttributes.Age),
                            Gender = face.FaceAttributes.Gender,
                            HeadPose = string.Format("Pitch: {0}, Roll: {1}, Yaw: {2}", Math.Round(face.FaceAttributes.HeadPose.Pitch, 2), Math.Round(face.FaceAttributes.HeadPose.Roll, 2), Math.Round(face.FaceAttributes.HeadPose.Yaw, 2)),
                            FacialHair = string.Format("FacialHair: {0}", face.FaceAttributes.FacialHair.Moustache + face.FaceAttributes.FacialHair.Beard + face.FaceAttributes.FacialHair.Sideburns > 0 ? "Yes" : "No"),
                            Glasses = string.Format("GlassesType: {0}", face.FaceAttributes.Glasses.ToString()),
                            Emotion = $"{GetEmotion(face.FaceAttributes.Emotion)}",
                            Hair = string.Format("Hair: {0}", GetHair(face.FaceAttributes.Hair)),
                            Makeup = string.Format("Makeup: {0}", ((face.FaceAttributes.Makeup.EyeMakeup || face.FaceAttributes.Makeup.LipMakeup) ? "Yes" : "No")),
                            EyeOcclusion = string.Format("EyeOccluded: {0}", ((face.FaceAttributes.Occlusion.EyeOccluded) ? "Yes" : "No")),
                            ForeheadOcclusion = string.Format("ForeheadOccluded: {0}", (face.FaceAttributes.Occlusion.ForeheadOccluded ? "Yes" : "No")),
                            MouthOcclusion = string.Format("MouthOccluded: {0}", (face.FaceAttributes.Occlusion.MouthOccluded ? "Yes" : "No")),
                            Accessories = $"{GetAccessories(face.FaceAttributes.Accessories)}",
                            Blur = string.Format("Blur: {0}", face.FaceAttributes.Blur.BlurLevel.ToString()),
                            Exposure = string.Format("{0}", face.FaceAttributes.Exposure.ExposureLevel.ToString()),
                            Noise = string.Format("Noise: {0}", face.FaceAttributes.Noise.NoiseLevel.ToString()),
                        });
                    }

                    // Convert detection result into UI binding object for rendering
                    foreach (var face in UIHelper.CalculateFaceRectangleForRendering(faces, MaxImageSize, imageInfo))
                    {
                        ResultCollection.Add(face);
                    }

                    // Start train large person group
                    MainWindow.Log("Request: Training group \"{0}\"", _scanGroup.Group.LargePersonGroupId);
                    await RetryHelper.VoidOperationWithBasicRetryAsync(() =>
                        _faceServiceClient.TrainLargePersonGroupAsync(_scanGroup.Group.LargePersonGroupId),
                        new[] { typeof(FaceAPIException) },
                        traceWriter: _mainWindowLogTraceWriter);
                    
                    // Wait until train completed
                    while (true)
                    {
                        await Task.Delay(1000);

                        try // Temporary
                        {
                            var status = await _faceServiceClient.GetLargePersonGroupTrainingStatusAsync(_scanGroup.Group.LargePersonGroupId);
                            MainWindow.Log("Response: {0}. Group \"{1}\" training process is {2}", "Success", _scanGroup.Group.LargePersonGroupId, status.Status);
                            if (status.Status != Microsoft.ProjectOxford.Face.Contract.Status.Running)
                            {
                                break;
                            }
                        }
                        catch (Exception ex)
                        {
                            MainWindow.Log($"Error: {ex.Message}");
                            // retry
                        }
                    }

                    await GoGetMatches();

                }
                catch (FaceAPIException ex)
                {
                    MainWindow.Log("Response: {0}. {1}", ex.ErrorCode, ex.ErrorMessage);
                    GC.Collect();
                    return;
                }
                GC.Collect();
            }

            btnNext.IsEnabled = true;
        }

        /// <summary>
        /// Goes the get matches.
        /// </summary>
        /// <returns></returns>
        private async Task GoGetMatches()
        {
            // Identify each face
            // Call identify REST API, the result contains identified person information
            //var identifyResult = await _faceServiceClient.IdentifyAsync(_detectedFaces.Select(ff => new Guid(ff.FaceId)).ToArray(), largePersonGroupId: this._scanGroup.Group.LargePersonGroupId);
            var identifyResult = await RetryHelper.OperationWithBasicRetryAsync(async () => await
                _faceServiceClient.IdentifyAsync(_detectedFaces.Select(ff => new Guid(ff.FaceId)).ToArray(), largePersonGroupId: this._scanGroup.Group.LargePersonGroupId),
                new[] { typeof(FaceAPIException) },
                traceWriter: _mainWindowLogTraceWriter);

            for (int idx = 0; idx < _detectedFaces.Count; idx++)
            {
                // Update identification result for rendering
                var face = DetectedFaces[idx];
                var res = identifyResult[idx];
                if (res.Candidates.Length > 0 && _scanGroup.GroupPersons.Any(p => p.Person.PersonId == res.Candidates[0].PersonId))
                {
                    var pers = _scanGroup.GroupPersons.Where(p => p.Person.PersonId == res.Candidates[0].PersonId).First().Person;
                    face.PersonName = pers.Name;
                    face.PersonId = pers.PersonId;
                    face.PersonSourcePath = pers.UserData;
                }
                else
                {
                    face.PersonName = "Unknown";
                }
            }

            var outString = new StringBuilder();
            foreach (var face in DetectedFaces)
            {
                outString.AppendFormat("Face {0} is identified as {1}. ", face.FaceId, face.PersonName);
            }

            MainWindow.Log("Response: Success. {0}", outString);

        }

        /// <summary>
        /// Gets the hair.
        /// </summary>
        /// <param name="hair">The hair.</param>
        /// <returns></returns>
        private string GetHair(Hair hair)
        {
            if (hair.HairColor.Length == 0)
            {
                if (hair.Invisible)
                    return "Invisible";
                else
                    return "Bald";
            }
            else
            {
                HairColorType returnColor = HairColorType.Unknown;
                double maxConfidence = 0.0f;

                for (int i = 0; i < hair.HairColor.Length; ++i)
                {
                    if (hair.HairColor[i].Confidence > maxConfidence)
                    {
                        maxConfidence = hair.HairColor[i].Confidence;
                        returnColor = hair.HairColor[i].Color;
                    }
                }

                return returnColor.ToString();
            }
        }

        /// <summary>
        /// Gets the accessories.
        /// </summary>
        /// <param name="accessories">The accessories.</param>
        /// <returns></returns>
        private string GetAccessories(Accessory[] accessories)
        {
            if (accessories.Length == 0)
            {
                return "NoAccessories";
            }

            string[] accessoryArray = new string[accessories.Length];

            for (int i = 0; i < accessories.Length; ++i)
            {
                accessoryArray[i] = accessories[i].Type.ToString();
            }

            return "Accessories: " + String.Join(",", accessoryArray);
        }

        /// <summary>
        /// Gets the emotion.
        /// </summary>
        /// <param name="emotion">The emotion.</param>
        /// <returns></returns>
        private string GetEmotion(Microsoft.ProjectOxford.Common.Contract.EmotionScores emotion)
        {
            string emotionType = string.Empty;
            double emotionValue = 0.0;
            if (emotion.Anger > emotionValue)
            {
                emotionValue = emotion.Anger;
                emotionType = "Anger";
            }
            if (emotion.Contempt > emotionValue)
            {
                emotionValue = emotion.Contempt;
                emotionType = "Contempt";
            }
            if (emotion.Disgust > emotionValue)
            {
                emotionValue = emotion.Disgust;
                emotionType = "Disgust";
            }
            if (emotion.Fear > emotionValue)
            {
                emotionValue = emotion.Fear;
                emotionType = "Fear";
            }
            if (emotion.Happiness > emotionValue)
            {
                emotionValue = emotion.Happiness;
                emotionType = "Happiness";
            }
            if (emotion.Neutral > emotionValue)
            {
                emotionValue = emotion.Neutral;
                emotionType = "Neutral";
            }
            if (emotion.Sadness > emotionValue)
            {
                emotionValue = emotion.Sadness;
                emotionType = "Sadness";
            }
            if (emotion.Surprise > emotionValue)
            {
                emotionValue = emotion.Surprise;
                emotionType = "Surprise";
            }
            return $"{emotionType}";
        }

        /// <summary>
        /// Handles the MouseDown event of the imgCurrent control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
        private void imgCurrent_MouseDown(object sender, MouseButtonEventArgs e)
        {
            canvDrag.Children.Clear();
            _isDragging = true;

            _selectRectangleStartPoint = e.GetPosition((IInputElement)sender);
            SelectRectangle = new Rectangle() { IsHitTestVisible = false, Width = 1, Height = 1, Stroke = new SolidColorBrush(Colors.Black), StrokeThickness = 1 };
            SelectRectangle.SetValue(Canvas.LeftProperty, _selectRectangleStartPoint.X);
            SelectRectangle.SetValue(Canvas.TopProperty, _selectRectangleStartPoint.Y);

            canvDrag.Children.Add(SelectRectangle);
        }

        /// <summary>
        /// Handles the MouseMove event of the imgCurrent control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        private void imgCurrent_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                var point = e.GetPosition((IInputElement)sender);
                var width = point.X - _selectRectangleStartPoint.X;
                var height = point.Y - _selectRectangleStartPoint.Y;

                if (width == 0 || height == 0)
                {
                    return;
                }

                if (width > 0)
                {
                    SelectRectangle.Width = width;
                }
                else
                {
                    _selectRectangleStartPoint.X += width;
                    SelectRectangle.SetValue(Canvas.LeftProperty, _selectRectangleStartPoint.X);
                    SelectRectangle.Width -= width;
                }

                if (height > 0)
                {
                    SelectRectangle.Height = height;
                }
                else
                {
                    _selectRectangleStartPoint.Y += height;
                    SelectRectangle.SetValue(Canvas.TopProperty, _selectRectangleStartPoint.Y);
                    SelectRectangle.Height -= height;
                }
            }
        }

        /// <summary>
        /// Handles the MouseLeave event of the imgCurrent control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        private void imgCurrent_MouseLeave(object sender, MouseEventArgs e)
        {
            _isDragging = false;
            canvDrag.Children.Clear();
        }

        /// <summary>
        /// Handles the MouseUp event of the imgCurrent control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
        private void imgCurrent_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var scale = ((1 / imgCurrent.Source.Width) * imgCurrent.ActualWidth);
            var face = new Models.Face { Height = (int)(SelectRectangle.Height / scale), Width = (int)(SelectRectangle.Width / scale), Left = (int)(_selectRectangleStartPoint.X / scale), Top = (int)(_selectRectangleStartPoint.Y / scale), ImageFile = SelectedFile };
            DetectedFaces.Add(face);
            _isDragging = false;
            canvDrag.Children.Clear();
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            var file = new PictureFile { DateAdded = DateTime.Now, FilePath = _selectedFilePath, IsConfirmed = true };
            db.AddFile(file, _scanGroup.Group.LargePersonGroupId);

            for (var ix = 0; ix < DetectedFaces.Count; ix++)
            {
                var face = DetectedFaces[ix];
                var person = new PicturePerson
                {
                    DateAdded = DateTime.Now,
                    PersonId = face.PersonId,
                    PictureFileId = file.Id,
                    LargePersonGroupId = _scanGroup.Group.LargePersonGroupId,
                    FaceJSON = Newtonsoft.Json.JsonConvert.SerializeObject(face),
                    IsConfirmed = true
                };
                db.AddPerson(person);

                if (face.AddToGroup)
                {
                    var contentControl = face.ContextBinder.Parent as ContentControl;
                    var parentGrid = contentControl.Parent as Grid;
                    var croppedImage = parentGrid.Children[1] as Image;

                    var filePath = face.ImageFile.ToString().Replace("file:///", "");
                    filePath = filePath.Replace('\\', '/');

                    var fileName = $"DbId-{person.Id}_" + System.IO.Path.GetFileName(filePath); // unique file name, into training folder
                    var newFilePath = System.IO.Path.Combine(face.PersonSourcePath, fileName);
                    
                    CropToSquare(filePath, newFilePath, face.Left, face.Top, face.Width, face.Height);
                    
                    await AddFaceToLargePersonGroup(_scanGroup.Group.LargePersonGroupId, newFilePath, face.PersonId);
                }
            }

            GetNextFile();
        }

        /// <summary>
        /// Crops an image to square.
        /// </summary>
        /// <param name="oldFilename">The old filename.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="left">The left position.</param>
        /// <param name="top">The top position.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        private void CropToSquare(string oldFilename, string fileName, int left, int top, int width, int height)
        {
            // Create a new image at the cropped size
            System.Drawing.Bitmap cropped = new System.Drawing.Bitmap(width, height);

            //Load image from file
            using (System.Drawing.Image image = System.Drawing.Image.FromFile(oldFilename))
            {
                // Create a Graphics object to do the drawing, *with the new bitmap as the target*
                using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(cropped))
                {
                    // Draw the desired area of the original into the graphics object
                    g.DrawImage(image, new System.Drawing.Rectangle(0, 0, width, height), new System.Drawing.Rectangle(left, top, width, height), System.Drawing.GraphicsUnit.Pixel);
                    // Save the result
                    cropped.Save(fileName);
                }
                cropped.Dispose();
            }
        }

        /// <summary>
        /// Adds the face to a LargePersonGroup.
        /// </summary>
        /// <param name="largePersonGroupId">The large person group identifier.</param>
        /// <param name="imgPath">The img path.</param>
        /// <param name="PersonId">The person identifier.</param>
        /// <returns></returns>
        private async Task AddFaceToLargePersonGroup(string largePersonGroupId, string imgPath, Guid PersonId)
        {
            var imageList = new ConcurrentBag<string>(new [] { imgPath });

            string img;
            while (imageList.TryTake(out img))
            {
                using (var fStream = File.OpenRead(img))
                {
                    try
                    {
                        //face.image.Save(m, image.RawFormat);
                        // Update person faces on server side
                        var persistFace = await _faceServiceClient.AddPersonFaceInLargePersonGroupAsync(largePersonGroupId, PersonId, fStream, img);
                        return;
                    }
                    catch (FaceAPIException ex)
                    {
                        // if operation conflict, retry.
                        if (ex.ErrorCode.Equals("ConcurrentOperationConflict"))
                        {
                            MainWindow.Log("Concurrent Operation Conflict. Re-queuing");
                            imageList.Add(img);
                            continue;
                        }
                        // if operation cause rate limit exceed, retry.
                        else if (ex.ErrorCode.Equals("RateLimitExceeded"))
                        {
                            imageList.Add(img);
                            MainWindow.Log("Rate Limit Exceeded. Re-queuing in 1 second");
                            await Task.Delay(1000);
                            continue;
                        }

                        MainWindow.Log($"Error: {ex.Message}");

                        // Here we simply ignore all detection failure in this sample
                        // You may handle these exceptions by check the Error.Error.Code and Error.Message property for ClientException object
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Handles the 1 event of the btnRemove_Click control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            var ctrl = sender as Button;

            var f = ctrl.DataContext as Models.Face;
            DetectedFaces.Remove(f);
        }

        /// <summary>
        /// Handles the Click event of the btnNext control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            GetNextFile();
        }
    }
}
