using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FireSharp.EventStreaming
{
    internal class FireCache : Dictionary<string, object>
    {
        private Object thisLock = new Object();

        #region Events
        public event ValueAddedEventHandler Added;
        public event ValueChangedEventHandler Changed;
        public event ValueRemovedEventHandler Removed;
        public event ValueMovedEventHandler Moved;
        #endregion

        // Clear all data (internal to dictionary)
        //public void Clear();

        public void Set(string path, JsonReader reader)
        {
            if (path == "/")
            {
                if (reader.Path != "data") throw new InvalidOperationException("Expecting to read object 'data' from Firebase stream.");
                PopulateCacheWithInitialValues(reader);
            }
            else
            {
                var value = GetValueFromJsonReader(path, reader);
                if (this.ContainsKey(path))
                {
                    this[path] = value;
                }
                else
                {
                    this.Add(path, value);
                }
            }

        }

        public void PopulateCacheWithInitialValues(JsonReader reader)
        {
            lock (thisLock)
            {
                string currentPropertyName = null;
                while (reader.Read())
                {
                    var pathSegments = reader.Path.Split(new[] { '.' });
                    if (reader.TokenType == JsonToken.PropertyName)
                    {
                        currentPropertyName = reader.Value.ToString();
                    }
                    else if (reader.TokenType == JsonToken.String
                        || reader.TokenType == JsonToken.Boolean
                        || reader.TokenType == JsonToken.Bytes
                        || reader.TokenType == JsonToken.Date
                        || reader.TokenType == JsonToken.Float
                        || reader.TokenType == JsonToken.Integer
                        || reader.TokenType == JsonToken.Null)
                    {
                        var value = reader.Value;
                        var path = PathBuilder(currentPropertyName, pathSegments);
                        this[path] = value;
                        currentPropertyName = null;
                    }
                    else
                    {
                        currentPropertyName = null;
                        //throw new NotImplementedException(string.Format("JSON reading of type {0} not implemented.", reader.TokenType));
                    }
                }

            }

        }

        private string PathBuilder(string propertyName, string[] jsonReaderPathSegments)
        {
            if (jsonReaderPathSegments == null || jsonReaderPathSegments.Length == 0) throw new InvalidOperationException("Expecting JsonReader path.");
            if (jsonReaderPathSegments[0] != "data") throw new InvalidOperationException("Expecting to first path segment 'data' from JsonReader.");
            if (propertyName != jsonReaderPathSegments[jsonReaderPathSegments.Length - 1])
            {
                throw new InvalidOperationException(string.Format("Expecting JsonReader path '{1}' to match property name '{0}'.", propertyName, string.Join("/", jsonReaderPathSegments)));
            }
            string result = null;
            bool skipSegment = true;
            foreach(var segment in jsonReaderPathSegments)
            {
                if (skipSegment)
                {
                    skipSegment = false;
                    continue; // Do not take 'data'
                }
                result = result + "/" + segment;
            }
            return result;
        }

        public object GetValueFromJsonReader(string path, JsonReader reader)
        {
            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.PropertyName)
                {
                    // Test against path passed in parameter
                    throw new NotImplementedException();
                }
                else if (reader.TokenType == JsonToken.String
                    || reader.TokenType == JsonToken.Boolean
                    || reader.TokenType == JsonToken.Bytes
                    || reader.TokenType == JsonToken.Date
                    || reader.TokenType == JsonToken.Float
                    || reader.TokenType == JsonToken.Integer
                    || reader.TokenType == JsonToken.Null)
                {
                    var value = reader.Value;
                    return value;
                }
                else throw new NotImplementedException(string.Format("JSON reading of type {0} not implemented.", reader.TokenType));
            }
            throw new JsonReaderException(string.Format("JSON value for property {0} not found.", path));

        }
    }
}
