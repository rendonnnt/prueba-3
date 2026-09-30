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
        vrC: parseFloat(vrCa),
        vrO: parseFloat(vrOf),
        vrE: parseFloat(vrEx),
        kC: Number(kCa),
        kO: Number(kOf),
        kE: Number(kEx)
    };

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
}
