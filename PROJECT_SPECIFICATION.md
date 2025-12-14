# Aras CATIA Add-in Project Specification

## Document Purpose

Project specification for AI coding assistants (GitHub Copilot, Claude Code, Cursor, etc.) to implement a CATIA V5 COM Add-in integrated with Aras Innovator PLM.

**Role**: You are the developer. This document tells you WHAT to build.

---

## Executive Summary

Build a toolbar add-in for CATIA V5 that connects to Aras Innovator PLM system. Users should be able to check-in, check-out, search, and synchronize CAD data without leaving CATIA.

---

## Technology Decisions

| Decision | Choice | Rationale |
|----------|--------|-----------|
| Language | C# | Best COM interop support, team expertise |
| Framework | .NET Framework 4.7.2 | CATIA compatibility, stable |
| Add-in Type | COM Add-in using IDTExtensibility2 | Native CATIA integration |
| CATIA API | COM Automation | Included with CATIA, no CAA license needed |
| Aras API | IOM (Innovator Object Model) | Official Aras .NET API |
| UI Framework | Windows Forms | Simple, works well with COM |
| Config Storage | JSON file in AppData | Human readable, easy to edit |
| Logging | Text file in AppData | Simple debugging |

---

## Target Environment

| Component | Version | Notes |
|-----------|---------|-------|
| CATIA | V5-6R2022 SP6 (P3) | Also known as V5 R32 |
| Aras Innovator | Community Edition 14+ | Free version |
| Windows | 10 or 11 (64-bit) | Client workstations |
| Visual Studio | 2022 | Development |

---

## Architecture Overview

### System Components

1. **CATIA Application** - Host application, provides COM interfaces
2. **Add-in DLL** - Our code, loaded by CATIA at startup
3. **Aras Server** - PLM backend, accessed via HTTP/IOM
4. **Local Workspace** - Folder for downloaded files
5. **Config File** - User settings stored locally

### Data Flow

1. User clicks toolbar button in CATIA
2. Add-in receives command
3. Add-in reads data from CATIA via COM
4. Add-in communicates with Aras via IOM
5. Add-in shows result to user via Windows Forms dialog

### Integration Points

| System | Interface | Direction |
|--------|-----------|-----------|
| CATIA | COM Type Libraries | Read/Write |
| Aras | IOM.dll over HTTP | Read/Write |
| File System | Local workspace folder | Read/Write |
| Windows Registry | COM registration | Write (install only) |

---

## Project Structure

### Solution Organization

```
ArasCatiaAddin/
├── ArasCatiaAddin.sln           (Solution file)
├── ArasCatiaAddin/              (Main project)
│   ├── Entry point class        (COM add-in lifecycle)
│   ├── Toolbar manager          (Create/remove toolbar)
│   ├── CATIA service            (All CATIA operations)
│   ├── Aras service             (All Aras operations)
│   ├── Config manager           (Settings load/save)
│   ├── Logger                   (Logging utility)
│   ├── Commands/                (One class per command)
│   ├── Forms/                   (Windows Forms dialogs)
│   ├── Models/                  (Data classes)
│   └── Utilities/               (Helpers)
├── Lib/                         (External DLLs)
├── Setup/                       (Installation scripts)
└── Documentation/               (User guides)
```

### Class Responsibilities

| Class | Single Responsibility |
|-------|----------------------|
| Connect | Handle add-in lifecycle (load, unload) |
| AddinToolbar | Create toolbar, route button clicks to commands |
| CatiaService | All CATIA COM operations (get doc, read properties, extract BOM) |
| ArasService | All Aras operations (connect, CRUD, file transfer) |
| ConfigManager | Load and save user settings |
| Logger | Write log entries to file |
| LoginCommand | Handle login workflow |
| CheckInCommand | Handle check-in workflow |
| CheckOutCommand | Handle check-out workflow |
| GetLatestCommand | Handle get latest workflow |
| SearchCommand | Handle search workflow |
| BomSyncCommand | Handle BOM sync workflow |

---

## Feature Specifications

### F1: Toolbar

**Description**: Add-in creates a toolbar in CATIA with buttons for each feature.

