@echo off
echo ============================================
echo   HostelMS - Smart Hostel Management System
echo ============================================
echo.

REM Check .NET
dotnet --version >nul 2>&1
IF %ERRORLEVEL% NEQ 0 (
    echo [ERROR] .NET SDK not found. Install from https://dotnet.microsoft.com/download/dotnet/8.0
    pause
    exit /b 1
)

echo [OK] .NET SDK found.

REM Check MySQL
mysql --version >nul 2>&1
IF %ERRORLEVEL% NEQ 0 (
    echo [WARNING] mysql CLI not found in PATH. Make sure MySQL is running.
) ELSE (
    echo [OK] MySQL CLI found.
)

echo.
echo [1/4] Restoring NuGet packages...
dotnet restore HostelMS.sln
IF %ERRORLEVEL% NEQ 0 ( echo [ERROR] Restore failed. & pause & exit /b 1 )

echo.
echo [2/4] Building solution...
dotnet build HostelMS.sln --no-restore -c Release
IF %ERRORLEVEL% NEQ 0 ( echo [ERROR] Build failed. & pause & exit /b 1 )

echo.
echo [3/4] Running EF Core migrations...
cd HostelMS.API
dotnet ef database update
IF %ERRORLEVEL% NEQ 0 (
    echo [ERROR] Migration failed. Check your MySQL connection in appsettings.json
    cd ..
    pause
    exit /b 1
)
cd ..

echo.
echo [4/4] Starting application...
echo.
echo Starting API on https://localhost:7000
start "HostelMS API" cmd /k "cd HostelMS.API && dotnet run"

timeout /t 3 /nobreak >nul

echo Starting Blazor on https://localhost:7001
start "HostelMS Blazor" cmd /k "cd HostelMS.Blazor && dotnet run"

timeout /t 5 /nobreak >nul

echo.
echo ============================================
echo  Application started!
echo  Blazor UI: https://localhost:7001
echo  API Docs:  https://localhost:7000/swagger
echo  Login:     admin@hostelms.com / Admin@123
echo ============================================
echo.
pause
