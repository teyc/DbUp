Write-Output "Hello World!"

If (Get-Path "SchemaVersions")
{
	Write-Output "SchemaVersions directory found"
}
else
{
	Write-Output "SchemaVersions directory missing"
}