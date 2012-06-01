namespace Arc.Tests.CommandLineParserTests
{
    using Arc;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class When_parsing_enum_options
    {
        public enum Enum1 { one, two, three };
        public enum Enum2 { aaa, bbb, ccc };
        public enum Enum3 { mon, tue, wed };

        public class CommandLine : CommandLineParser
        {
            [CommandLineSwitch("opt1", HelpText = "This is option1")]
            public Enum1 Option1 { get; set; }

            [CommandLineSwitch("opt2")]
            public Enum2 Option2 { get; set; }

            [CommandLineSwitch("opt3")]
            public Enum3 Option3 { get; set; }

            [CommandLineSwitch("opt5", DefaultValue = Enum3.tue)]
            public Enum3 Option5 { get; set; }

            public CommandLine(string commandLine)
                : base(commandLine)
            {
            }
        }

        [TestMethod]
        public void Then_options_should_be_set_if_provided()
        {
            var commandLine = new CommandLine("exe -opt1:two -opt4 -opt2:\"bbb\"");

            Assert.AreEqual(Enum1.two, commandLine.Option1);
            Assert.AreEqual(Enum2.bbb, commandLine.Option2);
        }

        [TestMethod]
        public void Then_option_should_not_be_set_if_not_provided()
        {
            var commandLine = new CommandLine("exe -opt1:two -opt4 -opt2:\"bbb\"");

            Assert.AreEqual(Enum3.mon, commandLine.Option3);
        }

        [TestMethod]
        public void Then_option_should_be_set_to_default_if_not_provided()
        {
            var commandLine = new CommandLine("exe -opt1:two -opt4 -opt2:\"bbb\"");

            Assert.AreEqual(Enum3.tue, commandLine.Option5);
        }
    }
}