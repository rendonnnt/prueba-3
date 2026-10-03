# Práctica 3 – API Web básica: "SusCopias SAS"

Solución de la guía de trabajo **FDE 048 – API Web Básica**. Es una aplicación cliente‑servidor formada por **dos proyectos independientes**. No hay archivo de solución (`.sln`) ni referencias entre ellos: cada uno se abre, compila y ejecuta por separado, y solo se comunican por HTTP enviando y recibiendo JSON.

| Proyecto | Tipo en Visual Studio | Función | URL (IIS Express) |
| --- | --- | --- | --- |
| `apiSusCopias` | Aplicación web ASP.NET (.NET Framework) – **Web API** | Servidor: recibe el JSON, aplica las reglas de negocio, guarda la factura en **SQL Server** y responde otro JSON | `http://localhost:50100/` |
| `webSusCopias` | Aplicación web ASP.NET (.NET Framework) – **Web Forms** | Cliente: formulario HTML + JavaScript (jQuery + `fetch`) | `http://localhost:50200/` |

> Se trabaja en **Visual Studio 2022/2026 (el IDE "morado")**, no en Visual Studio Code, porque las plantillas de *Aplicación web ASP.NET (.NET Framework)* y IIS Express solo existen allí.

```
apiSusCopias/                          ← Proyecto 1: Web API (Actividad 3)
├── apiSusCopias.csproj                ← se abre este archivo en Visual Studio
├── lib/                               ← referencias: DLL de Web API, CORS y Newtonsoft.Json (ya incluidas)
├── App_Start/WebApiConfig.cs          ← config.EnableCors()
├── BaseDatos/bdSusCopias.sql          ← script de la base de datos (SQL Server)
├── Clases/clsOpeSusCopias.cs          ← clase de operaciones
├── Clases/clsDatSusCopias.cs          ← clase de datos (conexión a SQL Server)
├── Controllers/servSusCopiasController.cs
├── Models/modSusCopias.cs             ← clase modelo (request / response)
└── Global.asax(.cs), Web.config      ← Web.config: cadena de conexión cnxSusCopias

webSusCopias/                          ← Proyecto 2: cliente web (Actividades 4 y 5)
├── webSusCopias.csproj                ← se abre este archivo en Visual Studio
├── Paginas/frmFact.html
├── Scripts/jquery-3.7.0.min.js
└── scripts_form/ctlFrmFact.js
```

**Problema:** calcular el pago de un servicio de copias en blanco y negro de tres tipos: Carta (C), Oficio (O) y Extra‑Oficio (E). Si la cantidad total de copias está **entre 50 y 100** se otorga **10 %** de descuento; si es **mayor a 100**, **15 %**. El **IVA es del 7.5 %**. No necesariamente se facturan todos los tipos de copia al mismo tiempo.

---

## Actividad 1. Lógica y algoritmo

### Entradas

| Dato | Variable | Tipo |
| --- | --- | --- |
| Valor unitario de la copia Carta, Oficio y Extra‑Oficio (se piden al iniciar y no se pueden modificar) | `vrC`, `vrO`, `vrE` | real |
| Nombre y número de documento del cliente | `nomCli`, `docCli` | cadena |
| Cantidad de copias Carta, Oficio y Extra‑Oficio | `kC`, `kO`, `kE` | entero |

### Proceso

1. Valor a pagar por tipo de copia: `vrTotC = vrC * kC`, `vrTotO = vrO * kO`, `vrTotE = vrE * kE`.
2. Subtotal: `vrSubTot = vrTotC + vrTotO + vrTotE`.
3. Total de copias: `totCopias = kC + kO + kE`.
4. Porcentaje de descuento: 15 si `totCopias > 100`; 10 si `50 <= totCopias <= 100`; 0 en otro caso.
5. Valor del descuento: `vrDscto = vrSubTot * porcDscto / 100`.
6. IVA sobre el valor ya descontado: `vrIva = (vrSubTot - vrDscto) * 7.5 / 100`.
7. Total a pagar: `vrAPag = vrSubTot - vrDscto + vrIva`.
8. Guardar el cliente y la factura en la base de datos, que asigna el número de factura `nroFact`.

### Salidas

`nroFact`, `vrTotC`, `vrTotO`, `vrTotE`, `vrSubTot`, `porcDscto`, `vrDscto`, `vrIva`, `vrAPag` (o un mensaje de error).

### Seudocódigo

```text
Algoritmo FacturarSusCopias
    Constante PORC_IVA ← 7.5

    // ---- Entradas (valores de las copias: se piden una sola vez) ----
    Repetir
        Leer vrC, vrO, vrE
    Hasta Que vrC > 0 Y vrO > 0 Y vrE > 0

    Leer nomCli, docCli
    Leer kC, kO, kE                              // una cantidad vacía se toma como 0
    Si kC < 0 O kO < 0 O kE < 0 O (kC + kO + kE) = 0 Entonces
        Escribir "Error: cantidades no válidas"
        Terminar
    FinSi

    // ---- Proceso ----
    vrTotC   ← vrC * kC
    vrTotO   ← vrO * kO
    vrTotE   ← vrE * kE
    vrSubTot ← vrTotC + vrTotO + vrTotE

    totCopias ← kC + kO + kE
    Si totCopias > 100 Entonces
        porcDscto ← 15
    SiNo Si totCopias >= 50 Entonces
        porcDscto ← 10
    SiNo
        porcDscto ← 0
    FinSi

    vrDscto ← vrSubTot * porcDscto / 100
    vrIva   ← (vrSubTot - vrDscto) * PORC_IVA / 100
    vrAPag  ← vrSubTot - vrDscto + vrIva

    // ---- Guardar en la base de datos ----
    Guardar cliente (docCli, nomCli) y factura → nroFact

    // ---- Salidas ----
    Escribir nroFact, vrTotC, vrTotO, vrTotE, vrSubTot, porcDscto, vrDscto, vrIva, vrAPag
FinAlgoritmo
```

