using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace CatiaTest
{
    /// <summary>
    /// CLI test program to debug CATIA COM connection.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== CATIA COM Connection Test v2 ===");
            Console.WriteLine();
            Console.WriteLine("This test will help diagnose CATIA COM automation issues.");
            Console.WriteLine();

            // First, check if CATIA is running
            var catiaProcesses = Process.GetProcessesByName("CNEXT");
            Console.WriteLine($"[Info] CATIA processes found: {catiaProcesses.Length}");
            foreach (var p in catiaProcesses)
            {
                Console.WriteLine($"  PID: {p.Id}, Started: {p.StartTime}");
            }
            Console.WriteLine();

            // Method 1: Try Marshal.GetActiveObject
            Console.WriteLine("[Test 1] Trying Marshal.GetActiveObject(\"CATIA.Application\")...");
            dynamic catiaFromROT = null;
            try
            {
                catiaFromROT = Marshal.GetActiveObject("CATIA.Application");
                Console.WriteLine("  SUCCESS: Got object from ROT");
                Console.WriteLine($"  Object type: {catiaFromROT.GetType()}");
            }
            catch (COMException ex)
            {
                Console.WriteLine($"  FAILED (COMException): {ex.Message}");
                Console.WriteLine($"  HResult: 0x{ex.HResult:X8}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  FAILED: {ex.GetType().Name} - {ex.Message}");
            }

            // Test the object if we got it
            if (catiaFromROT != null)
            {
                Console.WriteLine();
                Console.WriteLine("[Test 1b] Testing CATIA object from ROT...");
                TestCatiaInstance(catiaFromROT);

                try { Marshal.ReleaseComObject(catiaFromROT); } catch { }
            }

            Console.WriteLine();
            Console.WriteLine("========================================");
            Console.WriteLine();

            // Method 2: Launch CATIA fresh with CATSTART and automation enabled
            Console.WriteLine("[Test 2] Launching NEW CATIA instance with CATSTART...");
            Console.WriteLine("  This method enables COM automation properly.");
            Console.WriteLine();

            string catStartPath = @"C:\Program Files\Dassault Systemes\B32\win_b64\code\bin\CATSTART.exe";
            if (!File.Exists(catStartPath))
            {
                Console.WriteLine($"  CATSTART not found at: {catStartPath}");
                Console.WriteLine("  Trying alternative paths...");

                // Try to find CATSTART
                string[] possiblePaths = new[]
                {
                    @"C:\Program Files\Dassault Systemes\B31\win_b64\code\bin\CATSTART.exe",
                    @"C:\Program Files\Dassault Systemes\B30\win_b64\code\bin\CATSTART.exe",
                    @"C:\Program Files\Dassault Systemes\B29\win_b64\code\bin\CATSTART.exe",
                };

                foreach (var path in possiblePaths)
                {
                    if (File.Exists(path))
                    {
                        catStartPath = path;
                        Console.WriteLine($"  Found: {catStartPath}");
                        break;
                    }
                }
            }

            if (File.Exists(catStartPath))
            {
                Console.WriteLine();
                Console.WriteLine("  Do you want to launch a NEW CATIA instance? (y/n)");
                Console.WriteLine("  NOTE: Close your existing CATIA first for best results.");
                var key = Console.ReadKey();
                Console.WriteLine();

                if (key.KeyChar == 'y' || key.KeyChar == 'Y')
                {
                    Console.WriteLine();
                    Console.WriteLine("  Launching CATIA via CATSTART...");

                    try
                    {
                        var process = new Process();
                        process.StartInfo.FileName = catStartPath;
                        // -batch flag is NOT used here - we want interactive mode with automation
                        process.StartInfo.Arguments = "-run \"CNEXT.exe\" -env CATIA_P3.V5-6R2022.B32 -direnv \"C:\\ProgramData\\DassaultSystemes\\CATEnv\"";
                        process.StartInfo.UseShellExecute = false;
                        process.Start();

                        Console.WriteLine("  CATIA is starting...");
                        Console.WriteLine("  Waiting 30 seconds for CATIA to initialize...");

                        for (int i = 30; i > 0; i--)
                        {
                            Console.Write($"\r  {i} seconds remaining...  ");
                            Thread.Sleep(1000);
                        }
                        Console.WriteLine();
                        Console.WriteLine();

                        // Now try to connect
                        Console.WriteLine("[Test 2b] Connecting to newly launched CATIA...");
                        try
                        {
                            dynamic catia = Marshal.GetActiveObject("CATIA.Application");
                            Console.WriteLine("  SUCCESS: Connected to CATIA!");
                            TestCatiaInstance(catia);
                            Marshal.ReleaseComObject(catia);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"  FAILED: {ex.Message}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"  Launch failed: {ex.Message}");
                    }
                }
            }
            else
            {
                Console.WriteLine("  CATSTART.exe not found. Cannot test fresh launch.");
            }

            Console.WriteLine();
            Console.WriteLine("========================================");
            Console.WriteLine();

            // Method 3: Try CreateInstance
            Console.WriteLine("[Test 3] Trying Type.GetTypeFromProgID + CreateInstance...");
            Console.WriteLine("  This will create a new CATIA instance via COM...");
            try
            {
                Type catiaType = Type.GetTypeFromProgID("CATIA.Application");
                if (catiaType != null)
                {
                    Console.WriteLine($"  Found type: {catiaType.FullName}");
                    Console.WriteLine("  Creating instance...");

                    dynamic catia = Activator.CreateInstance(catiaType);
                    if (catia != null)
                    {
                        Console.WriteLine("  SUCCESS: Created CATIA instance");

                        try
                        {
                            catia.Visible = true;
                            Console.WriteLine("  Set Visible = true");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"  Visible failed: {ex.Message}");
                        }

                        TestCatiaInstance(catia);
                        Marshal.ReleaseComObject(catia);
                    }
                    else
                    {
                        Console.WriteLine("  FAILED: Instance is null");
                    }
                }
                else
                {
                    Console.WriteLine("  FAILED: Type not found");
                }
            }
            catch (COMException ex)
            {
                Console.WriteLine($"  FAILED (COMException): {ex.Message}");
                Console.WriteLine($"  HResult: 0x{ex.HResult:X8}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  FAILED: {ex.GetType().Name} - {ex.Message}");
            }

            Console.WriteLine();
            Console.WriteLine("========================================");
            Console.WriteLine();
            Console.WriteLine("RECOMMENDATIONS:");
            Console.WriteLine("1. If E_FAIL errors occur, CATIA was not started with automation enabled.");
            Console.WriteLine("2. Close CATIA completely and use CATSTART.exe to launch it.");
            Console.WriteLine("3. Or use Test 2 above to launch CATIA properly.");
            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        static void TestCatiaInstance(dynamic catia)
        {
            Console.WriteLine();
            Console.WriteLine("  --- Testing CATIA Instance ---");

            // Test basic properties
            try
            {
                Console.WriteLine($"  Caption: {catia.Caption}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  Caption: ERROR - {ex.Message}");
            }

            try
            {
                Console.WriteLine($"  Visible: {catia.Visible}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  Visible: ERROR - {ex.Message}");
            }

            // Test Documents collection
            Console.WriteLine();
            Console.WriteLine("  --- Testing Documents Collection ---");
            try
            {
                dynamic documents = catia.Documents;
                Console.WriteLine($"  Documents object: {(documents != null ? "OK" : "NULL")}");

                if (documents != null)
                {
                    try
                    {
                        int count = documents.Count;
                        Console.WriteLine($"  Documents.Count: {count}");

                        if (count > 0)
                        {
                            Console.WriteLine();
                            Console.WriteLine("  --- Listing Documents ---");
                            for (int i = 1; i <= count; i++)
                            {
                                try
                                {
                                    dynamic doc = documents.Item(i);
                                    string name = doc.Name;
                                    string fullName = doc.FullName;
                                    Console.WriteLine($"  [{i}] Name: {name}");
                                    Console.WriteLine($"      Path: {fullName}");
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"  [{i}] ERROR: {ex.Message}");
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"  Documents.Count: ERROR - {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  Documents: ERROR - {ex.Message}");
            }

            // Test ActiveDocument
            Console.WriteLine();
            Console.WriteLine("  --- Testing ActiveDocument ---");
            try
            {
                dynamic activeDoc = catia.ActiveDocument;
                if (activeDoc != null)
                {
                    Console.WriteLine("  ActiveDocument: OK (not null)");

                    try
                    {
                        Console.WriteLine($"  ActiveDocument.Name: {activeDoc.Name}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"  ActiveDocument.Name: ERROR - {ex.Message}");
                    }

                    try
                    {
                        Console.WriteLine($"  ActiveDocument.FullName: {activeDoc.FullName}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"  ActiveDocument.FullName: ERROR - {ex.Message}");
                    }

                    try
                    {
                        Console.WriteLine($"  ActiveDocument.Saved: {activeDoc.Saved}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"  ActiveDocument.Saved: ERROR - {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine("  ActiveDocument: NULL");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ActiveDocument: ERROR - {ex.Message}");
            }

            // Test Windows collection
            Console.WriteLine();
            Console.WriteLine("  --- Testing Windows ---");
            try
            {
                dynamic windows = catia.Windows;
                if (windows != null)
                {
                    int winCount = windows.Count;
                    Console.WriteLine($"  Windows.Count: {winCount}");

                    for (int i = 1; i <= winCount; i++)
                    {
                        try
                        {
                            dynamic win = windows.Item(i);
                            Console.WriteLine($"  [{i}] Caption: {win.Caption}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"  [{i}] ERROR: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  Windows: ERROR - {ex.Message}");
            }

            Console.WriteLine();
        }
    }
}
