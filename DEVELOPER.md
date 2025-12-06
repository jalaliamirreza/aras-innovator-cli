# Aras Innovator CLI - Developer Documentation

This document provides comprehensive technical documentation for AI code developers and software engineers working on the Aras Innovator CLI project.

## Project Overview

**Project Name**: Aras Innovator CLI  
**Language**: C# (.NET Framework 4.7.2)  
**UI Framework**: Windows Forms  
**Aras SDK**: Aras.IOM 15.0.1  
**Build System**: MSBuild  
**Repository**: https://github.com/jalaliamirreza/aras-innovator-cli

## Architecture

### Application Structure

The application operates in two modes:
1. **CLI Mode**: Command-line interface for AML execution and batch operations
2. **GUI Mode**: Windows Forms application for interactive document uploads

### Entry Point

**File**: `ArasCLI/Program.cs`

The `Main` method determines the execution mode:

```csharp
[STAThread]  // Required for drag-and-drop functionality
static void Main(string[] args)
{
    // Mode detection logic
    // - No args or --gui flag → GUI mode
    // - Send To file path → GUI mode with pre-filled file
    // - Otherwise → CLI mode
}
```

### Key Components

#### 1. Program.cs

**Purpose**: Main entry point and CLI logic

**Key Methods**:
- `CreateDocumentAndCheckinFile()`: Core method for document creation and file upload
  - Creates Document item
  - Uploads file using `attachPhysicalFile()`
  - Creates Document File relationship
  - Verifies upload success

**Dependencies**:
- `Aras.IOM` namespace
- `Aras.IOME` namespace (for CheckinManager, though not currently used)

#### 2. MainForm.cs

**Purpose**: Windows Forms GUI implementation

**Key Features**:
- Connection settings input
- File selection (browse, type, drag-and-drop)
- Document properties input
- Real-time logging
- Settings persistence
- Send To integration

**Key Methods**:
- `InitializeComponent()`: Creates and arranges UI controls
- `BtnBrowseFile_Click()`: Handles file browser dialog
- `BtnUpload_Click()`: Initiates upload process
- `PerformUpload()`: Background thread upload execution
- `BtnSendToSetup_Click()`: Creates Send To shortcut
- `SaveSettings()` / `LoadSettings()`: Settings persistence

## File Upload Implementation

### Process Flow

1. **Document Creation**:
   ```csharp
   Item document = inn.newItem("Document", "add");
   document.setProperty("item_number", itemNum); // or auto-generate
   document.setProperty("name", docName);
   document = document.apply();
   ```

2. **File Item Creation**:
   ```csharp
   Item fileItem = inn.newItem("File", "add");
   fileItem.setProperty("filename", fileName);
   fileItem.attachPhysicalFile(absoluteFilePath);
   Item fileResult = fileItem.apply();
   ```

3. **Relationship Creation**:
   ```csharp
   Item docFileRel = inn.newItem("Document File", "add");
   docFileRel.setProperty("source_id", document.getID());
   docFileRel.setProperty("related_id", fileId);
   Item relResult = docFileRel.apply();
   ```

### Key Implementation Details

**File Upload Method**: Uses `Item.attachPhysicalFile()` method from Aras IOM SDK. This is the recommended approach based on Aras community patterns.

**Reference**: https://community.aras.com/discussions/development/upload-a-file-using-c-as-a-server-method/6209

**Why not `setFileProperty()`?**: Initial attempts with `setFileProperty()` resulted in "File not found in container" errors. The `attachPhysicalFile()` method proved more reliable.

**Why not `CheckinManager`?**: The `CheckinManager` class requires specific setup and was found to be more complex for this use case. The direct `attachPhysicalFile()` approach is simpler and more reliable.

## Build Configuration

### Project File: ArasCLI.csproj

**Target Framework**: .NET Framework 4.7.2  
**Output Type**: WinExe (Windows executable, no console window)  
**References**: 
- System.Windows.Forms
- System.Drawing
- Aras.IOM (via NuGet)

### Build Commands

**Debug Build**:
```bash
msbuild ArasCLI.sln /t:Build /p:Configuration=Debug
```

**Release Build**:
```bash
msbuild ArasCLI.sln /t:Build /p:Configuration=Release
```

**Restore Packages**:
```bash
msbuild ArasCLI.sln /t:Restore
```

### NuGet Packages

- **Aras.IOM** (15.0.1): Aras Innovator Object Model SDK
- Dependencies are automatically resolved via NuGet

## Code Patterns

### Threading

