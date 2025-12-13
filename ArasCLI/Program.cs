using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Aras.IOM;
using Aras.IOME;

namespace ArasCLI
{
   
    class Program
    {
        public static Item CreateDocumentAndCheckinFile(Innovator inn, HttpServerConnection conn, string filePath, string itemNum, string docName)
        {
            try
            {
                // Create a new Document item
                System.Console.WriteLine("Creating Document item...");
                Item document = inn.newItem("Document", "add");
                
                // Set item_number - required field, generate if not provided
                if (!string.IsNullOrEmpty(itemNum))
                {
                    document.setProperty("item_number", itemNum);
                }
                else
                {
                    // Generate a simple item number based on timestamp
                    string generatedItemNumber = "DOC-" + DateTime.Now.ToString("yyyyMMdd-HHmmss");
                    document.setProperty("item_number", generatedItemNumber);
                    System.Console.WriteLine("Generated item_number: " + generatedItemNumber);
                }
                
                // Set name if provided
                if (!string.IsNullOrEmpty(docName))
                {
                    document.setProperty("name", docName);
                }
                
                // Apply the item to create it in Aras
                document = document.apply();
                
                if (document.isError())
                {
                    System.Console.WriteLine("Error creating document: " + document.getErrorString());
                    return document;
                }
                
                System.Console.WriteLine("Document created successfully. ID: " + document.getID());
                
                // Upload file to the document using Aras file upload mechanism
                System.Console.WriteLine("Uploading file to document...");
                
                try
                {
                    string fileName = Path.GetFileName(filePath);
                    FileInfo fileInfo = new FileInfo(filePath);
                    
                    if (!fileInfo.Exists)
                    {
                        Item errorItem = inn.newError("File not found: " + filePath);
                        return errorItem;
                    }
                    
                    System.Console.WriteLine("File: " + fileName + " (" + fileInfo.Length + " bytes)");
                    
                    // Based on Aras community example: Create File item, attachPhysicalFile, then apply
                    // Reference: https://community.aras.com/discussions/development/upload-a-file-using-c-as-a-server-method/6209
                    System.Console.WriteLine("Creating File item and uploading file...");
                    
                    try
                    {
                        string absoluteFilePath = Path.GetFullPath(filePath);
                        System.Console.WriteLine("File path: " + absoluteFilePath);
                        
                        // Step 1: Create File item (don't set source_id - we'll link via relationship)
                        System.Console.WriteLine("Creating File item...");
                        Item fileItem = inn.newItem("File", "add");
                        fileItem.setProperty("filename", fileName);
                        
                        // Step 2: Attach physical file using attachPhysicalFile
                        // This method marks the file for upload when apply() is called
                        System.Console.WriteLine("Attaching physical file...");
                        fileItem.attachPhysicalFile(absoluteFilePath);
                        
                        // Step 3: Apply the File item - this uploads the file to the vault
                        System.Console.WriteLine("Uploading file to vault...");
                        Item fileResult = fileItem.apply();
                        
                        if (fileResult.isError())
                        {
                            System.Console.WriteLine("File upload error: " + fileResult.getErrorString());
                            System.Console.WriteLine("Document created successfully. File upload may need manual intervention.");
                            return document;
                        }
                        
                        string fileId = fileResult.getID();
                        System.Console.WriteLine("File uploaded successfully. File ID: " + fileId);
                        
                        // Step 4: Create Document File relationship to link the file to the document
                        // Use createRelationship and setRelatedItem to properly link them
                        System.Console.WriteLine("Creating Document File relationship...");
                        Item docFileRel = document.createRelationship("Document File", "add");
                        docFileRel.setRelatedItem(fileResult); // Use setRelatedItem with the File item
                        Item relResult = docFileRel.apply();
                        
                        if (relResult.isError())
                        {
                            System.Console.WriteLine("Warning: Could not create Document File relationship: " + relResult.getErrorString());
                            System.Console.WriteLine("Attempting alternative method...");
                            
                            // Alternative: Create relationship using newItem
                            Item altRel = inn.newItem("Document File", "add");
                            altRel.setProperty("source_id", document.getID());
                            altRel.setProperty("related_id", fileId);
                            Item altResult = altRel.apply();
                            
                            if (altResult.isError())
                            {
                                System.Console.WriteLine("Alternative method also failed: " + altResult.getErrorString());
                                System.Console.WriteLine("File is uploaded but may not be linked to document. File ID: " + fileId);
                            }
                            else
                            {
                                System.Console.WriteLine("Document File relationship created successfully (alternative method).");
                                System.Console.WriteLine("File has been uploaded to vault and linked to document.");
                            }
                        }
                        else
                        {
                            System.Console.WriteLine("Document File relationship created successfully.");
                            System.Console.WriteLine("File has been uploaded to vault and linked to document.");
                        }
                        
                        // Verify the relationship was created
                        System.Console.WriteLine("Verifying relationship...");
                        // Get document with relationships explicitly loaded
                        Item docQuery = inn.newItem("Document", "get");
                        docQuery.setID(document.getID());
                        docQuery.setAttribute("select", "id,item_number,name");
                        Item docFileRelQuery = docQuery.createRelationship("Document File", "get");
                        docFileRelQuery.setAttribute("select", "id,related_id");
                        Item fileQuery = docFileRelQuery.createRelatedItem("File", "get");
                        fileQuery.setAttribute("select", "id,filename,file_size");
                        Item refreshedDoc = docQuery.apply();
                        
                        if (refreshedDoc != null && !refreshedDoc.isError())
                        {
                            Item docFiles = refreshedDoc.getRelationships("Document File");
                            if (docFiles != null && docFiles.getItemCount() > 0)
                            {
                                System.Console.WriteLine("Verified: Document File relationship exists (" + docFiles.getItemCount() + " file(s) linked).");
                                for (int i = 0; i < docFiles.getItemCount(); i++)
                                {
                                    Item rel = docFiles.getItemByIndex(i);
                                    Item relatedFile = rel.getRelatedItem();
                                    if (relatedFile != null && !relatedFile.isError())
                                    {
                                        string relFileId = relatedFile.getID();
                                        string relFileName = relatedFile.getProperty("filename");
                                        string relFileSize = relatedFile.getProperty("file_size");
                                        System.Console.WriteLine("  - Linked File: " + relFileName + " (" + relFileSize + " bytes, ID: " + relFileId + ")");
                                    }
                                }
                            }
                            else
                            {
                                System.Console.WriteLine("Warning: Document File relationship not found in query result.");
                                System.Console.WriteLine("Note: Relationship may be in pending state. Check Document in Aras UI.");
                            }
                        }
                        else
                        {
                            System.Console.WriteLine("Warning: Could not query document for verification.");
                        }
                        
                        // Verify the file was uploaded
                        Item verifyFile = inn.getItemById("File", fileId);
                        if (verifyFile != null && !verifyFile.isError())
                        {
                            string verifyFilename = verifyFile.getProperty("filename");
                            string fileSize = verifyFile.getProperty("file_size");
                            System.Console.WriteLine("Verified: File item exists with filename: " + verifyFilename + " (" + fileSize + " bytes)");
                        }
                        
                        return fileResult;
                    }
                    catch (Exception uploadEx)
                    {
                        System.Console.WriteLine("Exception during file upload: " + uploadEx.Message);
                        if (uploadEx.InnerException != null)
                        {
                            System.Console.WriteLine("Inner exception: " + uploadEx.InnerException.Message);
                        }
                        System.Console.WriteLine("Document created successfully. File upload may need manual intervention.");
                        return document;
                    }
                }
                catch (Exception uploadEx)
                {
                    System.Console.WriteLine("Exception during file upload: " + uploadEx.Message);
                    System.Console.WriteLine("Document created successfully. File upload may need manual intervention.");
                    // Don't fail completely - document was created
                }
                
                // Return the document item with checkin result
                return document;
            }
            catch (Exception ex)
            {
                System.Console.WriteLine("Exception during document creation/checkin: " + ex.Message);
                System.Console.WriteLine(ex.StackTrace);
                
                // Return an error item
                Item errorItem = inn.newError(ex.Message);
                return errorItem;
            }
        }
        
