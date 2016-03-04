﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Web.Script.Serialization;
using System.Net.Http.Headers;

namespace rosette_api
{
    /// <summary>C# Rosette API.
    /// <para>
    /// Primary class for interfacing with the Rosette API
    /// @copyright 2014-2015 Basis Technology Corporation.
    /// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance
    /// with the License. You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
    /// Unless required by applicable law or agreed to in writing, software distributed under the License is
    /// distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
    /// See the License for the specific language governing permissions and limitations under the License.
    /// </para>
    /// </summary>
    public class CAPI
    {
        /// <summary>
        /// Internal string to hold the uri ending for each endpoint. 
        /// Set when an endpoint is called.
        /// </summary>
        private string _uri = null;

        /// <summary>
        /// Internal list of Morphological features added to the end of the Morphology URI.
        /// </summary>
        private List<string> Morphofeatures = null;

        /// <summary>
        /// Internal check to see if the version matches. Defaults to false and set during initialization.
        /// </summary>
        private bool version_checked;

        /// <summary>
        /// Internal time value of the last version check. Set on first version check. Resets the version check after 24hrs. 
        /// </summary>
        private DateTime last_version_check;

        /// <summary>
        /// String to set version number. Must be updated on API update.
        /// </summary>
        private string binding_version = "0.7";

        /// <summary>C# API class
        /// <para>Rosette Python Client Binding API; representation of a Rosette server.
        /// Instance methods of the C# API provide communication with specific Rosette server endpoints.
        /// Requires user_key to start and has 3 additional parameters to be specified. 
        /// Will run a Version Check against the Rosette Server. If the version check fails, a 
        /// RosetteException will be thrown. 
        /// </para>
        /// </summary>
        /// <param name="user_key">string: API key required by the Rosette server to allow access to endpoints</param>
        /// <param name="uristring">(string, optional): Base URL for the HttpClient requests. If none is given, will use the default API URI</param>
        /// <param name="maxRetry">(int, optional): Maximum number of times to retry a request on HttpResponse error. Default is 3 times.</param> 
        /// <param name="client">(HttpClient, optional): Forces the API to use a custom HttpClient.</param> 
        public CAPI(string user_key, string uristring = "https://api.rosette.com/rest/v1/", int maxRetry = 1, HttpClient client = null)
        {
            UserKey = user_key;
            URIstring = (uristring == null) ? "https://api.rosette.com/rest/v1/" : uristring;
            MaxRetry = (maxRetry == 0) ? 1: maxRetry;
            Debug = false;
            Morphofeatures = new List<string> { "complete", "lemmas", "parts-of-speech", "compound-components", "han-readings" };
            Version = binding_version;
            Timeout = 300;
            Client = client;
            version_checked = checkVersion();
            last_version_check = default(DateTime);
        }

        /// <summary>UserKey
        /// <para>
        /// Getter, Setter for the UserKey
        /// UserKey: API key required by the Rosette Server
        /// Allows users to change their UserKey later (e.g. if instantiated class incorrectly)
        /// </para>
        /// </summary>
        public string UserKey { get; set; }

        /// <summary>URIstring
        /// <para>
        /// Getter, Setter for the URIstring
        /// URIstring: Base URI for the HttpClient.
        /// Allows users to change their URIstring later (e.g. if instantiated class incorrectly)
        /// </para>
        /// </summary>
        public string URIstring { get; set; }

        /// <summary>Version
        /// <para>
        /// Getter, Setter for the Version
        /// Version: Internal Server Version number.
        /// </para>
        /// </summary>
        private string Version { get; set; }

        /// <summary>MaxRetry
        /// <para>
        /// Getter, Setter for the MaxRetry
        /// MaxRetry: Maximum number of times to retry a request on HTTPResponse error.
        /// Allows users to change their MaxRetry later (e.g. if instantiated class incorrectly)
        /// </para>
        /// </summary>
        public int MaxRetry { get; set; }

        /// <summary>Client
        /// <para>
        /// Getter, Setter for the Client
        /// Client: Forces the API to use a custom HttpClient.
        /// </para>
        /// </summary>
        public HttpClient Client { get; set; }

        /// <summary>Debug
        /// <para>
        /// Getter, Setter for the Debug
        /// Debug: Sets the debug mode parameter for the Rosette server.
        /// </para>
        /// </summary>
        public bool Debug { get; set; }

        /// <summary>Timeout
        /// <para>
        /// Getter, Setter for the Timeout
        /// Timeout: Sets the Timeout for the HttpClient.
        /// </para>
        /// </summary>
        public int Timeout { get; set; }

        /// <summary>Categories
        /// <para>
        /// (POST)Categories Endpoint: Returns an ordered list of categories identified in the input. The categories are Tier 1 contextual categories defined in the QAG Taxonomy.
        /// </para>
        /// </summary>
        /// <param name="content">(string, optional): Input to process (JSON string or base64 encoding of non-JSON string)</param>
        /// <param name="language">(string, optional): Language: ISO 639-3 code (ignored for the /language endpoint)</param>
        /// <param name="contentType">(string, optional): MIME type of the input (required for base64 content; if content type is unknown, set to "application/octet-stream")</param>
        /// <param name="unit">(string, optional): Input unit: "doc" (document, the default) or "sentence"</param>
        /// <param name="contentUri">(string, optional): URI to accessible content (content and contentUri are mutually exclusive)</param>
        /// <returns>Dictionary&lt;string, object&gt;: Dictionary containing the results of the request.
        /// The response is the contextual categories identified in the input.
        /// </returns>
        public Dictionary<string, object> Categories(string content = null, string language = null, string contentType = null, string unit = null, string contentUri = null)
        {
            _uri = "categories/";
            return Process(content, language, contentType, unit, contentUri);
        }

        /// <summary>Categories
        /// <para>
        /// (POST)Categories Endpoint: Returns an ordered list of categories identified in the input. The categories are Tier 1 contextual categories defined in the QAG Taxonomy.
        /// </para>
        /// </summary>
        /// <param name="dict">Dictionary&lt;object, object&gt;: Dictionary containing parameters as (key,value) pairs</param>
        /// <returns>Dictionary&lt;string, object&gt;: Dictionary containing the results of the request.
        /// The response is the contextual categories identified in the input.
        /// </returns>
        public Dictionary<string, object> Categories(Dictionary<object, object> dict)
        {
            _uri = "categories/";
            return getResponse(SetupClient(), new JavaScriptSerializer().Serialize(dict));
        }

        /// <summary>Categories
        /// <para>
        /// (POST)Categories Endpoint: Returns an ordered list of categories identified in the input. The categories are Tier 1 contextual categories defined in the QAG Taxonomy.
        /// </para>
        /// </summary>
        /// <param name="file">RosetteFile: RosetteFile Object containing the file (and possibly options) to upload</param>
        /// <returns>Dictionary&lt;string, object&gt;: Dictionary containing the results of the request.
        /// The response is the contextual categories identified in the input.
        /// </returns>
        public Dictionary<string, object> Categories(RosetteFile file)
        {
            _uri = "categories/";
            return Process(file);
        }

