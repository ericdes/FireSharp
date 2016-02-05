using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FireSharp.Interfaces;
using Newtonsoft.Json;
using FireSharp.Response;
using System.Collections;
using System.Reflection;

namespace FireSharp
{
    public partial class JsonPatcher : IJsonPatchManager
    {
        ISerializer _serializer;

        public JsonPatcher(ISerializer serializer)
        {
            _serializer = serializer;
            var dummy = new JsonPatch(_serializer); // To set static serializer
        }

        #region ----- Constructors, builders
        public JsonPatch BuildJsonPatch<T>(T value, JsonPatchOperation operation, string path)
        {
            JsonPatch jsonPatch;
            if (value == null)
            {
                jsonPatch = new JsonPatch(operation, path) { Data = "null" };
            }
            else
            {
                jsonPatch = new JsonPatch(operation, path) { Value = value };
                //jsonPatch = new JsonPatch(operation, path) { Data = _serializer.Serialize<T>(value) };
            }
            return jsonPatch;
        }

        /// <param name="data">JSON-formatted data</param>
        public JsonPatch BuildJsonPatch(JsonPatchOperation operation, string data, string path)
        {
            var jsonPatch = new JsonPatch(operation, path) { Data = data };
            return jsonPatch;
        }
        #endregion

        /// <summary>
        /// Generate a JSON patch from a Firebase streaming response
        /// Note: For us, 'replace' means 'delete' then 'add'.
        /// </summary>
        /// <param name="eventStreamingResponse">Firebase streaming response</param>
        /// <returns></returns>
        public JsonPatch GeneratePatchFrom(IEventStreamingResponse eventStreamingResponse)
        {
            JsonPatch jsonPatch;
            if (eventStreamingResponse.EventName == "put")
            {
                JsonPatchOperation jsonPatchOperation;
                if (eventStreamingResponse.Data.Data == "null")
                {
                    jsonPatchOperation = JsonPatchOperation.Remove;
                }
                else
                {
                    jsonPatchOperation = getJsonOperationFromSetAction(eventStreamingResponse.Data.Path);
                }
                jsonPatch = new JsonPatch(jsonPatchOperation, eventStreamingResponse.Data.Path)
                {
                    Data = eventStreamingResponse.Data.Data
                };
            }
            else if (eventStreamingResponse.EventName == "patch")
            {
                var jsonPatchOperation = JsonPatchOperation.Add;
                jsonPatch = new JsonPatch(jsonPatchOperation, eventStreamingResponse.Data.Path)
                {
                    Data = eventStreamingResponse.Data.Data
                };
            }
            else
            {
                throw new NotImplementedException("Event " + eventStreamingResponse.EventName);
            }

            return jsonPatch;
        }


        public JsonPatch GeneratePatchFrom(object newValue, string valuePath)
        {
            JsonPatchOperation operation;
            if (newValue == null)
            {
                operation = JsonPatchOperation.Remove;
                return new JsonPatch(operation, valuePath);
            }
            if (typeof(IList).IsAssignableFrom(newValue.GetType()))
            {
                // Firebase does not take empty lists => request remove
                if (((IList)(newValue)).Count == 0)
                {
                    operation = JsonPatchOperation.Remove;
                    return new JsonPatch(operation, valuePath);
                }
            }
            var jsonPatchOperation = getJsonOperationFromSetAction(valuePath);
            var jsonPatch = new JsonPatch(jsonPatchOperation, valuePath)
            {
                Value = newValue
            };
            return jsonPatch;
        }

        private JsonPatchOperation getJsonOperationFromSetAction(string valuePath)
        {
            var path = valuePath.Trim(new[] { '/' }); // to avoid empty paths segments
            var pathSegments = path.Split(new[] { '/' });
            var lastSegment = pathSegments[pathSegments.Length - 1];
            JsonPatchOperation jsonPatchOperation;
            if (lastSegment == "")
            {
                jsonPatchOperation = JsonPatchOperation.Replace; // set / put => replace of root
            }
            else if (lastSegment == "-")
            {
                // IList by JSON patch standard
                jsonPatchOperation = JsonPatchOperation.Add; // Appending item to array
            }
            else if (lastSegment.All(char.IsDigit))
            {
                // IList by Firebase convention
                jsonPatchOperation = JsonPatchOperation.Replace; // Replacing item to specified array index
            }
            else
            {
                jsonPatchOperation = JsonPatchOperation.Replace; // set / put => replace (in general)
            }
            return jsonPatchOperation;
        }

    }
}