        [STAThread]
        static void Main(string[] args)
        {
            // Check for GUI mode or Send To mode
            bool guiMode = false;
            bool sendToMode = false;
            bool catiaLoginMode = false;
            bool catiaCheckOutMode = false;
            bool catiaCheckInMode = false;
            string sendToFile = null;

            // Check if any argument is a file path (Windows Send To passes file path directly)
            foreach (string arg in args)
            {
                if (arg == "--gui" || arg == "-gui")
                {
                    guiMode = true;
                }
                else if (arg == "--sendto-file" || arg == "-sendto-file")
                {
                    sendToMode = true;
                }
                else if (arg == "--catia-login" || arg == "-catia-login")
                {
                    catiaLoginMode = true;
                }
                else if (arg == "--catia-checkout" || arg == "-catia-checkout")
                {
                    catiaCheckOutMode = true;
                }
                else if (arg == "--catia-checkin" || arg == "-catia-checkin")
                {
                    catiaCheckInMode = true;
                }
                else if (!arg.StartsWith("-") && File.Exists(arg))
                {
                    // This is likely a file path from Send To
                    sendToFile = arg;
                    sendToMode = true;
                }
            }

            // CATIA Login mode - show only login form
            if (catiaLoginMode)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                using (var loginForm = new CatiaLoginForm())
                {
                    loginForm.ShowDialog();
                    // Session is now stored in encrypted file and will persist
                    // for Check-In, Check-Out, and other operations
                }
                return;
            }