        /// <summary>CategoriesInfo
        /// <para>
        /// (GETCategoriesInfo Endpoint: Response is a JSON string with request id, version, build info, and support (null for now).
        /// </para>
        /// </summary>
        /// <returns>Dictionary&lt;string, object&gt;: Dictionary containing the results of the info GET.</returns>
        public Dictionary<string, object> CategoriesInfo()
        {
            _uri = "categories/info";
            return getResponse(SetupClient());
        }

        /// <summary>EntitiesLinked
        /// <para>
        /// (POST)EntitiesLinked Endpoint: Links entities in the input to entities in the knowledge base.
        /// </para>
        /// </summary>
        /// <param name="content">(string, optional): Input to process (JSON string or base64 encoding of non-JSON string)</param>
        /// <param name="language">(string, optional): Language: ISO 639-3 code (ignored for the /language endpoint)</param>
        /// <param name="contentType">(string, optional): MIME type of the input (required for base64 content; if content type is unknown, set to "application/octet-stream")</param>
        /// <param name="unit">(string, optional): Input unit: "doc" (document, the default) or "sentence"</param>
        /// <param name="contentUri">(string, optional): URI to accessible content (content and contentUri are mutually exclusive)</param>
        /// <returns>Dictionary&lt;string, object&gt;: Dictionary containing the results of the request.
        /// The response identifies the entities in the input that have been linked to entities in the knowledge base. 
        /// Each entity includes an entity id (from the knowledge base), a chain id (all instances of the same entity share a chain id), 
        /// the mention (entity text from the input), and confidence associated with the linking.
        /// </returns>
        public Dictionary<string, object> EntitiesLinked(string content = null, string language = null, string contentType = null, string unit = null, string contentUri = null)
        {
            _uri = "entities/linked/";
            return Process(content, language, contentType, unit, contentUri);
        }

        /// <summary>EntitiesLinked
        /// <para>
        /// (POST)EntitiesLinked Endpoint: Links entities in the input to entities in the knowledge base.
        /// </para>
        /// </summary>
        /// <param name="dict">Dictionary&lt;object, object&gt;: Dictionary containing parameters as (key,value) pairs</param>
        /// <returns>Dictionary&lt;string, object&gt;: Dictionary containing the results of the request.
        /// The response identifies the entities in the input that have been linked to entities in the knowledge base. 
        /// Each entity includes an entity id (from the knowledge base), a chain id (all instances of the same entity share a chain id), 
        /// the mention (entity text from the input), and confidence associated with the linking.
        /// </returns>
        public Dictionary<string, object> EntitiesLinked(Dictionary<object, object> dict)
        {
            _uri = "entities/linked/";
            return getResponse(SetupClient(), new JavaScriptSerializer().Serialize(dict));
        }

        /// <summary>EntitiesLinked
        /// <para>
        /// (POST)EntitiesLinked Endpoint: Links entities in the input to entities in the knowledge base.
        /// </para>
        /// </summary>
        /// <param name="file">RosetteFile: RosetteFile Object containing the file (and possibly options) to upload</param>
        /// <returns>Dictionary&lt;string, object&gt;: Dictionary containing the results of the request.
        /// The response identifies the entities in the input that have been linked to entities in the knowledge base. 
        /// Each entity includes an entity id (from the knowledge base), a chain id (all instances of the same entity share a chain id), 
        /// the mention (entity text from the input), and confidence associated with the linking.
        /// </returns>
        public Dictionary<string, object> EntitiesLinked(RosetteFile file)
        {
            _uri = "entities/linked/";
            return Process(file);
        }

        /// <summary>EntitiesLinkedInfo
        /// <para>
        /// (GET)EntitiesLinkedInfo Endpoint: Response is a JSON string with request id, version, build info, and support (null for now).
        /// </para>
        /// </summary>
        /// <returns>Dictionary&lt;string, object&gt;: Dictionary containing the results of the info GET.</returns>
        public Dictionary<string, object> EntitiesLinkedInfo()
        {
            _uri = "entities/linked/info";
            return getResponse(SetupClient());
        }

        /// <summary>Entity
        /// <para>
        /// (POST)Entity Endpoint: Returns each entity extracted from the input.
        /// </para>
        /// </summary>
        /// <param name="content">(string, optional): Input to process (JSON string or base64 encoding of non-JSON string)</param>
        /// <param name="language">(string, optional): Language: ISO 639-3 code (ignored for the /language endpoint)</param>
        /// <param name="contentType">(string, optional): MIME type of the input (required for base64 content; if content type is unknown, set to "application/octet-stream")</param>
        /// <param name="unit">(string, optional): Input unit: "doc" (document, the default) or "sentence"</param>
        /// <param name="contentUri">(string, optional): URI to accessible content (content and contentUri are mutually exclusive)</param>
        /// <returns>Dictionary&lt;string, object&gt;: Dictionary containing the results of the request. 
        /// The response is a list of extracted entities. 
        /// Each entity includes chain ID (all instances of the same entity share a chain id), mention (entity text in the input), 
        /// normalized text (the most complete form of this entity that appears in the input), count (how many times this entity appears in the input), 
        /// and the confidence associated with the extraction.
        /// </returns>
        public Dictionary<string, object> Entity(string content = null, string language = null, string contentType = null, string unit = null, string contentUri = null)
        {
            _uri = "entities/";
            return Process(content, language, contentType, unit, contentUri);
        }

        /// <summary>Entity
        /// <para>
        /// (POST)Entity Endpoint: Returns each entity extracted from the input.
        /// </para>
        /// </summary>
        /// <param name="dict">Dictionary&lt;object, object&gt;: Dictionary containing parameters as (key,value) pairs</param>
        /// <returns>Dictionary&lt;string, object&gt;: Dictionary containing the results of the request. 
        /// The response is a list of extracted entities. 
        /// Each entity includes chain ID (all instances of the same entity share a chain id), mention (entity text in the input), 
        /// normalized text (the most complete form of this entity that appears in the input), count (how many times this entity appears in the input), 
        /// and the confidence associated with the extraction.
        /// </returns>
        public Dictionary<string, object> Entity(Dictionary<object, object> dict)
        {
            _uri = "entities/";
            return getResponse(SetupClient(), new JavaScriptSerializer().Serialize(dict));
        }

        /// <summary>Entity
        /// <para>
        /// (POST)Entity Endpoint: Returns each entity extracted from the input.
        /// </para>
        /// </summary>
        /// <param name="file">RosetteFile: RosetteFile Object containing the file (and possibly options) to upload</param>
        /// <returns>Dictionary&lt;string, object&gt;: Dictionary containing the results of the request. 
        /// The response is a list of extracted entities. 
        /// Each entity includes chain ID (all instances of the same entity share a chain id), mention (entity text in the input), 
        /// normalized text (the most complete form of this entity that appears in the input), count (how many times this entity appears in the input), 
        /// and the confidence associated with the extraction.
        /// </returns>
        public Dictionary<string, object> Entity(RosetteFile file)
        {
            _uri = "entities/";
            return Process(file);
        }

        /// <summary>EntityInfo
        /// <para>
        /// (GET)EntityInfo Endpoint: Response is a JSON string with request id, version, build info, and support (null for now).
        /// </para>
        /// </summary>
        /// <returns>Dictionary&lt;string, object&gt;: Dictionary containing the results of the info GET.</returns>
        public Dictionary<string, object> EntityInfo()
        {
            _uri = "entities/info";
            return getResponse(SetupClient());
        }

