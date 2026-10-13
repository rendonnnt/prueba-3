# Guía para el parcial: plantilla API Web + formulario

Plantilla base con la misma estructura de la Práctica 3 (SusCopias), lista para adaptarla a **cualquier ejercicio de cálculo**:

- **`apiPlantilla`**: proyecto Web API con la clase modelo, la clase de operaciones, el controlador con CORS y un único `POST`.
- **`webPlantilla`**: formulario HTML con tablas al 80 % y `.txt-titulo`, más el script con jQuery, `fetch` (`async/await`), `prompt()` y `fnro()`.

Trae un ejemplo funcionando: una venta con descuento por tipo de cliente y por cantidad, más IVA. En el parcial **reemplazas ese ejemplo por el ejercicio del profesor**. Todo lo que se cambia está marcado en el código con **`>>> CAMBIAR`**.

---

## 1. Lo más importante en 30 segundos

1. **Toda la lógica de cálculo va en la clase de operaciones** (`Clases/clsOpe….cs`). Nunca en el JavaScript.
2. **El mismo nombre en todas partes.** Cada dato se llama igual en el modelo (C#), en el JSON que arma el JavaScript (`datosOut`) y en la respuesta (`Rpta.…`).
3. **El controlador casi nunca cambia.** Solo cambian los nombres de las clases y el puerto en CORS.
4. **El JavaScript solo hace cuatro cosas:** lee las cajas, valida que estén bien escritas, envía el JSON y muestra la respuesta.
5. **Primero el algoritmo y la prueba de escritorio** (Actividad 1). Así sabes qué resultados deben salir antes de programar.

---

## 2. Qué hay en la carpeta `plantilla`

```
plantilla/
├── crearProyecto.ps1                    ← copia la plantilla con el nombre del ejercicio
├── apiPlantilla/                        ← PROYECTO 1: Web API (servidor)
│   ├── apiPlantilla.csproj
│   ├── Models/modPlantilla.cs           ← (1) MODELO: entradas y salidas
│   ├── Clases/clsOpePlantilla.cs        ← (2) OPERACIONES: aquí va la LÓGICA
│   ├── Controllers/servPlantillaController.cs  ← (3) CONTROLADOR: POST + CORS
│   ├── App_Start/WebApiConfig.cs        ← config.EnableCors() (no se toca)
│   ├── Global.asax(.cs), Web.config     ← arranque y configuración (no se tocan)
│   └── lib/                             ← DLL de Web API y CORS (no se tocan)
└── webPlantilla/                        ← PROYECTO 2: página web (cliente)
    ├── webPlantilla.csproj
    ├── Paginas/frmPlantilla.html        ← (4) FORMULARIO: cajas de entrada y salida
    ├── scripts_form/ctlFrmPlantilla.js  ← (5) SCRIPT: leer, enviar y mostrar
    └── Scripts/jquery-3.7.0.min.js      ← jQuery (no se toca)
```

| # | Archivo | Para qué sirve | ¿Qué cambio en el parcial? |
| --- | --- | --- | --- |
| 1 | `modPlantilla.cs` | Define el JSON de entrada y de salida. Web API convierte el JSON que llega en este objeto y el objeto que se retorna en JSON. | Las propiedades de **entrada** (lo que "se captura") y de **salida** (lo que hay que "mostrar"), y el constructor. |
| 2 | `clsOpePlantilla.cs` | Hace los cálculos: constantes del enunciado, `validar()`, el método público (`Calcular`) y los métodos privados de cada regla. | **Casi todo**: es donde va el ejercicio. |
| 3 | `servPlantillaController.cs` | Recibe el `POST`, crea la clase de operaciones, llama al método y retorna el modelo. `[EnableCors]` permite que la página lo llame desde otro puerto. | Nada, salvo nombres o el puerto de `origins`. |
| 4 | `frmPlantilla.html` | La página: una tabla de entradas (`tblEntradas`), una de resultados (`tblSalidas`) y los botones. | Las filas `<tr>` de entradas y salidas, y los títulos. |
| 5 | `ctlFrmPlantilla.js` | Al cargar pide los valores iniciales con `prompt()`. Al dar **Procesar**: lee y valida → arma `datosOut` → `fetch` POST → muestra `Rpta`. | Las líneas marcadas: `pedirValores`, PASO 1 (leer), PASO 2 (JSON) y PASO 3 (mostrar). |

---

## 3. Cómo viaja un dato (y dónde se escribe cada nombre)

```text
 frmPlantilla.html          ctlFrmPlantilla.js                  modPlantilla.cs / clsOpePlantilla.cs
 ─────────────────          ──────────────────                  ─────────────────────────────────────
 <input id="txtCant">  ──►  let cant = leerEntero("#txtCant")
                            datosOut = { cant: cant }  ──POST──► public int cant  →  pMod.cant (se usa en el cálculo)
                                                                  pMod.vrTotal = …  (se calcula)
 <input id="txtTotal"> ◄──  $("#txtTotal").val(fnro(Rpta.vrTotal)) ◄──JSON── public float vrTotal
```

**Regla de oro:** `cant` en `datosOut` = `cant` en el modelo, y `Rpta.vrTotal` = `vrTotal` en el modelo.

- El servidor acepta mayúsculas o minúsculas en la entrada.
- En la **respuesta** el nombre sale **exactamente** como en C#: `Rpta.Error` lleva la E mayúscula.
- Si un nombre de `Rpta.…` no coincide, en la caja aparece **`¿undefined?`**.
- Si una clave de `datosOut` no coincide, ese dato llega al servidor como **0** o vacío y sale su error de validación.

Para **agregar un dato de entrada** se toca en 5 lugares:

| Dónde | Qué escribir (ejemplo: "minutos") |
| --- | --- |
| Modelo | `public int minutos { get; set; }` y en el constructor `minutos = 0;` |
| Operaciones | Usarlo: `pMod.minutos`. Validarlo en `validar()` si hace falta. |
| HTML (`tblEntradas`) | Una fila con `<input type="text" id="txtMinutos" />` |
| JS – PASO 1 | `let minutos = leerEntero("#txtMinutos", "los minutos", false); if (minutos === null) return;` |
| JS – PASO 2 | En `datosOut`: `minutos: minutos` |

Para **agregar un resultado (salida)** se toca en 4 lugares:

| Dónde | Qué escribir (ejemplo: "horas") |
| --- | --- |
| Modelo | `public int horas { get; set; }` y en el constructor `horas = 0;` |
| Operaciones | Calcularlo: `pMod.horas = …;` |
| HTML (`tblSalidas`) | Una fila con `<input type="text" id="txtHoras" readonly />` |
| JS – PASO 3 | `$("#txtHoras").val(Rpta.horas);`. Usa `fnro(...)` para dinero o decimales. |

No hay que tocar **Limpiar**, ni el borrado de resultados al cambiar un dato: funcionan solos para todas las filas de las dos tablas.

---

## 4. Paso a paso en el parcial

### 4.0 Antes del parcial (hazlo hoy)

1. Descarga el ZIP del repositorio y **desbloquéalo**: clic derecho → Propiedades → *Desbloquear*. Luego extráelo, por ejemplo en `C:\Parcial`.
2. Abre `plantilla\apiPlantilla\apiPlantilla.csproj` → **Ctrl+F5**. Abre en otra ventana de Visual Studio `plantilla\webPlantilla\webPlantilla.csproj` → clic derecho en `Paginas\frmPlantilla.html` → **Ver en el explorador**.
3. Prueba con estos datos:
   - valor unitario `1000`, tipo **Mayorista**, cantidad `150`;
   - debe dar: subtotal `150.000,00`, descuento `15,00` % = `22.500,00`, IVA `24.225,00`, total **`151.725,00`**.
4. Practica una vez el [ejemplo resuelto del parqueadero](#5-ejemplo-resuelto-parqueadero). Es exactamente lo que harás en el parcial.

> La plantilla usa los puertos **50300** (API) y **50400** (web). Son distintos de los de SusCopias (50100/50200), así que no chocan.

### 4.1 Leer el enunciado y hacer el algoritmo (Actividad 1)

Subraya el enunciado con tres colores:

| En el enunciado dice… | Es… | Va en… |
| --- | --- | --- |
| "se capturan…", "se ingresa…", "el usuario digita…" | **Entrada** | Modelo (entrada), HTML `tblEntradas`, JS PASO 1 y 2 |
| "pedir inicialmente el valor…" | Entrada con `prompt()` | `pedirValores()` + caja `readonly` |
| "mostrar…", "calcular y dar como salida…" | **Salida** | Modelo (salida), HTML `tblSalidas`, JS PASO 3 |
| porcentajes, tarifas, topes ("IVA 19 %", "máximo 10 horas") | **Constante** | `clsOpe…` → PASO 1: `private const …` |
| "si… entonces…", "entre … y …", "según el tipo…" | **Regla** | `clsOpe…` → un método privado (`hallar…`) con `if` o `switch` |
| fórmulas ("subtotal = cantidad × valor") | **Proceso** | `clsOpe…` → PASO 3, en orden |
| "no puede ser…", "debe ser mayor a…" | **Validación** | `validar()` en C#. El formato (vacío, número) lo valida el JS. |

Plantilla del algoritmo para llenar en papel:

```text
Algoritmo NombreDelEjercicio
    Constantes: PORC_IVA ← …, …
    Entradas:   Leer …, …, …
    Validar:    Si … Entonces Escribir "Error…" y Terminar
    Proceso:    resultado1 ← fórmula
                Si condición Entonces … SiNo … FinSi
                total ← …
    Salidas:    Escribir resultado1, …, total
FinAlgoritmo
```

**Prueba de escritorio:** una tabla con columnas = todas las variables y filas = casos. Prueba siempre:
- un caso normal;
- los **límites** de cada rango. Para "entre 50 y 100", prueba 49, 50, 100 y 101.

### 4.2 Crear el proyecto con el nombre del ejercicio

**Opción A: con el script (2 minutos).**
1. Abre la carpeta `plantilla` en el Explorador de archivos → clic derecho en un espacio vacío → **Abrir en Terminal**. En Windows 10: Shift + clic derecho → *Abrir la ventana de PowerShell aquí*.
2. Escribe, cambiando `Parqueadero` por el nombre del ejercicio (solo letras y números):
   ```powershell
   powershell -ExecutionPolicy Bypass -File .\crearProyecto.ps1 -Nombre Parqueadero
   ```
3. Se crean `apiParqueadero` y `webParqueadero`, con todo renombrado: `modParqueadero`, `clsOpeParqueadero`, `servParqueaderoController`, `frmParqueadero.html`, `ctlFrmParqueadero.js`, los namespaces y la dirección `api/servParqueadero`. Abre esos `.csproj`.

**Opción B: sin script.** Si el computador no deja ejecutarlo, usa la plantilla tal cual: funciona con sus nombres. Cambia solo los nombres que pida el profesor (ver la [sección 8](#8-nombres-que-pida-el-profesor)).

### 4.3 Modelo (`Models/mod….cs`)

1. Borra las propiedades del ejemplo: `nomCli`, `tipoCli`, `vrUnit`, `cant`, `vrSubTot`…
2. Escribe una propiedad por cada entrada y cada salida. Tipos:
   - **`string`**: textos y códigos de una lista (`"M"`, `"C"`);
   - **`int`**: cantidades, minutos, días;
   - **`float`**: dinero, porcentajes, notas;
   - **`bool`**: casillas de sí/no;
   - **`DateTime`**: fechas.
3. **No borres `Error`.**
4. En el constructor, inicializa cada propiedad: números en `0`, cadenas en `""`, bool en `false`, fechas en `DateTime.Today`.

### 4.4 Clase de operaciones (`Clases/clsOpe….cs`): aquí va la lógica

Orden de trabajo:
1. **PASO 1, constantes:** cada porcentaje o tarifa fija del enunciado, con nombre. Por ejemplo, `private const float PORC_IVA = 19f;`. Los decimales llevan `f`.
2. **PASO 2, `validar()`:** un `if` por cada regla de validación. Si algo falla: `pMod.Error = "mensaje"; return false;`.
3. **PASO 3, proceso en `Calcular`:** las fórmulas **en el mismo orden del algoritmo**. Cada regla con condiciones va en su propio método privado (`hallarDescuento()`, `hallarTarifa()`…) y se llama desde `Calcular`.
4. Siempre se trabaja sobre **`pMod`**: `pMod.entrada` para leer, `pMod.salida = …` para escribir.

Busca la fórmula que necesitas en el [catálogo de cálculos](#6-catálogo-de-cálculos-c).

### 4.5 Controlador (`Controllers/serv…Controller.cs`)

El método se llama `Post`, como en la guía; así Web API sabe que responde al método POST. No se toca, salvo en dos casos:
- **El profesor pide otro nombre para el método** (`Facturar` en vez de `Calcular`): renómbralo en la clase de operaciones con *Cambiar nombre* y el controlador se actualiza solo.
- **El proyecto web corre en otro puerto:** cambia `origins: "http://localhost:50400"`.

### 4.6 Formulario (`Paginas/frm….html`)

1. Cambia los dos títulos del encabezado.
2. En `tblEntradas`, deja **una fila `<tr>` por cada entrada**. Copia y pega una fila parecida y cambia la etiqueta y el `id`.
3. En `tblSalidas`, una fila por cada salida, **siempre con `readonly`**.
4. Prefijos de los `id`: `txt` (caja de texto), `cbo` (lista), `chk` (casilla), `rb` (opción), `dtp` (fecha), `btn` (botón).

Las filas listas para copiar están en el [catálogo de campos](#7-catálogo-de-campos-html--javascript).

### 4.7 Script (`scripts_form/ctlFrm….js`)

1. **`var dir`**: el puerto de la API (50300 en la plantilla). Si el script cambió el nombre, ya dice `api/servNombre`.
2. **`pedirValores()`**:
   - Si el enunciado pide valores iniciales, deja un `prompt` por cada uno y muéstralo en su caja `readonly`.
   - **Si no los pide, borra la llamada `pedirValores();`** (está al inicio del archivo).
3. **PASO 1:** una línea `leer…` por cada entrada, en el mismo orden del formulario.
4. **PASO 2:** `datosOut` con una clave por cada entrada, **con el mismo nombre del modelo**.
5. **PASO 3:** una línea por cada salida.

### 4.8 Ejecutar y probar

1. API: **Ctrl+Shift+B** (compilar) → **Ctrl+F5**. Deja Visual Studio abierto.
2. Web, en otra ventana de Visual Studio: clic derecho en el `.html` → **Ver en el explorador**.
3. Prueba los casos de tu prueba de escritorio. Los resultados deben ser los mismos.
4. Si sale un error, busca el mensaje en la [sección 11](#11-errores-comunes-y-solución).

---

## 5. Ejemplo resuelto: parqueadero

> **Enunciado de práctica.** El parqueadero ParkCentro cobra por **hora o fracción** según el tipo de vehículo: Moto $1.200, Carro $3.000 y Camioneta $4.500. Se captura la placa, el tipo de vehículo, los minutos de permanencia y si es cliente frecuente. Si permanece más de 10 horas, se cobran máximo 10 horas. El cliente frecuente tiene 20 % de descuento. El IVA es del 19 %. Mostrar las horas cobradas, el valor de la hora, el valor bruto, el porcentaje y el valor del descuento, el IVA y el total a pagar.

**Análisis:**
- **Entradas:** `placa` (string), `tipoVeh` (string: M/C/K), `minutos` (int), `frecuente` (bool). No se piden valores con `prompt` porque las tarifas son fijas.
- **Constantes:** las 3 tarifas, el tope de 10 horas, 20 % de descuento y 19 % de IVA.
- **Proceso:**
  1. `horas = techo(minutos / 60)`, con tope de 10;
  2. `vrHora` según el tipo (`switch`);
  3. `vrBruto = vrHora × horas`;
  4. descuento si es frecuente;
  5. IVA sobre el valor después del descuento;
  6. total.
- **Salidas:** `horas`, `vrHora`, `vrBruto`, `porcDscto`, `vrDscto`, `vrIva`, `vrTotal`.

**Prueba de escritorio** (estos valores salieron al ejecutarlo):

| Tipo | Minutos | Frecuente | horas | vrHora | vrBruto | % | vrDscto | vrIva | vrTotal |
| --- | ---: | :---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| Carro | 125 | Sí | 3 | 3.000 | 9.000 | 20 | 1.800 | 1.368 | **8.568** |
| Moto | 700 | No | 10 (tope) | 1.200 | 12.000 | 0 | 0 | 2.280 | **14.280** |
| Camioneta | 60 | No | 1 | 4.500 | 4.500 | 0 | 0 | 855 | **5.355** |

**Paso 1:** se crearon los proyectos con `.\crearProyecto.ps1 -Nombre Parqueadero`.

**Paso 2, modelo** (`apiParqueadero/Models/modParqueadero.cs`):

```csharp
namespace apiParqueadero.Models
{
    public class modParqueadero
    {
        // ================= PROPIEDADES DE ENTRADA =================
        public string placa { get; set; }     // Placa del vehículo
        public string tipoVeh { get; set; }   // M = Moto, C = Carro, K = Camioneta
        public int minutos { get; set; }      // Minutos que permaneció
        public bool frecuente { get; set; }   // Casilla "cliente frecuente" (true / false)

        // ================= PROPIEDADES DE SALIDA ==================
        public int horas { get; set; }        // Horas cobradas (hora o fracción, con tope)
        public float vrHora { get; set; }     // Tarifa por hora según el tipo
        public float vrBruto { get; set; }    // vrHora * horas
        public float porcDscto { get; set; }
        public float vrDscto { get; set; }
        public float vrIva { get; set; }
        public float vrTotal { get; set; }
        public string Error { get; set; }

        public modParqueadero()
        {
            placa = "";
            tipoVeh = "";
            minutos = 0;
            frecuente = false;

            horas = 0;
            vrHora = 0;
            vrBruto = 0;
            porcDscto = 0;
            vrDscto = 0;
            vrIva = 0;
            vrTotal = 0;
            Error = "";
        }
    }
}
```

**Paso 3, operaciones** (`apiParqueadero/Clases/clsOpeParqueadero.cs`):

```csharp
using System;
using apiParqueadero.Models;

namespace apiParqueadero.Clases
{
    public class clsOpeParqueadero
    {
        // ========== PASO 1: CONSTANTES DEL ENUNCIADO ==========
        private const float TARIFA_MOTO = 1200f;
        private const float TARIFA_CARRO = 3000f;
        private const float TARIFA_CAMIONETA = 4500f;
        private const int HORAS_TOPE = 10;               // Máximo se cobra el día completo
        private const float PORC_DSCTO_FRECUENTE = 20f;
        private const float PORC_IVA = 19f;

        public modParqueadero pMod { get; set; }

        public clsOpeParqueadero()
        {
            pMod = new modParqueadero();
        }

        public modParqueadero Calcular(modParqueadero objIN)
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

            // ========== PASO 3: PROCESO ==========
            // Hora o fracción: 61 minutos = 2 horas → se redondea hacia arriba
            pMod.horas = (int)Math.Ceiling(pMod.minutos / 60.0);

            // Tope: nunca se cobran más de HORAS_TOPE horas
            if (pMod.horas > HORAS_TOPE)
            {
                pMod.horas = HORAS_TOPE;
            }

            hallarTarifa();
            pMod.vrBruto = pMod.vrHora * pMod.horas;

            // Condición de sí/no (checkbox) → operador ternario
            pMod.porcDscto = pMod.frecuente ? PORC_DSCTO_FRECUENTE : 0;
            pMod.vrDscto = pMod.vrBruto * pMod.porcDscto / 100;

            pMod.vrIva = (pMod.vrBruto - pMod.vrDscto) * PORC_IVA / 100;
            pMod.vrTotal = pMod.vrBruto - pMod.vrDscto + pMod.vrIva;

            return pMod;
        }

        // ========== PASO 2: VALIDACIONES ==========
        private bool validar()
        {
            if (string.IsNullOrWhiteSpace(pMod.placa))
            {
                pMod.Error = "Debe ingresar la placa";
                return false;
            }
            pMod.placa = pMod.placa.Trim().ToUpper();

            if (pMod.tipoVeh != "M" && pMod.tipoVeh != "C" && pMod.tipoVeh != "K")
            {
                pMod.Error = "El tipo de vehículo debe ser M, C o K";
                return false;
            }

            if (pMod.minutos <= 0)
            {
                pMod.Error = "Los minutos deben ser mayores a cero";
                return false;
            }

            return true;
        }

        // Tarifa por categoría → switch
        private void hallarTarifa()
        {
            switch (pMod.tipoVeh)
            {
                case "M":
                    pMod.vrHora = TARIFA_MOTO;
                    break;
                case "C":
                    pMod.vrHora = TARIFA_CARRO;
                    break;
                default:
                    pMod.vrHora = TARIFA_CAMIONETA;
                    break;
            }
        }
    }
}
```

**Paso 4, controlador:** sin cambios.

**Paso 5, formulario** (`webParqueadero/Paginas/frmParqueadero.html`): se reemplazaron las filas de las dos tablas.

```html
    <table class="tabla" id="tblEntradas">
        <tr>
            <td colspan="2" class="txt-titulo">Datos de entrada</td>
        </tr>
        <tr>
            <td class="txt-etiqueta" style="width: 50%;">Placa:</td>
            <td style="width: 50%;"><input type="text" id="txtPlaca" maxlength="10" /></td>
        </tr>
        <tr>
            <td class="txt-etiqueta">Tipo de vehículo:</td>
            <td>
                <select id="cboTipoVeh">
                    <option value="">-- Seleccione --</option>
                    <option value="M">Moto</option>
                    <option value="C">Carro</option>
                    <option value="K">Camioneta</option>
                </select>
            </td>
        </tr>
        <tr>
            <td class="txt-etiqueta">Minutos de permanencia:</td>
            <td><input type="text" id="txtMinutos" inputmode="numeric" /></td>
        </tr>
        <tr>
            <td class="txt-etiqueta">Cliente frecuente:</td>
            <td><input type="checkbox" id="chkFrec" /></td>
        </tr>
    </table>

    <table class="tabla" id="tblSalidas">
        <tr>
            <td colspan="2" class="txt-titulo">Resultados</td>
        </tr>
        <tr>
            <td class="txt-etiqueta" style="width: 50%;">Horas cobradas:</td>
            <td style="width: 50%;"><input type="text" id="txtHoras" readonly /></td>
        </tr>
        <tr>
            <td class="txt-etiqueta">Valor hora:</td>
            <td><input type="text" id="txtVrHora" readonly /></td>
        </tr>
        <tr>
            <td class="txt-etiqueta">Valor bruto:</td>
            <td><input type="text" id="txtBruto" readonly /></td>
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
            <td class="txt-etiqueta">IVA (19 %):</td>
            <td><input type="text" id="txtIva" readonly /></td>
        </tr>
        <tr>
            <td class="txt-etiqueta">Total a pagar:</td>
            <td><input type="text" id="txtTotal" readonly /></td>
        </tr>
    </table>
```

**Paso 6, script** (`webParqueadero/scripts_form/ctlFrmParqueadero.js`):
1. Se **borró** la línea `pedirValores();`, porque no hay prompt.
2. PASO 1 y PASO 2 quedaron así:

```javascript
    // ========== PASO 1: leer y validar las entradas ==========
    // >>> CAMBIAR: una línea por cada dato de entrada.
    //   leerTexto(id, nombre)                    → textos y listas (select)
    //   leerNumero(id, nombre, permitirCero)     → números con decimales (float)
    //   leerEntero(id, nombre, permitirCero)     → números enteros (int)
    let placa = leerTexto("#txtPlaca", "la placa");                    if (placa === null) return;
    let tipoVeh = leerTexto("#cboTipoVeh", "el tipo de vehículo");     if (tipoVeh === null) return;
    let minutos = leerEntero("#txtMinutos", "los minutos", false);     if (minutos === null) return;
    let frecuente = $("#chkFrec").is(":checked");                      // Checkbox → true / false

    // ========== PASO 2: armar el JSON ==========
    // Las claves deben llamarse IGUAL que las propiedades de entrada del modelo.
    // >>> CAMBIAR
    const datosOut = {
        placa: placa,
        tipoVeh: tipoVeh,
        minutos: minutos,
        frecuente: frecuente
    };
```

3. PASO 3 quedó así:

```javascript
        // ========== PASO 3: mostrar la respuesta ==========
        // >>> CAMBIAR: una línea por cada salida: $("#idCaja").val(fnro(Rpta.propiedad));
        //   Usar fnro() para números. Para textos (ej. "Aprobó"): $("#txt...").val(Rpta.propiedad);
        $("#txtHoras").val(Rpta.horas);                 // Entero: sin fnro
        $("#txtVrHora").val(fnro(Rpta.vrHora));
        $("#txtBruto").val(fnro(Rpta.vrBruto));
        $("#txtPorcDscto").val(fnro(Rpta.porcDscto));
        $("#txtDscto").val(fnro(Rpta.vrDscto));
        $("#txtIva").val(fnro(Rpta.vrIva));
        $("#txtTotal").val(fnro(Rpta.vrTotal));
```

---

## 6. Catálogo de cálculos (C#)

Copia el bloque que necesites dentro de `Calcular` o de un método privado. Cambia los nombres por los de tu modelo. Las constantes en MAYÚSCULAS se declaran en el PASO 1, por ejemplo `private const float PORC_IVA = 19f;`.

### 6.1 Porcentajes: descuento, IVA, recargo, comisión, retención

```csharp
// Descuento: se RESTA
pMod.vrDscto = pMod.vrSubTot * pMod.porcDscto / 100;

// IVA sobre el valor ya descontado (si el enunciado dice "sobre el subtotal", usar pMod.vrSubTot)
pMod.vrIva = (pMod.vrSubTot - pMod.vrDscto) * PORC_IVA / 100;

// Recargo o comisión: igual que el descuento, pero se SUMA
pMod.vrRecargo = pMod.vrSubTot * PORC_RECARGO / 100;

pMod.vrTotal = pMod.vrSubTot - pMod.vrDscto + pMod.vrRecargo + pMod.vrIva;
```

### 6.2 Rangos: "entre … y …", "mayor a …" (`if / else if`)

Se escribe **del rango más alto al más bajo**. "Entre 50 y 100" incluye el 50 y el 100.

```csharp
int totCopias = pMod.kC + pMod.kO + pMod.kE;

if (totCopias > 100)           // "mayor a 100"
{
    pMod.porcDscto = 15;
}
else if (totCopias >= 50)      // "entre 50 y 100"
{
    pMod.porcDscto = 10;
}
else                           // "menos de 50"
{
    pMod.porcDscto = 0;
}
```

### 6.3 Categorías: "según el tipo…" (`switch`)

```csharp
switch (pMod.tipoVeh)          // El valor viene del <select>: "M", "C", "K"
{
    case "M":
        pMod.vrHora = TARIFA_MOTO;
        break;
    case "C":
        pMod.vrHora = TARIFA_CARRO;
        break;
    default:
        pMod.vrHora = TARIFA_CAMIONETA;
        break;
}
```

### 6.4 Sí / No: una casilla (`bool`) → operador ternario

```csharp
// condición ? valorSiEsVerdad : valorSiEsFalso
pMod.porcDscto = pMod.frecuente ? PORC_DSCTO_FRECUENTE : 0;
```

### 6.5 Hora o fracción, tope máximo y cobro mínimo

```csharp
pMod.horas = (int)Math.Ceiling(pMod.minutos / 60.0);  // 61 min → 2 horas. ¡Usar 60.0, no 60!
pMod.horas = Math.Min(pMod.horas, HORAS_TOPE);        // Tope: nunca más de HORAS_TOPE
pMod.vrTotal = Math.Max(pMod.vrTotal, COBRO_MINIMO);  // Mínimo: nunca menos de COBRO_MINIMO
```

### 6.6 División entera y residuo

```csharp
int horasCompletas = pMod.minutos / 60;   // Entre enteros la división es entera: 125 / 60 = 2
int minSobrantes = pMod.minutos % 60;     // Residuo: 125 % 60 = 5
float promedio = pMod.suma / 3f;          // Para decimales, al menos un número debe ser float (3f)
```

### 6.7 Tarifa escalonada por tramos (energía, agua, impuestos)

"Los primeros 100 kWh a $500, de 101 a 300 a $600 y el exceso a $800". Cada tramo cobra solo su parte:

```csharp
float tramo1 = Math.Min(pMod.consumo, 100);                     // hasta 100
float tramo2 = Math.Min(Math.Max(pMod.consumo - 100, 0), 200);  // de 101 a 300 (200 unidades)
float tramo3 = Math.Max(pMod.consumo - 300, 0);                 // más de 300
pMod.vrConsumo = tramo1 * 500 + tramo2 * 600 + tramo3 * 800;
```

Si el enunciado dice que **todo** el consumo se cobra a la tarifa del rango ("si consume más de 300, el kWh vale $800"), usa el patrón de rangos 6.2 y multiplica el consumo total.

### 6.8 Notas: promedio ponderado y resultado en texto

```csharp
pMod.notaDef = pMod.nota1 * 0.30f + pMod.nota2 * 0.30f + pMod.nota3 * 0.40f;  // 30 %, 30 %, 40 %
pMod.notaDef = (float)Math.Round(pMod.notaDef, 1);                              // 1 decimal
pMod.estado = pMod.notaDef >= 3.0f ? "Aprobó" : "Reprobó";                      // Salida string
pMod.mayor = Math.Max(pMod.nota1, Math.Max(pMod.nota2, pMod.nota3));            // La mayor de 3
```

### 6.9 Nómina: devengado, extras, auxilio y deducciones

Usa los valores que dé el enunciado (SMMLV, auxilio, horas del mes, porcentajes):

```csharp
float vrHora = pMod.salario / HORAS_MES;
pMod.vrExtras = pMod.horasExtra * vrHora * (1 + PORC_RECARGO_EXTRA / 100);     // Recargo del 25 % → 1.25
pMod.vrDevengado = pMod.salario / 30 * pMod.dias + pMod.vrExtras;
pMod.vrAuxTransp = pMod.salario <= 2 * SMMLV ? AUX_TRANSPORTE : 0;            // Solo si gana ≤ 2 SMMLV
pMod.vrSalud = pMod.vrDevengado * PORC_SALUD / 100;                           // 4 %
pMod.vrPension = pMod.vrDevengado * PORC_PENSION / 100;                       // 4 %
pMod.vrNeto = pMod.vrDevengado + pMod.vrAuxTransp - pMod.vrSalud - pMod.vrPension;
```

### 6.10 Intereses y préstamos

```csharp
float i = pMod.tasa / 100;                                                    // 2 % → 0.02
pMod.vrInteres = pMod.capital * i * pMod.meses;                               // Simple:    I = C · i · n
pMod.vrMonto = pMod.capital * (float)Math.Pow(1 + i, pMod.meses);             // Compuesto: M = C · (1 + i)^n
pMod.vrCuota = pMod.capital * i / (1 - (float)Math.Pow(1 + i, -pMod.meses));  // Cuota fija (validar tasa > 0)
```

### 6.11 Fechas: días o noches entre dos fechas (hotel, alquiler)

En el modelo: `public DateTime fecIng { get; set; }` y `public DateTime fecSal { get; set; }`. El JS envía el texto `"2026-10-13"` y Web API lo convierte en fecha.

```csharp
// En validar():
if (pMod.fecSal <= pMod.fecIng)
{
    pMod.Error = "La fecha de salida debe ser posterior a la de ingreso";
    return false;
}

// En Calcular():
pMod.noches = (pMod.fecSal - pMod.fecIng).Days;
```

Si una fecha es **salida**, llega al JS como `"2026-10-16T00:00:00"`. Para mostrar solo la fecha: `$("#txtFecSal").val(Rpta.fecSal.substring(0, 10));`.

### 6.12 Varios ítems fijos (como Carta / Oficio / Extra-Oficio)

```csharp
pMod.vrTotC = pMod.vrC * pMod.kC;
pMod.vrTotO = pMod.vrO * pMod.kO;
pMod.vrTotE = pMod.vrE * pMod.kE;
pMod.vrSubTot = pMod.vrTotC + pMod.vrTotO + pMod.vrTotE;
```

En el JS, las cantidades que pueden quedar vacías se leen con `permitirCero = true`. Una caja vacía cuenta como 0.

### 6.13 Redondear

```csharp
pMod.vrTotal = (float)Math.Round(pMod.vrTotal, 2);  // 2 decimales
pMod.vrTotal = (float)Math.Round(pMod.vrTotal);     // Sin decimales (al entero más cercano)
float vrRedondeado = (float)Math.Ceiling(pMod.vrTotal / 100) * 100;  // Al siguiente múltiplo de 100
```

---

## 7. Catálogo de campos (HTML + JavaScript)

| Tipo de dato | Fila en el HTML (`tblEntradas`) | Leer en el JS (PASO 1) | En el modelo |
| --- | --- | --- | --- |
| Texto | `<input type="text" id="txtPlaca" />` | `let placa = leerTexto("#txtPlaca", "la placa"); if (placa === null) return;` | `string` |
| Número con decimales | `<input type="text" id="txtPeso" />` | `let peso = leerNumero("#txtPeso", "el peso", false); if (peso === null) return;` | `float` |
| Número entero | `<input type="text" id="txtCant" />` | `let cant = leerEntero("#txtCant", "la cantidad", false); if (cant === null) return;` | `int` |
| Entero que puede quedar vacío (= 0) | `<input type="text" id="txtCantO" />` | `let cantO = leerEntero("#txtCantO", "la cantidad de oficio", true); if (cantO === null) return;` | `int` |
| Lista (`select`) | `<select id="cboTipo"><option value="">-- Seleccione --</option><option value="M">Moto</option></select>` | `let tipo = leerTexto("#cboTipo", "el tipo"); if (tipo === null) return;` | `string` |
| Casilla sí/no | `<input type="checkbox" id="chkFrec" />` | `let frec = $("#chkFrec").is(":checked");` | `bool` |
| Opciones (radio) | `<input type="radio" name="rbJornada" value="D" /> Diurna <input type="radio" name="rbJornada" value="N" /> Nocturna` | `let jornada = $("input[name='rbJornada']:checked").val(); if (jornada === undefined) { alert("Error, seleccione la jornada"); return; }` | `string` |
| Fecha | `<input type="date" id="dtpFecIng" />` | `let fecIng = leerTexto("#dtpFecIng", "la fecha de ingreso"); if (fecIng === null) return;` | `DateTime` |
| Valor inicial con `prompt` | `<input type="text" id="txtVrUnit" readonly />` | `let vrUnit = leerNumero("#txtVrUnit", "el valor unitario", false); if (vrUnit === null) return;` | `float` |

Cada entrada va en una fila completa:

```html
<tr>
    <td class="txt-etiqueta">Placa:</td>
    <td><input type="text" id="txtPlaca" /></td>
</tr>
```

Mostrar resultados (PASO 3):

| Tipo de salida | Línea en el JS | Caja en `tblSalidas` |
| --- | --- | --- |
| Dinero o decimal (`float`) | `$("#txtTotal").val(fnro(Rpta.vrTotal));` | `<input type="text" id="txtTotal" readonly />` |
| Entero (`int`) | `$("#txtHoras").val(Rpta.horas);` | `<input type="text" id="txtHoras" readonly />` |
| Texto (`string`) | `$("#txtEstado").val(Rpta.estado);` | `<input type="text" id="txtEstado" readonly />` |

`fnro(118787.5)` → `118.787,50`: formato de Colombia (`es-CO`) con 2 decimales.

---

## 8. Nombres que pida el profesor

| Si el profesor pide… | Haz esto |
| --- | --- |
| Otro nombre de proyecto o de clases con el mismo patrón (`apiX`, `modX`, `clsOpeX`, `servXController`, `frmX`, `ctlFrmX`) | Script `crearProyecto.ps1 -Nombre X` (sección 4.2). |
| Otro nombre de clase o método (ej. `Facturar`) | En Visual Studio: cursor sobre el nombre → **Ctrl+R, Ctrl+R** (o clic derecho → *Cambiar nombre*) → Enter. Cambia todas las referencias en C#. |
| Otro nombre de **propiedad** del modelo | Igual (Ctrl+R, Ctrl+R), **y además** cámbialo a mano en el JS (`datosOut` y `Rpta.…`). |
| Otro nombre de archivo `.cs` | En el Explorador de soluciones: clic derecho → *Cambiar nombre*. |
| Otro nombre del formulario (`frmFact.html`) | Clic derecho → *Cambiar nombre* en el Explorador de soluciones. |
| Otro nombre del script (`ctlFrmFact.js`) | Cambia el nombre del archivo **y** el `<script src="../scripts_form/…">` al final del HTML. |
| Otro nombre de controlador (`servFacturaController`) | Cambia el nombre de la clase **y** la dirección en el JS: `var dir = ".../api/servFactura"`. No se quita la palabra `Controller`. |

---

## 9. Si el profesor pide crear los proyectos desde cero (asistente de Visual Studio)

1. **Crear un proyecto** → *Aplicación web ASP.NET (.NET Framework)* → nombre `apiX` → plantilla **API web** → **desmarcar HTTPS** → *Crear*. Visual Studio crea una solución y con ella NuGet funciona.
2. *Herramientas → Administrador de paquetes NuGet → Consola* → `Install-Package Microsoft.AspNet.WebApi.Cors`.
3. En `App_Start/WebApiConfig.cs`, agrega `config.EnableCors();` dentro de `Register`.
4. Crea la carpeta `Clases` y agrega las clases (*Agregar → Clase*). El controlador se agrega con *Agregar → Controlador → Web API 2 en blanco*.
5. **Copia el contenido** de los archivos de la plantilla. **Ojo:** deja el `namespace` que creó Visual Studio (`apiX.Models`, `apiX.Clases`…) y ajusta los `using`.
6. Haz lo mismo con el proyecto web: plantilla **Web Forms**, sin HTTPS. Crea las carpetas `Paginas` y `scripts_form` y copia el HTML y el JS.
7. **Puertos:** el asistente asigna puertos aleatorios. Míralos en *Propiedades del proyecto → Web → Dirección URL del proyecto* y actualiza `var dir` (puerto de la API) y `origins` (puerto de la web).

---

## 10. Si piden guardar en una base de datos (SQL Server)

Usa como modelo lo que ya está hecho en `apiSusCopias`:

1. **Script SQL** (copia `apiSusCopias/BaseDatos/bdSusCopias.sql`):
   - una tabla con una columna por cada propiedad del modelo: `float` → `DECIMAL(14,2)`, `int` → `INT`, `string` → `NVARCHAR(100)`, `bool` → `BIT`, `DateTime` → `DATE`;
   - un procedimiento `spGuardar…` con un parámetro por columna.
2. **Clase de datos** (copia `apiSusCopias/Clases/clsDatSusCopias.cs`): cambia el nombre del procedimiento y los parámetros. Para `bool` usa `SqlDbType.Bit` y para `DateTime` `SqlDbType.Date`.
3. **Cadena de conexión** en `Web.config` (sección `<connectionStrings>` de `apiSusCopias/Web.config`).
4. **Llamada** al final de `Calcular`, dentro de `try/catch` (igual que en `clsOpeSusCopias.Facturar`).
5. **Agrega al `.csproj`** la clase nueva: en Visual Studio, *Agregar → Elemento existente*. ADO.NET ya viene en .NET Framework y no necesita NuGet.

---

## 11. Errores comunes y solución

| Lo que ves | Causa | Solución |
| --- | --- | --- |
| `Error, TypeError: Failed to fetch` | Cuatro causas posibles: la API no está corriendo, el puerto de `var dir` es otro, el nombre del controlador en `var dir` está mal escrito, o `origins` en `[EnableCors]` no tiene el puerto de la web. | Ctrl+F5 en la API. Revisa los puertos en *Propiedades → Web*. `var dir = ".../api/servX"` debe ser el nombre de la clase **sin** `Controller`. |
| `Error, el servicio respondió con el estado: 500` | Falla el código C# mientras calcula, por ejemplo una división entre cero con enteros | Revisa las fórmulas. En `validar()`, comprueba que el divisor no sea 0. |
| En una caja sale `¿Infinity?` o `¿NaN?` | División entre cero con `float` | Igual: valida que el divisor no sea 0. |
| Sale un error de validación (*"… debe ser mayor a cero"*) aunque sí escribiste el dato | La clave en `datosOut` no se llama igual que la propiedad del modelo, o el tipo no coincide (2.5 en un `int`). Ese dato llega como 0. | Compara `datosOut` con el modelo. Si el dato tiene decimales, la propiedad debe ser `float`. |
| `Error, No se recibieron los datos (revise los nombres y tipos del JSON)` | Lo que llegó no es un JSON válido | Revisa que esté `body: JSON.stringify(datosOut)` y el header `"Content-Type": "application/json"`. |
| En una caja sale `¿undefined?` | `Rpta.nombre` no coincide con la propiedad del modelo | Escríbelo exactamente igual, con mayúsculas y minúsculas. |
| Un resultado sale en 0 | Falta calcularlo en la clase de operaciones | Revisa que exista la línea `pMod.resultado = …;`. |
| Un cálculo da un entero raro (125/60 = 2) | División entre dos `int` | Usa `60.0` o `60f`. |
| `CS0103: El nombre '…' no existe` | Nombre mal escrito o constante no declarada | Declárala en el PASO 1 o corrige el nombre. |
| `CS0266: No se puede convertir implícitamente 'double' en 'float'` | `Math.Round`, `Math.Pow` y `Math.Ceiling` devuelven `double` | Pon `(float)` o `(int)` adelante: `(float)Math.Round(x, 2)`. |
| La página no cambia después de editar el JS | El navegador tiene la versión vieja | **Ctrl+F5** en el navegador. |
| Al iniciar la API dice que el puerto está en uso | Otra API sigue corriendo en IIS Express | Cierra IIS Express desde el ícono junto al reloj, o cierra la otra ventana de Visual Studio. |

---

## 12. Lista de chequeo antes de entregar

- [ ] Algoritmo con entradas, proceso y salidas, y prueba de escritorio con los **límites** de cada rango.
- [ ] Nombres de proyectos, clases, métodos y archivos **exactamente** como los pide el profesor.
- [ ] Modelo: todas las entradas y salidas, constructor completo, `Error` presente.
- [ ] Lógica solo en la clase de operaciones, con constantes con nombre y `validar()`.
- [ ] Controlador con `[EnableCors]` y un único `Post`.
- [ ] Formulario con tablas al 80 %, `.txt-titulo` y salidas `readonly`.
- [ ] JS: `prompt` si se pide, `datosOut` con los nombres del modelo, `fetch` POST con `JSON.stringify` y `fnro()` en los valores.
- [ ] Probado con los casos de la prueba de escritorio: **los resultados coinciden**.
