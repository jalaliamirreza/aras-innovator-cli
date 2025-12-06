# Aras Innovator CLI

A command-line and graphical user interface tool for interacting with Aras Innovator PLM Solution. This tool enables document creation, file uploads, and AML (Aras Markup Language) execution through both CLI and GUI interfaces.

## Features

- **Command-Line Interface (CLI)**: Execute AML queries and commands
- **Graphical User Interface (GUI)**: User-friendly Windows Forms application for document uploads
- **Document Creation**: Create Document items in Aras Innovator
- **File Upload**: Upload files to Aras vault with automatic linking
- **Send To Integration**: Right-click any file in Windows Explorer to upload directly
- **Settings Persistence**: Save connection settings for quick access

## Installation

### Prerequisites

- .NET Framework 4.7.2 or higher
- Windows operating system
- Access to Aras Innovator server

### Build from Source

1. Clone the repository:
```bash
git clone https://github.com/jalaliamirreza/aras-innovator-cli.git
cd aras-innovator-cli
```

2. Restore NuGet packages:
```bash
nuget restore
```

3. Build the solution:
```bash
msbuild ArasCLI.sln /t:Build /p:Configuration=Release
```

The executable will be in `ArasCLI\bin\Release\ArasCLI.exe`

## Usage

### GUI Mode

1. **Launch the GUI**:
   - Double-click `ArasCLI.exe` or run with `--gui` flag
   - The GUI will open automatically if no command-line arguments are provided

2. **Fill in Connection Settings**:
   - **Aras URL**: Your Aras Innovator server URL (e.g., `http://server/InnovatorServer`)
   - **Database**: Database name (e.g., `InnovatorSolutions`)
   - **User**: Your Aras username
   - **Password**: Your Aras password
   - Check "Save credentials" to remember URL, Database, and User (password is never saved)

3. **Select File to Upload**:
   - Click "Browse..." button to select a file
   - Or type/paste the file path directly
   - Or drag and drop a file into the "File to Upload" field

4. **Set Document Properties** (optional):
   - **Document Name**: Name for the document (auto-filled from filename if empty)
   - **Item Number**: Item number for the document (auto-generated if empty)

5. **Upload**:
   - Click "Upload to Aras" button
   - Monitor progress in the log area
   - Status will show success or error messages

### Send To Integration

1. **Setup Send To**:
   - Open the GUI
   - Click "Setup Send To..." button
   - A shortcut will be created in your Windows SendTo folder

2. **Use Send To**:
   - Right-click any file in Windows Explorer
   - Select "Send to" → "Aras Innovator Upload"
   - The GUI will open with the file pre-selected
   - Fill in connection settings and click "Upload to Aras"

### Command-Line Interface

#### Basic Syntax

```bash
ArasCLI.exe -l <url> -d <database> -u <user> -p <password> [options]
```

#### Mandatory Parameters

- `-l` or `--url`: Aras Innovator server URL
- `-d` or `--database`: Database name
- `-u` or `--user`: Username
- `-p` or `--password`: Password

#### Optional Parameters

- `-f` or `--inputfile`: Input AML file path
- `-o` or `--outputfile`: Output file path for results
- `-g` or `--log`: Log file path
- `-c` or `--config`: Configuration file path
- `--create-doc`: Enable document creation mode
- `--file` or `-file`: File path to upload (required with `--create-doc`)
- `--name` or `-name`: Document name
- `--item-number` or `-n`: Document item number (auto-generated if not provided)
- `--gui`: Launch GUI mode

#### Examples

**Execute AML file**:
```bash
ArasCLI.exe -l http://server/InnovatorServer -d InnovatorSolutions -u admin -p innovator -f query.xml
```

**Create document and upload file**:
```bash
ArasCLI.exe -l http://server/InnovatorServer -d InnovatorSolutions -u admin -p innovator --create-doc --file "C:\path\to\file.pdf" --name "My Document"
```

**Launch GUI**:
```bash
ArasCLI.exe --gui
```

**Show help**:
```bash
ArasCLI.exe -h
```

## Configuration File

You can create a configuration file to store connection settings:

```
l:http://server/InnovatorServer
d:InnovatorSolutions
u:admin
p:innovator
```

Use it with:
```bash
ArasCLI.exe -c config.txt [other options]
```

## Technical Details

### File Upload Process

1. **Document Creation**: Creates a Document item in Aras Innovator
2. **File Upload**: Uploads the file to the Aras vault using `attachPhysicalFile()` method
3. **Relationship Creation**: Creates a Document File relationship to link the file to the document
4. **Verification**: Verifies the file was uploaded and linked successfully

### Architecture

- **Language**: C# (.NET Framework 4.7.2)
- **UI Framework**: Windows Forms
- **Aras SDK**: Aras.IOM 15.0.1
- **Build System**: MSBuild

### Project Structure

```
aras-innovator-cli/
├── ArasCLI/
│   ├── Program.cs          # Main entry point, CLI logic
│   ├── MainForm.cs         # GUI form implementation
│   ├── ArasCLI.csproj      # Project file
│   └── App.config          # Application configuration
├── README.md               # This file
├── DEVELOPER.md            # Developer documentation
└── ArasCLI.sln            # Solution file
```

## Troubleshooting

### Browse Button Not Responding
- Ensure you're using the latest build
- Try typing the file path directly or using drag-and-drop
- Check Windows event logs for errors

### Connection Errors
- Verify the Aras server URL is correct
- Check network connectivity
- Ensure credentials are correct
- Verify database name matches your Aras instance

### File Upload Errors
- Ensure the file path is accessible
- Check file permissions
- Verify Aras vault server is running and accessible
- Check Aras server logs for detailed error messages

### Send To Not Working
- Run "Setup Send To..." again from the GUI
- Check if shortcut exists in: `%APPDATA%\Microsoft\Windows\SendTo\`
- Ensure you have write permissions to the SendTo folder

## License

This project is open source. Please refer to the license file for details.

## Contributing

Contributions are welcome! Please feel free to submit pull requests or open issues.

## Support

For issues, questions, or contributions, please visit the GitHub repository:
https://github.com/jalaliamirreza/aras-innovator-cli

## Version History

### Version 0.1
- Initial release
- CLI functionality for AML execution
- GUI for document creation and file upload
- Send To integration
- Settings persistence

## Acknowledgments

- Built using Aras IOM SDK
- Based on Aras community patterns for file upload
- Community discussion reference: https://community.aras.com/discussions/development/upload-a-file-using-c-as-a-server-method/6209