        /// <summary>Info
        /// <para>
        /// (GET)Info Endpoint: Response is a JSON string with Rosette API information including buildNumber, name, version, and buildTime.
        /// </para>
        /// </summary>
        /// <returns>Dictionary&lt;string, object&gt;: Dictionary containing the results of the info GET.</returns>
        public Dictionary<string, object> Info()
        {
            _uri = "info/";
            return getResponse(SetupClient());
        }

        /// <summary>Language
        /// <para>
        /// (POST)Language Endpoint: Returns list of candidate languages in order of descending confidence.
        /// </para>
        /// </summary>
        /// <param name="content">(string, optional): Input to process (JSON string or base64 encoding of non-JSON string)</param>
        /// <param name="language">(string, optional): Language: ISO 639-3 code (ignored for the /language endpoint)</param>
        /// <param name="contentType">(string, optional): MIME type of the input (required for base64 content; if content type is unknown, set to "application/octet-stream")</param>
        /// <param name="unit">(string, optional): Input unit: "doc" (document, the default) or "sentence"</param>
        /// <param name="contentUri">(string, optional): URI to accessible content (content and contentUri are mutually exclusive)</param>
        /// <returns>Dictionary&lt;string, object&gt;: Dictionary containing the results of the request. 
        /// The response is an ordered list of detected languages, including language and detection confidence, sorted by descending confidence.
        /// </returns>
        public Dictionary<string, object> Language(string content = null, string language = null, string contentType = null, string unit = null, string contentUri = null)
        {
            _uri = "language/";
            return Process(content, language, contentType, unit, contentUri);
        }

        /// <summary>Language
        /// <para>
        /// (POST)Language Endpoint: Returns list of candidate languages in order of descending confidence.
        /// </para>
        /// </summary>
        /// <param name="dict">Dictionary&lt;object, object&gt;: Dictionary containing parameters as (key,value) pairs</param>
        /// <returns>Dictionary&lt;string, object&gt;: Dictionary containing the results of the request. 
        /// The response is an ordered list of detected languages, including language and detection confidence, sorted by descending confidence.
        /// </returns>
        public Dictionary<string, object> Language(Dictionary<object, object> dict)
        {
            _uri = "language/";
            return getResponse(SetupClient(), new JavaScriptSerializer().Serialize(dict));
        }

        /// <summary>Language
        /// <para>
        /// (POST)Language Endpoint: Returns list of candidate languages in order of descending confidence.
        /// </para>
        /// </summary>
        /// <param name="file">RosetteFile: RosetteFile Object containing the file (and possibly options) to upload</param>
        /// <returns>Dictionary&lt;string, object&gt;: Dictionary containing the results of the request. 
        /// The response is an ordered list of detected languages, including language and detection confidence, sorted by descending confidence.
        /// </returns>
        public Dictionary<string, object> Language(RosetteFile file)
        {
            _uri = "language/";
            return Process(file);
        }

        /// <summary>LanguageInfo
        /// <para>
        /// (GET)LanguageInfo Endpoint: Response is a JSON string with request id and supported languages, each with a list of one or more supported scripts..
        /// </para>
        /// </summary>
        /// <returns>Dictionary&lt;string, object&gt;: Dictionary containing the results of the info GET.</returns>
        public Dictionary<string, object> LanguageInfo()
        {
            _uri = "language/info";
            return getResponse(SetupClient());
        }

        /// <summary>Morphology
        /// <para>
        /// (POST)Morphology Endpoint: Returns morphological analysis of input.
        /// </para>
        /// </summary>
        /// <param name="content">(string, optional): Input to process (JSON string or base64 encoding of non-JSON string)</param>
        /// <param name="language">(string, optional): Language: ISO 639-3 code (ignored for the /language endpoint)</param>
        /// <param name="contentType">(string, optional): MIME type of the input (required for base64 content; if content type is unknown, set to "application/octet-stream")</param>
        /// <param name="unit">(string, optional): Input unit: "doc" (document, the default) or "sentence"</param>
        /// <param name="contentUri">(string, optional): URI to accessible content (content and contentUri are mutually exclusive)</param>
        /// <param name="feature">(string, optional): Description of what morphology feature to request from the Rosette server</param>
        /// <returns>Dictionary&lt;string, object&gt;: Dictionary containing the results of the request. 
        /// The response may include lemmas, part of speech tags, compound word components, and Han readings. 
        /// Support for specific return types depends on language.
        /// </returns>
        public Dictionary<string, object> Morphology(string content = null, string language = null, string contentType = null, string unit = null, string contentUri = null, string feature = "complete")
        {
            _uri = Morphofeatures.Contains(feature) ? "morphology/" + feature : "morphology/complete";
            return Process(content, language, contentType, unit, contentUri);
        }

        /// <summary>Morphology
        /// <para>
        /// (POST)Morphology Endpoint: Returns morphological analysis of input.
        /// </para>
        /// </summary>
        /// <param name="dict">Dictionary&lt;object, object&gt;: Dictionary containing parameters as (key,value) pairs</param>
        /// <param name="feature">(string, optional): Description of what morphology feature to request from the Rosette server</param>
        /// <returns>Dictionary&lt;string, object&gt;: Dictionary containing the results of the request. 
        /// The response may include lemmas, part of speech tags, compound word components, and Han readings. 
        /// Support for specific return types depends on language.
        /// </returns>
        public Dictionary<string, object> Morphology(Dictionary<object, object> dict, string feature = "complete")
        {
            _uri = Morphofeatures.Contains(feature) ? "morphology/" + feature : "morphology/complete";
            return getResponse(SetupClient(), new JavaScriptSerializer().Serialize(dict));
        }

        /// <summary>Morphology
        /// <para>
        /// (POST)Morphology Endpoint: Returns morphological analysis of input.
        /// </para>
        /// </summary>
        /// <param name="file">RosetteFile: RosetteFile Object containing the file (and possibly options) to upload</param>
        /// <param name="feature">(string, optional): Description of what morphology feature to request from the Rosette server</param>
        /// <returns>Dictionary&lt;string, object&gt;: Dictionary containing the results of the request. 
        /// The response may include lemmas, part of speech tags, compound word components, and Han readings. 
        /// Support for specific return types depends on language.
        /// </returns>
        public Dictionary<string, object> Morphology(RosetteFile file, string feature = "complete")
        {
            _uri = Morphofeatures.Contains(feature) ? "morphology/" + feature : "morphology/complete";
            return Process(file);
        }

        /// <summary>NameSimilarity
        /// <para>
        /// (POST)NameSimilarity Endpoint: Returns the result of matching 2 names.
        /// </para>
        /// </summary>
        /// <param name="n1">Name: First name to be matched</param>
        /// <param name="n2">Name: Second name to be matched</param>
        /// <returns>Dictionary&lt;string, object&gt;: Dictionary containing the results of the request. 
        /// </returns>
        public Dictionary<string, object> NameSimilarity(Name n1, Name n2)
        {
            _uri = "name-similarity/";

            return getResponse(SetupClient(), new JavaScriptSerializer().Serialize(new Dictionary<string, object>(){
                { "name1", n1},
                { "name2", n2}
            }));
        }

