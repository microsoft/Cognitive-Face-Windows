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

namespace Microsoft.ProjectOxford.Face.Contract
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// Definition of blur level
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum BlurLevel
    {
        /// <summary>
        /// Low blur level indicating a clear face image
        /// </summary>
        Low,

        /// <summary>
        /// Medium blur level indicating a slightly blurry face image
        /// </summary>
        Medium,

        /// <summary>
        /// High blur level indicating a extremely blurry face image
        /// </summary>
        High
    }

    /// <summary>
    /// Face Blur class contains blur information
    /// </summary>
    public class Blur
    {
        #region Properties

        /// <summary>
        /// Indicating the blur level of face image
        /// </summary>
        public BlurLevel BlurLevel
        {
            get; set;
        }

        /// <summary>
        /// Blur value is in range [0, 1]. Larger value means the face image is more blurry.
        /// [0, 0.25) is low blur level.
        /// [0.25, 0.75) is medium blur level.
        /// [0.75, 1] is high blur level.
        /// </summary>
        public double Value
        {
            get; set;
        }

        #endregion Properties
    }
}
