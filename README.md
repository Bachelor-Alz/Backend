# Backend

## Setup
1. Install Docker and Docker Compose on your machine. [Download Docker](https://www.docker.com/products/docker-desktop)
2. Build and start the project:
   ```sh
   docker-compose up --build
   ```

## Update the Database
1. Delete the `migrations` folder inside the `backend` directory.
2. Create the initial migration:
   ```sh
   dotnet ef migrations add InitialCreate
   ```
3. Restart the project:
   ```sh
   docker-compose up --build
   ```

## Swagger
To access the Swagger documentation, visit:

[http://localhost:5171/swagger/index.html](http://localhost:5171/swagger/index.html)
