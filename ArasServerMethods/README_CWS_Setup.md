# PLM Document Approval via Configurable Web Services (CWS)

## Overview
This setup allows one-click approve/reject of documents from email links using Aras CWS.

## Step 1: Create the Server Method

1. Go to **TOC > Administration > Methods**
2. Create new Method:
   - **Name**: `PLM_ApproveDocument`
   - **Method Type**: `C#` or `JavaScript`
3. Paste the code from `PLM_ApproveDocument_Method.cs` (or `.js`)
4. Save

## Step 2: Create the Web Service (CWS)

1. Go to **TOC > Administration > External Control > Web Services**
2. Create new Web Service:
   - **Name**: `PLM_Approval`
   - **Title**: `PLM Document Approval`
3. Save

## Step 3: Create the Endpoint

1. Open the Web Service you just created
2. Go to **Relationships > Endpoints** tab
3. Add new Endpoint:
   - **Name**: `approve`
   - **Type**: `Method`
   - **Method**: Select `PLM_ApproveDocument`
   - **Alias**: `approve`
4. Configure Parameters:
   - Add `id` (string) - Document ID
   - Add `type` (string) - ItemType (default: Document)
   - Add `action` (string) - approve or reject
5. Set **Execution allowed to**: `World` (for anonymous access) or configure API Key
6. Save

## Step 4: Configure API Key (Optional but Recommended)

1. Go to **TOC > Administration > External Control > API Keys**
2. Create new API Key:
   - **Name**: `PLM_Approval_Key`
   - **API Key**: (auto-generated or custom)
3. Assign to Web Service Endpoint

## Final URL Format

```
POST https://your-aras-server/InnovatorServer/Server/ws/PLM_Approval/v1/PLM_ApproveDocument
Content-Type: application/json
Authorization: Bearer <API_KEY>

{
    "id": "ABC123...",
    "type": "Document",
    "action": "approve"
}
```

Or with query parameters (if configured):
```
https://your-aras-server/InnovatorServer/Server/ws/PLM_Approval/v1/PLM_ApproveDocument?id=xxx&type=Document&action=approve
```

## Alternative: Simple HTML Page

If CWS is not available in your Aras version, use `PLM_ApproveDocument.html`:
1. Host the HTML file on any web server
2. Pass all parameters including server credentials in URL
3. The page calls Aras SOAP API directly

## Email Button URLs

The email template generates URLs like:
```
https://your-aras-server/InnovatorServer/Server/ws/PLM_Approval/v1/PLM_ApproveDocument?id=xxx&type=Document&action=approve
https://your-aras-server/InnovatorServer/Server/ws/PLM_Approval/v1/PLM_ApproveDocument?id=xxx&type=Document&action=reject
```
