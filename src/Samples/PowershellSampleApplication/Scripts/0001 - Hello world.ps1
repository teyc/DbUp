Write-Output "Hello World!"

If (Test-Path "SchemaVersions")
{
	Write-Output "SchemaVersions directory found"
}
else
{
	Write-Output "SchemaVersions directory missing"
}


Write-Error "Error written"

Write-Output "Next"

function Foo()
{
	Write-Output "Inside Foo"
	throw "Foo error"

}

Foo;

