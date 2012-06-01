namespace Arc.Tests.CommandLineParserTests
{
    using Arc;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class When_parsing_int_options
    {
        public class CommandLine : CommandLineParser
        {
            [CommandLineSwitch("opt1", HelpText = "This is option1")]
            public int Option1 { get; set; }

            [CommandLineSwitch("opt2")]
            public int Option2 { get; set; }

            [CommandLineSwitch("opt3")]
            public int Option3 { get; set; }

            [CommandLineSwitch("opt5", DefaultValue = 27)]
            public int Option5 { get; set; }
            
            public CommandLine(string commandLine)
                : base(commandLine)
            {
            }
        }

        [TestMethod]
        public void Then_options_should_be_set_if_provided()
        {
            var commandLine = new CommandLine("exe -opt1:12 -opt4 -opt2:\"13\"");

            Assert.AreEqual(12, commandLine.Option1);
            Assert.AreEqual(13, commandLine.Option2);
        }

        [TestMethod]
        public void Then_option_should_not_be_set_if_not_provided()
        {
            var commandLine = new CommandLine("exe -opt1:12 -opt4 -opt2:\"13\"");
 
            Assert.AreEqual(-1, commandLine.Option3);
        }

        [TestMethod]
        public void Then_option_should_be_set_to_default_if_not_provided()
        {
            var commandLine = new CommandLine("exe -opt1:12 -opt4 -opt2:\"13\"");

            Assert.AreEqual(27, commandLine.Option5);
        }
    }
}