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

namespace Photo_Detect_Catalogue_Search_WPF_App.Models
{
    using Microsoft.ProjectOxford.Face.Contract;
    using System.Collections.ObjectModel;

    /// <summary>
    /// Extends the LargePersonGroup class to include other useful bits, could change to inherited
    /// </summary>
    public class LargePersonGroupExtended
    {
        /// <summary>
        /// The group persons
        /// </summary>
        private ObservableCollection<PersonExtended> _groupPersons = new ObservableCollection<PersonExtended>();

        /// <summary>
        /// Gets or sets the group.
        /// </summary>
        /// <value>
        /// The group.
        /// </value>
        public LargePersonGroup Group { get; set; }

        /// <summary>
        /// Gets or sets the group persons.
        /// </summary>
        /// <value>
        /// The group persons.
        /// </value>
        public ObservableCollection<PersonExtended>GroupPersons
        {
            get
            {
                return _groupPersons;
            }
            set
            {
                _groupPersons = value;
            }
        }

        /// <summary>
        /// Gets or sets the selected person.
        /// </summary>
        /// <value>
        /// The selected person.
        /// </value>
        public PersonExtended SelectedPerson { get; set; }
    }
}
