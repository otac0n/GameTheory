Param(
    [Parameter(Mandatory=$true)]
    [string]$Namespace
)
echo "Creating namespace GameTheory.$Namespace..."

$replacements = @{
    '$namespace$' = $Namespace;
    '$year$' = (Get-Date).Year;
}
for ($i = 1; $i -le 10; $i = $i + 1) {
    $guid = [guid]::NewGuid()
    $replacements['$guid' + $i + '$'] = $guid.ToString()
    $replacements['$guid' + $i + '.lower$'] = $guid.ToString().ToLower()
    $replacements['$guid' + $i + '.upper$'] = $guid.ToString().ToUpper()
}

$source = split-path -parent $MyInvocation.MyCommand.Definition
$dest = split-path -parent $source
$encoding = [Text.Encoding]::GetEncoding(28591);
ls -r -exclude *.ps1 | %{
    $name = $_.FullName.Substring($source.Length)
    $replacements.Keys | %{
        $name = $name -replace [Text.RegularExpressions.Regex]::Escape($_), $replacements[$_]
    }
    $path = $dest + $name
    if ($_.PSIsContainer) {
        if (!(Test-Path $path)) {
            echo "Creating $name..."
            md $path | Out-Null
        }
    } elseif (Test-Path $path) {
        Write-Warning "Won't overwrite $path with $name."
    } else {
        echo "Copying $name..."
        $content = [IO.File]::ReadAllText($_.FullName, $encoding)
        $newcontent = $content
        $replacements.Keys | %{
            $newcontent = $newcontent -replace [Text.RegularExpressions.Regex]::Escape($_), $replacements[$_]
        }
        [IO.File]::WriteAllText($path, $newcontent, $encoding)
    }
}
