const express = require("express");
const cors = require("cors");
const app = express();
const PORT = 7012;

// Middleware
app.use(cors());
app.use(express.json());

// Mock data
let users = [{ id: 1, email: "test@example.com", password: "password123" }];

let clipboardEntries = [
  {
    id: 1,
    content: "Hello from CopyHere!",
    createdAt: new Date().toISOString(),
    isPinned: false,
    isArchived: false,
  },
  {
    id: 2,
    content: "This is a sample clipboard entry",
    createdAt: new Date(Date.now() - 86400000).toISOString(),
    isPinned: true,
    isArchived: false,
  },
];

let devices = [
  {
    id: 1,
    deviceName: "My Desktop",
    deviceType: "desktop",
    registeredAt: new Date().toISOString(),
  },
];

let authTokens = new Map();

// Helper function to generate tokens
function generateToken() {
  return Math.random().toString(36).substring(2) + Date.now().toString(36);
}

// Helper function to verify token
function verifyToken(req, res, next) {
  const authHeader = req.headers.authorization;
  if (!authHeader || !authHeader.startsWith("Bearer ")) {
    return res.status(401).json({ message: "No token provided" });
  }

  const token = authHeader.substring(7);
  if (!authTokens.has(token)) {
    return res.status(401).json({ message: "Invalid token" });
  }

  req.user = authTokens.get(token);
  next();
}

// Auth endpoints
app.post("/api/auth/register", (req, res) => {
  const { email, password } = req.body;

  if (users.find((u) => u.email === email)) {
    return res.status(400).json({ message: "User already exists" });
  }

  const newUser = { id: users.length + 1, email, password };
  users.push(newUser);

  res.status(201).json({ message: "User registered successfully" });
});

app.post("/api/auth/login", (req, res) => {
  const { email, password } = req.body;

  const user = users.find((u) => u.email === email && u.password === password);
  if (!user) {
    return res.status(401).json({ message: "Invalid credentials" });
  }

  const accessToken = generateToken();
  const refreshToken = generateToken();

  authTokens.set(accessToken, { id: user.id, email: user.email });

  res.json({
    accessToken,
    refreshToken,
    user: { email: user.email },
  });
});

app.post("/api/auth/refresh", (req, res) => {
  const { refreshToken } = req.body;

  // In a real app, you'd validate the refresh token
  // For mock purposes, we'll just generate new tokens
  const accessToken = generateToken();
  const newRefreshToken = generateToken();

  // Find a user to associate with the token (mock behavior)
  const user = users[0];
  authTokens.set(accessToken, { id: user.id, email: user.email });

  res.json({
    accessToken,
    refreshToken: newRefreshToken,
  });
});

app.post("/api/auth/revoke", (req, res) => {
  const { refreshToken } = req.body;

  // In a real app, you'd invalidate the refresh token
  // For mock purposes, we'll just return success
  res.json({ message: "Token revoked successfully" });
});

// Clipboard endpoints
app.get("/api/clipboard/latest", verifyToken, (req, res) => {
  if (clipboardEntries.length === 0) {
    return res.status(404).json({ message: "No clipboard entries found" });
  }

  const latest = clipboardEntries[clipboardEntries.length - 1];
  res.json(latest);
});

app.get("/api/clipboard/history", verifyToken, (req, res) => {
  const { skip = 0, take = 10 } = req.query;
  const start = parseInt(skip);
  const end = start + parseInt(take);

  const paginatedEntries = clipboardEntries.slice(start, end);
  res.json(paginatedEntries);
});

app.post("/api/clipboard/restore/:id", verifyToken, (req, res) => {
  const { id } = req.params;
  const entry = clipboardEntries.find((e) => e.id === parseInt(id));

  if (!entry) {
    return res.status(404).json({ message: "Entry not found" });
  }

  // Move to the end (make it latest)
  clipboardEntries = clipboardEntries.filter((e) => e.id !== parseInt(id));
  clipboardEntries.push(entry);

  res.json({ message: "Clipboard restored successfully" });
});

app.delete("/api/clipboard/:id", verifyToken, (req, res) => {
  const { id } = req.params;
  const index = clipboardEntries.findIndex((e) => e.id === parseInt(id));

  if (index === -1) {
    return res.status(404).json({ message: "Entry not found" });
  }

  clipboardEntries.splice(index, 1);
  res.json({ message: "Entry deleted successfully" });
});

app.delete("/api/clipboard/clear", verifyToken, (req, res) => {
  clipboardEntries = [];
  res.json({ message: "All entries cleared successfully" });
});

// Device endpoints
app.get("/api/clipboard/devices", verifyToken, (req, res) => {
  res.json(devices);
});

app.post("/api/clipboard/devices/register", verifyToken, (req, res) => {
  const { deviceName, deviceType } = req.body;

  const newDevice = {
    id: devices.length + 1,
    deviceName,
    deviceType,
    registeredAt: new Date().toISOString(),
  };

  devices.push(newDevice);
  res.status(201).json(newDevice);
});

app.delete("/api/clipboard/devices/:id", verifyToken, (req, res) => {
  const { id } = req.params;
  const index = devices.findIndex((d) => d.id === parseInt(id));

  if (index === -1) {
    return res.status(404).json({ message: "Device not found" });
  }

  devices.splice(index, 1);
  res.json({ message: "Device removed successfully" });
});

// Health check
app.get("/api/health", (req, res) => {
  res.json({ status: "OK", message: "CopyHere API is running" });
});

// Start server
app.listen(PORT, () => {
  console.log(`Mock server running on https://localhost:${PORT}`);
  console.log(`API available at https://localhost:${PORT}/api`);
  console.log(`Health check: https://localhost:${PORT}/api/health`);
});
