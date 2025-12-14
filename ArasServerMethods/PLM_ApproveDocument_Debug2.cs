// DEBUG VERSION 2 - Test getItemById
Innovator inn = this.getInnovator();

string itemId = this.getProperty("id", "");
string itemType = this.getProperty("type", "Document");
string action = this.getProperty("action", "");

if (string.IsNullOrEmpty(itemId))
{
    return inn.newResult("ERROR: Missing item ID");
}

if (string.IsNullOrEmpty(action) || (action != "approve" && action != "reject"))
{
    return inn.newResult("ERROR: Invalid action: " + action);
}

Item item = inn.getItemById(itemType, itemId);
if (item.isError())
{
    return inn.newResult("ERROR: Document not found: " + item.getErrorString());
}

string itemNumber = item.getProperty("item_number", "Unknown");
string currentState = item.getProperty("state", "Unknown");

return inn.newResult("OK: Found " + itemNumber + ", state=" + currentState);
