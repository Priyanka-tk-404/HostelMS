# 🏠 HostelMS — Smart Hostel Management System

> **Blazor WebAssembly + ASP.NET Core 8 + MySQL + JWT + QR Entry**

![.NET](https://img.shields.io/badge/.NET-8.0-purple)
![Blazor](https://img.shields.io/badge/Blazor-WASM-blue)
![MySQL](https://img.shields.io/badge/MySQL-8.0-orange)
![EF Core](https://img.shields.io/badge/EF_Core-8.0-green)

---

## 🎯 Features

| Module | Description |
|---|---|
| 🏠 Room Allocation | Manage rooms, assign students, track occupancy |
| 💰 Fee Management | Monthly fees, payments, bulk generation, receipts |
| 🚪 Visitor Entry | Check-in/out log with ID proof tracking |
| 📢 Complaint System | Categories, priority, assignment & resolution |
| 📋 Attendance | QR scan + manual marking, reports |
| 📱 QR Entry | HMAC-signed, time-bound QR codes per student |
| 📌 Notice Board | Post announcements with expiry |
| 📊 Dashboard | Live stats — students, rooms, fees, visitors |

## 🏗️ Tech Stack

```
Frontend  →  Blazor WebAssembly (.NET 8)
Backend   →  ASP.NET Core Web API (.NET 8)
Database  →  MySQL 8.0 via Pomelo EF Core
Auth      →  JWT Bearer + BCrypt passwords
QR Code   →  QRCoder (PNG, base64)
ORM       →  Entity Framework Core 8
```

## 🚀 Quick Start

```bash
# 1. Setup MySQL database
mysql -u root -p
CREATE DATABASE hostelms_db CHARACTER SET utf8mb4;
CREATE USER 'hostelms_user'@'localhost' IDENTIFIED BY 'HostelMS@2024';
GRANT ALL PRIVILEGES ON hostelms_db.* TO 'hostelms_user'@'localhost';
FLUSH PRIVILEGES;
EXIT;

# 2. Run migrations
cd HostelMS.API
dotnet ef database update

# 3. Start API (Terminal 1)
dotnet run
# → https://localhost:7000/swagger

# 4. Start Blazor (Terminal 2)
cd ../HostelMS.Blazor
dotnet run
# → https://localhost:7001
```

**Login:** `admin@hostelms.com` / `Admin@123`

> 📖 See **SETUP_GUIDE.md** for complete MySQL setup instructions.

## 📁 Project Structure

```
HostelMS/
├── HostelMS.API/
│   ├── Controllers/     ← REST endpoints
│   ├── Models/          ← Domain entities
│   ├── DTOs/            ← Request/response shapes
│   ├── Services/        ← Business logic
│   ├── Data/            ← DbContext (MySQL)
│   ├── Migrations/      ← EF Core migrations
│   └── appsettings.json ← DB connection string + JWT
│
├── HostelMS.Blazor/
│   ├── Pages/           ← All UI pages
│   ├── Shared/          ← Layout, NavMenu
│   ├── Services/        ← API clients
│   ├── Models/          ← Shared DTOs
│   └── wwwroot/css/     ← Violet+Blue theme
│
└── SETUP_GUIDE.md       ← Full MySQL setup guide
```

## 🎨 Design

- **Colors:** Violet (#6C3FC5) + Blue (#3B82F6) + White
- **Font:** Inter (Google Fonts)
- Responsive sidebar layout, card-based UI, animated modals

## 🔐 Roles

| Role | Access |
|---|---|
| Admin | Full access — all modules |
| Warden | Students, Rooms, Attendance, Complaints |
| Student | View own profile, fees, complaints |
