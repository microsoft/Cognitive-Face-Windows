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
using System.IO;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.ProjectOxford.Face.Contract;

namespace Microsoft.ProjectOxford.Face
{
    using Face = Microsoft.ProjectOxford.Face.Contract.Face;

    #region Enumerations

    /// <summary>
    /// Supported face attribute types
    /// </summary>
    public enum FaceAttributeType
    {
        /// <summary>
        /// Analyses age
        /// </summary>
        Age,

        /// <summary>
        /// Analyses gender
        /// </summary>
        Gender,

        /// <summary>
        /// Analyses head pose
        /// </summary>
        HeadPose,

        /// <summary>
        /// Analyses whether is smiling
        /// </summary>
        Smile,

        /// <summary>
        /// Analyses facial hair
        /// </summary>
        FacialHair,

        /// <summary>
        /// Analyses glasses type
        /// </summary>
        Glasses,

        /// <summary>
        /// Analyses emotion
        /// </summary>
        Emotion,

        /// <summary>
        /// Analyses hair
        /// </summary>
        Hair,

        /// <summary>
        /// Analyses makeup
        /// </summary>
        Makeup,

        /// <summary>
        /// Analyses occlusion
        /// </summary>
        Occlusion,

        /// <summary>
        /// Analyses accessory
        /// </summary>
        Accessories,

        /// <summary>
        /// Analyses blur
        /// </summary>
        Blur,

        /// <summary>
        /// Analyses exposure
        /// </summary>
        Exposure,

        /// <summary>
        /// Analyses noise
        /// </summary>
        Noise
    }

    /// <summary>
    /// two working modes of Face - Find Similar
    /// </summary>
    public enum FindSimilarMatchMode
    {
        /// <summary>
        /// matchPerson mode of Face - Find Similar, return the similar faces of the same person with the query face.
        /// </summary>
        matchPerson,

        /// <summary>
        /// matchFace mode of Face - Find Similar, return the similar faces of the query face, ignoring if they belong to the same person.
        /// </summary>
        matchFace
    }

    #endregion Enumerations

    /// <summary>
    /// The face service client proxy interface.
    /// </summary>
    public interface IFaceServiceClient
    {
        #region Properties

        /// <summary>
        /// Gets default request headers for all following http request
        /// </summary>
        HttpRequestHeaders DefaultRequestHeaders
        {
            get;
        }

        #endregion Properties

        #region Face Methods

        /// <summary>
        /// Detects an URL asynchronously.
        /// </summary>
        /// <param name="imageUrl">The image URL.</param>
        /// <param name="returnFaceId">If set to <c>true</c> [return face ID].</param>
        /// <param name="returnFaceLandmarks">If set to <c>true</c> [return face landmarks].</param>
        /// <param name="returnFaceAttributes">Return face attributes.</param>
        /// <returns>The detected faces.</returns>
        Task<Face[]> DetectAsync(string imageUrl, bool returnFaceId = true, bool returnFaceLandmarks = false, IEnumerable<FaceAttributeType> returnFaceAttributes = null);

        /// <summary>
        /// Detects an image asynchronously.
        /// </summary>
        /// <param name="imageStream">The image stream.</param>
        /// <param name="returnFaceId">If set to <c>true</c> [return face ID].</param>
        /// <param name="returnFaceLandmarks">If set to <c>true</c> [return face landmarks].</param>
        /// <param name="returnFaceAttributes">Return face attributes.</param>
        /// <returns>The detected faces.</returns>
        Task<Face[]> DetectAsync(Stream imageStream, bool returnFaceId = true, bool returnFaceLandmarks = false, IEnumerable<FaceAttributeType> returnFaceAttributes = null);

        /// <summary>
        /// Finds the similar faces asynchronously.
        /// </summary>
        /// <param name="faceId">The face identifier.</param>
        /// <param name="faceIds">The face identifiers.</param>
        /// <param name="maxNumOfCandidatesReturned">The max number of candidates returned.</param>
        /// <returns>
        /// The similar faces.
        /// </returns>
        Task<SimilarFace[]> FindSimilarAsync(Guid faceId, Guid[] faceIds, int maxNumOfCandidatesReturned = 20);

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
        Task<SimilarFace[]> FindSimilarAsync(Guid faceId, Guid[] faceIds, FindSimilarMatchMode mode, int maxNumOfCandidatesReturned = 20);

