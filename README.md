# SampleWithHttps

## `Create-Certs.ps1` for windows containers:

```powershell

[string]$Password = [Guid]::NewGuid().ToString("N")
Set-Content -Path "${PSScriptRoot}\Password.txt" -Value $Password -NoNewline

# Remove the Docker command
# docker run --rm --entrypoint="/bin/bash" -v "${PSScriptRoot}:/Certs" -w="/Certs" mcr.microsoft.com/dotnet/aspnet:8.0 "/Certs/CreateCerts.sh"

# Generate certificates using PowerShell
$cert = New-SelfSignedCertificate -Type Custom -Subject "CN=Test CA" -KeyExportPolicy Exportable -KeySpec Signature -KeyLength 4096 -CertStoreLocation "${PSScriptRoot}" -NotAfter (Get-Date).AddYears(1)
$certPassword = ConvertTo-SecureString -String $Password -Force -AsPlainText
Export-PfxCertificate -Cert "${PSScriptRoot}\$($cert.Thumbprint)" -FilePath "${PSScriptRoot}\test-ca.pfx" -Password $certPassword
Export-Certificate -Cert "${PSScriptRoot}\$($cert.Thumbprint)" -FilePath "${PSScriptRoot}\test-ca.crt"

# Repeat for backend and samplewithhttps
$backendCert = New-SelfSignedCertificate -Type Custom -Subject "CN=Backend" -KeyExportPolicy Exportable -KeySpec Signature -KeyLength 4096 -CertStoreLocation "${PSScriptRoot}" -NotAfter (Get-Date).AddYears(1)
Export-PfxCertificate -Cert "${PSScriptRoot}\$($backendCert.Thumbprint)" -FilePath "${PSScriptRoot}\Backend.pfx" -Password $certPassword

$sampleWithHttpsCert = New-SelfSignedCertificate -Type Custom -Subject "CN=SampleWithHttps" -KeyExportPolicy Exportable -KeySpec Signature -KeyLength 4096 -CertStoreLocation "${PSScriptRoot}" -NotAfter (Get-Date).AddYears(1)
Export-PfxCertificate -Cert "${PSScriptRoot}\$($sampleWithHttpsCert.Thumbprint)" -FilePath "${PSScriptRoot}\SampleWithHttps.pfx" -Password $certPassword

# Update environment files
$envTemplate = Get-Content -Path "${PSScriptRoot}\ContainerCerts.env.template"
$envWPassword = $envTemplate.Replace("`$Password", $Password)

$sampeWithHttpsEnv = $envWPassword.Replace("`$ProjectName", "SampleWithHttps")
Set-Content -Path "${PSScriptRoot}\..\SampleWithHttps\ContainerCerts.env" -Value $sampeWithHttpsEnv

$backEndEnv = $envWPassword.Replace("`$ProjectName", "Backend")
Set-Content -Path "${PSScriptRoot}\..\BackEnd\ContainerCerts.env" -Value $backEndEnv

# Add the CA certificate to the store
$testCaCert = New-Object -TypeName "System.Security.Cryptography.X509Certificates.X509Certificate2" @("${PSScriptRoot}\test-ca.crt", $null)

$storeName = [System.Security.Cryptography.X509Certificates.StoreName]::Root;
$storeLocation = [System.Security.Cryptography.X509Certificates.StoreLocation]::CurrentUser
$store = New-Object System.Security.Cryptography.X509Certificates.X509Store($storeName, $storeLocation)
$store.Open(([System.Security.Cryptography.X509Certificates.OpenFlags]::ReadWrite))
try
{
    $store.Add($testCaCert)
}
finally
{
    $store.Close()
    $store.Dispose()
}

```


## Update to the `Dockerfile` for all microservices:

```Dockerfile
# This stage is used to add the CA certificate to the Windows certificate store
FROM base AS testCerts
COPY Certs/test-ca.crt C:/certs/test-ca.crt
RUN powershell -Command `
    $cert = New-Object System.Security.Cryptography.X509Certificates.X509Certificate2("C:/certs/test-ca.crt"); `
    $store = New-Object System.Security.Cryptography.X509Certificates.X509Store([System.Security.Cryptography.X509Certificates.StoreName]::Root, [System.Security.Cryptography.X509Certificates.StoreLocation]::LocalMachine); `
    $store.Open([System.Security.Cryptography.X509Certificates.OpenFlags]::ReadWrite); `
    $store.Add($cert); `
    $store.Close()

```


OR

```Dockerfile
# This stage is used to add the CA certificate to the Windows certificate store
FROM base AS testCerts
COPY Certs/test-ca.crt C:/certs/test-ca.crt
RUN powershell -Command `
    $testCaCert = New-Object -TypeName "System.Security.Cryptography.X509Certificates.X509Certificate2" @("C:/certs/test-ca.crt", $null); `
    $storeName = [System.Security.Cryptography.X509Certificates.StoreName]::Root; `
    $storeLocation = [System.Security.Cryptography.X509Certificates.StoreLocation]::LocalMachine; `
    $store = New-Object System.Security.Cryptography.X509Certificates.X509Store($storeName, $storeLocation); `
    $store.Open([System.Security.Cryptography.X509Certificates.OpenFlags]::ReadWrite); `
    try { $store.Add($testCaCert) } `
    finally { $store.Close(); $store.Dispose() }
```





