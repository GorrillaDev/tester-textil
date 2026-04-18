$ErrorActionPreference = "Stop"
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $scriptDir

$kitBin = "C:\Program Files (x86)\Windows Kits\10\bin\10.0.26100.0\x64"
$makecat = Join-Path $kitBin "makecat.exe"
$signtool = Join-Path $kitBin "signtool.exe"

if (-not (Test-Path $makecat)) {
    throw "No se encontro makecat.exe en $makecat"
}

if (-not (Test-Path $signtool)) {
    throw "No se encontro signtool.exe en $signtool"
}

$catPath = Join-Path $scriptDir "AcuratexControlBridge.cat"
$cerPath = Join-Path $scriptDir "AcuratexTest.cer"

if (Test-Path $catPath) {
    Remove-Item $catPath -Force
}

Write-Host "Creando catalogo..."
& $makecat "AcuratexControlBridge.cdf"

Write-Host "Creando certificado de prueba..."
$cert = New-SelfSignedCertificate `
    -Type CodeSigningCert `
    -Subject "CN=Acuratex Test" `
    -CertStoreLocation "Cert:\LocalMachine\My" `
    -KeyAlgorithm RSA `
    -KeyLength 2048 `
    -HashAlgorithm SHA256 `
    -NotAfter (Get-Date).AddYears(5)

Export-Certificate -Cert $cert -FilePath $cerPath -Force | Out-Null

Write-Host "Confiando certificado en Root y TrustedPublisher..."
Import-Certificate -FilePath $cerPath -CertStoreLocation "Cert:\LocalMachine\Root" | Out-Null
Import-Certificate -FilePath $cerPath -CertStoreLocation "Cert:\LocalMachine\TrustedPublisher" | Out-Null

Write-Host "Firmando catalogo..."
& $signtool sign /v /fd SHA256 /sha1 $cert.Thumbprint /s My /sm $catPath

Write-Host ""
Write-Host "Listo. Ahora ejecuta instalar_driver_firmado.cmd como administrador."
