using HostelMS.API.Data;
using HostelMS.API.DTOs;
using HostelMS.API.Models;
using Microsoft.EntityFrameworkCore;

namespace HostelMS.API.Services
{
    // ─── Fee Service ───────────────────────────────────────────────────────────
    public interface IFeeService
    {
        Task<List<FeeRecordDto>> GetAllAsync(int? studentId = null);
        Task<FeeRecordDto?> GetByIdAsync(int id);
        Task<FeeRecordDto> CreateAsync(CreateFeeDto dto);
        Task<FeeRecordDto?> PayAsync(int id, PayFeeDto dto);
        Task BulkGenerateMonthlyAsync(string month, int year);
    }

    public class FeeService : IFeeService
    {
        private readonly HostelDbContext _db;
        public FeeService(HostelDbContext db) => _db = db;

        public async Task<List<FeeRecordDto>> GetAllAsync(int? studentId = null)
        {
            var q = _db.FeeRecords.Include(f => f.Student).AsQueryable();
            if (studentId.HasValue) q = q.Where(f => f.StudentId == studentId.Value);
            return await q.OrderByDescending(f => f.DueDate).Select(f => ToDto(f)).ToListAsync();
        }

        public async Task<FeeRecordDto?> GetByIdAsync(int id)
        {
            var f = await _db.FeeRecords.Include(f => f.Student).FirstOrDefaultAsync(f => f.Id == id);
            return f == null ? null : ToDto(f);
        }

        public async Task<FeeRecordDto> CreateAsync(CreateFeeDto dto)
        {
            var fee = new FeeRecord
            {
                StudentId = dto.StudentId,
                Amount = dto.Amount,
                DueDate = dto.DueDate,
                Type = dto.Type,
                Month = dto.Month,
                Year = dto.Year,
                Status = FeeStatus.Pending
            };
            _db.FeeRecords.Add(fee);
            await _db.SaveChangesAsync();
            var full = await _db.FeeRecords.Include(f => f.Student).FirstAsync(f => f.Id == fee.Id);
            return ToDto(full);
        }

        public async Task<FeeRecordDto?> PayAsync(int id, PayFeeDto dto)
        {
            var fee = await _db.FeeRecords.Include(f => f.Student).FirstOrDefaultAsync(f => f.Id == id);
            if (fee == null) return null;
            fee.PaidAmount += dto.PaidAmount;
            fee.PaidDate = TimeHelper.NowIST();
            fee.TransactionId = dto.TransactionId;
            fee.PaymentMode = dto.PaymentMode;
            fee.Remarks = dto.Remarks;
            fee.Status = fee.PaidAmount >= fee.Amount ? FeeStatus.Paid : FeeStatus.PartiallyPaid;
            await _db.SaveChangesAsync();
            return ToDto(fee);
        }

        public async Task BulkGenerateMonthlyAsync(string month, int year)
        {
            var students = await _db.Students
                .Include(s => s.Room)
                .Where(s => s.Status == StudentStatus.Active && s.RoomId != null)
                .ToListAsync();

            foreach (var s in students)
            {
                var exists = await _db.FeeRecords.AnyAsync(f => f.StudentId == s.Id && f.Month == month && f.Year == year && f.Type == FeeType.Monthly);
                if (!exists)
                {
                    _db.FeeRecords.Add(new FeeRecord
                    {
                        StudentId = s.Id,
                        Amount = s.Room!.MonthlyFee,
                        DueDate = new DateTime(year, int.Parse(month.Split('-')[1]), 5),
                        Type = FeeType.Monthly,
                        Month = month,
                        Year = year,
                        Status = FeeStatus.Pending
                    });
                }
            }
            await _db.SaveChangesAsync();
        }

        private static FeeRecordDto ToDto(FeeRecord f) => new()
        {
            Id = f.Id,
            StudentId = f.StudentId,
            StudentName = f.Student?.FullName ?? "",
            StudentRollNo = f.Student?.StudentId ?? "",
            Amount = f.Amount,
            PaidAmount = f.PaidAmount,
            DueDate = f.DueDate,
            PaidDate = f.PaidDate,
            Status = f.Status.ToString(),
            Type = f.Type.ToString(),
            TransactionId = f.TransactionId,
            PaymentMode = f.PaymentMode,
            Month = f.Month,
            Year = f.Year
        };
    }

