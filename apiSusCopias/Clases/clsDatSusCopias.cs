using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using apiSusCopias.Models;

namespace apiSusCopias.Clases
{
    /// <summary>
    /// Clase de datos: conecta la Web API con la base de datos bdSusCopias (SQL Server)
    /// usando ADO.NET (System.Data.SqlClient, incluido en .NET Framework).
    /// </summary>
    public class clsDatSusCopias
    {
        // Nombre de la cadena de conexión definida en Web.config (sección connectionStrings)
        private const string NOMBRE_CNX = "cnxSusCopias";

        private readonly string cadenaCnx;

        public clsDatSusCopias()
        {
            ConnectionStringSettings cnxConfig = ConfigurationManager.ConnectionStrings[NOMBRE_CNX];
            if (cnxConfig == null)
            {
                throw new InvalidOperationException("No existe la cadena de conexión '" + NOMBRE_CNX + "' en Web.config");
            }
            cadenaCnx = cnxConfig.ConnectionString;
        }

        /// <summary>
        /// Guarda el cliente y la factura con el procedimiento almacenado spGuardarFactura.
        /// Retorna el número de la factura que generó la base de datos.
        /// </summary>
        public int GuardarFactura(modSusCopias objMod)
        {
            using (SqlConnection cnx = new SqlConnection(cadenaCnx))
            using (SqlCommand cmd = new SqlCommand("dbo.spGuardarFactura", cnx))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@docCli", SqlDbType.VarChar, 20).Value = objMod.docCli;
                cmd.Parameters.Add("@nomCli", SqlDbType.NVarChar, 100).Value = objMod.nomCli;
                agregarValor(cmd, "@vrC", objMod.vrC);
                agregarValor(cmd, "@vrO", objMod.vrO);
                agregarValor(cmd, "@vrE", objMod.vrE);
                cmd.Parameters.Add("@kC", SqlDbType.Int).Value = objMod.kC;
                cmd.Parameters.Add("@kO", SqlDbType.Int).Value = objMod.kO;
                cmd.Parameters.Add("@kE", SqlDbType.Int).Value = objMod.kE;
                agregarValor(cmd, "@vrTotC", objMod.vrTotC);
                agregarValor(cmd, "@vrTotO", objMod.vrTotO);
                agregarValor(cmd, "@vrTotE", objMod.vrTotE);
                agregarValor(cmd, "@vrSubTot", objMod.vrSubTot);
                agregarValor(cmd, "@porcDscto", objMod.porcDscto);
                agregarValor(cmd, "@vrDscto", objMod.vrDscto);
                agregarValor(cmd, "@vrIva", objMod.vrIva);
                agregarValor(cmd, "@vrAPag", objMod.vrAPag);

                cnx.Open();
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        // Agrega un parámetro DECIMAL redondeado a 2 decimales (los valores en el modelo son float)
        private static void agregarValor(SqlCommand cmd, string nombre, float valor)
        {
            SqlParameter par = cmd.Parameters.Add(nombre, SqlDbType.Decimal);
            par.Precision = 14;
            par.Scale = 2;
            par.Value = Math.Round((decimal)valor, 2, MidpointRounding.AwayFromZero);
        }
    }
}
