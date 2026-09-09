param(
    [string]$SigningKey = "local-development-key-change-before-production-2026",
    [string]$Issuer = "integration-bff",
    [string]$Audience = "partner-api",
    [int]$LifetimeMinutes = 60
)

$header = @{ alg = "HS256"; typ = "JWT" } | ConvertTo-Json -Compress
$now = [DateTimeOffset]::UtcNow
$payload = @{
    sub = "local-partner-client"
    scope = "partner.transactions.write partner.transactions.read"
    iss = $Issuer
    aud = $Audience
    iat = $now.ToUnixTimeSeconds()
    exp = $now.AddMinutes($LifetimeMinutes).ToUnixTimeSeconds()
} | ConvertTo-Json -Compress

function ConvertTo-Base64Url([byte[]]$Bytes) {
    return [Convert]::ToBase64String($Bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_')
}

$encoding = [Text.Encoding]::UTF8
$unsignedToken = "$(ConvertTo-Base64Url $encoding.GetBytes($header)).$(ConvertTo-Base64Url $encoding.GetBytes($payload))"
$hmac = [Security.Cryptography.HMACSHA256]::new($encoding.GetBytes($SigningKey))
try {
    $signature = ConvertTo-Base64Url $hmac.ComputeHash($encoding.GetBytes($unsignedToken))
}
finally {
    $hmac.Dispose()
}

Write-Output "$unsignedToken.$signature"
