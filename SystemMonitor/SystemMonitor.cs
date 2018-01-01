using System;
using System.IO;
using System.Windows.Forms;


namespace CrossCutting
{
    // Code will first need to be refactored to make it more testable it currently contains a large number of concrete dependencies.
    // To promote a more testable design we will need to add some "Layers of indirection" as osherove refers to them as. 
    public class SystemMonitor
    {
        // I've replaced the concrete dependencies with interfaces. This allows for a seam to be created for testing
        public IExtensionManager fileManager { get; set; }
        public ICrashLogger crashLogger { get; set; }
        public IEmailLogger emailLogger { get; set; }
        public ICorruptFileLogger corruptFileLogger { get; set; }
        public IFileSystemInteractions fileSystemInteractions { get; set; }

        string path = Application.StartupPath + @"/Dumps";
        string dumpFileStatus;

        public SystemMonitor()
        {
             fileManager = new FileExtensionManager();
             crashLogger = new CrashLoggingService();
             emailLogger = new EmailService();
             corruptFileLogger = new CorruptFileLoggingService();
             fileSystemInteractions = new FileSystemInteractorService(path);

        }

        public void ProcessDumps()
        {
            string[] dumpFileNames;

            // The CreateDumpFolderIfNeccessary Method requires interactacting with the file system. Doing so would mean integration testing. I'm going to create a separate class for this method in ServiceManagers
          fileSystemInteractions.createDumpFolderIfNecessary(); // Allows me to add a layer of indirection for testing. I can now sub in a fake

            dumpFileNames = fileManager.scanAndReadDumpfileNames();   // Off to disk to read what dump files are there
            if (dumpFileNames.Length != 0)
            {
                foreach (string filespec in dumpFileNames)  // Stay in loop processing each dump file. 
                {
                    if (filespec.Contains(".dmp") && filespec.Length < 1600)  // business logic to check the dump file is valid
                    {
                        /* Dump file valid so log details with the crashLoggingService Web service. */
                        dumpFileStatus = fileManager.readAndDeleteDumpfile(filespec);
                        try
                        {
                            crashLogger.LogError("Dump file is valid ", filespec + dumpFileStatus);  // Call the crashLogger to record the crash
                                                                                                     // called crashLoggingService

                        }
                        catch (Exception e)
                        {
                            // The CrashLogger threw an exception so use the emailLogger to email the helpdesk
                            emailLogger.SendEmail("HelpDesk@lit.ie", "crashLoggingService Web service threw exception", e.Message);
                        }
                    }
                    else
                    {
                        /* Dump file is invalid so log details with the corruptFileLoggingService Web service. */
                        dumpFileStatus = fileManager.readAndDeleteDumpfile(filespec);
                        try
                        {
                            corruptFileLogger.LogCorruptionDetails("Dump file is corrupt: ", filespec + dumpFileStatus); // Call the coppuptFileLogger as the crash resulted in a corrupt dump file. 
                        }
                        catch (Exception e)
                        {
                            // The corruptFileLogger threw an exception so use the emailLogger to email the helpdesk
                            emailLogger.SendEmail("HelpDesk@lit.ie", "corruptFileLoggingService Web service threw exception", e.Message);
                        }
                    }
                }
            }

        }

       


    }
}
