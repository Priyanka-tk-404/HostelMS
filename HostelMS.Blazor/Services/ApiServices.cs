using System.Net.Http.Json;
using Blazored.LocalStorage;
using HostelMS.Blazor.Models;

namespace HostelMS.Blazor.Services
{
    // ─── Token Helper ──────────────────────────────────────────────────────────
    public static class TokenHelper
    {
        public static async Task SetToken(HttpClient http, ILocalStorageService storage)
        {
            var token = await storage.GetItemAsStringAsync("auth_token");
            if (!string.IsNullOrEmpty(token))
                http.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }
    }

    // ─── Dashboard ─────────────────────────────────────────────────────────────
    public interface IDashboardApiService { Task<DashboardStatsDto?> GetStatsAsync(); }
    public class DashboardApiService : IDashboardApiService
    {
        private readonly HttpClient _http; private readonly ILocalStorageService _s;
        public DashboardApiService(HttpClient http, ILocalStorageService s) { _http = http; _s = s; }
        public async Task<DashboardStatsDto?> GetStatsAsync()
        { await TokenHelper.SetToken(_http, _s); try { return await _http.GetFromJsonAsync<DashboardStatsDto>("api/dashboard"); } catch { return null; } }
    }

    // ─── Rooms ─────────────────────────────────────────────────────────────────
    public interface IRoomApiService
    {
        Task<List<RoomDto>> GetAllAsync();
        Task<RoomDto?> CreateAsync(CreateRoomDto dto);
        Task<RoomDto?> UpdateAsync(int id, CreateRoomDto dto);
        Task<bool> DeleteAsync(int id);
    }
    public class RoomApiService : IRoomApiService
    {
        private readonly HttpClient _http; private readonly ILocalStorageService _s;
        public RoomApiService(HttpClient http, ILocalStorageService s) { _http = http; _s = s; }
        public async Task<List<RoomDto>> GetAllAsync()
        { await TokenHelper.SetToken(_http, _s); try { return await _http.GetFromJsonAsync<List<RoomDto>>("api/rooms") ?? new(); } catch { return new(); } }
        public async Task<RoomDto?> CreateAsync(CreateRoomDto dto)
        { await TokenHelper.SetToken(_http, _s); var r = await _http.PostAsJsonAsync("api/rooms", dto); return r.IsSuccessStatusCode ? await r.Content.ReadFromJsonAsync<RoomDto>() : null; }
        public async Task<RoomDto?> UpdateAsync(int id, CreateRoomDto dto)
        { await TokenHelper.SetToken(_http, _s); var r = await _http.PutAsJsonAsync($"api/rooms/{id}", dto); return r.IsSuccessStatusCode ? await r.Content.ReadFromJsonAsync<RoomDto>() : null; }
        public async Task<bool> DeleteAsync(int id)
        { await TokenHelper.SetToken(_http, _s); var r = await _http.DeleteAsync($"api/rooms/{id}"); return r.IsSuccessStatusCode; }
    }

    // ─── Students ──────────────────────────────────────────────────────────────
    public interface IStudentApiService
    {
        Task<List<StudentDto>> GetAllAsync();
        Task<StudentDto?> GetByIdAsync(int id);
        Task<StudentDto?> CreateAsync(CreateStudentDto dto);
        Task<StudentDto?> UpdateAsync(int id, CreateStudentDto dto);
        Task<bool> DeleteAsync(int id);
        Task<string?> RegenerateQRAsync(int id);
        Task<StudentDto?> AllocateRoomAsync(int studentId, int roomId);
    }
    public class StudentApiService : IStudentApiService
    {
        private readonly HttpClient _http; private readonly ILocalStorageService _s;
        public StudentApiService(HttpClient http, ILocalStorageService s) { _http = http; _s = s; }
        public async Task<List<StudentDto>> GetAllAsync()
        { await TokenHelper.SetToken(_http, _s); try { return await _http.GetFromJsonAsync<List<StudentDto>>("api/students") ?? new(); } catch { return new(); } }
        public async Task<StudentDto?> GetByIdAsync(int id)
        { await TokenHelper.SetToken(_http, _s); try { return await _http.GetFromJsonAsync<StudentDto>($"api/students/{id}"); } catch { return null; } }
        public async Task<StudentDto?> CreateAsync(CreateStudentDto dto)
        { await TokenHelper.SetToken(_http, _s); var r = await _http.PostAsJsonAsync("api/students", dto); return r.IsSuccessStatusCode ? await r.Content.ReadFromJsonAsync<StudentDto>() : null; }
        public async Task<StudentDto?> UpdateAsync(int id, CreateStudentDto dto)
        { await TokenHelper.SetToken(_http, _s); var r = await _http.PutAsJsonAsync($"api/students/{id}", dto); return r.IsSuccessStatusCode ? await r.Content.ReadFromJsonAsync<StudentDto>() : null; }
        public async Task<bool> DeleteAsync(int id)
        { await TokenHelper.SetToken(_http, _s); var r = await _http.DeleteAsync($"api/students/{id}"); return r.IsSuccessStatusCode; }
        public async Task<string?> RegenerateQRAsync(int id)
        { await TokenHelper.SetToken(_http, _s); var r = await _http.PostAsync($"api/students/{id}/regenerate-qr", null); if (!r.IsSuccessStatusCode) return null; var obj = await r.Content.ReadFromJsonAsync<Dictionary<string, string>>(); return obj?.GetValueOrDefault("qrCode"); }
        public async Task<StudentDto?> AllocateRoomAsync(int studentId, int roomId)
        { await TokenHelper.SetToken(_http, _s); var r = await _http.PostAsync($"api/students/{studentId}/allocate-room/{roomId}", null); return r.IsSuccessStatusCode ? await r.Content.ReadFromJsonAsync<StudentDto>() : null; }
    }

