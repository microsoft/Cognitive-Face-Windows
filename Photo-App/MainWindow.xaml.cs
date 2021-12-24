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

//
// SCENARIO: Photo Manager. Identify and tag photos automatically.
//
// This scenario combines several Face API features into a fully working example
// of a photo store management tool. With this tool, you can catalog all your
// photos and holiday snaps. Once the AI service is sufficiently trained, the
// hope is that it can identify and tag up to 80% of images. The tool also 
// enables you to show all photos containing a certain person.
//

namespace Photo_Detect_Catalogue_Search_WPF_App
{
    using System.ComponentModel;
    using System.Net;
    using System.Runtime.CompilerServices;
    using System.Windows;
    using Photo_Detect_Catalogue_Search_WPF_App.Controls;
    using Photo_Detect_Catalogue_Search_WPF_App.Helpers;
    using SampleUserControlLibrary;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        #region Constants

        private const string DefaultEndpoint = "https://westus.api.cognitive.microsoft.com/face/v1.0/";

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow" /> class
        /// </summary>
        public MainWindow()
        {
            FileTraceWriter.LogMessage("App Started");

            InitializeComponent();
            ServicePointManager.DefaultConnectionLimit = 1000;

            // You can use the next line to insert your own subscription key, instead of using UI to set license key.
            this.ViewModel = new MainViewModel()
            {
                FaceIdentificationDescription = "Tell whom an input face belongs to given a tagged person database. Here we only handle tagged person database in following format: 1). One root folder. 2). Sub-folders are named as person's name. 3). Each person's images are put into their own sub-folder. Pick the root folder, then choose an image to identify, all faces will be shown on the image with the identified person's name.",
                SortMyPhotosDescription = "Process your photo stores into a people searchable database.",
            };
            this.DataContext = this.ViewModel;
            this._scenariosControl.SampleScenarioList = new Scenario[]
            {
                new Scenario()
                {
                    PageClass = typeof(FaceIdentificationPage),
                    Title = "Face Identification",
                },
                new Scenario()
                {
                    PageClass = typeof(SortMyPhotosPage),
                    Title = "Sort My Photos",
                },
            };

            // Set the default endpoint when main windows is initiated.
            var mainWindow = GetWindow(this) as MainWindow;
            mainWindow?._scenariosControl.SetSubscriptionPageEndpoint(DefaultEndpoint);

            FileTraceWriter.LogMessage("Main Window constructed");
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets view model instance for MainWindow
        /// </summary>
        public MainViewModel ViewModel
        {
            get; private set;
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Log message in main window log pane
        /// </summary>
        /// <param name="format">format string</param>
        /// <param name="args">format arguments</param>
        public static void Log(string format, params object[] args)
        {
            ((MainWindow)Application.Current.MainWindow)._scenariosControl.Log(string.Format(format, args));
        }

        #endregion Methods

        #region Nested Types

        /// <summary>
        /// View model for MainWindow, covers display image, text
        /// </summary>
        public class MainViewModel : INotifyPropertyChanged
        {
            #region Events

            /// <summary>
            /// Implements INotifyPropertyChanged interface
            /// </summary>
            public event PropertyChangedEventHandler PropertyChanged;

            #endregion Events

            #region Properties

            /// <summary>
            /// Gets or sets description of identification 
            /// </summary>
            public string FaceIdentificationDescription
            {
                get;
                set;
            }

            /// <summary>
            /// Gets or sets description of identification 
            /// </summary>
            public string SortMyPhotosDescription
            {
                get;
                set;
            }

            #endregion Properties

            #region Methods

            /// <summary>
            /// Helper function for INotifyPropertyChanged interface 
            /// </summary>
            /// <typeparam name="T">Property type</typeparam>
            /// <param name="caller">Property name</param>
            private void OnPropertyChanged<T>([CallerMemberName]string caller = null)
            {
                var handler = PropertyChanged;
                if (handler != null)
                {
                    handler(this, new PropertyChangedEventArgs(caller));
                }
            }

            #endregion Methods
        }

        #endregion Nested Types
    }
}