### Prueba de escritorio

Valores de las copias: `vrC = 100`, `vrO = 200`, `vrE = 300`.

| Caso | kC | kO | kE | totCopias | vrTotC | vrTotO | vrTotE | vrSubTot | porcDscto | vrDscto | vrIva | vrAPag |
| --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| 1 (datos de la guía) | 400 | 300 | 100 | 800 | 40.000 | 60.000 | 30.000 | 130.000 | 15 % | 19.500 | 8.287,50 | **118.787,50** |
| 2 (límite inferior: 50) | 20 | 20 | 10 | 50 | 2.000 | 4.000 | 3.000 | 9.000 | 10 % | 900 | 607,50 | 8.707,50 |
| 3 (límite superior: 100) | 40 | 40 | 20 | 100 | 4.000 | 8.000 | 6.000 | 18.000 | 10 % | 1.800 | 1.215,00 | 17.415,00 |
| 4 (más de 100: 101) | 41 | 40 | 20 | 101 | 4.100 | 8.000 | 6.000 | 18.100 | 15 % | 2.715 | 1.153,88 | 16.538,88 |
| 5 (menos de 50, sin Extra‑Oficio) | 10 | 5 | 0 | 15 | 1.000 | 1.000 | 0 | 2.000 | 0 % | 0 | 150,00 | 2.150,00 |

Detalle del caso 1: 130.000 × 15 % = 19.500 → base 130.000 − 19.500 = 110.500 → IVA 110.500 × 7.5 % = 8.287,50 → total 110.500 + 8.287,50 = **118.787,50**, igual al resultado esperado en la guía.

---

## Actividad 3. Proyecto backend (Web API en ASP.NET)

### Paso a paso en Visual Studio

1. **Crear un proyecto** → *Aplicación web ASP.NET (.NET Framework)* en C# → nombre `apiSusCopias` → plantilla **API web** (o *Vacío* marcando *Web API*) → **desmarcar "Configurar para HTTPS"** → *Crear*.
2. En la carpeta **Models** agregar la clase `modSusCopias`.
3. Crear la carpeta **Clases** y agregar la clase `clsOpeSusCopias`.
4. Instalar CORS: *Herramientas → Administrador de paquetes NuGet → Consola del Administrador de paquetes* y ejecutar:
   ```powershell
   Install-Package Microsoft.AspNet.WebApi.Cors
   ```
   (En el proyecto de este repositorio ya está hecho: las DLL de CORS están en la carpeta `lib`.)
5. En `App_Start/WebApiConfig.cs`, dentro del método `Register`, agregar `config.EnableCors();`.
6. En **Controllers** → *Agregar → Controlador → Controlador de Web API 2 – en blanco* con nombre `servSusCopiasController`. No se quita la palabra `Controller`: Web API la usa para reconocer el servicio (la ruta queda `api/servSusCopias`).

### Referencias del proyecto `apiSusCopias`

Las referencias externas son DLL que vienen **incluidas en la carpeta `apiSusCopias/lib`**, y el `.csproj` las referencia directamente (`<HintPath>lib\...`). Al compilar se copian a `bin`. Por eso el proyecto compila apenas se abre, solo, sin solución, sin NuGet y sin internet.

¿Por qué no se usa NuGet? Visual Studio solo descarga paquetes NuGet cuando hay una solución (`.sln`) guardada. Al abrir el `.csproj` solo, mostraría el error *"No se guardó la solución. Guarde la solución antes de administrar paquetes NuGet"* y las referencias quedarían sin resolver.

| Referencia (DLL en `lib/`) | Paquete NuGet de origen | Para qué se usa |
| --- | --- | --- |
| `System.Web.Http` | Microsoft.AspNet.WebApi.Core 5.2.9 | `ApiController`, `HttpConfiguration`, rutas, `[FromBody]` |
| `System.Web.Http.WebHost` | Microsoft.AspNet.WebApi.WebHost 5.2.9 | Ejecutar la Web API en IIS Express (`GlobalConfiguration`) |
| `System.Net.Http.Formatting` | Microsoft.AspNet.WebApi.Client 5.2.9 | Convertir el JSON del request/response en objetos |
| `Newtonsoft.Json` | Newtonsoft.Json 13.0.3 | Motor JSON que usa Web API |
| `System.Web.Http.Cors` | **Microsoft.AspNet.WebApi.Cors 5.2.9** | `[EnableCors]` y `config.EnableCors()` |
| `System.Web.Cors` | Microsoft.AspNet.Cors 5.2.9 | Núcleo de CORS (dependencia del anterior) |

Además tiene las referencias estándar de .NET Framework 4.8: `System`, `System.Core`, `System.Web`, `System.Net.Http`, `System.Xml`, etc. En el código, el controlador las usa con:

```csharp
using System.Web.Http;          // ApiController
using System.Web.Http.Cors;     // EnableCors
using apiSusCopias.Models;      // modSusCopias
using apiSusCopias.Clases;      // clsOpeSusCopias
```

### Clase modelo: `Models/modSusCopias.cs`

Sirve a la vez como **request** y como **response**: Web API convierte el JSON que llega en un objeto `modSusCopias` y convierte el objeto que se retorna en JSON. Por eso los nombres de las propiedades deben coincidir con las claves del JSON que arma el cliente. El constructor deja los números en cero y las cadenas vacías; así, si falta algún dato en el JSON, la propiedad queda en 0.

Para la base de datos se agregaron a las propiedades de la guía: `nomCli` y `docCli` (entrada: nombre y documento del cliente) y `nroFact` (salida: número de la factura guardada).

Archivo: [`apiSusCopias/Models/modSusCopias.cs`](apiSusCopias/Models/modSusCopias.cs)