        /// <summary>
        /// Finds the similar faces asynchronously.
        /// </summary>
        /// <param name="faceId">The face identifier.</param>
        /// <param name="faceListId">The face list identifier.</param>
        /// <param name="maxNumOfCandidatesReturned">The max number of candidates returned.</param>
        /// <returns>
        /// The similar persisted faces.
        /// </returns>
        [Obsolete("use the FindSimilarAsync support both Face List and Large Face List instead")]
        Task<SimilarPersistedFace[]> FindSimilarAsync(Guid faceId, string faceListId, int maxNumOfCandidatesReturned = 20);

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
        [Obsolete("use the FindSimilarAsync support both Face List and Large Face List instead")]
        Task<SimilarPersistedFace[]> FindSimilarAsync(Guid faceId, string faceListId, FindSimilarMatchMode mode, int maxNumOfCandidatesReturned = 20);

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
        Task<SimilarPersistedFace[]> FindSimilarAsync(Guid faceId, string faceListId = null, string largeFaceListId = null, FindSimilarMatchMode mode = FindSimilarMatchMode.matchPerson, int maxNumOfCandidatesReturned = 20);

        /// <summary>
        /// Groups the face asynchronously.
        /// </summary>
        /// <param name="faceIds">The face ids.</param>
        /// <returns>Task object.</returns>
        Task<GroupResult> GroupAsync(Guid[] faceIds);

        /// <summary>
        /// Identities the faces in a given person group asynchronously.
        /// </summary>
        /// <param name="personGroupId">The person group id.</param>
        /// <param name="faceIds">The face ids.</param>
        /// <param name="maxNumOfCandidatesReturned">The maximum number of candidates returned for each face.</param>
        /// <returns>The identification results</returns>
        [Obsolete("use the IdentifyAsync support both Person Group and Large Person Group instead")]
        Task<IdentifyResult[]> IdentifyAsync(string personGroupId, Guid[] faceIds, int maxNumOfCandidatesReturned = 1);

        /// <summary>
        /// Identities the faces in a given person group asynchronously.
        /// </summary>
        /// <param name="personGroupId">The person group id.</param>
        /// <param name="faceIds">The face ids.</param>
        /// <param name="confidenceThreshold">user-specified confidence threshold.</param>
        /// <param name="maxNumOfCandidatesReturned">The maximum number of candidates returned for each face.</param>
        /// <returns>The identification results</returns>
        [Obsolete("use the IdentifyAsync support both Person Group and Large Person Group instead")]
        Task<IdentifyResult[]> IdentifyAsync(string personGroupId, Guid[] faceIds, float confidenceThreshold, int maxNumOfCandidatesReturned = 1);

        /// <summary>
        /// Identities the faces in a given person group or large person group asynchronously.
        /// </summary>
        /// <param name="faceIds">The face ids.</param>
        /// <param name="personGroupId">The person group id.</param>
        /// <param name="largePersonGroupId">The large person group id.</param>
        /// <param name="confidenceThreshold">user-specified confidence threshold.</param>
        /// <param name="maxNumOfCandidatesReturned">The maximum number of candidates returned for each face.</param>
        /// <returns>The identification results</returns>
        Task<IdentifyResult[]> IdentifyAsync(Guid[] faceIds, string personGroupId = null, string largePersonGroupId = null, float confidenceThreshold = 0.5f, int maxNumOfCandidatesReturned = 1);

        /// <summary>
        /// Verifies whether the specified two faces belong to the same person asynchronously.
        /// </summary>
        /// <param name="faceId1">The face id 1.</param>
        /// <param name="faceId2">The face id 2.</param>
        /// <returns>The verification result.</returns>
        Task<VerifyResult> VerifyAsync(Guid faceId1, Guid faceId2);

