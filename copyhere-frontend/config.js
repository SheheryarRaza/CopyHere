// CopyHere Frontend Configuration
const CONFIG = {
    // Backend API Configuration
    API: {
        BASE_URL: 'https://localhost:7012/api',
        SIGNALR_URL: 'https://localhost:7012/clipboardHub'
    },
    
    // Application Settings
    APP: {
        NAME: 'CopyHere',
        VERSION: '1.0.0',
        POLLING_INTERVAL: 3000, // 3 seconds for clipboard updates (more responsive)
        ITEMS_PER_PAGE: 10,
        TOAST_DURATION: 3000 // 3 seconds
    },
    
    // UI Settings
    UI: {
        THEME: {
            PRIMARY_COLOR: '#667eea',
            SECONDARY_COLOR: '#764ba2',
            SUCCESS_COLOR: '#28a745',
            ERROR_COLOR: '#dc3545',
            INFO_COLOR: '#17a2b8'
        },
        ANIMATIONS: {
            ENABLED: true,
            DURATION: 300
        }
    }
};

// Export for use in other files
if (typeof module !== 'undefined' && module.exports) {
    module.exports = CONFIG;
} 