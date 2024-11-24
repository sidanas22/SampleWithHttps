# Stop and remove the containers.
docker compose -p sampleproject down

# Read the process IDs from the file
$processIds = Get-Content -Path "docker_log_processes.txt"

# Stop the processes
foreach ($processId in $processIds) {
    Stop-Process -Id $processId -Force
}

# Optionally, remove the file after stopping the processes
Remove-Item -Path "docker_log_processes.txt"