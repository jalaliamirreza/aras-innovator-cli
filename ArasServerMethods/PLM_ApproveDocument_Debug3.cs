// DEBUG VERSION 3 - No validation, direct getItemById
Innovator inn = this.getInnovator();

string itemId = this.getProperty("id", "NONE");
string itemType = this.getProperty("type", "Document");

Item item = inn.getItemById(itemType, itemId);
if (item.isError())
{
    return inn.newResult("ERROR: " + item.getErrorString());
}

return inn.newResult("OK: Found item");