**Buttons**:
| Button | Icon | Tooltip | Action |
|--------|------|---------|--------|
| Login | Key/Lock | "Login to Aras Innovator" | Open login dialog |
| Check-in | Upload arrow | "Check in to Aras" | Open check-in dialog |
| Check-out | Download arrow | "Check out from Aras" | Open search for check-out |
| Get Latest | Refresh | "Get latest from Aras" | Open search for get latest |
| Search | Magnifier | "Search Aras" | Open search dialog |
| BOM Sync | Hierarchy | "Sync BOM to Aras" | Open BOM sync dialog |
| Settings | Gear | "Settings" | Open settings dialog |

**Behavior**:
- Toolbar appears when CATIA starts
- Buttons are disabled until logged in (except Login and Settings)
- Toolbar is removed when CATIA closes

---

### F2: Login

**Description**: Connect to Aras Innovator server with credentials.

**Inputs**:
| Field | Type | Required | Default | Validation |
|-------|------|----------|---------|------------|
| Server URL | Text | Yes | http://localhost/InnovatorServer | Valid URL format |
| Database | Text | Yes | InnovatorSolutions | Not empty |
| Username | Text | Yes | (empty) | Not empty |
| Password | Password | Yes | (empty) | Not empty |
| Remember Password | Checkbox | No | Unchecked | None |

**Success Criteria**:
- Connection established
- User authenticated
- Innovator instance obtained
- Other toolbar buttons enabled

**Error Handling**:
- Invalid URL: Show "Invalid server URL"
- Connection failed: Show "Cannot connect to server"
- Authentication failed: Show "Invalid username or password"

---

### F3: Check-in

**Description**: Upload the currently open CATIA document to Aras.

**Preconditions**:
- User is logged in to Aras
- A document is open in CATIA

**Workflow**:
1. Get active document from CATIA
2. If unsaved changes, prompt user to save first
3. Extract properties from CATIA document
4. Show dialog with pre-filled values
5. User confirms or edits values
6. Create Document item in Aras
7. Upload file to Aras vault
8. Show success message with document number

**Inputs** (Check-in Dialog):
| Field | Source | Editable | Required |
|-------|--------|----------|----------|
| Item Number | CATIA PartNumber or auto-generate | Yes | Yes |
| Name | CATIA Nomenclature | Yes | Yes |
| Description | CATIA Definition | Yes | No |
| Document Type | Default from settings | Yes | Yes |
| File Path | CATIA document path | No | Yes |

**Property Mapping**:
| CATIA Property | Aras Property |
|----------------|---------------|
| PartNumber | item_number |
| Nomenclature | name |
| Definition | description |
| Revision | major_rev |

**Success Criteria**:
- Document created in Aras
- File uploaded to vault
- User shown confirmation with item number

---

### F4: Check-out

**Description**: Download a document from Aras with lock for editing.

**Preconditions**:
- User is logged in to Aras

**Workflow**:
1. Open search dialog
2. User searches for document
3. User selects document from results
4. Check if document is already locked
5. If locked by others, show warning and stop
6. Lock document in Aras
7. Download file to local workspace
8. Open file in CATIA
9. Show success message

**Lock Behavior**:
- Document locked by current user in Aras
- Other users see document as "Checked out by [username]"
- Lock released on check-in or explicit unlock

---

### F5: Get Latest

**Description**: Download latest version of document without locking.

**Preconditions**:
- User is logged in to Aras

**Workflow**:
1. Open search dialog
2. User searches for document
3. User selects document from results
4. Download file to local workspace
5. Open file in CATIA
6. Show success message

**Difference from Check-out**:
- No lock applied
- File is for viewing/reference only
- Changes should not be checked back in

---

### F6: Search

**Description**: Find documents in Aras.

**Preconditions**:
- User is logged in to Aras

**Search Criteria**:
| Field | Type | Operator |
|-------|------|----------|
| Item Number | Text | Contains (wildcard) |
| Name | Text | Contains (wildcard) |
| Document Type | Dropdown | Equals |
| State | Dropdown | Equals |

**Results Display**:
| Column | Description |
|--------|-------------|
| Item Number | Document identifier |
| Name | Document name |
| Type | Document type |
| State | Lifecycle state |
| Locked By | Who has it checked out |
| Modified | Last modified date |

**Actions on Selected**:
- View Details: Show all properties
- Check-out: Lock and download
- Get Latest: Download without lock

---

### F7: BOM Sync

**Description**: Synchronize assembly structure from CATProduct to Aras.

**Preconditions**:
- User is logged in to Aras
- A CATProduct (assembly) is open in CATIA

