
param(
  [Parameter(Mandatory = $true)]
  [ValidateNotNullOrEmpty()]
  [Alias('n')]
  [string]
  $name
)

$rootDirectory = git rev-parse --show-toplevel
$apiDirectory = $rootDirectory + "/src/GlucoPilot.Data"

Push-Location -Path $apiDirectory

Write-Host "Adding migration $name"

<# MSSQL #>
Write-Host "Adding migrations for MSSQL"

dotnet ef migrations -v add $name --project ../GlucoPilot.Data.Migrators.MSSQL/ --context GlucoPilotDbContext --startup-project ../GlucoPilot.Api --output-dir Migrations

Write-Host -NoNewLine 'Migrations Added. Press any key to continue...';
$null = $Host.UI.RawUI.ReadKey('NoEcho,IncludeKeyDown');

Pop-Location