    // ─── Visitor Service ───────────────────────────────────────────────────────
    public interface IVisitorService
    {
        Task<List<VisitorLogDto>> GetAllAsync(DateTime? date = null);
        Task<VisitorLogDto> CheckInAsync(CreateVisitorDto dto);
        Task<VisitorLogDto?> CheckOutAsync(int id);
        Task<List<VisitorLogDto>> GetByStudentAsync(int studentId);
    }

    public class VisitorService : IVisitorService
    {
        private readonly HostelDbContext _db;
        public VisitorService(HostelDbContext db) => _db = db;

        public async Task<List<VisitorLogDto>> GetAllAsync(DateTime? date = null)
        {
            var q = _db.VisitorLogs.Include(v => v.Student).ThenInclude(s => s!.Room).AsQueryable();
            if (date.HasValue) q = q.Where(v => v.InTime.Date == date.Value.Date);
            return await q.OrderByDescending(v => v.InTime).Select(v => ToDto(v)).ToListAsync();
        }

        public async Task<VisitorLogDto> CheckInAsync(CreateVisitorDto dto)
        {
            var log = new VisitorLog
            {
                VisitorName = dto.VisitorName,
                VisitorPhone = dto.VisitorPhone,
                Relation = dto.Relation,
                IdProof = dto.IdProof,
                StudentId = dto.StudentId,
                Purpose = dto.Purpose,
                InTime = TimeHelper.NowIST(),
                Status = VisitorStatus.Inside
            };
            _db.VisitorLogs.Add(log);
            await _db.SaveChangesAsync();
            var full = await _db.VisitorLogs.Include(v => v.Student).ThenInclude(s => s!.Room)
                .FirstAsync(v => v.Id == log.Id);
            return ToDto(full);
        }

        public async Task<VisitorLogDto?> CheckOutAsync(int id)
        {
            var log = await _db.VisitorLogs.Include(v => v.Student).ThenInclude(s => s!.Room)
                .FirstOrDefaultAsync(v => v.Id == id);
            if (log == null) return null;
            log.OutTime = TimeHelper.NowIST();
            log.Status = VisitorStatus.CheckedOut;
            await _db.SaveChangesAsync();
            return ToDto(log);
        }

        public async Task<List<VisitorLogDto>> GetByStudentAsync(int studentId)
        {
            return await _db.VisitorLogs.Include(v => v.Student).ThenInclude(s => s!.Room)
                .Where(v => v.StudentId == studentId)
                .OrderByDescending(v => v.InTime).Select(v => ToDto(v)).ToListAsync();
        }

        private static VisitorLogDto ToDto(VisitorLog v) => new()
        {
            Id = v.Id,
            VisitorName = v.VisitorName,
            VisitorPhone = v.VisitorPhone,
            Relation = v.Relation,
            IdProof = v.IdProof,
            StudentId = v.StudentId,
            StudentName = v.Student?.FullName ?? "",
            RoomNumber = v.Student?.Room?.RoomNumber,
            InTime = v.InTime,
            OutTime = v.OutTime,
            Status = v.Status.ToString(),
            Purpose = v.Purpose
        };
    }

    // ─── Complaint Service ─────────────────────────────────────────────────────
    public interface IComplaintService
    {
        Task<List<ComplaintDto>> GetAllAsync(ComplaintStatus? status = null);
        Task<ComplaintDto?> GetByIdAsync(int id);
        Task<ComplaintDto> CreateAsync(CreateComplaintDto dto);
        Task<ComplaintDto?> UpdateAsync(int id, UpdateComplaintDto dto);
        Task<List<ComplaintDto>> GetByStudentAsync(int studentId);
    }

    public class ComplaintService : IComplaintService
    {
        private readonly HostelDbContext _db;
        public ComplaintService(HostelDbContext db) => _db = db;

        public async Task<List<ComplaintDto>> GetAllAsync(ComplaintStatus? status = null)
        {
            var q = _db.Complaints.Include(c => c.Student).AsQueryable();
            if (status.HasValue) q = q.Where(c => c.Status == status.Value);
            return await q.OrderByDescending(c => c.CreatedAt).Select(c => ToDto(c)).ToListAsync();
        }

        public async Task<ComplaintDto?> GetByIdAsync(int id)
        {
            var c = await _db.Complaints.Include(c => c.Student).FirstOrDefaultAsync(c => c.Id == id);
            return c == null ? null : ToDto(c);
        }