**Workflow**:
1. Extract BOM from CATProduct
2. Display BOM tree in dialog
3. For each item, check if exists in Aras
4. Show status (New, Exists, Modified)
5. User reviews and selects items to sync
6. User confirms sync
7. Create missing Parts in Aras
8. Create/update Part BOM relationships
9. Upload CAD files as Documents
10. Link Documents to Parts
11. Show summary

**BOM Item Properties**:
| Property | Source |
|----------|--------|
| Part Number | CATIA Product.PartNumber |
| Name | CATIA Product.Name |
| Nomenclature | CATIA Product.Nomenclature |
| Revision | CATIA Product.Revision |
| Quantity | Count of instances |
| Level | Assembly hierarchy depth |
| File Path | Source CATPart/CATProduct path |

**Sync Status**:
| Status | Meaning | Icon Color |
|--------|---------|------------|
| New | Not in Aras, will be created | Green |
| Exists | Already in Aras, matches | Gray |
| Modified | In Aras but different | Yellow |
| Error | Problem detected | Red |

---

### F8: Settings

**Description**: Configure add-in behavior.

**Settings Categories**:

**Connection Settings**:
| Setting | Type | Default |
|---------|------|---------|
| Server URL | Text | http://localhost/InnovatorServer |
| Database | Text | InnovatorSolutions |
| Remember Password | Boolean | False |
| Auto-login on startup | Boolean | False |

**Workspace Settings**:
| Setting | Type | Default |
|---------|------|---------|
| Local Workspace Path | Folder | Documents\ArasWorkspace |
| Overwrite existing files | Boolean | False |

**Check-in Settings**:
| Setting | Type | Default |
|---------|------|---------|
| Auto-generate item number | Boolean | True |
| Item number prefix | Text | DOC |
| Default document type | Dropdown | 3D Model |
| Confirm before check-in | Boolean | True |

**BOM Sync Settings**:
| Setting | Type | Default |
|---------|------|---------|
| Create missing parts | Boolean | True |
| Sync properties | Boolean | True |

---

## Data Models

### CatiaDocumentInfo

Represents information extracted from a CATIA document.

| Property | Type | Description |
|----------|------|-------------|
| FullPath | String | Complete file path |
| FileName | String | File name with extension |
| DocumentType | Enum | CATPart, CATProduct, CATDrawing |
| PartNumber | String | From CATIA properties |
| Nomenclature | String | From CATIA properties |
| Revision | String | From CATIA properties |
| Definition | String | From CATIA properties |
| IsSaved | Boolean | Has unsaved changes |

### ArasDocument

Represents a Document item from Aras.

| Property | Type | Description |
|----------|------|-------------|
| Id | String | Aras item ID (GUID) |
| ItemNumber | String | Document number |
| Name | String | Document name |
| Description | String | Document description |
| State | String | Lifecycle state |
| LockedById | String | User ID if locked |
| LockedByName | String | User name if locked |
| ModifiedOn | DateTime | Last modified |
| DocumentType | String | Type classification |

### BomItem

Represents an item in a bill of materials.

| Property | Type | Description |
|----------|------|-------------|
| PartNumber | String | Part identifier |
| Name | String | Part name |
| Nomenclature | String | Description |
| Revision | String | Revision level |
| Quantity | Decimal | Count in assembly |
| Level | Integer | Hierarchy depth (0=root) |
| FilePath | String | Source CAD file |
| FileType | String | CATPart or CATProduct |
| Children | List | Child BOM items |
| ArasPartId | String | Linked Aras Part ID |
| SyncStatus | Enum | New, Exists, Modified, Error |

### AppConfig

Represents user settings.

| Property | Type | Description |
|----------|------|-------------|
| ArasServerUrl | String | Server address |
| ArasDatabase | String | Database name |
| ArasUsername | String | Last used username |
| ArasPassword | String | Saved password (if remember) |
| RememberPassword | Boolean | Save password |
| AutoLogin | Boolean | Login on startup |
| LocalWorkspace | String | Download folder path |
| OverwriteExisting | Boolean | Overwrite files |
| AutoGenerateItemNumber | Boolean | Generate numbers |
| ItemNumberPrefix | String | Prefix for generated numbers |
| DefaultDocumentType | String | Default type |
| ConfirmBeforeCheckin | Boolean | Show confirmation |
| CreateMissingParts | Boolean | Auto-create parts |
| SyncProperties | Boolean | Update properties |

---

## User Interface Specifications

### Login Dialog

