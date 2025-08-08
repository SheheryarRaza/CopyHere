# CopyHere Frontend

A modern, responsive frontend for the CopyHere clipboard synchronization service.

## Features

### 🔐 Authentication

- User registration and login
- JWT token-based authentication
- Automatic token refresh
- Secure logout with token revocation

### 📋 Clipboard Management

- **Upload Content**: Add new text content to your clipboard
- **Current Clipboard**: View your latest clipboard content
- **Copy to System**: Copy clipboard content to your system clipboard
- **Clear All**: Remove all clipboard entries

### 📚 History & Organization

- **Clipboard History**: View all your clipboard entries with timestamps
- **Search & Filter**: Find specific entries by content or status
- **Pin/Unpin**: Mark important entries for quick access
- **Archive/Unarchive**: Organize entries by archiving them
- **Tags**: Add custom tags to organize your clipboard entries
- **Pagination**: Navigate through large history lists

### 📱 Device Management

- **Auto-Registration**: Devices are automatically registered on first use
- **Device List**: View all your registered devices
- **Last Seen**: Track when devices were last active
- **Manual Registration**: Add devices manually through the interface
- **Device Removal**: Remove unwanted devices

### 🎨 User Interface

- **Modern Design**: Clean, responsive interface
- **Real-time Updates**: Automatic polling for new content (3-second intervals)
- **Cross-Tab Updates**: Clipboard content updates automatically across all tabs
- **Visual Indicators**: "NEW" badge appears when content changes
- **Toast Notifications**: User-friendly feedback messages
- **Mobile Responsive**: Works on all device sizes
- **Keyboard Shortcuts**: Ctrl+Enter to quickly upload content

## Technical Details

### Auto-Device Registration

The frontend automatically handles device registration:

1. **First Login**: If no devices exist, a new device is automatically registered
2. **Existing Devices**: If devices already exist, the first available device is used
3. **Cross-Session Validation**: Device IDs are validated against the current user's devices
4. **Manual Override**: Users can manually register additional devices
5. **Multi-Instance Support**: Each browser instance properly validates its device ownership

### API Integration

- **Base URL**: `https://localhost:7012/api`
- **Authentication**: Bearer token in Authorization header
- **Error Handling**: Comprehensive error handling with user feedback
- **Token Refresh**: Automatic token refresh on expiration
- **Real-time Polling**: 3-second intervals for clipboard updates
- **Change Detection**: Smart content change detection to avoid unnecessary updates

### Browser Compatibility

- Modern browsers with ES6+ support
- Clipboard API for system clipboard integration
- Local Storage for persistent authentication

## Setup Instructions

### Prerequisites

- Backend API running on `https://localhost:7012`
- Modern web browser
- Local web server (for CORS compliance)

### Quick Start

1. **Start Backend**: Ensure your CopyHere backend is running
2. **Start Frontend**: Use one of the provided startup scripts:

   ```bash
   # Windows
   start.bat

   # PowerShell
   start.ps1

   # Or manually with Python
   python -m http.server 8080
   ```

3. **Access**: Open `http://localhost:8080` in your browser
4. **Register/Login**: Create an account or login with existing credentials

### Development

- **Files**: All frontend code is in vanilla HTML, CSS, and JavaScript
- **Configuration**: Edit `config.js` to modify API endpoints and settings
- **Styling**: Modify `styles.css` for custom theming
- **Functionality**: Extend `app.js` for additional features

## Troubleshooting

### Common Issues

1. **CORS Errors**: Ensure you're running via a web server, not opening files directly
2. **Authentication Failures**: Check that the backend is running and accessible
3. **Device Registration**: Devices are auto-registered; manual registration is optional
4. **Delete Operations**: Ensure you have proper permissions for the content
5. **Multi-Instance Issues**: If you get "device does not belong to user" errors when opening multiple browser instances:
   - The application now automatically validates device ownership
   - Each instance will use a valid device for the current user
   - If issues persist, try refreshing the page or clearing browser storage

### Debug Mode

The application includes comprehensive console logging:

- API request/response logging
- Error details for troubleshooting
- Device registration status
- Upload operation details

## API Endpoints Used

### Authentication

- `POST /auth/register` - User registration
- `POST /auth/login` - User login
- `POST /auth/refresh` - Token refresh
- `POST /auth/revoke` - Token revocation

### Clipboard

- `GET /clipboard/latest` - Get latest clipboard entry
- `POST /clipboard` - Upload new clipboard content
- `GET /clipboard/history` - Get clipboard history
- `DELETE /clipboard/{id}` - Delete specific entry
- `POST /clipboard/restore/{id}` - Restore entry
- `PUT /clipboard/{id}/pin` - Pin entry
- `PUT /clipboard/{id}/unpin` - Unpin entry
- `PUT /clipboard/{id}/archive` - Archive entry
- `PUT /clipboard/{id}/unarchive` - Unarchive entry
- `PUT /clipboard/{id}/tags` - Update tags
- `DELETE /clipboard/clear` - Clear all entries

### Devices

- `GET /clipboard/devices` - Get user devices
- `POST /clipboard/devices/register` - Register new device
- `DELETE /clipboard/devices/{id}` - Remove device

## License

This frontend is part of the CopyHere project and follows the same licensing terms.
