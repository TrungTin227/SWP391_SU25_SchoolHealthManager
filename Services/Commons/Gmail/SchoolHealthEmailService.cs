﻿using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Services.Implementations
{
    public class SchoolHealthEmailService : ISchoolHealthEmailService
    {
        private readonly IEmailQueueService _emailQueueService;
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<SchoolHealthEmailService> _logger;

        public SchoolHealthEmailService(
            IEmailQueueService emailQueueService,
            IOptions<EmailSettings> emailSettings,
            ILogger<SchoolHealthEmailService> logger)
        {
            _emailQueueService = emailQueueService;
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }

        public async Task SendVaccinationConsentRequestAsync(string parentEmail, string studentName, string vaccineName, DateTime scheduledDate)
        {
            var subject = $"[{_emailSettings.SchoolName}] Thông báo tiêm chủng cho học sinh {studentName}";
            var message = GenerateVaccinationConsentEmail(studentName, vaccineName, scheduledDate);
            await QueueEmailAsync(parentEmail, subject, message);
        }

        public async Task SendHealthCheckupNotificationAsync(string parentEmail, string studentName, DateTime checkupDate)
        {
            var subject = $"[{_emailSettings.SchoolName}] Thông báo kiểm tra sức khỏe định kỳ - {studentName}";
            var message = GenerateHealthCheckupNotificationEmail(studentName, checkupDate);
            await QueueEmailAsync(parentEmail, subject, message);
        }

        public async Task SendMedicationDeliveryConfirmationAsync(string parentEmail, string studentName, string medicationName)
        {
            var subject = $"[{_emailSettings.SchoolName}] Xác nhận nhận thuốc cho học sinh {studentName}";
            var message = GenerateMedicationDeliveryConfirmationEmail(studentName, medicationName);
            await QueueEmailAsync(parentEmail, subject, message);
        }
        public async Task SendMedicationDeliverdAsync(string parentEmail, string studentName, string medicationName)
        {
            var subject = $"[{_emailSettings.SchoolName}] Xác nhận nhận thuốc cho học sinh {studentName}";
            var message = GenerateMedicationDeliveredEmail(studentName, medicationName);
            await QueueEmailAsync(parentEmail, subject, message);
        }

        public async Task SendHealthEventNotificationAsync(string parentEmail, string studentName, string eventDescription, string treatmentProvided)
        {
            var subject = $"[{_emailSettings.SchoolName}] THÔNG BÁO Y TẾ KHẨN CẤP - {studentName}";
            var message = GenerateHealthEventNotificationEmail(studentName, eventDescription, treatmentProvided);
            await QueueEmailAsync(parentEmail, subject, message);
        }

        public async Task SendDailyHealthReportAsync(List<string> nurseEmails, string reportContent)
        {
            var subject = $"[{_emailSettings.SchoolName}] Báo cáo y tế hàng ngày - {DateTime.Now:dd/MM/yyyy}";
            var message = GenerateDailyHealthReportEmail(reportContent);
            await QueueEmailAsync(nurseEmails, subject, message);
        }

        public async Task SendMedicalSupplyLowStockAlertAsync(List<string> adminEmails, List<string> lowStockItems)
        {
            var subject = $"[{_emailSettings.SchoolName}] CẢNH BÁO: Vật tư y tế sắp hết";
            var message = GenerateLowStockAlertEmail(lowStockItems);
            await QueueEmailAsync(adminEmails, subject, message);
        }

        public async Task SendEmergencyHealthEventAlertAsync(List<string> staffEmails, string studentName, string emergencyDetails)
        {
            var subject = $"[{_emailSettings.SchoolName}] KHẨN CẤP Y TẾ - {studentName}";
            var message = GenerateEmergencyEventEmail(studentName, emergencyDetails);
            await QueueEmailAsync(staffEmails, subject, message);
        }

        public async Task SendVaccinationCampaignReminderAsync(List<string> staffEmails, string campaignName, DateTime startDate)
        {
            var subject = $"[{_emailSettings.SchoolName}] Nhắc nhở chiến dịch tiêm chủng: {campaignName}";
            var message = GenerateCampaignReminderEmail(campaignName, startDate, "tiêm chủng");
            await QueueEmailAsync(staffEmails, subject, message);
        }

        public async Task SendHealthCheckupCampaignReminderAsync(List<string> staffEmails, string campaignName, DateTime startDate)
        {
            var subject = $"[{_emailSettings.SchoolName}] Nhắc nhở chiến dịch kiểm tra sức khỏe: {campaignName}";
            var message = GenerateCampaignReminderEmail(campaignName, startDate, "kiểm tra sức khỏe");
            await QueueEmailAsync(staffEmails, subject, message);
        }

        public async Task SendMonthlyHealthReportAsync(List<string> adminEmails, string reportPeriod, string reportSummary)
        {
            var subject = $"[{_emailSettings.SchoolName}] Báo cáo y tế tháng {reportPeriod}";
            var message = GenerateMonthlyReportEmail(reportPeriod, reportSummary);
            await QueueEmailAsync(adminEmails, subject, message);
        }

        #region Email Templates
        // Thay thế hoàn toàn phương thức cũ bằng phương thức này
        public async Task SendHospitalReferralAckAsync(
            string parentEmail,
            string studentName,
            string referralHospital,
            DateTime departureTime,
            string transportBy,
            string initialSymptoms,
            string injuredBodyParts,
            string firstAidDescription,
            Guid eventId,
            string ackToken)
        {
            // 1. Chuẩn bị dữ liệu cho email
            var subject = $"[THÔNG BÁO KHẨN CẤP] Về việc học sinh {studentName} nhập viện";
            var ackLink = $"{_emailSettings.BaseUrl}/api/health-events/{eventId}/parent-ack?token={ackToken}";

            // Tạo một mô tả sự việc súc tích, dễ hiểu
            string eventSummary = $"{initialSymptoms}. Vị trí chấn thương: {injuredBodyParts}.";
            // Đảm bảo có nội dung cho phần sơ cứu
            string firstAidText = string.IsNullOrWhiteSpace(firstAidDescription)
                                    ? "Chưa có sơ cứu hoặc không cần thiết."
                                    : firstAidDescription;

            // 2. Tạo nội dung email từ template mới
            var message = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <title>{subject}</title>
</head>
<body style='font-family: ""Segoe UI"", Tahoma, Geneva, Verdana, sans-serif; line-height: 1.7; color: #333; background-color: #f9f9f9; margin: 0; padding: 20px;'>
    <div style='max-width: 600px; margin: auto; background-color: white; border: 2px solid #d32f2f; border-radius: 10px; overflow: hidden;'>
        <div style='text-align: center; background-color: #d32f2f; color: white; padding: 20px;'>
            <h1 style='margin: 0; font-size: 28px; letter-spacing: 1px;'>🚑 THÔNG BÁO KHẨN CẤP</h1>
            <p style='margin: 5px 0 0; font-size: 16px;'>V/v sức khỏe của học sinh</p>
        </div>
        <div style='padding: 25px 30px;'>
            <p style='font-size: 16px;'>Kính gửi Quý phụ huynh của học sinh <strong>{studentName}</strong>,</p>
            <p>Phòng Y tế trường <strong>{_emailSettings.SchoolName}</strong> xin thông báo khẩn: do tình hình sức khỏe, nhà trường đã tiến hành thủ tục chuyển con em đến cơ sở y tế để được chăm sóc tốt nhất.</p>
            
            <div style='background-color: #fff3e0; border-left: 5px solid #ffb300; padding: 15px 20px; margin: 25px 0; border-radius: 5px;'>
                <h3 style='margin-top: 0; color: #c66900;'>Tóm Tắt Sự Việc & Sơ Cứu Ban Đầu</h3>
                <p style='margin: 5px 0;'><strong>• Tình trạng ban đầu:</strong> {eventSummary}</p>
                <p style='margin: 5px 0;'><strong>• Sơ cứu đã thực hiện:</strong> {firstAidText}</p>
            </div>

            <div style='background-color: #ffebee; border: 1px solid #d32f2f; border-radius: 8px; padding: 20px; text-align: center;'>
                <h2 style='margin-top: 0; color: #c62828;'>THÔNG TIN CHUYỂN VIỆN</h2>
                <p style='font-size: 18px; margin: 10px 0;'><strong>Bệnh viện tiếp nhận:</strong></p>
                <p style='font-size: 22px; font-weight: bold; margin: 5px 0; color: #d32f2f;'>{referralHospital}</p>
                <hr style='border: 0; border-top: 1px solid #ffcdd2; margin: 20px 0;'>
                <table style='width: 100%; text-align: left; font-size: 15px;'>
                    <tr>
                        <td style='padding: 5px;'><strong>Thời gian rời trường:</strong></td>
                        <td style='padding: 5px; font-weight: bold;'>{departureTime:HH:mm 'ngày' dd/MM/yyyy}</td>
                    </tr>
                    <tr>
                        <td style='padding: 5px;'><strong>Phương tiện di chuyển:</strong></td>
                        <td style='padding: 5px; font-weight: bold;'>{transportBy}</td>
                    </tr>
                </table>
            </div>

            <p style='margin-top: 25px;'><strong>Khuyến nghị:</strong> Quý phụ huynh vui lòng bình tĩnh và di chuyển đến bệnh viện để cùng phối hợp. Vui lòng nhấn nút dưới đây để xác nhận đã nhận được thông báo này.</p>
            
            <div style='text-align: center; margin: 30px 0;'>
                <a href='{ackLink}' style='background-color: #c62828; color: white; padding: 15px 30px; text-decoration: none; border-radius: 8px; font-size: 18px; font-weight: bold; display: inline-block;'>⚠️ Tôi đã nhận được thông báo</a>
            </div>
        </div>
        <div style='background-color: #f7f7f7; padding: 20px 30px; font-size: 13px; color: #666;'>
            <p style='margin: 0;'>Nếu cần thêm thông tin, vui lòng liên hệ khẩn cấp:</p>
            <strong>Phòng Y tế - {_emailSettings.SchoolName}</strong><br>
            <strong>Điện thoại: {_emailSettings.SchoolPhone}</strong> | Email: {_emailSettings.HealthDepartmentEmail}
        </div>
    </div>
</body>
</html>";

            // 3. Gửi email vào hàng đợi
            await QueueEmailAsync(parentEmail, subject, message);
        }

        // Đừng quên cập nhật lại method này để sử dụng _schoolSettings thay vì _emailSettings
        // cho các thông tin của trường học nhé.

        public async Task SendHealthEventAckMailAsync(
            string parentEmail,
            string studentName,
            string eventDescription,
            string treatmentProvided,
            DateTime occurredAt, 
            string location,    
            string symptoms,    
            Guid eventId,
            string ackToken)
        {
            // Lấy thông tin từ _schoolSettings (như đã hướng dẫn ở câu trả lời trước)
            var subject = $"[{_emailSettings.FromName}] Thông báo về sức khỏe của học sinh {studentName}";
            var ackLink = $"{_emailSettings.BaseUrl}/api/health-events/{eventId}/parent-ack?token={ackToken}";

            var message = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <title>{subject}</title>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; background-color: #f4f4f4; margin: 0; padding: 20px;'>
    <div style='max-width: 600px; margin: auto; background-color: white; border: 1px solid #ddd; border-radius: 10px; overflow: hidden;'>
        <div style='text-align: center; background-color: #f7f7f7; padding: 20px;'>
            <img src='{_emailSettings.SchoolLogoUrl}' alt='Logo Trường' style='max-height: 70px; margin-bottom: 15px;'>
            <h2 style='margin: 0; color: #e53935; font-size: 24px;'>THÔNG BÁO VỀ SỨC KHỎE HỌC SINH</h2>
        </div>
        <div style='padding: 25px 30px;'>
            <p>Kính gửi Quý phụ huynh học sinh <strong>{studentName}</strong>,</p>
            <p>Phòng Y tế trường <strong>{_emailSettings.SchoolName}</strong> xin thông báo về một sự việc liên quan đến sức khỏe của con em đã xảy ra tại trường, cụ thể như sau:</p>
            
            <div style='background-color: #fff3e0; border-left: 5px solid #ff9800; padding: 15px 20px; margin: 25px 0; border-radius: 5px;'>
                <h3 style='margin-top: 0; color: #ff5722;'>CHI TIẾT SỰ VIỆC</h3>
                <table style='width: 100%; border-collapse: collapse; font-size: 14px;'>
                    <tbody>
                        <tr>
                            <td style='padding: 8px 0; width: 150px; vertical-align: top;'><strong>Thời gian:</strong></td>
                            <td style='padding: 8px 0; vertical-align: top;'>{occurredAt:HH:mm 'ngày' dd/MM/yyyy}</td>
                        </tr>
                        <tr>
                            <td style='padding: 8px 0; vertical-align: top;'><strong>Địa điểm:</strong></td>
                            <td style='padding: 8px 0; vertical-align: top;'>{location}</td>
                        </tr>
                        <tr>
                            <td style='padding: 8px 0; vertical-align: top;'><strong>Mô tả sự việc:</strong></td>
                            <td style='padding: 8px 0; vertical-align: top;'>{eventDescription}</td>
                        </tr>
                        <tr>
                            <td style='padding: 8px 0; vertical-align: top;'><strong>Triệu chứng:</strong></td>
                            <td style='padding: 8px 0; vertical-align: top;'>{symptoms}</td>
                        </tr>
                        <tr style='font-weight: bold;'>
                            <td style='padding: 8px 0; vertical-align: top;'><strong>Sơ cứu tại trường:</strong></td>
                            <td style='padding: 8px 0; vertical-align: top;'>{treatmentProvided}</td>
                        </tr>
                    </tbody>
                </table>
            </div>

            <p>Để đảm bảo Quý phụ huynh đã nắm được thông tin, vui lòng nhấn nút xác nhận bên dưới. Việc này rất quan trọng để nhà trường biết rằng thông báo đã được gửi đến Quý vị.</p>
            
            <div style='text-align: center; margin: 30px 0;'>
                <a href='{ackLink}' style='background-color: #4CAF50; color: white; padding: 14px 25px; text-decoration: none; border-radius: 8px; font-size: 16px; font-weight: bold; display: inline-block;'>✅ Xác nhận đã nhận thông báo</a>
            </div>

            <p><strong>Khuyến nghị:</strong> Quý phụ huynh vui lòng tiếp tục theo dõi tình trạng của con tại nhà. Nếu có bất kỳ câu hỏi nào hoặc cần trao đổi thêm, xin đừng ngần ngại liên hệ với Phòng Y tế.</p>
        </div>
        <div style='background-color: #f7f7f7; padding: 20px 30px; font-size: 12px; color: #666; text-align: left;'>
            <hr style='border: 0; border-top: 1px solid #ddd; margin-bottom: 15px;'>
            <strong>{_emailSettings.SchoolName}</strong><br>
            Địa chỉ: {_emailSettings.SchoolAddress}<br>
            Điện thoại khẩn cấp: {_emailSettings.SchoolPhone} | Email: {_emailSettings.HealthDepartmentEmail}
        </div>
    </div>
</body>
</html>";

            await QueueEmailAsync(parentEmail, subject, message);
        }

        private string GenerateVaccinationConsentEmail(string studentName, string vaccineName, DateTime scheduledDate)
        {
            return $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; margin: 0; padding: 20px; background-color: #f5f5f5; }}
                        .container {{ max-width: 600px; margin: 0 auto; background-color: white; padding: 30px; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
                        .header {{ text-align: center; border-bottom: 3px solid #2c5aa0; padding-bottom: 20px; margin-bottom: 30px; }}
                        .school-name {{ color: #2c5aa0; font-size: 24px; font-weight: bold; }}
                        .content {{ line-height: 1.6; }}
                        .highlight {{ background-color: #e3f2fd; padding: 15px; border-left: 4px solid #2196f3; margin: 20px 0; }}
                        .footer {{ margin-top: 30px; padding-top: 20px; border-top: 1px solid #e0e0e0; font-size: 12px; color: #666; }}
                        .btn {{ background-color: #4caf50; color: white; padding: 12px 25px; text-decoration: none; border-radius: 5px; display: inline-block; margin: 15px 0; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <div class='school-name'>{_emailSettings.SchoolName}</div>
                            <div>PHÒNG Y TẾ TRƯỜNG HỌC</div>
                        </div>
                        
                        <div class='content'>
                            <h2 style='color: #2c5aa0;'>THÔNG BÁO TIÊM CHỦNG</h2>
                            
                            <p>Kính gửi Quý phụ huynh,</p>
                            
                            <p>Trường {_emailSettings.SchoolName} trân trọng thông báo về lịch tiêm chủng cho học sinh <strong>{studentName}</strong>:</p>
                            
                            <div class='highlight'>
                                <strong>📅 Thông tin tiêm chủng:</strong><br>
                                • Loại vaccine: <strong>{vaccineName}</strong><br>
                                • Ngày tiêm dự kiến: <strong>{scheduledDate:dd/MM/yyyy}</strong><br>
                                • Thời gian: <strong>8:00 - 11:00</strong><br>
                                • Địa điểm: Phòng y tế trường
                            </div>
                            
                            <p><strong>Quý phụ huynh vui lòng:</strong></p>
                            <ul>
                                <li>Xác nhận đồng ý tiêm chủng cho con em qua đường link sau: <a href=""http://localhost:5173/parents/vaccine-overview"">link</a>
</li>
                                <li>Đảm bảo con em có mặt đúng thời gian</li>
                                <li>Thông báo nếu con em có tiền sử dị ứng</li>
                                <li>Cho con em ăn sáng đầy đủ trước khi tiêm</li>
                            </ul>
                            
                            <p>Mọi thắc mắc xin liên hệ phòng y tế: <strong>{_emailSettings.HealthDepartmentEmail}</strong></p>
                        </div>
                        
                        <div class='footer'>
                            <p><strong>{_emailSettings.SchoolName}</strong><br>
                            📍 {_emailSettings.SchoolAddress}<br>
                            📞 {_emailSettings.SchoolPhone}<br>
                            📧 {_emailSettings.FromEmail}</p>
                        </div>
                    </div>
                </body>
                </html>";
        }

        private string GenerateHealthCheckupNotificationEmail(string studentName, DateTime checkupDate)
        {
            return $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; margin: 0; padding: 20px; background-color: #f5f5f5; }}
                        .container {{ max-width: 600px; margin: 0 auto; background-color: white; padding: 30px; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
                        .header {{ text-align: center; border-bottom: 3px solid #4caf50; padding-bottom: 20px; margin-bottom: 30px; }}
                        .school-name {{ color: #4caf50; font-size: 24px; font-weight: bold; }}
                        .content {{ line-height: 1.6; }}
                        .highlight {{ background-color: #e8f5e8; padding: 15px; border-left: 4px solid #4caf50; margin: 20px 0; }}
                        .footer {{ margin-top: 30px; padding-top: 20px; border-top: 1px solid #e0e0e0; font-size: 12px; color: #666; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <div class='school-name'>{_emailSettings.SchoolName}</div>
                            <div>PHÒNG Y TẾ TRƯỜNG HỌC</div>
                        </div>
                        
                        <div class='content'>
                            <h2 style='color: #4caf50;'>THÔNG BÁO KIỂM TRA SỨC KHỎE ĐỊNH KỲ</h2>
                            
                            <p>Kính gửi Quý phụ huynh học sinh <strong>{studentName}</strong>,</p>
                            
                            <div class='highlight'>
                                <strong>🏥 Thông tin kiểm tra:</strong><br>
                                • Ngày kiểm tra: <strong>{checkupDate:dd/MM/yyyy}</strong><br>
                                • Thời gian: <strong>7:30 - 11:00</strong><br>
                                • Địa điểm: Phòng y tế trường<br>
                                • Nội dung: Khám tổng quát, đo chiều cao, cân nặng, kiểm tra thị lực, thính lực
                            </div>
                            
                            <p><strong>Chuẩn bị trước khi kiểm tra:</strong></p>
                            <ul>
                                <li>Xác nhận đồng ý khám cho con em qua đường link sau: <a href=""http://localhost:5173/checkup-schedules"">link</a>
                                <li>Cho con em ăn sáng nhẹ</li>
                                <li>Đảm bảo con em ngủ đủ giấc</li>
                                <li>Mang theo sổ sức khỏe (nếu có)</li>
                                <li>Thông báo tình trạng sức khỏe đặc biệt</li>
                            </ul>
                        </div>
                        
                        <div class='footer'>
                            <p><strong>{_emailSettings.SchoolName}</strong><br>
                            📍 {_emailSettings.SchoolAddress}<br>
                            📞 {_emailSettings.SchoolPhone}</p>
                        </div>
                    </div>
                </body>
                </html>";
        }

        private string GenerateHealthEventNotificationEmail(string studentName, string eventDescription, string treatmentProvided)
        {
            return $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; margin: 0; padding: 20px; background-color: #fff3e0; }}
                        .container {{ max-width: 600px; margin: 0 auto; background-color: white; padding: 30px; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); border: 3px solid #ff9800; }}
                        .header {{ text-align: center; border-bottom: 3px solid #ff5722; padding-bottom: 20px; margin-bottom: 30px; }}
                        .school-name {{ color: #ff5722; font-size: 24px; font-weight: bold; }}
                        .content {{ line-height: 1.6; }}
                        .alert {{ background-color: #ffebee; padding: 15px; border-left: 4px solid #f44336; margin: 20px 0; }}
                        .treatment {{ background-color: #e8f5e8; padding: 15px; border-left: 4px solid #4caf50; margin: 20px 0; }}
                        .footer {{ margin-top: 30px; padding-top: 20px; border-top: 1px solid #e0e0e0; font-size: 12px; color: #666; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <div class='school-name'>{_emailSettings.SchoolName}</div>
                            <div style='color: #ff5722; font-weight: bold;'>🚨 THÔNG BÁO Y TẾ KHẨN CẤP</div>
                        </div>
                        
                        <div class='content'>
                            <h2 style='color: #ff5722;'>THÔNG BÁO SỰ KIỆN Y TẾ</h2>
                            
                            <p>Kính gửi Quý phụ huynh học sinh <strong>{studentName}</strong>,</p>
                            
                            <p>Chúng tôi thông báo về sự kiện y tế xảy ra với con em Quý vị:</p>
                            
                            <div class='alert'>
                                <strong>⚠️ Mô tả sự kiện:</strong><br>
                                {eventDescription}
                            </div>
                            
                            <div class='treatment'>
                                <strong>🏥 Xử lý đã thực hiện:</strong><br>
                                {treatmentProvided}
                            </div>
                            
                            <p><strong>Khuyến nghị:</strong></p>
                            <ul>
                                <li>Theo dõi tình trạng sức khỏe của con em</li>
                                <li>Liên hệ ngay với trường nếu có biến chứng</li>
                                <li>Đưa con đến cơ sở y tế nếu cần thiết</li>
                                <li>Thông báo cho giáo viên chủ nhiệm về tình trạng của con</li>
                            </ul>
                            
                            <p style='color: #f44336; font-weight: bold;'>
                                📞 Liên hệ khẩn cấp: {_emailSettings.SchoolPhone}<br>
                                📧 Email: {_emailSettings.HealthDepartmentEmail}
                            </p>
                        </div>
                        
                        <div class='footer'>
                            <p>Thời gian gửi: {DateTime.Now:dd/MM/yyyy HH:mm}<br>
                            <strong>{_emailSettings.SchoolName} - Phòng Y tế</strong></p>
                        </div>
                    </div>
                </body>
                </html>";
        }

        private string GenerateMedicationDeliveryConfirmationEmail(string studentName, string medicationName)
        {
            return $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; margin: 0; padding: 20px; background-color: #f5f5f5; }}
                        .container {{ max-width: 600px; margin: 0 auto; background-color: white; padding: 30px; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
                        .header {{ text-align: center; border-bottom: 3px solid #9c27b0; padding-bottom: 20px; margin-bottom: 30px; }}
                        .school-name {{ color: #9c27b0; font-size: 24px; font-weight: bold; }}
                        .content {{ line-height: 1.6; }}
                        .highlight {{ background-color: #f3e5f5; padding: 15px; border-left: 4px solid #9c27b0; margin: 20px 0; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <div class='school-name'>{_emailSettings.SchoolName}</div>
                            <div>XÁC NHẬN NHẬN THUỐC</div>
                        </div>
                        
                        <div class='content'>
                            <h2 style='color: #9c27b0;'>XÁC NHẬN GIAO NHẬN THUỐC</h2>
                            
                            <p>Kính gửi Quý phụ huynh học sinh <strong>{studentName}</strong>,</p>
                            
                            <div class='highlight'>
                                <strong>💊 Thuốc đã nhận:</strong><br>
                                • Tên thuốc: <strong>{medicationName}</strong><br>
                                • Thời gian nhận: <strong>{DateTime.Now:dd/MM/yyyy HH:mm}</strong><br>
                                • Người nhận: Cô/Thầy y tế trường<br>
                                • Trạng thái: Đã lưu trữ an toàn
                            </div>
                            
                            <p>Phòng y tế sẽ:</p>
                            <ul>
                                <li>Bảo quản thuốc theo đúng hướng dẫn</li>
                                <li>Cho học sinh uống đúng liều lượng và thời gian</li>
                                <li>Ghi chép đầy đủ trong sổ theo dõi</li>
                                <li>Thông báo nếu có bất thường</li>
                            </ul>
                        </div>
                    </div>
                </body>
                </html>";
        }

        private string GenerateMedicationDeliveredEmail(string studentName, string medicationName)
        {
            return $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; margin: 0; padding: 20px; background-color: #f5f5f5; }}
                        .container {{ max-width: 600px; margin: 0 auto; background-color: white; padding: 30px; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
                        .header {{ text-align: center; border-bottom: 3px solid #9c27b0; padding-bottom: 20px; margin-bottom: 30px; }}
                        .school-name {{ color: #9c27b0; font-size: 24px; font-weight: bold; }}
                        .content {{ line-height: 1.6; }}
                        .highlight {{ background-color: #f3e5f5; padding: 15px; border-left: 4px solid #9c27b0; margin: 20px 0; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <div class='school-name'>{_emailSettings.SchoolName}</div>
                            <div>XÁC NHẬN ĐÃ GIAO THUỐC</div>
                        </div>
                        
                        <div class='content'>
                            <h2 style='color: #9c27b0;'>XÁC NHẬN GIAO NHẬN THUỐC</h2>
                            
                            <p>Kính gửi Quý phụ huynh học sinh <strong>{studentName}</strong>,</p>
                            
                            <div class='highlight'>
                                <strong>💊 Thuốc đã giao:</strong><br>
                                • Tên thuốc: <strong>{medicationName}</strong><br>
                                • Thời gian nhận: <strong>{DateTime.Now:dd/MM/yyyy HH:mm}</strong><br>
                                • Người giao: Cô/Thầy y tế trường<br>
                                • Trạng thái: Đã giao an toàn
                            </div>
                        </div>
                    </div>
                </body>
                </html>";
        }

        private string GenerateLowStockAlertEmail(List<string> lowStockItems)
        {
            var itemsList = string.Join("<br>• ", lowStockItems.Select(item => $"<strong>{item}</strong>"));

            return $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; margin: 0; padding: 20px; background-color: #fff3e0; }}
                        .container {{ max-width: 600px; margin: 0 auto; background-color: white; padding: 30px; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); border: 3px solid #ff9800; }}
                        .header {{ text-align: center; border-bottom: 3px solid #ff9800; padding-bottom: 20px; margin-bottom: 30px; }}
                        .warning {{ background-color: #fff3e0; padding: 15px; border-left: 4px solid #ff9800; margin: 20px 0; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <div style='color: #ff9800; font-size: 24px; font-weight: bold;'>{_emailSettings.SchoolName}</div>
                            <div style='color: #ff9800; font-weight: bold;'>⚠️ CẢNH BÁO THIẾU VẬT TƯ Y TẾ</div>
                        </div>
                        
                        <div class='warning'>
                            <strong>📦 Vật tư sắp hết:</strong><br>
                            • {itemsList}
                        </div>
                        
                        <p><strong>Cần thực hiện ngay:</strong></p>
                        <ul>
                            <li>Kiểm tra số lượng tồn kho</li>
                            <li>Lập đơn đặt hàng bổ sung</li>
                            <li>Thông báo ban giám hiệu</li>
                        </ul>
                    </div>
                </body>
                </html>";
        }

        private string GenerateCampaignReminderEmail(string campaignName, DateTime startDate, string campaignType)
        {
            return $@"
                <html>
                <body style='font-family: Arial, sans-serif; padding: 20px;'>
                    <div style='max-width: 600px; margin: 0 auto; background-color: white; padding: 30px; border-radius: 10px;'>
                        <h2 style='color: #2c5aa0;'>NHẮC NHỞ CHIẾN DỊCH {campaignType.ToUpper()}</h2>
                        
                        <p><strong>Chiến dịch:</strong> {campaignName}</p>
                        <p><strong>Ngày bắt đầu:</strong> {startDate:dd/MM/yyyy}</p>
                        <p><strong>Còn lại:</strong> {(startDate - DateTime.Now).Days} ngày</p>
                        
                        <p><strong>Cần chuẩn bị:</strong></p>
                        <ul>
                            <li>Kiểm tra danh sách học sinh</li>
                            <li>Chuẩn bị vật tư y tế</li>
                            <li>Thông báo cho phụ huynh</li>
                            <li>Cập nhật lịch trình</li>
                        </ul>
                    </div>
                </body>
                </html>";
        }

        private string GenerateDailyHealthReportEmail(string reportContent)
        {
            return $@"
                <html>
                <body style='font-family: Arial, sans-serif; padding: 20px;'>
                    <div style='max-width: 600px; margin: 0 auto;'>
                        <h2 style='color: #4caf50;'>BÁO CÁO Y TẾ HÀNG NGÀY</h2>
                        <p><strong>Ngày:</strong> {DateTime.Now:dd/MM/yyyy}</p>
                        <div style='background-color: #f5f5f5; padding: 15px; border-radius: 5px;'>
                            {reportContent}
                        </div>
                    </div>
                </body>
                </html>";
        }

        private string GenerateMonthlyReportEmail(string reportPeriod, string reportSummary)
        {
            return $@"
                <html>
                <body style='font-family: Arial, sans-serif; padding: 20px;'>
                    <div style='max-width: 600px; margin: 0 auto;'>
                        <h2 style='color: #2c5aa0;'>BÁO CÁO Y TẾ THÁNG {reportPeriod}</h2>
                        <div style='background-color: #f8f9fa; padding: 20px; border-radius: 5px;'>
                            {reportSummary}
                        </div>
                    </div>
                </body>
                </html>";
        }

        private string GenerateEmergencyEventEmail(string studentName, string emergencyDetails)
        {
            return $@"
                <html>
                <body style='font-family: Arial, sans-serif; padding: 20px; background-color: #ffebee;'>
                    <div style='max-width: 600px; margin: 0 auto; background-color: white; padding: 30px; border: 3px solid #f44336;'>
                        <h2 style='color: #f44336;'>🚨 KHẨN CẤP Y TẾ</h2>
                        <p><strong>Học sinh:</strong> {studentName}</p>
                        <div style='background-color: #ffebee; padding: 15px; border-left: 4px solid #f44336;'>
                            <strong>Chi tiết:</strong><br>
                            {emergencyDetails}
                        </div>
                        <p style='color: #f44336; font-weight: bold;'>
                            Vui lòng liên hệ ngay: {_emailSettings.SchoolPhone}
                        </p>
                    </div>
                </body>
                </html>";
        }

        #endregion

        private async Task QueueEmailAsync(string email, string subject, string message)
        {
            try
            {
                await _emailQueueService.QueueEmailAsync(email, subject, message);
                _logger.LogInformation("Email queued successfully for {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to queue email for {Email}", email);
                throw;
            }
        }

        private async Task QueueEmailAsync(List<string> emails, string subject, string message)
        {
            try
            {
                await _emailQueueService.QueueEmailAsync(emails, subject, message);
                _logger.LogInformation("Email queued successfully for {EmailCount} recipients", emails.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to queue email for {EmailCount} recipients", emails.Count);
                throw;
            }
        }
    }
}
