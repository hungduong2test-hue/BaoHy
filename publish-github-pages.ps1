param(
    [int]$Port = 5088,
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

$Root = $PSScriptRoot
$ProjectPath = Join-Path $Root "ThiepCuoi\ThiepCuoi.csproj"
$ProjectDll = Join-Path $Root "ThiepCuoi\bin\$Configuration\net10.0\ThiepCuoi.dll"
$WwwRoot = Join-Path $Root "ThiepCuoi\wwwroot"
$DocsPath = Join-Path $Root "docs"
$Url = "http://127.0.0.1:$Port"
$OutputLog = Join-Path $Root "github-pages-export.out.log"
$ErrorLog = Join-Path $Root "github-pages-export.err.log"

function Assert-ChildPath {
    param(
        [string]$Parent,
        [string]$Child
    )

    $parentPath = [System.IO.Path]::GetFullPath($Parent).TrimEnd('\') + '\'
    $childPath = [System.IO.Path]::GetFullPath($Child)

    if (-not $childPath.StartsWith($parentPath, [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "Refusing to write outside project folder: $Child"
    }
}

Assert-ChildPath -Parent $Root -Child $DocsPath

if (Test-Path $DocsPath) {
    Remove-Item -LiteralPath $DocsPath -Recurse -Force
}

New-Item -ItemType Directory -Path $DocsPath | Out-Null

$appProcess = $null

try {
    if (-not (Test-Path -LiteralPath $ProjectDll)) {
        dotnet build $ProjectPath --configuration $Configuration --nologo
    }

    $arguments = "`"$ProjectDll`" --urls `"$Url`""

    $appProcess = Start-Process `
        -FilePath "dotnet" `
        -ArgumentList $arguments `
        -WorkingDirectory $Root `
        -PassThru `
        -WindowStyle Hidden `
        -RedirectStandardOutput $OutputLog `
        -RedirectStandardError $ErrorLog

    $ready = $false
    for ($i = 0; $i -lt 60; $i++) {
        try {
            $response = Invoke-WebRequest -Uri "$Url/?preview=1" -UseBasicParsing -TimeoutSec 2
            if ($response.StatusCode -eq 200) {
                $ready = $true
                break
            }
        }
        catch {
            Start-Sleep -Milliseconds 500
        }
    }

    if (-not $ready) {
        throw "The local web server did not start. Check $ErrorLog for details."
    }

    foreach ($assetDirectory in @("css", "images", "js", "media")) {
        $destinationDirectory = Join-Path $DocsPath $assetDirectory
        New-Item -ItemType Directory -Path $destinationDirectory | Out-Null
        Copy-Item `
            -Path (Join-Path $WwwRoot "$assetDirectory\*") `
            -Destination $destinationDirectory `
            -Recurse `
            -Force
    }

    $siteCssPath = Join-Path $DocsPath "css\site.css"
    if (Test-Path $siteCssPath) {
        $css = Get-Content -Path $siteCssPath -Raw
        $css = $css -replace 'url\("/', 'url("../'
        $css = $css -replace "url\('/", "url('../"
        Set-Content -Path $siteCssPath -Value $css -Encoding UTF8
    }

    foreach ($favicon in @("favicon.ico", "favicon.svg")) {
        $faviconPath = Join-Path $WwwRoot $favicon
        if (Test-Path $faviconPath) {
            Copy-Item -Path $faviconPath -Destination $DocsPath -Force
        }
    }

    $html = (Invoke-WebRequest -Uri "$Url/?preview=1" -UseBasicParsing).Content
    $html = $html -replace '(?s)\s*<script type="importmap">.*?</script>', ''
    $html = $html -replace 'data-gallery="/', 'data-gallery="./'
    $html = $html -replace 'href="/', 'href="./'
    $html = $html -replace 'src="/', 'src="./'
    $html = $html -replace 'url\("/', 'url("./'
    $html = $html -replace "url\('/", "url('./"
    $html = $html -replace '\./js/rsvp-config\.[^"]+\.js', './js/rsvp-config.js'
    $html = $html -replace '\./css/site\.[^"]+\.css', './css/site.css'
    $html = $html -replace '\./js/site\.[^"]+\.js', './js/site.js'
    $html = $html -replace '\./media/my-love\.[^"]+\.mp3', './media/my-love.mp3'
    $html = $html -replace '\./favicon\.[^"]+\.svg', './favicon.svg'
    $html = $html -replace '\./favicon\.[^"]+\.ico', './favicon.ico'

    Set-Content -Path (Join-Path $DocsPath "index.html") -Value $html -Encoding UTF8
    Set-Content -Path (Join-Path $DocsPath ".nojekyll") -Value "" -Encoding UTF8
    Copy-Item -Path (Join-Path $DocsPath "index.html") -Destination (Join-Path $DocsPath "404.html") -Force

    Write-Host "GitHub Pages files exported to: $DocsPath"
}
finally {
    if ($appProcess -and -not $appProcess.HasExited) {
        Stop-Process -Id $appProcess.Id -Force
        $appProcess.WaitForExit()
    }
}
