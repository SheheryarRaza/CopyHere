// Global state
let currentUser = null;
let authToken = localStorage.getItem("authToken");
let refreshToken = localStorage.getItem("refreshToken");
let currentDeviceId = localStorage.getItem("currentDeviceId");
let currentPage = 1;
let itemsPerPage = CONFIG.APP.ITEMS_PER_PAGE;
let clipboardHistory = [];
let devices = [];
let signalRConnection = null;
let isRefreshingToken = false;

// DOM Elements
const loadingScreen = document.getElementById("loading-screen");
const authContainer = document.getElementById("auth-container");
const mainContainer = document.getElementById("main-container");
const loginForm = document.getElementById("login-form");
const registerForm = document.getElementById("register-form");
const logoutBtn = document.getElementById("logout-btn");
const userEmailSpan = document.getElementById("user-email");
const navBtns = document.querySelectorAll(".nav-btn");
const tabContents = document.querySelectorAll(".tab-content");
const currentClipboard = document.getElementById("current-clipboard");
const copyBtn = document.getElementById("copy-btn");
const clearBtn = document.getElementById("clear-btn");
const historyList = document.getElementById("history-list");
const searchInput = document.getElementById("search-input");
const filterSelect = document.getElementById("filter-select");
const prevPageBtn = document.getElementById("prev-page");
const nextPageBtn = document.getElementById("next-page");
const pageInfo = document.getElementById("page-info");
const devicesList = document.getElementById("devices-list");
const registerDeviceBtn = document.getElementById("register-device-btn");
const deviceModal = document.getElementById("device-modal");
const deviceForm = document.getElementById("device-form");
const cancelDeviceBtn = document.getElementById("cancel-device");

// Upload functionality elements
const clipboardTextInput = document.getElementById("clipboard-text-input");
const uploadBtn = document.getElementById("upload-btn");
const clearInputBtn = document.getElementById("clear-input-btn");

// Initialize the application
document.addEventListener("DOMContentLoaded", function () {
  initializeApp();
  setupEventListeners();
});

// Test backend connectivity
async function testBackendConnection() {
  try {
    const response = await fetch(`${CONFIG.API.BASE_URL}/auth/login`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify({ email: "test", password: "test" }),
    });
    return response.status !== 0; // If we get any response, backend is reachable
  } catch (error) {
    return false;
  }
}

// Initialize the application
async function initializeApp() {
  try {
    if (authToken) {
      // Try to validate the token
      const isValid = await validateToken();
      if (isValid) {
        showMainApp();
        await loadInitialData();
        initializeSignalR();
      } else {
        showAuthScreen();
      }
    } else {
      showAuthScreen();
    }
  } catch (error) {
    console.error("Error initializing app:", error);
    // Show auth screen even if there are network errors
    showAuthScreen();
    if (error.message.includes("Network error")) {
      showToast(
        "Unable to connect to server. Please check your connection.",
        "error"
      );
    }
  } finally {
    hideLoadingScreen();
  }
}

// Setup event listeners
function setupEventListeners() {
  // Auth form events
  document
    .getElementById("show-register")
    .addEventListener("click", showRegisterForm);
  document
    .getElementById("show-login")
    .addEventListener("click", showLoginForm);
  loginForm.addEventListener("submit", handleLogin);
  registerForm.addEventListener("submit", handleRegister);
  logoutBtn.addEventListener("click", handleLogout);

  // Navigation events
  navBtns.forEach((btn) => {
    btn.addEventListener("click", () => switchTab(btn.dataset.tab));
  });

  // Clipboard events
  copyBtn.addEventListener("click", copyToClipboard);
  clearBtn.addEventListener("click", clearAllClipboard);

  // Upload functionality events
  uploadBtn.addEventListener("click", handleUploadClipboard);
  clearInputBtn.addEventListener("click", clearTextInput);

  // Keyboard shortcuts for upload
  clipboardTextInput.addEventListener("keydown", (e) => {
    if (e.ctrlKey && e.key === "Enter") {
      e.preventDefault();
      handleUploadClipboard();
    }
  });

  // History events
  searchInput.addEventListener("input", debounce(filterHistory, 300));
  filterSelect.addEventListener("change", filterHistory);
  prevPageBtn.addEventListener("click", () => changePage(-1));
  nextPageBtn.addEventListener("click", () => changePage(1));

  // Device events
  registerDeviceBtn.addEventListener("click", showDeviceModal);
  deviceForm.addEventListener("submit", handleRegisterDevice);
  cancelDeviceBtn.addEventListener("click", hideDeviceModal);
  document
    .querySelector(".modal-close")
    .addEventListener("click", hideDeviceModal);
  deviceModal.addEventListener("click", (e) => {
    if (e.target === deviceModal) hideDeviceModal();
  });
}