        /// <summary>
        /// Verify whether one face belong to a person.
        /// </summary>
        /// <param name="faceId">The face id.</param>
        /// <param name="personGroupId">The person group id.</param>
        /// <param name="personId">The person id.</param>
        /// <returns>Verification result.</returns>
        [Obsolete("use the VerifyAsync supports both Person Group and Large Person Group instead")]
        Task<VerifyResult> VerifyAsync(Guid faceId, string personGroupId, Guid personId);

        /// <summary>
        /// Verify whether one face belong to a person.
        /// </summary>
        /// <param name="faceId">The face id.</param>
        /// <param name="personId">The person id.</param>
        /// <param name="personGroupId">The person group id.</param>
        /// <param name="largePersonGroupId">The large person group id.</param>
        /// <returns>Verification result.</returns>
        Task<VerifyResult> VerifyAsync(Guid faceId, Guid personId, string personGroupId = null, string largePersonGroupId = null);

        #endregion

        #region FaceList Methods

        /// <summary>
        /// Creates the face list asynchronously.
        /// </summary>
        /// <param name="faceListId">The face list identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="userData">The user data.</param>
        /// <returns>Task object.</returns>
        Task CreateFaceListAsync(string faceListId, string name, string userData = null);

        /// <summary>
        /// Deletes the face list asynchronously.
        /// </summary>
        /// <param name="faceListId">The face list identifier.</param>
        /// <returns>Task object.</returns>
        Task DeleteFaceListAsync(string faceListId);

        /// <summary>
        /// Gets the face list asynchronously.
        /// </summary>
        /// <param name="faceListId">The face list identifier.</param>
        /// <returns>Face list object.</returns>
        Task<FaceList> GetFaceListAsync(string faceListId);

        /// <summary>
        /// Lists the face lists asynchronously.
        /// </summary>
        /// <returns>FaceListMetadata array.</returns>
        Task<FaceListMetadata[]> ListFaceListsAsync();

        /// <summary>
        /// Updates the face list asynchronously.
        /// </summary>
        /// <param name="faceListId">The face list identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="userData">The user data.</param>
        /// <returns>Task object.</returns>
        Task UpdateFaceListAsync(string faceListId, string name, string userData);

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
        Task<AddPersistedFaceResult> AddFaceToFaceListAsync(string faceListId, string imageUrl, string userData = null, FaceRectangle targetFace = null);

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
        Task<AddPersistedFaceResult> AddFaceToFaceListAsync(string faceListId, Stream imageStream, string userData = null, FaceRectangle targetFace = null);

        /// <summary>
        /// Deletes the face from face list asynchronously.
        /// </summary>
        /// <param name="faceListId">The face list identifier.</param>
        /// <param name="persistedFaceId">The persisted face identifier.</param>
        /// <returns>Task object.</returns>
        Task DeleteFaceFromFaceListAsync(string faceListId, Guid persistedFaceId);

        #endregion

        #region PersonGroup Methods

        /// <summary>
        /// Creates the person group asynchronously.
        /// </summary>
        /// <param name="personGroupId">The person group identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="userData">The user data.</param>
        /// <returns>Task object.</returns>
        Task CreatePersonGroupAsync(string personGroupId, string name, string userData = null);

        /// <summary>
        /// Deletes a person group asynchronously.
        /// </summary>
        /// <param name="personGroupId">The person group id.</param>
        /// <returns>Task object.</returns>
        Task DeletePersonGroupAsync(string personGroupId);

        /// <summary>
        /// Gets a person group asynchronously.
        /// </summary>
        /// <param name="personGroupId">The person group id.</param>
        /// <returns>The person group entity.</returns>
        Task<PersonGroup> GetPersonGroupAsync(string personGroupId);

        /// <summary>
        /// Gets person group training status asynchronously.
        /// </summary>
        /// <param name="personGroupId">The person group id.</param>
        /// <returns>The person group training status.</returns>
        Task<TrainingStatus> GetPersonGroupTrainingStatusAsync(string personGroupId);

        /// <summary>
        /// Asynchronously list the person groups.
        /// </summary>
        /// <returns>Person group entity array.</returns>
        [Obsolete("use ListPersonGroupsAsync instead")]
        Task<PersonGroup[]> GetPersonGroupsAsync();

