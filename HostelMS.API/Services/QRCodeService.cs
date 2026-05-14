using QRCoder;

namespace HostelMS.API.Services
{
    public interface IQRCodeService
    {
        string GenerateQRCode(string data);
        string GenerateStudentQR(string studentId, string studentName);
        string GenerateAttendanceToken(int studentId);
        (int studentId, bool isValid) ValidateAttendanceToken(string token);
    }

    public class QRCodeService : IQRCodeService
    {
        private readonly IConfiguration _cfg;
        public QRCodeService(IConfiguration cfg) => _cfg = cfg;

        public string GenerateQRCode(string data)
        {
            using var qrGenerator = new QRCodeGenerator();
            var qrData = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrData);
            var bytes = qrCode.GetGraphic(10);
            return $"data:image/png;base64,{Convert.ToBase64String(bytes)}";
        }

        public string GenerateStudentQR(string studentId, string studentName)
        {
            // QR image encodes the attendance token so scanning gives a valid token directly
            var token = GenerateAttendanceTokenForId(int.Parse(studentId.Replace("HMS", "").TrimStart('0').PadLeft(1, '0')));
            return GenerateQRCode(token);
        }

        // Internal helper using actual int id
        private string GenerateAttendanceTokenForId(int studentId)
        {
            var secret = _cfg["Jwt:Key"] ?? "DefaultSecret";
            // Use date only (not hour) so QR is valid all day
            var raw = $"{studentId}:{DateTime.UtcNow:yyyyMMdd}:{secret}";
            var hash = Convert.ToBase64String(
                System.Security.Cryptography.SHA256.HashData(
                    System.Text.Encoding.UTF8.GetBytes(raw)));
            var token = $"{studentId}:{DateTime.UtcNow:yyyyMMdd}:{hash}";
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(token));
        }

        public string GenerateAttendanceToken(int studentId)
        {
            var secret = _cfg["Jwt:Key"] ?? "DefaultSecret";
            // Valid all day (date only, not hour)
            var raw = $"{studentId}:{DateTime.UtcNow:yyyyMMdd}:{secret}";
            var hash = Convert.ToBase64String(
                System.Security.Cryptography.SHA256.HashData(
                    System.Text.Encoding.UTF8.GetBytes(raw)));
            var token = $"{studentId}:{DateTime.UtcNow:yyyyMMdd}:{hash}";
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(token));
        }

        public (int studentId, bool isValid) ValidateAttendanceToken(string encodedToken)
        {
            try
            {
                var raw = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(encodedToken));
                var parts = raw.Split(':');
                if (parts.Length != 3) return (0, false);

                var studentId = int.Parse(parts[0]);
                var day = parts[1];
                var today = DateTime.UtcNow.ToString("yyyyMMdd");
                var yesterday = DateTime.UtcNow.AddDays(-1).ToString("yyyyMMdd");

                // Valid for today or yesterday
                if (day != today && day != yesterday) return (0, false);

                var secret = _cfg["Jwt:Key"] ?? "DefaultSecret";
                var expected = $"{studentId}:{day}:{secret}";
                var expectedHash = Convert.ToBase64String(
                    System.Security.Cryptography.SHA256.HashData(
                        System.Text.Encoding.UTF8.GetBytes(expected)));

                return parts[2] == expectedHash ? (studentId, true) : (0, false);
            }
            catch { return (0, false); }
        }
    }
}
