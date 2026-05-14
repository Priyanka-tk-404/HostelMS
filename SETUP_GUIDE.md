# HostelMS — Smart Hostel Management System
## Complete Setup Guide with MySQL Connection

---

## TABLE OF CONTENTS
1. Prerequisites Installation
2. MySQL Database Setup (Detailed)
3. Project Configuration
4. Running the Application
5. Database Schema Reference
6. API Endpoints Reference
7. QR-Based Student Entry — How It Works
8. Troubleshooting

---

## STEP 1 — PREREQUISITES

Install the following tools before proceeding:

### 1.1 .NET 8 SDK
- Download from: https://dotnet.microsoft.com/download/dotnet/8.0
- Verify installation:
  ```
  dotnet --version
  ```
  Expected output: `8.0.x`

### 1.2 MySQL Server 8.0+
- Download: https://dev.mysql.com/downloads/mysql/
- During installation:
  - Choose "Developer Default" setup type
  - Set root password (remember this — you'll need it)
  - Default port: 3306

- Verify MySQL is running:
  ```
  mysql --version
  ```

### 1.3 MySQL Workbench (Optional but recommended)
- Download: https://dev.mysql.com/downloads/workbench/
- Visual database management tool

### 1.4 Node.js (for Blazor WASM build)
- Download: https://nodejs.org (v18+)

### 1.5 Visual Studio 2022 OR VS Code
- Visual Studio 2022 Community: https://visualstudio.microsoft.com/
  - Install workloads: "ASP.NET and web development"
- VS Code: https://code.visualstudio.com/
  - Install extension: C# Dev Kit

---

## STEP 2 — MYSQL DATABASE SETUP (DETAILED)

### 2.1 Start MySQL Service

**Windows:**
```
# Option A: Via Services
Press Win+R → services.msc → Find "MySQL80" → Start

# Option B: Via Command Prompt (Run as Admin)
net start MySQL80
```

**macOS:**
```bash
brew services start mysql
# or
sudo /usr/local/mysql/support-files/mysql.server start
```

**Linux (Ubuntu/Debian):**
```bash
sudo systemctl start mysql
sudo systemctl enable mysql   # auto-start on boot
```

---

### 2.2 Connect to MySQL

```bash
mysql -u root -p
# Enter your root password when prompted
```

---

### 2.3 Create Database and User

After connecting, run these SQL commands:

```sql
-- Create the database
CREATE DATABASE hostelms_db
  CHARACTER SET utf8mb4
  COLLATE utf8mb4_unicode_ci;

-- Verify it was created
SHOW DATABASES;

-- Create a dedicated user (more secure than using root)
CREATE USER 'hostelms_user'@'localhost' IDENTIFIED BY 'HostelMS@2024';

-- Grant privileges to the user on our database
GRANT ALL PRIVILEGES ON hostelms_db.* TO 'hostelms_user'@'localhost';

-- Apply the privilege changes
FLUSH PRIVILEGES;

-- Verify the user was created
SELECT User, Host FROM mysql.user WHERE User = 'hostelms_user';

-- Exit MySQL
EXIT;
```

---

### 2.4 Test the New User Connection

```bash
mysql -u hostelms_user -p hostelms_db
# Enter: HostelMS@2024
```

If it connects successfully, type `EXIT;` and proceed.

---

### 2.5 Update Connection String in the Project

Open file: `HostelMS.API/appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=hostelms_db;User=hostelms_user;Password=HostelMS@2024;"
  },
  "Jwt": {
    "Key": "HostelMS_SuperSecret_JWT_Key_2024_Min32Chars!!",
    "Issuer": "HostelMS.API",
    "Audience": "HostelMS.Blazor"
  }
}
```

**Connection String Parameters Explained:**
| Parameter  | Value           | Description                    |
|------------|-----------------|--------------------------------|
| Server     | localhost       | MySQL server host              |
| Port       | 3306            | MySQL default port             |
| Database   | hostelms_db     | The database we created        |
| User       | hostelms_user   | The user we created            |
| Password   | HostelMS@2024   | The password we set            |

> **Remote MySQL (optional):** Change `Server=localhost` to your remote server IP.
> Example: `Server=192.168.1.100;Port=3306;...`

---

### 2.6 Run EF Core Migrations (Creates All Tables Automatically)

Open terminal in the project root folder:

```bash
# Navigate to API project
cd HostelMS.API

# Install EF Core tools (only once)
dotnet tool install --global dotnet-ef

# Verify EF tools
dotnet ef --version

# Apply migrations → creates all tables in MySQL
dotnet ef database update

# You should see:
# Build started...
# Applying migration '20240101000000_InitialCreate'
# Done.
```

---

### 2.7 Verify Tables Were Created

```bash
mysql -u hostelms_user -p hostelms_db
```

```sql
-- List all tables
SHOW TABLES;
```

Expected output:
```
+-------------------------+
| Tables_in_hostelms_db   |
+-------------------------+
| __EFMigrationsHistory   |
| AttendanceRecords        |
| Complaints               |
| FeeRecords               |
| Notices                  |
| Rooms                    |
| Students                 |
| Users                    |
| VisitorLogs              |
+-------------------------+
```

```sql
-- Check admin user was seeded
SELECT Id, FullName, Email, Role FROM Users;

-- Check rooms were seeded
SELECT * FROM Rooms;

EXIT;
```

---

## STEP 3 — PROJECT CONFIGURATION

### 3.1 Restore NuGet Packages

```bash
# In the solution root (where HostelMS.sln is)
dotnet restore
```

### 3.2 Configure Blazor API URL

Open `HostelMS.Blazor/Program.cs` and confirm:
```csharp
builder.Services.AddScoped(sp =>
    new HttpClient { BaseAddress = new Uri("https://localhost:7000/") });
```

Open `HostelMS.Blazor/wwwroot/appsettings.json`:
```json
{
  "ApiBaseUrl": "https://localhost:7000/"
}
```

### 3.3 Development HTTPS Certificate

```bash
# Trust the dev certificate (run once)
dotnet dev-certs https --trust
```

---

## STEP 4 — RUNNING THE APPLICATION

### 4.1 Start the API (Terminal 1)

```bash
cd HostelMS.API
dotnet run
```

Expected output:
```
info: Microsoft.Hosting.Lifetime[0]
      Now listening on: https://localhost:7000
info: Microsoft.Hosting.Lifetime[0]
      Now listening on: http://localhost:5000
```

Swagger UI: https://localhost:7000/swagger

### 4.2 Start the Blazor Frontend (Terminal 2)

```bash
cd HostelMS.Blazor
dotnet run
```

Expected output:
```
Now listening on: https://localhost:7001
Now listening on: http://localhost:5001
```

Open browser: **https://localhost:7001**

### 4.3 Login Credentials

| Role  | Email                  | Password   |
|-------|------------------------|------------|
| Admin | admin@hostelms.com     | Admin@123  |

---

## STEP 5 — DATABASE SCHEMA REFERENCE

### Tables Overview

```
hostelms_db
│
├── Users               (Login accounts for Admin/Warden/Student)
├── Rooms               (Room inventory with capacity & fees)
├── Students            (Student profiles linked to rooms & users)
│
├── FeeRecords          (Monthly/misc fee tracking per student)
├── VisitorLogs         (Visitor check-in/out entries)
├── Complaints          (Maintenance/food/security complaints)
├── AttendanceRecords   (Daily presence with QR or manual type)
└── Notices             (Notice board posts)
```

### Key Relationships

```
Users ──(1:1)── Students
Rooms ──(1:N)── Students
Students ──(1:N)── FeeRecords
Students ──(1:N)── VisitorLogs
Students ──(1:N)── Complaints
Students ──(1:N)── AttendanceRecords
```

### Students Table

```sql
DESCRIBE Students;
```
| Column          | Type          | Notes                        |
|-----------------|---------------|------------------------------|
| Id              | int PK        | Auto-increment               |
| StudentId       | varchar(20)   | Unique, e.g. HMS20240001     |
| FullName        | varchar(100)  |                              |
| Email           | varchar(150)  | Unique                       |
| Phone           | varchar(20)   |                              |
| Department      | varchar(100)  |                              |
| Year            | varchar(20)   | 1st Year / 2nd Year...       |
| JoiningDate     | datetime      |                              |
| Status          | int           | 0=Active,1=Inactive...       |
| QRCode          | longtext      | Base64 PNG of QR image       |
| RoomId          | int FK        | → Rooms.Id                   |
| UserId          | int FK        | → Users.Id                   |
| GuardianName    | varchar(100)  |                              |
| GuardianPhone   | varchar(20)   |                              |

### Rooms Table

```sql
DESCRIBE Rooms;
```
| Column       | Type          | Notes                     |
|--------------|---------------|---------------------------|
| Id           | int PK        |                           |
| RoomNumber   | varchar(20)   | e.g. 101, 202             |
| Type         | int           | 0=Single,1=Double,2=Triple,3=Dorm |
| Capacity     | int           | Max occupants             |
| OccupiedCount| int           | Current occupants         |
| Status       | int           | 0=Available,1=Full,2=Maintenance |
| MonthlyFee   | decimal(10,2) |                           |
| Floor        | varchar(50)   |                           |

### FeeRecords Table

```sql
DESCRIBE FeeRecords;
```
| Column       | Type         | Notes                     |
|--------------|--------------|---------------------------|
| Id           | int PK       |                           |
| StudentId    | int FK       | → Students.Id             |
| Amount       | decimal(10,2)|                           |
| PaidAmount   | decimal(10,2)|                           |
| DueDate      | datetime     |                           |
| PaidDate     | datetime?    | NULL if unpaid            |
| Status       | int          | 0=Pending,1=Paid,2=Partial,3=Overdue |
| Type         | int          | 0=Monthly,1=Security...   |
| TransactionId| varchar(100) | UTR / Reference           |
| PaymentMode  | varchar(50)  | Cash/UPI/Bank Transfer    |
| Month        | varchar(10)  | e.g. 2024-01              |

### AttendanceRecords Table

```sql
DESCRIBE AttendanceRecords;
```
| Column    | Type      | Notes                            |
|-----------|-----------|----------------------------------|
| Id        | int PK    |                                  |
| StudentId | int FK    | → Students.Id                    |
| Date      | datetime  |                                  |
| Type      | int       | 0=QR, 1=Manual, 2=Biometric      |
| Status    | int       | 0=Present,1=Absent,2=Late,3=Leave|
| InTime    | datetime? |                                  |
| OutTime   | datetime? |                                  |
| QRToken   | longtext  | The token that was scanned       |

---

## STEP 6 — API ENDPOINTS REFERENCE

Base URL: `https://localhost:7000/api`

### Auth
| Method | Endpoint         | Description      | Auth |
|--------|-----------------|------------------|------|
| POST   | /auth/login     | Login            | ❌   |
| POST   | /auth/register  | Create user      | Admin|

### Students
| Method | Endpoint                              | Description         |
|--------|---------------------------------------|---------------------|
| GET    | /students                             | List all students   |
| GET    | /students/{id}                        | Get by ID           |
| POST   | /students                             | Create student      |
| PUT    | /students/{id}                        | Update student      |
| DELETE | /students/{id}                        | Soft-delete         |
| POST   | /students/{id}/regenerate-qr          | New QR code         |
| POST   | /students/{sid}/allocate-room/{rid}   | Assign room         |

### Rooms
| Method | Endpoint      | Description     |
|--------|--------------|-----------------|
| GET    | /rooms        | List all rooms  |
| POST   | /rooms        | Create room     |
| PUT    | /rooms/{id}   | Update room     |
| DELETE | /rooms/{id}   | Delete room     |

### Fees
| Method | Endpoint                  | Description         |
|--------|--------------------------|---------------------|
| GET    | /fees?studentId={id}      | Get fee records     |
| POST   | /fees                     | Create fee          |
| POST   | /fees/{id}/pay            | Record payment      |
| POST   | /fees/bulk-generate       | Generate all monthly|

### Attendance
| Method | Endpoint                  | Description         |
|--------|--------------------------|---------------------|
| GET    | /attendance?date={date}   | Get records by date |
| POST   | /attendance/qr            | QR scan entry       |
| POST   | /attendance/manual        | Manual mark         |
| GET    | /attendance/report/{id}   | Monthly report      |

### Visitors
| Method | Endpoint               | Description      |
|--------|------------------------|------------------|
| GET    | /visitors?date={date}  | Visitor log      |
| POST   | /visitors/checkin      | Check in         |
| POST   | /visitors/{id}/checkout| Check out        |

### Complaints
| Method | Endpoint          | Description      |
|--------|------------------|------------------|
| GET    | /complaints       | List all         |
| POST   | /complaints       | Create           |
| PUT    | /complaints/{id}  | Update / Resolve |

### Dashboard
| Method | Endpoint    | Description        |
|--------|-------------|--------------------|
| GET    | /dashboard  | All stats summary  |

---

## STEP 7 — QR-BASED STUDENT ENTRY (HOW IT WORKS)

### Overview

Each student gets a unique QR code stored in the database. The QR encodes a **time-sensitive token** that expires after 1 hour. This prevents QR sharing/misuse.

### Flow

```
1. Admin creates student  →  QR code auto-generated & stored in DB
2. Student shows QR code  →  Guard opens /qr-scanner page
3. Guard scans / pastes token  →  POST /attendance/qr
4. API validates token:
   - Decodes base64 token
   - Extracts studentId + hourStamp
   - Recomputes HMAC hash using JWT secret
   - Validates current hour matches (±1 hour window)
5. If valid → AttendanceRecord created (Present / In/Out time)
6. If invalid → 400 Bad Request returned
```

### QR Token Structure

```
Base64Encode( "{studentId}:{yyyyMMddHH}:{SHA256_HMAC}" )
```

Example decoded:
```
42:2024011514:X9kZq2mPwN3...
 │      │           └── HMAC(studentId + hour + JWT_secret)
 │      └──────────────  2024-01-15, Hour 14 (2 PM)
 └─────────────────────  Student internal ID = 42
```

### Regenerating QR Codes

If a student's QR is compromised:
1. Go to Students page → Click 📱 (QR icon)
2. Click "Regenerate QR" button
3. New QR immediately stored in DB
4. Old QR code (different image, same token logic) becomes invalid

### Manual Override

Guards can also mark attendance manually from the Attendance page without scanning — useful for network-down scenarios.

---

## STEP 8 — USEFUL SQL QUERIES

### Get all active students with room info:
```sql
SELECT s.StudentId, s.FullName, s.Email, s.Department,
       r.RoomNumber, r.Floor, r.MonthlyFee
FROM Students s
LEFT JOIN Rooms r ON s.RoomId = r.Id
WHERE s.Status = 0
ORDER BY s.FullName;
```

### Get today's attendance summary:
```sql
SELECT
  COUNT(*) AS Total,
  SUM(CASE WHEN Status = 0 THEN 1 ELSE 0 END) AS Present,
  SUM(CASE WHEN Status = 1 THEN 1 ELSE 0 END) AS Absent,
  SUM(CASE WHEN Type = 0 THEN 1 ELSE 0 END) AS QR_Scans,
  SUM(CASE WHEN Type = 1 THEN 1 ELSE 0 END) AS Manual
FROM AttendanceRecords
WHERE DATE(Date) = CURDATE();
```

### Get pending fees with student names:
```sql
SELECT s.StudentId, s.FullName, f.Month, f.Amount,
       f.PaidAmount, (f.Amount - f.PaidAmount) AS Balance, f.DueDate
FROM FeeRecords f
JOIN Students s ON f.StudentId = s.Id
WHERE f.Status IN (0, 3)   -- 0=Pending, 3=Overdue
ORDER BY f.DueDate ASC;
```

### Get open complaints:
```sql
SELECT c.Id, c.Title, s.FullName AS Student,
       c.Category, c.Priority, c.CreatedAt
FROM Complaints c
JOIN Students s ON c.StudentId = s.Id
WHERE c.Status IN (0, 1)   -- 0=Open, 1=InProgress
ORDER BY c.Priority DESC, c.CreatedAt ASC;
```

### Room occupancy report:
```sql
SELECT RoomNumber, Floor, Type, Capacity,
       OccupiedCount,
       (Capacity - OccupiedCount) AS Available,
       ROUND(OccupiedCount / Capacity * 100, 1) AS OccupancyPct,
       MonthlyFee
FROM Rooms
ORDER BY Floor, RoomNumber;
```

### Monthly fee collection report:
```sql
SELECT Month, Year,
       COUNT(*) AS TotalRecords,
       SUM(Amount) AS TotalBilled,
       SUM(PaidAmount) AS TotalCollected,
       SUM(Amount - PaidAmount) AS TotalDue,
       ROUND(SUM(PaidAmount) / SUM(Amount) * 100, 1) AS CollectionPct
FROM FeeRecords
GROUP BY Year, Month
ORDER BY Year DESC, Month DESC;
```

---

## STEP 9 — TROUBLESHOOTING

### ❌ "Unable to connect to MySQL"
```
Check:
1. MySQL service is running (see Step 2.1)
2. Port 3306 is not blocked by firewall
3. Username/password in appsettings.json is correct
4. Database 'hostelms_db' exists
```

### ❌ "Migration failed"
```bash
# Drop and recreate
dotnet ef database drop --force
dotnet ef database update
```

### ❌ "Access denied for user"
```sql
-- Re-run in MySQL as root:
DROP USER IF EXISTS 'hostelms_user'@'localhost';
CREATE USER 'hostelms_user'@'localhost' IDENTIFIED BY 'HostelMS@2024';
GRANT ALL PRIVILEGES ON hostelms_db.* TO 'hostelms_user'@'localhost';
FLUSH PRIVILEGES;
```

### ❌ "CORS error in Blazor"
- Verify API is running on https://localhost:7000
- Check CORS policy in API `Program.cs` includes Blazor origin

### ❌ "SSL connection error"
Add `SslMode=None` to connection string:
```json
"DefaultConnection": "Server=localhost;Port=3306;Database=hostelms_db;User=hostelms_user;Password=HostelMS@2024;SslMode=None;"
```

### ❌ "Blazor 401 Unauthorized"
- JWT token expired — log out and log in again
- Check system clock (JWT timestamps are UTC-based)

---

## PRODUCTION DEPLOYMENT NOTES

1. **Change JWT Secret** — Use a 64+ char random string
2. **Change DB Password** — Use a strong unique password
3. **Enable HTTPS** — Get SSL certificate (Let's Encrypt for free)
4. **Environment Variables** — Never put secrets in appsettings.json for production:
   ```bash
   export ConnectionStrings__DefaultConnection="Server=prod-server;..."
   export Jwt__Key="your-super-secret-production-key-here"
   ```
5. **MySQL Production Hardening:**
   ```sql
   -- Remove test databases
   DROP DATABASE IF EXISTS test;
   -- Restrict remote root login
   DELETE FROM mysql.user WHERE User='root' AND Host!='localhost';
   FLUSH PRIVILEGES;
   ```

---

*HostelMS v1.0 — Built with Blazor WASM + ASP.NET Core 8 + MySQL*
