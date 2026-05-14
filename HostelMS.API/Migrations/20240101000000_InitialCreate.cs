using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HostelMS.API.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Notices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", 1),
                    Title = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    Content = table.Column<string>(type: "longtext", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_Notices", x => x.Id))
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Rooms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", 1),
                    RoomNumber = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Capacity = table.Column<int>(type: "int", nullable: false),
                    OccupiedCount = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    MonthlyFee = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Floor = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    Amenities = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table => table.PrimaryKey("PK_Rooms", x => x.Id))
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", 1),
                    FullName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                    PasswordHash = table.Column<string>(type: "longtext", nullable: false),
                    Phone = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_Users", x => x.Id))
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Students",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", 1),
                    StudentId = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    FullName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                    Phone = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    Address = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    Department = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    Year = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true),
                    JoiningDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    LeavingDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    QRCode = table.Column<string>(type: "longtext", nullable: true),
                    ProfilePhoto = table.Column<string>(type: "longtext", nullable: true),
                    RoomId = table.Column<int>(type: "int", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    GuardianName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    GuardianPhone = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true),
                    GuardianRelation = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Students", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Students_Rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Students_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AttendanceRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", 1),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    InTime = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    OutTime = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Remarks = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    QRToken = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttendanceRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AttendanceRecords_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Complaints",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", 1),
                    Title = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "longtext", nullable: false),
                    Category = table.Column<int>(type: "int", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Resolution = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    AssignedTo = table.Column<int>(type: "int", nullable: true),
                    AttachmentUrl = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Complaints", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Complaints_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "FeeRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", 1),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    PaidAmount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    PaidDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    TransactionId = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    PaymentMode = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    Remarks = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    Month = table.Column<string>(type: "longtext", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeeRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FeeRecords_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "VisitorLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", 1),
                    VisitorName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    VisitorPhone = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true),
                    Relation = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    IdProof = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    InTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    OutTime = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Purpose = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    ApprovedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VisitorLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VisitorLogs_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            // Indexes
            migrationBuilder.CreateIndex("IX_Students_Email",    "Students", "Email",     unique: true);
            migrationBuilder.CreateIndex("IX_Students_StudentId","Students", "StudentId", unique: true);
            migrationBuilder.CreateIndex("IX_Students_RoomId",   "Students", "RoomId");
            migrationBuilder.CreateIndex("IX_Students_UserId",   "Students", "UserId",    unique: true);
            migrationBuilder.CreateIndex("IX_Users_Email",       "Users",    "Email",     unique: true);
            migrationBuilder.CreateIndex("IX_FeeRecords_StudentId",        "FeeRecords",        "StudentId");
            migrationBuilder.CreateIndex("IX_VisitorLogs_StudentId",       "VisitorLogs",       "StudentId");
            migrationBuilder.CreateIndex("IX_Complaints_StudentId",        "Complaints",        "StudentId");
            migrationBuilder.CreateIndex("IX_AttendanceRecords_StudentId", "AttendanceRecords", "StudentId");

            // Seed admin (Admin@123)
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id","FullName","Email","PasswordHash","Phone","Role","CreatedAt","IsActive" },
                values: new object[] {
                    1, "Super Admin", "admin@hostelms.com",
                    "$2a$11$KvYzqZQ1MnIv2Ud.TuNvOOrgDTR9OI4v9xfyPNyQRSGXB6q3m.hCa",
                    "9999999999", 0,
                    new DateTime(2024,1,1,0,0,0,DateTimeKind.Utc), true
                });

            // Seed rooms
            migrationBuilder.InsertData(
                table: "Rooms",
                columns: new[] { "Id","RoomNumber","Type","Capacity","OccupiedCount","Status","MonthlyFee","Floor" },
                values: new object[,]
                {
                    { 1, "101", 0, 1, 0, 0, 3500m, "Ground" },
                    { 2, "102", 1, 2, 0, 0, 2500m, "Ground" },
                    { 3, "201", 2, 3, 0, 0, 2000m, "First"  },
                    { 4, "202", 3, 6, 0, 0, 1500m, "First"  }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("VisitorLogs");
            migrationBuilder.DropTable("FeeRecords");
            migrationBuilder.DropTable("Complaints");
            migrationBuilder.DropTable("AttendanceRecords");
            migrationBuilder.DropTable("Students");
            migrationBuilder.DropTable("Users");
            migrationBuilder.DropTable("Rooms");
            migrationBuilder.DropTable("Notices");
        }
    }
}