// Utility functions
function showLoadingScreen() {
  loadingScreen.classList.remove("hidden");
}

function hideLoadingScreen() {
  loadingScreen.classList.add("hidden");
}

function showAuthScreen() {
  authContainer.classList.remove("hidden");
  mainContainer.classList.add("hidden");
}

function showMainApp() {
  authContainer.classList.add("hidden");
  mainContainer.classList.remove("hidden");
}

function showLoginForm() {
  loginForm.classList.remove("hidden");
  registerForm.classList.add("hidden");
}

function showRegisterForm() {
  loginForm.classList.add("hidden");
  registerForm.classList.remove("hidden");
}

function showDeviceModal() {
  deviceModal.classList.remove("hidden");
}

function hideDeviceModal() {
  deviceModal.classList.add("hidden");
  deviceForm.reset();
}

function switchTab(tabName) {
  // Update navigation buttons
  navBtns.forEach((btn) => {
    btn.classList.toggle("active", btn.dataset.tab === tabName);
  });

  // Update tab content
  tabContents.forEach((content) => {
    content.classList.toggle("active", content.id === `${tabName}-tab`);
  });

  // Load data for the selected tab
  switch (tabName) {
    case "clipboard":
      loadLatestClipboard();
      break;
    case "history":
      loadClipboardHistory();
      break;
    case "devices":
      loadDevices();
      break;
  }
}

// API functions
async function apiRequest(endpoint, options = {}) {
  const url = `${CONFIG.API.BASE_URL}${endpoint}`;
  const config = {
    headers: {
      "Content-Type": "application/json",
      ...options.headers,
    },
    ...options,
  };

  if (authToken) {
    config.headers.Authorization = `Bearer ${authToken}`;
  }

  try {
    const response = await fetch(url, config);

    if (response.status === 401 && !isRefreshingToken) {
      // Token expired, try to refresh
      const refreshed = await refreshAuthToken();
      if (refreshed) {
        // Retry the request with new token
        config.headers.Authorization = `Bearer ${authToken}`;
        const retryResponse = await fetch(url, config);

        if (!retryResponse.ok) {
          let errorMessage = "Request failed";
          try {
            const error = await retryResponse.json();
            errorMessage = error.message || errorMessage;
          } catch (e) {
            // If response is not JSON, use status text
            errorMessage = retryResponse.statusText || errorMessage;
          }
          throw new Error(errorMessage);
        }

        return await retryResponse.json();
      } else {
        // Refresh failed, redirect to login
        handleLogout();
        throw new Error("Authentication failed");
      }
    }

    if (!response.ok) {
      let errorMessage = "Request failed";
      try {
        const error = await response.json();
        errorMessage = error.message || errorMessage;
      } catch (e) {
        // If response is not JSON, use status text
        errorMessage = response.statusText || errorMessage;
      }
      throw new Error(errorMessage);
    }

    return await response.json();
  } catch (error) {
    console.error("API request failed:", error);
    // Don't throw network errors for better UX
    if (error.name === "TypeError" && error.message.includes("fetch")) {
      throw new Error("Network error: Unable to connect to server");
    }
    throw error;
  }
}

// Authentication functions
async function handleLogin(event) {
  event.preventDefault();

  const email = document.getElementById("login-email").value;
  const password = document.getElementById("login-password").value;

  if (!email || !password) {
    showToast("Please enter both email and password", "error");
    return;
  }

  try {
    showLoadingScreen();
    const response = await apiRequest("/auth/login", {
      method: "POST",
      body: JSON.stringify({ email, password }),
    });

    authToken = response.token;
    refreshToken = response.refreshToken;
    currentUser = { email: response.email };

    localStorage.setItem("authToken", authToken);
    localStorage.setItem("refreshToken", refreshToken);

    showMainApp();
    await loadInitialData();
    initializeSignalR();
    showToast(`Welcome back, ${currentUser.email}!`, "success");
  } catch (error) {
    console.error("Login error:", error);
    if (error.message.includes("Network error")) {
      showToast(
        "Unable to connect to server. Please check if the backend is running.",
        "error"
      );
    } else {
      showToast(error.message || "Login failed", "error");
    }
  } finally {
    hideLoadingScreen();
  }
}

