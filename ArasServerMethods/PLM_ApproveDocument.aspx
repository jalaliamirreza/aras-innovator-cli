<%@ Page Language="C#" %>
<%@ Import Namespace="System" %>
<%@ Import Namespace="Aras.IOM" %>
<!DOCTYPE html>
<html>
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Document Approval - Aras PLM</title>
</head>
<body style="margin: 0; padding: 0; font-family: -apple-system, BlinkMacSystemFont, Segoe UI, Roboto, Arial, sans-serif; background-color: #f5f7fa;">
<%
    string itemId = Request.QueryString["id"];
    string itemType = Request.QueryString["type"] ?? "Document";
    string action = Request.QueryString["action"];

    string title = "";
    string message = "";
    string bgColor = "";
    string iconEmoji = "";
    string headerBg = "";

    if (string.IsNullOrEmpty(itemId))
    {
        title = "Error";
        message = "Missing item ID";
        bgColor = "#fee2e2";
        iconEmoji = "❌";
        headerBg = "linear-gradient(135deg, #ef4444 0%, #dc2626 100%)";
    }
    else if (string.IsNullOrEmpty(action) || (action != "approve" && action != "reject"))
    {
        title = "Error";
        message = "Invalid action. Must be 'approve' or 'reject'";
        bgColor = "#fee2e2";
        iconEmoji = "❌";
        headerBg = "linear-gradient(135deg, #ef4444 0%, #dc2626 100%)";
    }
    else
    {
        try
        {
            // Connect to Aras using service account
            string innovatorServerUrl = System.Configuration.ConfigurationManager.AppSettings["InnovatorServerUrl"] ?? "http://localhost/InnovatorServer";
            string database = System.Configuration.ConfigurationManager.AppSettings["InnovatorDatabase"] ?? "InnovatorSolutions";
            string username = System.Configuration.ConfigurationManager.AppSettings["InnovatorServiceUser"] ?? "admin";
            string password = System.Configuration.ConfigurationManager.AppSettings["InnovatorServicePassword"] ?? "innovator";

            HttpServerConnection conn = IomFactory.CreateHttpServerConnection(
                innovatorServerUrl,
                database,
                username,
                Innovator.ScalcMD5(password)
            );

            Item loginResult = conn.Login();
            if (loginResult.isError())
            {
                throw new Exception("Login failed: " + loginResult.getErrorString());
            }

            Innovator inn = new Innovator(conn);

            // Get the item
            Item item = inn.getItemById(itemType, itemId);
            if (item.isError())
            {
                throw new Exception("Document not found: " + item.getErrorString());
            }

            string itemNumber = item.getProperty("item_number", "Unknown");
            string itemName = item.getProperty("name", "");
            string currentState = item.getProperty("state", "");

            // Check if item is in "In Review" state
            if (currentState != "In Review")
            {
                title = "Cannot Process";
                message = "Document " + itemNumber + " is not in 'In Review' state.<br><br>Current state: <b>" + currentState + "</b>";
                bgColor = "#fef3c7";
                iconEmoji = "⚠️";
                headerBg = "linear-gradient(135deg, #f59e0b 0%, #d97706 100%)";
            }
            else if (action == "approve")
            {
                // Promote to Released
                Item promoteItem = inn.newItem(itemType, "promoteItem");
                promoteItem.setID(itemId);
                promoteItem.setProperty("state", "Released");
                Item result = promoteItem.apply();

                if (result.isError())
                {
                    throw new Exception("Could not approve: " + result.getErrorString());
                }

                title = "Document Approved!";
                message = "Document <b>" + itemNumber + "</b> has been successfully released.<br><br>" +
                    "Name: " + itemName + "<br>" +
                    "New State: <span style='color: #10b981; font-weight: bold;'>Released</span>";
                bgColor = "#d1fae5";
                iconEmoji = "✅";
                headerBg = "linear-gradient(135deg, #10b981 0%, #059669 100%)";
            }
            else if (action == "reject")
            {
                // Demote back to Preliminary
                Item demoteItem = inn.newItem(itemType, "promoteItem");
                demoteItem.setID(itemId);
                demoteItem.setProperty("state", "Preliminary");
                Item result = demoteItem.apply();

                if (result.isError())
                {
                    throw new Exception("Could not reject: " + result.getErrorString());
                }

                title = "Document Rejected";
                message = "Document <b>" + itemNumber + "</b> has been rejected and returned to Preliminary state.<br><br>" +
                    "Name: " + itemName + "<br>" +
                    "New State: <span style='color: #f59e0b; font-weight: bold;'>Preliminary</span>";
                bgColor = "#fef3c7";
                iconEmoji = "↩️";
                headerBg = "linear-gradient(135deg, #f59e0b 0%, #d97706 100%)";
            }

            conn.Logout();
        }
        catch (Exception ex)
        {
            title = "Error";
            message = "An error occurred: " + ex.Message;
            bgColor = "#fee2e2";
            iconEmoji = "❌";
            headerBg = "linear-gradient(135deg, #ef4444 0%, #dc2626 100%)";
        }
    }
%>
    <table width="100%" cellpadding="0" cellspacing="0" style="background-color: #f5f7fa; padding: 40px 20px; min-height: 100vh;">
        <tr>
            <td align="center" valign="middle">
                <table width="500" cellpadding="0" cellspacing="0" style="background-color: #ffffff; border-radius: 12px; box-shadow: 0 4px 20px rgba(0,0,0,0.1); overflow: hidden;">
                    <tr>
                        <td style="background: <%=headerBg%>; padding: 30px 40px; text-align: center;">
                            <div style="font-size: 48px; margin-bottom: 10px;"><%=iconEmoji%></div>
                            <h1 style="margin: 0; color: #ffffff; font-size: 24px; font-weight: 600;"><%=title%></h1>
                        </td>
                    </tr>
                    <tr>
                        <td style="padding: 40px; text-align: center;">
                            <div style="background: <%=bgColor%>; border-radius: 10px; padding: 25px; margin-bottom: 25px;">
                                <p style="margin: 0; color: #333; font-size: 16px; line-height: 1.6;"><%=message%></p>
                            </div>
                            <p style="margin: 0; color: #64748b; font-size: 13px;">You can close this window.</p>
                        </td>
                    </tr>
                    <tr>
                        <td style="background: #f8fafc; padding: 20px 40px; text-align: center; border-top: 1px solid #e2e8f0;">
                            <p style="margin: 0; color: #0066cc; font-size: 14px; font-weight: 600;">🔧 CATIA - Aras PLM Integration</p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>