        /// <summary>
        /// Asynchronously list the top person groups whose Id is larger than "start".
        /// </summary>
        /// <param name="start">the start point string in listing person groups</param>
        /// <param name="top">the number of person groups to list</param>
        /// <returns>Person group entity array.</returns>
        Task<PersonGroup[]> ListPersonGroupsAsync(string start = "", int top = 1000);

        /// <summary>
        /// Trains the person group asynchronously.
        /// </summary>
        /// <param name="personGroupId">The person group id.</param>
        /// <returns>Task object.</returns>
        Task TrainPersonGroupAsync(string personGroupId);

        /// <summary>
        /// Updates a person group asynchronously.
        /// </summary>
        /// <param name="personGroupId">The person group id.</param>
        /// <param name="name">The name.</param>
        /// <param name="userData">The user data.</param>
        /// <returns>Task object.</returns>
        Task UpdatePersonGroupAsync(string personGroupId, string name, string userData = null);

        #endregion

        #region PersonGroup Person Methods

        /// <summary>
        /// Creates a person asynchronously.
        /// </summary>
        /// <param name="personGroupId">The person group id.</param>
        /// <param name="name">The name.</param>
        /// <param name="userData">The user data.</param>
        /// <returns>The CreatePersonResult entity.</returns>
        [Obsolete("use CreatePersonInPersonGroupAsync instead")]
        Task<CreatePersonResult> CreatePersonAsync(string personGroupId, string name, string userData = null);

        /// <summary>
        /// Creates a person asynchronously.
        /// </summary>
        /// <param name="personGroupId">The person group id.</param>
        /// <param name="name">The name.</param>
        /// <param name="userData">The user data.</param>
        /// <returns>The CreatePersonResult entity.</returns>
        Task<CreatePersonResult> CreatePersonInPersonGroupAsync(string personGroupId, string name, string userData = null);

        /// <summary>
        /// Deletes a person asynchronously.
        /// </summary>
        /// <param name="personGroupId">The person group id.</param>
        /// <param name="personId">The person id.</param>
        /// <returns>Task object.</returns>
        [Obsolete("use DeletePersonFromPersonGroupAsync instead")]
        Task DeletePersonAsync(string personGroupId, Guid personId);

        /// <summary>
        /// Deletes a person asynchronously.
        /// </summary>
        /// <param name="personGroupId">The person group id.</param>
        /// <param name="personId">The person id.</param>
        /// <returns>Task object.</returns>
        Task DeletePersonFromPersonGroupAsync(string personGroupId, Guid personId);

        /// <summary>
        /// Gets a person asynchronously.
        /// </summary>
        /// <param name="personGroupId">The person group id.</param>
        /// <param name="personId">The person id.</param>
        /// <returns>The person entity.</returns>
        [Obsolete("use GetPersonInPersonGroupAsync instead")]
        Task<Person> GetPersonAsync(string personGroupId, Guid personId);

        /// <summary>
        /// Gets a person asynchronously.
        /// </summary>
        /// <param name="personGroupId">The person group id.</param>
        /// <param name="personId">The person id.</param>
        /// <returns>The person entity.</returns>
        Task<Person> GetPersonInPersonGroupAsync(string personGroupId, Guid personId);

        /// <summary>
        /// Gets persons inside a person group asynchronously.
        /// </summary>
        /// <param name="personGroupId">The person group id.</param>
        /// <returns>
        /// The person entity array.
        /// </returns>
        [Obsolete("use ListPersonsInPersonGroupAsync instead")]
        Task<Person[]> GetPersonsAsync(string personGroupId);

        /// <summary>
        /// Asynchronously list the top persons whose Id is larger than "start".
        /// </summary>
        /// <param name="personGroupId">The person group id.</param>
        /// <param name="start">The start point string in listing persons</param>
        /// <param name="top">The number of persons to list</param>
        /// <returns>Person entity array.</returns>
        [Obsolete("use ListPersonsInPersonGroupAsync instead")]
        Task<Person[]> ListPersonsAsync(string personGroupId, string start = "", int top = 1000);

