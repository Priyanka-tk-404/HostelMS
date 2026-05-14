using HostelMS.API.Services;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HostelMS.API.Models
{
    // ─── User / Auth ───────────────────────────────────────────────────────────
    public class ApplicationUser
    {
        public int Id { get; set; }
        [Required, MaxLength(100)] public string FullName { get; set; } = string.Empty;
        [Required, MaxLength(150)] public string Email { get; set; } = string.Empty;
        [Required] public string PasswordHash { get; set; } = string.Empty;
        [MaxLength(20)] public string Phone { get; set; } = string.Empty;
        public UserRole Role { get; set; } = UserRole.Student;
        public DateTime CreatedAt { get; set; } = TimeHelper.NowIST();
        public bool IsActive { get; set; } = true;

        public Student? Student { get; set; }
    }

    public enum UserRole { Admin, Warden, Student }

    // ─── Room ──────────────────────────────────────────────────────────────────
    public class Room
    {
        public int Id { get; set; }
        [Required, MaxLength(20)] public string RoomNumber { get; set; } = string.Empty;
        public RoomType Type { get; set; }
        public int Capacity { get; set; }
        public int OccupiedCount { get; set; }
        public RoomStatus Status { get; set; } = RoomStatus.Available;
        [Column(TypeName = "decimal(10,2)")] public decimal MonthlyFee { get; set; }
        [MaxLength(50)] public string Floor { get; set; } = string.Empty;
        [MaxLength(200)] public string? Amenities { get; set; }

        public ICollection<Student> Students { get; set; } = new List<Student>();
    }

    public enum RoomType { Single, Double, Triple, Dormitory }
    public enum RoomStatus { Available, Full, UnderMaintenance }

    // ─── Student ───────────────────────────────────────────────────────────────
    public class Student
    {
        public int Id { get; set; }
        [Required, MaxLength(20)] public string StudentId { get; set; } = string.Empty;
        [Required, MaxLength(100)] public string FullName { get; set; } = string.Empty;
        [Required, MaxLength(150)] public string Email { get; set; } = string.Empty;
        [MaxLength(20)] public string Phone { get; set; } = string.Empty;
        [MaxLength(200)] public string? Address { get; set; }
        [MaxLength(100)] public string? Department { get; set; }
        [MaxLength(20)] public string? Year { get; set; }
        public DateTime JoiningDate { get; set; }
        public DateTime? LeavingDate { get; set; }
        public StudentStatus Status { get; set; } = StudentStatus.Active;
        public string? QRCode { get; set; }            // base64 PNG
        public string? ProfilePhoto { get; set; }

        // FK
        public int? RoomId { get; set; }
        public Room? Room { get; set; }
        public int? UserId { get; set; }
        public ApplicationUser? User { get; set; }

        // Emergency contact
        [MaxLength(100)] public string? GuardianName { get; set; }
        [MaxLength(20)] public string? GuardianPhone { get; set; }
        [MaxLength(100)] public string? GuardianRelation { get; set; }

        public ICollection<FeeRecord> FeeRecords { get; set; } = new List<FeeRecord>();
        public ICollection<Complaint> Complaints { get; set; } = new List<Complaint>();
        public ICollection<AttendanceRecord> AttendanceRecords { get; set; } = new List<AttendanceRecord>();
        public ICollection<VisitorLog> VisitorLogs { get; set; } = new List<VisitorLog>();
    }

    public enum StudentStatus { Active, Inactive, Graduated, Suspended }

    // ─── Fee ───────────────────────────────────────────────────────────────────
    public class FeeRecord
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public Student Student { get; set; } = null!;
        [Column(TypeName = "decimal(10,2)")] public decimal Amount { get; set; }
        [Column(TypeName = "decimal(10,2)")] public decimal PaidAmount { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? PaidDate { get; set; }
        public FeeStatus Status { get; set; } = FeeStatus.Pending;
        public FeeType Type { get; set; } = FeeType.Monthly;
        [MaxLength(100)] public string? TransactionId { get; set; }
        [MaxLength(50)] public string? PaymentMode { get; set; }
        [MaxLength(200)] public string? Remarks { get; set; }
        public string Month { get; set; } = string.Empty;   // "2024-01"
        public int Year { get; set; }
    }

    public enum FeeStatus { Pending, Paid, PartiallyPaid, Overdue, Waived }
    public enum FeeType { Monthly, Security, Maintenance, Miscellaneous }

    // ─── Visitor ───────────────────────────────────────────────────────────────
    public class VisitorLog
    {
        public int Id { get; set; }
        [Required, MaxLength(100)] public string VisitorName { get; set; } = string.Empty;
        [MaxLength(20)] public string? VisitorPhone { get; set; }
        [MaxLength(50)] public string? Relation { get; set; }
        [MaxLength(100)] public string? IdProof { get; set; }
        public int StudentId { get; set; }
        public Student Student { get; set; } = null!;
        public DateTime InTime { get; set; } = TimeHelper.NowIST();
        public DateTime? OutTime { get; set; }
        public VisitorStatus Status { get; set; } = VisitorStatus.Inside;
        [MaxLength(200)] public string? Purpose { get; set; }
        public int? ApprovedBy { get; set; }  // staff id
    }

    public enum VisitorStatus { Inside, CheckedOut, Denied }

    // ─── Complaint ─────────────────────────────────────────────────────────────
    public class Complaint
    {
        public int Id { get; set; }
        [Required, MaxLength(200)] public string Title { get; set; } = string.Empty;
        [Required] public string Description { get; set; } = string.Empty;
        public ComplaintCategory Category { get; set; }
        public ComplaintPriority Priority { get; set; } = ComplaintPriority.Medium;
        public ComplaintStatus Status { get; set; } = ComplaintStatus.Open;
        public int StudentId { get; set; }
        public Student Student { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = TimeHelper.NowIST();
        public DateTime? ResolvedAt { get; set; }
        [MaxLength(500)] public string? Resolution { get; set; }
        public int? AssignedTo { get; set; }
        public string? AttachmentUrl { get; set; }
    }

    public enum ComplaintCategory { Maintenance, Food, Security, Housekeeping, Internet, Other }
    public enum ComplaintPriority { Low, Medium, High, Urgent }
    public enum ComplaintStatus { Open, InProgress, Resolved, Closed, Rejected }

    // ─── Attendance ────────────────────────────────────────────────────────────
    public class AttendanceRecord
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public Student Student { get; set; } = null!;
        public DateTime Date { get; set; }
        public AttendanceType Type { get; set; }   // QR or Manual
        public AttendanceStatus Status { get; set; }
        public DateTime? InTime { get; set; }
        public DateTime? OutTime { get; set; }
        [MaxLength(200)] public string? Remarks { get; set; }
        public string? QRToken { get; set; }
    }

    public enum AttendanceType { QR, Manual, Biometric }
    public enum AttendanceStatus { Present, Absent, Late, OnLeave }

    // ─── Notice Board ──────────────────────────────────────────────────────────
    public class Notice
    {
        public int Id { get; set; }
        [Required, MaxLength(200)] public string Title { get; set; } = string.Empty;
        [Required] public string Content { get; set; } = string.Empty;
        public NoticeType Type { get; set; } = NoticeType.General;
        public DateTime CreatedAt { get; set; } = TimeHelper.NowIST();
        public DateTime? ExpiresAt { get; set; }
        public bool IsActive { get; set; } = true;
        public int CreatedBy { get; set; }
    }

    public enum NoticeType { General, Urgent, Event, Maintenance }
}
