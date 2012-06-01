namespace Arc.Tests.CommandLineParserTests
{
    using Arc;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class When_parsing_toggle_options
    {
        public class CommandLine : CommandLineParser
        {
            [CommandLineSwitch("opt1", HelpText = "This is option1")]
            public bool Option1 { get; set; }

            [CommandLineSwitch("opt2")]
            public bool Option2 { get; set; }

            [CommandLineSwitch("opt3")]
            public bool Option3 { get; set; }

            [CommandLineSwitch("opt5", DefaultValue = true)]
            public bool Option5 { get; set; }

            public CommandLine(string commandLine)
                : base(commandLine)
            {
            }
        }

        [TestMethod]
        public void Then_options_should_be_set_if_provided()
        {
            var commandLine = new CommandLine("exe -opt1 -opt4 -opt2");

            Assert.IsTrue(commandLine.Option1);
            Assert.IsTrue(commandLine.Option2);
        }

        [TestMethod]
        public void Then_option_should_not_be_set_if_not_provided()
        {
            var commandLine = new CommandLine("exe -opt1 -opt4 -opt2");

            Assert.IsFalse(commandLine.Option3);
        }

        [TestMethod]
        public void Then_option_should_be_set_to_default_if_not_provided()
        {
            var commandLine = new CommandLine("exe -opt1 -opt4 -opt2");

            Assert.AreEqual(true, commandLine.Option5);
        }

        [TestMethod]
        public void Then_UnmatchedParameters_should_contain_the_unmatched_arguments_only()
        {
            var commandLine = new CommandLine("exe -opt1 -opt4 -opt2 -opt6");

            Assert.IsFalse(commandLine.UnmatchedParameters.Contains("-opt1"));
            Assert.IsFalse(commandLine.UnmatchedParameters.Contains("-opt2"));
            Assert.IsTrue(commandLine.UnmatchedParameters.Contains("-opt4"));
            Assert.IsFalse(commandLine.UnmatchedParameters.Contains("-opt5"));
            Assert.IsTrue(commandLine.UnmatchedParameters.Contains("-opt6"));
        }
    }
}


