using System;
using System.Net.Http;
using System.Threading.Tasks;
using FireSharp.Config;

namespace FireSharp.Interfaces
{
    public interface IRequestManager : IDisposable
    {
        Task<HttpResponseMessage> ListenAsync(string path);
        Task<HttpResponseMessage> RequestAsync(HttpMethod method, string path, object payload = null);
        Task<HttpResponseMessage> RequestAsync<T>(HttpMethod method, string path, T payload);
    }
}