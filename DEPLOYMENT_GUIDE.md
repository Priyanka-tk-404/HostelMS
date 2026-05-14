# HostelMS — Complete Deployment Guide
## Railway (API + MySQL) + Netlify (Blazor Frontend)

---

## OVERVIEW

```
GitHub Repo
    │
    ├── HostelMS.API   ──► Railway.app  (API + MySQL)
    │                       URL: https://hostelms-api.up.railway.app
    │
    └── HostelMS.Blazor ──► Netlify.com (Frontend)
                            URL: https://hostelms.netlify.app
```

---

## STEP 1 — PUSH CODE TO GITHUB

### 1.1 Create GitHub Repository
1. Go to github.com → New Repository
2. Name: `HostelMS`
3. Keep it Private
4. Click Create

### 1.2 Push your code
Open PowerShell in your HostelMS folder:
```powershell
git init
git add .
git commit -m "Initial commit"
git branch -M main
git remote add origin https://github.com/YOUR_USERNAME/HostelMS.git
git push -u origin main
```

---

## STEP 2 — DEPLOY MYSQL ON RAILWAY

1. Go to **railway.app** → Login with GitHub
2. Click **New Project**
3. Click **Add Service** → Select **MySQL**
4. Wait 30 seconds — MySQL is created and Online

### 2.1 Get MySQL Connection Details
1. Click your **MySQL service**
2. Click **"Variables"** tab
3. Note down these values:
   - `MYSQL_HOST`
   - `MYSQL_PORT` (3306)
   - `MYSQL_DATABASE`
   - `MYSQL_USER`
   - `MYSQL_PASSWORD`

---

## STEP 3 — DEPLOY API ON RAILWAY

### 3.1 Add API Service
1. In your Railway project, click **"+" → GitHub Repo**
2. Select your `HostelMS` repository
3. Set **Root Directory** → type `HostelMS.API`
4. Railway detects the Dockerfile automatically

### 3.2 Add Environment Variables
Click your API service → **Variables** tab → Add each:

```
ConnectionStrings__DefaultConnection
= Server=MYSQL_HOST;Port=3306;Database=MYSQL_DATABASE;User=MYSQL_USER;Password=MYSQL_PASSWORD;SslMode=None;AllowPublicKeyRetrieval=True;

Jwt__Key
= HostelMS_SuperSecret_JWT_Key_2024_Min32Chars!!

Jwt__Issuer
= HostelMS.API

Jwt__Audience
= HostelMS.Blazor

ASPNETCORE_ENVIRONMENT
= Production

ASPNETCORE_URLS
= http://0.0.0.0:8080
```

> Replace MYSQL_HOST, MYSQL_DATABASE, MYSQL_USER, MYSQL_PASSWORD
> with the actual values from Step 2.1

### 3.3 Get Your API URL
1. Click your API service → **Settings** tab
2. Under "Networking" → click **Generate Domain**
3. You get: `https://hostelms-api.up.railway.app`
4. **Save this URL** — you need it for Blazor

### 3.4 Verify API is working
Open in browser:
```
https://hostelms-api.up.railway.app/swagger
```
You should see the Swagger UI page ✅

---

## STEP 4 — UPDATE BLAZOR API URL

Before deploying Blazor, update the API URL.

Open `HostelMS.Blazor/Program.cs`:
```csharp
// Change this line:
new HttpClient { BaseAddress = new Uri("http://localhost:7100/") }

// To your Railway API URL:
new HttpClient { BaseAddress = new Uri("https://hostelms-api.up.railway.app/") }
```

Commit and push:
```powershell
git add .
git commit -m "Update API URL for production"
git push
```

---

## STEP 5 — DEPLOY BLAZOR ON NETLIFY

### 5.1 Build Blazor locally
```powershell
cd HostelMS.Blazor
dotnet publish -c Release -o publish
```

### 5.2 Add _redirects file
Create file `HostelMS.Blazor/publish/wwwroot/_redirects` with content:
```
/* /index.html 200
```
This is REQUIRED for Blazor page routing to work.

### 5.3 Deploy to Netlify
**Option A — Drag & Drop (Easiest):**
1. Go to **netlify.com** → Login
2. Click **"Add new site"** → **"Deploy manually"**
3. Drag the folder `HostelMS.Blazor/publish/wwwroot` into Netlify
4. Done! You get URL like `https://random-name.netlify.app`

**Option B — Connect GitHub (Auto-deploy on push):**
1. Netlify → New Site → Import from GitHub
2. Select your `HostelMS` repo
3. Set build settings:
   - Base directory: `HostelMS.Blazor`
   - Build command: `dotnet publish -c Release -o publish`
   - Publish directory: `HostelMS.Blazor/publish/wwwroot`
4. Deploy

### 5.4 Custom domain (optional)
- Netlify → Site Settings → Domain Management
- Add your own domain or use free `.netlify.app` domain

---

## STEP 6 — FINAL VERIFICATION

1. Open your Netlify URL: `https://hostelms.netlify.app`
2. Login: `admin@hostelms.com` / `Admin@123`
3. Check all features work

---

## ENVIRONMENT VARIABLES SUMMARY

### Railway API Service:
| Variable | Value |
|---|---|
| `ConnectionStrings__DefaultConnection` | `Server=...;Port=3306;...` |
| `Jwt__Key` | `HostelMS_SuperSecret_JWT_Key_2024_Min32Chars!!` |
| `Jwt__Issuer` | `HostelMS.API` |
| `Jwt__Audience` | `HostelMS.Blazor` |
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `ASPNETCORE_URLS` | `http://0.0.0.0:8080` |

---

## TROUBLESHOOTING

### ❌ API not starting on Railway
- Check Deployment logs in Railway
- Make sure Dockerfile exists in `HostelMS.API/`
- Check all environment variables are set

### ❌ Blazor shows "Failed to fetch"
- Make sure API URL in `Program.cs` matches Railway URL exactly
- Make sure API has CORS allowing all origins (`AllowAnyOrigin`)

### ❌ Database tables missing
- Railway runs migrations automatically on startup (via `Program.cs`)
- Check Railway deployment logs for migration errors

### ❌ Blazor routing broken (404 on refresh)
- Make sure `_redirects` file exists in `wwwroot`
- Content must be exactly: `/* /index.html 200`

---

## COSTS

| Service | Free Tier |
|---|---|
| Railway | $5 credit/month (enough for small apps) |
| Netlify | 100GB bandwidth/month free |
| GitHub | Free for public/private repos |

**Total: ₹0 for small hostel usage** ✅
