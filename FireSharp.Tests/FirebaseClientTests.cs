using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FireSharp.Exceptions;
using FireSharp.Interfaces;
using FireSharp.Tests.Models;
using Moq;
using NUnit.Framework;
using Newtonsoft.Json;
using System.Collections.Generic;
using FireSharp.Config;

namespace FireSharp.Tests
{
    public class Serializer : ISerializer
    {
        private static JsonSerializerSettings JSON_SETTINGS = new JsonSerializerSettings
        {
            ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
            TypeNameHandling = TypeNameHandling.Auto,
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
            Converters = new List<JsonConverter>
            {
                new Newtonsoft.Json.Converters.StringEnumConverter(),
            },
        };
        public T Deserialize<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json, JSON_SETTINGS);
        }

        public string Serialize(object value)
        {
            return JsonConvert.SerializeObject(value, JSON_SETTINGS);
        }

        public string Serialize<T>(T value)
        {
            return JsonConvert.SerializeObject(value, typeof(T), JSON_SETTINGS);
        }
    }

    public class FirebaseClientTests : TestBase
    {
        private Todo _expected;
        private HttpResponseMessage _expectedResponse;
        private HttpResponseMessage _failureResponse;
        private FirebaseClient _firebaseClient;
        private Mock<IRequestManager> _firebaseRequestManagerMock;

        protected override void FinalizeSetUp()
        {
            IFirebaseConfig config = new FirebaseConfig
            {
                Serializer = new Serializer(),
            };

            _expected = new Todo
            {
                name = "Do your homework!",
                priority = 1
            };

            _firebaseRequestManagerMock = MockFor<IRequestManager>();
            _firebaseClient = new FirebaseClient(_firebaseRequestManagerMock.Object, config);

            _expectedResponse = new HttpResponseMessage
            {
                Content = new StringContent(_firebaseClient.Serializer.Serialize(_expected)),
                StatusCode = HttpStatusCode.OK
            };
            _failureResponse = new HttpResponseMessage {
                Content = new StringContent("error"),
                StatusCode = HttpStatusCode.InternalServerError
            };

        }

        [Test]
        public async void PushAsync()
        {
            _firebaseRequestManagerMock.Setup(
                firebaseRequestManager => firebaseRequestManager.RequestAsync(HttpMethod.Post, "todos", _expected))
                .Returns(Task.FromResult(_expectedResponse));

            var response = await _firebaseClient.PushAsync("todos", _expected);
            Assert.NotNull(response);
            Assert.AreEqual(response.Body, _firebaseClient.Serializer.Serialize(_expected));
        }

        [Test]
        public void PushAsyncFailure()
        {
            _firebaseRequestManagerMock.Setup(
                firebaseRequestManager => firebaseRequestManager.RequestAsync(HttpMethod.Post, "todos", _expected))
                .ReturnsAsync(_failureResponse);

            Assert.Throws<FirebaseException>(async () => await _firebaseClient.PushAsync("todos", _expected));
        }

        [Test]
        public async void SetAsync()
        {
            _firebaseRequestManagerMock.Setup(
                firebaseRequestManager => firebaseRequestManager.RequestAsync(HttpMethod.Put, "todos", _expected))
                .Returns(Task.FromResult(_expectedResponse));

            var response = await _firebaseClient.SetAsync("todos", _expected);
            Assert.NotNull(response);
            Assert.AreEqual(response.Body, _firebaseClient.Serializer.Serialize(_expected));
        }

        [Test]
        public void SetAsyncFailure()
        {
            _firebaseRequestManagerMock.Setup(
                firebaseRequestManager => firebaseRequestManager.RequestAsync(HttpMethod.Put, "todos", _expected))
                .ReturnsAsync(_failureResponse);

            Assert.Throws<FirebaseException>(async () => await _firebaseClient.SetAsync("todos", _expected));
        }

        [Test]
        public async Task GetAsync()
        {
            _firebaseRequestManagerMock.Setup(firebaseRequestManager => 
                firebaseRequestManager.RequestAsync(HttpMethod.Get, "todos", null))
                .Returns(Task.FromResult(_expectedResponse));

            var firebaseResponse = await _firebaseClient.GetAsync("todos");
            Assert.NotNull(firebaseResponse);
            Assert.AreEqual(firebaseResponse.Body, _firebaseClient.Serializer.Serialize(_expected));
        }

        [Test]
        public void GetAsyncFailure()
        {
            _firebaseRequestManagerMock.Setup(
                firebaseRequestManager => firebaseRequestManager.RequestAsync(HttpMethod.Get, "todos", null))
                .ReturnsAsync(_failureResponse);

            Assert.Throws<FirebaseException>(async () => await _firebaseClient.GetAsync("todos"));
        }

        [Test]
        public void GetFailure()
        {
            _firebaseRequestManagerMock.Setup(
                firebaseRequestManager => firebaseRequestManager.RequestAsync(HttpMethod.Get, "todos", null))
                .ReturnsAsync(_failureResponse);

            Assert.Throws<FirebaseException>(() => _firebaseClient.Get("todos"));
        }

        [Test]
        public async Task DeleteAsync()
        {
            _firebaseRequestManagerMock.Setup(firebaseRequestManager => 
                firebaseRequestManager.RequestAsync(HttpMethod.Delete, "todos", null))
                .Returns(Task.FromResult(_expectedResponse));

            var response = await _firebaseClient.DeleteAsync("todos");
            Assert.NotNull(response);
        }

        [Test]
        public void DeleteAsyncFailure()
        {
            _firebaseRequestManagerMock.Setup(
                firebaseRequestManager => firebaseRequestManager.RequestAsync(HttpMethod.Delete, "todos", null))
                .ReturnsAsync(_failureResponse);

            Assert.Throws<FirebaseException>(async () => await _firebaseClient.DeleteAsync("todos"));
        }

        [Test]
        public async Task UpdateAsync()
        {
            _firebaseRequestManagerMock.Setup(
                firebaseRequestManager => firebaseRequestManager.RequestAsync(RequestManager.Patch, "todos", _expected))
                .Returns(Task.FromResult(_expectedResponse));

            var response = await _firebaseClient.UpdateAsync("todos", _expected);
            Assert.NotNull(response);
            Assert.AreEqual(response.Body, _firebaseClient.Serializer.Serialize(_expected));
        }

        [Test]
        public void UpdateAsyncFailure()
        {
            _firebaseRequestManagerMock.Setup(
                firebaseRequestManager => firebaseRequestManager.RequestAsync(RequestManager.Patch, "todos", _expected))
                .ReturnsAsync(_failureResponse);

            Assert.Throws<FirebaseException>(async () => await _firebaseClient.UpdateAsync("todos", _expected));
        }
    }
}