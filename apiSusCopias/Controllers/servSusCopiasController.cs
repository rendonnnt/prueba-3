using System.Web.Http;
using System.Web.Http.Cors;
using apiSusCopias.Models;
using apiSusCopias.Clases;

namespace apiSusCopias.Controllers
{
    // Habilitación manual del componente CORS para el cliente que consume el servicio.
    // origins: URL y puerto donde corre el proyecto web (webSusCopias).
    [EnableCors(origins: "http://localhost:50200", headers: "*", methods: "*")]
    public class servSusCopiasController : ApiController
    {
        // POST: api/servSusCopias
        // Recibe el JSON de entrada en objIN y retorna el JSON con todos los datos del modelo.
        public modSusCopias Post([FromBody] modSusCopias objIN)
        {
            clsOpeSusCopias objOpe = new clsOpeSusCopias();
            return objOpe.Facturar(objIN);
        }
    }
}