```csharp
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
        public string nomCli { get; set; }    // Nombre del cliente (se guarda en la base de datos)
        public string docCli { get; set; }    // Número de documento del cliente

        // ---------------- Propiedades de salida -----------------
        public float vrTotC { get; set; }     // Valor a pagar por las copias tipo Carta
        public float vrTotO { get; set; }     // Valor a pagar por las copias tipo Oficio
        public float vrTotE { get; set; }     // Valor a pagar por las copias tipo Extra-Oficio
        public float vrSubTot { get; set; }   // Subtotal de la factura
        public float porcDscto { get; set; }  // Porcentaje de descuento otorgado (0, 10 o 15)
        public float vrDscto { get; set; }    // Valor del descuento
        public float vrIva { get; set; }      // Valor del impuesto IVA (7.5 %)
        public float vrAPag { get; set; }     // Valor total a pagar
        public int nroFact { get; set; }      // Número de la factura guardada en la base de datos
        public string Error { get; set; }     // Mensaje de error ("" si no hay error)

        /// <summary>
        /// Constructor: inicializa en cero los valores numéricos y en vacío las cadenas.
        /// </summary>
        public modSusCopias()
        {
            vrC = 0;
            vrO = 0;
            vrE = 0;
            kC = 0;
            kO = 0;
            kE = 0;
            nomCli = "";
            docCli = "";

            vrTotC = 0;
            vrTotO = 0;
            vrTotE = 0;
            vrSubTot = 0;
            porcDscto = 0;
            vrDscto = 0;
            vrIva = 0;
            vrAPag = 0;
            nroFact = 0;
            Error = "";
        }
    }
}
```

### Clase de operaciones: `Clases/clsOpeSusCopias.cs`

Diagrama UML:

```text
┌─────────────────────────────────────────────────┐
│                 clsOpeSusCopias                 │
├─────────────────────────────────────────────────┤
│ + pMod : modSusCopias                           │
├─────────────────────────────────────────────────┤
│ + Facturar(objIN : modSusCopias) : modSusCopias │
│ - validar() : bool                              │
│ - hallarDescuento() : void                      │
└─────────────────────────────────────────────────┘
                    │ usa
                    ▼
┌───────────────────────────────────────────────┐
│                clsDatSusCopias                │
├───────────────────────────────────────────────┤
│ - cadenaCnx : string                          │
├───────────────────────────────────────────────┤
│ + GuardarFactura(objMod : modSusCopias) : int │
└───────────────────────────────────────────────┘
```