async function handleRegister(event) {
  event.preventDefault();

  const email = document.getElementById("register-email").value;
  const password = document.getElementById("register-password").value;
  const confirmPassword = document.getElementById(
    "register-confirm-password"
  ).value;

  if (!email || !password || !confirmPassword) {
    showToast("Please fill in all fields", "error");
    return;
  }

  if (password !== confirmPassword) {
    showToast("Passwords do not match", "error");
    return;
  }

  if (password.length < 6) {
    showToast("Password must be at least 6 characters long", "error");
    return;
  }

  try {
    showLoadingScreen();
    await apiRequest("/auth/register", {
      method: "POST",
      body: JSON.stringify({ email, password }),
    });

    showToast("Registration successful! Please login.", "success");
    showLoginForm();
    registerForm.reset();
  } catch (error) {
    console.error("Registration error:", error);
    if (error.message.includes("Network error")) {
      showToast(
        "Unable to connect to server. Please check if the backend is running.",
        "error"
      );
    } else {
      showToast(error.message || "Registration failed", "error");
    }
  } finally {
    hideLoadingScreen();
  }
}

async function handleLogout() {
  try {
    if (refreshToken) {
      await apiRequest("/auth/revoke", {
        method: "POST",
        body: JSON.stringify({ refreshToken }),
      });
    }
  } catch (error) {
    console.error("Error revoking token:", error);
  }

  // Clear local storage and state
  localStorage.removeItem("authToken");
  localStorage.removeItem("refreshToken");
  localStorage.removeItem("currentDeviceId");
  authToken = null;
  refreshToken = null;
  currentUser = null;
  currentDeviceId = null;
  lastClipboardContent = null;

  // Disconnect SignalR
  if (signalRConnection) {
    signalRConnection.stop();
    signalRConnection = null;
  }

  // Clear polling interval
  if (pollingInterval) {
    clearInterval(pollingInterval);
    pollingInterval = null;
  }

  showAuthScreen();
  showToast("Logged out successfully", "info");
}

async function validateToken() {
  try {
    await apiRequest("/clipboard/latest");
    return true;
  } catch (error) {
    return false;
  }
}

async function refreshAuthToken() {
  if (isRefreshingToken) {
    return false;
  }

  isRefreshingToken = true;

  try {
    // Don't use apiRequest here to avoid infinite loop
    const response = await fetch(`${CONFIG.API.BASE_URL}/auth/refresh`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify({ token: authToken, refreshToken }),
    });

    if (!response.ok) {
      console.error("Token refresh failed with status:", response.status);
      return false;
    }

    const data = await response.json();
    authToken = data.token;
    refreshToken = data.refreshToken;
    localStorage.setItem("authToken", authToken);
    localStorage.setItem("refreshToken", refreshToken);
    return true;
  } catch (error) {
    console.error("Token refresh failed:", error);
    return false;
  } finally {
    isRefreshingToken = false;
  }
}

// Clipboard functions
async function loadLatestClipboard() {
  try {
    const clipboard = await apiRequest("/clipboard/latest");
    console.log("Latest clipboard data:", clipboard);

    // Update the last clipboard content for change detection
    lastClipboardContent = clipboard?.contentText || clipboard?.content || "";

    displayClipboard(clipboard);
  } catch (error) {
    console.error("Error loading latest clipboard:", error);
    if (error.message.includes("No clipboard entries found")) {
      lastClipboardContent = "";
      displayClipboard(null);
    } else if (error.message.includes("Authentication failed")) {
      // Stop polling if authentication fails
      if (pollingInterval) {
        clearInterval(pollingInterval);
        pollingInterval = null;
      }
      showToast("Authentication failed. Please login again.", "error");
      handleLogout();
    } else {
      showToast("Failed to load clipboard", "error");
    }
  }
}

