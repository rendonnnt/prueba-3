namespace apiPlantilla.Models
{
    /// <summary>
    /// CLASE MODELO: define el JSON de entrada (request) y el JSON de salida (response).
    /// Regla de oro: cada propiedad se llama IGUAL que la clave del JSON en el JavaScript
    /// (datosOut en ctlFrm...js) y que lo que se lee de la respuesta (Rpta.xxx).
    /// </summary>
    public class modPlantilla
    {
        // ================= PROPIEDADES DE ENTRADA =================
        // >>> CAMBIAR: una propiedad por cada dato que "se captura" en el enunciado.
        // Tipos: texto -> string | número con decimales -> float | número entero -> int
        public string nomCli { get; set; }    // Nombre del cliente
        public string tipoCli { get; set; }   // Tipo de cliente elegido en la lista: N, M o E
        public float vrUnit { get; set; }     // Valor unitario (se pide con prompt al iniciar)
        public int cant { get; set; }         // Cantidad comprada

        // ================= PROPIEDADES DE SALIDA ==================
        // >>> CAMBIAR: una propiedad por cada resultado que el enunciado pide "mostrar".
        public float vrSubTot { get; set; }   // Subtotal = vrUnit * cant
        public float porcDscto { get; set; }  // Porcentaje de descuento
        public float vrDscto { get; set; }    // Valor del descuento
        public float vrIva { get; set; }      // Valor del IVA
        public float vrTotal { get; set; }    // Total a pagar
        public string Error { get; set; }     // NO BORRAR: mensaje de error ("" si no hay error)

        /// <summary>
        /// Constructor: inicializa en cero los valores numéricos y en vacío las cadenas.
        /// >>> CAMBIAR: una línea por cada propiedad (las de entrada y las de salida).
        /// </summary>
        public modPlantilla()
        {
            nomCli = "";
            tipoCli = "";
            vrUnit = 0;
            cant = 0;

            vrSubTot = 0;
            porcDscto = 0;
            vrDscto = 0;
            vrIva = 0;
            vrTotal = 0;
            Error = "";
        }
    }
}