        /// deprecated
        /// <summary>MatchedName
        /// <para>
        /// (POST)MatchedName Endpoint: Returns the result of matching 2 names.
        /// </para>
        /// </summary>
        /// <param name="n1">Name: First name to be matched</param>
        /// <param name="n2">Name: Second name to be matched</param>
        /// <returns>Dictionary&lt;string, object&gt;: Dictionary containing the results of the request. 
        /// </returns>
        public Dictionary<string, object> MatchedName(Name n1, Name n2)
        {
            return NameSimilarity(n1, n2);
        }

        /// <summary>NameSimilarity
        /// <para>
        /// (POST)NameSimilarity Endpoint: Returns the result of matching 2 names.
        /// </para>
        /// </summary>
        /// <param name="dict">Dictionary&lt;object, object&gt;: Dictionary containing parameters as (key,value) pairs</param>
        /// <returns>Dictionary&lt;string, object&gt;: Dictionary containing the results of the request. 
        /// </returns>
        public Dictionary<string, object> NameSimilarity(Dictionary<object, object> dict)
        {
            _uri = "name-similarity/";
            return getResponse(SetupClient(), new JavaScriptSerializer().Serialize(dict));
        }

        /// deprecated
        /// <summary>MatchedName
        /// <para>
        /// (POST)MatchedName Endpoint: Returns the result of matching 2 names.
        /// </para>
        /// </summary>
        /// <param name="dict">Dictionary&lt;object, object&gt;: Dictionary containing parameters as (key,value) pairs</param>
        /// <returns>Dictionary&lt;string, object&gt;: Dictionary containing the results of the request. 
        /// </returns>
        public Dictionary<string, object> MatchedName(Dictionary<object, object> dict)
        {
            return NameSimilarity(dict);
        }

        /// <summary>NameSimilarityInfo
        /// <para>
        /// (GET)NameSimilarityInfo Endpoint: Response is a JSON string with request id, version, build info, and support (null for now).
        /// </para>
        /// </summary>
        /// <returns>Dictionary&lt;string, object&gt;: Dictionary containing the results of the info GET.</returns>
        public Dictionary<string, object> NameSimilarityInfo()
        {
            _uri = "name-similarity/info";
            return getResponse(SetupClient());
        }

        /// deprecated
        /// <summary>MatchedNameInfo
        /// <para>
        /// (GET)MatchedNameInfo Endpoint: Response is a JSON string with request id, version, build info, and support (null for now).
        /// </para>
        /// </summary>
        /// <returns>Dictionary&lt;string, object&gt;: Dictionary containing the results of the info GET.</returns>
        public Dictionary<string, object> MatchedNameInfo()
        {
            return NameSimilarityInfo();
        }

        /// <summary>Ping
        /// (GET)Ping Endpoint: Pings Rosette API for a response indicting that the service is available
        /// </summary>
        /// <returns>Dictionary&lt;string, object&gt;: Dictionary containing the results of the info GET.
        /// The reponse contains a message and time.
        /// </returns>
        public Dictionary<string, object> Ping()
        {
            _uri = "ping";
            return getResponse(SetupClient());
        }

        /// <summary>Relationships
        /// <para>
        /// (POST)Relationships Endpoint: Returns each relationship extracted from the input.
        /// </para>
        /// </summary>
        /// <param name="content">(string, optional): Input to process (JSON string or base64 encoding of non-JSON string)</param>
        /// <param name="language">(string, optional): Language: ISO 639-3 code (ignored for the /language endpoint)</param>
        /// <param name="contentType">(string, optional): MIME type of the input (required for base64 content; if content type is unknown, set to "application/octet-stream")</param>
        /// <param name="unit">(string, optional): Input unit: "doc" (document, the default) or "sentence"</param>
        /// <param name="contentUri">(string, optional): URI to accessible content (content and contentUri are mutually exclusive)</param>
        /// <returns>
        /// The response is a list of extracted relationships. A relationship contains
        /// 
        /// predicate - usually the main verb, property or action that is expressed by the text
        /// arg1 - usually the subject, agent or main actor of the relationship
        /// arg2 [optional] - complements the predicate and is usually the object, theme or patient of the relationship
        /// arg3 [optional] - usually an additional object in ditransitive verbs
        /// adjuncts [optional] - contain all optional parts of a relationship which are not temporal or locative expressions
        /// locatives [optional] - usually express the locations the action expressed by the relationship took place
        /// temporals [optional] - usually express the time in which the action expressed by the relationship took place
        /// confidence = a measure of quality of relationship extraction, between 0 - 1
        /// </returns>
        public Dictionary<string, object> Relationships(string content = null, string language = null, string contentType = null, string unit = null, string contentUri = null)
        {
            _uri = "relationships/";
            return Process(content, language, contentType, unit, contentUri);
        }

        /// <summary>Relationships
        /// <para>
        /// (POST)Relationships Endpoint: Returns each relationship extracted from the input.
        /// </para>
        /// </summary>
        /// <param name="dict">Dictionary&lt;object, object&gt;: Dictionary containing parameters as (key,value) pairs</param>
        /// <returns>
        /// The response is a list of extracted relationships. A relationship contains
        /// 
        /// predicate - usually the main verb, property or action that is expressed by the text
        /// arg1 - usually the subject, agent or main actor of the relationship
        /// arg2 [optional] - complements the predicate and is usually the object, theme or patient of the relationship
        /// arg3 [optional] - usually an additional object in ditransitive verbs
        /// adjuncts [optional] - contain all optional parts of a relationship which are not temporal or locative expressions
        /// locatives [optional] - usually express the locations the action expressed by the relationship took place
        /// temporals [optional] - usually express the time in which the action expressed by the relationship took place
        /// confidence = a measure of quality of relationship extraction, between 0 - 1
        /// </returns>
        public Dictionary<string, object> Relationships(Dictionary<object, object> dict)
        {
            _uri = "relationships/";
            return getResponse(SetupClient(), new JavaScriptSerializer().Serialize(dict));
        }

        /// <summary>Relationships
        /// <para>
        /// (POST)Relationships Endpoint: Returns each relationship extracted from the input.
        /// </para>
        /// </summary>
        /// <param name="file">RosetteFile: RosetteFile Object containing the file (and possibly options) to upload</param>
        /// <returns>
        /// The response is a list of extracted relationships. A relationship contains
        /// 
        /// predicate - usually the main verb, property or action that is expressed by the text
        /// arg1 - usually the subject, agent or main actor of the relationship
        /// arg2 [optional] - complements the predicate and is usually the object, theme or patient of the relationship
        /// arg3 [optional] - usually an additional object in ditransitive verbs
        /// adjuncts [optional] - contain all optional parts of a relationship which are not temporal or locative expressions
        /// locatives [optional] - usually express the locations the action expressed by the relationship took place
        /// temporals [optional] - usually express the time in which the action expressed by the relationship took place
        /// confidence = a measure of quality of relationship extraction, between 0 - 1
        /// </returns>
        public Dictionary<string, object> Relationships(RosetteFile file)
        {
            _uri = "relationships/";
            return Process(file);
        }

