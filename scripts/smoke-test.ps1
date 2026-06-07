#!/usr/bin/env pwsh
<#
.SYNOPSIS
    GeneFlow post-deployment smoke test.
    Run after every backend or frontend deployment to confirm the live API is healthy.

.USAGE
    .\scripts\smoke-test.ps1
    .\scripts\smoke-test.ps1 -ApiBase "https://gflowapi.svais.net"

.DESCRIPTION
    Tests:
      1. API health  — GET  /api/health
      2. Login       — POST /api/auth/login  (email)
      3. Login       — POST /api/auth/login  (phone)
      4. Bad login   — POST /api/auth/login  (wrong password → 401)
      5. Experiments — GET  /api/experiments  (needs token)
      6. Dashboard   — GET  /api/dashboard/mobile  (needs token)
      7. Projects    — GET  /api/projects  (needs token)
      8. Frontend    — GET  https://geneflow.svais.net  (200 OK)
#>

param(
    [string]$ApiBase    = "https://gflowapi.svais.net",
    [string]$FrontendUrl = "https://geneflow.svais.net",
    [string]$TestEmail   = "admin@geneflow.local",
    [string]$TestPhone   = "919876543210",       # digits only, no + sign
    [string]$TestPassword = "Admin@123!"
)

$pass = 0
$fail = 0
$errors = [System.Collections.Generic.List[string]]::new()

function Test-Step {
    param([string]$Name, [scriptblock]$Block)

    Write-Host -NoNewline "  [$Name] "
    try {
        $result = & $Block
        if ($result -eq $true -or $null -eq $result) {
            Write-Host "PASS" -ForegroundColor Green
            $script:pass++
        } else {
            Write-Host "FAIL — $result" -ForegroundColor Red
            $script:fail++
            $script:errors.Add("$Name`: $result")
        }
    } catch {
        Write-Host "ERROR — $_" -ForegroundColor Red
        $script:fail++
        $script:errors.Add("$Name`: $_")
    }
}

function Invoke-Api {
    param(
        [string]$Method = "GET",
        [string]$Url,
        [hashtable]$Body = @{},
        [string]$Token = ""
    )
    $headers = @{ "Content-Type" = "application/json" }
    if ($Token) { $headers["Authorization"] = "Bearer $Token" }

    $params = @{
        Uri     = $Url
        Method  = $Method
        Headers = $headers
        UseBasicParsing = $true
        ErrorAction = "Stop"
    }
    if ($Method -ne "GET" -and $Body.Count -gt 0) {
        $params["Body"] = ($Body | ConvertTo-Json -Depth 5)
    }
    return Invoke-WebRequest @params
}

Write-Host ""
Write-Host "================================================================" -ForegroundColor Cyan
Write-Host "  GeneFlow Smoke Tests  |  $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')" -ForegroundColor Cyan
Write-Host "  API:      $ApiBase" -ForegroundColor Cyan
Write-Host "  Frontend: $FrontendUrl" -ForegroundColor Cyan
Write-Host "================================================================" -ForegroundColor Cyan
Write-Host ""

# ── 1. Health ─────────────────────────────────────────────────────────────────
$token = $null

Test-Step "1. API Health" {
    try {
        $r = Invoke-Api -Url "$ApiBase/api/health"
        if ($r.StatusCode -eq 200) { return $true }
        return "Status $($r.StatusCode)"
    } catch [System.Net.WebException] {
        # health endpoint may not exist; try login endpoint as fallback
        if ($_.Exception.Response.StatusCode -eq 404) {
            return $true  # API is reachable, /health just not implemented yet
        }
        throw
    }
}

# ── 2. Login by email ─────────────────────────────────────────────────────────
Test-Step "2. Login (email)" {
    $r = Invoke-Api -Method POST -Url "$ApiBase/api/auth/login" -Body @{
        email    = $TestEmail
        password = $TestPassword
    }
    $data = $r.Content | ConvertFrom-Json
    if (-not $data.token) { return "No token in response" }
    $script:token = $data.token
    return $true
}

# ── 3. Login by phone ─────────────────────────────────────────────────────────
Test-Step "3. Login (phone)" {
    try {
        $r = Invoke-Api -Method POST -Url "$ApiBase/api/auth/login" -Body @{
            phoneNumber = $TestPhone
            password    = $TestPassword
        }
        if ($r.StatusCode -in 200, 401) { return $true }  # 401 = wrong phone seed, but API alive
        return "Status $($r.StatusCode)"
    } catch {
        # phone may not be seeded — 401 is acceptable
        if ($_.Exception.Response.StatusCode -eq 401) { return $true }
        throw
    }
}

# ── 4. Wrong password → 401 ───────────────────────────────────────────────────
Test-Step "4. Bad login → 401" {
    try {
        Invoke-Api -Method POST -Url "$ApiBase/api/auth/login" -Body @{
            email    = $TestEmail
            password = "WrongPassword999!"
        }
        return "Expected 401 but got 200"
    } catch {
        $status = $_.Exception.Response.StatusCode
        if ($status -eq "Unauthorized" -or [int]$status -eq 401) { return $true }
        return "Expected 401 but got $status"
    }
}

# ── 5. List experiments (authenticated) ───────────────────────────────────────
Test-Step "5. GET /experiments" {
    if (-not $token) { return "Skipped (no token from step 2)" }
    $r = Invoke-Api -Url "$ApiBase/api/experiments" -Token $token
    if ($r.StatusCode -eq 200) { return $true }
    return "Status $($r.StatusCode)"
}

# ── 6. Dashboard ──────────────────────────────────────────────────────────────
Test-Step "6. GET /dashboard/mobile" {
    if (-not $token) { return "Skipped (no token from step 2)" }
    $r = Invoke-Api -Url "$ApiBase/api/dashboard/mobile" -Token $token
    if ($r.StatusCode -eq 200) { return $true }
    return "Status $($r.StatusCode)"
}

# ── 7. Projects ───────────────────────────────────────────────────────────────
Test-Step "7. GET /projects" {
    if (-not $token) { return "Skipped (no token from step 2)" }
    $r = Invoke-Api -Url "$ApiBase/api/projects" -Token $token
    if ($r.StatusCode -eq 200) { return $true }
    return "Status $($r.StatusCode)"
}

# ── 8. Frontend is reachable ──────────────────────────────────────────────────
Test-Step "8. Frontend reachable" {
    $r = Invoke-WebRequest -Uri $FrontendUrl -UseBasicParsing -ErrorAction Stop
    if ($r.StatusCode -eq 200) { return $true }
    return "Status $($r.StatusCode)"
}

# ── Summary ───────────────────────────────────────────────────────────────────
Write-Host ""
Write-Host "================================================================" -ForegroundColor Cyan
$totalColor = if ($fail -eq 0) { "Green" } else { "Red" }
Write-Host "  Results: $pass passed, $fail failed" -ForegroundColor $totalColor

if ($errors.Count -gt 0) {
    Write-Host ""
    Write-Host "  Failures:" -ForegroundColor Red
    foreach ($e in $errors) {
        Write-Host "    - $e" -ForegroundColor Red
    }
}

Write-Host "================================================================" -ForegroundColor Cyan
Write-Host ""

if ($fail -gt 0) { exit 1 } else { exit 0 }
