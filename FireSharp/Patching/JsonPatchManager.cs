using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FireSharp.Interfaces;
using Newtonsoft.Json;
using FireSharp.Response;

namespace FireSharp
{
    public partial class JsonPatchManager : IJsonPatchManager
    {
        ISerializer _serializer;

        public JsonPatchManager(ISerializer serializer)
        {
            _serializer = serializer;
            var dummy = new JsonPatch(_serializer); // To set static serializer
        }

        public JsonPatch BuildJsonPatch<T>(T value, JsonPatchOperation operation, string path)
        {
            var jsonPatch = new JsonPatch(operation, path) { Value = value };
            return jsonPatch;
        }

        /// <param name="data">JSON-formatted data</param>
        public JsonPatch BuildJsonPatch(JsonPatchOperation operation, string data, string path)
        {
            var jsonPatch = new JsonPatch(operation, path) { Data = data };
            return jsonPatch;
        }

        /// <summary>
        /// Note: For us, 'replace' means 'delete' then 'add'.
        /// </summary>
        /// <param name="eventStreamingResponse"></param>
        /// <returns></returns>
        public JsonPatch BuildJsonPatch(IEventStreamingResponse eventStreamingResponse)
        {
            JsonPatch jsonPatch;
            if (eventStreamingResponse.EventName == "put")
            {
                var path = eventStreamingResponse.Data.Path.Trim(new[] { '/' }); // to avaoid empty paths segments
                var pathSegments = path.Split(new[] { '/' });
                var lastSegment = pathSegments[pathSegments.Length - 1];
                string jsonPatchOperation;
                if (lastSegment == "-")
                {
                    jsonPatchOperation = "add"; // Appending item to array
                }
                else if (lastSegment.All(char.IsDigit))
                {
                    jsonPatchOperation = "add"; // Adding item to specified array index
                }
                else
                {
                    jsonPatchOperation = "replace"; // put => replace (in general)
                }
                jsonPatch = new JsonPatch(jsonPatchOperation, eventStreamingResponse.Data.Path)
                {
                    Data = eventStreamingResponse.Data.Data
                };
            }
            else if (eventStreamingResponse.EventName == "patch")
            {
                var jsonPatchOperation = "add";
                jsonPatch = new JsonPatch(jsonPatchOperation, eventStreamingResponse.Data.Path)
                {
                    Data = eventStreamingResponse.Data.Data
                };
            }
            else
            {
                throw new NotImplementedException();
            }

            return jsonPatch;
        }


    }
}
