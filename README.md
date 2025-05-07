# HealthDevice Backend

## How to Run the Project

1. **Install Docker**  
   Make sure Docker and Docker Compose are installed on your machine. You can download Docker from the following link:  
   [Download Docker](https://www.docker.com/products/docker-desktop)

2. **Build and Start the Project**  
   Use the following command to build and start the project:
   ```sh
   docker-compose up --build
   ```
3. **Close the Project**
   Use the following command to exit the project:
   ```sh
   docker-compose down
   ```

## Docker with watch

Use the following command to run the project in watch mode which automatically reloads upon changes

```sh
docker-compose -f docker-compose.dev.yml up --build
```

## Swagger

To access the Swagger documentation, visit:

[http://localhost:5171/swagger/index.html](http://localhost:5171/swagger/index.html)

## How to update the migrations

1. **Navigate to project folder**

   ```sh
   cd HealthDevice

   ```

2. **Add new migration**
   Add the new migrations with the following command

   ```sh
   dotnet ef migrations add {newMigrationName}

   ```

3. **Apply the migrations**
   This happens the first time you call
   ```sh
   docker-compose up --build
   ```

## Delete the database

To delete the data in the database run

```sh
docker-compose down -v
```

## Update to the newest version of the HealthDevice

To update the project to the newest version run

```sh
docker pull ghcr.io/bachelor-alz/backend:latest

```
