// DEBUG VERSION 5 - Hardcoded ID
Innovator inn = this.getInnovator();

// Hardcode the ID to test
string itemId = "6452D4E099024F8E8518649B9D82A666";
string itemType = "Document";

Item item = inn.newItem(itemType, "get");
item.setID(itemId);
item = item.apply();

if (item.isError())
{
    return inn.newResult("ERROR: " + item.getErrorString());
}

string itemNumber = item.getProperty("item_number", "UNKNOWN");
string state = item.getProperty("state", "UNKNOWN");

return inn.newResult("OK: Found " + itemNumber + ", state=" + state);
