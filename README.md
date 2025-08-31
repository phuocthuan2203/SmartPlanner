# SmartPlanner

A comprehensive task and project management application built with ASP.NET Core MVC.

## Features

- User authentication and registration
- Task management with progress tracking
- Subject/project organization
- Dashboard with overview analytics
- Responsive web interface

## Technology Stack

- **Backend**: ASP.NET Core MVC
- **Database**: Entity Framework Core with SQLite/PostgreSQL
- **Frontend**: HTML, CSS, JavaScript with Bootstrap
- **Authentication**: JWT tokens

## Setup Instructions

### Prerequisites
- .NET 8.0 SDK
- PostgreSQL (optional, SQLite is used by default)

### Getting Started

1. Clone the repository:
   ```bash
   git clone <your-repo-url>
   cd SmartPlanner
   ```

2. Set up configuration files:
   ```bash
   cp src/appsettings.json.template src/appsettings.json
   cp src/appsettings.Development.json.template src/appsettings.Development.json
   ```

3. Update the configuration files with your actual values:
   - Edit `src/appsettings.json` with your database connection strings and JWT secret key
   - Generate a secure JWT secret key (minimum 32 characters)

4. Restore dependencies:
   ```bash
   dotnet restore
   ```

5. Run database migrations:
   ```bash
   cd src
   dotnet ef database update
   ```

6. Run the application:
   ```bash
   dotnet run
   ```

## Configuration

### Database
The application supports both SQLite (default) and PostgreSQL. Update the connection strings in `appsettings.json`:

- **SQLite**: `"DefaultConnection": "Data Source=smartplanner.db"`
- **PostgreSQL**: Update the `PostgreSQLConnection` with your database details

### JWT Settings
Generate a secure secret key for JWT token signing. You can use online tools or run:
```bash
openssl rand -hex 32
```

## Security Notes
- Never commit `appsettings.json` or `appsettings.Development.json` to version control
- Use environment variables or Azure Key Vault for production secrets
- Regularly rotate JWT secret keys

## Project Structure

```
SmartPlanner/
├── src/
│   ├── Application/          # Application layer (DTOs, Services, Mappers)
│   ├── Controllers/          # MVC Controllers
│   ├── Domain/              # Domain entities and value objects
│   ├── Infrastructure/      # Data access and external services
│   ├── Views/               # Razor views
│   ├── wwwroot/            # Static files (CSS, JS, images)
│   └── Migrations/         # Entity Framework migrations
└── README.md
```

## Development
- The application uses Entity Framework Core for data access
- Authentication is handled via JWT tokens
- The project follows Clean Architecture principles

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License.