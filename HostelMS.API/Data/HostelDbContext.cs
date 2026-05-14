using Microsoft.EntityFrameworkCore;
using HostelMS.API.Models;

namespace HostelMS.API.Data
{
    public class HostelDbContext : DbContext
    {
        public HostelDbContext(DbContextOptions<HostelDbContext> options) : base(options) { }

        public DbSet<ApplicationUser> Users => Set<ApplicationUser>();
        public DbSet<Student> Students => Set<Student>();
        public DbSet<Room> Rooms => Set<Room>();
        public DbSet<FeeRecord> FeeRecords => Set<FeeRecord>();
        public DbSet<VisitorLog> VisitorLogs => Set<VisitorLog>();
        public DbSet<Complaint> Complaints => Set<Complaint>();
        public DbSet<AttendanceRecord> AttendanceRecords => Set<AttendanceRecord>();
        public DbSet<Notice> Notices => Set<Notice>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // User – unique email
            builder.Entity<ApplicationUser>()
                .HasIndex(u => u.Email).IsUnique();

            // Student – unique StudentId & email
            builder.Entity<Student>()
                .HasIndex(s => s.StudentId).IsUnique();
            builder.Entity<Student>()
                .HasIndex(s => s.Email).IsUnique();

            // Student → Room  (many students per room)
            builder.Entity<Student>()
                .HasOne(s => s.Room)
                .WithMany(r => r.Students)
                .HasForeignKey(s => s.RoomId)
                .OnDelete(DeleteBehavior.SetNull);

            // Student → User  (one-to-one)
            builder.Entity<Student>()
                .HasOne(s => s.User)
                .WithOne(u => u.Student)
                .HasForeignKey<Student>(s => s.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            // FeeRecord → Student
            builder.Entity<FeeRecord>()
                .HasOne(f => f.Student)
                .WithMany(s => s.FeeRecords)
                .HasForeignKey(f => f.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            // VisitorLog → Student
            builder.Entity<VisitorLog>()
                .HasOne(v => v.Student)
                .WithMany(s => s.VisitorLogs)
                .HasForeignKey(v => v.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Complaint → Student
            builder.Entity<Complaint>()
                .HasOne(c => c.Student)
                .WithMany(s => s.Complaints)
                .HasForeignKey(c => c.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            // AttendanceRecord → Student
            builder.Entity<AttendanceRecord>()
                .HasOne(a => a.Student)
                .WithMany(s => s.AttendanceRecords)
                .HasForeignKey(a => a.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Seed admin user
            builder.Entity<ApplicationUser>().HasData(new ApplicationUser
            {
                Id = 1,
                FullName = "Super Admin",
                Email = "admin@hostelms.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                Phone = "9999999999",
                Role = UserRole.Admin,
                IsActive = true,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            });

            // Seed sample rooms
            builder.Entity<Room>().HasData(
                new Room { Id = 1, RoomNumber = "101", Type = RoomType.Single, Capacity = 1, OccupiedCount = 0, Status = RoomStatus.Available, MonthlyFee = 3500, Floor = "Ground" },
                new Room { Id = 2, RoomNumber = "102", Type = RoomType.Double, Capacity = 2, OccupiedCount = 0, Status = RoomStatus.Available, MonthlyFee = 2500, Floor = "Ground" },
                new Room { Id = 3, RoomNumber = "201", Type = RoomType.Triple, Capacity = 3, OccupiedCount = 0, Status = RoomStatus.Available, MonthlyFee = 2000, Floor = "First" },
                new Room { Id = 4, RoomNumber = "202", Type = RoomType.Dormitory, Capacity = 6, OccupiedCount = 0, Status = RoomStatus.Available, MonthlyFee = 1500, Floor = "First" }
            );
        }
    }
}
