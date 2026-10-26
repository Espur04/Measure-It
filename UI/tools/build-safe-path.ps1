$ErrorActionPreference = "Stop"

$uiRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$projectRoot = Split-Path -Parent $uiRoot
$driveNames = @("Z", "Y", "X", "W", "V", "U", "T", "S", "R", "Q", "P", "O", "N", "M", "L", "K", "J", "I", "H", "G", "F", "E")
$driveLetter = $driveNames | Where-Object { -not (Get-PSDrive -Name $_ -ErrorAction SilentlyContinue) } | Select-Object -First 1

if (-not $driveLetter) {
    throw "No free drive letter is available to build the UI from a safe path."
}

$buildExitCode = 1
$locationPushed = $false
try {
    & subst.exe "$driveLetter`:" $projectRoot | Out-Null
    if ($LASTEXITCODE -ne 0) {
        throw "Could not create a temporary drive mapping for the project."
    }

    Push-Location "$driveLetter`:\UI"
    $locationPushed = $true
    & npm.cmd run build:webpack
    $buildExitCode = $LASTEXITCODE
}
finally {
    if ($locationPushed) {
        Pop-Location
    }

    & subst.exe "$driveLetter`:" /d | Out-Null
}

exit $buildExitCode