        /// <summary>
        /// Asynchronously list the top persons whose Id is larger than "start".
        /// </summary>
        /// <param name="personGroupId">The person group id.</param>
        /// <param name="start">The start point string in listing persons</param>
        /// <param name="top">The number of persons to list</param>
        /// <returns>Person entity array.</returns>
        Task<Person[]> ListPersonsInPersonGroupAsync(string personGroupId, string start = "", int top = 1000);

        /// <summary>
        /// Updates a person asynchronously.
        /// </summary>
        /// <param name="personGroupId">The person group id.</param>
        /// <param name="personId">The person id.</param>
        /// <param name="name">The name.</param>
        /// <param name="userData">The user data.</param>
        /// <returns>Task object.</returns>
        [Obsolete("use UpdatePersonInPersonGroupAsync instead")]
        Task UpdatePersonAsync(string personGroupId, Guid personId, string name, string userData = null);

        /// <summary>
        /// Updates a person asynchronously.
        /// </summary>
        /// <param name="personGroupId">The person group id.</param>
        /// <param name="personId">The person id.</param>
        /// <param name="name">The name.</param>
        /// <param name="userData">The user data.</param>
        /// <returns>Task object.</returns>
        Task UpdatePersonInPersonGroupAsync(string personGroupId, Guid personId, string name, string userData = null);

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
        [Obsolete("use AddPersonFaceInPersonGroupAsync instead")]
        Task<AddPersistedFaceResult> AddPersonFaceAsync(string personGroupId, Guid personId, string imageUrl, string userData = null, FaceRectangle targetFace = null);

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
        Task<AddPersistedFaceResult> AddPersonFaceInPersonGroupAsync(string personGroupId, Guid personId, string imageUrl, string userData = null, FaceRectangle targetFace = null);

        /// <summary>
        /// Adds a face to a person asynchronously.
        /// </summary>
        /// <param name="personGroupId">The person group id.</param>
        /// <param name="personId">The person id.</param>
        /// <param name="imageStream">The face image stream.</param>
        /// <param name="userData">The user data.</param>
        /// <param name="targetFace">The Target Face.</param>
        /// <returns>
        /// Add person face result.
        /// </returns>
        [Obsolete("use AddPersonFaceInPersonGroupAsync instead")]
        Task<AddPersistedFaceResult> AddPersonFaceAsync(string personGroupId, Guid personId, Stream imageStream, string userData = null, FaceRectangle targetFace = null);

        /// <summary>
        /// Adds a face to a person asynchronously.
        /// </summary>
        /// <param name="personGroupId">The person group id.</param>
        /// <param name="personId">The person id.</param>
        /// <param name="imageStream">The face image stream.</param>
        /// <param name="userData">The user data.</param>
        /// <param name="targetFace">The Target Face.</param>
        /// <returns>
        /// Add person face result.
        /// </returns>
        Task<AddPersistedFaceResult> AddPersonFaceInPersonGroupAsync(string personGroupId, Guid personId, Stream imageStream, string userData = null, FaceRectangle targetFace = null);

        /// <summary>
        /// Deletes a face of a person asynchronously.
        /// </summary>
        /// <param name="personGroupId">The person group id.</param>
        /// <param name="personId">The person id.</param>
        /// <param name="persistedFaceId">The persisted face id.</param>
        /// <returns>Task object.</returns>
        [Obsolete("use DeletePersonFaceFromPersonGroupAsync instead")]
        Task DeletePersonFaceAsync(string personGroupId, Guid personId, Guid persistedFaceId);

        /// <summary>
        /// Deletes a face of a person asynchronously.
        /// </summary>
        /// <param name="personGroupId">The person group id.</param>
        /// <param name="personId">The person id.</param>
        /// <param name="persistedFaceId">The persisted face id.</param>
        /// <returns>Task object.</returns>
        Task DeletePersonFaceFromPersonGroupAsync(string personGroupId, Guid personId, Guid persistedFaceId);

