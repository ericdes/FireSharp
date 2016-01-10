using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FireSharp.Interfaces
{
    /// <summary>
    /// JSON Patch format following RFC6902 specifications.
    /// </summary>
    /// <see cref="https://tools.ietf.org/html/rfc6902"/>
    public interface IJsonPatch
    {
        /// <summary>
        /// JSON-formatted value
        /// </summary>
        string Data { get; }

        string Op { get; }

        string Path { get; }

    }

}
