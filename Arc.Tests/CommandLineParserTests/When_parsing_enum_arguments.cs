namespace Arc.Tests.CommandLineParserTests
{
    using Arc;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class When_parsing_enum_arguments
    {
        public enum Enum1 { one, two, three };
        public enum Enum2 { aaa, bbb, ccc };
        public enum Enum3 { mon, tue, wed };

        public class CommandLine : CommandLineParser
        {
            [CommandLineArgument("opt1", 1, HelpText = "This is option1")]
            public Enum1 Option1 { get; set; }

            [CommandLineArgument("opt2", 2)]
            public Enum2 Option2 { get; set; }

            [CommandLineArgument("opt3", 3)]
            public Enum3 Option3 { get; set; }

            [CommandLineArgument("opt5", 4, DefaultValue = Enum3.tue)]
            public Enum3 Option5 { get; set; }

            public CommandLine(string commandLine)
                : base(commandLine)
            {
            }
        }

        [TestMethod]
        public void Then_options_should_be_set_if_provided()
        {
            var commandLine = new CommandLine("exe two \"bbb\"");

            Assert.AreEqual(Enum1.two, commandLine.Option1);
            Assert.AreEqual(Enum2.bbb, commandLine.Option2);
        }

        [TestMethod]
        public void Then_option_should_not_be_set_if_not_provided()
        {
            var commandLine = new CommandLine("exe two \"bbb\"");

            Assert.AreEqual(Enum3.mon, commandLine.Option3);
        }

        [TestMethod]
        public void Then_option_should_be_set_to_default_if_not_provided()
        {
            var commandLine = new CommandLine("exe two \"bbb\"");

            Assert.AreEqual(Enum3.tue, commandLine.Option5);
        }

    }
}