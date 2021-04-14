# Personal finance management system

## How to build
1. Install .NET SDK 5
2. Install Docker
3. Restore tool
    ```
    dotnet tool restore
    ```
4. Build solution
    ```
    dotnet nuke
    ```
5. Run image in Docker
    ```
    docker-compose up
    ```
6. Open browser on [http://localhost:5000](http://localhost:5000)