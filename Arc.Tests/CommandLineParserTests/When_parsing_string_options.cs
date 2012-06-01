namespace Arc.Tests.CommandLineParserTests
{
    using Arc;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class When_parsing_string_options
    {
        public class CommandLine : CommandLineParser
        {
            [CommandLineSwitch("opt1", HelpText = "This is option1")]
            public string Option1 { get; set; }

            [CommandLineSwitch("opt2")]
            public string Option2 { get; set; }

            [CommandLineSwitch("opt3")]
            public string Option3 { get; set; }

            [CommandLineSwitch("opt5", DefaultValue = "blabla")]
            public string Option5 { get; set; }

            public CommandLine(string commandLine)
                : base(commandLine)
            {
            }
        }

        [TestMethod]
        public void Then_options_should_be_set_if_provided()
        {
            var commandLine = new CommandLine("exe -opt1:value1 -opt4 -opt2:\"value2\"");

            Assert.AreEqual("value1", commandLine.Option1);
            Assert.AreEqual("value2", commandLine.Option2);
        }

        [TestMethod]
        public void Then_option_should_not_be_set_if_not_provided()
        {
            var commandLine = new CommandLine("exe -opt1:value1 -opt4 -opt2:\"value2\"");

            Assert.AreEqual(string.Empty, commandLine.Option3);
        }

        [TestMethod]
        public void Then_option_should_be_set_to_default_if_not_provided()
        {
            var commandLine = new CommandLine("exe -opt1:value1 -opt4 -opt2:\"value2\"");

            Assert.AreEqual("blabla", commandLine.Option5);
        }
    }
}