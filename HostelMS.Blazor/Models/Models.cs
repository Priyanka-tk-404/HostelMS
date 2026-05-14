namespace HostelMS.Blazor.Models
{
    public enum UserRole { Admin, Warden, Student }

    public record LoginDto(string Email, string Password);
    public record AuthResponseDto(string Token, string FullName, string Email, UserRole Role, int UserId);

    public class RoomDto
    {
        public int Id { get; set; }
        public string RoomNumber { get; set; } = "";
        public string Type { get; set; } = "";
        public int Capacity { get; set; }
        public int OccupiedCount { get; set; }
        public string Status { get; set; } = "";
        public decimal MonthlyFee { get; set; }
        public string Floor { get; set; } = "";
        public string? Amenities { get; set; }
        public int AvailableSlots => Capacity - OccupiedCount;
    }

    public class CreateRoomDto
    {
        public string RoomNumber { get; set; } = "";
        public string Type { get; set; } = "Single";
        public int Capacity { get; set; }
        public string Status { get; set; } = "Available";
        public decimal MonthlyFee { get; set; }
        public string Floor { get; set; } = "";
        public string? Amenities { get; set; }
    }

    public class StudentDto
    {
        public int Id { get; set; }
        public string StudentId { get; set; } = "";
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Phone { get; set; } = "";
        public string? Department { get; set; }
        public string? Year { get; set; }
        public string? RoomNumber { get; set; }
        public int? RoomId { get; set; }
        public string Status { get; set; } = "";
        public DateTime JoiningDate { get; set; }
        public string? QRCode { get; set; }
        public string? ProfilePhoto { get; set; }
        public string? GuardianName { get; set; }
        public string? GuardianPhone { get; set; }
    }

    public class CreateStudentDto
    {
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Phone { get; set; } = "";
        public string? Address { get; set; }
        public string? Department { get; set; }
        public string? Year { get; set; }
        public int? RoomId { get; set; }
        public DateTime JoiningDate { get; set; } = DateTime.Today;
        public string? GuardianName { get; set; }
        public string? GuardianPhone { get; set; }
        public string? GuardianRelation { get; set; }
        public string Password { get; set; } = "Student@123";
    }

    public class FeeRecordDto
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = "";
        public string StudentRollNo { get; set; } = "";
        public decimal Amount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal Balance => Amount - PaidAmount;
        public DateTime DueDate { get; set; }
        public DateTime? PaidDate { get; set; }
        public string Status { get; set; } = "";
        public string Type { get; set; } = "";
        public string? TransactionId { get; set; }
        public string? PaymentMode { get; set; }
        public string Month { get; set; } = "";
        public int Year { get; set; }
    }

    public class CreateFeeDto
    {
        public int StudentId { get; set; }
        public decimal Amount { get; set; }
        public DateTime DueDate { get; set; } = DateTime.Today.AddDays(5);
        public string Type { get; set; } = "Monthly";
        public string Month { get; set; } = "";
        public int Year { get; set; } = DateTime.Today.Year;
    }

    public class PayFeeDto
    {
        public decimal PaidAmount { get; set; }
        public string? TransactionId { get; set; }
        public string? PaymentMode { get; set; }
        public string? Remarks { get; set; }
    }

    public class VisitorLogDto
    {
        public int Id { get; set; }
        public string VisitorName { get; set; } = "";
        public string? VisitorPhone { get; set; }
        public string? Relation { get; set; }
        public string? IdProof { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = "";
        public string? RoomNumber { get; set; }
        public DateTime InTime { get; set; }
        public DateTime? OutTime { get; set; }
        public string Status { get; set; } = "";
        public string? Purpose { get; set; }
    }

    public class CreateVisitorDto
    {
        public string VisitorName { get; set; } = "";
        public string? VisitorPhone { get; set; }
        public string? Relation { get; set; }
        public string? IdProof { get; set; }
        public int StudentId { get; set; }
        public string? Purpose { get; set; }
    }

    public class ComplaintDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string Category { get; set; } = "";
        public string Priority { get; set; } = "";
        public string Status { get; set; } = "";
        public int StudentId { get; set; }
        public string StudentName { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public string? Resolution { get; set; }
    }

    public class CreateComplaintDto
    {
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string Category { get; set; } = "Maintenance";
        public string Priority { get; set; } = "Medium";
        public int StudentId { get; set; }
    }

    public class UpdateComplaintDto
    {
        public string Status { get; set; } = "";
        public string? Resolution { get; set; }
    }

    public class AttendanceDto
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = "";
        public string StudentRollNo { get; set; } = "";
        public DateTime Date { get; set; }
        public string Type { get; set; } = "";
        public string Status { get; set; } = "";
        public DateTime? InTime { get; set; }
        public DateTime? OutTime { get; set; }
        public string? Remarks { get; set; }
    }

    public class NoticeDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Content { get; set; } = "";
        public string Type { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateNoticeDto
    {
        public string Title { get; set; } = "";
        public string Content { get; set; } = "";
        public string Type { get; set; } = "General";
        public DateTime? ExpiresAt { get; set; }
        public int CreatedBy { get; set; }
    }

    public class DashboardStatsDto
    {
        public int TotalStudents { get; set; }
        public int ActiveStudents { get; set; }
        public int TotalRooms { get; set; }
        public int AvailableRooms { get; set; }
        public int OpenComplaints { get; set; }
        public int TodayVisitors { get; set; }
        public int TodayPresent { get; set; }
        public decimal TotalFeeDue { get; set; }
        public decimal TotalFeeCollected { get; set; }
        public List<NoticeDto> RecentNotices { get; set; } = new();
        public List<ComplaintDto> RecentComplaints { get; set; } = new();
    }
}
