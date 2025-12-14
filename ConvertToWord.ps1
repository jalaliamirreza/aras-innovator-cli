# PowerShell script to convert HTML to Word document
# Requires Microsoft Word to be installed

param(
    [string]$HtmlFile = "Aras_Innovator_CLI_Production_Planning_FA.html",
    [string]$OutputFile = "Aras_Innovator_CLI_Production_Planning_FA.docx"
)

Write-Host "Converting HTML to Word document..." -ForegroundColor Cyan

try {
    # Check if HTML file exists
    if (-not (Test-Path $HtmlFile)) {
        Write-Host "Error: HTML file not found: $HtmlFile" -ForegroundColor Red
        exit 1
    }

    # Create Word application object
    $word = New-Object -ComObject Word.Application
    $word.Visible = $false
    
    Write-Host "Opening HTML file in Word..." -ForegroundColor Yellow
    
    # Open HTML file
    $doc = $word.Documents.Open((Resolve-Path $HtmlFile).Path)
    
    Write-Host "Saving as Word document..." -ForegroundColor Yellow
    
    # Save as Word document
    $outputPath = Join-Path (Get-Location) $OutputFile
    $doc.SaveAs([ref]$outputPath, [ref]16) # 16 = wdFormatDocumentDefault
    
    # Close document and Word
    $doc.Close()
    $word.Quit()
    
    # Release COM objects
    [System.Runtime.Interopservices.Marshal]::ReleaseComObject($doc) | Out-Null
    [System.Runtime.Interopservices.Marshal]::ReleaseComObject($word) | Out-Null
    [System.GC]::Collect()
    [System.GC]::WaitForPendingFinalizers()
    
    Write-Host "Success! Word document created: $OutputFile" -ForegroundColor Green
    Write-Host "Location: $outputPath" -ForegroundColor Cyan
    
} catch {
    Write-Host "Error: $_" -ForegroundColor Red
    Write-Host ""
    Write-Host "Manual conversion:" -ForegroundColor Yellow
    Write-Host "1. Open '$HtmlFile' in Microsoft Word" -ForegroundColor White
    Write-Host "2. File > Save As > Word Document (.docx)" -ForegroundColor White
    Write-Host "3. Save as '$OutputFile'" -ForegroundColor White
    exit 1
}


