$connectionString = "Server=(LocalDB)\MSSqlLocalDB;Database=DbUpSample;Trusted_Connection=Yes;Connection Timeout=2;" 

# Path to directory containing dbup-core.dll and dbup-sqlserver.dll
$PathToDbUpBinaries = "C:\DEV\OSS\DbUp\src\dbup-sqlserver\bin\Release\net35\";

$Script = "$PSScriptRoot\Publish-DbUp.ps1"

& $Script -ScriptDirectory "$PSScriptRoot\Scripts\" `
 -Verbose `
 -ConnectionString $connectionString -EnsureDatabase -PathToDbUpBinaries $PathToDbUpBinaries