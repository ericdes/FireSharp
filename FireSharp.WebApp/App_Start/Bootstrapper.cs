using System.Web.Mvc;
using System.Web.Routing;
using System.Collections.Generic;
using Autofac;
using Autofac.Integration.Mvc;
using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.WebApp.App_Start;
using Newtonsoft.Json;

namespace FireSharp.WebApp
{
    public static class Bootstrapper
    {
        private const string BASE_PATH = "https://firesharp.firebaseio.com/";
        private const string FIREBASE_SECRET = "fubr9j2Kany9KU3SHCIHBLm142anWCzvlBs1D977";
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
                JsonSerializer = ToJson,
                JsonDeserializer = FromJson,
            }).As<IFirebaseConfig>().SingleInstance();

            builder.RegisterType<FirebaseClient>().As<IFirebaseClient>().SingleInstance();


            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
        }
    }
}