        /// <summary>Sentences
        /// <para>
        /// (POST)Sentences Endpoint: Divides the input into sentences.
        /// </para>
        /// </summary>
        /// <param name="content">(string, optional): Input to process (JSON string or base64 encoding of non-JSON string)</param>
        /// <param name="language">(string, optional): Language: ISO 639-3 code (ignored for the /language endpoint)</param>
        /// <param name="contentType">(string, optional): MIME type of the input (required for base64 content; if content type is unknown, set to "application/octet-stream")</param>
        /// <param name="unit">(string, optional): Input unit: "doc" (document, the default) or "sentence"</param>
        /// <param name="contentUri">(string, optional): URI to accessible content (content and contentUri are mutually exclusive)</param>
        /// <returns>Dictionary&lt;string, object&gt;: Dictionary containing the results of the request. 
        /// The response contains a list of sentences.
        /// </returns>
        public Dictionary<string, object> Sentences(string content = null, string language = null, string contentType = null, string unit = null, string contentUri = null)
        {
            _uri = "sentences/";
            return Process(content, language, contentType, unit, contentUri);
        }

        /// <summary>Sentences
        /// <para>
        /// (POST)Sentences Endpoint: Divides the input into sentences.
        /// </para>
        /// </summary>
        /// <param name="dict">Dictionary&lt;object, object&gt;: Dictionary containing parameters as (key,value) pairs</param>
        /// <returns>Dictionary&lt;string, object&gt;: Dictionary containing the results of the request. 
        /// The response contains a list of sentences.
        /// </returns>
        public Dictionary<string, object> Sentences(Dictionary<object, object> dict)
        {
            _uri = "sentences/";
            return getResponse(SetupClient(), new JavaScriptSerializer().Serialize(dict));
        }

        /// <summary>Sentences
        /// <para>
        /// (POST)Sentences Endpoint: Divides the input into sentences.
        /// </para>
        /// </summary>
        /// <param name="file">RosetteFile: RosetteFile Object containing the file (and possibly options) to upload</param>
        /// <returns>Dictionary&lt;string, object&gt;: Dictionary containing the results of the request. 
        /// The response contains a list of sentences.
        /// </returns>
        public Dictionary<string, object> Sentences(RosetteFile file)
        {
            _uri = "sentences/";
            return Process(file);
        }

        /// <summary>Sentiment
        /// <para>
        /// (POST)Sentiment Endpoint: Analyzes the positive and negative sentiment expressed by the input.
        /// </para>
        /// </summary>
        /// <param name="content">(string, optional): Input to process (JSON string or base64 encoding of non-JSON string)</param>
        /// <param name="language">(string, optional): Language: ISO 639-3 code (ignored for the /language endpoint)</param>
        /// <param name="contentType">(string, optional): MIME type of the input (required for base64 content; if content type is unknown, set to "application/octet-stream")</param>
        /// <param name="unit">(string, optional): Input unit: "doc" (document, the default) or "sentence"</param>
        /// <param name="contentUri">(string, optional): URI to accessible content (content and contentUri are mutually exclusive)</param>
        /// <returns>Dictionary&lt;string, object&gt;: Dictionary containing the results of the request. 
        /// The response contains sentiment analysis results.
        /// </returns>
        public Dictionary<string, object> Sentiment(string content = null, string language = null, string contentType = null, string unit = null, string contentUri = null)
        {
            _uri = "sentiment/";
            return Process(content, language, contentType, unit, contentUri);
        }

        /// <summary>Sentiment
        /// <para>
        /// (POST)Sentiment Endpoint: Analyzes the positive and negative sentiment expressed by the input.
        /// </para>
        /// </summary>
        /// <param name="dict">Dictionary&lt;object, object&gt;: Dictionary containing parameters as (key,value) pairs</param>
        /// <returns>Dictionary&lt;string, object&gt;: Dictionary containing the results of the request. 
        /// The response contains sentiment analysis results.
        /// </returns>
        public Dictionary<string, object> Sentiment(Dictionary<object, object> dict)
        {
            _uri = "sentiment/";
            return getResponse(SetupClient(), new JavaScriptSerializer().Serialize(dict));
        }

        /// <summary>Sentiment
        /// <para>
        /// (POST)Sentiment Endpoint: Analyzes the positive and negative sentiment expressed by the input.
        /// </para>
        /// </summary>
        /// <param name="file">RosetteFile: RosetteFile Object containing the file (and possibly options) to upload</param>
        /// <returns>Dictionary&lt;string, object&gt;: Dictionary containing the results of the request. 
        /// The response contains sentiment analysis results.
        /// </returns>
        public Dictionary<string, object> Sentiment(RosetteFile file)
        {
            _uri = "sentiment/";
            return Process(file);
        }

        /// <summary>SentimentInfo
        /// <para>
        /// (GET)SentimentInfo Endpoint: Response is a JSON string with request id, version, build info, and support (null for now).
        /// </para>
        /// </summary>
        /// <returns>Dictionary&lt;string, object&gt;: Dictionary containing the results of the info GET.</returns>
        public Dictionary<string, object> SentimentInfo()
        {
            _uri = "sentiment/info";
            return getResponse(SetupClient());
        }

        /// <summary>Tokens
        /// <para>
        /// (POST)Tokens Endpoint: Divides the input into tokens.
        /// </para>
        /// </summary>
        /// <param name="content">(string, optional): Input to process (JSON string or base64 encoding of non-JSON string)</param>
        /// <param name="language">(string, optional): Language: ISO 639-3 code (ignored for the /language endpoint)</param>
        /// <param name="contentType">(string, optional): MIME type of the input (required for base64 content; if content type is unknown, set to "application/octet-stream")</param>
        /// <param name="unit">(string, optional): Input unit: "doc" (document, the default) or "sentence"</param>
        /// <param name="contentUri">(string, optional): URI to accessible content (content and contentUri are mutually exclusive)</param>
        /// <returns>Dictionary&lt;string, object&gt;: Dictionary containing the results of the request. 
        /// The response contains a list of tokens.
        /// </returns>
        public Dictionary<string, object> Tokens(string content = null, string language = null, string contentType = null, string unit = null, string contentUri = null)
        {
            _uri = "tokens/";
            return Process(content, language, contentType, unit, contentUri);
        }

        /// <summary>Tokens
        /// <para>
        /// (POST)Tokens Endpoint: Divides the input into tokens.
        /// </para>
        /// </summary>
        /// <param name="dict">Dictionary&lt;object, object&gt;: Dictionary containing parameters as (key,value) pairs</param>
        /// <returns>Dictionary&lt;string, object&gt;: Dictionary containing the results of the request. 
        /// The response contains a list of tokens.
        /// </returns>
        public Dictionary<string, object> Tokens(Dictionary<object, object> dict)
        {
            _uri = "tokens/";
            return getResponse(SetupClient(), new JavaScriptSerializer().Serialize(dict));
        }

        /// <summary>Tokens
        /// <para>
        /// (POST)Tokens Endpoint: Divides the input into tokens.
        /// </para>
        /// </summary>
        /// <param name="file">RosetteFile: RosetteFile Object containing the file (and possibly options) to upload</param>
        /// <returns>Dictionary&lt;string, object&gt;: Dictionary containing the results of the request. 
        /// The response contains a list of tokens.
        /// </returns>
        public Dictionary<string, object> Tokens(RosetteFile file)
        {
            _uri = "tokens/";
            return Process(file);
        }