| Element | Type | Position | Behavior |
|---------|------|----------|----------|
| Server URL | TextBox | Row 1 | Pre-filled from config |
| Database | TextBox | Row 2 | Pre-filled from config |
| Username | TextBox | Row 3 | Pre-filled from config |
| Password | TextBox (masked) | Row 4 | Pre-filled if remembered |
| Remember Password | CheckBox | Row 5 | Checked if was remembered |
| OK Button | Button | Bottom | Attempt login |
| Cancel Button | Button | Bottom | Close dialog |
| Status Label | Label | Bottom | Show "Connecting..." or errors |

Size: 400 x 250 pixels

### Check-in Dialog

| Element | Type | Position | Behavior |
|---------|------|----------|----------|
| Item Number | TextBox | Row 1 | Pre-filled from CATIA or generated |
| Name | TextBox | Row 2 | Pre-filled from CATIA Nomenclature |
| Description | TextBox (multiline) | Row 3 | Pre-filled from CATIA Definition |
| Document Type | ComboBox | Row 4 | Default from settings |
| File Path | TextBox (readonly) | Row 5 | From CATIA |
| OK Button | Button | Bottom | Perform check-in |
| Cancel Button | Button | Bottom | Close dialog |
| Progress Bar | ProgressBar | Bottom | Show upload progress |

Size: 450 x 350 pixels

### Search Dialog

| Element | Type | Position | Behavior |
|---------|------|----------|----------|
| Item Number | TextBox | Top row | Search criteria |
| Name | TextBox | Top row | Search criteria |
| Document Type | ComboBox | Top row | Filter |
| Search Button | Button | Top row | Execute search |
| Results Grid | DataGridView | Center | Display results |
| Details Panel | Panel | Right | Show selected item details |
| OK Button | Button | Bottom | Select and close |
| Cancel Button | Button | Bottom | Close dialog |

Size: 800 x 500 pixels

### BOM Sync Dialog

| Element | Type | Position | Behavior |
|---------|------|----------|----------|
| BOM Tree | TreeView | Left | Hierarchical BOM display |
| Details Panel | Panel | Right | Selected item details |
| Status Column | In tree | - | Icon showing sync status |
| Select All | CheckBox | Top | Select all items |
| Sync Button | Button | Bottom | Perform sync |
| Cancel Button | Button | Bottom | Close dialog |
| Progress Bar | ProgressBar | Bottom | Show sync progress |
| Log Panel | TextBox | Bottom | Show actions taken |

Size: 900 x 600 pixels

### Settings Dialog

| Element | Type | Position | Behavior |
|---------|------|----------|----------|
| Tab Control | TabControl | Full | Organize settings |
| Connection Tab | Tab | - | Server settings |
| Workspace Tab | Tab | - | File settings |
| Check-in Tab | Tab | - | Check-in settings |
| BOM Tab | Tab | - | BOM sync settings |
| Save Button | Button | Bottom | Save and close |
| Cancel Button | Button | Bottom | Discard and close |
| Reset Button | Button | Bottom | Reset to defaults |

Size: 500 x 400 pixels

---

## Error Handling

### Error Categories

| Category | Example | User Message | Action |
|----------|---------|--------------|--------|
| Connection | Server unreachable | "Cannot connect to Aras server. Check your network and server URL." | Show dialog, log |
| Authentication | Wrong password | "Invalid username or password." | Show dialog, stay on login |
| Permission | No access to item | "You do not have permission to access this document." | Show dialog, log |
| Lock Conflict | Already checked out | "Document is checked out by [username]." | Show dialog, offer refresh |
| File Error | File not found | "File not found: [path]" | Show dialog, log |
| CATIA Error | No document open | "No document is open in CATIA." | Show dialog |
| Validation | Missing required field | "Item Number is required." | Highlight field |

### Logging Requirements

| Log Level | When to Use |
|-----------|-------------|
| DEBUG | Detailed flow for troubleshooting |
| INFO | Normal operations (login, check-in, etc.) |
| WARNING | Recoverable issues (file overwrite, etc.) |
| ERROR | Failures that stop operation |

Log File Location: %APPDATA%\ArasCatiaAddin\Logs\

Log Format: [Timestamp] [Level] [Message]

---

## Registration and Deployment

### COM Registration Requirements

The add-in DLL must be registered with Windows for CATIA to load it.

Required Registry Entries:
1. CLSID registration for the Connect class
2. ProgId registration (ArasCatiaAddin.Connect)
3. CATIA add-in registration

