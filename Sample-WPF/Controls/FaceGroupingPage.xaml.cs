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
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ClientContract = Microsoft.ProjectOxford.Face.Contract;

namespace Microsoft.ProjectOxford.Face.Controls
{
    /// <summary>
    /// Interaction logic for FaceDetection.xaml
    /// </summary>
    public partial class FaceGroupingPage : Page
    {
        
        #region Fields

        /// <summary>
        /// Description dependency property
        /// </summary>
        public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register("Description", typeof(string), typeof(FaceGroupingPage));

        /// <summary>
        /// Faces to group
        /// </summary>
        private ObservableCollection<Face> _faces = new ObservableCollection<Face>();

        /// <summary>
        /// Grouping results
        /// </summary>
        private ObservableCollection<GroupingResult> _groupedFaces = new ObservableCollection<GroupingResult>();

        /// <summary>
        /// max concurrent process number for client query.
        /// </summary>
        private int _maxConcurrentProcesses;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FaceGroupingPage"/> class
        /// </summary>
        public FaceGroupingPage()
        {
            InitializeComponent();
            _maxConcurrentProcesses = 4;
        }

        #endregion Constructors

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
        /// Gets faces to group
        /// </summary>
        public ObservableCollection<Face> Faces
        {
            get
            {
                return _faces;
            }
        }

        /// <summary>
        /// Gets face grouping result
        /// </summary>
        public ObservableCollection<GroupingResult> GroupedFaces
        {
            get
            {
                return _groupedFaces;
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Pick folder, then group detected faces by similarity
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event arguments</param>
        private async void Grouping_Click(object sender, RoutedEventArgs e)
        {
            // Show folder picker
            System.Windows.Forms.FolderBrowserDialog dlg = new System.Windows.Forms.FolderBrowserDialog();
            var result = dlg.ShowDialog();

            // Set the suggestion count is intent to minimum the data preparation step only,
            // it's not corresponding to service side constraint
            const int SuggestionCount = 10;

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                // User picked one folder
                List<Task> tasks = new List<Task>();
                int processCount = 0;
                bool forceContinue = false;

                // Clear previous grouping result
                GroupedFaces.Clear();
                Faces.Clear();

                MainWindow mainWindow = Window.GetWindow(this) as MainWindow;
                string subscriptionKey = mainWindow._scenariosControl.SubscriptionKey;
                string endpoint = mainWindow._scenariosControl.SubscriptionEndpoint;
                var faceServiceClient = new FaceServiceClient(subscriptionKey, endpoint);

                MainWindow.Log("Request: Preparing faces for grouping, detecting faces in chosen folder.");
                
                var imageList =
                    new ConcurrentBag<string>(
                        Directory.EnumerateFiles(dlg.SelectedPath, "*.*", SearchOption.AllDirectories)
                            .Where(s => s.ToLower().EndsWith(".jpg") || s.ToLower().EndsWith(".png") || s.ToLower().EndsWith(".bmp") || s.ToLower().EndsWith(".gif")));

                string img;
                while (imageList.TryTake(out img))
                {
                    tasks.Add(Task.Factory.StartNew(
                        async (obj) =>
                        {
                            var imgPath = obj as string;                            
                            // Detect faces in image
                            using (var fStream = File.OpenRead(imgPath))
                            {
                                try
                                {
                                    var faces = await faceServiceClient.DetectAsync(fStream);
                                    return new Tuple<string, ClientContract.Face[]>(imgPath, faces);
                                }
                                catch (FaceAPIException ex)
                                {
                                    // if operation conflict, retry.
                                    if (ex.ErrorCode.Equals("ConcurrentOperationConflict"))
                                    {
                                        imageList.Add(imgPath);
                                        return null;
                                    }
                                    // Here we simply ignore all detection failure in this sample
                                    // You may handle these exceptions by check the Error.Error.Code and Error.Message property for ClientException object
                                    return new Tuple<string, ClientContract.Face[]>(imgPath, null);
                                }
                            }
                        },
                        img).Unwrap().ContinueWith((detectTask) =>
                        {
                            // Update detected faces on UI
                            var res = detectTask?.Result;
                            if (res?.Item2 == null)
                            {
                                return;
                            }

                            foreach (var f in res.Item2)
                            {
                                this.Dispatcher.Invoke(
                                    new Action<ObservableCollection<Face>, string, ClientContract.Face>(UIHelper.UpdateFace),
                                    Faces,
                                    res.Item1,
                                    f);
                            }
                        }));
                    if (processCount >= SuggestionCount && !forceContinue)
                    {
                        var continueProcess = System.Windows.Forms.MessageBox.Show("Found many images under chosen folder, may take long time if proceed. Continue?", "Warning", System.Windows.Forms.MessageBoxButtons.YesNo);
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
                
                MainWindow.Log("Response: Success. Total {0} faces are detected.", Faces.Count);

                try
                {
                   MainWindow.Log("Request: Grouping {0} faces.", Faces.Count);

                    // Call grouping, the grouping result is a group collection, each group contains similar faces
                    var groupRes = await faceServiceClient.GroupAsync(Faces.Select(f => Guid.Parse(f.FaceId)).ToArray());

                    // Update grouping results for rendering
                    foreach (var g in groupRes.Groups)
                    {
                        var gg = new GroupingResult()
                        {
                            Faces = new ObservableCollection<Face>(),
                            IsMessyGroup = false,
                        };

                        foreach (var fr in g)
                        {
                            gg.Faces.Add(Faces.First(f => f.FaceId == fr.ToString()));
                        }

                        GroupedFaces.Add(gg);
                    }

                    // MessyGroup contains all faces which are not similar to any other faces.
                    // Take an extreme case for example:
                    // On grouping faces which are not similar to any other faces, the grouping result will contains only one messy group
                    if (groupRes.MessyGroup.Length > 0)
                    {
                        var messyGroup = new GroupingResult()
                        {
                            Faces = new ObservableCollection<Face>(),
                            IsMessyGroup = true
                        };
                        foreach (var messy in groupRes.MessyGroup)
                        {
                            messyGroup.Faces.Add(Faces.First(f => f.FaceId == messy.ToString()));
                        }

                        GroupedFaces.Add(messyGroup);
                    }

                    MainWindow.Log("Response: Success. {0} faces are grouped into {1} groups.", Faces.Count, GroupedFaces.Count);
                }
                catch (FaceAPIException ex)
                {
                    MainWindow.Log("Response: {0}. {1}", ex.ErrorCode, ex.ErrorMessage);
                }
            }
            GC.Collect();
        }

        #endregion Methods

        #region Nested Types

        /// <summary>
        /// Grouping result for UI binding
        /// </summary>
        public class GroupingResult : INotifyPropertyChanged
        {
            #region Fields

            /// <summary>
            /// Grouped faces collection
            /// </summary>
            private ObservableCollection<Face> _faces = new ObservableCollection<Face>();

            /// <summary>
            /// Indicate whether the group is a messy group
            /// </summary>
            private bool _isMessyGroup = false;

            #endregion Fields

            #region Events

            /// <summary>
            /// Implement INotifyPropertyChanged interface
            /// </summary>
            public event PropertyChangedEventHandler PropertyChanged;

            #endregion Events

            #region Properties

            /// <summary>
            /// Gets or sets grouped faces collection
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
            /// Gets or sets a value indicating whether the group is a messy group
            /// </summary>
            public bool IsMessyGroup
            {
                get
                {
                    return _isMessyGroup;
                }

                set
                {
                    _isMessyGroup = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("Is"));
                    }
                }
            }

            #endregion Properties
        }

        #endregion Nested Types
    }
}