        /// <summary>NameTranslation
        /// <para>
        /// (POST)NameTranslation Endpoint: Returns the translation of a name. You must specify the name to translate and the target language for the translation.
        /// </para>
        /// </summary>
        /// <param name="name">string: Name to be translated</param>
        /// <param name="sourceLanguageOfUse">(string, optional): ISO 639-3 code for the name's language of use</param>
        /// <param name="sourceScript">(string, optional): ISO 15924 code for the name's script</param>
        /// <param name="targetLanguage">(string): ISO 639-3 code for the translation language</param>
        /// <param name="targetScript">(string, optional): ISO 15924 code for the translation script</param>
        /// <param name="targetScheme">(string, optional): transliteration scheme for the translation</param>
        /// <param name="sourceLanguageOfOrigin">(string, optional): ISO 639-3 code for the name's language of origin</param>
        /// <param name="entityType">(string, optional): Entity type of the name: PERSON, LOCATION, or ORGANIZATION</param>
        /// <returns>Dictionary&lt;string, object&gt;: Dictionary containing the results of the request. 
        /// </returns>
        public Dictionary<string, object> NameTranslation(string name, string sourceLanguageOfUse = null, string sourceScript = null, string targetLanguage = null, string targetScript = null, string targetScheme = null, string sourceLanguageOfOrigin = null, string entityType = null)
        {
            _uri = "name-translation/";

            return getResponse(SetupClient(), new JavaScriptSerializer().Serialize(new Dictionary<string, string>(){
                { "name", name},
                { "sourceLanguageOfUse", sourceLanguageOfUse},
                { "sourceScript", sourceScript},
                { "targetLanguage", targetLanguage},
                { "targetScript", targetScript},
                { "targetScheme", targetScheme},
                { "sourceLanguageOfOrigin", sourceLanguageOfOrigin},
                { "entityType", entityType}
            }.Where(f => !String.IsNullOrEmpty(f.Value)).ToDictionary(x => x.Key, x => x.Value)));
        }

        /// deprecated
        /// <summary>TranslatedName
        /// <para>
        /// (POST)TranslatedName Endpoint: Returns the translation of a name. You must specify the name to translate and the target language for the translation.
        /// </para>
        /// </summary>
        /// <param name="name">string: Name to be translated</param>
        /// <param name="sourceLanguageOfUse">(string, optional): ISO 639-3 code for the name's language of use</param>
        /// <param name="sourceScript">(string, optional): ISO 15924 code for the name's script</param>
        /// <param name="targetLanguage">(string): ISO 639-3 code for the translation language</param>
        /// <param name="targetScript">(string, optional): ISO 15924 code for the translation script</param>
        /// <param name="targetScheme">(string, optional): transliteration scheme for the translation</param>
        /// <param name="sourceLanguageOfOrigin">(string, optional): ISO 639-3 code for the name's language of origin</param>
        /// <param name="entityType">(string, optional): Entity type of the name: PERSON, LOCATION, or ORGANIZATION</param>
        /// <returns>Dictionary&lt;string, object&gt;: Dictionary containing the results of the request. 
        /// </returns>
        public Dictionary<string, object> TranslatedName(string name, string sourceLanguageOfUse = null, string sourceScript = null, string targetLanguage = null, string targetScript = null, string targetScheme = null, string sourceLanguageOfOrigin = null, string entityType = null)
        {
            return NameTranslation(name, sourceLanguageOfUse, sourceScript, targetLanguage, targetScript, targetScheme, sourceLanguageOfOrigin, entityType);
        }

        /// <summary>NameTranslation
        /// <para>
        /// (POST)NameTranslation Endpoint: Returns the translation of a name. You must specify the name to translate and the target language for the translation.
        /// </para>
        /// </summary>
        /// <param name="dict">Dictionary&lt;object, object&gt;: Dictionary containing parameters as (key,value) pairs</param>
        /// <returns>Dictionary&lt;string, object&gt;: Dictionary containing the results of the request. </returns>
        public Dictionary<string, object> NameTranslation(Dictionary<object, object> dict)
        {
            _uri = "name-translation/";
            return getResponse(SetupClient(), new JavaScriptSerializer().Serialize(dict));
        }

        /// deprecated
        /// <summary>TranslatedName
        /// <para>
        /// (POST)TranslatedName Endpoint: Returns the translation of a name. You must specify the name to translate and the target language for the translation.
        /// </para>
        /// </summary>
        /// <param name="dict">Dictionary&lt;object, object&gt;: Dictionary containing parameters as (key,value) pairs</param>
        /// <returns>Dictionary&lt;string, object&gt;: Dictionary containing the results of the request. </returns>
        public Dictionary<string, object> TranslatedName(Dictionary<object, object> dict)
        {
            return NameTranslation(dict);
        }

        /// <summary>NameTranslationInfo
        /// <para>
        /// (GET)NameTranslationInfo Endpoint: Response is a JSON string with request id, version, build info, and support (null for now).
        /// </para>
        /// </summary>
        /// <returns>Dictionary&lt;string, object&gt;: Dictionary containing the results of the info GET.</returns>
        public Dictionary<string, object> NameTranslationInfo()
        {
            _uri = "name-translation/info";
            return getResponse(SetupClient());
        }

        /// deprecated
        /// <summary>TranslatedNameInfo
        /// <para>
        /// (GET)TranslatedNameInfo Endpoint: Response is a JSON string with request id, version, build info, and support (null for now).
        /// </para>
        /// </summary>
        /// <returns>Dictionary&lt;string, object&gt;: Dictionary containing the results of the info GET.</returns>
        public Dictionary<string, object> TranslatedNameInfo()
        {
            return NameTranslationInfo();
        }

        /// <summary>getResponse
        /// <para>
        /// getResponse: Internal function to get the response from the Rosette API server using the request
        /// </para>
        /// </summary>
        /// <param name="client">HttpClient: Client to use to access the Rosette server.</param>
        /// <param name="jsonRequest">(string, optional): Content to use as the request to the server with POST. If none given, assume an Info endpoint and use GET</param>
        /// <returns>Dictionary&lt;string, object&gt;: Dictionary containing the results of the response from the server.</returns>
        private Dictionary<string, Object> getResponse(HttpClient client, string jsonRequest = null)
        {
            if (client != null && version_checked)
            {     
                HttpResponseMessage responseMsg = null;
                int retry = 0;
                string wholeURI = Debug ? _uri + "?debug=true" : _uri;
                if (wholeURI.StartsWith("/")) {
                    wholeURI = wholeURI.Substring(1);
                }

                while (responseMsg == null || (!responseMsg.IsSuccessStatusCode && retry <= MaxRetry))
                {
                    if (retry > 0)
                    {
                        System.Threading.Thread.Sleep(500);
                    }
                    if (jsonRequest != null)
                    {
                        HttpContent content = new StringContent(jsonRequest);
                        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                        responseMsg = client.PostAsync(wholeURI, content).Result;
                    }
                    else
                    {
                        responseMsg = client.GetAsync(wholeURI).Result;
                    }
                    retry = retry + 1;
                }
                List<string> message = new List<string>();
                Dictionary<string, object> dict = null;
                try
                {
                    message = getMessage(responseMsg);
                    dict = new JavaScriptSerializer().Deserialize<dynamic>(message[0]);
                    dict.Add("responseHeaders", new JavaScriptSerializer().Deserialize<Dictionary<string, string>>(message[1]));
                }
                catch (RosetteException e)
                {
                    throw e;
                }
                return dict;               

            }
            return null;
        }