        /// <summary>
        /// Gets a face of a person asynchronously.
        /// </summary>
        /// <param name="personGroupId">The person group id.</param>
        /// <param name="personId">The person id.</param>
        /// <param name="persistedFaceId">The persisted face id.</param>
        /// <returns>The person face entity.</returns>
        [Obsolete("use GetPersonFaceInPersonGroupAsync instead")]
        Task<PersistedFace> GetPersonFaceAsync(string personGroupId, Guid personId, Guid persistedFaceId);

        /// <summary>
        /// Gets a face of a person asynchronously.
        /// </summary>
        /// <param name="personGroupId">The person group id.</param>
        /// <param name="personId">The person id.</param>
        /// <param name="persistedFaceId">The persisted face id.</param>
        /// <returns>The person face entity.</returns>
        Task<PersistedFace> GetPersonFaceInPersonGroupAsync(string personGroupId, Guid personId, Guid persistedFaceId);

        /// <summary>
        /// Updates a face of a person asynchronously.
        /// </summary>
        /// <param name="personGroupId">The person group id.</param>
        /// <param name="personId">The person id.</param>
        /// <param name="persistedFaceId">The persisted face id.</param>
        /// <param name="userData">The user data.</param>
        /// <returns>Task object.</returns>
        [Obsolete("use UpdatePersonFaceInPersonGroupAsync instead")]
        Task UpdatePersonFaceAsync(string personGroupId, Guid personId, Guid persistedFaceId, string userData = null);

        /// <summary>
        /// Updates a face of a person asynchronously.
        /// </summary>
        /// <param name="personGroupId">The person group id.</param>
        /// <param name="personId">The person id.</param>
        /// <param name="persistedFaceId">The persisted face id.</param>
        /// <param name="userData">The user data.</param>
        /// <returns>Task object.</returns>
        Task UpdatePersonFaceInPersonGroupAsync(string personGroupId, Guid personId, Guid persistedFaceId, string userData = null);

        #endregion

        #region LargeFaceList Methods

        /// <summary>
        /// Creates the large face list asynchronously.
        /// </summary>
        /// <param name="largeFaceListId">The large face list identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="userData">The user data.</param>
        /// <returns>Task object.</returns>
        Task CreateLargeFaceListAsync(string largeFaceListId, string name, string userData = null);

        /// <summary>
        /// Deletes the large face list asynchronously.
        /// </summary>
        /// <param name="largeFaceListId">The large face list identifier.</param>
        /// <returns>Task object.</returns>
        Task DeleteLargeFaceListAsync(string largeFaceListId);

        /// <summary>
        /// Gets the large face list asynchronously.
        /// </summary>
        /// <param name="largeFaceListId">The large face list identifier.</param>
        /// <returns>Face list object.</returns>
        Task<LargeFaceList> GetLargeFaceListAsync(string largeFaceListId);

        /// <summary>
        /// Gets large face list training status asynchronously.
        /// </summary>
        /// <param name="largeFaceListId">The large face list id.</param>
        /// <returns>The large face list training status.</returns>
        Task<TrainingStatus> GetLargeFaceListTrainingStatusAsync(string largeFaceListId);

        /// <summary>
        /// Lists the large face lists asynchronously.
        /// </summary>
        /// <param name="start">The start point string in listing large face lists</param>
        /// <param name="top">The number of large face lists to list</param>
        /// <returns>LargeFaceListMetadata array.</returns>
        Task<LargeFaceList[]> ListLargeFaceListsAsync(string start = "", int top = 1000);

        /// <summary>
        /// Trains the large face list asynchronously.
        /// </summary>
        /// <param name="largeFaceListId">The large face list id.</param>
        /// <returns>Task object.</returns>
        Task TrainLargeFaceListAsync(string largeFaceListId);

        /// <summary>
        /// Updates the large face list asynchronously.
        /// </summary>
        /// <param name="largeFaceListId">The large face list identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="userData">The user data.</param>
        /// <returns>Task object.</returns>
        Task UpdateLargeFaceListAsync(string largeFaceListId, string name, string userData);

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
        Task<AddPersistedFaceResult> AddFaceToLargeFaceListAsync(string largeFaceListId, string imageUrl, string userData = null, FaceRectangle targetFace = null);

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
        Task<AddPersistedFaceResult> AddFaceToLargeFaceListAsync(string largeFaceListId, Stream imageStream, string userData = null, FaceRectangle targetFace = null);

