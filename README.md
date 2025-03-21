# Backend
## Setup
1. Install Docker and Docker Compose on your machine. Link to download: [Docker](https://www.docker.com/products/docker-desktop)
2. Use Docker-Compose up --build to build the project.

## Update the database
1. Delete the migrations folder in the backend folder.
2. Run the command dotnet ef migrations add InitialCreate to create the initial migration.
3. Run the program again by using Docker-Compose up --build.

## Swagger
1. To access the Swagger documentation, go to the URL: http://localhost:5171/swagger/index.html