function displayClipboard(clipboard) {
  if (!clipboard) {
    currentClipboard.innerHTML = `
            <div class="clipboard-placeholder">
                <i class="fas fa-clipboard"></i>
                <p>No clipboard content available</p>
            </div>
        `;
    return;
  }

  const content = clipboard.contentText || clipboard.content || "";
  const isNewContent = content !== lastClipboardContent;

  currentClipboard.innerHTML = `
        <div class="clipboard-content-text">
            ${escapeHtml(content)}
            ${isNewContent ? '<span class="new-badge">NEW</span>' : ""}
        </div>
    `;

  // Remove the "NEW" badge after 5 seconds
  if (isNewContent) {
    setTimeout(() => {
      const newBadge = currentClipboard.querySelector(".new-badge");
      if (newBadge) {
        newBadge.remove();
      }
    }, 5000);
  }
}

async function copyToClipboard() {
  const content = currentClipboard.querySelector(".clipboard-content-text");
  if (!content) {
    showToast("No content to copy", "error");
    return;
  }

  try {
    await navigator.clipboard.writeText(content.textContent);
    showToast("Copied to clipboard!", "success");
  } catch (error) {
    showToast("Failed to copy to clipboard", "error");
  }
}

async function clearAllClipboard() {
  if (!confirm("Are you sure you want to clear all clipboard entries?")) {
    return;
  }

  try {
    await apiRequest("/clipboard/clear", { method: "DELETE" });
    lastClipboardContent = "";
    displayClipboard(null);
    showToast("All clipboard entries cleared", "success");
  } catch (error) {
    showToast("Failed to clear clipboard", "error");
  }
}

// Upload functionality
async function handleUploadClipboard() {
  const content = clipboardTextInput.value.trim();

  if (!content) {
    showToast("Please enter some text to upload", "error");
    return;
  }

  if (!currentDeviceId) {
    showToast("No device registered. Please refresh the page.", "error");
    return;
  }

  try {
    showLoadingScreen();
    console.log("Uploading content:", content);
    console.log("Using device ID:", currentDeviceId);
    console.log("Current user:", currentUser);

    // Upload the content to the clipboard
    const uploadResponse = await apiRequest("/clipboard", {
      method: "POST",
      body: JSON.stringify({
        deviceId: currentDeviceId,
        contentText: content,
        contentType: 0, // Text enum value
      }),
    });

    console.log("Upload response:", uploadResponse);

    // Clear the input
    clipboardTextInput.value = "";

    // Update the last clipboard content immediately
    lastClipboardContent =
      uploadResponse.contentText || uploadResponse.content || "";

    // Reload the latest clipboard to show the new content
    await loadLatestClipboard();

    // Reload history to show the new entry
    await loadClipboardHistory();

    showToast("Content uploaded to clipboard successfully!", "success");

    // Trigger immediate poll to ensure all instances get the update
    setTimeout(() => {
      pollForClipboardUpdates();
    }, 1000);
  } catch (error) {
    console.error("Upload error:", error);
    console.error("Error details:", {
      message: error.message,
      deviceId: currentDeviceId,
      user: currentUser,
    });
    showToast(error.message || "Failed to upload content", "error");
  } finally {
    hideLoadingScreen();
  }
}

function clearTextInput() {
  clipboardTextInput.value = "";
  showToast("Input cleared", "info");
}

// History functions
async function loadClipboardHistory() {
  try {
    const skip = (currentPage - 1) * itemsPerPage;
    const response = await apiRequest(
      `/clipboard/history?skip=${skip}&take=${itemsPerPage}`
    );
    console.log("Clipboard history data:", response);
    clipboardHistory = response;
    displayHistory();
  } catch (error) {
    console.error("Error loading history:", error);
    showToast("Failed to load history", "error");
  }
}

