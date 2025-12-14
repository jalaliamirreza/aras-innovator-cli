// DEBUG VERSION 6 - Compare hardcoded vs parameter
Innovator inn = this.getInnovator();

string paramId = this.getProperty("id", "NONE");
string hardcodedId = "6452D4E099024F8E8518649B9D82A666";

// Check if they match
bool match = (paramId == hardcodedId);

// Try to get with parameter
Item item = inn.newItem("Document", "get");
item.setID(paramId);
item = item.apply();

string result = "param=[" + paramId + "] hardcoded=[" + hardcodedId + "] match=" + match + " length=" + paramId.Length;
if (item.isError())
{
    result = result + " ERROR: " + item.getErrorString();
}
else
{
    result = result + " OK: Found";
}

return inn.newResult(result);