**GUI Upload**: Upload operations run in background thread to prevent UI freezing:
```csharp
System.Threading.Tasks.Task.Run(() => PerformUpload());
```

**UI Updates**: Use `Invoke()` for thread-safe UI updates:
```csharp
this.Invoke(new Action(() => { /* UI update */ }));
```

### Error Handling

**Pattern**: Try-catch blocks with user-friendly error messages and logging:
```csharp
try {
    // Operation
} catch (Exception ex) {
    LogMessage("ERROR: " + ex.Message);
    // User notification
}
```

### Settings Persistence

**Location**: `%APPDATA%\ArasCLI\settings.config`

**Format**: Simple key-value pairs:
```
Url=http://server/InnovatorServer
Database=InnovatorSolutions
User=admin
```

**Note**: Password is never saved for security reasons.

## Send To Integration

### Shortcut Creation

Uses Windows Script Host (VBScript) to create shortcut:
```csharp
string vbsScript = @"
Set oWS = WScript.CreateObject(""WScript.Shell"")
Set oLink = oWS.CreateShortcut(sLinkFile)
oLink.TargetPath = ""{exePath}""
oLink.Arguments = ""--gui""
oLink.Save
";
```

**Location**: `%APPDATA%\Microsoft\Windows\SendTo\Aras Innovator Upload.lnk`

### File Path Detection

When Windows Send To executes, it passes the file path as a command-line argument. The application detects this:
```csharp
foreach (string arg in args)
{
    if (!arg.StartsWith("-") && File.Exists(arg))
    {
        sendToFile = arg;
        sendToMode = true;
    }
}
```

## Testing

### Manual Testing Checklist

1. **CLI Mode**:
   - [ ] AML file execution
   - [ ] Document creation with file upload
   - [ ] Error handling for invalid credentials
   - [ ] Error handling for missing files

2. **GUI Mode**:
   - [ ] Form loads correctly
   - [ ] Browse button works
   - [ ] Drag-and-drop works
   - [ ] File path typing works
   - [ ] Upload process completes
   - [ ] Error messages display correctly
   - [ ] Settings save/load correctly

3. **Send To**:
   - [ ] Shortcut creation succeeds
   - [ ] File opens in GUI when using Send To
   - [ ] Upload works from Send To

### Test Data

**Connection Settings** (for testing):
- URL: `http://win-f29goep171e/InnovatorServer`
- Database: `InnovatorSolutions`
- User: `admin`
- Password: `innovator`

## Common Issues and Solutions

### Issue: Browse Button Hangs

**Solution**: 
- Added `[STAThread]` attribute to Main method
- Added form activation before showing dialog
- Made file path field editable as alternative

### Issue: Drag-and-Drop Error

**Solution**: 
- Added `[STAThread]` attribute (required for OLE operations)
- Proper event handlers for DragEnter and DragDrop

### Issue: File Upload "Not Found in Container"

**Solution**: 
- Switched from `setFileProperty()` to `attachPhysicalFile()`
- This method is more reliable for file uploads

### Issue: Relationship Not Created

**Solution**: 
- Use `inn.newItem("Document File", "add")` instead of `document.createRelationship()`
- Set both `source_id` and `related_id` explicitly

## Extension Points

### Adding New Features

1. **New CLI Commands**: Add to argument parsing in `Main()` method
2. **New GUI Features**: Add controls and handlers in `MainForm.cs`
3. **New Item Types**: Extend `CreateDocumentAndCheckinFile()` or create similar methods

### Code Style

- Use meaningful variable names
- Add comments for complex logic
- Follow existing error handling patterns
- Maintain thread safety for UI updates

## Dependencies

### External Libraries

- **Aras.IOM**: Provided by Aras via NuGet
- **System.Windows.Forms**: .NET Framework built-in
- **System.Drawing**: .NET Framework built-in

### System Requirements

- Windows OS
- .NET Framework 4.7.2 or higher
- Access to Aras Innovator server

## Version Control

### Git Workflow

- Main branch: `master`
- Commits should include descriptive messages
- Use conventional commit messages when possible

### Recent Changes

- Added GUI mode
- Added Send To integration
- Implemented file upload functionality
- Added settings persistence

## References

- Aras IOM SDK Documentation
- Aras Community: https://community.aras.com/
- File Upload Pattern: https://community.aras.com/discussions/development/upload-a-file-using-c-as-a-server-method/6209

## Future Enhancements

Potential improvements:
- Support for multiple file uploads
- Batch document creation
- Progress bar for large file uploads
- Configuration wizard for first-time setup
- Support for other item types (Part, CAD, etc.)
- Integration with Aras workflow system