        public async Task<ComplaintDto> CreateAsync(CreateComplaintDto dto)
        {
            var c = new Complaint
            {
                Title = dto.Title,
                Description = dto.Description,
                Category = Enum.TryParse<ComplaintCategory>(dto.Category, out var cat) ? cat : ComplaintCategory.Other,
                Priority = Enum.TryParse<ComplaintPriority>(dto.Priority, out var pri) ? pri : ComplaintPriority.Medium,
                StudentId = dto.StudentId,
                Status = ComplaintStatus.Open,
                CreatedAt = TimeHelper.NowIST()
            };
            _db.Complaints.Add(c);
            await _db.SaveChangesAsync();
            var full = await _db.Complaints.Include(x => x.Student).FirstAsync(x => x.Id == c.Id);
            return ToDto(full);
        }

        public async Task<ComplaintDto?> UpdateAsync(int id, UpdateComplaintDto dto)
        {
            var c = await _db.Complaints.Include(c => c.Student).FirstOrDefaultAsync(c => c.Id == id);
            if (c == null) return null;
            var newStatus = Enum.TryParse<ComplaintStatus>(dto.Status, out var st) ? st : c.Status;
            c.Status = newStatus;
            c.Resolution = dto.Resolution;
            c.AssignedTo = dto.AssignedTo;
            if (newStatus == ComplaintStatus.Resolved) c.ResolvedAt = TimeHelper.NowIST();
            await _db.SaveChangesAsync();
            return ToDto(c);
        }

        public async Task<List<ComplaintDto>> GetByStudentAsync(int studentId)
            => await _db.Complaints.Include(c => c.Student)
                .Where(c => c.StudentId == studentId)
                .OrderByDescending(c => c.CreatedAt).Select(c => ToDto(c)).ToListAsync();

        private static ComplaintDto ToDto(Complaint c) => new()
        {
            Id = c.Id,
            Title = c.Title,
            Description = c.Description,
            Category = c.Category.ToString(),
            Priority = c.Priority.ToString(),
            Status = c.Status.ToString(),
            StudentId = c.StudentId,
            StudentName = c.Student?.FullName ?? "",
            CreatedAt = c.CreatedAt,
            ResolvedAt = c.ResolvedAt,
            Resolution = c.Resolution
        };
    }

    // ─── Attendance Service ────────────────────────────────────────────────────
    public interface IAttendanceService
    {
        Task<List<AttendanceDto>> GetAllAsync(DateTime? date = null, int? studentId = null);
        Task<AttendanceDto?> MarkQRAttendanceAsync(QRAttendanceDto dto);
        Task<AttendanceDto> MarkManualAsync(int studentId, AttendanceStatus status, string? remarks = null);
        Task<object> GetAttendanceReportAsync(int studentId, string month, int year);
    }

    public class AttendanceService : IAttendanceService
    {
        private readonly HostelDbContext _db;
        private readonly IQRCodeService _qr;
        public AttendanceService(HostelDbContext db, IQRCodeService qr) { _db = db; _qr = qr; }

        public async Task<List<AttendanceDto>> GetAllAsync(DateTime? date = null, int? studentId = null)
        {
            var q = _db.AttendanceRecords.Include(a => a.Student).AsQueryable();
            if (date.HasValue) q = q.Where(a => a.Date.Date == date.Value.Date);
            if (studentId.HasValue) q = q.Where(a => a.StudentId == studentId.Value);
            return await q.OrderByDescending(a => a.Date).Select(a => ToDto(a)).ToListAsync();
        }

        public async Task<AttendanceDto?> MarkQRAttendanceAsync(QRAttendanceDto dto)
        {
            var (studentId, isValid) = _qr.ValidateAttendanceToken(dto.QRToken);
            if (!isValid) return null;

            var student = await _db.Students.FindAsync(studentId);
            if (student == null) return null;

            var today = TimeHelper.NowIST().Date;
            var existing = await _db.AttendanceRecords
                .FirstOrDefaultAsync(a => a.StudentId == studentId && a.Date.Date == today);

            if (existing == null)
            {
                existing = new AttendanceRecord
                {
                    StudentId = studentId,
                    Date = today,
                    Type = AttendanceType.QR,
                    Status = AttendanceStatus.Present,
                    InTime = TimeHelper.NowIST(),
                    QRToken = dto.QRToken
                };
                _db.AttendanceRecords.Add(existing);
            }
            else if (dto.Type == "Out")
            {
                existing.OutTime = TimeHelper.NowIST();
            }

            await _db.SaveChangesAsync();
            existing.Student = student;
            return ToDto(existing);
        }

