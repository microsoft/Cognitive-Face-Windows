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
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ProjectOxford.Face.Contract;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Microsoft.ProjectOxford.Face
{
    using Face = Microsoft.ProjectOxford.Face.Contract.Face;

    /// <summary>
    /// The face service client proxy implementation.
    /// </summary>
    public class FaceServiceClient : IDisposable, IFaceServiceClient
    {
        #region Fields

        /// <summary>
        /// The default service host.
        /// </summary>
        private const string DEFAULT_API_ROOT = "https://westus.api.cognitive.microsoft.com/face/v1.0";

        /// <summary>
        /// The JSON content type header.
        /// </summary>
        private const string JsonContentTypeHeader = "application/json";

        /// <summary>
        /// The stream content type header.
        /// </summary>
        private const string StreamContentTypeHeader = "application/octet-stream";

        /// <summary>
        /// The subscription key name.
        /// </summary>
        private const string SubscriptionKeyName = "ocp-apim-subscription-key";

        /// <summary>
        /// The detect.
        /// </summary>
        private const string DetectQuery = "detect";

        /// <summary>
        /// The verify.
        /// </summary>
        private const string VerifyQuery = "verify";

        /// <summary>
        /// The train query.
        /// </summary>
        private const string TrainQuery = "train";

        /// <summary>
        /// The training query.
        /// </summary>
        private const string TrainingQuery = "training";

        /// <summary>
        /// The person groups.
        /// </summary>
        private const string PersonGroupsQuery = "persongroups";

        /// <summary>
        /// The person groups.
        /// </summary>
        private const string LargePersonGroupsQuery = "largepersongroups";

        /// <summary>
        /// The persons.
        /// </summary>
        private const string PersonsQuery = "persons";

        /// <summary>
        /// The persisted faces query string.
        /// </summary>
        private const string PersistedFacesQuery = "persistedfaces";

        /// <summary>
        /// The face list query
        /// </summary>
        private const string FaceListsQuery = "facelists";

        /// <summary>
        /// The face list query
        /// </summary>
        private const string LargeFaceListsQuery = "largefacelists";

        /// <summary>
        /// The endpoint for Find Similar API.
        /// </summary>
        private const string FindSimilarsQuery = "findsimilars";

        /// <summary>
        /// The identify.
        /// </summary>
        private const string IdentifyQuery = "identify";

        /// <summary>
        /// The group.
        /// </summary>
        private const string GroupQuery = "group";

        /// <summary>
        /// The settings
        /// </summary>
        private static JsonSerializerSettings s_settings = new JsonSerializerSettings()
        {
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            NullValueHandling = NullValueHandling.Ignore,
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        /// <summary>
        /// The subscription key.
        /// </summary>
        private readonly string _subscriptionKey;

        /// <summary>
        /// The root URI for the service endpoint.
        /// </summary>
        private readonly string _apiRoot;

        /// <summary>
        /// The HTTP client
        /// </summary>
        private HttpClient _httpClient;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FaceServiceClient"/> class.
        /// </summary>
        /// <param name="subscriptionKey">The subscription key.</param>
        public FaceServiceClient(string subscriptionKey) : this(subscriptionKey, DEFAULT_API_ROOT) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="FaceServiceClient"/> class.
        /// </summary>
        /// <param name="subscriptionKey">The subscription key.</param>
        /// <param name="apiRoot">Root URI for the service endpoint.</param>
        public FaceServiceClient(string subscriptionKey, string apiRoot)
        {
            this._subscriptionKey = subscriptionKey;
            this._apiRoot = apiRoot?.TrimEnd('/');
            this._httpClient = new HttpClient();
            this._httpClient.DefaultRequestHeaders.Add(SubscriptionKeyName, this._subscriptionKey);
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="FaceServiceClient"/> class.
        /// </summary>
        ~FaceServiceClient()
        {
            this.Dispose(false);
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets service endpoint address, overridable by subclasses, default to free subscription's endpoint.
        /// </summary>
        protected virtual string ServiceHost => this._apiRoot;

        /// <summary>
        /// Gets default request headers for all following http request
        /// </summary>
        public HttpRequestHeaders DefaultRequestHeaders
        {
            get
            {
                return this._httpClient.DefaultRequestHeaders;
            }
        }

        #endregion Properties

        #region Face Methods

        /// <summary>
        /// Detects an URL asynchronously.
        /// </summary>
        /// <param name="imageUrl">The image URL.</param>
        /// <param name="returnFaceId">If set to <c>true</c> [return face ID].</param>
        /// <param name="returnFaceLandmarks">If set to <c>true</c> [return face landmarks].</param>
        /// <param name="returnFaceAttributes">Face attributes need to be returned.</param>
        /// <returns>The detected faces.</returns>
        public async Task<Face[]> DetectAsync(string imageUrl, bool returnFaceId = true, bool returnFaceLandmarks = false, IEnumerable<FaceAttributeType> returnFaceAttributes = null)
        {
            if (returnFaceAttributes != null)
            {
                var requestUrl =
                    $"{this.ServiceHost}/{DetectQuery}?returnFaceId={returnFaceId}&returnFaceLandmarks={returnFaceLandmarks}&returnFaceAttributes={GetAttributeString(returnFaceAttributes)}";

                return await this.SendRequestAsync<object, Face[]>(HttpMethod.Post, requestUrl, new { url = imageUrl });
            }
            else
            {
                var requestUrl =
                    $"{this.ServiceHost}/{DetectQuery}?returnFaceId={returnFaceId}&returnFaceLandmarks={returnFaceLandmarks}";

                return await this.SendRequestAsync<object, Face[]>(HttpMethod.Post, requestUrl, new { url = imageUrl });
            }
        }

        /// <summary>
        /// Detects an image asynchronously.
        /// </summary>
        /// <param name="imageStream">The image stream.</param>
        /// <param name="returnFaceId">If set to <c>true</c> [return face ID].</param>
        /// <param name="returnFaceLandmarks">If set to <c>true</c> [return face landmarks].</param>
        /// <param name="returnFaceAttributes">Face attributes need to be returned.</param>
        /// <returns>The detected faces.</returns>
        public async Task<Face[]> DetectAsync(Stream imageStream, bool returnFaceId = true, bool returnFaceLandmarks = false, IEnumerable<FaceAttributeType> returnFaceAttributes = null)
        {
            if (returnFaceAttributes != null)
            {
                var requestUrl =
                    $"{this.ServiceHost}/{DetectQuery}?returnFaceId={returnFaceId}&returnFaceLandmarks={returnFaceLandmarks}&returnFaceAttributes={GetAttributeString(returnFaceAttributes)}";

                return await this.SendRequestAsync<Stream, Face[]>(HttpMethod.Post, requestUrl, imageStream);
            }
            else
            {
                var requestUrl =
                    $"{this.ServiceHost}/{DetectQuery}?returnFaceId={returnFaceId}&returnFaceLandmarks={returnFaceLandmarks}";

                return await this.SendRequestAsync<Stream, Face[]>(HttpMethod.Post, requestUrl, imageStream);
            }
        }

        /// <summary>
        /// Finds the similar faces asynchronously.
        /// </summary>
        /// <param name="faceId">The face identifier.</param>
        /// <param name="faceIds">The face identifiers.</param>
        /// <param name="maxNumOfCandidatesReturned">The max number of candidates returned.</param>
        /// <returns>
        /// The similar faces.
        /// </returns>
        public async Task<SimilarFace[]> FindSimilarAsync(Guid faceId, Guid[] faceIds, int maxNumOfCandidatesReturned = 20)
        {
            return await this.FindSimilarAsync(faceId, faceIds, FindSimilarMatchMode.matchPerson, maxNumOfCandidatesReturned);
        }

        /// <summary>
        /// Finds the similar faces asynchronously.
        /// </summary>
        /// <param name="faceId">The face identifier.</param>
        /// <param name="faceIds">The face identifiers.</param>
        /// <param name="mode">Algorithm mode option, default as "matchPerson".</param>
        /// <param name="maxNumOfCandidatesReturned">The max number of candidates returned.</param>
        /// <returns>
        /// The similar faces.
        /// </returns>
        public async Task<SimilarFace[]> FindSimilarAsync(Guid faceId, Guid[] faceIds, FindSimilarMatchMode mode, int maxNumOfCandidatesReturned = 20)
        {
            var requestUrl = $"{this.ServiceHost}/{FindSimilarsQuery}";

            return await this.SendRequestAsync<object, SimilarFace[]>(
                HttpMethod.Post,
                requestUrl,
                new
                {
                    faceId = faceId,
                    faceIds = faceIds,
                    maxNumOfCandidatesReturned = maxNumOfCandidatesReturned,
                    mode = mode.ToString()
                });
        }

        /// <summary>
        /// Finds the similar faces asynchronously.
        /// </summary>
        /// <param name="faceId">The face identifier.</param>
        /// <param name="faceListId">The face list identifier.</param>
        /// <param name="maxNumOfCandidatesReturned">The max number of candidates returned.</param>
        /// <returns>
        /// The similar persisted faces.
        /// </returns>
        public async Task<SimilarPersistedFace[]> FindSimilarAsync(Guid faceId, string faceListId, int maxNumOfCandidatesReturned = 20)
        {
            return await this.FindSimilarAsync(faceId, faceListId, FindSimilarMatchMode.matchPerson, maxNumOfCandidatesReturned);
        }

        /// <summary>
        /// Finds the similar faces asynchronously.
        /// </summary>
        /// <param name="faceId">The face identifier.</param>
        /// <param name="faceListId">The face list identifier.</param>
        /// <param name="mode">Algorithm mode option, default as "matchPerson".</param>
        /// <param name="maxNumOfCandidatesReturned">The max number of candidates returned.</param>
        /// <returns>
        /// The similar persisted faces.
        /// </returns>
        public async Task<SimilarPersistedFace[]> FindSimilarAsync(Guid faceId, string faceListId, FindSimilarMatchMode mode, int maxNumOfCandidatesReturned = 20)
        {
            return await this.FindSimilarAsync(faceId, faceListId, null, mode, maxNumOfCandidatesReturned);
        }

        /// <summary>
        /// Finds the similar faces asynchronously.
        /// </summary>
        /// <param name="faceId">The face identifier.</param>
        /// <param name="faceListId">The face list identifier.</param>
        /// <param name="largeFaceListId">The large face list identifier.</param>
        /// <param name="mode">Algorithm mode option, default as "matchPerson".</param>
        /// <param name="maxNumOfCandidatesReturned">The max number of candidates returned.</param>
        /// <returns>
        /// The similar persisted faces.
        /// </returns>
        public async Task<SimilarPersistedFace[]> FindSimilarAsync(
            Guid faceId,
            string faceListId = null,
            string largeFaceListId = null,
            FindSimilarMatchMode mode = FindSimilarMatchMode.matchPerson,
            int maxNumOfCandidatesReturned = 20)
        {
            var requestUrl = $"{this.ServiceHost}/{FindSimilarsQuery}";

            return await this.SendRequestAsync<object, SimilarPersistedFace[]>(
                       HttpMethod.Post,
                       requestUrl,
                       new
                           {
                               faceId = faceId,
                               faceListId = faceListId,
                               largeFaceListId = largeFaceListId,
                               maxNumOfCandidatesReturned = maxNumOfCandidatesReturned,
                               mode = mode.ToString()
                           });
        }

        /// <summary>
        /// Groups the face asynchronously.
        /// </summary>
        /// <param name="faceIds">The face ids.</param>
        /// <returns>
        /// Task object.
        /// </returns>
        public async Task<GroupResult> GroupAsync(Guid[] faceIds)
        {
            var requestUrl = $"{this.ServiceHost}/{GroupQuery}";

            return await this.SendRequestAsync<object, GroupResult>(
                HttpMethod.Post,
                requestUrl,
                new
                {
                    faceIds = faceIds
                });
        }

        /// <summary>
        /// Identities the faces in a given person group asynchronously.
        /// </summary>
        /// <param name="personGroupId">The person group id.</param>
        /// <param name="faceIds">The face ids.</param>
        /// <param name="maxNumOfCandidatesReturned">The maximum number of candidates returned for each face.</param>
        /// <returns>The identification results</returns>
        public async Task<IdentifyResult[]> IdentifyAsync(string personGroupId, Guid[] faceIds, int maxNumOfCandidatesReturned = 1)
        {
            return await this.IdentifyAsync(personGroupId, faceIds, 0.5f, maxNumOfCandidatesReturned);
        }

        /// <summary>
        /// Identities the faces in a given person group asynchronously.
        /// </summary>
        /// <param name="personGroupId">The person group id.</param>
        /// <param name="faceIds">The face ids.</param>
        /// <param name="confidenceThreshold">user-specified confidence threshold.</param>
        /// <param name="maxNumOfCandidatesReturned">The maximum number of candidates returned for each face.</param>
        /// <returns>The identification results</returns>
        public async Task<IdentifyResult[]> IdentifyAsync(string personGroupId, Guid[] faceIds, float confidenceThreshold, int maxNumOfCandidatesReturned = 1)
        {
            return await this.IdentifyAsync(faceIds, personGroupId, null, confidenceThreshold, maxNumOfCandidatesReturned);
        }

        /// <summary>
        /// Identities the faces in a given person group or large person group asynchronously.
        /// </summary>
        /// <param name="faceIds">The face ids.</param>
        /// <param name="personGroupId">The person group id.</param>
        /// <param name="largePersonGroupId">The large person group id.</param>
        /// <param name="confidenceThreshold">user-specified confidence threshold.</param>
        /// <param name="maxNumOfCandidatesReturned">The maximum number of candidates returned for each face.</param>
        /// <returns>The identification results</returns>
        public async Task<IdentifyResult[]> IdentifyAsync(
            Guid[] faceIds,
            string personGroupId = null,
            string largePersonGroupId = null,
            float confidenceThreshold = 0.5f,
            int maxNumOfCandidatesReturned = 1)
        {
            var requestUrl = $"{this.ServiceHost}/{IdentifyQuery}";

            return await this.SendRequestAsync<object, IdentifyResult[]>(
                HttpMethod.Post,
                requestUrl,
                new
                {
                    faceIds = faceIds,
                    personGroupId = personGroupId,
                    largePersonGroupId = largePersonGroupId,
                    confidenceThreshold = confidenceThreshold,
                    maxNumOfCandidatesReturned = maxNumOfCandidatesReturned
                });
        }

        /// <summary>
        /// Verifies whether the specified two faces belong to the same person asynchronously.
        /// </summary>
        /// <param name="faceId1">The face id 1.</param>
        /// <param name="faceId2">The face id 2.</param>
        /// <returns>The verification result.</returns>
        public async Task<VerifyResult> VerifyAsync(Guid faceId1, Guid faceId2)
        {
            var requestUrl = $"{this.ServiceHost}/{VerifyQuery}";

            return await this.SendRequestAsync<object, VerifyResult>(
                HttpMethod.Post,
                requestUrl,
                new
                {
                    faceId1 = faceId1,
                    faceId2 = faceId2
                });
        }

        /// <summary>
        /// Verifies whether the specified face belong to the specified person asynchronously.
        /// </summary>
        /// <param name="faceId">the face ID</param>
        /// <param name="personGroupId">the person group ID</param>
        /// <param name="personId">the person ID</param>
        /// <returns>The verification result.</returns>
        public async Task<VerifyResult> VerifyAsync(Guid faceId, string personGroupId, Guid personId)
        {
            return await this.VerifyAsync(faceId, personId, personGroupId);
        }

        /// <summary>
        /// Verify whether one face belong to a person.
        /// </summary>
        /// <param name="faceId">The face id.</param>
        /// <param name="personId">The person id.</param>
        /// <param name="personGroupId">The person group id.</param>
        /// <param name="largePersonGroupId">The large person group id.</param>
        /// <returns>Verification result.</returns>
        public async Task<VerifyResult> VerifyAsync(
            Guid faceId,
            Guid personId,
            string personGroupId = null,
            string largePersonGroupId = null)
        {
            var requestUrl = $"{this.ServiceHost}/{VerifyQuery}";

            return await this.SendRequestAsync<object, VerifyResult>(
                HttpMethod.Post,
                requestUrl,
                new
                {
                    faceId = faceId,
                    personGroupId = personGroupId,
                    largePersonGroupId = largePersonGroupId,
                    personId = personId
                });
        }

        #endregion

        #region FaceList Methods

        /// <summary>
        /// Creates the face list asynchronously.
        /// </summary>
        /// <param name="faceListId">The face list identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="userData">The user data.</param>
        /// <returns>
        /// Task object.
        /// </returns>
        public async Task CreateFaceListAsync(string faceListId, string name, string userData = null)
        {
            var requestUrl = $"{this.ServiceHost}/{FaceListsQuery}/{faceListId}";

            await this.SendRequestAsync<object, object>(
                HttpMethod.Put,
                requestUrl,
                new
                {
                    name = name,
                    userData = userData
                });
        }

        /// <summary>
        /// Deletes the face list asynchronously.
        /// </summary>
        /// <param name="faceListId">The face list identifier.</param>
        /// <returns>
        /// Task object.
        /// </returns>
        public async Task DeleteFaceListAsync(string faceListId)
        {
            var requestUrl = $"{this.ServiceHost}/{FaceListsQuery}/{faceListId}";

            await this.SendRequestAsync<object, object>(HttpMethod.Delete, requestUrl, null);
        }

        /// <summary>
        /// Gets the face list asynchronously.
        /// </summary>
        /// <param name="faceListId">The face list identifier.</param>
        /// <returns>
        /// Face list object.
        /// </returns>
        public async Task<FaceList> GetFaceListAsync(string faceListId)
        {
            var requestUrl = $"{this.ServiceHost}/{FaceListsQuery}/{faceListId}";

            return await this.SendRequestAsync<object, FaceList>(HttpMethod.Get, requestUrl, null);
        }

        /// <summary>
        /// List the face lists asynchronously.
        /// </summary>
        /// <returns>
        /// FaceListMetadata array.
        /// </returns>
        public async Task<FaceListMetadata[]> ListFaceListsAsync()
        {
            var requestUrl = $"{this.ServiceHost}/{FaceListsQuery}";

            return await this.SendRequestAsync<object, FaceListMetadata[]>(HttpMethod.Get, requestUrl, null);
        }

        /// <summary>
        /// Updates the face list asynchronously.
        /// </summary>
        /// <param name="faceListId">The face list identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="userData">The user data.</param>
        /// <returns>
        /// Task object.
        /// </returns>
        public async Task UpdateFaceListAsync(string faceListId, string name, string userData)
        {
            var requestUrl = $"{this.ServiceHost}/{FaceListsQuery}/{faceListId}";

            await this.SendRequestAsync<object, object>(
                new HttpMethod("PATCH"),
                requestUrl,
                new
                {
                    name = name,
                    userData = userData
                });
        }

        #endregion

        #region FaceList Face Methods

        /// <summary>
        /// Adds the face to face list asynchronously.
        /// </summary>
        /// <param name="faceListId">The face list identifier.</param>
        /// <param name="imageUrl">The face image URL.</param>
        /// <param name="userData">The user data.</param>
        /// <param name="targetFace">The target face.</param>
        /// <returns>
        /// Add face result.
        /// </returns>
        public async Task<AddPersistedFaceResult> AddFaceToFaceListAsync(
            string faceListId,
            string imageUrl,
            string userData = null,
            FaceRectangle targetFace = null)
        {
            var targetFaceString = targetFace == null
                                       ? string.Empty
                                       : $"{targetFace.Left},{targetFace.Top},{targetFace.Width},{targetFace.Height}";
            var requestUrl =
                $"{this.ServiceHost}/{FaceListsQuery}/{faceListId}/{PersistedFacesQuery}?userData={userData}&targetFace={targetFaceString}";

            return await this.SendRequestAsync<object, AddPersistedFaceResult>(
                       HttpMethod.Post,
                       requestUrl,
                       new { url = imageUrl });
        }

        /// <summary>
        /// Adds the face to face list asynchronously.
        /// </summary>
        /// <param name="faceListId">The face list identifier.</param>
        /// <param name="imageStream">The face image stream.</param>
        /// <param name="userData">The user data.</param>
        /// <param name="targetFace">The target face.</param>
        /// <returns>
        /// Add face result.
        /// </returns>
        public async Task<AddPersistedFaceResult> AddFaceToFaceListAsync(
            string faceListId,
            Stream imageStream,
            string userData = null,
            FaceRectangle targetFace = null)
        {
            var targetFaceString = targetFace == null
                                       ? string.Empty
                                       : $"{targetFace.Left},{targetFace.Top},{targetFace.Width},{targetFace.Height}";
            var requestUrl =
                $"{this.ServiceHost}/{FaceListsQuery}/{faceListId}/{PersistedFacesQuery}?userData={userData}&targetFace={targetFaceString}";

            return await this.SendRequestAsync<object, AddPersistedFaceResult>(
                       HttpMethod.Post,
                       requestUrl,
                       imageStream);
        }

        /// <summary>
        /// Deletes the face from face list asynchronously.
        /// </summary>
        /// <param name="faceListId">The face list identifier.</param>
        /// <param name="persistedFaceId">The persisted face id.</param>
        /// <returns>Task object.</returns>
        public async Task DeleteFaceFromFaceListAsync(string faceListId, Guid persistedFaceId)
        {
            var requestUrl =
                $"{this.ServiceHost}/{FaceListsQuery}/{faceListId}/{PersistedFacesQuery}/{persistedFaceId}";

            await this.SendRequestAsync<object, object>(HttpMethod.Delete, requestUrl, null);
        }

        #endregion

        #region PersonGroup Methods

        /// <summary>
        /// Creates the person group asynchronously.
        /// </summary>
        /// <param name="personGroupId">The person group identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="userData">The user data.</param>
        /// <returns>Task object.</returns>
        public async Task CreatePersonGroupAsync(string personGroupId, string name, string userData = null)
        {
            var requestUrl = $"{this.ServiceHost}/{PersonGroupsQuery}/{personGroupId}";

            await this.SendRequestAsync<object, object>(
                HttpMethod.Put,
                requestUrl,
                new
                {
                    name = name,
                    userData = userData
                });
        }

        /// <summary>
        /// Deletes a person group asynchronously.
        /// </summary>
        /// <param name="personGroupId">The person group id.</param>
        /// <returns>Task object.</returns>
        public async Task DeletePersonGroupAsync(string personGroupId)
        {
            var requestUrl = $"{this.ServiceHost}/{PersonGroupsQuery}/{personGroupId}";

            await this.SendRequestAsync<object, object>(HttpMethod.Delete, requestUrl, null);
        }

        /// <summary>
        /// Gets a person group asynchronously.
        /// </summary>
        /// <param name="personGroupId">The person group id.</param>
        /// <returns>The person group entity.</returns>
        public async Task<PersonGroup> GetPersonGroupAsync(string personGroupId)
        {
            var requestUrl = $"{this.ServiceHost}/{PersonGroupsQuery}/{personGroupId}";

            return await this.SendRequestAsync<object, PersonGroup>(HttpMethod.Get, requestUrl, null);
        }

        /// <summary>
        /// Gets person group training status asynchronously.
        /// </summary>
        /// <param name="personGroupId">The person group id.</param>
        /// <returns>The person group training status.</returns>
        public async Task<TrainingStatus> GetPersonGroupTrainingStatusAsync(string personGroupId)
        {
            var requestUrl = $"{this.ServiceHost}/{PersonGroupsQuery}/{personGroupId}/{TrainingQuery}";

            return await this.SendRequestAsync<object, TrainingStatus>(HttpMethod.Get, requestUrl, null);
        }

        /// <summary>
        /// Gets person groups asynchronously.
        /// </summary>
        /// <returns>Person group entity array.</returns>
        [Obsolete("use ListPersonGroupsAsync instead")]
        public async Task<PersonGroup[]> GetPersonGroupsAsync()
        {
            return await this.ListPersonGroupsAsync();
        }

        /// <summary>
        /// Asynchronously list the top person groups whose Id is larger than "start".
        /// </summary>
        /// <param name="start">person group Id bar. List the person groups whose Id is larger than "start".</param>
        /// <param name="top">the number of person groups to list.</param>
        /// <returns>Person group entity array.</returns>
        public async Task<PersonGroup[]> ListPersonGroupsAsync(string start = "", int top = 1000)
        {
            var requestUrl =
                $"{this.ServiceHost}/{PersonGroupsQuery}?start={start}&top={top.ToString(CultureInfo.InvariantCulture)}";

            return await this.SendRequestAsync<object, PersonGroup[]>(HttpMethod.Get, requestUrl, null);
        }

        /// <summary>
        /// Trains the person group asynchronously.
        /// </summary>
        /// <param name="personGroupId">The person group id.</param>
        /// <returns>Task object.</returns>
        public async Task TrainPersonGroupAsync(string personGroupId)
        {
            var requestUrl = $"{this.ServiceHost}/{PersonGroupsQuery}/{personGroupId}/{TrainQuery}";

            await this.SendRequestAsync<object, object>(HttpMethod.Post, requestUrl, null);
        }

        /// <summary>
        /// Updates a person group asynchronously.
        /// </summary>
        /// <param name="personGroupId">The person group id.</param>
        /// <param name="name">The name.</param>
        /// <param name="userData">The user data.</param>
        /// <returns>Task object.</returns>
        public async Task UpdatePersonGroupAsync(string personGroupId, string name, string userData = null)
        {
            var requestUrl = $"{this.ServiceHost}/{PersonGroupsQuery}/{personGroupId}";

            await this.SendRequestAsync<object, object>(
                new HttpMethod("PATCH"),
                requestUrl,
                new
                {
                    name = name,
                    userData = userData
                });
        }

        #endregion

        #region PersonGroup Person Methods

        /// <summary>
        /// Creates a person asynchronously.
        /// </summary>
        /// <param name="personGroupId">The person group id.</param>
        /// <param name="name">The name.</param>
        /// <param name="userData">The user data.</param>
        /// <returns>The CreatePersonResult entity.</returns>
        public async Task<CreatePersonResult> CreatePersonAsync(
            string personGroupId,
            string name,
            string userData = null)
        {
            return await this.CreatePersonInPersonGroupAsync(personGroupId, name, userData);
        }

        /// <summary>
        /// Creates a person asynchronously.
        /// </summary>
        /// <param name="personGroupId">The person group id.</param>
        /// <param name="name">The name.</param>
        /// <param name="userData">The user data.</param>
        /// <returns>The CreatePersonResult entity.</returns>
        public async Task<CreatePersonResult> CreatePersonInPersonGroupAsync(
            string personGroupId,
            string name,
            string userData = null)
        {
            var requestUrl = $"{this.ServiceHost}/{PersonGroupsQuery}/{personGroupId}/{PersonsQuery}";

            return await this.SendRequestAsync<object, CreatePersonResult>(
                       HttpMethod.Post,
                       requestUrl,
                       new { name = name, userData = userData });
        }

        /// <summary>
        /// Deletes a person asynchronously.
        /// </summary>
        /// <param name="personGroupId">The person group id.</param>
        /// <param name="personId">The person id.</param>
        /// <returns>Task object.</returns>
        public async Task DeletePersonAsync(string personGroupId, Guid personId)
        {
            await this.DeletePersonFromPersonGroupAsync(personGroupId, personId);
        }

        /// <summary>
        /// Deletes a person asynchronously.
        /// </summary>
        /// <param name="personGroupId">The person group id.</param>
        /// <param name="personId">The person id.</param>
        /// <returns>Task object.</returns>
        public async Task DeletePersonFromPersonGroupAsync(string personGroupId, Guid personId)
        {
            var requestUrl = $"{this.ServiceHost}/{PersonGroupsQuery}/{personGroupId}/{PersonsQuery}/{personId}";
            await this.SendRequestAsync<object, object>(HttpMethod.Delete, requestUrl, null);
        }

        /// <summary>
        /// Gets a person asynchronously.
        /// </summary>
        /// <param name="personGroupId">The person group id.</param>
        /// <param name="personId">The person id.</param>
        /// <returns>The person entity.</returns>
        public async Task<Person> GetPersonAsync(string personGroupId, Guid personId)
        {
            return await this.GetPersonInPersonGroupAsync(personGroupId, personId);
        }

        /// <summary>
        /// Gets a person asynchronously.
        /// </summary>
        /// <param name="personGroupId">The person group id.</param>
        /// <param name="personId">The person id.</param>
        /// <returns>The person entity.</returns>
        public async Task<Person> GetPersonInPersonGroupAsync(string personGroupId, Guid personId)
        {
            var requestUrl = $"{this.ServiceHost}/{PersonGroupsQuery}/{personGroupId}/{PersonsQuery}/{personId}";

            return await this.SendRequestAsync<object, Person>(HttpMethod.Get, requestUrl, null);
        }

        /// <summary>
        /// Gets persons inside a person group asynchronously.
        /// </summary>
        /// <param name="personGroupId">The person group id.</param>
        /// <returns>
        /// The person entity array.
        /// </returns>
        [Obsolete("use ListPersonsAsync instead")]
        public async Task<Person[]> GetPersonsAsync(string personGroupId)
        {
            return await this.ListPersonsAsync(personGroupId);
        }

        /// <summary>
        /// List the top persons whose Id is larger than "start" inside a person group asynchronously.
        /// </summary>
        /// <param name="personGroupId">The person group id.</param>
        /// <param name="start">Person Id bar. List the persons whose Id is larger than "start".</param>
        /// <param name="top">The number of persons to list.</param>>
        /// <returns>
        /// The person entity array.
        /// </returns>
        public async Task<Person[]> ListPersonsAsync(string personGroupId, string start = "", int top = 1000)
        {
            return await this.ListPersonsInPersonGroupAsync(personGroupId, start, top);
        }

        /// <summary>
        /// List the top persons whose Id is larger than "start" inside a person group asynchronously.
        /// </summary>
        /// <param name="personGroupId">The person group id.</param>
        /// <param name="start">Person Id bar. List the persons whose Id is larger than "start".</param>
        /// <param name="top">The number of persons to list.</param>>
        /// <returns>
        /// The person entity array.
        /// </returns>
        public async Task<Person[]> ListPersonsInPersonGroupAsync(
            string personGroupId,
            string start = "",
            int top = 1000)
        {
            var requestUrl =
                $"{this.ServiceHost}/{PersonGroupsQuery}/{personGroupId}/{PersonsQuery}?start={start}&top={top.ToString(CultureInfo.InvariantCulture)}";

            return await this.SendRequestAsync<object, Person[]>(HttpMethod.Get, requestUrl, null);
        }

        /// <summary>
        /// Updates a person asynchronously.
        /// </summary>
        /// <param name="personGroupId">The person group id.</param>
        /// <param name="personId">The person id.</param>
        /// <param name="name">The name.</param>
        /// <param name="userData">The user data.</param>
        /// <returns>Task object.</returns>
        public async Task UpdatePersonAsync(string personGroupId, Guid personId, string name, string userData = null)
        {
            await this.UpdatePersonInPersonGroupAsync(personGroupId, personId, name, userData);
        }

        /// <summary>
        /// Updates a person asynchronously.
        /// </summary>
        /// <param name="personGroupId">The person group id.</param>
        /// <param name="personId">The person id.</param>
        /// <param name="name">The name.</param>
        /// <param name="userData">The user data.</param>
        /// <returns>Task object.</returns>
        public async Task UpdatePersonInPersonGroupAsync(
            string personGroupId,
            Guid personId,
            string name,
            string userData = null)
        {
            var requestUrl = $"{this.ServiceHost}/{PersonGroupsQuery}/{personGroupId}/{PersonsQuery}/{personId}";

            await this.SendRequestAsync<object, object>(
                new HttpMethod("PATCH"),
                requestUrl,
                new { name = name, userData = userData });
        }

        #endregion

        #region PersonGroup PersonFace Methods

        /// <summary>
        /// Adds a face to a person asynchronously.
        /// </summary>
        /// <param name="personGroupId">The person group id.</param>
        /// <param name="personId">The person id.</param>
        /// <param name="imageUrl">The face image URL.</param>
        /// <param name="userData">The user data.</param>
        /// <param name="targetFace">The target face.</param>
        /// <returns>
        /// Add person face result.
        /// </returns>
        public async Task<AddPersistedFaceResult> AddPersonFaceAsync(
            string personGroupId,
            Guid personId,
            string imageUrl,
            string userData = null,
            FaceRectangle targetFace = null)
        {
            return await this.AddPersonFaceInPersonGroupAsync(personGroupId, personId, imageUrl, userData, targetFace);
        }

        /// <summary>
        /// Adds a face to a person asynchronously.
        /// </summary>
        /// <param name="personGroupId">The person group id.</param>
        /// <param name="personId">The person id.</param>
        /// <param name="imageUrl">The face image URL.</param>
        /// <param name="userData">The user data.</param>
        /// <param name="targetFace">The target face.</param>
        /// <returns>
        /// Add person face result.
        /// </returns>
        public async Task<AddPersistedFaceResult> AddPersonFaceInPersonGroupAsync(
            string personGroupId,
            Guid personId,
            string imageUrl,
            string userData = null,
            FaceRectangle targetFace = null)
        {
            var targetFaceString = targetFace == null
                                       ? string.Empty
                                       : $"{targetFace.Left},{targetFace.Top},{targetFace.Width},{targetFace.Height}";
            var requestUrl =
                $"{this.ServiceHost}/{PersonGroupsQuery}/{personGroupId}/{PersonsQuery}/{personId}/{PersistedFacesQuery}?userData={userData}&targetFace={targetFaceString}";

            return await this.SendRequestAsync<object, AddPersistedFaceResult>(
                       HttpMethod.Post,
                       requestUrl,
                       new { url = imageUrl });
        }

        /// <summary>
        /// Adds a face to a person asynchronously.
        /// </summary>
        /// <param name="personGroupId">The person group id.</param>
        /// <param name="personId">The person id.</param>
        /// <param name="imageStream">The face image stream.</param>
        /// <param name="userData">The user data.</param>
        /// <param name="targetFace">The target face.</param>
        /// <returns>
        /// Add person face result.
        /// </returns>
        public async Task<AddPersistedFaceResult> AddPersonFaceAsync(
            string personGroupId,
            Guid personId,
            Stream imageStream,
            string userData = null,
            FaceRectangle targetFace = null)
        {
            return await this.AddPersonFaceInPersonGroupAsync(
                       personGroupId,
                       personId,
                       imageStream,
                       userData,
                       targetFace);
        }

        /// <summary>
        /// Adds a face to a person asynchronously.
        /// </summary>
        /// <param name="personGroupId">The person group id.</param>
        /// <param name="personId">The person id.</param>
        /// <param name="imageStream">The face image stream.</param>
        /// <param name="userData">The user data.</param>
        /// <param name="targetFace">The target face.</param>
        /// <returns>
        /// Add person face result.
        /// </returns>
        public async Task<AddPersistedFaceResult> AddPersonFaceInPersonGroupAsync(
            string personGroupId,
            Guid personId,
            Stream imageStream,
            string userData = null,
            FaceRectangle targetFace = null)
        {
            var targetFaceString = targetFace == null
                                       ? string.Empty
                                       : $"{targetFace.Left},{targetFace.Top},{targetFace.Width},{targetFace.Height}";
            var requestUrl =
                $"{this.ServiceHost}/{PersonGroupsQuery}/{personGroupId}/{PersonsQuery}/{personId}/{PersistedFacesQuery}?userData={userData}&targetFace={targetFaceString}";

            return await this.SendRequestAsync<Stream, AddPersistedFaceResult>(
                       HttpMethod.Post,
                       requestUrl,
                       imageStream);
        }

        /// <summary>
        /// Deletes a face of a person asynchronously.
        /// </summary>
        /// <param name="personGroupId">The person group id.</param>
        /// <param name="personId">The person id.</param>
        /// <param name="persistedFaceId">The persisted face id.</param>
        /// <returns>
        /// Task object.
        /// </returns>
        public async Task DeletePersonFaceAsync(string personGroupId, Guid personId, Guid persistedFaceId)
        {
            await this.DeletePersonFaceFromPersonGroupAsync(personGroupId, personId, persistedFaceId);
        }

        /// <summary>
        /// Deletes a face of a person asynchronously.
        /// </summary>
        /// <param name="personGroupId">The person group id.</param>
        /// <param name="personId">The person id.</param>
        /// <param name="persistedFaceId">The persisted face id.</param>
        /// <returns>
        /// Task object.
        /// </returns>
        public async Task DeletePersonFaceFromPersonGroupAsync(
            string personGroupId,
            Guid personId,
            Guid persistedFaceId)
        {
            var requestUrl =
                $"{this.ServiceHost}/{PersonGroupsQuery}/{personGroupId}/{PersonsQuery}/{personId}/{PersistedFacesQuery}/{persistedFaceId}";

            await this.SendRequestAsync<object, object>(HttpMethod.Delete, requestUrl, null);
        }

        /// <summary>
        /// Gets a face of a person asynchronously.
        /// </summary>
        /// <param name="personGroupId">The person group id.</param>
        /// <param name="personId">The person id.</param>
        /// <param name="persistedFaceId">The persisted face id.</param>
        /// <returns>
        /// The person face entity.
        /// </returns>
        public async Task<PersistedFace> GetPersonFaceAsync(string personGroupId, Guid personId, Guid persistedFaceId)
        {
            return await this.GetPersonFaceInPersonGroupAsync(personGroupId, personId, persistedFaceId);
        }

        /// <summary>
        /// Gets a face of a person asynchronously.
        /// </summary>
        /// <param name="personGroupId">The person group id.</param>
        /// <param name="personId">The person id.</param>
        /// <param name="persistedFaceId">The persisted face id.</param>
        /// <returns>
        /// The person face entity.
        /// </returns>
        public async Task<PersistedFace> GetPersonFaceInPersonGroupAsync(string personGroupId, Guid personId, Guid persistedFaceId)
        {
            var requestUrl =
                $"{this.ServiceHost}/{PersonGroupsQuery}/{personGroupId}/{PersonsQuery}/{personId}/{PersistedFacesQuery}/{persistedFaceId}";

            return await this.SendRequestAsync<object, PersistedFace>(HttpMethod.Get, requestUrl, null);
        }

        /// <summary>
        /// Updates a face of a person asynchronously.
        /// </summary>
        /// <param name="personGroupId">The person group id.</param>
        /// <param name="personId">The person id.</param>
        /// <param name="persistedFaceId">The persisted face id.</param>
        /// <param name="userData">The user data.</param>
        /// <returns>
        /// Task object.
        /// </returns>
        public async Task UpdatePersonFaceAsync(
            string personGroupId,
            Guid personId,
            Guid persistedFaceId,
            string userData)
        {
            await this.UpdatePersonFaceInPersonGroupAsync(personGroupId, personId, persistedFaceId, userData);
        }

        /// <summary>
        /// Updates a face of a person asynchronously.
        /// </summary>
        /// <param name="personGroupId">The person group id.</param>
        /// <param name="personId">The person id.</param>
        /// <param name="persistedFaceId">The persisted face id.</param>
        /// <param name="userData">The user data.</param>
        /// <returns>
        /// Task object.
        /// </returns>
        public async Task UpdatePersonFaceInPersonGroupAsync(
            string personGroupId,
            Guid personId,
            Guid persistedFaceId,
            string userData)
        {
            var requestUrl =
                $"{this.ServiceHost}/{PersonGroupsQuery}/{personGroupId}/{PersonsQuery}/{personId}/{PersistedFacesQuery}/{persistedFaceId}";

            await this.SendRequestAsync<object, object>(
                new HttpMethod("PATCH"),
                requestUrl,
                new { userData = userData });
        }

        #endregion

        #region LargeFaceList Methods

        /// <summary>
        /// Creates the large face list asynchronously.
        /// </summary>
        /// <param name="largeFaceListId">The large face list identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="userData">The user data.</param>
        /// <returns>Task object.</returns>
        public async Task CreateLargeFaceListAsync(string largeFaceListId, string name, string userData = null)
        {
            var requestUrl = $"{this.ServiceHost}/{LargeFaceListsQuery}/{largeFaceListId}";

            await this.SendRequestAsync<object, object>(
                HttpMethod.Put,
                requestUrl,
                new { name = name, userData = userData });
        }


        /// <summary>
        /// Deletes the large face list asynchronously.
        /// </summary>
        /// <param name="largeFaceListId">The large face list identifier.</param>
        /// <returns>public async Task object.</returns>
        public async Task DeleteLargeFaceListAsync(string largeFaceListId)
        {
            var requestUrl = $"{this.ServiceHost}/{LargeFaceListsQuery}/{largeFaceListId}";

            await this.SendRequestAsync<object, object>(HttpMethod.Delete, requestUrl, null);
        }

        /// <summary>
        /// Gets the large face list asynchronously.
        /// </summary>
        /// <param name="largeFaceListId">The large face list identifier.</param>
        /// <returns>Face list object.</returns>
        public async Task<LargeFaceList> GetLargeFaceListAsync(string largeFaceListId)
        {
            var requestUrl = $"{this.ServiceHost}/{LargeFaceListsQuery}/{largeFaceListId}";

            return await this.SendRequestAsync<object, LargeFaceList>(HttpMethod.Get, requestUrl, null);
        }

        /// <summary>
        /// Gets large face list training status asynchronously.
        /// </summary>
        /// <param name="largeFaceListId">The large face list id.</param>
        /// <returns>The large face list training status.</returns>
        public async Task<TrainingStatus> GetLargeFaceListTrainingStatusAsync(string largeFaceListId)
        {
            var requestUrl = $"{this.ServiceHost}/{LargeFaceListsQuery}/{largeFaceListId}/{TrainingQuery}";

            return await this.SendRequestAsync<object, TrainingStatus>(HttpMethod.Get, requestUrl, null);
        }

        /// <summary>
        /// Lists the large face lists asynchronously.
        /// </summary>
        /// <param name="start">The start point string in listing large face lists</param>
        /// <param name="top">The number of large face lists to list</param>
        /// <returns>LargeFaceListMetadata array.</returns>
        public async Task<LargeFaceList[]> ListLargeFaceListsAsync(string start = "", int top = 1000)
        {
            var requestUrl =
                $"{this.ServiceHost}/{LargeFaceListsQuery}?start={start}&top={top.ToString(CultureInfo.InvariantCulture)}";

            return await this.SendRequestAsync<object, LargeFaceList[]>(HttpMethod.Get, requestUrl, null);
        }

        /// <summary>
        /// Trains the large face list asynchronously.
        /// </summary>
        /// <param name="largeFaceListId">The large face list id.</param>
        /// <returns>public async Task object.</returns>
        public async Task TrainLargeFaceListAsync(string largeFaceListId)
        {
            var requestUrl = $"{this.ServiceHost}/{LargeFaceListsQuery}/{largeFaceListId}/{TrainQuery}";

            await this.SendRequestAsync<object, object>(HttpMethod.Post, requestUrl, null);
        }

        /// <summary>
        /// Updates the large face list asynchronously.
        /// </summary>
        /// <param name="largeFaceListId">The large face list identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="userData">The user data.</param>
        /// <returns>public async Task object.</returns>
        public async Task UpdateLargeFaceListAsync(string largeFaceListId, string name, string userData)
        {
            var requestUrl = $"{this.ServiceHost}/{LargeFaceListsQuery}/{largeFaceListId}";

            await this.SendRequestAsync<object, object>(
                new HttpMethod("PATCH"),
                requestUrl,
                new { name = name, userData = userData });
        }

        #endregion

        #region LargeFaceList Face Methods

        /// <summary>
        /// Adds the face to large face list asynchronously.
        /// </summary>
        /// <param name="largeFaceListId">The large face list identifier.</param>
        /// <param name="imageUrl">The face image URL.</param>
        /// <param name="userData">The user data.</param>
        /// <param name="targetFace">The target face.</param>
        /// <returns>
        /// Add face result.
        /// </returns>
        public async Task<AddPersistedFaceResult> AddFaceToLargeFaceListAsync(
            string largeFaceListId,
            string imageUrl,
            string userData = null,
            FaceRectangle targetFace = null)
        {
            var targetFaceString = targetFace == null
                                       ? string.Empty
                                       : $"{targetFace.Left},{targetFace.Top},{targetFace.Width},{targetFace.Height}";
            var requestUrl =
                $"{this.ServiceHost}/{LargeFaceListsQuery}/{largeFaceListId}/{PersistedFacesQuery}?userData={userData}&targetFace={targetFaceString}";

            return await this.SendRequestAsync<object, AddPersistedFaceResult>(
                       HttpMethod.Post,
                       requestUrl,
                       new { url = imageUrl });
        }

        /// <summary>
        /// Adds the face to large face list asynchronously.
        /// </summary>
        /// <param name="largeFaceListId">The large face list identifier.</param>
        /// <param name="imageStream">The face image stream.</param>
        /// <param name="userData">The user data.</param>
        /// <param name="targetFace">The target face.</param>
        /// <returns>
        /// Add face result.
        /// </returns>
        public async Task<AddPersistedFaceResult> AddFaceToLargeFaceListAsync(
            string largeFaceListId,
            Stream imageStream,
            string userData = null,
            FaceRectangle targetFace = null)
        {
            var targetFaceString = targetFace == null
                                       ? string.Empty
                                       : $"{targetFace.Left},{targetFace.Top},{targetFace.Width},{targetFace.Height}";
            var requestUrl =
                $"{this.ServiceHost}/{LargeFaceListsQuery}/{largeFaceListId}/{PersistedFacesQuery}?userData={userData}&targetFace={targetFaceString}";

            return await this.SendRequestAsync<object, AddPersistedFaceResult>(
                       HttpMethod.Post,
                       requestUrl,
                       imageStream);
        }

        /// <summary>
        /// Deletes the face from large face list asynchronously.
        /// </summary>
        /// <param name="largeFaceListId">The large face list identifier.</param>
        /// <param name="persistedFaceId">The persisted face identifier.</param>
        /// <returns>public async Task object.</returns>
        public async Task DeleteFaceFromLargeFaceListAsync(string largeFaceListId, Guid persistedFaceId)
        {
            var requestUrl =
                $"{this.ServiceHost}/{LargeFaceListsQuery}/{largeFaceListId}/{PersistedFacesQuery}/{persistedFaceId}";

            await this.SendRequestAsync<object, object>(HttpMethod.Delete, requestUrl, null);
        }

        /// <summary>
        /// Gets the face in large face list asynchronously.
        /// </summary>
        /// <param name="largeFaceListId">The large face list identifier.</param>
        /// <param name="persistedFaceId">The persisted face identifier.</param>
        /// <returns>Persisted Face object.</returns>
        public async Task<PersistedFace> GetFaceInLargeFaceListAsync(string largeFaceListId, Guid persistedFaceId)
        {
            var requestUrl =
                $"{this.ServiceHost}/{LargeFaceListsQuery}/{largeFaceListId}/{PersistedFacesQuery}/{persistedFaceId}";

            return await this.SendRequestAsync<object, PersistedFace>(HttpMethod.Get, requestUrl, null);
        }

        /// <summary>
        /// Lists the faces in large face lists asynchronously.
        /// </summary>
        /// <param name="largeFaceListId">The large face list identifier.</param>
        /// <param name="start">The start point string to list faces in large face lists.</param>
        /// <param name="top">The number of faces to list.</param>
        /// <returns>PersistedFace array.</returns>
        public async Task<PersistedFace[]> ListFacesInLargeFaceListAsync(
            string largeFaceListId,
            string start = "",
            int top = 1000)
        {
            var requestUrl =
                $"{this.ServiceHost}/{LargeFaceListsQuery}/{largeFaceListId}/{PersistedFacesQuery}?start={start}&top={top.ToString(CultureInfo.InvariantCulture)}";

            return await this.SendRequestAsync<object, PersistedFace[]>(HttpMethod.Get, requestUrl, null);
        }

        /// <summary>
        /// Updates the face in large face list asynchronously.
        /// </summary>
        /// <param name="largeFaceListId">The large face list identifier.</param>
        /// <param name="persistedFaceId">The persisted face identifier.</param>
        /// <param name="userData">The user data.</param>
        /// <returns>public async Task object.</returns>
        public async Task UpdateFaceInLargeFaceListAsync(string largeFaceListId, Guid persistedFaceId, string userData)
        {
            var requestUrl =
                $"{this.ServiceHost}/{LargeFaceListsQuery}/{largeFaceListId}/{PersistedFacesQuery}/{persistedFaceId}";

            await this.SendRequestAsync<object, object>(
                new HttpMethod("PATCH"),
                requestUrl,
                new { userData = userData });
        }

        #endregion

        #region LargePersonGroup Methods

        /// <summary>
        /// Creates the large person group asynchronously.
        /// </summary>
        /// <param name="largePersonGroupId">The large person group identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="userData">The user data.</param>
        /// <returns>public async Task object.</returns>
        public async Task CreateLargePersonGroupAsync(string largePersonGroupId, string name, string userData = null)
        {
            var requestUrl = $"{this.ServiceHost}/{LargePersonGroupsQuery}/{largePersonGroupId}";

            await this.SendRequestAsync<object, object>(
                HttpMethod.Put,
                requestUrl,
                new { name = name, userData = userData });
        }

        /// <summary>
        /// Deletes a large person group asynchronously.
        /// </summary>
        /// <param name="largePersonGroupId">The large person group id.</param>
        /// <returns>public async Task object.</returns>
        public async Task DeleteLargePersonGroupAsync(string largePersonGroupId)
        {
            var requestUrl = $"{this.ServiceHost}/{LargePersonGroupsQuery}/{largePersonGroupId}";

            await this.SendRequestAsync<object, object>(HttpMethod.Delete, requestUrl, null);
        }

        /// <summary>
        /// Gets a large person group asynchronously.
        /// </summary>
        /// <param name="largePersonGroupId">The large person group id.</param>
        /// <returns>The large person group entity.</returns>
        public async Task<LargePersonGroup> GetLargePersonGroupAsync(string largePersonGroupId)
        {
            var requestUrl = $"{this.ServiceHost}/{LargePersonGroupsQuery}/{largePersonGroupId}";

            return await this.SendRequestAsync<object, LargePersonGroup>(HttpMethod.Get, requestUrl, null);
        }

        /// <summary>
        /// Gets large person group training status asynchronously.
        /// </summary>
        /// <param name="largePersonGroupId">The large person group id.</param>
        /// <returns>The large person group training status.</returns>
        public async Task<TrainingStatus> GetLargePersonGroupTrainingStatusAsync(string largePersonGroupId)
        {
            var requestUrl = $"{this.ServiceHost}/{LargePersonGroupsQuery}/{largePersonGroupId}/{TrainingQuery}";

            return await this.SendRequestAsync<object, TrainingStatus>(HttpMethod.Get, requestUrl, null);
        }

        /// <summary>
        /// Asynchronously list the top large person groups whose Id is larger than "start".
        /// </summary>
        /// <param name="start">the start point string in listing large person groups</param>
        /// <param name="top">the number of large person groups to list</param>
        /// <returns>The large person group entity array.</returns>
        public async Task<LargePersonGroup[]> ListLargePersonGroupsAsync(string start = "", int top = 1000)
        {
            var requestUrl =
                $"{this.ServiceHost}/{LargePersonGroupsQuery}?start={start}&top={top.ToString(CultureInfo.InvariantCulture)}";

            return await this.SendRequestAsync<object, LargePersonGroup[]>(HttpMethod.Get, requestUrl, null);
        }

        /// <summary>
        /// Trains the large person group asynchronously.
        /// </summary>
        /// <param name="largePersonGroupId">The large person group id.</param>
        /// <returns>public async Task object.</returns>
        public async Task TrainLargePersonGroupAsync(string largePersonGroupId)
        {
            var requestUrl = $"{this.ServiceHost}/{LargePersonGroupsQuery}/{largePersonGroupId}/{TrainQuery}";

            await this.SendRequestAsync<object, object>(HttpMethod.Post, requestUrl, null);
        }

        /// <summary>
        /// Updates a large person group asynchronously.
        /// </summary>
        /// <param name="largePersonGroupId">The large person group id.</param>
        /// <param name="name">The name.</param>
        /// <param name="userData">The user data.</param>
        /// <returns>public async Task object.</returns>
        public async Task UpdateLargePersonGroupAsync(string largePersonGroupId, string name, string userData = null)
        {
            var requestUrl = $"{this.ServiceHost}/{LargePersonGroupsQuery}/{largePersonGroupId}";

            await this.SendRequestAsync<object, object>(
                new HttpMethod("PATCH"),
                requestUrl,
                new { name = name, userData = userData });
        }

        #endregion

        #region LargePersonGroup Person Methods

        /// <summary>
        /// Creates a person in large person group asynchronously.
        /// </summary>
        /// <param name="largePersonGroupId">The large person group id.</param>
        /// <param name="name">The name.</param>
        /// <param name="userData">The user data.</param>
        /// <returns>The CreatePersonResult entity.</returns>
        public async Task<CreatePersonResult> CreatePersonInLargePersonGroupAsync(
            string largePersonGroupId,
            string name,
            string userData = null)
        {
            var requestUrl = $"{this.ServiceHost}/{LargePersonGroupsQuery}/{largePersonGroupId}/{PersonsQuery}";

            return await this.SendRequestAsync<object, CreatePersonResult>(
                       HttpMethod.Post,
                       requestUrl,
                       new { name = name, userData = userData });
        }

        /// <summary>
        /// Deletes a person from large person group asynchronously.
        /// </summary>
        /// <param name="largePersonGroupId">The large person group id.</param>
        /// <param name="personId">The person id.</param>
        /// <returns>public async Task object.</returns>
        public async Task DeletePersonFromLargePersonGroupAsync(string largePersonGroupId, Guid personId)
        {
            var requestUrl = $"{this.ServiceHost}/{LargePersonGroupsQuery}/{largePersonGroupId}/{PersonsQuery}/{personId}";

            await this.SendRequestAsync<object, object>(HttpMethod.Delete, requestUrl, null);
        }

        /// <summary>
        /// Gets a person in large person group asynchronously.
        /// </summary>
        /// <param name="largePersonGroupId">The large person group id.</param>
        /// <param name="personId">The person id.</param>
        /// <returns>The person entity.</returns>
        public async Task<Person> GetPersonInLargePersonGroupAsync(string largePersonGroupId, Guid personId)
        {
            var requestUrl = $"{this.ServiceHost}/{LargePersonGroupsQuery}/{largePersonGroupId}/{PersonsQuery}/{personId}";

            return await this.SendRequestAsync<object, Person>(HttpMethod.Get, requestUrl, null);
        }

        /// <summary>
        /// Asynchronously list the top persons in large person group whose Id is larger than "start".
        /// </summary>
        /// <param name="largePersonGroupId">The large person group id.</param>
        /// <param name="start">The start point string in listing persons</param>
        /// <param name="top">The number of persons to list</param>
        /// <returns>Person entity array.</returns>
        public async Task<Person[]> ListPersonsInLargePersonGroupAsync(
            string largePersonGroupId,
            string start = "",
            int top = 1000)
        {
            var requestUrl =
                $"{this.ServiceHost}/{LargePersonGroupsQuery}/{largePersonGroupId}/{PersonsQuery}?start={start}&top={top.ToString(CultureInfo.InvariantCulture)}";

            return await this.SendRequestAsync<object, Person[]>(HttpMethod.Get, requestUrl, null);
        }

        /// <summary>
        /// Updates a person in large person group asynchronously.
        /// </summary>
        /// <param name="largePersonGroupId">The large person group id.</param>
        /// <param name="personId">The person id.</param>
        /// <param name="name">The name.</param>
        /// <param name="userData">The user data.</param>
        /// <returns>public async Task object.</returns>
        public async Task UpdatePersonInLargePersonGroupAsync(
            string largePersonGroupId,
            Guid personId,
            string name,
            string userData = null)
        {
            var requestUrl =
                $"{this.ServiceHost}/{LargePersonGroupsQuery}/{largePersonGroupId}/{PersonsQuery}/{personId}";

            await this.SendRequestAsync<object, object>(
                new HttpMethod("PATCH"),
                requestUrl,
                new { name = name, userData = userData });
        }

        #endregion

        #region LargePersonGroup PersonFace Methods

        /// <summary>
        /// Adds a face to a person in large person group asynchronously.
        /// </summary>
        /// <param name="largePersonGroupId">The large person group id.</param>
        /// <param name="personId">The person id.</param>
        /// <param name="imageUrl">The face image URL.</param>
        /// <param name="userData">The user data.</param>
        /// <param name="targetFace">The target face.</param>
        /// <returns>
        /// Add person face result.
        /// </returns>
        public async Task<AddPersistedFaceResult> AddPersonFaceInLargePersonGroupAsync(
            string largePersonGroupId,
            Guid personId,
            string imageUrl,
            string userData = null,
            FaceRectangle targetFace = null)
        {
            var targetFaceString = targetFace == null
                                       ? string.Empty
                                       : $"{targetFace.Left},{targetFace.Top},{targetFace.Width},{targetFace.Height}";
            var requestUrl =
                $"{this.ServiceHost}/{LargePersonGroupsQuery}/{largePersonGroupId}/{PersonsQuery}/{personId}/{PersistedFacesQuery}?userData={userData}&targetFace={targetFaceString}";

            return await this.SendRequestAsync<object, AddPersistedFaceResult>(
                       HttpMethod.Post,
                       requestUrl,
                       new { url = imageUrl });
        }

        /// <summary>
        /// Adds a face to a person in large person group asynchronously.
        /// </summary>
        /// <param name="largePersonGroupId">The large person group id.</param>
        /// <param name="personId">The person id.</param>
        /// <param name="imageStream">The face image stream.</param>
        /// <param name="userData">The user data.</param>
        /// <param name="targetFace">The Target Face.</param>
        /// <returns>
        /// Add person face result.
        /// </returns>
        public async Task<AddPersistedFaceResult> AddPersonFaceInLargePersonGroupAsync(
            string largePersonGroupId,
            Guid personId,
            Stream imageStream,
            string userData = null,
            FaceRectangle targetFace = null)
        {
            var targetFaceString = targetFace == null
                                       ? string.Empty
                                       : $"{targetFace.Left},{targetFace.Top},{targetFace.Width},{targetFace.Height}";
            var requestUrl =
                $"{this.ServiceHost}/{LargePersonGroupsQuery}/{largePersonGroupId}/{PersonsQuery}/{personId}/{PersistedFacesQuery}?userData={userData}&targetFace={targetFaceString}";

            return await this.SendRequestAsync<Stream, AddPersistedFaceResult>(
                       HttpMethod.Post,
                       requestUrl,
                       imageStream);
        }

        /// <summary>
        /// Deletes a face of a person from large person group asynchronously.
        /// </summary>
        /// <param name="largePersonGroupId">The large person group id.</param>
        /// <param name="personId">The person id.</param>
        /// <param name="persistedFaceId">The persisted face id.</param>
        /// <returns>public async Task object.</returns>
        public async Task DeletePersonFaceFromLargePersonGroupAsync(
            string largePersonGroupId,
            Guid personId,
            Guid persistedFaceId)
        {
            var requestUrl =
                $"{this.ServiceHost}/{LargePersonGroupsQuery}/{largePersonGroupId}/{PersonsQuery}/{personId}/{PersistedFacesQuery}/{persistedFaceId}";

            await this.SendRequestAsync<object, object>(HttpMethod.Delete, requestUrl, null);
        }

        /// <summary>
        /// Gets a face of a person in large person group asynchronously.
        /// </summary>
        /// <param name="largePersonGroupId">The large person group id.</param>
        /// <param name="personId">The person id.</param>
        /// <param name="persistedFaceId">The persisted face id.</param>
        /// <returns>The person face entity.</returns>
        public async Task<PersistedFace> GetPersonFaceInLargePersonGroupAsync(
            string largePersonGroupId,
            Guid personId,
            Guid persistedFaceId)
        {
            var requestUrl =
                $"{this.ServiceHost}/{LargePersonGroupsQuery}/{largePersonGroupId}/{PersonsQuery}/{personId}/{PersistedFacesQuery}/{persistedFaceId}";

            return await this.SendRequestAsync<object, PersistedFace>(HttpMethod.Get, requestUrl, null);
        }

        /// <summary>
        /// Updates a face of a person in large person group asynchronously.
        /// </summary>
        /// <param name="largePersonGroupId">The large person group id.</param>
        /// <param name="personId">The person id.</param>
        /// <param name="persistedFaceId">The persisted face id.</param>
        /// <param name="userData">The user data.</param>
        /// <returns>public async Task object.</returns>
        public async Task UpdatePersonFaceInLargePersonGroupAsync(
            string largePersonGroupId,
            Guid personId,
            Guid persistedFaceId,
            string userData = null)
        {
            var requestUrl =
                $"{this.ServiceHost}/{LargePersonGroupsQuery}/{largePersonGroupId}/{PersonsQuery}/{personId}/{PersistedFacesQuery}/{persistedFaceId}";

            await this.SendRequestAsync<object, object>(
                new HttpMethod("PATCH"),
                requestUrl,
                new { userData = userData });
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets face attribute query string from attribute types
        /// </summary>
        /// <param name="types">Face attribute types</param>
        /// <returns>Face attribute query string</returns>
        public static string GetAttributeString(IEnumerable<FaceAttributeType> types)
        {
            return string.Join(",", types.Select(attr =>
                {
                    var attrStr = attr.ToString();
                    return char.ToLowerInvariant(attrStr[0]) + attrStr.Substring(1);
                }).ToArray());
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this._httpClient != null)
                {
                    this._httpClient.Dispose();
                    this._httpClient = null;
                }
            }
        }

        /// <summary>
        /// Sends the request asynchronous.
        /// </summary>
        /// <typeparam name="TRequest">The type of the request.</typeparam>
        /// <typeparam name="TResponse">The type of the response.</typeparam>
        /// <param name="httpMethod">The HTTP method.</param>
        /// <param name="requestUrl">The request URL.</param>
        /// <param name="requestBody">The request body.</param>
        /// <returns>The response.</returns>
        /// <exception cref="OxfordAPIException">The client exception.</exception>
        private async Task<TResponse> SendRequestAsync<TRequest, TResponse>(HttpMethod httpMethod, string requestUrl, TRequest requestBody)
        {
            var request = new HttpRequestMessage(httpMethod, this.ServiceHost);
            request.RequestUri = new Uri(requestUrl);
            if (requestBody != null)
            {
                if (requestBody is Stream)
                {
                    request.Content = new StreamContent(requestBody as Stream);
                    request.Content.Headers.ContentType = new MediaTypeHeaderValue(StreamContentTypeHeader);
                }
                else
                {
                    request.Content = new StringContent(JsonConvert.SerializeObject(requestBody, s_settings), Encoding.UTF8, JsonContentTypeHeader);
                }
            }

            HttpResponseMessage response = await this._httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                string responseContent = null;
                if (response.Content != null)
                {
                    responseContent = await response.Content.ReadAsStringAsync();
                }

                if (!string.IsNullOrWhiteSpace(responseContent))
                {
                    return JsonConvert.DeserializeObject<TResponse>(responseContent, s_settings);
                }

                return default(TResponse);
            }
            else
            {
                if (response.Content != null && response.Content.Headers.ContentType.MediaType.Contains(JsonContentTypeHeader))
                {
                    var errorObjectString = await response.Content.ReadAsStringAsync();
                    ClientError ex = JsonConvert.DeserializeObject<ClientError>(errorObjectString);
                    if (ex.Error != null)
                    {
                        throw new FaceAPIException(ex.Error.ErrorCode, ex.Error.Message, response.StatusCode);
                    }
                    else
                    {
                        ServiceError serviceEx = JsonConvert.DeserializeObject<ServiceError>(errorObjectString);
                        if (ex != null)
                        {
                            throw new FaceAPIException(serviceEx.ErrorCode, serviceEx.Message, response.StatusCode);
                        }
                        else
                        {
                            throw new FaceAPIException("Unknown", "Unknown Error", response.StatusCode);
                        }
                    }
                }

                response.EnsureSuccessStatusCode();
            }

            return default(TResponse);
        }

        #endregion Methods
    }
}