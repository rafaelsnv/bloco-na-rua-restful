<p align="center">
  <a href="" rel="noopener">
 <img width=200px height=200px src="https://i.imgur.com/6wj0hh6.jpg" alt="Project logo"></a>
</p>

<h3 align="center">BlocoNaRua - RESTful API</h3>

<div align="center">

[![Status](https://img.shields.io/badge/status-active-success.svg)]()
[![GitHub Issues](https://img.shields.io/github/issues/rafaelsnv/bloco-na-rua-restful.svg)](https://github.com/rafaelsnv/bloco-na-rua-restful/issues)
[![GitHub Pull Requests](https://img.shields.io/github/issues-pr/rafaelsnv/bloco-na-rua-restful.svg)](https://github.com/rafaelsnv/bloco-na-rua-restful/pulls)

</div>

---

<p align="center">
    A RESTful API to manage carnival blocks, members, and meetings.
    <br>
</p>

## 📝 Table of Contents

- [📝 Table of Contents](#-table-of-contents)
- [🧐 About ](#-about-)
- [🏁 Getting Started ](#-getting-started-)
  - [Prerequisites](#prerequisites)
  - [Installing](#installing)
- [🔧 Running the tests ](#-running-the-tests-)
- [🎈 Usage ](#-usage-)
- [🚀 Deployment ](#-deployment-)
- [⛏️ Built Using ](#️-built-using-)
- [✍️ Authors ](#️-authors-)

## 🧐 About <a name = "about"></a>

This project is a RESTful API developed in .NET 8.0, designed to manage entities related to carnival blocks. It offers functionalities for the creation, reading, updating, and deletion of carnival blocks, members, and meetings. The project's architecture is divided into layers (Core, Data, Domain, Restful, Services) to ensure modularity, maintainability, and scalability.

The main objective is to provide a robust and flexible solution for the organization and management of carnival events, allowing administrators to efficiently control blocks, their participants, and scheduled meetings.

## 🏁 Getting Started <a name = "getting_started"></a>

These instructions will get you a copy of the project up and running on your local machine for development and testing purposes. See [deployment](#deployment) for notes on how to deploy the project on a live system.

### Prerequisites

To run this project, you will need to have the following installed:

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- A code editor (e.g., [Visual Studio](https://visualstudio.microsoft.com/downloads/) or [VS Code](https://code.visualstudio.com/))
- [PostgreSQL](https://www.postgresql.org/) (provided by [Supabase](https://supabase.com/)) - Database
- A tool to manage the database (e.g., [DBeaver](https://dbeaver.io/), [pgAdmin](https://www.pgadmin.org/))

### Installing

Follow the steps below to set up and run the project on your local machine:

1.  **Clone o repositório:**

    ```bash
    git clone https://github.com/rafaelsnv/BlocoNaRua-Restful.git
    cd BlocoNaRua-Restful
    ```

2.  **Restore dependencies:**

    ```bash
    dotnet restore
    ```

3.  **Configure the database:**

    - Open the `appsettings.Development.json` file in `BlocoNaRua.Restful/`.
    - Update the database connection string to point to your local instance.
    - Run migrations to create the database schema:
      ```bash
      dotnet ef database update --project BlocoNaRua.Data
      ```

4.  **Run the application:**
    ```bash
    dotnet run --project BlocoNaRua.Restful
    ```

The API will be available at `https://localhost:7001` (or the port configured in `launchSettings.json`).

## 🔧 Running the tests <a name = "running_the_tests"></a>

To run the automated tests for the project, navigate to the project root and use the .NET CLI command:

```bash
dotnet test BlocoNaRua.Tests/BlocoNaRua.Tests.csproj
```

This will execute all unit and integration tests defined in the `BlocoNaRua.Tests` project.

## 🎈 Usage <a name="usage"></a>

The RESTful API can be consumed by any HTTP client. After starting the application, you can interact with the endpoints in the following ways:

- **Swagger UI:** The interactive API documentation is available via Swagger UI. Navigate to `https://localhost:7001/swagger` (or the configured port) in your browser to explore endpoints, view data models, and make requests directly from the interface.

- **HTTP Tools:** Use tools like [Postman](https://www.postman.com/), [Insomnia](https://insomnia.rest/), or `curl` to send HTTP requests to the API endpoints.

Examples of endpoints:

- `GET /api/carnivalblocks` - Returns all carnival blocks.
- `POST /api/members` - Creates a new member.
- `PUT /api/meetings/{id}` - Updates an existing meeting.

## 🚀 Deployment <a name = "deployment"></a>

To deploy this API in a production environment, follow these general steps:

1.  **Publish the application:**

    ```bash
    dotnet publish BlocoNaRua.Restful/BlocoNaRua.Restful.csproj -c Release -o ./publish
    ```

    This command will compile and publish the application to the `publish` folder in `Release` mode.

2.  **Environment configuration:**

    - Ensure that the target environment has the .NET 8.0 Runtime installed.
    - Configure environment variables or `appsettings.json` for production settings, especially the database connection string.

3.  **Hosting:**
    A aplicação pode ser hospedada em diversas plataformas, como:
    - **Web Servers:** IIS on Windows, Nginx or Apache on Linux.
    - **Cloud:** Azure App Service, AWS Elastic Beanstalk, Google Cloud Run.
    - **Containers:** Docker and Kubernetes.

Refer to your specific hosting platform's documentation for detailed instructions.

## ⛏️ Built Using <a name = "built_using"></a>

- [.NET 8.0](https://dotnet.microsoft.com/download/dotnet/8.0) - Framework
- [ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/?view=aspnetcore-8.0) - Web Framework
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/) - ORM
- [PostgreSQL](https://www.postgresql.org/) (provided by [Supabase](https://supabase.com/)) - Database
- [Swashbuckle.AspNetCore](https://github.com/domaindrivendev/Swashbuckle.AspNetCore) - Swagger/OpenAPI Documentation Generation
- [Asp.Versioning.Mvc](https://github.com/dotnet/aspnet-api-versioning) - API Versioning

## ✍️ Authors <a name = "authors"></a>

- [@rafaelsnv](https://github.com/rafaelsnv) - Initial Development

See also the list of [contributors](https://github.com/rafaelsnv/BlocoNaRua-Restful/contributors) who participated in this project.