        /// <summary>Process
        /// <para>
        /// Process: Internal function to convert a RosetteFile into a dictionary to use for getResponse
        /// </para>
        /// </summary>
        /// <param name="file">RosetteFile: File being uploaded to use as a request to the Rosette server.</param>
        /// <returns>Dictionary&lt;string, object&gt;: Dictionary containing the results of the response from the server from the getResponse call.</returns>
        private Dictionary<string, Object> Process(RosetteFile file)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>(){
                { "content", file.getFileDataString()},
                { "contentType", file.getDataType()},
                { "unit", "doc"},
            };

            if(file.getOptions() != null){
                Dictionary<string, string> opts = new JavaScriptSerializer().Deserialize<Dictionary<string, string>>(file.getOptions());
                dict = (Dictionary<string, string>)dict.Concat(opts.Where(x=> !dict.Keys.Contains(x.Key)));
            }


            return getResponse(SetupClient(), new JavaScriptSerializer().Serialize(dict));
        }

        /// <summary>Process
        /// <para>
        /// Process: Internal function to convert a RosetteFile into a dictionary to use for getResponse
        /// </para>
        /// </summary>
        /// <param name="content">(string, optional): Input to process (JSON string or base64 encoding of non-JSON string)</param>
        /// <param name="language">(string, optional): Language: ISO 639-3 code (ignored for the /language endpoint)</param>
        /// <param name="contentType">(string, optional): MIME type of the input (required for base64 content; if content type is unknown, set to "application/octet-stream")</param>
        /// <param name="unit">(string, optional): Input unit: "doc" (document, the default) or "sentence"</param>
        /// <param name="contentUri">(string, optional): URI to accessible content (content and contentUri are mutually exclusive)</param>
        /// <returns>Dictionary&lt;string, object&gt;: Dictionary containing the results of the response from the server from the getResponse call.</returns>
        private Dictionary<string, Object> Process(string content = null, string language = null, string contentType = null, string unit = null, string contentUri = null)
        {
            if (content == null){
                if(contentUri == null){
                    throw new RosetteException("Must supply one of Content or ContentUri", -3);
                } 
            }else{
                if (contentUri != null){
                    throw new RosetteException("Cannot supply both Content and ContentUri", -3);
                }
            }

            if (unit == null)
            {
                unit = "doc";
            }

            Dictionary<string, string> dict = new Dictionary<string, string>(){
                { "language", language},
                { "content", content},
                { "contentType", contentType},
                { "unit", unit},
                { "contentUri", contentUri}
            }.Where(f => !String.IsNullOrEmpty(f.Value)).ToDictionary(x => x.Key, x => x.Value);

            return getResponse(SetupClient(), new JavaScriptSerializer().Serialize(dict));
        }

