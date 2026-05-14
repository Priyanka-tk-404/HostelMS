using HostelMS.API.DTOs;
using HostelMS.API.Models;
using HostelMS.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HostelMS.API.Controllers
{
    // ─── Auth Controller ───────────────────────────────────────────────────────
    [ApiController, Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _auth;
        public AuthController(IAuthService auth) => _auth = auth;

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var result = await _auth.LoginAsync(dto);
            if (result == null) return Unauthorized(new { message = "Invalid credentials" });
            return Ok(result);
        }

        [HttpPost("register"), Authorize(Roles = "Admin")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            var result = await _auth.RegisterAsync(dto);
            if (result == null) return BadRequest(new { message = "Email already exists" });
            return Ok(result);
        }
    }

    // ─── Rooms Controller ──────────────────────────────────────────────────────
    [ApiController, Route("api/[controller]"), Authorize]
    public class RoomsController : ControllerBase
    {
        private readonly HostelMS.API.Data.HostelDbContext _db;
        public RoomsController(HostelMS.API.Data.HostelDbContext db) => _db = db;

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var rooms = await _db.Rooms.ToListAsync();
            return Ok(rooms.Select(r => new RoomDto
            {
                Id = r.Id,
                RoomNumber = r.RoomNumber,
                Type = r.Type.ToString(),
                Capacity = r.Capacity,
                OccupiedCount = r.OccupiedCount,
                Status = r.Status.ToString(),
                MonthlyFee = r.MonthlyFee,
                Floor = r.Floor,
                Amenities = r.Amenities
            }));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var r = await _db.Rooms.FindAsync(id);
            if (r == null) return NotFound();
            return Ok(new RoomDto { Id = r.Id, RoomNumber = r.RoomNumber, Type = r.Type.ToString(), Capacity = r.Capacity, OccupiedCount = r.OccupiedCount, Status = r.Status.ToString(), MonthlyFee = r.MonthlyFee, Floor = r.Floor, Amenities = r.Amenities });
        }

        [HttpPost, Authorize(Roles = "Admin,Warden")]
        public async Task<IActionResult> Create(CreateRoomDto dto)
        {
            var room = new Room { RoomNumber = dto.RoomNumber, Type = dto.Type, Capacity = dto.Capacity, Status = dto.Status, MonthlyFee = dto.MonthlyFee, Floor = dto.Floor, Amenities = dto.Amenities };
            _db.Rooms.Add(room);
            await _db.SaveChangesAsync();
            return Ok(room);
        }

        [HttpPut("{id}"), Authorize(Roles = "Admin,Warden")]
        public async Task<IActionResult> Update(int id, CreateRoomDto dto)
        {
            var r = await _db.Rooms.FindAsync(id);
            if (r == null) return NotFound();
            r.RoomNumber = dto.RoomNumber; r.Type = dto.Type; r.Capacity = dto.Capacity;
            r.Status = dto.Status; r.MonthlyFee = dto.MonthlyFee; r.Floor = dto.Floor; r.Amenities = dto.Amenities;
            await _db.SaveChangesAsync();
            return Ok(r);
        }

        [HttpDelete("{id}"), Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var r = await _db.Rooms.FindAsync(id);
            if (r == null) return NotFound();
            _db.Rooms.Remove(r);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }

    // ─── Students Controller ───────────────────────────────────────────────────
    [ApiController, Route("api/[controller]"), Authorize]
    public class StudentsController : ControllerBase
    {
        private readonly IStudentService _svc;
        public StudentsController(IStudentService svc) => _svc = svc;

        [HttpGet] public async Task<IActionResult> GetAll() => Ok(await _svc.GetAllAsync());
        [HttpGet("{id}")] public async Task<IActionResult> GetById(int id) => Ok(await _svc.GetByIdAsync(id) ?? (object)NotFound());

        [HttpPost, Authorize(Roles = "Admin,Warden")]
        public async Task<IActionResult> Create(CreateStudentDto dto) => Ok(await _svc.CreateAsync(dto));

        [HttpPut("{id}"), Authorize(Roles = "Admin,Warden")]
        public async Task<IActionResult> Update(int id, CreateStudentDto dto)
        {
            var r = await _svc.UpdateAsync(id, dto);
            return r == null ? NotFound() : Ok(r);
        }

        [HttpDelete("{id}"), Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id) => await _svc.DeleteAsync(id) ? NoContent() : NotFound();

        [HttpPost("{id}/regenerate-qr"), Authorize(Roles = "Admin,Warden")]
        public async Task<IActionResult> RegenerateQR(int id) => Ok(new { qrCode = await _svc.RegenerateQRAsync(id) });

        [HttpPost("{studentId}/allocate-room/{roomId}"), Authorize(Roles = "Admin,Warden")]
        public async Task<IActionResult> AllocateRoom(int studentId, int roomId)
        {
            var r = await _svc.AllocateRoomAsync(studentId, roomId);
            return r == null ? BadRequest(new { message = "Room full or not found" }) : Ok(r);
        }
    }

    // ─── Fees Controller ───────────────────────────────────────────────────────
    [ApiController, Route("api/[controller]"), Authorize]
    public class FeesController : ControllerBase
    {
        private readonly IFeeService _svc;
        public FeesController(IFeeService svc) => _svc = svc;

        [HttpGet] public async Task<IActionResult> GetAll([FromQuery] int? studentId = null) => Ok(await _svc.GetAllAsync(studentId));
        [HttpGet("{id}")] public async Task<IActionResult> GetById(int id) => Ok(await _svc.GetByIdAsync(id) ?? (object)NotFound());

        [HttpPost, Authorize(Roles = "Admin,Warden")]
        public async Task<IActionResult> Create(CreateFeeDto dto) => Ok(await _svc.CreateAsync(dto));

        [HttpPost("{id}/pay"), Authorize(Roles = "Admin,Warden")]
        public async Task<IActionResult> Pay(int id, PayFeeDto dto)
        {
            var r = await _svc.PayAsync(id, dto);
            return r == null ? NotFound() : Ok(r);
        }

        [HttpPost("bulk-generate"), Authorize(Roles = "Admin")]
        public async Task<IActionResult> BulkGenerate([FromQuery] string month, [FromQuery] int year)
        {
            await _svc.BulkGenerateMonthlyAsync(month, year);
            return Ok(new { message = "Monthly fees generated" });
        }
    }

    // ─── Visitors Controller ───────────────────────────────────────────────────
    [ApiController, Route("api/[controller]"), Authorize]
    public class VisitorsController : ControllerBase
    {
        private readonly IVisitorService _svc;
        public VisitorsController(IVisitorService svc) => _svc = svc;

        [HttpGet] public async Task<IActionResult> GetAll([FromQuery] DateTime? date = null) => Ok(await _svc.GetAllAsync(date));
        [HttpGet("student/{studentId}")] public async Task<IActionResult> GetByStudent(int studentId) => Ok(await _svc.GetByStudentAsync(studentId));

        [HttpPost("checkin")]
        public async Task<IActionResult> CheckIn(CreateVisitorDto dto) => Ok(await _svc.CheckInAsync(dto));

        [HttpPost("{id}/checkout")]
        public async Task<IActionResult> CheckOut(int id)
        {
            var r = await _svc.CheckOutAsync(id);
            return r == null ? NotFound() : Ok(r);
        }
    }

    // ─── Complaints Controller ─────────────────────────────────────────────────
    [ApiController, Route("api/[controller]"), Authorize]
    public class ComplaintsController : ControllerBase
    {
        private readonly IComplaintService _svc;
        public ComplaintsController(IComplaintService svc) => _svc = svc;

        [HttpGet] public async Task<IActionResult> GetAll([FromQuery] ComplaintStatus? status = null) => Ok(await _svc.GetAllAsync(status));
        [HttpGet("{id}")] public async Task<IActionResult> GetById(int id) => Ok(await _svc.GetByIdAsync(id) ?? (object)NotFound());
        [HttpGet("student/{studentId}")] public async Task<IActionResult> GetByStudent(int studentId) => Ok(await _svc.GetByStudentAsync(studentId));

        [HttpPost] public async Task<IActionResult> Create(CreateComplaintDto dto) => Ok(await _svc.CreateAsync(dto));

        [HttpPut("{id}"), Authorize(Roles = "Admin,Warden")]
        public async Task<IActionResult> Update(int id, UpdateComplaintDto dto)
        {
            var r = await _svc.UpdateAsync(id, dto);
            return r == null ? NotFound() : Ok(r);
        }
    }

    // ─── Attendance Controller ─────────────────────────────────────────────────
    [ApiController, Route("api/[controller]"), Authorize]
    public class AttendanceController : ControllerBase
    {
        private readonly IAttendanceService _svc;
        public AttendanceController(IAttendanceService svc) => _svc = svc;

        [HttpGet] public async Task<IActionResult> GetAll([FromQuery] DateTime? date = null, [FromQuery] int? studentId = null) => Ok(await _svc.GetAllAsync(date, studentId));

        [HttpPost("qr"), AllowAnonymous]
        public async Task<IActionResult> QRAttendance(QRAttendanceDto dto)
        {
            var r = await _svc.MarkQRAttendanceAsync(dto);
            return r == null ? BadRequest(new { message = "Invalid or expired QR code" }) : Ok(r);
        }

        [HttpPost("manual"), Authorize(Roles = "Admin,Warden")]
        public async Task<IActionResult> Manual([FromQuery] int studentId, [FromQuery] AttendanceStatus status, [FromQuery] string? remarks = null) => Ok(await _svc.MarkManualAsync(studentId, status, remarks));

        [HttpGet("report/{studentId}")]
        public async Task<IActionResult> Report(int studentId, [FromQuery] string month, [FromQuery] int year) => Ok(await _svc.GetAttendanceReportAsync(studentId, month, year));
    }

    // ─── Dashboard Controller ──────────────────────────────────────────────────
    [ApiController, Route("api/[controller]"), Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _svc;
        public DashboardController(IDashboardService svc) => _svc = svc;
        [HttpGet] public async Task<IActionResult> Get() => Ok(await _svc.GetStatsAsync());
    }

    // ─── Notices Controller ────────────────────────────────────────────────────
    [ApiController, Route("api/[controller]"), Authorize]
    public class NoticesController : ControllerBase
    {
        private readonly HostelMS.API.Data.HostelDbContext _db;
        public NoticesController(HostelMS.API.Data.HostelDbContext db) => _db = db;

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var notices = await _db.Notices.Where(n => n.IsActive).OrderByDescending(n => n.CreatedAt).ToListAsync();
            return Ok(notices.Select(n => new NoticeDto { Id = n.Id, Title = n.Title, Content = n.Content, Type = n.Type.ToString(), CreatedAt = n.CreatedAt, ExpiresAt = n.ExpiresAt, IsActive = n.IsActive }));
        }

        [HttpPost, Authorize(Roles = "Admin,Warden")]
        public async Task<IActionResult> Create(CreateNoticeDto dto)
        {
            var n = new Notice {
                Title = dto.Title,
                Content = dto.Content,
                Type = Enum.TryParse<NoticeType>(dto.Type, out var t) ? t : NoticeType.General,
                ExpiresAt = dto.ExpiresAt,
                CreatedBy = dto.CreatedBy
            };
            _db.Notices.Add(n);
            await _db.SaveChangesAsync();
            return Ok(n);
        }

        [HttpDelete("{id}"), Authorize(Roles = "Admin,Warden")]
        public async Task<IActionResult> Delete(int id)
        {
            var n = await _db.Notices.FindAsync(id);
            if (n == null) return NotFound();
            n.IsActive = false;
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
