using System.Web.Http;
using System.Web.Http.Cors;
using apiPlantilla.Models;
using apiPlantilla.Clases;

namespace apiPlantilla.Controllers
{
    // Habilitación manual de CORS (antes de la clase).
    // >>> CAMBIAR origins si el proyecto WEB corre en otro puerto (sin "/" al final).
    [EnableCors(origins: "http://localhost:50400", headers: "*", methods: "*")]
    public class servPlantillaController : ApiController
    {
        // POST: api/servPlantilla
        // Recibe el JSON de entrada en objIN y retorna el JSON con todos los datos del modelo.
        // Casi nunca cambia: solo los nombres de las clases y del método de la clase de operaciones.
        public modPlantilla Post([FromBody] modPlantilla objIN)
        {
            clsOpePlantilla objOpe = new clsOpePlantilla();
            return objOpe.Calcular(objIN);
        }
    }
}
