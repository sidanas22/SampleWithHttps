# Start Docker Compose with --remove-orphans and -d options
docker compose --env-file ./docker.env.dev -f docker-compose.yml -f docker-compose.override.yml -f docker-compose.dev.debug.yml -p sampleproject --ansi never up --build --remove-orphans -d

# Function to start a new PowerShell window for service logs
function Start-ServiceLog {
    param (
        [string]$serviceName,
        [string]$windowTitle = "$serviceName Logs"
    )
    Start-Process powershell -ArgumentList "-NoExit", "-Command", "`$Host.UI.RawUI.WindowTitle = '$windowTitle'; docker compose -p sampleproject logs -f $serviceName" -PassThru
}

# Define services
$services = @(
    @{ Name = "samplewithhttps" },
    @{ Name = "backend" },
    @{ Name = "cache" }
)

# Start new PowerShell windows for each service log with custom titles
$processIds = @()
foreach ($service in $services) {
    $process = Start-ServiceLog -serviceName $service.Name
    $processIds += $process.Id
}

# Save the process IDs to a file for later use
$processIds | Out-File -FilePath "docker_log_processes.txt"