    // ─── Fees ──────────────────────────────────────────────────────────────────
    public interface IFeeApiService
    {
        Task<List<FeeRecordDto>> GetAllAsync(int? studentId = null);
        Task<FeeRecordDto?> CreateAsync(CreateFeeDto dto);
        Task<FeeRecordDto?> PayAsync(int id, PayFeeDto dto);
        Task BulkGenerateAsync(string month, int year);
    }
    public class FeeApiService : IFeeApiService
    {
        private readonly HttpClient _http; private readonly ILocalStorageService _s;
        public FeeApiService(HttpClient http, ILocalStorageService s) { _http = http; _s = s; }
        public async Task<List<FeeRecordDto>> GetAllAsync(int? studentId = null)
        { await TokenHelper.SetToken(_http, _s); var url = studentId.HasValue ? $"api/fees?studentId={studentId}" : "api/fees"; try { return await _http.GetFromJsonAsync<List<FeeRecordDto>>(url) ?? new(); } catch { return new(); } }
        public async Task<FeeRecordDto?> CreateAsync(CreateFeeDto dto)
        { await TokenHelper.SetToken(_http, _s); var r = await _http.PostAsJsonAsync("api/fees", dto); return r.IsSuccessStatusCode ? await r.Content.ReadFromJsonAsync<FeeRecordDto>() : null; }
        public async Task<FeeRecordDto?> PayAsync(int id, PayFeeDto dto)
        { await TokenHelper.SetToken(_http, _s); var r = await _http.PostAsJsonAsync($"api/fees/{id}/pay", dto); return r.IsSuccessStatusCode ? await r.Content.ReadFromJsonAsync<FeeRecordDto>() : null; }
        public async Task BulkGenerateAsync(string month, int year)
        { await TokenHelper.SetToken(_http, _s); await _http.PostAsync($"api/fees/bulk-generate?month={month}&year={year}", null); }
    }

    // ─── Visitors ──────────────────────────────────────────────────────────────
    public interface IVisitorApiService
    {
        Task<List<VisitorLogDto>> GetAllAsync(DateTime? date = null);
        Task<VisitorLogDto?> CheckInAsync(CreateVisitorDto dto);
        Task<VisitorLogDto?> CheckOutAsync(int id);
    }
    public class VisitorApiService : IVisitorApiService
    {
        private readonly HttpClient _http; private readonly ILocalStorageService _s;
        public VisitorApiService(HttpClient http, ILocalStorageService s) { _http = http; _s = s; }
        public async Task<List<VisitorLogDto>> GetAllAsync(DateTime? date = null)
        { await TokenHelper.SetToken(_http, _s); var url = date.HasValue ? $"api/visitors?date={date.Value:yyyy-MM-dd}" : "api/visitors"; try { return await _http.GetFromJsonAsync<List<VisitorLogDto>>(url) ?? new(); } catch { return new(); } }
        public async Task<VisitorLogDto?> CheckInAsync(CreateVisitorDto dto)
        { await TokenHelper.SetToken(_http, _s); var r = await _http.PostAsJsonAsync("api/visitors/checkin", dto); return r.IsSuccessStatusCode ? await r.Content.ReadFromJsonAsync<VisitorLogDto>() : null; }
        public async Task<VisitorLogDto?> CheckOutAsync(int id)
        { await TokenHelper.SetToken(_http, _s); var r = await _http.PostAsync($"api/visitors/{id}/checkout", null); return r.IsSuccessStatusCode ? await r.Content.ReadFromJsonAsync<VisitorLogDto>() : null; }
    }

