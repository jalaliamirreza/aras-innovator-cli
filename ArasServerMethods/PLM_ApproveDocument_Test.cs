// SIMPLE TEST VERSION - Copy this to PLM_ApproveDocument method in Aras first to test if CWS works
// If this returns success, the issue is in the logic. If it fails, the issue is CWS setup.

Innovator inn = this.getInnovator();

// Just return success to test if CWS is working
Item response = inn.newItem();
response.setProperty("status", "test");
response.setProperty("message", "CWS endpoint is working!");
response.setProperty("received_body", this.ToString());
return response;
