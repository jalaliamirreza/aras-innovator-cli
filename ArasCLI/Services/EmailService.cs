using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mail;

namespace ArasCLI.Services
{
    /// <summary>
    /// Service for sending email notifications.
    /// </summary>
    public class EmailService
    {
        private readonly AppConfig _config;

        public EmailService(AppConfig config)
        {
            _config = config;
        }

        /// <summary>
        /// Check if email settings are configured.
        /// </summary>
        public bool IsConfigured()
        {
            return !string.IsNullOrEmpty(_config.SmtpServer) &&
                   !string.IsNullOrEmpty(_config.ReviewerEmail) &&
                   !string.IsNullOrEmpty(_config.SenderEmail);
        }

        /// <summary>
        /// Send review notification email with beautiful HTML formatting.
        /// </summary>
        /// <param name="itemNumber">Document item number</param>
        /// <param name="itemName">Document name</param>
        /// <param name="revision">Document revision</param>
        /// <param name="submitterName">Name of the person submitting for review</param>
        /// <param name="itemId">Document item ID (optional)</param>
        /// <param name="itemType">Document item type (optional)</param>
        /// <param name="attachmentPath">Path to file to attach (optional)</param>
        /// <returns>True if email sent successfully</returns>
        public bool SendReviewNotification(string itemNumber, string itemName, string revision, string submitterName, string itemId = null, string itemType = null, string attachmentPath = null)
        {
            if (!IsConfigured())
            {
                return false;
            }

            try
            {
                string subject = $"[Review Required] {itemNumber} Rev {revision} - {itemName}";

                // Build Aras document link
                string arasLink = "";
                if (!string.IsNullOrEmpty(itemId) && !string.IsNullOrEmpty(itemType) && !string.IsNullOrEmpty(_config.ArasServerUrl))
                {
                    string baseUrl = _config.ArasServerUrl.TrimEnd('/');
                    arasLink = $"{baseUrl}/Client/scripts/Nash.aspx?ItemType={itemType}&ID={itemId}";
                }

                // Build beautiful HTML email
                string htmlBody = BuildHtmlEmail(itemNumber, itemName, revision, submitterName, itemType, itemId, arasLink, attachmentPath);

                // Generate approval HTML file to attach
                string approvalHtmlPath = null;
                if (!string.IsNullOrEmpty(itemId) && !string.IsNullOrEmpty(_config.ArasServerUrl))
                {
                    approvalHtmlPath = GenerateApprovalHtml(itemId, itemType ?? "Document", itemNumber, itemName);
                }

                // Send with both attachments (document + approval HTML)
                var attachments = new List<string>();
                if (!string.IsNullOrEmpty(attachmentPath) && File.Exists(attachmentPath))
                {
                    attachments.Add(attachmentPath);
                }
                if (!string.IsNullOrEmpty(approvalHtmlPath) && File.Exists(approvalHtmlPath))
                {
                    attachments.Add(approvalHtmlPath);
                }

                bool result = SendHtmlEmailWithMultipleAttachments(_config.ReviewerEmail, subject, htmlBody, attachments);

                // Clean up temp approval HTML
                if (!string.IsNullOrEmpty(approvalHtmlPath) && File.Exists(approvalHtmlPath))
                {
                    try { File.Delete(approvalHtmlPath); } catch { }
                }

                return result;
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Generate approval HTML file with embedded item info.
        /// </summary>
        private string GenerateApprovalHtml(string itemId, string itemType, string itemNumber, string itemName)
        {
            string serverUrl = _config.ArasServerUrl.TrimEnd('/');

            string html = $@"<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Approve Document {itemNumber} - Aras PLM</title>
    <style>
        * {{ box-sizing: border-box; }}
        body {{
            margin: 0; padding: 40px;
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Arial, sans-serif;
            background-color: #f5f7fa;
            min-height: 100vh;
        }}
        .card {{
            background: #fff;
            border-radius: 12px;
            box-shadow: 0 4px 20px rgba(0,0,0,0.1);
            overflow: hidden;
            max-width: 600px;
            margin: 0 auto;
        }}
        .header {{
            padding: 30px 40px;
            text-align: center;
            color: #fff;
            background: linear-gradient(135deg, #0066cc 0%, #004499 100%);
        }}
        .header h1 {{ margin: 0; font-size: 24px; font-weight: 600; }}
        .header p {{ margin: 10px 0 0 0; opacity: 0.9; }}
        .content {{ padding: 40px; }}
        .info-box {{
            background: #f8fafc;
            border-radius: 10px;
            padding: 20px;
            margin-bottom: 25px;
            border: 1px solid #e2e8f0;
        }}
        .info-row {{
            display: flex;
            padding: 10px 0;
            border-bottom: 1px solid #e2e8f0;
        }}
        .info-row:last-child {{ border-bottom: none; }}
        .info-label {{ color: #666; width: 120px; font-weight: 500; }}
        .info-value {{ color: #333; font-weight: 500; }}
        .buttons {{
            display: flex;
            gap: 15px;
            justify-content: center;
            margin: 30px 0;
        }}
        .btn {{
            padding: 14px 35px;
            border: none;
            border-radius: 8px;
            font-size: 16px;
            font-weight: 600;
            cursor: pointer;
            transition: transform 0.2s;
        }}
        .btn:hover {{ transform: translateY(-2px); }}
        .btn-approve {{
            background: linear-gradient(135deg, #10b981 0%, #059669 100%);
            color: white;
        }}
        .btn-reject {{
            background: linear-gradient(135deg, #ef4444 0%, #dc2626 100%);
            color: white;
        }}
        .result {{ display: none; text-align: center; padding: 20px; border-radius: 10px; margin-top: 20px; }}
        .result.success {{ display: block; background: #d1fae5; color: #065f46; }}
        .result.error {{ display: block; background: #fee2e2; color: #991b1b; }}
        .result.warning {{ display: block; background: #fef3c7; color: #92400e; }}
        .spinner {{
            border: 4px solid #e2e8f0;
            border-top: 4px solid #0066cc;
            border-radius: 50%;
            width: 30px; height: 30px;
            animation: spin 1s linear infinite;
            margin: 0 auto;
        }}
        @keyframes spin {{ 0% {{ transform: rotate(0deg); }} 100% {{ transform: rotate(360deg); }} }}
        .footer {{
            background: #f8fafc;
            padding: 20px 40px;
            text-align: center;
            border-top: 1px solid #e2e8f0;
        }}
        .footer p {{ margin: 0; color: #0066cc; font-size: 14px; font-weight: 600; }}
    </style>
</head>
<body>
    <div class=""card"">
        <div class=""header"">
            <h1>📋 Document Approval</h1>
            <p>Review and approve or reject this document</p>
        </div>
        <div class=""content"">
            <div class=""info-box"">
                <div class=""info-row"">
                    <span class=""info-label"">Document:</span>
                    <span class=""info-value"">{itemNumber}</span>
                </div>
                <div class=""info-row"">
                    <span class=""info-label"">Name:</span>
                    <span class=""info-value"">{itemName}</span>
                </div>
                <div class=""info-row"">
                    <span class=""info-label"">Type:</span>
                    <span class=""info-value"">{itemType}</span>
                </div>
                <div class=""info-row"">
                    <span class=""info-label"">Status:</span>
                    <span class=""info-value"" style=""color: #f59e0b; font-weight: bold;"">In Review</span>
                </div>
            </div>

            <div class=""buttons"" id=""buttons"">
                <button class=""btn btn-approve"" onclick=""processAction('approve')"">✅ Approve & Release</button>
                <button class=""btn btn-reject"" onclick=""processAction('reject')"">❌ Reject</button>
            </div>

            <div id=""loading"" style=""display: none; text-align: center;"">
                <div class=""spinner""></div>
                <p>Processing...</p>
            </div>

            <div id=""result"" class=""result""></div>
        </div>
        <div class=""footer"">
            <p>🔧 CATIA - Aras PLM Integration</p>
        </div>
    </div>

    <script>
        const CONFIG = {{
            serverUrl: '{serverUrl}',
            apiKey: '{_config.ApprovalApiKey ?? ""}',
            itemId: '{itemId}',
            itemType: '{itemType}'
        }};

        async function processAction(action) {{
            document.getElementById('buttons').style.display = 'none';
            document.getElementById('loading').style.display = 'block';
            document.getElementById('result').className = 'result';
            document.getElementById('result').style.display = 'none';

            try {{
                const cwsUrl = CONFIG.serverUrl + '/Server/ws/PLM_Approval/v1/PLM_ApproveDocument';

                const response = await fetch(cwsUrl, {{
                    method: 'POST',
                    headers: {{
                        'Content-Type': 'application/json',
                        'Authorization': 'ApiKey ' + CONFIG.apiKey
                    }},
                    body: JSON.stringify({{
                        id: CONFIG.itemId,
                        type: CONFIG.itemType,
                        action: action
                    }})
                }});

                document.getElementById('loading').style.display = 'none';

                if (!response.ok) {{
                    throw new Error('Server error: ' + response.status);
                }}

                const data = await response.json();
                const message = data.value || data.message || '';

                if (message.toLowerCase().indexOf('error') >= 0 || message.toLowerCase().indexOf('failed') >= 0) {{
                    throw new Error(message);
                }} else if (action === 'approve') {{
                    showResult('success', message || 'Document has been approved and released!');
                }} else {{
                    showResult('warning', message || 'Document has been rejected and returned to Preliminary.');
                }}
            }} catch (error) {{
                document.getElementById('loading').style.display = 'none';
                showResult('error', '❌ Error: ' + error.message);
                document.getElementById('buttons').style.display = 'flex';
            }}
        }}

        function showResult(type, message) {{
            const result = document.getElementById('result');
            result.className = 'result ' + type;
            result.innerHTML = '<p style=""margin:0;font-size:16px;"">' + message + '</p>';
            result.style.display = 'block';
        }}
    </script>
</body>
</html>";

            // Save to temp file
            string tempPath = Path.Combine(Path.GetTempPath(), $"Approve_{itemNumber.Replace("/", "_")}.html");
            File.WriteAllText(tempPath, html);
            return tempPath;
        }

        /// <summary>
        /// Build beautiful HTML email body.
        /// </summary>
        private string BuildHtmlEmail(string itemNumber, string itemName, string revision, string submitterName, string itemType, string itemId, string arasLink, string attachmentPath)
        {
            string attachmentInfo = "";
            if (!string.IsNullOrEmpty(attachmentPath) && File.Exists(attachmentPath))
            {
                string fileName = Path.GetFileName(attachmentPath);
                var fileInfo = new FileInfo(attachmentPath);
                string fileSize = FormatFileSize(fileInfo.Length);
                attachmentInfo = $@"
                <tr>
                    <td style='padding: 12px 15px; border-bottom: 1px solid #e0e0e0; color: #666; font-weight: 500;'>Attachment</td>
                    <td style='padding: 12px 15px; border-bottom: 1px solid #e0e0e0; color: #333;'>
                        <span style='background: #e3f2fd; padding: 4px 10px; border-radius: 4px; font-size: 13px;'>
                            📎 {fileName} ({fileSize})
                        </span>
                    </td>
                </tr>";
            }

            // Build approve/reject action URLs
            string approveUrl = "";
            string rejectUrl = "";
            string buttonHtml = "";

            if (!string.IsNullOrEmpty(itemId) && !string.IsNullOrEmpty(_config.ArasServerUrl))
            {
                string baseUrl = _config.ArasServerUrl.TrimEnd('/');
                // Use hosted HTML page that calls CWS endpoint (deploy PLM_ApproveDocument.html to Aras server)
                approveUrl = $"{baseUrl}/PLM_ApproveDocument.html?id={itemId}&type={itemType ?? "Document"}&action=approve";
                rejectUrl = $"{baseUrl}/PLM_ApproveDocument.html?id={itemId}&type={itemType ?? "Document"}&action=reject";

                buttonHtml = $@"
                <div style='text-align: center; margin: 30px 0;'>
                    <a href='{approveUrl}' style='display: inline-block; background: linear-gradient(135deg, #10b981 0%, #059669 100%); color: white; padding: 14px 35px; text-decoration: none; border-radius: 8px; font-weight: 600; font-size: 15px; box-shadow: 0 4px 15px rgba(16,185,129,0.3); margin-right: 15px;'>
                        ✅ Approve & Release
                    </a>
                    <a href='{rejectUrl}' style='display: inline-block; background: linear-gradient(135deg, #ef4444 0%, #dc2626 100%); color: white; padding: 14px 35px; text-decoration: none; border-radius: 8px; font-weight: 600; font-size: 15px; box-shadow: 0 4px 15px rgba(239,68,68,0.3);'>
                        ❌ Reject
                    </a>
                </div>
                <div style='text-align: center; margin: 15px 0;'>
                    <a href='{arasLink}' style='color: #0066cc; font-size: 13px; text-decoration: none;'>
                        🔗 Open Document in Aras for detailed review
                    </a>
                </div>";
            }
            else if (!string.IsNullOrEmpty(arasLink))
            {
                buttonHtml = $@"
                <div style='text-align: center; margin: 30px 0;'>
                    <a href='{arasLink}' style='display: inline-block; background: linear-gradient(135deg, #0066cc 0%, #004499 100%); color: white; padding: 14px 35px; text-decoration: none; border-radius: 8px; font-weight: 600; font-size: 15px; box-shadow: 0 4px 15px rgba(0,102,204,0.3);'>
                        🔗 Open Document in Aras
                    </a>
                </div>";
            }

            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
</head>
<body style='margin: 0; padding: 0; font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, ""Helvetica Neue"", Arial, sans-serif; background-color: #f5f7fa;'>
    <table width='100%' cellpadding='0' cellspacing='0' style='background-color: #f5f7fa; padding: 40px 20px;'>
        <tr>
            <td align='center'>
                <table width='600' cellpadding='0' cellspacing='0' style='background-color: #ffffff; border-radius: 12px; box-shadow: 0 4px 20px rgba(0,0,0,0.1); overflow: hidden;'>

                    <!-- Header -->
                    <tr>
                        <td style='background: linear-gradient(135deg, #0066cc 0%, #004499 100%); padding: 35px 40px; text-align: center;'>
                            <h1 style='margin: 0; color: #ffffff; font-size: 24px; font-weight: 600; letter-spacing: -0.5px;'>
                                📋 Document Review Request
                            </h1>
                            <p style='margin: 10px 0 0 0; color: rgba(255,255,255,0.85); font-size: 14px;'>
                                A new document requires your attention
                            </p>
                        </td>
                    </tr>

                    <!-- Main Content -->
                    <tr>
                        <td style='padding: 40px;'>

                            <!-- Greeting -->
                            <p style='margin: 0 0 25px 0; color: #333; font-size: 16px; line-height: 1.6;'>
                                Hello,<br><br>
                                <strong style='color: #0066cc;'>{submitterName}</strong> has submitted a document for your review. Please find the details below:
                            </p>

                            <!-- Document Details Card -->
                            <div style='background: #f8fafc; border-radius: 10px; padding: 5px; margin: 25px 0; border: 1px solid #e2e8f0;'>
                                <table width='100%' cellpadding='0' cellspacing='0' style='font-size: 14px;'>
                                    <tr>
                                        <td style='padding: 12px 15px; border-bottom: 1px solid #e0e0e0; color: #666; font-weight: 500; width: 140px;'>Item Number</td>
                                        <td style='padding: 12px 15px; border-bottom: 1px solid #e0e0e0;'>
                                            <span style='background: #0066cc; color: white; padding: 4px 12px; border-radius: 4px; font-weight: 600; font-size: 13px;'>{itemNumber}</span>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td style='padding: 12px 15px; border-bottom: 1px solid #e0e0e0; color: #666; font-weight: 500;'>Document Name</td>
                                        <td style='padding: 12px 15px; border-bottom: 1px solid #e0e0e0; color: #333; font-weight: 500;'>{itemName}</td>
                                    </tr>
                                    <tr>
                                        <td style='padding: 12px 15px; border-bottom: 1px solid #e0e0e0; color: #666; font-weight: 500;'>Revision</td>
                                        <td style='padding: 12px 15px; border-bottom: 1px solid #e0e0e0;'>
                                            <span style='background: #10b981; color: white; padding: 4px 12px; border-radius: 4px; font-weight: 600; font-size: 13px;'>Rev {revision}</span>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td style='padding: 12px 15px; border-bottom: 1px solid #e0e0e0; color: #666; font-weight: 500;'>Type</td>
                                        <td style='padding: 12px 15px; border-bottom: 1px solid #e0e0e0; color: #333;'>{itemType ?? "Document"}</td>
                                    </tr>
                                    <tr>
                                        <td style='padding: 12px 15px; border-bottom: 1px solid #e0e0e0; color: #666; font-weight: 500;'>Status</td>
                                        <td style='padding: 12px 15px; border-bottom: 1px solid #e0e0e0;'>
                                            <span style='background: #f59e0b; color: white; padding: 4px 12px; border-radius: 4px; font-weight: 600; font-size: 13px;'>In Review</span>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td style='padding: 12px 15px; border-bottom: 1px solid #e0e0e0; color: #666; font-weight: 500;'>Submitted By</td>
                                        <td style='padding: 12px 15px; border-bottom: 1px solid #e0e0e0; color: #333;'>{submitterName}</td>
                                    </tr>
                                    <tr>
                                        <td style='padding: 12px 15px; border-bottom: 1px solid #e0e0e0; color: #666; font-weight: 500;'>Date & Time</td>
                                        <td style='padding: 12px 15px; border-bottom: 1px solid #e0e0e0; color: #333;'>{DateTime.Now:dddd, MMMM dd, yyyy - HH:mm}</td>
                                    </tr>
                                    {attachmentInfo}
                                </table>
                            </div>

                            <!-- Action Button -->
                            {buttonHtml}

                            <!-- Instructions -->
                            <div style='background: #fffbeb; border-left: 4px solid #f59e0b; padding: 15px 20px; margin: 25px 0; border-radius: 0 8px 8px 0;'>
                                <p style='margin: 0; color: #92400e; font-size: 14px; line-height: 1.5;'>
                                    <strong>⚡ Action Required:</strong> Click <b>Approve & Release</b> to approve the document, or <b>Reject</b> to return it for revision.
                                </p>
                            </div>

                            <!-- Attached Approval File Note -->
                            <div style='background: #e0f2fe; border-left: 4px solid #0284c7; padding: 15px 20px; margin: 25px 0; border-radius: 0 8px 8px 0;'>
                                <p style='margin: 0; color: #0369a1; font-size: 14px; line-height: 1.5;'>
                                    <strong>📎 Alternative:</strong> Open the attached <b>Approve_*.html</b> file in your browser for one-click approval/rejection.
                                </p>
                            </div>

                        </td>
                    </tr>

                    <!-- Footer -->
                    <tr>
                        <td style='background: #f8fafc; padding: 25px 40px; border-top: 1px solid #e2e8f0;'>
                            <table width='100%' cellpadding='0' cellspacing='0'>
                                <tr>
                                    <td style='text-align: center;'>
                                        <p style='margin: 0 0 8px 0; color: #64748b; font-size: 12px;'>
                                            This is an automated notification from
                                        </p>
                                        <p style='margin: 0; color: #0066cc; font-size: 14px; font-weight: 600;'>
                                            🔧 CATIA - Aras PLM Integration
                                        </p>
                                        <p style='margin: 15px 0 0 0; color: #94a3b8; font-size: 11px;'>
                                            © {DateTime.Now.Year} Besterun Engineering
                                        </p>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>

                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
        }

        /// <summary>
        /// Format file size to human readable format.
        /// </summary>
        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            int order = 0;
            double size = bytes;
            while (size >= 1024 && order < sizes.Length - 1)
            {
                order++;
                size /= 1024;
            }
            return $"{size:0.##} {sizes[order]}";
        }

        /// <summary>
        /// Send HTML email with optional attachment.
        /// </summary>
        public bool SendHtmlEmail(string to, string subject, string htmlBody, string attachmentPath = null)
        {
            try
            {
                using (var client = new SmtpClient(_config.SmtpServer, _config.SmtpPort))
                {
                    client.EnableSsl = _config.SmtpUseSsl;
                    client.TargetName = "SMTPSVC/" + _config.SmtpServer;
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;
                    client.Timeout = 60000; // 60 seconds for attachments

                    if (!string.IsNullOrEmpty(_config.SmtpUsername))
                    {
                        client.UseDefaultCredentials = false;
                        client.Credentials = new NetworkCredential(_config.SmtpUsername, _config.SmtpPassword);
                    }
                    else
                    {
                        client.UseDefaultCredentials = true;
                    }

                    using (var message = new MailMessage())
                    {
                        message.From = new MailAddress(_config.SenderEmail, "Aras PLM Notification");
                        message.To.Add(to);
                        message.Subject = subject;
                        message.Body = htmlBody;
                        message.IsBodyHtml = true;

                        // Add attachment if provided
                        if (!string.IsNullOrEmpty(attachmentPath) && File.Exists(attachmentPath))
                        {
                            var attachment = new Attachment(attachmentPath);
                            message.Attachments.Add(attachment);
                        }

                        client.Send(message);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Email send failed: {ex.Message}");
                LastError = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Send HTML email with multiple attachments.
        /// </summary>
        public bool SendHtmlEmailWithMultipleAttachments(string to, string subject, string htmlBody, List<string> attachmentPaths)
        {
            try
            {
                using (var client = new SmtpClient(_config.SmtpServer, _config.SmtpPort))
                {
                    client.EnableSsl = _config.SmtpUseSsl;
                    client.TargetName = "SMTPSVC/" + _config.SmtpServer;
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;
                    client.Timeout = 120000; // 2 minutes for multiple attachments

                    if (!string.IsNullOrEmpty(_config.SmtpUsername))
                    {
                        client.UseDefaultCredentials = false;
                        client.Credentials = new NetworkCredential(_config.SmtpUsername, _config.SmtpPassword);
                    }
                    else
                    {
                        client.UseDefaultCredentials = true;
                    }

                    using (var message = new MailMessage())
                    {
                        message.From = new MailAddress(_config.SenderEmail, "Aras PLM Notification");
                        message.To.Add(to);
                        message.Subject = subject;
                        message.Body = htmlBody;
                        message.IsBodyHtml = true;

                        // Add all attachments
                        var attachments = new List<Attachment>();
                        if (attachmentPaths != null)
                        {
                            foreach (var path in attachmentPaths)
                            {
                                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                                {
                                    var attachment = new Attachment(path);
                                    message.Attachments.Add(attachment);
                                    attachments.Add(attachment);
                                }
                            }
                        }

                        client.Send(message);

                        // Dispose attachments
                        foreach (var attachment in attachments)
                        {
                            attachment.Dispose();
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Email send failed: {ex.Message}");
                LastError = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Send a generic email.
        /// </summary>
        public bool SendEmail(string to, string subject, string body)
        {
            try
            {
                using (var client = new SmtpClient(_config.SmtpServer, _config.SmtpPort))
                {
                    client.EnableSsl = _config.SmtpUseSsl;
                    client.TargetName = "SMTPSVC/" + _config.SmtpServer;
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;
                    client.Timeout = 30000;

                    if (!string.IsNullOrEmpty(_config.SmtpUsername))
                    {
                        client.UseDefaultCredentials = false;
                        client.Credentials = new NetworkCredential(_config.SmtpUsername, _config.SmtpPassword);
                    }
                    else
                    {
                        client.UseDefaultCredentials = true;
                    }

                    using (var message = new MailMessage())
                    {
                        message.From = new MailAddress(_config.SenderEmail);
                        message.To.Add(to);
                        message.Subject = subject;
                        message.Body = body;
                        message.IsBodyHtml = false;

                        client.Send(message);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Email send failed: {ex.Message}");
                LastError = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Last error message from email operation.
        /// </summary>
        public string LastError { get; private set; } = "";

        /// <summary>
        /// Test SMTP connection and settings.
        /// </summary>
        public string TestConnection()
        {
            if (string.IsNullOrEmpty(_config.SmtpServer))
                return "SMTP server not configured";

            if (string.IsNullOrEmpty(_config.SenderEmail))
                return "Sender email not configured";

            if (string.IsNullOrEmpty(_config.ReviewerEmail))
                return "Reviewer email not configured";

            try
            {
                using (var client = new SmtpClient(_config.SmtpServer, _config.SmtpPort))
                {
                    client.EnableSsl = _config.SmtpUseSsl;
                    client.TargetName = "SMTPSVC/" + _config.SmtpServer;

                    if (!string.IsNullOrEmpty(_config.SmtpUsername))
                    {
                        client.UseDefaultCredentials = false;
                        client.Credentials = new NetworkCredential(_config.SmtpUsername, _config.SmtpPassword);
                    }
                    else
                    {
                        client.UseDefaultCredentials = true;
                    }

                    client.DeliveryMethod = SmtpDeliveryMethod.Network;
                    client.Timeout = 10000;

                    using (var message = new MailMessage())
                    {
                        message.From = new MailAddress(_config.SenderEmail, "Aras PLM Test");
                        message.To.Add(_config.SenderEmail);
                        message.Subject = "CATIA-Aras Integration - SMTP Test";
                        message.Body = "This is a test email. Your SMTP settings are configured correctly.";
                        message.IsBodyHtml = false;

                        client.Send(message);
                    }
                }

                return "";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}