### Installation Steps

1. Copy DLL and dependencies to install folder
2. Run RegAsm to register COM
3. Add CATIA registry entries
4. Create Start Menu shortcuts
5. Create local workspace folder

### Uninstallation Steps

1. Remove CATIA registry entries
2. Run RegAsm /unregister
3. Delete install folder
4. Optionally delete config and logs

---

## Testing Requirements

### Unit Tests

| Test Area | Test Cases |
|-----------|------------|
| Config Manager | Load, Save, Reset, Handle missing file |
| Property Mapping | Map all properties correctly |
| BOM Extraction | Extract flat, nested, deeply nested |
| Validation | Required fields, URL format |

### Integration Tests

| Test Case | Precondition | Steps | Expected Result |
|-----------|--------------|-------|-----------------|
| Login Success | Aras running | Enter valid credentials | Connected, buttons enabled |
| Login Fail | Aras running | Enter wrong password | Error message, stay on dialog |
| Check-in CATPart | Logged in, part open | Click check-in, confirm | Document created in Aras |
| Check-in CATProduct | Logged in, assembly open | Click check-in, confirm | Document created in Aras |
| Check-out | Logged in, doc in Aras | Search, select, check-out | File downloaded, opened in CATIA |
| Get Latest | Logged in, doc in Aras | Search, select, get latest | File downloaded, opened |
| BOM Sync | Logged in, assembly open | Click sync, confirm | Parts and BOMs created |
| Lock Conflict | Doc locked by other | Try check-out | Warning message shown |

### User Acceptance Tests

| Scenario | Description |
|----------|-------------|
| New Part Workflow | Create part in CATIA, check in, close, check out, modify, check in |
| Assembly Workflow | Create assembly, sync BOM, verify in Aras |
| Collaboration | User A checks out, User B sees locked, User A checks in, User B can access |

---

## Performance Requirements

| Operation | Target Time |
|-----------|-------------|
| Add-in load | < 3 seconds |
| Login | < 5 seconds |
| Search (100 results) | < 3 seconds |
| Check-in (10 MB file) | < 30 seconds |
| BOM extract (100 items) | < 5 seconds |

---

## Security Considerations

| Concern | Mitigation |
|---------|------------|
| Password Storage | Encrypt if saving, or use Windows Credential Manager |
| Network | HTTPS recommended for production |
| File Access | Use user's workspace, respect permissions |
| Logging | Don't log passwords or sensitive data |

---

## Future Enhancements (Out of Scope)

- Lifecycle promotion from CATIA
- Where-used queries
- Revision comparison
- Drawing auto-generation
- SAP integration
- Multi-language support

---

## References

### CATIA COM API

- Application object: Start here, provides Documents collection
- Documents collection: Open, create, iterate documents
- Document object: Save, close, get path, check type
- PartDocument: Access Part object for CATParts
- ProductDocument: Access Product object for CATProducts
- Product object: PartNumber, Name, Nomenclature, Revision, Products collection
- Parameters collection: Read/write user properties

### Aras IOM API

- IomFactory: Create HTTP connection
- HttpServerConnection: Login, logout
- Innovator: Create items, run queries
- Item: Represents Aras items, supports CRUD operations
- Item actions: add, edit, get, delete, lock, unlock
- Relationships: Part BOM, Part Document, Document File

### Required CATIA Type Libraries

- INFITF (Infrastructure)
- MECMOD (Mechanical Modeler)
- PARTITF (Part Interfaces)
- ProductStructureTypeLib (Assembly)
- KnowledgewareTypeLib (Parameters)

### Required .NET References

- System.Windows.Forms
- System.Drawing
- Extensibility (for IDTExtensibility2)

---

## Glossary

| Term | Definition |
|------|------------|
| BOM | Bill of Materials - list of parts in an assembly |
| CATPart | CATIA single part file |
| CATProduct | CATIA assembly file |
| Check-in | Upload file to PLM with new version |
| Check-out | Download file from PLM with lock |
| COM | Component Object Model - Windows interop technology |
| Get Latest | Download file without locking |
| IDTExtensibility2 | Microsoft interface for Office/app add-ins |
| IOM | Innovator Object Model - Aras .NET API |
| PLM | Product Lifecycle Management |
| Vault | Aras file storage system |

---

## Document History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | Dec 2024 | Project Team | Initial specification |

---

**END OF SPECIFICATION**
