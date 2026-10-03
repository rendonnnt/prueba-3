/* =====================================================================
   Base de datos: bdSusCopias  (SQL Server)
   Proyecto:      apiSusCopias - SusCopias SAS
   Guarda los clientes y las facturas que procesa la Web API.

   Se puede ejecutar varias veces: solo crea lo que no existe y
   actualiza el procedimiento y la vista, sin borrar los datos.
   ===================================================================== */

IF DB_ID(N'bdSusCopias') IS NULL
    CREATE DATABASE bdSusCopias;
GO

USE bdSusCopias;
GO

-- Números de factura consecutivos: sin esto, SQL Server puede saltar
-- (por ejemplo de 3 a 1002) cuando el servidor se reinicia.
ALTER DATABASE SCOPED CONFIGURATION SET IDENTITY_CACHE = OFF;
GO

/* ---------------------------------------------------------------------
   Tabla de clientes
   --------------------------------------------------------------------- */
IF OBJECT_ID(N'dbo.tblCliente', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.tblCliente
    (
        docCli  VARCHAR(20)   NOT NULL,     -- Número de documento del cliente
        nomCli  NVARCHAR(100) NOT NULL,     -- Nombre del cliente
        CONSTRAINT PK_tblCliente PRIMARY KEY (docCli)
    );
END
GO

/* ---------------------------------------------------------------------
   Tabla de facturas (una fila por cada servicio facturado)
   Los nombres de las columnas son los mismos de la clase modSusCopias.
   --------------------------------------------------------------------- */
IF OBJECT_ID(N'dbo.tblFactura', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.tblFactura
    (
        nroFact    INT IDENTITY(1,1) NOT NULL,   -- Número de la factura (consecutivo)
        fecha      DATETIME      NOT NULL CONSTRAINT DF_tblFactura_fecha DEFAULT (GETDATE()),
        docCli     VARCHAR(20)   NOT NULL,       -- Cliente (FK a tblCliente)

        -- Entradas
        vrC        DECIMAL(12,2) NOT NULL,       -- Valor unitario copia Carta
        vrO        DECIMAL(12,2) NOT NULL,       -- Valor unitario copia Oficio
        vrE        DECIMAL(12,2) NOT NULL,       -- Valor unitario copia Extra-Oficio
        kC         INT           NOT NULL,       -- Cantidad copias Carta
        kO         INT           NOT NULL,       -- Cantidad copias Oficio
        kE         INT           NOT NULL,       -- Cantidad copias Extra-Oficio

        -- Salidas
        vrTotC     DECIMAL(14,2) NOT NULL,       -- Valor a pagar copias Carta
        vrTotO     DECIMAL(14,2) NOT NULL,       -- Valor a pagar copias Oficio
        vrTotE     DECIMAL(14,2) NOT NULL,       -- Valor a pagar copias Extra-Oficio
        vrSubTot   DECIMAL(14,2) NOT NULL,       -- Subtotal
        porcDscto  DECIMAL(5,2)  NOT NULL,       -- Porcentaje de descuento (0, 10 o 15)
        vrDscto    DECIMAL(14,2) NOT NULL,       -- Valor del descuento
        vrIva      DECIMAL(14,2) NOT NULL,       -- Valor del IVA (7.5 %)
        vrAPag     DECIMAL(14,2) NOT NULL,       -- Total a pagar

        CONSTRAINT PK_tblFactura PRIMARY KEY (nroFact),
        CONSTRAINT FK_tblFactura_tblCliente FOREIGN KEY (docCli) REFERENCES dbo.tblCliente (docCli),
        CONSTRAINT CK_tblFactura_valores CHECK (vrC > 0 AND vrO > 0 AND vrE > 0),
        CONSTRAINT CK_tblFactura_cantidades CHECK (kC >= 0 AND kO >= 0 AND kE >= 0 AND kC + kO + kE > 0)
    );
END
GO

/* ---------------------------------------------------------------------
   Procedimiento: guarda (o actualiza) el cliente e inserta la factura.
   Retorna el número de la factura generada (nroFact).
   Lo llama la clase clsDatSusCopias de la Web API.
   --------------------------------------------------------------------- */
CREATE OR ALTER PROCEDURE dbo.spGuardarFactura
    @docCli     VARCHAR(20),
    @nomCli     NVARCHAR(100),
    @vrC        DECIMAL(12,2),
    @vrO        DECIMAL(12,2),
    @vrE        DECIMAL(12,2),
    @kC         INT,
    @kO         INT,
    @kE         INT,
    @vrTotC     DECIMAL(14,2),
    @vrTotO     DECIMAL(14,2),
    @vrTotE     DECIMAL(14,2),
    @vrSubTot   DECIMAL(14,2),
    @porcDscto  DECIMAL(5,2),
    @vrDscto    DECIMAL(14,2),
    @vrIva      DECIMAL(14,2),
    @vrAPag     DECIMAL(14,2)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;   -- Si algo falla, se deshace toda la transacción

    BEGIN TRANSACTION;

    IF EXISTS (SELECT 1 FROM dbo.tblCliente WHERE docCli = @docCli)
        UPDATE dbo.tblCliente SET nomCli = @nomCli WHERE docCli = @docCli;
    ELSE
        INSERT INTO dbo.tblCliente (docCli, nomCli) VALUES (@docCli, @nomCli);

    INSERT INTO dbo.tblFactura
        (docCli, vrC, vrO, vrE, kC, kO, kE,
         vrTotC, vrTotO, vrTotE, vrSubTot, porcDscto, vrDscto, vrIva, vrAPag)
    VALUES
        (@docCli, @vrC, @vrO, @vrE, @kC, @kO, @kE,
         @vrTotC, @vrTotO, @vrTotE, @vrSubTot, @porcDscto, @vrDscto, @vrIva, @vrAPag);

    DECLARE @nroFact INT = CAST(SCOPE_IDENTITY() AS INT);

    COMMIT TRANSACTION;

    SELECT @nroFact AS nroFact;
END
GO

/* ---------------------------------------------------------------------
   Vista para consultar las facturas con los datos del cliente
   --------------------------------------------------------------------- */
CREATE OR ALTER VIEW dbo.vwFacturas
AS
SELECT f.nroFact, f.fecha, c.docCli, c.nomCli,
       f.vrC, f.vrO, f.vrE, f.kC, f.kO, f.kE,
       f.vrTotC, f.vrTotO, f.vrTotE, f.vrSubTot,
       f.porcDscto, f.vrDscto, f.vrIva, f.vrAPag
FROM dbo.tblFactura AS f
INNER JOIN dbo.tblCliente AS c ON c.docCli = f.docCli;
GO

/* Consulta para revisar las facturas guardadas:
   SELECT * FROM dbo.vwFacturas ORDER BY nroFact DESC;
*/
