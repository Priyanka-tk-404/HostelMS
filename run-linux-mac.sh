#!/bin/bash

echo "============================================"
echo "  HostelMS - Smart Hostel Management System"
echo "============================================"
echo ""

# Check .NET
if ! command -v dotnet &> /dev/null; then
    echo "[ERROR] .NET SDK not found. Install from https://dotnet.microsoft.com/download/dotnet/8.0"
    exit 1
fi
echo "[OK] .NET SDK: $(dotnet --version)"

# Check MySQL
if ! command -v mysql &> /dev/null; then
    echo "[WARNING] mysql CLI not found. Make sure MySQL server is running."
else
    echo "[OK] MySQL found"
fi

echo ""
echo "[1/4] Restoring packages..."
dotnet restore HostelMS.sln || { echo "[ERROR] Restore failed"; exit 1; }

echo ""
echo "[2/4] Building solution..."
dotnet build HostelMS.sln --no-restore -c Release || { echo "[ERROR] Build failed"; exit 1; }

echo ""
echo "[3/4] Running EF Core migrations..."
cd HostelMS.API
dotnet ef database update || { echo "[ERROR] Migration failed. Check appsettings.json connection string"; exit 1; }
cd ..

echo ""
echo "[4/4] Starting services..."

# Start API in background
cd HostelMS.API
dotnet run &
API_PID=$!
cd ..
echo "[OK] API starting (PID: $API_PID)"

sleep 3

# Start Blazor in background
cd HostelMS.Blazor
dotnet run &
BLAZOR_PID=$!
cd ..
echo "[OK] Blazor starting (PID: $BLAZOR_PID)"

sleep 3

echo ""
echo "============================================"
echo " Application started!"
echo " Blazor UI : https://localhost:7001"
echo " API Docs  : https://localhost:7000/swagger"
echo " Login     : admin@hostelms.com / Admin@123"
echo "============================================"
echo ""
echo "Press Ctrl+C to stop all services."

# Wait and cleanup
trap "kill $API_PID $BLAZOR_PID 2>/dev/null; echo 'Stopped.'; exit 0" INT TERM
wait
