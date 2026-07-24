# Instala o SQL Server 2022 Developer Edition (gratuito) com autenticacao mista (sa)
# usando a mesma senha ja configurada em KnowHowApi/appsettings.json.
#
# IMPORTANTE: abra o PowerShell como Administrador (clique direito > "Executar como
# administrador") antes de rodar este script. Rodar dentro do terminal integrado do
# VS Code pode nao ter permissao de elevacao e travar a instalacao no meio do caminho.

$ErrorActionPreference = "Stop"

$isAdmin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $isAdmin) {
    throw "Este script precisa rodar em um PowerShell como Administrador. Feche esta janela, abra o PowerShell com 'Executar como administrador' e rode o script de novo."
}

$mediaPath = "C:\SQLServerMedia"
$saPassword = "@Senha123@"

Write-Host "1) Baixando o bootstrapper do SQL Server 2022 Developer..."
$bootstrapper = "$env:TEMP\SQL2022-SSEI-Dev.exe"
Invoke-WebRequest -Uri "https://download.microsoft.com/download/c/c/9/cc9c6797-383c-4b24-8920-dc057c1de9d3/SQL2022-SSEI-Dev.exe" -OutFile $bootstrapper

Write-Host "2) Baixando a midia completa de instalacao (necessario para configurar modo misto)..."
Remove-Item $mediaPath -Recurse -Force -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Force -Path $mediaPath | Out-Null
$p = Start-Process -FilePath $bootstrapper -ArgumentList "/ACTION=Download /MEDIATYPE=CAB /MEDIAPATH=$mediaPath /QUIET" -Wait -PassThru
if ($p.ExitCode -ne 0) { throw "Download da midia falhou com codigo $($p.ExitCode)" }

Write-Host "3) Extraindo a midia..."
$exe = Get-ChildItem -Path $mediaPath -Filter "*.exe" | Select-Object -First 1
if (-not $exe) { throw "Nenhum executavel encontrado em $mediaPath apos o download" }
$extractPath = "$mediaPath\Extracted"
Start-Process -FilePath $exe.FullName -ArgumentList "/X:$extractPath /Q" -Wait

Write-Host "4) Instalando o SQL Server (Engine, modo misto, TCP habilitado)..."
$setup = "$extractPath\setup.exe"
$currentUser = "$env:USERDOMAIN\$env:USERNAME"
Start-Process -FilePath $setup -ArgumentList @(
    "/Q",
    "/ACTION=Install",
    "/FEATURES=SQLEngine",
    "/INSTANCENAME=MSSQLSERVER",
    "/SECURITYMODE=SQL",
    "/SAPWD=$saPassword",
    "/SQLSYSADMINACCOUNTS=`"BUILTIN\Administrators`" `"$currentUser`"",
    "/TCPENABLED=1",
    "/IACCEPTSQLSERVERLICENSETERMS",
    "/UPDATEENABLED=0"
) -Wait

Write-Host "5) Reiniciando o servico do SQL Server..."
Restart-Service -Name "MSSQLSERVER" -Force

Write-Host "Concluido! SQL Server Developer Edition instalado com sa/$saPassword em localhost."
