using System.Runtime.InteropServices;
using System.Reflection;

Console.WriteLine("Connecting to CATIA V5-6R2022SP6...");
Console.WriteLine("=" + new string('=', 50));

try
{
    object? catiaApp = null;
    
    // Get the CATIA COM type
    Type? catiaType = Type.GetTypeFromProgID("CATIA.Application");
    
    if (catiaType == null)
    {
        Console.WriteLine("ERROR: CATIA is not installed or not registered on this system.");
        Console.WriteLine("Please ensure CATIA V5-6R2022SP6 is properly installed.");
        return;
    }
    
    Console.WriteLine("CATIA COM type found in registry.");
    Console.WriteLine($"CLSID: {catiaType.GUID}");
    Console.WriteLine();
    
    // Try to create/connect to CATIA instance
    try
    {
        Console.WriteLine("Attempting to connect to CATIA...");
        catiaApp = Activator.CreateInstance(catiaType);
        
        if (catiaApp == null)
        {
            Console.WriteLine("ERROR: Failed to create CATIA instance.");
            return;
        }
        
        Console.WriteLine("CATIA instance obtained successfully!");
        Console.WriteLine("Waiting for CATIA to initialize...");
        
        // Give CATIA time to fully initialize
        Thread.Sleep(3000);
    }
    catch (COMException ex)
    {
        Console.WriteLine($"COM Error (0x{ex.ErrorCode:X8}): {ex.Message}");
        Console.WriteLine();
        Console.WriteLine("Possible reasons:");
        Console.WriteLine("  1. CATIA license server is not available");
        Console.WriteLine("  2. CATIA installation is corrupted");
        Console.WriteLine("  3. Insufficient permissions to launch CATIA");
        Console.WriteLine("  4. Another application is blocking CATIA");
        Console.WriteLine();
        Console.WriteLine($"Full error details: {ex}");
        return;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Unexpected error: {ex.Message}");
        Console.WriteLine($"Full details: {ex}");
        return;
    }
    
    // Successfully connected, now interact with CATIA
    try
    {
        Console.WriteLine();
        Console.WriteLine("Retrieving CATIA information...");
        
        // Get the actual runtime type of the COM object
        Type runtimeType = catiaApp.GetType();
        Console.WriteLine($"Runtime Type: {runtimeType.FullName}");
        
        // Make CATIA visible
        try
        {
            runtimeType.InvokeMember("Visible",
                BindingFlags.SetProperty | BindingFlags.Public | BindingFlags.Instance,
                null,
                catiaApp,
                new object[] { true });
            Console.WriteLine("✓ CATIA window is now visible");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  Warning: Could not set Visible property: {ex.Message}");
        }
        
        Console.WriteLine();
        Console.WriteLine("CATIA Information:");
        
        // Try to get Name property
        try
        {
            object? nameObj = runtimeType.InvokeMember("Name",
                BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance,
                null,
                catiaApp,
                null);
            Console.WriteLine($"  Application Name: {nameObj}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  Application Name: [Unable to retrieve - {ex.Message}]");
        }
        
        // Try to get Version property
        try
        {
            object? versionObj = runtimeType.InvokeMember("Version",
                BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance,
                null,
                catiaApp,
                null);
            Console.WriteLine($"  Version: {versionObj}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  Version: [Unable to retrieve - {ex.Message}]");
        }
        
        // Try to get FullName property
        try
        {
            object? fullNameObj = runtimeType.InvokeMember("FullName",
                BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance,
                null,
                catiaApp,
                null);
            Console.WriteLine($"  Full Path: {fullNameObj}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  Full Path: [Unable to retrieve - {ex.Message}]");
        }
        
        // Try to access Documents collection
        try
        {
            object? documents = runtimeType.InvokeMember("Documents",
                BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance,
                null,
                catiaApp,
                null);
                
            if (documents != null)
            {
                Type documentsType = documents.GetType();
                object? countObj = documentsType.InvokeMember("Count",
                    BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance,
                    null,
                    documents,
                    null);
                    
                int docCount = Convert.ToInt32(countObj);
                Console.WriteLine($"  Open Documents: {docCount}");
                
                if (docCount > 0)
                {
                    Console.WriteLine();
                    Console.WriteLine("Open Documents:");
                    for (int i = 1; i <= docCount; i++)
                    {
                        try
                        {
                            object? doc = documentsType.InvokeMember("Item",
                                BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Instance,
                                null,
                                documents,
                                new object[] { i });
                                
                            if (doc != null)
                            {
                                Type docType = doc.GetType();
                                object? docNameObj = docType.InvokeMember("Name",
                                    BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance,
                                    null,
                                    doc,
                                    null);
                                Console.WriteLine($"  {i}. {docNameObj}");
                                Marshal.ReleaseComObject(doc);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"  {i}. [Error retrieving document: {ex.Message}]");
                        }
                    }
                }
                
                Marshal.ReleaseComObject(documents);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  Open Documents: [Unable to retrieve - {ex.Message}]");
        }
        
        Console.WriteLine();
        Console.WriteLine("=" + new string('=', 50));
        Console.WriteLine("✓ Successfully connected to CATIA!");
        Console.WriteLine("  Check if CATIA window appeared on your screen.");
        Console.WriteLine("=" + new string('=', 50));
        Console.WriteLine();
        Console.WriteLine("Press any key to exit (CATIA will remain open)...");
        Console.ReadKey();
        
        // Clean up COM object
        Marshal.ReleaseComObject(catiaApp);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error while interacting with CATIA: {ex.Message}");
        Console.WriteLine($"Details: {ex}");
        
        if (catiaApp != null)
        {
            try { Marshal.ReleaseComObject(catiaApp); } catch { }
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Unexpected error: {ex.Message}");
    Console.WriteLine($"Full details: {ex}");
}