        /// <summary>
        /// Deletes the face from large face list asynchronously.
        /// </summary>
        /// <param name="largeFaceListId">The large face list identifier.</param>
        /// <param name="persistedFaceId">The persisted face identifier.</param>
        /// <returns>Task object.</returns>
        Task DeleteFaceFromLargeFaceListAsync(string largeFaceListId, Guid persistedFaceId);

        /// <summary>
        /// Gets the face in large face list asynchronously.
        /// </summary>
        /// <param name="largeFaceListId">The large face list identifier.</param>
        /// <param name="persistedFaceId">The persisted face identifier.</param>
        /// <returns>Persisted Face object.</returns>
        Task<PersistedFace> GetFaceInLargeFaceListAsync(string largeFaceListId, Guid persistedFaceId);

        /// <summary>
        /// Lists the faces in large face lists asynchronously.
        /// </summary>
        /// <param name="largeFaceListId">The large face list identifier.</param>
        /// <param name="start">The start point string to list faces in large face lists.</param>
        /// <param name="top">The number of faces to list.</param>
        /// <returns>PersistedFace array.</returns>
        Task<PersistedFace[]> ListFacesInLargeFaceListAsync(string largeFaceListId, string start = "", int top = 1000);

        /// <summary>
        /// Updates the face in large face list asynchronously.
        /// </summary>
        /// <param name="largeFaceListId">The large face list identifier.</param>
        /// <param name="persistedFaceId">The persisted face identifier.</param>
        /// <param name="userData">The user data.</param>
        /// <returns>Task object.</returns>
        Task UpdateFaceInLargeFaceListAsync(string largeFaceListId, Guid persistedFaceId, string userData);

        #endregion

        #region LargePersonGroup Methods

        /// <summary>
        /// Creates the large person group asynchronously.
        /// </summary>
        /// <param name="largePersonGroupId">The large person group identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="userData">The user data.</param>
        /// <returns>Task object.</returns>
        Task CreateLargePersonGroupAsync(string largePersonGroupId, string name, string userData = null);

        /// <summary>
        /// Deletes a large person group asynchronously.
        /// </summary>
        /// <param name="largePersonGroupId">The large person group id.</param>
        /// <returns>Task object.</returns>
        Task DeleteLargePersonGroupAsync(string largePersonGroupId);

        /// <summary>
        /// Gets a large person group asynchronously.
        /// </summary>
        /// <param name="largePersonGroupId">The large person group id.</param>
        /// <returns>The large person group entity.</returns>
        Task<LargePersonGroup> GetLargePersonGroupAsync(string largePersonGroupId);

        /// <summary>
        /// Gets large person group training status asynchronously.
        /// </summary>
        /// <param name="largePersonGroupId">The large person group id.</param>
        /// <returns>The large person group training status.</returns>
        Task<TrainingStatus> GetLargePersonGroupTrainingStatusAsync(string largePersonGroupId);

        /// <summary>
        /// Asynchronously list the top large person groups whose Id is larger than "start".
        /// </summary>
        /// <param name="start">the start point string in listing large person groups</param>
        /// <param name="top">the number of large person groups to list</param>
        /// <returns>The large person group entity array.</returns>
        Task<LargePersonGroup[]> ListLargePersonGroupsAsync(string start = "", int top = 1000);

        /// <summary>
        /// Trains the large person group asynchronously.
        /// </summary>
        /// <param name="largePersonGroupId">The large person group id.</param>
        /// <returns>Task object.</returns>
        Task TrainLargePersonGroupAsync(string largePersonGroupId);

        /// <summary>
        /// Updates a large person group asynchronously.
        /// </summary>
        /// <param name="largePersonGroupId">The large person group id.</param>
        /// <param name="name">The name.</param>
        /// <param name="userData">The user data.</param>
        /// <returns>Task object.</returns>
        Task UpdateLargePersonGroupAsync(string largePersonGroupId, string name, string userData = null);

