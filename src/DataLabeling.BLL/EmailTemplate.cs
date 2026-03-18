using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLabeling.BLL
{
    public class EmailTemplate
    {
        public static string GetAccountCreationEmail(string fullName, string email, string password)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <style>
        body {{
            font-family: Arial, sans-serif;
            line-height: 1.6;
            color: #333;
            margin: 0;
            padding: 0;
            background-color: #f4f4f4;
        }}
        .container {{
            max-width: 600px;
            margin: 20px auto;
            background-color: #ffffff;
            border-radius: 8px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
            overflow: hidden;
        }}
        .header {{
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            padding: 30px 20px;
            text-align: center;
        }}
        .header h1 {{
            margin: 0;
            font-size: 24px;
        }}
        .content {{
            padding: 30px 40px;
        }}
        .credentials {{
            background-color: #f8f9fa;
            border-left: 4px solid #667eea;
            padding: 20px;
            margin: 20px 0;
            border-radius: 4px;
        }}
        .credentials-item {{
            margin: 10px 0;
        }}
        .credentials-label {{
            font-weight: bold;
            color: #555;
            display: inline-block;
            width: 100px;
        }}
        .credentials-value {{
            color: #333;
            font-family: 'Courier New', monospace;
            background-color: #fff;
            padding: 5px 10px;
            border-radius: 3px;
            display: inline-block;
        }}
        .password-value {{
            font-size: 18px;
            font-weight: bold;
            color: #667eea;
        }}
        .warning {{
            background-color: #fff3cd;
            border-left: 4px solid #ffc107;
            padding: 15px;
            margin: 20px 0;
            border-radius: 4px;
        }}
        .warning-icon {{
            color: #856404;
            font-weight: bold;
        }}
        .footer {{
            background-color: #f8f9fa;
            padding: 20px;
            text-align: center;
            font-size: 12px;
            color: #666;
        }}
        .btn {{
            display: inline-block;
            padding: 12px 30px;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            text-decoration: none;
            border-radius: 5px;
            margin: 20px 0;
            font-weight: bold;
        }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>🎉 Tài Khoản Của Bạn Đã Được Tạo</h1>
        </div>
        
        <div class=""content"">
            <p>Xin chào <strong>{fullName}</strong>,</p>
            
            <p>Tài khoản của bạn đã được tạo thành công trên hệ thống Data Labeling. Dưới đây là thông tin đăng nhập của bạn:</p>
            
            <div class=""credentials"">
                <div class=""credentials-item"">
                    <span class=""credentials-label"">Email:</span>
                    <span class=""credentials-value"">{email}</span>
                </div>
                <div class=""credentials-item"">
                    <span class=""credentials-label"">Mật khẩu:</span>
                    <span class=""credentials-value password-value"">{password}</span>
                </div>
            </div>
            
            <div class=""warning"">
                <p class=""warning-icon"">⚠️ LƯU Ý QUAN TRỌNG:</p>
                <ul style=""margin: 10px 0; padding-left: 20px;"">
                    <li>Vui lòng <strong>lưu lại mật khẩu này</strong> ngay lập tức</li>
                    <li>Đổi mật khẩu ngay sau lần đăng nhập đầu tiên để đảm bảo bảo mật</li>
                    <li>Không chia sẻ thông tin đăng nhập với người khác</li>
                </ul>
            </div>
            
            <p>Bạn có thể đăng nhập vào hệ thống và bắt đầu sử dụng các tính năng dành cho bạn.</p>
            
            <p>Nếu bạn có bất kỳ câu hỏi nào, vui lòng liên hệ với quản trị viên hệ thống.</p>
            
            <p style=""margin-top: 30px;"">
                Trân trọng,<br>
                <strong>Data Labeling Team</strong>
            </p>
        </div>
        
        <div class=""footer"">
            <p>Email này được gửi tự động, vui lòng không trả lời.</p>
            <p>&copy; 2026 Data Labeling System. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
        }

        public static string GetForgotPasswordOtpEmail(string fullName, string otp)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <style>
        body {{
            font-family: Arial, sans-serif;
            line-height: 1.6;
            color: #333;
            margin: 0;
            padding: 0;
            background-color: #f4f4f4;
        }}
        .container {{
            max-width: 600px;
            margin: 20px auto;
            background-color: #ffffff;
            border-radius: 8px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
            overflow: hidden;
        }}
        .header {{
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            padding: 30px 20px;
            text-align: center;
        }}
        .header h1 {{
            margin: 0;
            font-size: 24px;
        }}
        .content {{
            padding: 30px 40px;
        }}
        .otp-box {{
            background-color: #f8f9fa;
            border: 2px dashed #667eea;
            padding: 30px;
            margin: 20px 0;
            border-radius: 8px;
            text-align: center;
        }}
        .otp-code {{
            font-size: 36px;
            font-weight: bold;
            color: #667eea;
            letter-spacing: 8px;
            font-family: 'Courier New', monospace;
            margin: 10px 0;
        }}
        .warning {{
            background-color: #fff3cd;
            border-left: 4px solid #ffc107;
            padding: 15px;
            margin: 20px 0;
            border-radius: 4px;
        }}
        .warning-icon {{
            color: #856404;
            font-weight: bold;
        }}
        .footer {{
            background-color: #f8f9fa;
            padding: 20px;
            text-align: center;
            font-size: 12px;
            color: #666;
        }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>🔐 Mã Xác Thực OTP</h1>
        </div>
        
        <div class=""content"">
            <p>Xin chào <strong>{fullName}</strong>,</p>
            
            <p>Bạn đã yêu cầu đặt lại mật khẩu cho tài khoản Data Labeling của mình. Vui lòng sử dụng mã OTP dưới đây để xác thực:</p>
            
            <div class=""otp-box"">
                <p style=""margin: 0; font-size: 14px; color: #666;"">Mã OTP của bạn</p>
                <div class=""otp-code"">{otp}</div>
                <p style=""margin: 10px 0 0 0; font-size: 12px; color: #999;"">Mã này có hiệu lực trong 5 phút</p>
            </div>
            
            <div class=""warning"">
                <p class=""warning-icon"">⚠️ LƯU Ý QUAN TRỌNG:</p>
                <ul style=""margin: 10px 0; padding-left: 20px;"">
                    <li>Không chia sẻ mã OTP này với bất kỳ ai</li>
                    <li>Mã OTP chỉ có hiệu lực trong <strong>5 phút</strong></li>
                    <li>Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này</li>
                </ul>
            </div>
            
            <p>Sau khi xác thực OTP thành công, bạn sẽ có thể đặt mật khẩu mới cho tài khoản của mình.</p>
            
            <p style=""margin-top: 30px;"">
                Trân trọng,<br>
                <strong>Data Labeling Team</strong>
            </p>
        </div>
        
        <div class=""footer"">
            <p>Email này được gửi tự động, vui lòng không trả lời.</p>
            <p>&copy; 2026 Data Labeling System. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
        }
    }
}