- **`Facturar(objIN)`** (público): recibe el modelo, lo valida, halla el valor de cada tipo de copia y el subtotal, llama a `hallarDescuento()`, halla el IVA y el total a pagar. Luego guarda la factura con `clsDatSusCopias` (ver [Base de datos](#base-de-datos-en-sql-server-bdsuscopias)) y **retorna el mismo modelo** con las salidas y el `nroFact`. Si la base de datos falla, deja el motivo en `Error`.
- **`validar()`** (privado): revisa que vengan el nombre y el documento del cliente, que los valores de las copias sean mayores a cero, que las cantidades no sean negativas y que haya al menos una copia. Si algo falla, deja el mensaje en `Error` y el cliente lo muestra.
- **`hallarDescuento()`** (privado): calcula el porcentaje (0, 10 o 15) según el total de copias, y luego el valor del descuento.
- Los porcentajes son constantes con nombre (`PORC_IVA`, etc.) para no repetir "números mágicos".

Archivo: [`apiSusCopias/Clases/clsOpeSusCopias.cs`](apiSusCopias/Clases/clsOpeSusCopias.cs)

```csharp
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
```

### Controlador: `Controllers/servSusCopiasController.cs`

- `[EnableCors]` va **antes de la clase** y habilita a mano el acceso del cliente. `origins` es la URL con el **puerto del proyecto web** (`webSusCopias`, aquí `50200`), sin `/` al final. `headers: "*"` y `methods: "*"` permiten cualquier encabezado y método. Hace falta porque la página y la API corren en puertos distintos, es decir, en *orígenes cruzados*, y el navegador bloquea esa petición si el servidor no la autoriza.
- Tiene un **único método `Post`**. Recibe `objIN` (el JSON ya convertido en `modSusCopias`), crea `clsOpeSusCopias`, llama a `Facturar` y retorna el modelo, que Web API serializa en JSON.

Archivo: [`apiSusCopias/Controllers/servSusCopiasController.cs`](apiSusCopias/Controllers/servSusCopiasController.cs)

```csharp
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
```

### Configuración: `App_Start/WebApiConfig.cs`

`config.EnableCors()` activa el componente CORS. Además se quita el formateador XML para que la respuesta sea siempre JSON.

Archivo: [`apiSusCopias/App_Start/WebApiConfig.cs`](apiSusCopias/App_Start/WebApiConfig.cs)

```csharp
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
```

> El `Web.config` del proyecto conserva `<remove name="OPTIONSVerbHandler" />`, que trae la plantilla de Web API. Es necesario porque, antes del `POST` con `Content-Type: application/json`, el navegador envía una petición **OPTIONS** (*preflight*) que debe atender Web API y no IIS.

### Estructura del JSON (request y response)

Request (lo que envía el cliente por `POST` a `http://localhost:50100/api/servSusCopias`):

```json
{ "nomCli": "Ana Pérez", "docCli": "1020304050",
  "vrC": 100, "vrO": 200, "vrE": 300, "kC": 400, "kO": 300, "kE": 100 }
```

Response (lo que retorna el servicio):

```json
{
  "vrC": 100.0, "vrO": 200.0, "vrE": 300.0, "kC": 400, "kO": 300, "kE": 100,
  "nomCli": "Ana Pérez", "docCli": "1020304050",
  "vrTotC": 40000.0, "vrTotO": 60000.0, "vrTotE": 30000.0,
  "vrSubTot": 130000.0, "porcDscto": 15.0, "vrDscto": 19500.0,
  "vrIva": 8287.5, "vrAPag": 118787.5, "nroFact": 1, "Error": ""
}
```

Si hay un error, por ejemplo `kC = kO = kE = 0`, las salidas quedan en 0 y `"Error": "Debe facturar al menos una copia"`.

---

## Actividades 4 y 5. Proyecto frontend y consumo de la API

### Paso a paso en Visual Studio

1. Por aparte, **Crear un proyecto** → *Aplicación web ASP.NET (.NET Framework)* en C# → nombre `webSusCopias` → plantilla **Web Forms** → **desmarcar "Configurar para HTTPS"** → *Crear*. La plantilla Web Forms ya trae `Scripts/jquery-3.7.0.min.js`. En este repositorio el proyecto es mínimo y solo tiene lo que usa la práctica.
2. Crear la carpeta **Paginas** y agregar la página HTML `frmFact.html` (Actividad 4).
3. Crear la carpeta **scripts_form** y agregar el archivo JavaScript `ctlFrmFact.js` (Actividad 5).
4. En `frmFact.html`, al final del `<body>`, enlazar primero `jquery-3.7.0.min.js` (arrastrándolo desde la carpeta `Scripts`) y después `ctlFrmFact.js`.

### Referencias del proyecto `webSusCopias`

- **jQuery 3.7.0:** el archivo `Scripts/jquery-3.7.0.min.js` viene incluido en el proyecto (no se necesita NuGet).
- **.NET Framework 4.8:** `System`, `System.Core`, `System.Web`, `System.Web.Extensions`, `System.Xml`, etc.
- **En la página:** `<script src="../Scripts/jquery-3.7.0.min.js">` y `<script src="../scripts_form/ctlFrmFact.js">`.
- **No referencia al proyecto `apiSusCopias`.** Lo consume como un servicio externo, por su URL (`var dir = "http://localhost:50100/api/servSusCopias"`).

### Actividad 4: formulario `Paginas/frmFact.html`

- Cinco tablas centradas al **80 %** del ancho (`width: 80%; margin: 0 auto`): encabezado, datos del cliente, detalle por tipo de copia (valor, cantidad y valor a pagar), totales y botones.
- La clase **`.txt-titulo`** usa la fuente `'Comic Sans MS'`, color `#3C2007` y texto centrado. `.txt-etiqueta` se usa para las etiquetas (alineadas a la derecha, en cursiva).
- Los valores de las copias y todas las salidas son `readonly`: el enunciado pide que los valores de las copias no se puedan modificar.
- Nombres de las cajas de texto: `txtNombre`, `txtNroDoc`, `txtVrCarta`/`txtVrOfic`/`txtVrExtOfic`, `txtCantCarta`/`txtCantOfic`/`txtCantExtOfic`, `txtSubTotCarta`/`txtSubTotOfic`/`txtSubTotExtOfic`, `txtNroFact`, `txtSubTot`, `txtPorcDscto`, `txtDscto`, `txtIva`, `txtAPagar`. Botones: `btnProcesar` y `btnLimpiar`.

Archivo: [`webSusCopias/Paginas/frmFact.html`](webSusCopias/Paginas/frmFact.html)

```html
<!DOCTYPE html>
<html lang="es">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>SusCopias SAS - Facturación</title>
    <style>
        body {
            background-color: #FDF6EC;
            margin: 20px 0;
        }

        .txt-titulo {
            font-family: 'Comic Sans MS', cursive;
            font-size: 20px;
            color: #3C2007;
            text-align: center;
            line-height: 1.5;
            font-weight: bold;
        }

        .txt-etiqueta {
            font-family: 'Comic Sans MS', cursive;
            font-size: 15px;
            color: #3C2007;
            text-align: right;
            font-style: italic;
            font-weight: bold;
        }

        .tabla {
            width: 80%;
            margin: 0 auto 15px auto;
            border: 2px solid #3C2007;
            border-radius: 8px;
            padding: 8px;
            background-color: #FFFFFF;
        }

        .tabla td {
            padding: 4px 8px;
        }

        .tabla input[type=text] {
            width: 95%;
            padding: 4px;
            font-size: 15px;
        }

        .tabla input[readonly] {
            background-color: #EFE6DA;
            text-align: right;
        }

        .centro {
            text-align: center;
        }

        .boton {
            font-family: 'Comic Sans MS', cursive;
            font-size: 16px;
            color: #FFFFFF;
            background-color: #3C2007;
            border: none;
            border-radius: 6px;
            padding: 8px 25px;
            margin: 0 10px;
            cursor: pointer;
        }
    </style>
</head>
<body>

    <!-- Encabezado -->
    <table class="tabla">
        <tr>
            <td class="txt-titulo">SusCopias SAS</td>
        </tr>
        <tr>
            <td class="txt-titulo">Facturación de copias en blanco y negro</td>
        </tr>
    </table>

    <!-- Datos del cliente -->
    <table class="tabla">
        <tr>
            <td colspan="4" class="txt-titulo">Datos del cliente</td>
        </tr>
        <tr>
            <td class="txt-etiqueta" style="width: 20%;">Nombre:</td>
            <td style="width: 35%;"><input type="text" id="txtNombre" maxlength="100" /></td>
            <td class="txt-etiqueta" style="width: 20%;">Nro. documento:</td>
            <td style="width: 25%;"><input type="text" id="txtNroDoc" maxlength="20" /></td>
        </tr>
    </table>

    <!-- Datos de entrada y valor a pagar por cada tipo de copia -->
    <table class="tabla">
        <tr>
            <td colspan="4" class="txt-titulo">Detalle del servicio</td>
        </tr>
        <tr>
            <td class="txt-titulo" style="width: 25%;">Tipo de copia</td>
            <td class="txt-titulo" style="width: 25%;">Valor copia</td>
            <td class="txt-titulo" style="width: 20%;">Cantidad</td>
            <td class="txt-titulo" style="width: 30%;">Valor a pagar</td>
        </tr>
        <tr>
            <td class="txt-etiqueta">Carta:</td>
            <td><input type="text" id="txtVrCarta" readonly /></td>
            <td><input type="text" id="txtCantCarta" inputmode="numeric" /></td>
            <td><input type="text" id="txtSubTotCarta" readonly /></td>
        </tr>
        <tr>
            <td class="txt-etiqueta">Oficio:</td>
            <td><input type="text" id="txtVrOfic" readonly /></td>
            <td><input type="text" id="txtCantOfic" inputmode="numeric" /></td>
            <td><input type="text" id="txtSubTotOfic" readonly /></td>
        </tr>
        <tr>
            <td class="txt-etiqueta">Extra-Oficio:</td>
            <td><input type="text" id="txtVrExtOfic" readonly /></td>
            <td><input type="text" id="txtCantExtOfic" inputmode="numeric" /></td>
            <td><input type="text" id="txtSubTotExtOfic" readonly /></td>
        </tr>
    </table>

    <!-- Totales de la factura (salida) -->
    <table class="tabla">
        <tr>
            <td colspan="2" class="txt-titulo">Totales de la factura</td>
        </tr>
        <tr>
            <td class="txt-etiqueta">Factura No.:</td>
            <td><input type="text" id="txtNroFact" readonly /></td>
        </tr>
        <tr>
            <td class="txt-etiqueta" style="width: 50%;">Subtotal:</td>
            <td style="width: 50%;"><input type="text" id="txtSubTot" readonly /></td>
        </tr>
        <tr>
            <td class="txt-etiqueta">Porcentaje descuento (%):</td>
            <td><input type="text" id="txtPorcDscto" readonly /></td>
        </tr>
        <tr>
            <td class="txt-etiqueta">Valor descuento:</td>
            <td><input type="text" id="txtDscto" readonly /></td>
        </tr>
        <tr>
            <td class="txt-etiqueta">IVA (7.5 %):</td>
            <td><input type="text" id="txtIva" readonly /></td>
        </tr>
        <tr>
            <td class="txt-etiqueta">Total a pagar:</td>
            <td><input type="text" id="txtAPagar" readonly /></td>
        </tr>
    </table>

    <!-- Botones -->
    <table class="tabla">
        <tr>
            <td class="centro">
                <button type="button" id="btnProcesar" class="boton">Procesar</button>
                <button type="button" id="btnLimpiar" class="boton">Limpiar</button>
            </td>
        </tr>
    </table>

    <!-- Scripts al final del body: primero jQuery y luego el script del formulario -->
    <script src="../Scripts/jquery-3.7.0.min.js"></script>
    <script src="../scripts_form/ctlFrmFact.js"></script>
</body>
</html>
```

### Actividad 5: integración `scripts_form/ctlFrmFact.js`

Flujo del script:

1. **`jQuery(function () { ... })`** se ejecuta cuando la página termina de cargar. Llama a `pedirValores()` y asocia el evento `click` de cada botón.
2. **`pedirValores()`** pide con `prompt()` el valor de cada tipo de copia. `validarVr()` revisa que el valor no sea `null` (Cancelar), no esté vacío, sea numérico y sea mayor a 0. Si alguno falla, se muestra el error y se vuelven a pedir los valores (`return pedirValores();`). Si todos son válidos, se muestran en las cajas de solo lectura.
3. **`Procesar()`** es `async` para poder usar `await`:
   - lee y valida los datos (nombre, documento y cantidades; una cantidad vacía cuenta como 0 y debe haber al menos una copia);
   - arma el objeto **`datosOut`** con las claves `nomCli, docCli, vrC, vrO, vrE, kC, kO, kE`, que coinciden con las propiedades del modelo;
   - lo envía con **`fetch(dir, { method: "POST", ... body: JSON.stringify(datosOut) })`**. `JSON.stringify` convierte el objeto JavaScript en texto JSON;
   - con `await response.json()` obtiene la respuesta en `Rpta`. Si `Rpta.Error` no está vacío, lo muestra; si no, muestra el número de factura (`Rpta.nroFact`) y asigna los totales a las cajas usando **`fnro()`**;
   - mientras espera la respuesta deshabilita el botón **Procesar**, para que un doble clic no guarde la misma factura dos veces.
4. **`fnro(vr)`** formatea con `toLocaleString('es-CO', { minimumFractionDigits: 2, maximumFractionDigits: 2 })`. Por ejemplo, `118787.5` → `118.787,50`.
5. **`limpiarTotal()` / `limpiarRpta()`** limpian el formulario o solo la respuesta. Al cambiar una cantidad también se limpia la respuesta anterior, para no mostrar totales que ya no corresponden.

Archivo: [`webSusCopias/scripts_form/ctlFrmFact.js`](webSusCopias/scripts_form/ctlFrmFact.js)

```javascript
// Dirección del servicio en la Web API (apiSusCopias).
// Si el proyecto de la API corre en otro puerto, cambiar 50100 por ese puerto.
var dir = "http://localhost:50100/api/servSusCopias";

// Al cargar la página: pedir los valores de las copias y asociar los eventos click
jQuery(function () {
    pedirValores();

    $("#btnLimpiar").on("click", function () {
        limpiarTotal();
    });

    $("#btnProcesar").on("click", function () {
        Procesar();
    });

    // Si cambian las cantidades, la respuesta anterior ya no es válida
    $("#txtCantCarta, #txtCantOfic, #txtCantExtOfic").on("input", function () {
        limpiarRpta();
    });
});

// Formatea un número como moneda con la configuración regional de Colombia y 2 decimales
function fnro(vr) {
    return vr.toLocaleString('es-CO', {
        minimumFractionDigits: 2,   // Mínimo de decimales
        maximumFractionDigits: 2    // Máximo de decimales
    });
}

// Valida el valor de una copia. Retorna true si el valor NO es válido
function validarVr(valor, txt) {
    if (valor == null || valor.trim() == "" || isNaN(valor) || parseFloat(valor) <= 0) {
        alert("Error, el valor de la copia tipo: " + txt + ", no es válido");
        return true;
    }
    return false;
}

// Valida una cantidad de copias (vacía = 0). Retorna true si la cantidad NO es válida
function validarCant(valor, txt) {
    if (isNaN(valor) || !Number.isInteger(Number(valor)) || Number(valor) < 0) {
        alert("Error, la cantidad de copias tipo: " + txt + ", no es válida");
        return true;
    }
    return false;
}

// Pide el valor de cada tipo de copia y lo muestra en su caja de texto (de solo lectura)
function pedirValores() {
    let vrCarta = prompt("Digite el valor de cada copia tipo Carta:", "");
    let vrOfici = prompt("Digite el valor de cada copia tipo Oficio:", "");
    let vrExtOf = prompt("Digite el valor de cada copia tipo Extra-Oficio:", "");

    // Si algún valor no es válido se vuelven a pedir los tres valores
    if (validarVr(vrCarta, "Carta") || validarVr(vrOfici, "Oficio") || validarVr(vrExtOf, "Extra-Oficio")) {
        return pedirValores();
    }
    else {
        $("#txtVrCarta").val(vrCarta.trim());
        $("#txtVrOfic").val(vrOfici.trim());
        $("#txtVrExtOfic").val(vrExtOf.trim());
        $("#txtNombre").trigger("focus");
    }
}

// Limpia los datos del cliente, las cantidades y la respuesta (no los valores de las copias)
function limpiarTotal() {
    $("#txtNombre").val("");
    $("#txtNroDoc").val("");
    $("#txtCantCarta").val("");
    $("#txtCantOfic").val("");
    $("#txtCantExtOfic").val("");
    limpiarRpta();
    $("#txtNombre").trigger("focus");
}

// Limpia las cajas de texto de salida (respuesta del servicio)
function limpiarRpta() {
    $("#txtNroFact").val("");
    $("#txtSubTotCarta").val("");
    $("#txtSubTotOfic").val("");
    $("#txtSubTotExtOfic").val("");
    $("#txtSubTot").val("");
    $("#txtPorcDscto").val("");
    $("#txtDscto").val("");
    $("#txtIva").val("");
    $("#txtAPagar").val("");
}

// Envía los datos al servicio y muestra la respuesta.
// Es asíncrona (async) para poder esperar (await) la respuesta de fetch
async function Procesar() {
    // Definir variables locales de entrada (let: variable local)
    let nombre = $("#txtNombre").val().trim();
    let nroDoc = $("#txtNroDoc").val().trim();
    let vrCa = $("#txtVrCarta").val();
    let vrOf = $("#txtVrOfic").val();
    let vrEx = $("#txtVrExtOfic").val();
    let kCa = $("#txtCantCarta").val().trim();
    let kOf = $("#txtCantOfic").val().trim();
    let kEx = $("#txtCantExtOfic").val().trim();

    if (nombre == "") {
        alert("Error, digite el nombre del cliente");
        $("#txtNombre").trigger("focus");
        return;
    }
    if (nroDoc == "") {
        alert("Error, digite el número de documento del cliente");
        $("#txtNroDoc").trigger("focus");
        return;
    }
    if (vrCa == undefined || isNaN(vrCa)) {
        alert("Error, valor tipo carta, no válido");
        return;
    }
    if (vrOf == undefined || isNaN(vrOf)) {
        alert("Error, valor tipo oficio, no válido");
        return;
    }
    if (vrEx == undefined || isNaN(vrEx)) {
        alert("Error, valor tipo extra-oficio, no válido");
        return;
    }
    if (validarCant(kCa, "Carta")) {
        $("#txtCantCarta").trigger("focus");
        return;
    }
    if (validarCant(kOf, "Oficio")) {
        $("#txtCantOfic").trigger("focus");
        return;
    }
    if (validarCant(kEx, "Extra-Oficio")) {
        $("#txtCantExtOfic").trigger("focus");
        return;
    }
    if (Number(kCa) + Number(kOf) + Number(kEx) == 0) {
        alert("Error, digite la cantidad de copias de al menos un tipo");
        $("#txtCantCarta").trigger("focus");
        return;
    }

    // Crear el objeto JSON con los datos de entrada.
    // Los nombres deben coincidir con los nombres de las propiedades del modelo
    const datosOut = {
        nomCli: nombre,
        docCli: nroDoc,
        vrC: parseFloat(vrCa),
        vrO: parseFloat(vrOf),
        vrE: parseFloat(vrEx),
        kC: Number(kCa),
        kO: Number(kOf),
        kE: Number(kEx)
    };

    // Se deshabilita el botón mientras responde el servicio, para no guardar la factura dos veces
    $("#btnProcesar").prop("disabled", true);

    try {
        // Invocar el servicio - Enviar la información y recuperar la respuesta.
        // fetch: conecta de forma asíncrona con la API por el método POST
        const response = await fetch(dir, {
            method: "POST",
            mode: "cors",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(datosOut)   // Convierte el objeto JavaScript en un texto con formato JSON
        });

        if (!response.ok) {
            alert("Error, el servicio respondió con el estado: " + response.status);
            return;
        }

        // Recuperar y leer la respuesta del servicio (en la constante Rpta queda el JSON de respuesta)
        const Rpta = await response.json();
        let error = Rpta.Error;
        if (error != undefined && error != "") {
            alert("Error, " + error);
            return;
        }

        $("#txtNroFact").val(Rpta.nroFact);
        $("#txtSubTotCarta").val(fnro(Rpta.vrTotC));
        $("#txtSubTotOfic").val(fnro(Rpta.vrTotO));
        $("#txtSubTotExtOfic").val(fnro(Rpta.vrTotE));
        $("#txtSubTot").val(fnro(Rpta.vrSubTot));
        $("#txtPorcDscto").val(fnro(Rpta.porcDscto));
        $("#txtDscto").val(fnro(Rpta.vrDscto));
        $("#txtIva").val(fnro(Rpta.vrIva));
        $("#txtAPagar").val(fnro(Rpta.vrAPag));
    }
    catch (e) {
        alert("Error, " + e);
    }
    finally {
        $("#btnProcesar").prop("disabled", false);
    }
}
```

---

## Base de datos en SQL Server (`bdSusCopias`)

La guía no define una base de datos. Se agregó una para que **cada factura procesada quede guardada** en SQL Server con los datos del cliente. Cada factura recibe un número consecutivo, que se muestra en el formulario en **Factura No.**

### Diseño

```text
┌──────────────────────────┐              ┌──────────────────────────────────┐
│        tblCliente        │              │            tblFactura            │
├──────────────────────────┤              ├──────────────────────────────────┤
│ PK docCli  VARCHAR(20)   │              │ PK nroFact   INT IDENTITY        │
│    nomCli  NVARCHAR(100) │ 1 ──────── N │    fecha     DATETIME            │
└──────────────────────────┘              │ FK docCli    VARCHAR(20)         │
                                          │    vrC, vrO, vrE   DECIMAL(12,2) │
                                          │    kC, kO, kE      INT           │
                                          │    vrTotC, vrTotO, vrTotE,       │
                                          │    vrSubTot, vrDscto,            │
                                          │    vrIva, vrAPag   DECIMAL(14,2) │
                                          │    porcDscto       DECIMAL(5,2)  │
                                          └──────────────────────────────────┘
```

| Objeto | Tipo | Para qué |
| --- | --- | --- |
| `tblCliente` | Tabla | Un registro por cliente (clave: número de documento). Si el cliente vuelve, se actualiza su nombre. |
| `tblFactura` | Tabla | Una fila por factura, con las entradas y salidas del cálculo. Los nombres de las columnas son los mismos de `modSusCopias`. |
| `spGuardarFactura` | Procedimiento almacenado | Guarda o actualiza el cliente e inserta la factura en una sola transacción. Retorna `nroFact`. |
| `vwFacturas` | Vista | Consulta las facturas con el nombre del cliente. |

Además, el script:
- tiene restricciones `CHECK` para que no entren valores en cero ni cantidades negativas, aunque alguien escriba directo en la tabla;
- desactiva `IDENTITY_CACHE` para que los números de factura no salten (por ejemplo, de 3 a 1002) cuando SQL Server se reinicia;
- se puede ejecutar varias veces: solo crea lo que falta y no borra datos.

### Recorrido de una factura

```text
frmFact.html ──JSON (nomCli, docCli, vrC…kE)──► servSusCopiasController.Post
                                                   │
                                                   ▼
                                     clsOpeSusCopias.Facturar   (calcula totales)
                                                   │
                                                   ▼
                                     clsDatSusCopias.GuardarFactura ──► spGuardarFactura (SQL Server)
                                                   │                          │
frmFact.html ◄──JSON (totales + nroFact)───────────┘◄──────── nroFact ────────┘
```

### Script: `BaseDatos/bdSusCopias.sql`

Archivo: [`apiSusCopias/BaseDatos/bdSusCopias.sql`](apiSusCopias/BaseDatos/bdSusCopias.sql)

```sql
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
```

### Conexión: `Web.config`

La API lee la cadena de conexión **`cnxSusCopias`** del `Web.config`:

```xml
<add name="cnxSusCopias"
     connectionString="Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=bdSusCopias;Integrated Security=True;Connect Timeout=30"
     providerName="System.Data.SqlClient" />
```

- **`Data Source`**: el servidor de SQL Server. Debe ser **el mismo donde se ejecutó el script**:

  | Si usas… | `Data Source=` |
  | --- | --- |
  | **LocalDB** (viene con Visual Studio, es el valor por defecto) | `(localdb)\MSSQLLocalDB` |
  | SQL Server **Express** | `.\SQLEXPRESS` |
  | SQL Server Developer / instancia predeterminada | `.` (o `localhost`) |
  | Otro nombre | El mismo que escribes en *Nombre del servidor* al conectarte en SSMS |

- **`Initial Catalog=bdSusCopias`**: la base de datos.
- **`Integrated Security=True`**: entra con tu usuario de Windows, sin usuario ni clave de SQL.

### Clase de datos: `Clases/clsDatSusCopias.cs`

Usa **ADO.NET** (`System.Data.SqlClient`), que ya viene en .NET Framework. No necesita NuGet, ni internet, ni solución. Pasos:
1. Lee la cadena de conexión del `Web.config` con `ConfigurationManager`.
2. Abre la conexión y llama al procedimiento `spGuardarFactura` con parámetros tipados. Los parámetros evitan la inyección SQL.
3. Los valores `float` del modelo se convierten a `decimal` redondeados a 2 decimales.
4. Retorna el número de factura con `ExecuteScalar()`.

`using` cierra la conexión aunque ocurra un error.

Archivo: [`apiSusCopias/Clases/clsDatSusCopias.cs`](apiSusCopias/Clases/clsDatSusCopias.cs)

```csharp
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
```

---

## Ejecutar la práctica (cada proyecto por aparte)

**Requisito:** Visual Studio 2022 o 2026 con la carga de trabajo **"Desarrollo de ASP.NET y web"**. Esa carga incluye .NET Framework 4.8, IIS Express y **SQL Server Express LocalDB**. No se necesita internet para compilar.

### 1. Descargar y extraer

1. En https://github.com/rendonnnt/prueba-3, haz clic en **Code → Download ZIP**.
2. Clic derecho sobre el `.zip` → **Propiedades** → si aparece, marca **Desbloquear** → **Aceptar**. Así Windows no bloquea las DLL de la carpeta `lib`.
3. Clic derecho → **Extraer todo…** → por ejemplo, en `C:\Practica3`. No abras el proyecto desde dentro del ZIP sin extraerlo.

### 2. Crear la base de datos (una sola vez)

**Opción A – LocalDB desde Visual Studio (recomendada, ya coincide con el `Web.config`):**

1. Abre `apiSusCopias\apiSusCopias.csproj` en Visual Studio → menú **Ver → Explorador de objetos de SQL Server**.
2. Expande **SQL Server** → clic derecho sobre **`(localdb)\MSSQLLocalDB`** → **Nueva consulta…**
   - Si no aparece, abre el *Visual Studio Installer* → **Modificar** → **Componentes individuales**, marca **SQL Server Express LocalDB** e instálalo.
3. En el Explorador de soluciones, abre `BaseDatos\bdSusCopias.sql`. Copia todo su contenido (Ctrl+A, Ctrl+C), pégalo en la ventana de la consulta y ejecútalo con el botón ▶ **Ejecutar** (Ctrl+Shift+E).
4. Debe terminar sin errores. En el Explorador de objetos de SQL Server, actualiza **Bases de datos**: aparece **bdSusCopias**, con las tablas `dbo.tblCliente` y `dbo.tblFactura`.

**Opción B – Tu SQL Server con SQL Server Management Studio (SSMS):**

1. Conéctate a tu servidor y anota el *Nombre del servidor*, por ejemplo `.\SQLEXPRESS`.
2. Abre `apiSusCopias\BaseDatos\bdSusCopias.sql` con **Archivo → Abrir → Archivo…** y ejecútalo con **F5**.
3. En `apiSusCopias\Web.config`, cambia `Data Source=(localdb)\MSSQLLocalDB` por el nombre de tu servidor (ver la tabla de [Conexión](#conexión-webconfig)).

### 3. API

1. Visual Studio → **Abrir un proyecto o una solución** → `apiSusCopias\apiSusCopias.csproj`.
2. Compilar con **Ctrl+Shift+B**. Debe terminar en *"1 correctos"*, sin errores.
3. Ejecutar con **Ctrl+F5** (*Iniciar sin depurar*) para que la API quede corriendo en `http://localhost:50100/`.
   - En la raíz, el navegador puede mostrar un error 403/404. Es normal: la API no tiene página; el servicio está en `/api/servSusCopias`.
   - Deja Visual Studio abierto.

### 4. Cliente (después)

1. Abre **otra ventana** de Visual Studio → **Abrir un proyecto o una solución** → `webSusCopias\webSusCopias.csproj`.
2. Compilar con **Ctrl+Shift+B**.
3. Clic derecho en `Paginas/frmFact.html` → **Ver en el explorador**.

### 5. Probar

1. Ingresa `100`, `200` y `300` como valores de las copias.
2. Llena los datos del cliente y las cantidades `400`, `300` y `100`.
3. Presiona **Procesar**. Debe aparecer: **Factura No. 1**, subtotal `130.000,00`, descuento `15,00` % = `19.500,00`, IVA `8.287,50` y total a pagar **`118.787,50`**.
4. Revisa que quedó guardada. En el Explorador de objetos de SQL Server (o en SSMS), haz clic derecho en la base de datos `bdSusCopias` → **Nueva consulta…** y ejecuta:
   ```sql
   SELECT * FROM dbo.vwFacturas ORDER BY nroFact DESC;
   ```

> Al cerrar, Visual Studio puede preguntar si desea guardar un archivo `.sln`. Puedes responder **No**: cada proyecto funciona sin solución.
>
> Si Visual Studio quedó con errores de una versión anterior de este repositorio, borra esa carpeta, descarga el ZIP de nuevo y repite los pasos.

### Si la factura no se guarda

Si al procesar sale *"Error, No se pudo guardar la factura en la base de datos: …"*, el texto que sigue indica la causa:

| El mensaje dice… | Causa y solución |
| --- | --- |
| *No se puede abrir la base de datos "bdSusCopias"* / *Cannot open database* | No se ejecutó el script en ese servidor. Haz el paso 2. |
| *Error relacionado con la red o específico de la instancia…* | El `Data Source` del `Web.config` no coincide con tu servidor, o el servicio de SQL Server está detenido. Con SQL Server Express, inicia el servicio **SQL Server (SQLEXPRESS)** desde `services.msc`. |
| *Error de inicio de sesión del usuario…* / *Login failed* | Tu usuario de Windows no tiene permiso en ese servidor. Usa LocalDB o un servidor donde seas administrador. |
| *Error en el nivel de transporte…* (justo después de reiniciar SQL Server) | La API tenía una conexión vieja. Presiona **Procesar** otra vez. |

Después de cambiar el `Web.config`, vuelve a ejecutar la API con **Ctrl+F5**.

### Los dos puertos deben coincidir

| Dónde | Qué puerto va | Valor en este repositorio |
| --- | --- | --- |
| `ctlFrmFact.js` → `var dir = "http://localhost:XXXXX/api/servSusCopias"` | Puerto de la **API** (`apiSusCopias`) | `50100` |
| `servSusCopiasController.cs` → `[EnableCors(origins: "http://localhost:XXXXX", ...)]` | Puerto del **proyecto web** (`webSusCopias`) | `50200` |

Si creas los proyectos tú mismo, Visual Studio asigna puertos aleatorios. Puedes verlos en *Propiedades del proyecto → Web → Dirección URL del proyecto* y reemplazarlos en esas dos líneas. Si el navegador muestra un error de CORS o `TypeError: Failed to fetch`, casi siempre es porque uno de estos puertos no coincide o porque la API no está corriendo.