function displayHistory() {
  if (clipboardHistory.length === 0) {
    historyList.innerHTML =
      '<p style="text-align: center; color: #666; padding: 20px;">No clipboard history found</p>';
    return;
  }

  historyList.innerHTML = clipboardHistory
    .map(
      (item) => `
        <div class="history-item">
            <div class="history-item-header">
                <span style="color: #666; font-size: 12px;">
                    ${new Date(item.createdAt).toLocaleString()}
                </span>
                <div class="history-item-actions">
                    ${
                      item.isPinned
                        ? '<span style="color: #667eea;"><i class="fas fa-thumbtack"></i></span>'
                        : ""
                    }
                    ${
                      item.isArchived
                        ? '<span style="color: #dc3545;"><i class="fas fa-archive"></i></span>'
                        : ""
                    }
                    <button class="btn btn-sm btn-outline-primary" onclick="togglePin('${
                      item.id
                    }', ${!item.isPinned})">
                        <i class="fas fa-thumbtack"></i> ${
                          item.isPinned ? "Unpin" : "Pin"
                        }
                    </button>
                    <button class="btn btn-sm btn-outline-warning" onclick="toggleArchive('${
                      item.id
                    }', ${!item.isArchived})">
                        <i class="fas fa-archive"></i> ${
                          item.isArchived ? "Unarchive" : "Archive"
                        }
                    </button>
                    <button class="btn btn-sm btn-outline-info" onclick="showTagsModal('${
                      item.id
                    }', '${escapeHtml((item.tags || []).join(", "))}')">
                        <i class="fas fa-tags"></i> Tags
                    </button>
                    <button class="btn btn-secondary" onclick="restoreClipboard('${
                      item.id
                    }')">
                        <i class="fas fa-undo"></i> Restore
                    </button>
                    <button class="btn btn-danger" onclick="deleteClipboard('${
                      item.id
                    }')">
                        <i class="fas fa-trash"></i> Delete
                    </button>
                </div>
            </div>
            <div class="history-item-content">${escapeHtml(
              item.contentText || item.content || ""
            )}</div>
            ${
              item.tags && item.tags.length > 0
                ? `
            <div class="history-item-tags">
                ${item.tags
                  .map((tag) => `<span class="tag">${escapeHtml(tag)}</span>`)
                  .join("")}
            </div>
            `
                : ""
            }
        </div>
    `
    )
    .join("");
}

function filterHistory() {
  const searchTerm = searchInput.value.toLowerCase();
  const filterValue = filterSelect.value;

  const filteredHistory = clipboardHistory.filter((item) => {
    const content = (item.contentText || item.content || "").toLowerCase();
    const matchesSearch = content.includes(searchTerm);
    const matchesFilter =
      filterValue === "all" ||
      (filterValue === "pinned" && item.isPinned) ||
      (filterValue === "archived" && item.isArchived);

    return matchesSearch && matchesFilter;
  });

  displayFilteredHistory(filteredHistory);
}

function displayFilteredHistory(filteredHistory) {
  if (filteredHistory.length === 0) {
    historyList.innerHTML =
      '<p style="text-align: center; color: #666; padding: 20px;">No matching entries found</p>';
    return;
  }

  historyList.innerHTML = filteredHistory
    .map(
      (item) => `
        <div class="history-item">
            <div class="history-item-header">
                <span style="color: #666; font-size: 12px;">
                    ${new Date(item.createdAt).toLocaleString()}
                </span>
                <div class="history-item-actions">
                    ${
                      item.isPinned
                        ? '<span style="color: #667eea;"><i class="fas fa-thumbtack"></i></span>'
                        : ""
                    }
                    ${
                      item.isArchived
                        ? '<span style="color: #dc3545;"><i class="fas fa-archive"></i></span>'
                        : ""
                    }
                    <button class="btn btn-sm btn-outline-primary" onclick="togglePin('${
                      item.id
                    }', ${!item.isPinned})">
                        <i class="fas fa-thumbtack"></i> ${
                          item.isPinned ? "Unpin" : "Pin"
                        }
                    </button>
                    <button class="btn btn-sm btn-outline-warning" onclick="toggleArchive('${
                      item.id
                    }', ${!item.isArchived})">
                        <i class="fas fa-archive"></i> ${
                          item.isArchived ? "Unarchive" : "Archive"
                        }
                    </button>
                    <button class="btn btn-sm btn-outline-info" onclick="showTagsModal('${
                      item.id
                    }', '${escapeHtml((item.tags || []).join(", "))}')">
                        <i class="fas fa-tags"></i> Tags
                    </button>
                    <button class="btn btn-secondary" onclick="restoreClipboard('${
                      item.id
                    }')">
                        <i class="fas fa-undo"></i> Restore
                    </button>
                    <button class="btn btn-danger" onclick="deleteClipboard('${
                      item.id
                    }')">
                        <i class="fas fa-trash"></i> Delete
                    </button>
                </div>
            </div>
            <div class="history-item-content">${escapeHtml(
              item.contentText || item.content || ""
            )}</div>
            ${
              item.tags && item.tags.length > 0
                ? `
            <div class="history-item-tags">
                ${item.tags
                  .map((tag) => `<span class="tag">${escapeHtml(tag)}</span>`)
                  .join("")}
            </div>
            `
                : ""
            }
        </div>
    `
    )
    .join("");
}

