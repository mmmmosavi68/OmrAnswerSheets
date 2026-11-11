
$projectRoot = $PSScriptRoot
$modelsPath = Join-Path $projectRoot "Models"

if (-not (Test-Path $modelsPath)) {
    New-Item -ItemType Directory -Path $modelsPath -Force | Out-Null
}

$files = @(
    "OmrRectangle.cs",
    "OmrLayoutProfile.cs"
)

foreach ($file in $files) {
    $path = Join-Path $modelsPath $file

    if (-not (Test-Path $path)) {
        New-Item -ItemType File -Path $path | Out-Null
        Write-Host "Created: $file" -ForegroundColor Green
    }
    else {
        Write-Host "Already exists: $file" -ForegroundColor Yellow
    }
}

Write-Host "`nOMR model files are ready." -ForegroundColor Cyan
