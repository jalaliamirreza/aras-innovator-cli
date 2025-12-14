// DEBUG VERSION 4 - Use traditional Item get
Innovator inn = this.getInnovator();

string itemId = this.getProperty("id", "NONE");
string itemType = this.getProperty("type", "Document");

Item item = inn.newItem(itemType, "get");
item.setID(itemId);
item = item.apply();

if (item.isError())
{
    return inn.newResult("ERROR: " + item.getErrorString());
}

return inn.newResult("OK: Found item");