async function restoreClipboard(entryId) {
  try {
    const response = await apiRequest(`/clipboard/restore/${entryId}`, {
      method: "POST",
    });
    showToast("Clipboard restored", "success");

    // Update the last clipboard content immediately
    lastClipboardContent = response.contentText || response.content || "";

    loadLatestClipboard();
    loadClipboardHistory();
    
    // Trigger immediate poll to ensure all instances get the update
    setTimeout(() => {
      pollForClipboardUpdates();
    }, 1000);
  } catch (error) {
    showToast("Failed to restore clipboard", "error");
  }
}

async function deleteClipboard(entryId) {
  if (!confirm("Are you sure you want to delete this clipboard entry?")) {
    return;
  }

  try {
    console.log("Deleting clipboard entry:", entryId);
    await apiRequest(`/clipboard/${entryId}`, { method: "DELETE" });
    showToast("Clipboard entry deleted", "success");
    await loadClipboardHistory();
  } catch (error) {
    console.error("Delete clipboard error:", error);
    showToast("Failed to delete clipboard entry", "error");
  }
}

function changePage(delta) {
  const newPage = currentPage + delta;
  if (newPage >= 1) {
    currentPage = newPage;
    loadClipboardHistory();
  }
}

// Device functions
async function loadDevices() {
  try {
    const response = await apiRequest("/clipboard/devices");
    console.log("Devices data:", response);
    console.log("Current device ID:", currentDeviceId);
    devices = response;
    displayDevices();
  } catch (error) {
    console.error("Error loading devices:", error);
    showToast("Failed to load devices", "error");
  }
}

function displayDevices() {
  if (devices.length === 0) {
    devicesList.innerHTML =
      '<p style="text-align: center; color: #666; padding: 20px;">No devices registered</p>';
    return;
  }

  devicesList.innerHTML = devices
    .map(
      (device) => `
        <div class="device-item">
            <div class="device-info">
                <div class="device-icon">
                    <i class="fas fa-${getDeviceIcon(device.deviceType)}"></i>
                </div>
                <div class="device-details">
                    <h4>${escapeHtml(device.deviceName)}</h4>
                    <p>${escapeHtml(device.deviceType)} • Registered ${new Date(
        device.registeredAt || device.lastSeen
      ).toLocaleDateString()}</p>
                    <p style="color: #666; font-size: 12px; margin-top: 5px;">
                        Last seen: ${
                          device.lastSeenDescription ||
                          new Date(device.lastSeen).toLocaleString()
                        }
                    </p>
                </div>
            </div>
            <button class="btn btn-danger" onclick="deleteDevice('${
              device.id
            }')">
                <i class="fas fa-trash"></i> Remove
            </button>
        </div>
    `
    )
    .join("");
}

function getDeviceIcon(deviceType) {
  switch (deviceType.toLowerCase()) {
    case "desktop":
      return "desktop";
    case "laptop":
      return "laptop";
    case "mobile":
      return "mobile-alt";
    case "tablet":
      return "tablet-alt";
    default:
      return "desktop";
  }
}

async function handleRegisterDevice(event) {
  event.preventDefault();

  const deviceName = document.getElementById("device-name").value;
  const deviceType = document.getElementById("device-type").value;

  try {
    await apiRequest("/clipboard/devices/register", {
      method: "POST",
      body: JSON.stringify({ deviceName, deviceType }),
    });

    hideDeviceModal();
    loadDevices();
    showToast("Device registered successfully", "success");
  } catch (error) {
    showToast(error.message || "Failed to register device", "error");
  }
}

