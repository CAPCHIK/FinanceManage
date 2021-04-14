. ./environment.ps1
$stack = docker-compose -f .\docker-compose.yml -f .\docker-compose.production.yml config
$stack = $stack -replace '\d+\.\d+$', '''$0'''
$stack > .\stack.yml