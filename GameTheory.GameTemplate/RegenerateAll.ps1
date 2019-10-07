$source = split-path -parent $MyInvocation.MyCommand.Definition
$dest = split-path -parent $source

Get-ChildItem $dest GameTheory.Games.* | %{
    [regex]::match($_.Name, '^GameTheory\.Games\.([^.]+)$')
} | ?{
    $_.Success
} | %{
    .\Create.ps1 -Game $_.Groups[1].Value
}
