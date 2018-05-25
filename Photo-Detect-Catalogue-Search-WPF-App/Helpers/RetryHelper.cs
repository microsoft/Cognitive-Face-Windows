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

namespace Photo_Detect_Catalogue_Search_WPF_App.Helpers
{
    using Newtonsoft.Json.Serialization;
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// This class retries on transient errors
    /// </summary>
    public class RetryHelper
    {
        /// <summary>
        /// The singleton
        /// </summary>
        static RetryHelper _singleton;

        /// <summary>
        /// Prevents a default instance of the <see cref="RetryHelper"/> class from being created.
        /// </summary>
        private RetryHelper(){}

        /// <summary>
        /// Gets the current.
        /// </summary>
        /// <value>
        /// The current.
        /// </value>
        public static RetryHelper Current
        {
            get
            {
                if (_singleton == null)
                {
                    _singleton = new RetryHelper();
                }
                return _singleton;
            }
        }

        /// <summary>
        /// Operations the with basic retry asynchronous.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="asyncOperation">The asynchronous operation.</param>
        /// <param name="transientExceptionTypes">The transient exception types.</param>
        /// <param name="retryDelayMilliseconds">The retry delay milliseconds.</param>
        /// <param name="maxRetries">The maximum retries.</param>
        /// <param name="traceWriter">The trace writer.</param>
        /// <returns></returns>
        public static async Task<T> OperationWithBasicRetryAsync<T>(Func<Task<T>> asyncOperation, Type[] transientExceptionTypes, int retryDelayMilliseconds = 1000, int maxRetries = 60, ITraceWriter traceWriter = null)
        {
            int retryCount = 0;

            while (true)
            {
                try
                {
                    return await asyncOperation();
                }
                catch (Exception ex)
                    when (IsTransientError(ex, transientExceptionTypes))
                {
                    if (traceWriter != null)
                    {
                        traceWriter.Trace(System.Diagnostics.TraceLevel.Error, $"Error: {ex.Message}. Retrying {retryCount}/{maxRetries}", ex);
                    }

                    if (++retryCount >= maxRetries)
                    {
                        throw;
                    }

                    Thread.Sleep(retryDelayMilliseconds);
                }
            }
        }

        /// <summary>
        /// Voids the operation with basic retry asynchronous.
        /// </summary>
        /// <param name="asyncOperation">The asynchronous operation.</param>
        /// <param name="transientExceptionTypes">The transient exception types.</param>
        /// <param name="retryDelayMilliseconds">The retry delay milliseconds.</param>
        /// <param name="maxRetries">The maximum retries.</param>
        /// <param name="traceWriter">The trace writer.</param>
        /// <returns></returns>
        public static async Task VoidOperationWithBasicRetryAsync(Func<Task> asyncOperation, Type[] transientExceptionTypes, int retryDelayMilliseconds = 1000, int maxRetries = 60, ITraceWriter traceWriter = null)
        {
            int retryCount = 0;

            while (true)
            {
                try
                {
                    await asyncOperation();
                    return;
                }
                catch (Exception ex)
                    when (IsTransientError(ex, transientExceptionTypes))
                {
                    if (traceWriter != null)
                    {
                        traceWriter.Trace(System.Diagnostics.TraceLevel.Error, $"Error: {ex.Message}. Retrying {retryCount}/{maxRetries}", ex);
                    }

                    if (++retryCount >= maxRetries)
                    {
                        throw;
                    }

                    Thread.Sleep(retryDelayMilliseconds);
                }
            }
        }

        /// <summary>
        /// Determines whether [is transient error] [the specified ex].
        /// </summary>
        /// <param name="ex">The ex.</param>
        /// <param name="transientExceptionTypes">The transient exception types.</param>
        /// <returns>
        ///   <c>true</c> if [is transient error] [the specified ex]; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsTransientError(Exception ex, Type[] transientExceptionTypes)
        {
            return transientExceptionTypes.Contains(ex.GetType());
        }
    }
}
