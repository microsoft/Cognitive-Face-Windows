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
    /// Definition of exposure level
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ExposureLevel
    {
        /// <summary>
        /// Indicating face image is in under exposure
        /// </summary>
        UnderExposure,

        /// <summary>
        /// Indicating face image is in good exposure
        /// </summary>
        GoodExposure,

        /// <summary>
        /// Indicating face image is in over exposure
        /// </summary>
        OverExposure
    }

    /// <summary>
    /// Face Exposure class contains exposure information
    /// </summary>
    public class Exposure
    {
        #region Properties

        /// <summary>
        /// Indicating exposure level of face image
        /// </summary>
        public ExposureLevel ExposureLevel
        {
            get; set;
        }

        /// <summary>
        /// Exposure value is in range [0, 1]. Larger value means the face image is more brighter.
        /// [0, 0.25) is under exposure.
        /// [0.25, 0.75) is good exposure.
        /// [0.75, 1] is over exposure.
        /// </summary>
        public double Value
        {
            get; set;
        }

        #endregion Properties
    }
}
