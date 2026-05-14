using HostelMS.API.Data;
using HostelMS.API.DTOs;
using HostelMS.API.Models;
using Microsoft.EntityFrameworkCore;

namespace HostelMS.API.Services
{
    public interface IStudentService
    {
        Task<List<StudentDto>> GetAllAsync();
        Task<StudentDto?> GetByIdAsync(int id);
        Task<StudentDto?> GetByStudentIdAsync(string studentId);
        Task<StudentDto> CreateAsync(CreateStudentDto dto);
        Task<StudentDto?> UpdateAsync(int id, CreateStudentDto dto);
        Task<bool> DeleteAsync(int id);
        Task<string> RegenerateQRAsync(int id);
        Task<StudentDto?> AllocateRoomAsync(int studentId, int roomId);
    }

    public class StudentService : IStudentService
    {
        private readonly HostelDbContext _db;
        private readonly IQRCodeService _qr;

        public StudentService(HostelDbContext db, IQRCodeService qr)
        {
            _db = db;
            _qr = qr;
        }

        public async Task<List<StudentDto>> GetAllAsync()
            => await _db.Students.Include(s => s.Room)
                .Select(s => ToDto(s)).ToListAsync();

        public async Task<StudentDto?> GetByIdAsync(int id)
        {
            var s = await _db.Students.Include(s => s.Room).FirstOrDefaultAsync(s => s.Id == id);
            return s == null ? null : ToDto(s);
        }

        public async Task<StudentDto?> GetByStudentIdAsync(string studentId)
        {
            var s = await _db.Students.Include(s => s.Room).FirstOrDefaultAsync(s => s.StudentId == studentId);
            return s == null ? null : ToDto(s);
        }

        public async Task<StudentDto> CreateAsync(CreateStudentDto dto)
        {
            var count = await _db.Students.CountAsync() + 1;
            var sid = $"HMS{TimeHelper.NowIST().Year}{count:D4}";

            // Create login user
            var user = new ApplicationUser
            {
                FullName = dto.FullName,
                Email = dto.Email,
                Phone = dto.Phone,
                Role = UserRole.Student,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            var student = new Student
            {
                StudentId = sid,
                FullName = dto.FullName,
                Email = dto.Email,
                Phone = dto.Phone,
                Address = dto.Address,
                Department = dto.Department,
                Year = dto.Year,
                RoomId = dto.RoomId,
                JoiningDate = dto.JoiningDate,
                GuardianName = dto.GuardianName,
                GuardianPhone = dto.GuardianPhone,
                GuardianRelation = dto.GuardianRelation,
                UserId = user.Id
            };
            student.QRCode = _qr.GenerateStudentQR(sid, dto.FullName);

            _db.Students.Add(student);

            // Update room occupancy
            if (dto.RoomId.HasValue)
            {
                var room = await _db.Rooms.FindAsync(dto.RoomId.Value);
                if (room != null)
                {
                    room.OccupiedCount++;
                    if (room.OccupiedCount >= room.Capacity)
                        room.Status = RoomStatus.Full;
                }
            }

            await _db.SaveChangesAsync();
            return ToDto(student);
        }

        public async Task<StudentDto?> UpdateAsync(int id, CreateStudentDto dto)
        {
            var s = await _db.Students.FindAsync(id);
            if (s == null) return null;

            s.FullName = dto.FullName;
            s.Email = dto.Email;
            s.Phone = dto.Phone;
            s.Address = dto.Address;
            s.Department = dto.Department;
            s.Year = dto.Year;
            s.GuardianName = dto.GuardianName;
            s.GuardianPhone = dto.GuardianPhone;
            s.GuardianRelation = dto.GuardianRelation;
            await _db.SaveChangesAsync();
            return ToDto(s);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var s = await _db.Students.FindAsync(id);
            if (s == null) return false;
            s.Status = StudentStatus.Inactive;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<string> RegenerateQRAsync(int id)
        {
            var s = await _db.Students.FindAsync(id);
            if (s == null) return string.Empty;
            s.QRCode = _qr.GenerateStudentQR(s.StudentId, s.FullName);
            await _db.SaveChangesAsync();
            return s.QRCode;
        }

        public async Task<StudentDto?> AllocateRoomAsync(int studentId, int roomId)
        {
            var student = await _db.Students.Include(s => s.Room).FirstOrDefaultAsync(s => s.Id == studentId);
            var newRoom = await _db.Rooms.FindAsync(roomId);
            if (student == null || newRoom == null) return null;
            if (newRoom.OccupiedCount >= newRoom.Capacity) return null;

            // Free old room
            if (student.RoomId.HasValue)
            {
                var oldRoom = await _db.Rooms.FindAsync(student.RoomId.Value);
                if (oldRoom != null)
                {
                    oldRoom.OccupiedCount = Math.Max(0, oldRoom.OccupiedCount - 1);
                    oldRoom.Status = RoomStatus.Available;
                }
            }

            student.RoomId = roomId;
            newRoom.OccupiedCount++;
            if (newRoom.OccupiedCount >= newRoom.Capacity) newRoom.Status = RoomStatus.Full;

            await _db.SaveChangesAsync();
            student.Room = newRoom;
            return ToDto(student);
        }

        private static StudentDto ToDto(Student s) => new()
        {
            Id = s.Id,
            StudentId = s.StudentId,
            FullName = s.FullName,
            Email = s.Email,
            Phone = s.Phone,
            Department = s.Department,
            Year = s.Year,
            RoomNumber = s.Room?.RoomNumber,
            RoomId = s.RoomId,
            Status = s.Status.ToString(),
            JoiningDate = s.JoiningDate,
            QRCode = s.QRCode,
            ProfilePhoto = s.ProfilePhoto,
            GuardianName = s.GuardianName,
            GuardianPhone = s.GuardianPhone
        };
    }
}
