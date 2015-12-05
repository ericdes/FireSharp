using System;
using System.Collections.Generic;
using FireSharp.Config;
using FireSharp.Response;
using Newtonsoft.Json;

namespace FireSharp.Test.Console
{
    public class Program
    {
        protected const string BasePath = "https://firesharp.firebaseio.com/";
        protected const string FirebaseSecret = "fubr9j2Kany9KU3SHCIHBLm142anWCzvlBs1D977";
        private static FirebaseClient _client;

        #region JSON serializing / deserializing methods
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
        private static string ToJson(object payload)
        {
            return JsonConvert.SerializeObject(payload, JSON_SETTINGS);
        }
        private static object FromJson(string json)
        {
            return JsonConvert.DeserializeObject(json, JSON_SETTINGS);
        }
        #endregion

        private static void Main()
        {
            IFirebaseConfig config = new FirebaseConfig
            {
                AuthSecret = FirebaseSecret,
                BasePath = BasePath,
            };

            _client = new FirebaseClient(config);

            EventStreaming();
            //Crud();

            System.Console.Read();
        }


        private static async void Crud()
        {
            var setResponse = await _client.SetAsync("todos", new { name = "SET CALL" });
            System.Console.WriteLine(setResponse.Body);
        }

        private static async void EventStreaming()
        {
            await _client.DeleteAsync("chat");

            await _client.OnAsync("chat",
                added: async (sender, args) =>
                {
                    System.Console.WriteLine(args.Data + "-> 1\n");
                    await _client.PushAsync("chat/", new
                    {
                        name = "someone",
                        text = "Console 1:" + DateTime.Now.ToString("f")
                    });
                },
                changed: (sender, args) => { System.Console.WriteLine(args.Data); },
                removed: (sender, args) => { System.Console.WriteLine(args.Path); });

            EventStreamResponse response = await _client.OnAsync("chat",
                added: (sender, args) => { System.Console.WriteLine(args.Data + " -> 2\n"); },
                changed: (sender, args) => { System.Console.WriteLine(args.Data); },
                removed: (sender, args) => { System.Console.WriteLine(args.Path); });

            //Call dispose to stop listening for events
            //response.Dispose();
        }
    }
}