            // CATIA Check-Out mode - show check-out form
            if (catiaCheckOutMode)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                using (var checkOutForm = new CatiaCheckOutForm())
                {
                    checkOutForm.ShowDialog();
                }
                return;
            }

            // CATIA Check-In mode - show check-in form
            if (catiaCheckInMode)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                using (var checkInForm = new CatiaCheckInForm())
                {
                    checkInForm.ShowDialog();
                }
                return;
            }

            // If no arguments or GUI mode requested, show GUI
            if (args.Length == 0 || guiMode || sendToMode)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                MainForm form = new MainForm();

                // If Send To mode, set the file path
                if (!string.IsNullOrEmpty(sendToFile))
                {
                    form.SetFileFromSendTo(sendToFile);
                }

                Application.Run(form);
                return;
            }
            
            // CLI mode - continue with existing logic
            string login = "";
            string password = "";
            string database = "";
            string url = "";
            string configFile = "";
            string filepathin = "";
            string filepathout = "";
            string filepathlog = "";
            bool createDocument = false;
            string fileToCheckin = "";
            string itemNumber = "";
            string documentName = "";

            System.Console.WriteLine(@"


                               _____                             _                _____ _      _____ 
        /\                   |_   _|                           | |              / ____| |    |_   _|
       /  \   _ __ __ _ ___    | |  _ __  _ __   _____   ____ _| |_ ___  _ __  | |    | |      | |  
      / /\ \ | '__/ _` / __|   | | | '_ \| '_ \ / _ \ \ / / _` | __/ _ \| '__| | |    | |      | |  
     / ____ \| | | (_| \__ \  _| |_| | | | | | | (_) \ V / (_| | || (_) | |    | |____| |____ _| |_ 
    /_/    \_\_|  \__,_|___/ |_____|_| |_|_| |_|\___/ \_/ \__,_|\__\___/|_|     \_____|______|_____|
                                                                                                 
                                                                                                 
    V0.1

");

            // Test if any input arguments were supplied:
            if (args.Length == 0)
            {
                // display available commands
                System.Console.WriteLine(@"
    AML Loader

    =========== SHORT ===========

    Aras Innovator CLI is an open source project for communicating 
    with Aras Innovator PLM Solution through a command line

    -h or --help for more information

    =========== MANDATORY ===========
    -l  <url>           => Aras URL
    -d  <dbname>        => Aras Database
    -u  <user login>    => Aras User
    -p  <password>      => Aras Password
    -f  <filepath>      => Input AML File
                ");
                return ;
            }

            // test if first argument is -h
            if ((args[0] == "-h") || (args[0] == "--help"))
            {

                System.Console.WriteLine(@"


    =========== DESCRIPTION ===========


    =========== USAGE SYNTAX ===========


    =========== OPTIONS ===========


    ==> MANDATORY 
    -l  <url>           => Aras URL
    -d  <dbname>        => Aras Database
    -u  <user login>    => Aras User
    -p  <password>      => Aras Password
    -f  <filepath>      => Input AML File

    ==> OPTIONNAL
    -g  <filepath>      => Log output file
    -o  <filepath>      => Result output file
    -c  <filepath>      => Instance Config File 
                       template : 
                            l:http://localhost/InnovatorServer
                            d:InnovatorSolution
                            u:admin
                            p:innovator                             

                ");
                return;
            }

                System.Console.WriteLine("");
            // read input arguments
            for (int i = 0; i < args.Length; i++)
            {
                
                switch (args[i])
                {
                    case "-c":
                    case "--config":
                        configFile = args[i + 1];
                        System.Console.WriteLine(" - Config Input = " + args[i + 1]);
                        break;
                    case "-l":
                    case "--url":
                        url = args[i + 1];
                        System.Console.WriteLine(" - URL = "+args[i+1]);
                        break;
                    case "-d":
                    case "--database":
                        database = args[i + 1];
                        System.Console.WriteLine(" - Db = " + args[i + 1]);
                        break;
                    case "-u":
                    case "--user":
                        login = args[i + 1];
                        System.Console.WriteLine(" - Login = " + args[i + 1]);
                        break;
                    case "-p":
                    case "--password":
                        password = args[i + 1];
                        System.Console.WriteLine(" - Password = *****");
                        break;
                    case "-f":
                    case "--inputfile":
                        filepathin = args[i + 1];
                        System.Console.WriteLine(" - Source file = " + args[i + 1]);
                        break;
                    case "-o":
                    case "--outputfile":
                        filepathout = args[i + 1];
                        System.Console.WriteLine(" - Output file = " + args[i + 1]);
                        break;
                    case "-g":
                    case "--log":
                        filepathlog = args[i + 1];
                        System.Console.WriteLine(" - Output file = " + args[i + 1]);
                        break;
                    case "--create-doc":
                    case "-cd":
                        createDocument = true;
                        System.Console.WriteLine(" - Document creation mode enabled");
                        break;
                    case "--file":
                    case "-file":
                        fileToCheckin = args[i + 1];
                        System.Console.WriteLine(" - File to checkin = " + args[i + 1]);
                        break;
                    case "--item-number":
                    case "-n":
                        itemNumber = args[i + 1];
                        System.Console.WriteLine(" - Item number = " + args[i + 1]);
                        break;
                    case "--name":
                    case "-name":
                        documentName = args[i + 1];
                        System.Console.WriteLine(" - Document name = " + args[i + 1]);
                        break;

                }
            }

            System.Console.WriteLine("");
            // test if config file is available and read the content
            System.Console.WriteLine(configFile);

            if (configFile != "")
            {
                string[] configContent;
                try
                {
                    configContent = File.ReadAllLines(configFile);
                    
                    foreach (string line in configContent)
                    {
                        switch (line.Substring(0, 2))
                        {
                            case "l:":
                                url = line.Substring(2);
                                break;
                            case "d:":
                                database = line.Substring(2);
                                break;
                            case "u:":
                                login = line.Substring(2);
                                break;
                            case "p:":
                                password = line.Substring(2);
                                break;
                        }
                    }
                } catch (Exception ex)
                {
                    System.Console.WriteLine(ex);
                    return;
                }
            }


            // test if mandatory arguments are provided
            if (login!="" && password!="" && database!="" && url != "")
            {
                // Aras Connection
                System.Console.WriteLine("... connection ...");
                HttpServerConnection conn;
                Innovator inn;
                try
                {
                    conn = IomFactory.CreateHttpServerConnection(url, database, login, password);
                    Item login_result = conn.Login();
                    System.Console.WriteLine("Log in result : " + !login_result.isError());
                    if (login_result.isError())
                    {
                        System.Console.WriteLine(login_result.getErrorString());
                        return;
                    }
                    System.Console.WriteLine("Instanciate Innovator");
                    inn = IomFactory.CreateInnovator(conn);
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine(ex);
                    return;
                }

                try
                {
                    Item result;
                    
                    // Check if document creation mode is enabled
                    if (createDocument)
                    {
                        // Validate file parameter
                        if (string.IsNullOrEmpty(fileToCheckin))
                        {
                            System.Console.WriteLine("Error: --file parameter is required when using --create-doc");
                            conn.Logout();
                            return;
                        }

                        // Validate file exists
                        if (!File.Exists(fileToCheckin))
                        {
                            System.Console.WriteLine("Error: File not found: " + fileToCheckin);
                            conn.Logout();
                            return;
                        }

                        System.Console.WriteLine("");
                        System.Console.WriteLine("Creating document and checking in file...");
                        result = CreateDocumentAndCheckinFile(inn, conn, fileToCheckin, itemNumber, documentName);
                    }
                    else
                    {
                        // Original AML execution workflow
                        System.Console.WriteLine("");
                        // read AML
                        System.Console.WriteLine("Read AML file");
                        string readAML;
                        try
                        {
                            readAML = File.ReadAllText(filepathin);
                        }
                        catch (Exception ex)
                        {
                            System.Console.WriteLine(ex);
                            conn.Logout();
                            return;
                        }

                        System.Console.WriteLine("Commit AML file");
                        result = inn.applyAML(readAML);
                    }

                    if (result.isError())
                    {
                        System.Console.Write(result.getErrorString());
                    }
                    else
                    {
                        System.Console.Write(result.dom.OuterXml);
                    }

                    if (filepathout != "")
                    {
                        try
                        {
                            File.WriteAllText(filepathout, result.dom.OuterXml);
                        }
                        catch (Exception ex)
                        {
                            System.Console.WriteLine(ex);
                            conn.Logout();
                            return;
                        }
                    }

                    if (filepathlog != "")
                    {
                        try
                        {
                            File.WriteAllText(filepathlog, result.dom.OuterXml);
                        }
                        catch (Exception ex)
                        {
                            System.Console.WriteLine(ex);
                            conn.Logout();
                            return;
                        }
                    }
                }
                finally
                {
                    conn.Logout();
                }
                System.Console.WriteLine(@"

                ");
            } else
            {

                System.Console.WriteLine(@"
 No connection settings provided !
                ");
            }
        }
    }
}
