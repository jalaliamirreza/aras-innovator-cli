// Aras Server Method: PLM_ApproveDocument
// Method Type: C#
// CWS Global Method for approving/rejecting documents from email links

Innovator inn = this.getInnovator();

// Get parameters from CWS
string itemId = this.getProperty("id", "");
string itemType = this.getProperty("type", "Document");
string action = this.getProperty("action", "");

// Validate
if (itemId == "")
{
    return inn.newError("Missing item ID");
}

if (action != "approve" && action != "reject")
{
    return inn.newError("Invalid action: " + action);
}

// Get the document
Item item = inn.newItem(itemType, "get");
item.setID(itemId);
item = item.apply();

if (item.isError())
{
    return inn.newError("Document not found: " + item.getErrorString());
}

string itemNumber = item.getProperty("item_number", "Unknown");
string currentState = item.getProperty("state", "");

// Check state
if (currentState != "In Review")
{
    return inn.newError("Document " + itemNumber + " is not in 'In Review' state. Current: " + currentState);
}

// Promote
string targetState = (action == "approve") ? "Released" : "Preliminary";

Item promoteItem = inn.newItem(itemType, "promoteItem");
promoteItem.setID(itemId);
promoteItem.setProperty("state", targetState);
Item result = promoteItem.apply();

if (result.isError())
{
    return inn.newError("Failed to " + action + ": " + result.getErrorString());
}

// Success
string msg = (action == "approve")
    ? "Document " + itemNumber + " approved and released!"
    : "Document " + itemNumber + " rejected.";

return inn.newResult(msg);
