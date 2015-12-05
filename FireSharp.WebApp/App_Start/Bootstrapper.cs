using System.Web.Mvc;
using System.Web.Routing;
using System.Collections.Generic;
using Autofac;
using Autofac.Integration.Mvc;
using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.WebApp.App_Start;
using Newtonsoft.Json;
using System;

namespace FireSharp.WebApp
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
    public static class Bootstrapper
    {
        private const string BASE_PATH = "https://firesharp.firebaseio.com/";
        private const string FIREBASE_SECRET = "fubr9j2Kany9KU3SHCIHBLm142anWCzvlBs1D977";

        public static void Start()
        {
            AreaRegistration.RegisterAllAreas();

            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            var builder = new ContainerBuilder();

            // Register your MVC controllers.
            builder.RegisterControllers(typeof (MvcApplication).Assembly);

            builder.Register(context => new FirebaseConfig
            {
                AuthSecret = FIREBASE_SECRET,
                BasePath = BASE_PATH,
                Serializer = new Serializer(),
            }).As<IFirebaseConfig>().SingleInstance();

            builder.RegisterType<FirebaseClient>().As<IFirebaseClient>().SingleInstance();


            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
        }
    }
}