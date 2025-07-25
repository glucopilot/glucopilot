param(
  [Parameter(Mandatory = $false)]
  [Alias('c')]
  [string]
  $dbContext = 'GlucoPilotDbContext',

  [Parameter(Mandatory = $true)]
  [ValidateNotNullOrEmpty()]
  [Alias('n')]
  [string]
  $name
)

$rootDirectory = git rev-parse --show-toplevel

switch ($dbContext) {
    'GlucoPilotDbContext' {
        $workingDirectory = $rootDirectory + "/src/GlucoPilot.Data"
        $projectPath = '../GlucoPilot.Data.Migrators.MSSQL/'
        $startupProject = '../GlucoPilot.Api'
    }
    'GlucoPilotNutritionDbContext' {
        $workingDirectory = $rootDirectory + "/src/GlucoPilot.Nutrition.Data"
        $projectPath = '../GlucoPilot.Nutrition.Data.Migrators.MSSQL/'
        $startupProject = '../GlucoPilot.Nutrition.Data.Importer'
    }
}

Push-Location $workingDirectory

Write-Host "Adding $dbContext migration $name"

<# MSSQL #>
Write-Host "Adding migrations for MSSQL"

dotnet ef migrations -v add $name --project $projectPath --context $dbContext --startup-project $startupProject --output-dir Migrations

Write-Host -NoNewLine 'Migrations Added. Press any key to continue...';
$null = $Host.UI.RawUI.ReadKey('NoEcho,IncludeKeyDown');

Pop-Location
