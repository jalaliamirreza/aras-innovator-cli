// Aras Server Method: PLM_ApproveDocument
// Type: JavaScript
// Method Type: Server Event
//
// This method handles one-click approve/reject from email notifications.
// Called via: /InnovatorServer/Server/InnovatorServer.aspx?method=PLM_ApproveDocument&id=xxx&type=Document&action=approve
//
// To install:
// 1. Go to Aras Innovator > TOC > Administration > Methods
// 2. Create new Method with name "PLM_ApproveDocument"
// 3. Set Method Type to "JavaScript"
// 4. Paste this code
// 5. Save

// Get parameters from URL
var itemId = inn.getQueryString("id");
var itemType = inn.getQueryString("type") || "Document";
var action = inn.getQueryString("action"); // "approve" or "reject"

// Validate parameters
if (!itemId) {
    return generateHtmlResponse("Error", "Missing item ID", "error");
}

if (!action || (action !== "approve" && action !== "reject")) {
    return generateHtmlResponse("Error", "Invalid action. Must be 'approve' or 'reject'", "error");
}

try {
    // Get the item
    var item = inn.getItemById(itemType, itemId);
    if (item.isError()) {
        return generateHtmlResponse("Error", "Document not found: " + item.getErrorString(), "error");
    }

    var itemNumber = item.getProperty("item_number", "Unknown");
    var itemName = item.getProperty("name", "");
    var currentState = item.getProperty("state", "");

    // Check if item is in "In Review" state
    if (currentState !== "In Review") {
        return generateHtmlResponse("Cannot Process",
            "Document " + itemNumber + " is not in 'In Review' state.<br><br>Current state: <b>" + currentState + "</b>",
            "warning");
    }

    if (action === "approve") {
        // Promote to Released
        var promoteItem = inn.newItem(itemType, "promoteItem");
        promoteItem.setID(itemId);
        promoteItem.setProperty("state", "Released");
        var result = promoteItem.apply();

        if (result.isError()) {
            return generateHtmlResponse("Approval Failed",
                "Could not approve document " + itemNumber + ".<br><br>Error: " + result.getErrorString(),
                "error");
        }

        return generateHtmlResponse("Document Approved!",
            "Document <b>" + itemNumber + "</b> has been successfully released.<br><br>" +
            "Name: " + itemName + "<br>" +
            "New State: <span style='color: #10b981; font-weight: bold;'>Released</span>",
            "success");
    }
    else if (action === "reject") {
        // Demote back to Preliminary (or previous state)
        var demoteItem = inn.newItem(itemType, "promoteItem");
        demoteItem.setID(itemId);
        demoteItem.setProperty("state", "Preliminary");
        var result = demoteItem.apply();

        if (result.isError()) {
            return generateHtmlResponse("Rejection Failed",
                "Could not reject document " + itemNumber + ".<br><br>Error: " + result.getErrorString(),
                "error");
        }

        return generateHtmlResponse("Document Rejected",
            "Document <b>" + itemNumber + "</b> has been rejected and returned to Preliminary state.<br><br>" +
            "Name: " + itemName + "<br>" +
            "New State: <span style='color: #f59e0b; font-weight: bold;'>Preliminary</span>",
            "rejected");
    }
}
catch (ex) {
    return generateHtmlResponse("Error", "An error occurred: " + ex.message, "error");
}

// Helper function to generate HTML response page
function generateHtmlResponse(title, message, type) {
    var bgColor, iconEmoji, headerBg;

    switch(type) {
        case "success":
            bgColor = "#d1fae5";
            iconEmoji = "✅";
            headerBg = "linear-gradient(135deg, #10b981 0%, #059669 100%)";
            break;
        case "rejected":
            bgColor = "#fef3c7";
            iconEmoji = "↩️";
            headerBg = "linear-gradient(135deg, #f59e0b 0%, #d97706 100%)";
            break;
        case "warning":
            bgColor = "#fef3c7";
            iconEmoji = "⚠️";
            headerBg = "linear-gradient(135deg, #f59e0b 0%, #d97706 100%)";
            break;
        case "error":
        default:
            bgColor = "#fee2e2";
            iconEmoji = "❌";
            headerBg = "linear-gradient(135deg, #ef4444 0%, #dc2626 100%)";
            break;
    }

    var html = '<!DOCTYPE html>' +
        '<html><head><meta charset="UTF-8"><meta name="viewport" content="width=device-width, initial-scale=1.0">' +
        '<title>' + title + ' - Aras PLM</title></head>' +
        '<body style="margin: 0; padding: 0; font-family: -apple-system, BlinkMacSystemFont, Segoe UI, Roboto, Arial, sans-serif; background-color: #f5f7fa;">' +
        '<table width="100%" cellpadding="0" cellspacing="0" style="background-color: #f5f7fa; padding: 40px 20px; min-height: 100vh;">' +
        '<tr><td align="center" valign="middle">' +
        '<table width="500" cellpadding="0" cellspacing="0" style="background-color: #ffffff; border-radius: 12px; box-shadow: 0 4px 20px rgba(0,0,0,0.1); overflow: hidden;">' +
        '<tr><td style="background: ' + headerBg + '; padding: 30px 40px; text-align: center;">' +
        '<div style="font-size: 48px; margin-bottom: 10px;">' + iconEmoji + '</div>' +
        '<h1 style="margin: 0; color: #ffffff; font-size: 24px; font-weight: 600;">' + title + '</h1>' +
        '</td></tr>' +
        '<tr><td style="padding: 40px; text-align: center;">' +
        '<div style="background: ' + bgColor + '; border-radius: 10px; padding: 25px; margin-bottom: 25px;">' +
        '<p style="margin: 0; color: #333; font-size: 16px; line-height: 1.6;">' + message + '</p>' +
        '</div>' +
        '<p style="margin: 0; color: #64748b; font-size: 13px;">You can close this window.</p>' +
        '</td></tr>' +
        '<tr><td style="background: #f8fafc; padding: 20px 40px; text-align: center; border-top: 1px solid #e2e8f0;">' +
        '<p style="margin: 0; color: #0066cc; font-size: 14px; font-weight: 600;">🔧 CATIA - Aras PLM Integration</p>' +
        '</td></tr>' +
        '</table></td></tr></table></body></html>';

    return inn.newResult(html);
}
