<#
  crearProyecto.ps1
  Crea una copia de la plantilla (apiPlantilla y webPlantilla) con otro nombre.
  Cambia "Plantilla" por el nombre indicado en carpetas, archivos, clases y namespaces.

  Uso (en la carpeta "plantilla": clic derecho en un espacio vacio -> "Abrir en Terminal"):

      powershell -ExecutionPolicy Bypass -File .\crearProyecto.ps1 -Nombre Parqueadero

  Resultado (junto a la plantilla):
      apiParqueadero\apiParqueadero.csproj   (modParqueadero, clsOpeParqueadero, servParqueaderoController)
      webParqueadero\webParqueadero.csproj   (Paginas\frmParqueadero.html, scripts_form\ctlFrmParqueadero.js)

  Opcional: -Destino C:\Parcial   para crear las carpetas en otra ubicacion.
#>
param(
    [Parameter(Mandatory = $true)]
    [string]$Nombre,

    [string]$Destino = $PSScriptRoot
)

$ErrorActionPreference = 'Stop'

if ($Nombre -notmatch '^[A-Za-z][A-Za-z0-9]*$') {
    throw "El nombre solo puede tener letras y numeros, y debe empezar por una letra (ejemplo: Parqueadero)."
}

$token = 'Plantilla'
$utf8SinBom = New-Object System.Text.UTF8Encoding($false)
$extensionesTexto = @('.cs', '.csproj', '.config', '.asax', '.html', '.js')

if (-not (Test-Path $Destino)) {
    New-Item -ItemType Directory -Path $Destino | Out-Null
}

foreach ($tipo in @('api', 'web')) {
    $origen = Join-Path $PSScriptRoot ($tipo + $token)
    $nuevo = Join-Path $Destino ($tipo + $Nombre)

    if (-not (Test-Path $origen)) {
        throw "No se encontro la carpeta $origen. Ejecute el script dentro de la carpeta plantilla."
    }
    if (Test-Path $nuevo) {
        throw "Ya existe la carpeta $nuevo. Borrela o use otro nombre."
    }

    Copy-Item -Path $origen -Destination $nuevo -Recurse

    # Quitar carpetas generadas por Visual Studio (si la plantilla ya se habia abierto)
    foreach ($carpeta in @('bin', 'obj', '.vs')) {
        $ruta = Join-Path $nuevo $carpeta
        if (Test-Path $ruta) {
            Remove-Item -Path $ruta -Recurse -Force
        }
    }

    # Renombrar los archivos que contienen "Plantilla"
    Get-ChildItem -Path $nuevo -Recurse -File -Force |
        Where-Object { $_.Name.Contains($token) } |
        ForEach-Object { Rename-Item -Path $_.FullName -NewName ($_.Name.Replace($token, $Nombre)) }

    # Reemplazar "Plantilla" dentro del codigo
    Get-ChildItem -Path $nuevo -Recurse -File -Force |
        Where-Object { $extensionesTexto -contains $_.Extension.ToLower() } |
        ForEach-Object {
            $texto = [System.IO.File]::ReadAllText($_.FullName)
            if ($texto.Contains($token)) {
                [System.IO.File]::WriteAllText($_.FullName, $texto.Replace($token, $Nombre), $utf8SinBom)
            }
        }

    Write-Host "Creado: $nuevo"
}

Write-Host ""
Write-Host "Listo. Abra en Visual Studio:"
Write-Host "  $(Join-Path $Destino ('api' + $Nombre))\api$Nombre.csproj"
Write-Host "  $(Join-Path $Destino ('web' + $Nombre))\web$Nombre.csproj"
