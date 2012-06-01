namespace Arc.Tests.CommandLineParserTests
{
    using System.Collections.Generic;
    using Arc;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class When_parsing_a_mixed_command_line
    {
        public enum Command { open, close, read, write };
        public enum LogLevel { debug, info, error };

        public class CommandLine : CommandLineParser
        {
            [CommandLineArgument("command", 1)]
            public Command Command { get; set; }

            [CommandLineArgument("password", 2)]
            public string Password { get; set; }

            [CommandLineArgument("fileName", 3)]
            public string FileName { get; set; }

            [CommandLineArgument("repeat", 4, DefaultValue = 1)]
            public int Repeat { get; set; }

            [CommandLineSwitch("silent")]
            public bool Silent { get; set; }

            [CommandLineSwitch("overwrite", DefaultValue = true)]
            public bool Overwrite { get; set; }

            [CommandLineSwitch("loglevel", DefaultValue = LogLevel.info)]
            public LogLevel LogLevel { get; set; }

            [CommandLineSwitch("buffer")]
            public int BufferSize { get; set; }

            [CommandLineSwitch("logfile", DefaultValue = "log.txt")]
            public string LogFile { get; set; }

            public CommandLine(string commandLine)
                : base(commandLine)
            {
            }

            public CommandLine()
                : base(string.Empty)
            {
            }

        }

        public class TestCase
        {
            public string CommandLine;
            public CommandLine Expected;
        }

        private readonly List<TestCase> _testCases = new List<TestCase>
        {
            new TestCase
            {
                CommandLine=@"exe close hax0r ""database.xml"" 3 -silent -overwrite -loglevel:error -buffer:1024 -logfile:""logfile.log""",
                Expected=new CommandLine{Command = Command.close, Password = "hax0r", FileName = "database.xml", Repeat = 3, Silent = true, Overwrite = true, LogLevel = LogLevel.error, BufferSize = 1024, LogFile = "logfile.log"}
            },
            new TestCase
            {
                CommandLine=@"exe read hax0r ""database.xml"" -silent -overwrite -buffer:1024 -logfile:""logfile.log""",
                Expected=new CommandLine{Command = Command.read, Password = "hax0r", FileName = "database.xml", Repeat = 1, Silent = true, Overwrite = true, LogLevel = LogLevel.info, BufferSize = 1024, LogFile = "logfile.log"}
            },
            new TestCase
            {
                CommandLine=@"exe open   -silent -overwrite -loglevel:error -buffer:1024 -logfile:""log.txt"" hax0r ""database.xml"" 3 ",
                Expected=new CommandLine{Command = Command.open, Password = "hax0r", FileName = "database.xml", Repeat = 3, Silent = true, Overwrite = true, LogLevel = LogLevel.error, BufferSize = 1024, LogFile = "log.txt"}
            },
            new TestCase
            {
                CommandLine=@"exe write -loglevel:error",
                Expected=new CommandLine{Command = Command.write, Password = string.Empty, FileName = string.Empty, Repeat = 1, Silent = false, Overwrite = true, LogLevel = LogLevel.error, BufferSize = -1, LogFile = "log.txt"}
            },
            new TestCase
            {
                CommandLine=@"exe",
                Expected=new CommandLine{Command = Command.open, Password = string.Empty, FileName = string.Empty, Repeat = 1, Silent = false, Overwrite = true, LogLevel = LogLevel.info, BufferSize = -1, LogFile = "log.txt"}
            }
        };

        [TestMethod]
        public void All_cases_shuold_pass()
        {
            foreach (var testCase in _testCases)
            {
                var actual = new CommandLine(testCase.CommandLine);

                AssertTestCase(testCase.CommandLine, testCase.Expected, actual);
            }
        }

        private void AssertTestCase(string testCase, CommandLine expected, CommandLine actual)
        {
            Assert.AreEqual(testCase, actual.CommandLine, string.Format("Property: Command, Test case: {0}", testCase));
            Assert.AreEqual(expected.Command, actual.Command, string.Format("Property: Command, Test case: {0}", testCase));
            Assert.AreEqual(expected.Password, actual.Password, string.Format("Property: Password, Test case: {0}", testCase));
            Assert.AreEqual(expected.FileName, actual.FileName, string.Format("Property: FileName, Test case: {0}", testCase));
            Assert.AreEqual(expected.Repeat, actual.Repeat, string.Format("Property: Repeat, Test case: {0}", testCase));
            Assert.AreEqual(expected.Silent, actual.Silent, string.Format("Property: Silent, Test case: {0}", testCase));
            Assert.AreEqual(expected.Overwrite, actual.Overwrite, string.Format("Property: Overwrite, Test case: {0}", testCase));
            Assert.AreEqual(expected.LogLevel, actual.LogLevel, string.Format("Property: LogLevel, Test case: {0}", testCase));
            Assert.AreEqual(expected.BufferSize, actual.BufferSize, string.Format("Property: BufferSize, Test case: {0}", testCase));
            Assert.AreEqual(expected.LogFile, actual.LogFile, string.Format("Property: LogFile, Test case: {0}", testCase));
        }
    }
}