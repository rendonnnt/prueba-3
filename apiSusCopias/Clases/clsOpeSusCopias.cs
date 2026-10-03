using System;
using apiSusCopias.Models;

namespace apiSusCopias.Clases
{
    /// <summary>
    /// Clase de operaciones del servicio: aplica las reglas de negocio de SusCopias SAS.
    /// </summary>
    public class clsOpeSusCopias
    {
        // Reglas de negocio (porcentajes)
        private const float PORC_IVA = 7.5f;          // IVA del servicio
        private const float PORC_DSCTO_50_100 = 10f;  // Entre 50 y 100 copias
        private const float PORC_DSCTO_MAS_100 = 15f; // Más de 100 copias

        // Propiedad: objeto modelo con los datos de entrada y de salida
        public modSusCopias pMod { get; set; }

        public clsOpeSusCopias()
        {
            pMod = new modSusCopias();
        }

        /// <summary>
        /// Método público: recibe los datos de entrada, halla el valor a pagar por cada
        /// tipo de copia y el subtotal, luego el descuento, el IVA y el total a pagar, y guarda
        /// la factura en la base de datos. Retorna el mismo objeto modelo con las salidas.
        /// </summary>
        public modSusCopias Facturar(modSusCopias objIN)
        {
            if (objIN == null)
            {
                pMod.Error = "No se recibieron los datos de la factura";
                return pMod;
            }

            pMod = objIN;
            pMod.Error = "";

            if (!validar())
            {
                return pMod;
            }

            // Valor a pagar por cada tipo de copia = valor unitario * cantidad
            pMod.vrTotC = pMod.vrC * pMod.kC;
            pMod.vrTotO = pMod.vrO * pMod.kO;
            pMod.vrTotE = pMod.vrE * pMod.kE;

            pMod.vrSubTot = pMod.vrTotC + pMod.vrTotO + pMod.vrTotE;

            hallarDescuento();

            // El IVA se aplica sobre el subtotal menos el descuento
            pMod.vrIva = (pMod.vrSubTot - pMod.vrDscto) * PORC_IVA / 100;
            pMod.vrAPag = pMod.vrSubTot - pMod.vrDscto + pMod.vrIva;

            // Guardar la factura en la base de datos (SQL Server) y obtener su número
            try
            {
                clsDatSusCopias objDat = new clsDatSusCopias();
                pMod.nroFact = objDat.GuardarFactura(pMod);
            }
            catch (Exception ex)
            {
                pMod.Error = "No se pudo guardar la factura en la base de datos: " + ex.Message;
            }

            return pMod;
        }

        /// <summary>
        /// Método privado: valida los datos de entrada. Retorna true si todos son válidos;
        /// si no, deja el mensaje en la propiedad Error y retorna false.
        /// </summary>
        private bool validar()
        {
            if (string.IsNullOrWhiteSpace(pMod.nomCli) || string.IsNullOrWhiteSpace(pMod.docCli))
            {
                pMod.Error = "Debe ingresar el nombre y el número de documento del cliente";
                return false;
            }

            pMod.nomCli = pMod.nomCli.Trim();
            pMod.docCli = pMod.docCli.Trim();

            if (pMod.nomCli.Length > 100 || pMod.docCli.Length > 20)
            {
                pMod.Error = "El nombre admite máximo 100 caracteres y el documento máximo 20";
                return false;
            }

            if (pMod.vrC <= 0 || pMod.vrO <= 0 || pMod.vrE <= 0)
            {
                pMod.Error = "Los valores unitarios de las copias deben ser mayores a cero";
                return false;
            }

            if (pMod.kC < 0 || pMod.kO < 0 || pMod.kE < 0)
            {
                pMod.Error = "Las cantidades de copias no pueden ser negativas";
                return false;
            }

            if (pMod.kC + pMod.kO + pMod.kE == 0)
            {
                pMod.Error = "Debe facturar al menos una copia";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Método privado: halla el porcentaje y el valor del descuento a partir
        /// de la cantidad total de copias del servicio.
        /// </summary>
        private void hallarDescuento()
        {
            int totCopias = pMod.kC + pMod.kO + pMod.kE;

            if (totCopias > 100)
            {
                pMod.porcDscto = PORC_DSCTO_MAS_100;
            }
            else if (totCopias >= 50)
            {
                pMod.porcDscto = PORC_DSCTO_50_100;
            }
            else
            {
                pMod.porcDscto = 0;
            }

            pMod.vrDscto = pMod.vrSubTot * pMod.porcDscto / 100;
        }
    }
}
