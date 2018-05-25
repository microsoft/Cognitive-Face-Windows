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
    using Photo_Detect_Catalogue_Search_WPF_App.Data;
    using Photo_Detect_Catalogue_Search_WPF_App.Models;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for ShowPersonMatchedFilesControl.xaml
    /// </summary>
    public partial class ShowPersonMatchedFilesControl : UserControl, INotifyPropertyChanged
    {
        /// <summary>
        /// The person
        /// </summary>
        private PersonExtended _person;

        /// <summary>
        /// The database
        /// </summary>
        private SqlDataProvider db = new SqlDataProvider();

        /// <summary>
        /// The matched files
        /// </summary>
        private ObservableCollection<PicturePerson> _matchedFiles = new ObservableCollection<PicturePerson>();

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets or sets the matched files.
        /// </summary>
        /// <value>
        /// The matched files.
        /// </value>
        public ObservableCollection<PicturePerson> MatchedFiles
        {
            get
            {
                return _matchedFiles;
            }
            set
            {
                _matchedFiles = value;
                PropertyChanged(this, new PropertyChangedEventArgs("MatchedFiles"));
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShowPersonMatchedFilesControl"/> class.
        /// </summary>
        /// <param name="person">The person.</param>
        public ShowPersonMatchedFilesControl(PersonExtended person)
        {
            _person = person;
            InitializeComponent();
            txtTitle.Text = $"Listing matched files for {person.Person.Name}";
            Loaded += ShowPersonMatchedFilesControl_Loaded;
        }

        /// <summary>
        /// Handles the Loaded event of the ShowPersonMatchedFilesControl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void ShowPersonMatchedFilesControl_Loaded(object sender, RoutedEventArgs e)
        {
            var people = db.GetFilesForPersonId(_person.Person.PersonId);
            foreach(var person in people)
            {
                // add value
                MatchedFiles.Add(person);
            }
        }

        /// <summary>
        /// Handles the Click event of the btnOpenFileLocation control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnOpenFileLocation_Click(object sender, RoutedEventArgs e)
        {
            var person = lstFiles.SelectedItem as PicturePerson;
            var directory = System.IO.Path.GetDirectoryName(person.PictureFile.FilePath);

            var runExplorer = new System.Diagnostics.ProcessStartInfo();
            runExplorer.FileName = "explorer.exe";
            runExplorer.Arguments = "/select,\"" + person.PictureFile.FilePath + "\"";
            System.Diagnostics.Process.Start(runExplorer);
        }
    }
}
