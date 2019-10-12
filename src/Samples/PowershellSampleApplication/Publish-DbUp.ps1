<#
.SYNOPSIS
  Publishes SQL scripts to SQL Server
  using DbUp

.EXAMPLE

    .\Publish-DbUp .\ScriptDir "Server=(LocalDB)\MsSqlLocalDB;Database=DbUpExample;Trusted_Connection=Yes" -EnsureDatabase

    Upgrades database with files from .\ScriptDir\*.sql . If the database does not exist, create it.
    
.EXAMPLE

    .\Publish-DbUp .\ScriptDir "Server=(LocalDB)\MsSqlLocalDB;Database=DbUpExample;Trusted_Connection=Yes"
#>
[CmdletBinding()]
param (
    # Connection String to Sql Server
    # Specifies a path to sql scripts.
    [Parameter(Mandatory = $true,
        Position = 0,
        ValueFromPipeline = $true,
        ValueFromPipelineByPropertyName = $true,
        HelpMessage = "Path SQL Scripts.")]
    [string[]]
    $ScriptDirectory,

    [Parameter(Mandatory, Position = 1)]
    [string]
    $ConnectionString,

    # Creates database if it is not present
    [Parameter(Mandatory = $false )]
    [Switch]
    $EnsureDatabase,

    # Specifies a path to dbup-core.dll.
    [Parameter(Mandatory = $false,
        ValueFromPipelineByPropertyName = $true,
        HelpMessage = "Path to dbup-core.dll.")]
    [string[]]
    $PathToDbUpBinaries
)

If ($null -eq $PathToDbUpBinaries) {
    $PathToDbUpBinaries = "C:\DEV\OSS\DbUp\src\dbup-sqlserver\bin\Release\net35\";
}

function ExitWithCode 
{
    param
    (
        $exitcode
    )

    $host.SetShouldExit($exitcode)
    exit
}

If (-Not (Test-Path -PathType Container $PathToDbUpBinaries)) {
    Write-Error "Directory does not exist $PathToDbUpBinaries"
    ExitWithCode 1
}

[System.Reflection.Assembly]::LoadFile("$PathToDbUpBinaries\dbup-core.dll") | Out-Null
[System.Reflection.Assembly]::LoadFile("$PathToDbUpBinaries\dbup-sqlserver.dll") | Out-Null

$deployChangesTo = [DbUp.DeployChanges]::To
$dbUp =
[StandardExtensions]::WithScriptsFromFileSystem(
    [StandardExtensions]::LogToConsole( 
        [SqlServerExtensions]::SqlDatabase(
            $deployChangesTo, 
            $ConnectionString)),
    $ScriptDirectory).Build()

If ($EnsureDatabase) {
    Write-Verbose "Ensuring database $connectionstring exists"
    [SqlServerExtensions]::SqlDatabase([DbUp.EnsureDatabase]::For, $ConnectionString)

    If (-Not $?) {
        ExitWithCode 1
        Return
    }
}
   
    
Write-Verbose "Performing database upgrade"
$result = $dbUp.PerformUpgrade();                
if (-Not ($result.Successful)) {
    ExitWithCode 1
}