    // ─── Complaints ────────────────────────────────────────────────────────────
    public interface IComplaintApiService
    {
        Task<List<ComplaintDto>> GetAllAsync(string? status = null);
        Task<ComplaintDto?> CreateAsync(CreateComplaintDto dto);
        Task<ComplaintDto?> UpdateAsync(int id, UpdateComplaintDto dto);
    }
    public class ComplaintApiService : IComplaintApiService
    {
        private readonly HttpClient _http; private readonly ILocalStorageService _s;
        public ComplaintApiService(HttpClient http, ILocalStorageService s) { _http = http; _s = s; }
        public async Task<List<ComplaintDto>> GetAllAsync(string? status = null)
        { await TokenHelper.SetToken(_http, _s); var url = status != null ? $"api/complaints?status={status}" : "api/complaints"; try { return await _http.GetFromJsonAsync<List<ComplaintDto>>(url) ?? new(); } catch { return new(); } }
        public async Task<ComplaintDto?> CreateAsync(CreateComplaintDto dto)
        { await TokenHelper.SetToken(_http, _s); var r = await _http.PostAsJsonAsync("api/complaints", dto); return r.IsSuccessStatusCode ? await r.Content.ReadFromJsonAsync<ComplaintDto>() : null; }
        public async Task<ComplaintDto?> UpdateAsync(int id, UpdateComplaintDto dto)
        { await TokenHelper.SetToken(_http, _s); var r = await _http.PutAsJsonAsync($"api/complaints/{id}", dto); return r.IsSuccessStatusCode ? await r.Content.ReadFromJsonAsync<ComplaintDto>() : null; }
    }

    // ─── Attendance ────────────────────────────────────────────────────────────
    public interface IAttendanceApiService
    {
        Task<List<AttendanceDto>> GetAllAsync(DateTime? date = null, int? studentId = null);
        Task<AttendanceDto?> MarkManualAsync(int studentId, string status, string? remarks = null);
        Task<AttendanceDto?> QRScanAsync(string token, string type = "In");
    }
    public class AttendanceApiService : IAttendanceApiService
    {
        private readonly HttpClient _http; private readonly ILocalStorageService _s;
        public AttendanceApiService(HttpClient http, ILocalStorageService s) { _http = http; _s = s; }
        public async Task<List<AttendanceDto>> GetAllAsync(DateTime? date = null, int? studentId = null)
        {
            await TokenHelper.SetToken(_http, _s);
            var url = "api/attendance"; var q = new List<string>();
            if (date.HasValue) q.Add($"date={date.Value:yyyy-MM-dd}");
            if (studentId.HasValue) q.Add($"studentId={studentId}");
            if (q.Count > 0) url += "?" + string.Join("&", q);
            try { return await _http.GetFromJsonAsync<List<AttendanceDto>>(url) ?? new(); } catch { return new(); }
        }
        public async Task<AttendanceDto?> MarkManualAsync(int studentId, string status, string? remarks = null)
        {
            await TokenHelper.SetToken(_http, _s);
            var url = $"api/attendance/manual?studentId={studentId}&status={status}";
            if (remarks != null) url += $"&remarks={Uri.EscapeDataString(remarks)}";
            var r = await _http.PostAsync(url, null);
            return r.IsSuccessStatusCode ? await r.Content.ReadFromJsonAsync<AttendanceDto>() : null;
        }
        public async Task<AttendanceDto?> QRScanAsync(string token, string type = "In")
        { var r = await _http.PostAsJsonAsync("api/attendance/qr", new { QRToken = token, Type = type }); return r.IsSuccessStatusCode ? await r.Content.ReadFromJsonAsync<AttendanceDto>() : null; }
    }

    // ─── Notices ───────────────────────────────────────────────────────────────
    public interface INoticeApiService
    {
        Task<List<NoticeDto>> GetAllAsync();
        Task<NoticeDto?> CreateAsync(CreateNoticeDto dto);
        Task<bool> DeleteAsync(int id);
    }
    public class NoticeApiService : INoticeApiService
    {
        private readonly HttpClient _http; private readonly ILocalStorageService _s;
        public NoticeApiService(HttpClient http, ILocalStorageService s) { _http = http; _s = s; }
        public async Task<List<NoticeDto>> GetAllAsync()
        { await TokenHelper.SetToken(_http, _s); try { return await _http.GetFromJsonAsync<List<NoticeDto>>("api/notices") ?? new(); } catch { return new(); } }
        public async Task<NoticeDto?> CreateAsync(CreateNoticeDto dto)
        { await TokenHelper.SetToken(_http, _s); var r = await _http.PostAsJsonAsync("api/notices", dto); return r.IsSuccessStatusCode ? await r.Content.ReadFromJsonAsync<NoticeDto>() : null; }
        public async Task<bool> DeleteAsync(int id)
        { await TokenHelper.SetToken(_http, _s); var r = await _http.DeleteAsync($"api/notices/{id}"); return r.IsSuccessStatusCode; }
    }
}
