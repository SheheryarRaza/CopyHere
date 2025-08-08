# CopyHere - Cross-Device Clipboard Synchronization Platform

[![.NET](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![Status](https://img.shields.io/badge/Status-Active-brightgreen.svg)]()

A modern, secure clipboard synchronization service that enables seamless sharing of clipboard content across multiple devices in real-time. Built with a clean architecture using ASP.NET Core 8.0 backend and vanilla JavaScript frontend.

## 🌟 Features

### Core Functionality

- **🔄 Real-time Sync**: Instant clipboard synchronization across all registered devices
- **📱 Multi-Device Support**: Register and manage unlimited devices per user
- **🔐 Secure Authentication**: JWT-based authentication with refresh token mechanism
- **📋 Rich Content Support**: Text, images, files, and HTML content synchronization
- **⏱️ Automatic Polling**: 3-second polling interval for seamless updates

### Advanced Features

- **📚 Clipboard History**: Complete history with search and filtering capabilities
- **🏷️ Tag Management**: Organize clipboard entries with custom tags
- **📌 Pin & Archive**: Pin important entries and archive old ones
- **🔍 Content Search**: Search through clipboard history by content and tags
- **📊 Device Management**: View and manage all registered devices
- **🛡️ Rate Limiting**: Built-in protection against abuse

## 🏗️ Architecture

CopyHere follows Clean Architecture principles with a layered approach:

```
CopyHere/
├── 📁 CopyHere.Api/              # Presentation Layer (Web API)
│   ├── Controllers/              # API endpoints
│   ├── Hubs/                     # SignalR hubs for real-time communication
│   └── Program.cs                # Application entry point
├── 📁 CopyHere.Application/      # Application Layer (Business Logic)
│   ├── Services/                 # Business services
│   ├── DTOs/                     # Data Transfer Objects
│   ├── Validators/               # FluentValidation rules
│   └── Mappings/                 # AutoMapper profiles
├── 📁 CopyHere.Core/             # Domain Layer (Entities & Interfaces)
│   ├── Entity/                   # Domain entities
│   ├── Interfaces/               # Repository and service contracts
│   └── Enumerations/             # Domain enums
├── 📁 CopyHere.Infrastructure/   # Infrastructure Layer (Data & External)
│   ├── Data/                     # Entity Framework context & configs
│   ├── Repository/               # Repository implementations
│   ├── Authentication/           # JWT service implementation
│   └── Migrations/               # Database migrations
└── 📁 copyhere-frontend/         # Frontend Application
    ├── index.html                # Main application page
    ├── app.js                    # Core application logic
    ├── styles.css                # Styling
    └── config.js                 # Configuration
```

## 🛠️ Technology Stack

### Backend

- **Framework**: ASP.NET Core 8.0
- **Database**: SQL Server with Entity Framework Core
- **Authentication**: JWT Bearer tokens with refresh tokens
- **Real-time**: SignalR for WebSocket communication
- **Validation**: FluentValidation
- **Mapping**: AutoMapper
- **Documentation**: Swagger/OpenAPI

### Frontend

- **Language**: Vanilla JavaScript (ES6+)
- **Styling**: CSS3 with modern design patterns
- **HTTP Client**: Fetch API
- **Real-time**: SignalR JavaScript client
- **Storage**: Local Storage for session management

### Development Tools

- **Package Manager**: NuGet (.NET), npm (Frontend)
- **Database Migrations**: Entity Framework Core
- **API Testing**: Built-in HTTP files
- **Development Server**: http-server (Node.js) or Python

## 📋 Prerequisites

Before running CopyHere, ensure you have the following installed:

- **.NET 8.0 SDK** - [Download here](https://dotnet.microsoft.com/download/dotnet/8.0)
- **SQL Server** - Express, Developer, or higher edition
- **Node.js** (optional) - For frontend development server
- **Python** (optional) - Alternative frontend server
- **Modern Web Browser** - Chrome, Firefox, Safari, or Edge

## 🚀 Quick Start Guide

### 1. Clone and Setup

```bash
git clone <repository-url>
cd CopyHere
```

### 2. Database Configuration

Update the connection string in `CopyHere.Api/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER; Database=CopyHere; User ID=YOUR_USER; Password=YOUR_PASSWORD; TrustServerCertificate=True;"
  }
}
```

### 3. Backend Setup

```bash
# Restore dependencies
dotnet restore

# Build the solution
dotnet build

# Navigate to API project
cd CopyHere.Api

# Run database migrations
dotnet ef database update

# Start the API server
dotnet run
```

The API will be available at: `https://localhost:7012`

### 4. Frontend Setup

```bash
# Navigate to frontend directory
cd copyhere-frontend

# Option 1: Using Node.js (recommended)
npm install
npm start

# Option 2: Using Python
python -m http.server 8080

# Option 3: Using provided scripts
# Windows: start.bat
# PowerShell: start.ps1
```

The frontend will be available at: `http://localhost:8080`

### 5. First Use

1. Open `http://localhost:8080` in your browser
2. Register a new account or login with existing credentials
3. Start sharing clipboard content across your devices!

## 🔧 Configuration

### Application Settings

Key configuration options in `appsettings.json`:

```json
{
  "ApplicationSettings": {
    "JwtSecret": "YOUR_SECURE_JWT_SECRET_KEY",
    "JwtTokenExpiryMinutes": 10,
    "RefreshTokenExpiryDays": 7
  }
}
```

### Environment Variables

For production, use environment variables:

```bash
# Database
ConnectionStrings__DefaultConnection="Server=prod-server;Database=CopyHere;..."

# JWT
ApplicationSettings__JwtSecret="your-production-secret"
ApplicationSettings__JwtTokenExpiryMinutes=15
ApplicationSettings__RefreshTokenExpiryDays=30
```

## 📚 API Documentation

### Authentication Endpoints

| Method | Endpoint             | Description          |
| ------ | -------------------- | -------------------- |
| `POST` | `/api/auth/register` | Register new user    |
| `POST` | `/api/auth/login`    | User login           |
| `POST` | `/api/auth/refresh`  | Refresh JWT token    |
| `POST` | `/api/auth/revoke`   | Revoke refresh token |

### Clipboard Endpoints

| Method   | Endpoint                   | Description                  |
| -------- | -------------------------- | ---------------------------- |
| `GET`    | `/api/clipboard/latest`    | Get latest clipboard entry   |
| `POST`   | `/api/clipboard`           | Upload new clipboard content |
| `GET`    | `/api/clipboard/history`   | Get clipboard history        |
| `PUT`    | `/api/clipboard/{id}/tags` | Update entry tags            |
| `DELETE` | `/api/clipboard/{id}`      | Delete clipboard entry       |

### Device Management

| Method | Endpoint                          | Description         |
| ------ | --------------------------------- | ------------------- |
| `GET`  | `/api/clipboard/devices`          | Get user devices    |
| `POST` | `/api/clipboard/devices/register` | Register new device |

### Interactive Documentation

- **Swagger UI**: `https://localhost:7012/swagger`
- **API Testing**: Use the provided `CopyHere.Api.http` file

## 🎯 Usage Guide

### Basic Operations

1. **Upload Content**: Type or paste content in the text area and click "Upload"
2. **View History**: Switch to the History tab to see all clipboard entries
3. **Search**: Use the search bar to find specific content or tags
4. **Organize**: Pin important entries, archive old ones, and add tags

### Keyboard Shortcuts

- `Ctrl + Enter`: Upload clipboard content
- `Ctrl + C/V`: System clipboard operations
- `Ctrl + F`: Search in history
- `Escape`: Clear search or close modals

### Multi-Device Setup

1. Register/login on each device
2. Content automatically syncs across all devices
3. Each browser instance is treated as a separate device
4. View all devices in the Devices tab

## 🔒 Security Features

- **JWT Authentication**: Secure token-based authentication
- **Refresh Tokens**: Automatic token renewal
- **Rate Limiting**: Protection against abuse
- **CORS Configuration**: Secure cross-origin requests
- **Input Validation**: Comprehensive validation using FluentValidation
- **SQL Injection Protection**: Entity Framework Core parameterized queries

## 🐛 Troubleshooting

### Common Issues

**CORS Errors**

- Ensure frontend is served via HTTP server, not file:// protocol
- Check CORS configuration in `Program.cs`

**Database Connection Issues**

- Verify SQL Server is running
- Check connection string in `appsettings.json`
- Run `dotnet ef database update` to ensure migrations are applied

**Authentication Problems**

- Verify backend is running on correct port
- Check JWT secret configuration
- Clear browser local storage if needed

**Real-time Updates Not Working**

- Check SignalR hub configuration
- Verify WebSocket support in browser
- Check network connectivity

### Development Debugging

```bash
# Check API logs
dotnet run --environment Development

# Verify database migrations
dotnet ef migrations list

# Test API endpoints
dotnet test
```

## 🧪 Development

### Project Structure

The solution follows Clean Architecture with clear separation of concerns:

- **Domain Layer** (`CopyHere.Core`): Business entities and interfaces
- **Application Layer** (`CopyHere.Application`): Business logic and use cases
- **Infrastructure Layer** (`CopyHere.Infrastructure`): Data access and external services
- **Presentation Layer** (`CopyHere.Api`): API controllers and SignalR hubs

### Adding New Features

1. **Domain Changes**: Update entities in `CopyHere.Core/Entity/`
2. **Business Logic**: Add services in `CopyHere.Application/Services/`
3. **Data Access**: Implement repositories in `CopyHere.Infrastructure/Repository/`
4. **API Endpoints**: Add controllers in `CopyHere.Api/Controllers/`
5. **Database**: Create and run migrations

## 🔮 Roadmap

### Planned Features

- [ ] **Mobile Applications**: iOS and Android apps
- [ ] **Offline Support**: Local caching and sync when online
- [ ] **Two-Factor Authentication**: Enhanced security
- [ ] **File Sharing**: Direct file upload and sharing
- [ ] **Collaboration**: Share clipboard with other users
- [ ] **Analytics**: Usage statistics and insights

## 🤝 Contributing

We welcome contributions! Please follow these steps:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

**CopyHere** - Seamlessly sync your clipboard across all devices! 📋✨

_Built with ❤️ using ASP.NET Core and modern web technologies_
