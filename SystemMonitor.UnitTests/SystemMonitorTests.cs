using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using CrossCutting;

namespace SysMon.UnitTests
{
    [TestFixture]
    public class SystemMonitorTests
    {

        [Test]
        public void CheckCrashloggerCalled_Called_returnsTrue() // Determines whether the Crashlogger is called
        {
            ValidFileExtensionFakeExtensionManager stub = new ValidFileExtensionFakeExtensionManager(); // stub file extension manager

            FakeCrashLoggerNoException mockCrashLogger = new FakeCrashLoggerNoException();

            FakeCorruptFileLogger fakeCorruptFileLogger = new FakeCorruptFileLogger();

            FakeFileSystemInteractor stubFileSystemInteractor = new FakeFileSystemInteractor();

            var sm = new SystemMonitor();

            sm.corruptFileLogger = fakeCorruptFileLogger;

            sm.fileSystemInteractions = stubFileSystemInteractor;

            sm.crashLogger = mockCrashLogger; // Assign the mock object for testing

            sm.fileManager = stub;

            sm.ProcessDumps();

            Assert.IsTrue(mockCrashLogger.crashLoggerCalled); // We've provided the systemMonitor valid test data. The crashlogger should be called

            
        }


        [Test]
        public void CheckCrashLoggerCalled_NotCalled_returnsTrue() // Determines whether the crashlogger is called -> with the data provided here, it shouldn't be
        {
            NotValidFileExtensionManager stub = new NotValidFileExtensionManager(); // stub file extension manager

            FakeCrashLoggerNoException mockCrashLogger = new FakeCrashLoggerNoException();

            FakeFileSystemInteractor stubFileSystemInteractor = new FakeFileSystemInteractor();

            FakeCorruptFileLogger stubCorruptFileLogger = new FakeCorruptFileLogger();

            var sm = new SystemMonitor();

            sm.crashLogger = mockCrashLogger; // Assign the mock object for testing

            sm.fileManager = stub;

            sm.corruptFileLogger = stubCorruptFileLogger;

            sm.fileSystemInteractions = stubFileSystemInteractor;

            sm.ProcessDumps();

            Assert.IsFalse(mockCrashLogger.crashLoggerCalled); // We've provided the systemMonitor invalid data i.e there's no .dmp files in test data. The crashlogger shouldn't be called


        }

        [Test]
        public void CheckThatEmailServiceIsCalledWhenCrashLoggerThrowsExep_ThrowsException_ReturnsTrue() // Email Logger should be called when the crashLogger throws and Exception
        {
            ValidFileExtensionFakeExtensionManager stub = new ValidFileExtensionFakeExtensionManager(); // stub file extension manager

            FakeCrashLoggerThrowsException stubCrashLogger = new FakeCrashLoggerThrowsException();

            FakeEmailLogger mockEmailLogger = new FakeEmailLogger();

            FakeFileSystemInteractor stubFileSystemInteractor = new FakeFileSystemInteractor();

            var sm = new SystemMonitor();

            sm.crashLogger = stubCrashLogger;

            sm.fileManager = stub;

            sm.fileSystemInteractions = stubFileSystemInteractor;

            sm.emailLogger = mockEmailLogger;

            sm.ProcessDumps();

            Assert.IsTrue(mockEmailLogger.sendmailCalled); // The Email logger should have it's sendmail method called as the crashlogger has thrown an exception


        }

        [Test]
        public void CheckCorruptFileLoggerCalledWhenGivenInvalidFileExtension_GivenInvalidFileExtension_ReturnsTrue() 
        {
            NotValidFileExtensionManager stub = new NotValidFileExtensionManager(); // stub file extension manager

            FakeCorruptFileLogger fakeCorruptFileLogger = new FakeCorruptFileLogger();

            FakeFileSystemInteractor stubFileSystemInteractor = new FakeFileSystemInteractor();


            var sm = new SystemMonitor();

            sm.fileManager = stub;

            sm.fileSystemInteractions = stubFileSystemInteractor;

            sm.corruptFileLogger = fakeCorruptFileLogger;

            sm.ProcessDumps();

            Assert.IsTrue(fakeCorruptFileLogger.loggerCalled); // The Email logger should have it's sendmail method called as the crashlogger has thrown an exception

        }

