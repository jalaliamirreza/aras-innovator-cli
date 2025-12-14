// DEBUG VERSION - Test parameter access
Innovator inn = this.getInnovator();

string itemId = this.getProperty("id", "NOT_FOUND");
string itemType = this.getProperty("type", "NOT_FOUND");
string action = this.getProperty("action", "NOT_FOUND");

return inn.newResult("id=" + itemId + ", type=" + itemType + ", action=" + action);
