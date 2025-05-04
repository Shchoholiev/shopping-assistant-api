# shopping-assistant-api
.NET Back-end API with OpenAI integration for a Shopping Assistant that utilizes Natural Language Processing (NLP) technology to interpret user queries for products and gifts. Users interact through a chat-style interface to communicate their shopping requirements.

## Table of Contents
- [Features](#features)
- [Stack](#stack)
- [Installation](#installation)
  - [Prerequisites](#prerequisites)
  - [Setup Instructions](#setup-instructions)
- [Configuration](#configuration)

## Features
- Chat-style interface that processes natural language queries for product and gift recommendations.
- Integration with OpenAI API to generate intelligent product search responses.
- Server-Sent Events (SSE) streaming product search results to clients in real-time.
- Personal wishlist management with CRUD operations.
- Role and user management with authorization and JWT-based authentication.
- GraphQL API implemented with HotChocolate for flexible queries and mutations.
- Pagination support for roles, users, wishlists, messages, and products.

## Stack
- Language: C# (.NET 7)
- Frameworks & Libraries:
  - ASP.NET Core Web API
  - HotChocolate GraphQL
  - MongoDB with official C# driver
- Authentication & Security:
  - JWT Bearer Authentication
  - Password hashing with PBKDF2 (Rfc2898DeriveBytes)
- Cloud & DevOps:
  - Azure App Configuration
  - GitHub Actions for CI/CD workflows
- External APIs:
  - OpenAI API for natural language processing and chat completions
- Tools & Extensions:
  - Swagger / OpenAPI for API documentation
  - Server-Sent Events for realtime streaming responses
  - Visual Studio Code DevContainer for development environment

## Installation

### Prerequisites
- [.NET 7 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/7.0)
- [MongoDB](https://www.mongodb.com/try/download/community) instance (local or hosted)
- An OpenAI API key to access the OpenAI services

### Setup Instructions

1. Clone the repository:
   ```bash
   git clone https://github.com/Shchoholiev/shopping-assistant-api.git
   cd shopping-assistant-api
   ```

2. Navigate to the API project and restore dependencies:
   ```bash
   dotnet restore ShoppingAssistantApi.Api/ShoppingAssistantApi.Api.csproj
   ```

3. Build the solution:
   ```bash
   dotnet build ShoppingAssistantApi.sln
   ```

4. Configure the environment variables (see Configuration section).

5. Run the API locally:
   ```bash
   cd ShoppingAssistantApi.Api
   dotnet run
   ```

6. The API will start and Swagger UI will be available (usually at `https://localhost:7268/swagger`).

7. Use GraphQL Playground or REST clients to interact with the API.

## Configuration

The application uses configuration files and environment variables to configure key settings. Important configuration values are:

- **MongoDB connection:**
  - Set your MongoDB connection string (default database name is `ShoppingAssistant`) in `appsettings.json` or environment variable:
    ```json
    "ConnectionStrings": {
      "MongoDatabaseName": "ShoppingAssistant"
    }
    ```
  - The connection string URL is typically configured externally.

- **JWT Authentication:**
  Configure JWT validation parameters in your configuration with keys like:
  ```json
  "JsonWebTokenKeys": {
    "ValidateIssuer": true,
    "ValidateAudience": true,
    "ValidateLifetime": true,
    "ValidateIssuerSigningKey": true,
    "ValidIssuer": "<your-issuer>",
    "ValidAudience": "<your-audience>",
    "IssuerSigningKey": "<your-secret-key>"
  }
  ```

- **OpenAI API:**
  API endpoint and API key settings:
  ```json
  "OpenAi": {
    "ApiUrl": "https://api.openai.com/v1/chat/completions",
    "ApiKey": "<your_openai_api_key>"
  }
  ```
  Set the API key securely via environment variables or Azure App Configuration.