async function deleteDevice(deviceId) {
  if (!confirm("Are you sure you want to remove this device?")) {
    return;
  }

  try {
    console.log("Deleting device:", deviceId);
    await apiRequest(`/clipboard/devices/${deviceId}`, { method: "DELETE" });
    showToast("Device removed successfully", "success");
    await loadDevices();
  } catch (error) {
    console.error("Delete device error:", error);
    showToast("Failed to remove device", "error");
  }
}

// SignalR functions
let pollingInterval = null;
let lastClipboardContent = null; // Track last clipboard content for change detection

// Separate function for polling to allow manual triggering
async function pollForClipboardUpdates() {
  try {
    // Always poll for clipboard updates regardless of active tab
    const clipboard = await apiRequest("/clipboard/latest");

    // Check if clipboard content has changed
    const currentContent =
      clipboard?.contentText || clipboard?.content || "";
    if (currentContent !== lastClipboardContent) {
      console.log("Clipboard content changed, updating display");
      lastClipboardContent = currentContent;
      displayClipboard(clipboard);

      // Show notification if not on clipboard tab
      if (
        !document
          .querySelector("#clipboard-tab")
          .classList.contains("active")
      ) {
        showToast("New clipboard content available!", "info");
      }
    }
  } catch (error) {
    // Only log errors, don't show toasts for polling errors
    console.debug(
      "Polling error (normal if no clipboard content):",
      error.message
    );
  }
}

function initializeSignalR() {
  if (signalRConnection) {
    signalRConnection.stop();
  }

  // Clear any existing polling interval
  if (pollingInterval) {
    clearInterval(pollingInterval);
  }

  // Note: You'll need to include the SignalR client library
  // For now, we'll simulate real-time updates with polling
  // Add a small delay before starting polling to ensure initial data loads
  setTimeout(() => {
    pollingInterval = setInterval(pollForClipboardUpdates, CONFIG.APP.POLLING_INTERVAL);
  }, 2000); // 2 second delay
}

// Utility functions
function escapeHtml(text) {
  const div = document.createElement("div");
  div.textContent = text;
  return div.innerHTML;
}

function debounce(func, wait) {
  let timeout;
  return function executedFunction(...args) {
    const later = () => {
      clearTimeout(timeout);
      func(...args);
    };
    clearTimeout(timeout);
    timeout = setTimeout(later, wait);
  };
}

function showToast(message, type = "info") {
  const toast = document.createElement("div");
  toast.className = `toast ${type}`;
  toast.innerHTML = `
        <i class="fas fa-${
          type === "success"
            ? "check-circle"
            : type === "error"
            ? "exclamation-circle"
            : "info-circle"
        }"></i>
        <span>${message}</span>
    `;

  const container = document.getElementById("toast-container");
  container.appendChild(toast);

  setTimeout(() => {
    toast.style.opacity = "0";
    setTimeout(() => {
      container.removeChild(toast);
    }, 300);
  }, CONFIG.APP.TOAST_DURATION);
}

async function loadInitialData() {
  userEmailSpan.textContent = currentUser.email;

  // Ensure we have a current device
  await ensureCurrentDevice();

  await loadLatestClipboard();
  await loadClipboardHistory();
  await loadDevices();
}

async function ensureCurrentDevice() {
  let deviceFound = false;

  if (currentDeviceId) {
    try {
      // Attempt to fetch all devices for the current user
      const userDevices = await apiRequest("/clipboard/devices");
      // Check if the stored currentDeviceId is among the user's active devices
      const foundDevice = userDevices.find((d) => d.id === currentDeviceId);
      if (foundDevice) {
        console.log("Using existing device from localStorage:", foundDevice);
        currentDeviceId = foundDevice.id; // Re-confirm in case it was a stale ID but now valid
        localStorage.setItem("currentDeviceId", currentDeviceId);
        deviceFound = true;
      } else {
        console.warn(
          "Stored device ID not found among user's devices or invalid. Clearing localStorage."
        );
        localStorage.removeItem("currentDeviceId");
        currentDeviceId = null; // Clear it so the next block can register/pick
      }
    } catch (error) {
      console.error("Error validating stored device ID:", error);
      // If there's an error fetching devices (e.g., auth error), treat currentDeviceId as invalid
      localStorage.removeItem("currentDeviceId");
      currentDeviceId = null;
    }
  }

  if (!deviceFound) {
    // This block runs if currentDeviceId was initially null, or if it was invalid/cleared
    try {
      // Try to get existing devices again (if not already fetched or if previous fetch failed)
      const existingDevices = await apiRequest("/clipboard/devices");
      if (existingDevices && existingDevices.length > 0) {
        // Use the first available device
        currentDeviceId = existingDevices[0].id;
        localStorage.setItem("currentDeviceId", currentDeviceId);
        console.log(
          "Using first available existing device:",
          existingDevices[0]
        );
      } else {
        // No devices exist, auto-register a new one
        const deviceName = `${navigator.platform} - ${navigator.userAgent
          .split(" ")
          .pop()}`;
        const deviceType = getDeviceType();

        const response = await apiRequest("/clipboard/devices/register", {
          method: "POST",
          body: JSON.stringify({
            deviceName: deviceName,
            deviceType: deviceType,
          }),
        });

        currentDeviceId = response.id;
        localStorage.setItem("currentDeviceId", currentDeviceId);
        console.log("Auto-registered new device:", response);
      }
    } catch (error) {
      console.error(
        "Failed to ensure current device (registration/selection):",
        error
      );
      showToast(
        "Failed to set up current device. Please try logging in again.",
        "error"
      );
    }
  }
}

