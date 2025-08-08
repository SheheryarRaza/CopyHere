# CopyHere - Cross-Device Clipboard Synchronization

A clipboard synchronization service that allows users to share clipboard content across multiple devices in real-time. Built with ASP.NET Core backend and vanilla JavaScript frontend.

## 🚀 Features

- **Cross-Device Sync**: Share clipboard content across multiple devices
- **Real-time Updates**: Automatic synchronization with 3-second polling
- **Multi-Content Support**: Text, images, files, and HTML content
- **Secure Authentication**: JWT-based authentication with refresh tokens
- **Clipboard History**: Complete history with pin, archive, and tag features
- **Device Management**: Register, track, and manage multiple devices

## 🛠️ Tech Stack

**Backend**: ASP.NET Core 8.0, SQL Server, Entity Framework Core, JWT Authentication  
**Frontend**: Vanilla JavaScript, CSS3, Local Storage, Fetch API

## 📋 Prerequisites

- .NET 8.0 SDK
- SQL Server (Express or higher)
- Modern Web Browser
- Python or Node.js (for local web server)

## 🚀 Quick Start

### 1. Database Setup

Update connection string in `CopyHere.Api/appsettings.json`:

```json
"ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER; Database=CopyHere; User ID=YOUR_USER; Password=YOUR_PASSWORD; TrustServerCertificate=True;"
}
```

### 2. Backend Setup

```bash
cd CopyHere
dotnet restore
dotnet build
cd CopyHere.Api
dotnet ef database update
dotnet run
```

API available at: `https://localhost:7012`

### 3. Frontend Setup

```bash
cd copyhere-frontend
python -m http.server 8080
# or use: start.bat / start.ps1
```

Frontend available at: `http://localhost:8080`

### 4. First Use

1. Open `http://localhost:8080`
2. Register/login
3. Start sharing clipboard content!

## 🔧 Key API Endpoints

**Authentication**: `/api/auth/register`, `/api/auth/login`, `/api/auth/refresh`  
**Clipboard**: `/api/clipboard/latest`, `/api/clipboard`, `/api/clipboard/history`  
**Devices**: `/api/clipboard/devices`, `/api/clipboard/devices/register`

## 🎯 Usage

- **Upload Content**: Use the text area to add new clipboard content
- **View History**: Check history tab for all entries
- **Organize**: Pin, archive, and tag your clipboard entries
- **Multi-Device**: Content automatically syncs across all devices
- **Keyboard Shortcuts**: `Ctrl + Enter` to upload, `Ctrl + C/V` for system clipboard

## 🐛 Troubleshooting

**CORS Errors**: Always run frontend via web server, not file:// protocol  
**Database Issues**: Verify connection string and run `dotnet ef database update`  
**Authentication**: Check backend is running and accessible  
**Multi-Instance**: Each browser instance validates device ownership

## 📁 Project Structure

```
CopyHere/
├── CopyHere.Api/           # Web API layer
├── CopyHere.Application/   # Business logic & services
├── CopyHere.Core/         # Domain entities & interfaces
├── CopyHere.Infrastructure/ # Data access & external services
└── copyhere-frontend/     # Frontend application
```

## 📝 API Documentation

- **Swagger UI**: `https://localhost:7012/swagger`
- **Frontend Docs**: See `copyhere-frontend/README.md`

## 🔮 Future Enhancements

- SignalR integration for real-time WebSocket updates
- Mobile applications
- Offline support
- Two-factor authentication

---

**CopyHere** - Seamlessly sync your clipboard across all devices! 📋✨