        [Test]
        public void CheckEmailLoggerCalledCorruptLoggerThrowsException_ThrowsException_ReturnsTrue()
        {
            NotValidFileExtensionManager stub = new NotValidFileExtensionManager(); // stub file extension manager

            FakeCorruptFileLoggerThrowsException fakeCorruptFileLogger = new FakeCorruptFileLoggerThrowsException();

            FakeEmailLogger mockEmailLogger = new FakeEmailLogger();

            FakeFileSystemInteractor stubFileSystemInteractor = new FakeFileSystemInteractor();

            var sm = new SystemMonitor();

            sm.fileManager = stub;

            sm.corruptFileLogger = fakeCorruptFileLogger;

            sm.fileSystemInteractions = stubFileSystemInteractor;

            sm.emailLogger = mockEmailLogger;

            sm.ProcessDumps();

            Assert.IsTrue(mockEmailLogger.sendmailCalled); // The Email logger should have it's sendmail method called as the corruptFileLogger has thrown an exception

        }

        [Test]
        public void CheckDumpFolderCalled_IfCalled_returnsTrue()
        {
            // Setup fakes
            NotValidFileExtensionManager stubFileExtensionManager = new NotValidFileExtensionManager(); // stub file extension manager

            FakeCorruptFileLoggerThrowsException stubCorruptFileLogger = new FakeCorruptFileLoggerThrowsException();

            FakeEmailLogger stubEmailLogger = new FakeEmailLogger();

            FakeFileSystemInteractor mockFileSystemInteractor = new FakeFileSystemInteractor();

            var sm = new SystemMonitor();

            sm.fileManager = stubFileExtensionManager;

            sm.corruptFileLogger = stubCorruptFileLogger;

            sm.emailLogger = stubEmailLogger;

            sm.fileSystemInteractions = mockFileSystemInteractor;

            sm.ProcessDumps();

            Assert.IsTrue(mockFileSystemInteractor.createDircalled); // The Email logger should have it's sendmail method called as the corruptFileLogger has thrown an exception

        }

        

        internal class ValidFileExtensionFakeExtensionManager : IExtensionManager
        {
            public bool readAndDeleteCalled = false;

            public string readAndDeleteDumpfile(string dumpFilespec)
            {
                readAndDeleteCalled = true;
                return "Normal";
            }


            public string[] scanAndReadDumpfileNames()
            {
                string[] array = { ".dmp", ".dmp", ".dmp" };
                return array;
            }

        }

        internal class FakeFileSystemInteractor : IFileSystemInteractions
        {
            public bool createDircalled = false;

            public void createDumpFolderIfNecessary()
            {
                createDircalled = true; // Method was called
            }
        }





        internal class NotValidFileExtensionManager : IExtensionManager
        {
            public bool readAndDeleteCalled = false;

            public string readAndDeleteDumpfile(string dumpFilespce)
            {
                readAndDeleteCalled = true; // With the given file extensions, this method shouldn't be called
                return "Normal";
            }

            public string[] scanAndReadDumpfileNames()
            {
                string[] array = {".exe", ".exe", ".exe"}; // Provide invalid file extension
                return array;
            }
        }

        internal class FakeCrashLoggerNoException : ICrashLogger
        {
            public bool crashLoggerCalled = false;

            public void LogError(string message, string dumpFilespecAndStatus)
            {
                crashLoggerCalled = true;
            }
        }

        internal class FakeCrashLoggerThrowsException : ICrashLogger // Always throws an Exception
        {
            public void LogError(string message, string dumpFilespecAndStatus)
            {
                throw new Exception();
            }
        }

        internal class FakeEmailLogger: IEmailLogger
        {
            public bool sendmailCalled = false;

            public void SendEmail(string to, string subject, string body)
            {
                sendmailCalled = true;
            }
        }

        internal class FakeCorruptFileLogger: ICorruptFileLogger
        {
            public bool loggerCalled = false;

            public void LogCorruptionDetails(string message, string dumpFilespecAndStatus)
            {
                loggerCalled = true;
            }

        }

        internal class FakeCorruptFileLoggerThrowsException : ICorruptFileLogger
        {
            public void LogCorruptionDetails(string message, string dumpFilespecAndStatus)
            {
                throw new Exception();
            }
        }
    }
}