function getDeviceType() {
  const userAgent = navigator.userAgent.toLowerCase();
  if (/mobile|android|iphone|ipad|phone/i.test(userAgent)) {
    return "mobile";
  } else if (/tablet|ipad/i.test(userAgent)) {
    return "tablet";
  } else if (/laptop/i.test(userAgent)) {
    return "laptop";
  } else {
    return "desktop";
  }
}

// Pin, Archive, and Tags functions
async function togglePin(entryId, isPinned) {
  try {
    const endpoint = isPinned
      ? `/clipboard/${entryId}/pin`
      : `/clipboard/${entryId}/unpin`;
    await apiRequest(endpoint, { method: "PUT" });
    showToast(
      `Clipboard ${isPinned ? "pinned" : "unpinned"} successfully`,
      "success"
    );
    await loadClipboardHistory();
  } catch (error) {
    console.error("Toggle pin error:", error);
    showToast("Failed to toggle pin status", "error");
  }
}

async function toggleArchive(entryId, isArchived) {
  try {
    const endpoint = isArchived
      ? `/clipboard/${entryId}/archive`
      : `/clipboard/${entryId}/unarchive`;
    await apiRequest(endpoint, { method: "PUT" });
    showToast(
      `Clipboard ${isArchived ? "archived" : "unarchived"} successfully`,
      "success"
    );
    await loadClipboardHistory();
  } catch (error) {
    console.error("Toggle archive error:", error);
    showToast("Failed to toggle archive status", "error");
  }
}

function showTagsModal(entryId, currentTags) {
  const tags = prompt("Enter tags (comma-separated):", currentTags);
  if (tags !== null) {
    updateTags(entryId, tags);
  }
}

async function updateTags(entryId, tagsString) {
  try {
    const tags = tagsString
      .split(",")
      .map((tag) => tag.trim())
      .filter((tag) => tag.length > 0);
    await apiRequest(`/clipboard/${entryId}/tags`, {
      method: "PUT",
      body: JSON.stringify({ tags: tags }),
    });
    showToast("Tags updated successfully", "success");
    await loadClipboardHistory();
  } catch (error) {
    console.error("Update tags error:", error);
    showToast("Failed to update tags", "error");
  }
}

// Global functions for onclick handlers
window.restoreClipboard = restoreClipboard;
window.deleteClipboard = deleteClipboard;
window.deleteDevice = deleteDevice;
window.togglePin = togglePin;
window.toggleArchive = toggleArchive;
window.showTagsModal = showTagsModal;

// Debug function to manually refresh device selection
window.refreshDeviceSelection = async function () {
  console.log("Manually refreshing device selection...");
  localStorage.removeItem("currentDeviceId");
  currentDeviceId = null;
  await ensureCurrentDevice();
  showToast("Device selection refreshed", "info");
};

// Initialize clipboardManager globally for onclick handlers
window.clipboardManager = {
  restoreClipboard: restoreClipboard,
  deleteClipboard: deleteClipboard,
  deleteDevice: deleteDevice,
  togglePin: togglePin,
  toggleArchive: toggleArchive,
  showTagsModal: showTagsModal,
};
