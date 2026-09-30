namespace apiSusCopias.Models
{
    /// <summary>
    /// Clase modelo del servicio. Define la estructura del JSON de entrada (request)
    /// y del JSON de retorno (response) entre el cliente y la Web API.
    /// </summary>
    public class modSusCopias
    {
        // ---------------- Propiedades de entrada ----------------
        public float vrC { get; set; }        // Valor unitario de la copia tipo Carta
        public float vrO { get; set; }        // Valor unitario de la copia tipo Oficio
        public float vrE { get; set; }        // Valor unitario de la copia tipo Extra-Oficio
        public int kC { get; set; }           // Cantidad de copias tipo Carta
        public int kO { get; set; }           // Cantidad de copias tipo Oficio
        public int kE { get; set; }           // Cantidad de copias tipo Extra-Oficio

        // ---------------- Propiedades de salida -----------------
        public float vrTotC { get; set; }     // Valor a pagar por las copias tipo Carta
        public float vrTotO { get; set; }     // Valor a pagar por las copias tipo Oficio
        public float vrTotE { get; set; }     // Valor a pagar por las copias tipo Extra-Oficio
        public float vrSubTot { get; set; }   // Subtotal de la factura
        public float porcDscto { get; set; }  // Porcentaje de descuento otorgado (0, 10 o 15)
        public float vrDscto { get; set; }    // Valor del descuento
        public float vrIva { get; set; }      // Valor del impuesto IVA (7.5 %)
        public float vrAPag { get; set; }     // Valor total a pagar
        public string Error { get; set; }     // Mensaje de error ("" si no hay error)

        /// <summary>
        /// Constructor: inicializa en cero los valores numéricos y en vacío la cadena.
        /// </summary>
        public modSusCopias()
        {
            vrC = 0;
            vrO = 0;
            vrE = 0;
            kC = 0;
            kO = 0;
            kE = 0;

            vrTotC = 0;
            vrTotO = 0;
            vrTotE = 0;
            vrSubTot = 0;
            porcDscto = 0;
            vrDscto = 0;
            vrIva = 0;
            vrAPag = 0;
            Error = "";
        }
    }
}
