using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FireSharp.Interfaces
{
    public interface IJsonPatchManager
    {
        /// <summary>
        /// JSON patch method. The object in paramater gets patched against the JSON patch.
        /// </summary>
        /// <typeparam name="T">Type of target object to patch</typeparam>
        /// <param name="jsonPatch">JSON patch following RFC 6902 standard.</param>
        /// <see cref="https://tools.ietf.org/html/rfc6902"/>
        void Patch<T>(T target, JsonPatch patch);

        JsonPatch GeneratePatchFrom(IEventStreamingResponse eventStreamingResponse);
        JsonPatch GeneratePatchFrom(object newValue, string valuePath);
    }
}
