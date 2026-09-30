using System.Web.Http;

namespace apiSusCopias
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Configuración y servicios de Web API

            // Habilita el componente CORS (los orígenes permitidos se indican en el controlador)
            config.EnableCors();

            // Las respuestas del servicio se entregan siempre en formato JSON
            config.Formatters.Remove(config.Formatters.XmlFormatter);

            // Rutas de Web API
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