        public async Task<AttendanceDto> MarkManualAsync(int studentId, AttendanceStatus status, string? remarks = null)
        {
            var today = TimeHelper.NowIST().Date;
            var existing = await _db.AttendanceRecords.Include(a => a.Student)
                .FirstOrDefaultAsync(a => a.StudentId == studentId && a.Date.Date == today);

            if (existing != null)
            {
                existing.Status = status;
                existing.Remarks = remarks;
            }
            else
            {
                existing = new AttendanceRecord
                {
                    StudentId = studentId,
                    Date = today,
                    Type = AttendanceType.Manual,
                    Status = status,
                    Remarks = remarks
                };
                if (status == AttendanceStatus.Present) existing.InTime = TimeHelper.NowIST();
                _db.AttendanceRecords.Add(existing);
            }
            await _db.SaveChangesAsync();
            var full = await _db.AttendanceRecords.Include(a => a.Student).FirstAsync(a => a.Id == existing.Id);
            return ToDto(full);
        }

        public async Task<object> GetAttendanceReportAsync(int studentId, string month, int year)
        {
            var records = await _db.AttendanceRecords
                .Where(a => a.StudentId == studentId && a.Date.Month == int.Parse(month) && a.Date.Year == year)
                .ToListAsync();
            return new
            {
                TotalDays = records.Count,
                Present = records.Count(r => r.Status == AttendanceStatus.Present),
                Absent = records.Count(r => r.Status == AttendanceStatus.Absent),
                Late = records.Count(r => r.Status == AttendanceStatus.Late),
                OnLeave = records.Count(r => r.Status == AttendanceStatus.OnLeave),
                Percentage = records.Count > 0 ? (double)records.Count(r => r.Status == AttendanceStatus.Present) / records.Count * 100 : 0
            };
        }

        private static AttendanceDto ToDto(AttendanceRecord a) => new()
        {
            Id = a.Id,
            StudentId = a.StudentId,
            StudentName = a.Student?.FullName ?? "",
            StudentRollNo = a.Student?.StudentId ?? "",
            Date = a.Date,
            Type = a.Type.ToString(),
            Status = a.Status.ToString(),
            InTime = a.InTime,
            OutTime = a.OutTime,
            Remarks = a.Remarks
        };
    }

    // ─── Dashboard Service ─────────────────────────────────────────────────────
    public interface IDashboardService
    {
        Task<DashboardStatsDto> GetStatsAsync();
    }

    public class DashboardService : IDashboardService
    {
        private readonly HostelDbContext _db;
        public DashboardService(HostelDbContext db) => _db = db;

        public async Task<DashboardStatsDto> GetStatsAsync()
        {
            var today = TimeHelper.NowIST().Date;
            return new DashboardStatsDto
            {
                TotalStudents = await _db.Students.CountAsync(),
                ActiveStudents = await _db.Students.CountAsync(s => s.Status == StudentStatus.Active),
                TotalRooms = await _db.Rooms.CountAsync(),
                AvailableRooms = await _db.Rooms.CountAsync(r => r.Status == RoomStatus.Available),
                OpenComplaints = await _db.Complaints.CountAsync(c => c.Status == ComplaintStatus.Open || c.Status == ComplaintStatus.InProgress),
                TodayVisitors = await _db.VisitorLogs.CountAsync(v => v.InTime.Date == today),
                TodayPresent = await _db.AttendanceRecords.CountAsync(a => a.Date.Date == today && a.Status == AttendanceStatus.Present),
                TotalFeeDue = await _db.FeeRecords.Where(f => f.Status == FeeStatus.Pending || f.Status == FeeStatus.Overdue).SumAsync(f => f.Amount - f.PaidAmount),
                TotalFeeCollected = await _db.FeeRecords.SumAsync(f => f.PaidAmount),
                RecentNotices = await _db.Notices.Where(n => n.IsActive).OrderByDescending(n => n.CreatedAt).Take(5)
                    .Select(n => new NoticeDto { Id = n.Id, Title = n.Title, Content = n.Content, Type = n.Type.ToString(), CreatedAt = n.CreatedAt, IsActive = n.IsActive }).ToListAsync(),
                RecentComplaints = await _db.Complaints.Include(c => c.Student).OrderByDescending(c => c.CreatedAt).Take(5)
                    .Select(c => new ComplaintDto { Id = c.Id, Title = c.Title, Category = c.Category.ToString(), Priority = c.Priority.ToString(), Status = c.Status.ToString(), StudentName = c.Student!.FullName, CreatedAt = c.CreatedAt }).ToListAsync()
            };
        }
    }
}
