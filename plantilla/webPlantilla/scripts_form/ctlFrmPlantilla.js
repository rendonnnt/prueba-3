// =====================================================================
// Script del formulario (cliente). Lo que se adapta está marcado con >>> CAMBIAR
// =====================================================================

// Dirección del servicio en la Web API: http://localhost:PUERTO_API/api/NOMBRE_SIN_Controller
// >>> CAMBIAR el puerto si el proyecto API corre en otro (Propiedades del proyecto → Web)
var dir = "http://localhost:50300/api/servPlantilla";

// Al cargar la página
jQuery(function () {
    // >>> BORRAR esta línea si el enunciado NO pide valores iniciales con ventanas prompt
    pedirValores();

    $("#btnProcesar").on("click", function () {
        Procesar();
    });

    $("#btnLimpiar").on("click", function () {
        limpiarTotal();
    });

    // Si cambia cualquier dato de entrada, la respuesta anterior ya no es válida
    $("#tblEntradas input, #tblEntradas select").on("input change", function () {
        limpiarRpta();
    });
});

// ---------------------------------------------------------------------
// Valores iniciales con prompt (como en la guía). Se muestran en cajas readonly.
// ---------------------------------------------------------------------
function pedirValores() {
    // >>> CAMBIAR: un prompt por cada valor inicial que pida el enunciado
    let vrUnit = prompt("Digite el valor unitario del producto:", "");

    if (!esValorValido(vrUnit)) {
        alert("Error, el valor unitario no es válido (debe ser un número mayor a 0)");
        return pedirValores();
    }

    // >>> CAMBIAR: mostrar cada valor en su caja de texto
    $("#txtVrUnit").val(vrUnit.trim());
}

// Retorna true si el texto de un prompt es un número mayor a cero
function esValorValido(valor) {
    return valor != null && valor.trim() != "" && !isNaN(valor) && Number(valor) > 0;
}

// ---------------------------------------------------------------------
// Procesar: leer entradas → armar JSON → enviar al servicio → mostrar respuesta.
// Es async para poder esperar (await) la respuesta de fetch.
// ---------------------------------------------------------------------
async function Procesar() {
    // ========== PASO 1: leer y validar las entradas ==========
    // >>> CAMBIAR: una línea por cada dato de entrada.
    //   leerTexto(id, nombre)                    → textos y listas (select)
    //   leerNumero(id, nombre, permitirCero)     → números con decimales (float)
    //   leerEntero(id, nombre, permitirCero)     → números enteros (int)
    let nomCli = leerTexto("#txtNomCli", "el nombre del cliente");     if (nomCli === null) return;
    let tipoCli = leerTexto("#cboTipoCli", "el tipo de cliente");      if (tipoCli === null) return;
    let vrUnit = leerNumero("#txtVrUnit", "el valor unitario", false); if (vrUnit === null) return;
    let cant = leerEntero("#txtCant", "la cantidad", false);           if (cant === null) return;

    // ========== PASO 2: armar el JSON ==========
    // Las claves deben llamarse IGUAL que las propiedades de entrada del modelo.
    // >>> CAMBIAR
    const datosOut = {
        nomCli: nomCli,
        tipoCli: tipoCli,
        vrUnit: vrUnit,
        cant: cant
    };

    // Se deshabilita el botón mientras responde el servicio (evita doble clic)
    $("#btnProcesar").prop("disabled", true);

    try {
        // Enviar la información al servicio (POST) y esperar la respuesta
        const response = await fetch(dir, {
            method: "POST",
            mode: "cors",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(datosOut)   // Convierte el objeto JavaScript en texto JSON
        });

        if (!response.ok) {
            alert("Error, el servicio respondió con el estado: " + response.status +
                (response.status == 500 ? " (falla en el código C#, por ejemplo una división entre cero)" : ""));
            return;
        }

        // En Rpta queda el JSON de respuesta (las propiedades del modelo)
        const Rpta = await response.json();
        if (Rpta.Error != undefined && Rpta.Error != "") {
            alert("Error, " + Rpta.Error);
            return;
        }

        // ========== PASO 3: mostrar la respuesta ==========
        // >>> CAMBIAR: una línea por cada salida: $("#idCaja").val(fnro(Rpta.propiedad));
        //   Usar fnro() para números. Para textos (ej. "Aprobó"): $("#txt...").val(Rpta.propiedad);
        $("#txtSubTot").val(fnro(Rpta.vrSubTot));
        $("#txtPorcDscto").val(fnro(Rpta.porcDscto));
        $("#txtDscto").val(fnro(Rpta.vrDscto));
        $("#txtIva").val(fnro(Rpta.vrIva));
        $("#txtTotal").val(fnro(Rpta.vrTotal));
    }
    catch (e) {
        alert("Error, " + e);
    }
    finally {
        $("#btnProcesar").prop("disabled", false);
    }
}

// =====================================================================
// Funciones de apoyo (NO necesitan cambios)
// =====================================================================

// Formatea un número con la configuración regional de Colombia y 2 decimales: 1234.5 → 1.234,50
function fnro(vr) {
    if (typeof vr !== "number") {
        // Si en pantalla sale ¿undefined?, el nombre en Rpta.xxx no coincide con la propiedad del modelo
        return "¿" + vr + "?";
    }
    return vr.toLocaleString('es-CO', {
        minimumFractionDigits: 2,   // Mínimo de decimales
        maximumFractionDigits: 2    // Máximo de decimales
    });
}

// Muestra un error y pone el cursor en la caja
function mostrarError(id, mensaje) {
    alert("Error, " + mensaje);
    $(id).trigger("focus");
}

// Lee un texto obligatorio (también sirve para un select). Retorna null si falta.
function leerTexto(id, nombre) {
    let valor = $(id).val().trim();
    if (valor == "") {
        mostrarError(id, "falta " + nombre);
        return null;
    }
    return valor;
}

// Lee un número (con decimales). Si permitirCero es true, una caja vacía cuenta como 0.
// Retorna null si no es válido.
function leerNumero(id, nombre, permitirCero) {
    let texto = $(id).val().trim();
    if (texto == "" && permitirCero) {
        return 0;
    }
    let valor = Number(texto);
    if (texto == "" || isNaN(valor) || valor < 0 || (valor == 0 && !permitirCero)) {
        mostrarError(id, "revise " + nombre + ": debe ser un número " + (permitirCero ? "mayor o igual a 0" : "mayor a 0"));
        return null;
    }
    return valor;
}

// Lee un número entero. Retorna null si no es válido.
function leerEntero(id, nombre, permitirCero) {
    let valor = leerNumero(id, nombre, permitirCero);
    if (valor === null) {
        return null;
    }
    if (!Number.isInteger(valor)) {
        mostrarError(id, "revise " + nombre + ": debe ser un número entero");
        return null;
    }
    return valor;
}

// Limpia los datos de entrada (no los valores pedidos con prompt) y la respuesta
function limpiarTotal() {
    $("#tblEntradas input:not([readonly]):not([type=checkbox]):not([type=radio])").val("");
    $("#tblEntradas input[type=checkbox], #tblEntradas input[type=radio]").prop("checked", false);
    $("#tblEntradas select").prop("selectedIndex", 0);
    limpiarRpta();
    $("#tblEntradas input:not([readonly]), #tblEntradas select").first().trigger("focus");
}

// Limpia todas las cajas de la tabla de resultados
function limpiarRpta() {
    $("#tblSalidas input").val("");
}