        #endregion

        #region LargePersonGroup Person Methods

        /// <summary>
        /// Creates a person in large person group asynchronously.
        /// </summary>
        /// <param name="largePersonGroupId">The large person group id.</param>
        /// <param name="name">The name.</param>
        /// <param name="userData">The user data.</param>
        /// <returns>The CreatePersonResult entity.</returns>
        Task<CreatePersonResult> CreatePersonInLargePersonGroupAsync(string largePersonGroupId, string name, string userData = null);

        /// <summary>
        /// Deletes a person from large person group asynchronously.
        /// </summary>
        /// <param name="largePersonGroupId">The large person group id.</param>
        /// <param name="personId">The person id.</param>
        /// <returns>Task object.</returns>
        Task DeletePersonFromLargePersonGroupAsync(string largePersonGroupId, Guid personId);

        /// <summary>
        /// Gets a person in large person group asynchronously.
        /// </summary>
        /// <param name="largePersonGroupId">The large person group id.</param>
        /// <param name="personId">The person id.</param>
        /// <returns>The person entity.</returns>
        Task<Person> GetPersonInLargePersonGroupAsync(string largePersonGroupId, Guid personId);

        /// <summary>
        /// Asynchronously list the top persons in large person group whose Id is larger than "start".
        /// </summary>
        /// <param name="largePersonGroupId">The large person group id.</param>
        /// <param name="start">The start point string in listing persons</param>
        /// <param name="top">The number of persons to list</param>
        /// <returns>Person entity array.</returns>
        Task<Person[]> ListPersonsInLargePersonGroupAsync(string largePersonGroupId, string start = "", int top = 1000);

        /// <summary>
        /// Updates a person in large person group asynchronously.
        /// </summary>
        /// <param name="largePersonGroupId">The large person group id.</param>
        /// <param name="personId">The person id.</param>
        /// <param name="name">The name.</param>
        /// <param name="userData">The user data.</param>
        /// <returns>Task object.</returns>
        Task UpdatePersonInLargePersonGroupAsync(string largePersonGroupId, Guid personId, string name, string userData = null);

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
        Task<AddPersistedFaceResult> AddPersonFaceInLargePersonGroupAsync(string largePersonGroupId, Guid personId, string imageUrl, string userData = null, FaceRectangle targetFace = null);

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
        Task<AddPersistedFaceResult> AddPersonFaceInLargePersonGroupAsync(string largePersonGroupId, Guid personId, Stream imageStream, string userData = null, FaceRectangle targetFace = null);

        /// <summary>
        /// Deletes a face of a person from large person group asynchronously.
        /// </summary>
        /// <param name="largePersonGroupId">The large person group id.</param>
        /// <param name="personId">The person id.</param>
        /// <param name="persistedFaceId">The persisted face id.</param>
        /// <returns>Task object.</returns>
        Task DeletePersonFaceFromLargePersonGroupAsync(string largePersonGroupId, Guid personId, Guid persistedFaceId);

        /// <summary>
        /// Gets a face of a person in large person group asynchronously.
        /// </summary>
        /// <param name="largePersonGroupId">The large person group id.</param>
        /// <param name="personId">The person id.</param>
        /// <param name="persistedFaceId">The persisted face id.</param>
        /// <returns>The person face entity.</returns>
        Task<PersistedFace> GetPersonFaceInLargePersonGroupAsync(string largePersonGroupId, Guid personId, Guid persistedFaceId);

        /// <summary>
        /// Updates a face of a person in large person group asynchronously.
        /// </summary>
        /// <param name="largePersonGroupId">The large person group id.</param>
        /// <param name="personId">The person id.</param>
        /// <param name="persistedFaceId">The persisted face id.</param>
        /// <param name="userData">The user data.</param>
        /// <returns>Task object.</returns>
        Task UpdatePersonFaceInLargePersonGroupAsync(string largePersonGroupId, Guid personId, Guid persistedFaceId, string userData = null);

        #endregion
    }
}