        /// <summary>SetupClient
        /// <para>
        /// SetupClient: Internal function to setup the HttpClient
        /// Uses the Client if one has been set. Otherwise create a new one. 
        /// </para>
        /// </summary>
        /// <returns>HttpClient client to use to access the Rosette server.</returns>
        private HttpClient SetupClient()
        {
            HttpClient client;
            if (!URIstring.EndsWith("/")) {
                URIstring = URIstring + "/";
            }

            if (Client == null)
            {
                client =
                    new HttpClient(
                        new HttpClientHandler
                        {
                            AutomaticDecompression = DecompressionMethods.GZip
                                                     | DecompressionMethods.Deflate
                        });
                client.BaseAddress = new Uri(URIstring);
                client.Timeout = new TimeSpan(0, 0, Timeout);
            }
            else
            {
                client = Client;
                if (client.BaseAddress == null)
                {
                    client.BaseAddress = new Uri(URIstring);
                }
                if (client.Timeout == null)
                {
                    client.Timeout = new TimeSpan(0, 0, Timeout);
                }
            }
            try
            {
                client.DefaultRequestHeaders.Clear();
            }
            catch
            {
                // exception can be ignored
            }
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("user_key", UserKey);
            client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("gzip"));
            client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("deflate"));
            client.DefaultRequestHeaders.Add("User-Agent", "RosetteAPICsharp/" + binding_version);

            return client;
        }

        /// <summary>checkVersion
        /// <para>
        /// checkVersion: Internal function to check whether or not the version matches the server version
        /// </para>
        /// </summary>
        /// <param name="versionToCheck">(string, optional): Version to check against the server version</param>
        /// <returns>bool: Whether or not the versions match</returns>
        private bool checkVersion(string versionToCheck = null)
        {
            if (!version_checked || last_version_check.AddDays(1) < DateTime.Now)
            {
                if (versionToCheck == null)
                {
                    versionToCheck = Version;
                }
                HttpClient client = SetupClient();
                HttpResponseMessage responseMsg = null;
                int retry = 0;

                while (responseMsg == null || (!responseMsg.IsSuccessStatusCode && retry <= MaxRetry))
                {
                    if (retry > 0)
                    {
                        System.Threading.Thread.Sleep(500);
                    }
                    string url = string.Format("info?clientVersion={0}", versionToCheck);
                    HttpContent content = new StringContent(string.Empty);
                    responseMsg = client.PostAsync(url, content).Result;
                    retry = retry + 1;
                }
                string text = "";
                try
                {
                    text = getMessage(responseMsg)[0];
                }
                catch(RosetteException e)
                {
                    throw e;
                }
                var result = new JavaScriptSerializer().Deserialize<dynamic>(text);
                // compatibility with server side is at minor version level of semver
                string serverVersion = result["version"].ToString();
                if (!result["versionChecked"])
                {
                    throw new RosetteException("The server version is not compatible with binding version " + versionToCheck, -6);
                }
                else
                {
                    version_checked = true;
                    last_version_check = DateTime.Now;
                }
            }
            return version_checked;
        }

        /// <summary>getMessage
        /// <para>Helper function to parse out responseMsg based on gzip or not</para>
        /// </summary>
        /// <param name="responseMsg">(HttpResponseMessage): Response Message sent from the server</param>
        /// <returns>(string): Content of the response message</returns>
        private List<string> getMessage(HttpResponseMessage responseMsg)
        {
            if (responseMsg.IsSuccessStatusCode)
            {
                byte[] byteArray = responseMsg.Content.ReadAsByteArrayAsync().Result;
                IEnumerator<KeyValuePair<string, IEnumerable<string>>> responseHeadersEnum = responseMsg.Headers.GetEnumerator();
                Dictionary<string, string> responseHeadersDict = new Dictionary<string, string>();
                while(responseHeadersEnum.MoveNext())
                {
                    KeyValuePair<string, IEnumerable<string>> pair = responseHeadersEnum.Current;
                    responseHeadersDict.Add(pair.Key, pair.Value.ToArray()[0]);
                }

                if (responseMsg.Content.Headers.ContentEncoding.Contains("gzip") || (byteArray[0] == '\x1f' && byteArray[1] == '\x8b' && byteArray[2] == '\x08'))
                {
                    byteArray = Decompress(byteArray);
                }
                MemoryStream stream = new MemoryStream(byteArray);
                StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                List<string> message = new List<string>();
                message.Add(reader.ReadToEnd());
                message.Add(new JavaScriptSerializer().Serialize(responseHeadersDict));
                return message;
            }
            else
            {
                throw new RosetteException(responseMsg.ReasonPhrase, (int)responseMsg.StatusCode);
            }
        }

        /// <summary>Decompress
        /// <para>Method to decompress GZIP files
        /// Source: http://www.dotnetperls.com/decompress
        /// </para>
        /// </summary>
        /// <param name="gzip">(byte[]): Data in byte form to decompress</param>
        /// <returns>(byte[]) Decompressed data</returns>
        private static byte[] Decompress(byte[] gzip)
        {
            // Create a GZIP stream with decompression mode.
            // ... Then create a buffer and write into while reading from the GZIP stream.
            using (GZipStream stream = new GZipStream(new MemoryStream(gzip), CompressionMode.Decompress))
            {
                const int size = 4096;
                byte[] buffer = new byte[size];
                using (MemoryStream memory = new MemoryStream())
                {
                    int count = 0;
                    do
                    {
                        count = stream.Read(buffer, 0, size);
                        if (count > 0)
                        {
                            memory.Write(buffer, 0, count);
                        }
                    }
                    while (count > 0);
                    return memory.ToArray();
                }
            }
        }
    }



    /// <summary>RosetteException Class
    /// <para>
    /// RosetteException: Custom exception to describe an exception from the Rosette API.
    /// </para>
    /// </summary>
    [Serializable]
    public class RosetteException : Exception
    {
        /// <summary>RosetteException
        /// <para>
        /// RosetteException: Custom exception to describe an exception from the Rosette API.
        /// </para>
        /// </summary>
        /// <param name="message">(string, optional): Message describing exception details</param>
        /// <param name="code">(int, optional): Code number of the exception</param>
        /// <param name="requestid">(string, optional): RequestID if there is one</param>
        /// <param name="file">(string, optional): Filename if in file</param>
        /// <param name="line">(string, optional): Line if in file</param>
        public RosetteException(string message = null, int code = 0, string requestid = null, string file = null, string line = null) : base(message)
        {
            Code = code;
            RequestID = requestid;
            File = file;
            Line = line;
        }

        /// <summary>Code
        /// <para>
        /// Getter, Setter for the Code
        /// Code: Code number of the exception
        /// Allows users to access the Exception Code
        /// </para>
        /// </summary>
        public int Code { get; set; }

        /// <summary>RequestID
        /// <para>
        /// Getter, Setter for the RequestID
        /// RequestID: RequestID if there is one
        /// Allows users to access the Exception RequestID
        /// </para>
        /// </summary>
        public string RequestID { get; set; }

        /// <summary>File
        /// <para>
        /// Getter, Setter for the File
        /// File: Filename if in file
        /// Allows users to access the Exception File if in file
        /// </para>
        /// </summary>
        public string File { get; set; }

        /// <summary>Line
        /// <para>
        /// Getter, Setter for the Line
        /// Line: Line if in file
        /// Allows users to access the Exception Line if in file
        /// </para>
        /// </summary>
        public string Line { get; set; }
    }

    /// <summary>RosetteFile Class
    /// <para>
    /// RosetteFile: Custom Datatype containing information about files for upload, and methods to read the files
    /// </para>
    /// </summary>
    public class RosetteFile
    {

        /// <summary>
        /// Internal string name of the path to the data file
        /// </summary>
        private string _file;

        /// <summary>
        /// Internal string name of the dataType
        /// </summary>
        private string _dataType;

        /// <summary>
        /// Internal string name of the path to the options file
        /// </summary>
        private string _options;

        /// <summary>RosetteFile
        /// <para>
        /// RosetteFile: Custom Datatype containing information about files for upload, and methods to read the files
        /// </para>
        /// </summary>
        /// <param name="file">string: Path to the data file</param>
        /// <param name="dataType">(string, optional): Description of the datatype of the data file. "application/octet-stream" is used if unsure.</param>
        /// <param name="options">(string, optional): Json Options file to add extra information</param>
        public RosetteFile(string file, string dataType = "application/octet-stream", string options = null)
        {
            _file = file;
            _dataType = dataType;
            _options = options;
        }

        /// <summary>getFilename
        /// <para>
        /// getFilename: Get the filename
        /// </para>
        /// </summary>
        /// <returns>string: String of the filename</returns>
        public string getFilename()
        {
            return _file;
        }

        /// <summary>getDataType
        /// <para>
        /// getDataType: Get the datatype
        /// </para>
        /// </summary>
        /// <returns>string: String of the datatype</returns>
        public string getDataType()
        {
            return _dataType;
        }

        /// <summary>getFileData
        /// <para>
        /// getFileData: Get the FileData in byte form
        /// </para>
        /// </summary>
        /// <returns>byte[]: Byte Array of the file data</returns>
        public byte[] getFileData()
        {
            byte[] bytes = null;
            try
            {
                bytes = File.ReadAllBytes(_file);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }
            return bytes;
        }

        /// <summary>getFileDataString
        /// <para>
        /// getFileDataString: Get the FileData in string form
        /// </para>
        /// </summary>
        /// <returns>string: String of the file data</returns>
        public string getFileDataString(){
            try
            {
                using (StreamReader ff = File.OpenText(_file))
                {
                    return System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(ff.ReadToEnd()));
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }
            return null;
        }

        /// <summary>getOptions
        /// <para>
        /// getOptions: Get the options
        /// </para>
        /// </summary>
        /// <returns>string: String of the options</returns>
        public string getOptions(){
            try{
                using (StreamReader ff = File.OpenText(_options))
                {
                    return ff.ReadToEnd();
                }
            }catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }
            return null;
        }
    }

    /// <summary>Name Class
    /// <para>
    /// Name: Custom Datatype to use in Matched Name endpoint
    /// </para>
    /// </summary>
    public class Name
    {
        /// <summary>Name
        /// <para>
        /// Name: Custom Datatype to use in Matched Name endpoint
        /// </para>
        /// </summary>
        /// <param name="Text">(string, optional): Text describing the name</param>
        /// <param name="Language">(string, optional): Language: ISO 639-3 code (ignored for the /language endpoint)</param>
        /// <param name="Script">(string, optional): ISO 15924 code for the name's script</param>
        /// <param name="EntityType">(string, optional): Entity type of the name: PERSON, LOCATION, or ORGANIZATION</param>
        public Name(string Text = null, string Language = null, string Script = null, string EntityType = null)
        {
            text = Text;
            language = Language;
            script = Script;
            entityType = EntityType;
        }

        /// <summary>text
        /// <para>
        /// Getter, Setter for the text
        /// text: Text describing the name
        /// </para>
        /// </summary>
        public string text { get; set; }

        /// <summary>language
        /// <para>
        /// Getter, Setter for the language
        /// language: Language: ISO 639-3 code
        /// </para>
        /// </summary>
        public string language { get; set; }

        /// <summary>script
        /// <para>
        /// Getter, Setter for the script
        /// script: ISO 15924 code for the name's script
        /// </para>
        /// </summary>
        public string script { get; set; }

        /// <summary>entityType
        /// <para>
        /// Getter, Setter for the entityType
        /// entityType: Entity type of the name: PERSON, LOCATION, or ORGANIZATION
        /// </para>
        /// </summary>
        public string entityType { get; set; }
    }
}