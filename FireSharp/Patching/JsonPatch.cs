using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FireSharp.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;

namespace FireSharp
{
    public enum JsonPatchOperation
    {
        [JsonProperty("add")]
        Add,
        [JsonProperty("replace")]
        Replace,
        [JsonProperty("remove")]
        Remove,
    }

    public class JsonPatch : IJsonPatch
    {
        private static ISerializer _serializer;

        [Flags]
        private enum SetMethod
        {
            None = 0,
            Value = 1 << 0,
            Token = 1 << 1,
            Json = 1 << 2,
        }

        private SetMethod _setMethod;


        private JToken _dataToken;
        /// <summary>
        /// Value as JToken.
        /// </summary>         
        [JsonIgnore]
        public JToken DataToken
        {
            get
            {
                if (_setMethod.HasFlag(SetMethod.Token)) return _dataToken;
                if (this.Data != null)
                {
                    _dataToken = JToken.Parse(this.Data);
                }
                else return null;
                _setMethod = _setMethod | SetMethod.Token;
                return _dataToken;
            }
            set
            {
                _dataToken = value;
                _setMethod = _setMethod | SetMethod.Token;
            }
        }

        private string _data;
        /// <summary>
        /// JSON formatted value.
        /// </summary>         
        [JsonIgnore]
        public string Data
        {
            get
            {
                if (_setMethod.HasFlag(SetMethod.Json)) return _data;
                if (_setMethod.HasFlag(SetMethod.Token))
                {
                    _data = _dataToken.ToString(Formatting.None);
                }
                else if (_setMethod.HasFlag(SetMethod.Value))
                {
                    if (_serializer == null)
                    {
                        throw new ArgumentException("Static serializer in JsonPatch has not been set.");
                    }
                    _data = _serializer.Serialize(_value);
                }
                else return null;
                _setMethod = _setMethod | SetMethod.Json;
                return _data;
            }
            set
            {
                _data = value;
                _setMethod = _setMethod | SetMethod.Json;
            }
        }

        [JsonIgnore]
        public JsonPatchOperation Operation { get; private set; }

        #region --- op / path / data fields => JSON patch
        [JsonProperty("op")]
        public string Op
        {
            get
            {
                string op;
                if (this.Operation == JsonPatchOperation.Add)
                {
                    op = "add";
                }
                else if (this.Operation == JsonPatchOperation.Replace)
                {
                    op = "replace";
                }
                else if (this.Operation == JsonPatchOperation.Remove)
                {
                    op = "remove";
                }
                else throw new NotImplementedException();

                return op;
            }
        }

        private string _path;
        [JsonProperty("path")]
        public string Path
        {
            get { return _path; }
            private set
            {
                _path = FormatPath(value);
            }
        }


        private object _value;
        /// <summary>
        /// Value.
        /// </summary>         
        [JsonProperty("data")]
        public object Value
        {
            // No public getter necessary, or we'd have to go with _serializer.
            private get
            {
                // Only for the purpose debugging with nice indentified JSON returned by ToString().
                if (_setMethod.HasFlag(SetMethod.Value)) return _value;
                _value = JsonConvert.DeserializeObject(this.Data);
                _setMethod = _setMethod | SetMethod.Value;
                return _value;
            }
            set
            {
                _value = value;
                _setMethod = _setMethod | SetMethod.Value;
            }
        }
        #endregion

        /// <summary>
        /// Better use JsonPatchManager for convenience.
        /// </summary>
        internal JsonPatch(string operation, string path)
        {
            JsonPatchOperation op;
            if (!Enum.TryParse<JsonPatchOperation>(operation, true, out op))
            {
                throw new InvalidCastException(string.Format("Unrecognized JSON patch operation '{0}'.", operation));
            }
            this.Operation = op;
            this.Path = path;
            _setMethod = SetMethod.None;
        }

        /// <summary>
        /// Call this constructor first to set static serializer
        /// </summary>
        /// <param name="serializer"></param>
        internal JsonPatch(ISerializer serializer)
        {
            _serializer = serializer;
        }
    

        /// <summary>
        /// Better use JsonPatchManager for convenience.
        /// </summary>
        internal JsonPatch(JsonPatchOperation operation, string path)
        {
            this.Operation = operation;
            this.Path = path;
            _setMethod = SetMethod.None;
            if (operation == JsonPatchOperation.Remove)
            {
                this.Data = "null";
            }
        }

        public static string FormatPath(string path)
        {
            if (path == null) return null;
            var clean = path.Trim(new[] { '/' });
            return "/" + clean;
        }


        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }


        #region Implementing IEquatable<T>
        public bool Equals(JsonPatch other)
        {
            if (other == null) return false;
            if (this.Path != other.Path) return false;
            if (this.Operation != other.Operation) return false;
            if (!JToken.DeepEquals(this.DataToken, other.DataToken)) return false;
            return true;
        }
        public override bool Equals(object o)
        {
            if (o == null) return false;
            if (o is JsonPatch) return Equals(o as JsonPatch);
            return base.Equals(o);
        }

        #endregion

        #region Implicit operators
        public static bool operator ==(JsonPatch x, JsonPatch y) { return (!object.ReferenceEquals(x, null)) ? x.Equals(y) : object.ReferenceEquals(y, null); }
        public static bool operator !=(JsonPatch x, JsonPatch y) { return (!object.ReferenceEquals(x, null)) ? !x.Equals(y) : !object.ReferenceEquals(y, null); }
        #endregion

    }

}
