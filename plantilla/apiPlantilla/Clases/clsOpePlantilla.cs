using System;
using apiPlantilla.Models;

namespace apiPlantilla.Clases
{
    /// <summary>
    /// CLASE DE OPERACIONES: aquí va TODA la lógica de cálculo del ejercicio.
    /// Ejemplo incluido: venta con descuento por tipo de cliente y por volumen, más IVA.
    /// </summary>
    public class clsOpePlantilla
    {
        // ========== PASO 1: CONSTANTES DEL ENUNCIADO ==========
        // >>> CAMBIAR: porcentajes, tarifas, topes y límites que da el enunciado.
        // (Se escriben una sola vez aquí y se usan por nombre en los cálculos.)
        private const float PORC_IVA = 19f;              // IVA del 19 %
        private const float PORC_DSCTO_MAYORISTA = 10f;  // Cliente tipo M
        private const float PORC_DSCTO_ESTUDIANTE = 5f;  // Cliente tipo E
        private const float PORC_DSCTO_VOLUMEN = 5f;     // Adicional si compra más de CANT_VOLUMEN
        private const int CANT_VOLUMEN = 100;

        // Propiedad: objeto modelo con los datos de entrada y de salida
        public modPlantilla pMod { get; set; }

        public clsOpePlantilla()
        {
            pMod = new modPlantilla();
        }

        /// <summary>
        /// Método público (lo llama el controlador): valida, calcula y retorna el modelo
        /// con las propiedades de salida llenas.
        /// >>> CAMBIAR el nombre si el profesor lo pide (Facturar, Liquidar...): clic derecho → Cambiar nombre.
        /// </summary>
        public modPlantilla Calcular(modPlantilla objIN)
        {
            if (objIN == null)
            {
                pMod.Error = "No se recibieron los datos (revise los nombres y tipos del JSON)";
                return pMod;
            }

            pMod = objIN;
            pMod.Error = "";

            if (!validar())
            {
                return pMod;
            }

            // ========== PASO 3: PROCESO (las fórmulas del enunciado, en orden) ==========
            // >>> CAMBIAR: reemplazar por las fórmulas del ejercicio.
            pMod.vrSubTot = pMod.vrUnit * pMod.cant;

            hallarDescuento();

            // El IVA se aplica sobre el subtotal menos el descuento
            pMod.vrIva = (pMod.vrSubTot - pMod.vrDscto) * PORC_IVA / 100;
            pMod.vrTotal = pMod.vrSubTot - pMod.vrDscto + pMod.vrIva;

            return pMod;
        }

        /// <summary>
        /// ========== PASO 2: VALIDACIONES ==========
        /// Retorna true si los datos son válidos; si no, deja el mensaje en Error y retorna false.
        /// >>> CAMBIAR: un "if" por cada regla de validación del enunciado.
        /// </summary>
        private bool validar()
        {
            if (string.IsNullOrWhiteSpace(pMod.nomCli))
            {
                pMod.Error = "Debe ingresar el nombre del cliente";
                return false;
            }
            pMod.nomCli = pMod.nomCli.Trim();

            if (pMod.tipoCli != "N" && pMod.tipoCli != "M" && pMod.tipoCli != "E")
            {
                pMod.Error = "El tipo de cliente debe ser N, M o E";
                return false;
            }

            if (pMod.vrUnit <= 0)
            {
                pMod.Error = "El valor unitario debe ser mayor a cero";
                return false;
            }

            if (pMod.cant <= 0)
            {
                pMod.Error = "La cantidad debe ser mayor a cero";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Método privado de apoyo: halla el porcentaje y el valor del descuento.
        /// Muestra los dos patrones más comunes: por categoría (switch) y por rango (if).
        /// >>> CAMBIAR: un método privado por cada "regla" del enunciado (hallarRecargo, hallarHoras...).
        /// </summary>
        private void hallarDescuento()
        {
            // Regla por categoría (tipo de cliente) -> switch
            switch (pMod.tipoCli)
            {
                case "M":
                    pMod.porcDscto = PORC_DSCTO_MAYORISTA;
                    break;
                case "E":
                    pMod.porcDscto = PORC_DSCTO_ESTUDIANTE;
                    break;
                default:
                    pMod.porcDscto = 0;
                    break;
            }

            // Regla por rango (cantidad) -> if
            if (pMod.cant > CANT_VOLUMEN)
            {
                pMod.porcDscto = pMod.porcDscto + PORC_DSCTO_VOLUMEN;
            }

            pMod.vrDscto = pMod.vrSubTot * pMod.porcDscto / 100;
        }
    }
}
