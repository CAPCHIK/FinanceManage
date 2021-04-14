# Personal finance management system

## How to build
1. Install .NET SDK 5
2. Install Docker
3. Restore tool
    ```bash
    dotnet tool restore
    ```
4. Build solution
    ```bash
    dotnet nuke
    ```
5. Run image in Docker
    ```bash
    docker-compose up
    ```
6. Open browser on [http://localhost:5000](http://localhost:5000)

## How to create deploy stack file
1. Create file `environment.ps1`
    ```powershell
    $Env:POSTGRES_CONNECTION_STRING="postgres connection string on target server"
    ```
2. Create `stack.yml` file
    ```powershell
    ./genStack.ps1
    ```
3. Use file `stack.yml` to deploy docker stack 