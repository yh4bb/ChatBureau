param(
 [ValidatePattern('^\d+\.\d+\.\d+$')][string]$Version = '1.5.0',
 [ValidatePattern('^([A-Za-z0-9_.-]+/[A-Za-z0-9_.-]+)?$')][string]$Repository = '',
 [switch]$Test
)
$ErrorActionPreference = 'Stop'
Set-Location $PSScriptRoot
$buildDir = Join-Path $PSScriptRoot 'build'
$distDir = Join-Path $PSScriptRoot 'dist'
New-Item -ItemType Directory -Path $buildDir,$distDir -Force | Out-Null
@"
using System.Reflection;
[assembly: AssemblyVersion("$Version.0")]
[assembly: AssemblyFileVersion("$Version.0")]
namespace ChatBureau { static class BuildInfo { public const string Version="$Version"; public const string Repository="$Repository"; } }
"@ | Set-Content (Join-Path $buildDir 'BuildInfo.cs') -Encoding UTF8
$sources = @(Get-ChildItem src -Filter '*.cs' | ForEach-Object FullName) + (Join-Path $buildDir 'BuildInfo.cs')
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
$wpf = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\WPF'
& $compiler /nologo /target:winexe /optimize+ /out:dist\ChatBureau.exe /reference:System.Drawing.dll /reference:System.Windows.Forms.dll /reference:System.Xml.dll /reference:System.Web.Extensions.dll /reference:"$wpf\UIAutomationClient.dll" /reference:"$wpf\UIAutomationTypes.dll" /reference:"$wpf\WindowsBase.dll" $sources
if ($LASTEXITCODE -ne 0) { throw 'Compilation échouée' }
if ($Test) {
 $testDir=Join-Path $buildDir 'tests'
 New-Item -ItemType Directory -Path $testDir -Force | Out-Null
 foreach ($arguments in @(@('--ui-test',('"'+$testDir+'"')),@('--update-test',('"'+$testDir+'"')),@('--smoke-test'))) {
  $process=Start-Process -FilePath (Join-Path $distDir 'ChatBureau.exe') -ArgumentList $arguments -WindowStyle Hidden -PassThru
  if(-not $process.WaitForExit(60000)){Stop-Process -Id $process.Id;throw 'Test expiré'}
  if($process.ExitCode -ne 0){throw "Test échoué : $arguments (voir build/tests)"}
 }
}
Copy-Item README.md -Destination $distDir
Compress-Archive -Path (Join-Path $distDir 'ChatBureau.exe'),(Join-Path $distDir 'README.md') -DestinationPath (Join-Path $distDir 'ChatBureau-Windows.zip') -Force
$hash=(Get-FileHash (Join-Path $distDir 'ChatBureau.exe') -Algorithm SHA256).Hash.ToLowerInvariant()
"$hash  ChatBureau.exe" | Set-Content (Join-Path $distDir 'ChatBureau.exe.sha256') -Encoding ASCII
Write-Output "ChatBureau $Version compilé. Dépôt de mise à jour : $Repository"
