param (
    [string]$ModelUrl = "https://huggingface.co/Xenova/all-MiniLM-L6-v2/resolve/main/onnx/model.onnx",
    [string]$VocabUrl = "https://huggingface.co/Xenova/all-MiniLM-L6-v2/resolve/main/vocab.txt",
    [string]$OutputDir = "./Models/Local"
)

# Klasörü oluştur
if (!(Test-Path -Path $OutputDir)) {
    New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null
    Write-Host "Klasör oluşturuldu: $OutputDir"
}

# Modeli indir
$modelPath = Join-Path -Path $OutputDir -ChildPath "model.onnx"
if (!(Test-Path -Path $modelPath)) {
    Write-Host "Model indiriliyor: $ModelUrl"
    Invoke-WebRequest -Uri $ModelUrl -OutFile $modelPath
    Write-Host "Model indirildi: $modelPath"
} else {
    Write-Host "Model zaten var: $modelPath"
}

# Vocab dosyasını indir
$vocabPath = Join-Path -Path $OutputDir -ChildPath "vocab.txt"
if (!(Test-Path -Path $vocabPath)) {
    Write-Host "Vocab indiriliyor: $VocabUrl"
    Invoke-WebRequest -Uri $VocabUrl -OutFile $vocabPath
    Write-Host "Vocab indirildi: $vocabPath"
} else {
    Write-Host "Vocab zaten var: $vocabPath"
}

Write-Host "İşlem tamamlandı."
Write-Host "Config Ayarlarınız:"
Write-Host "--------------------"
Write-Host "LocalModelPath = '$([System.IO.Path]::GetFullPath($modelPath))'"
Write-Host "TokenizerPath = '$([System.IO.Path]::GetFullPath($vocabPath))'"
