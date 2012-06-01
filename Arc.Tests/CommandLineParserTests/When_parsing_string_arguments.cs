namespace Arc.Tests.CommandLineParserTests
{
    using Arc;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class When_parsing_string_arguments
    {
        public class CommandLine : CommandLineParser
        {
            [CommandLineArgument("opt1", 1, HelpText = "This is option1")]
            public string Option1 { get; set; }

            [CommandLineArgument("opt2", 2)]
            public string Option2 { get; set; }

            [CommandLineArgument("opt3", 3)]
            public string Option3 { get; set; }

            [CommandLineArgument("opt5", 4, DefaultValue = "blabla")]
            public string Option5 { get; set; }

            public CommandLine(string commandLine)
                : base(commandLine)
            {
            }
        }

        [TestMethod]
        public void Then_arguments_should_be_set_if_provided()
        {
            var commandLine = new CommandLine("exe value1 \"value2\"");

            Assert.AreEqual("value1", commandLine.Option1);
            Assert.AreEqual("value2", commandLine.Option2);
        }

        [TestMethod]
        public void Then_option_should_not_be_set_if_not_provided()
        {
            var commandLine = new CommandLine("exe value1 \"value2\"");

            Assert.AreEqual(string.Empty, commandLine.Option3);
        }

        [TestMethod]
        public void Then_option_should_be_set_to_default_if_not_provided()
        {
            var commandLine = new CommandLine("exe value1 \"value2\"");

            Assert.AreEqual("blabla", commandLine.Option5);
        }

        [TestMethod]
        public void Then_UnmatchedParameters_should_contain_the_unmatched_arguments_only()
        {
            var commandLine = new CommandLine("exe value1 \"value2\" value3 value4 value5 value6");

            Assert.IsFalse(commandLine.UnmatchedParameters.Contains("value1"));
            Assert.IsFalse(commandLine.UnmatchedParameters.Contains("value2"));
            Assert.IsFalse(commandLine.UnmatchedParameters.Contains("value3"));
            Assert.IsFalse(commandLine.UnmatchedParameters.Contains("value4"));
            Assert.IsTrue(commandLine.UnmatchedParameters.Contains("value5"));
            Assert.IsTrue(commandLine.UnmatchedParameters.Contains("value6"));
        }

        [TestMethod]
        public void Then_UnmatchedArguments_should_contain_the_unmatched_argumetns_only()
        {
            var commandLine = new CommandLine("exe value1 \"value2\"");

            Assert.IsFalse(commandLine.UnmatchedArguments.Contains("opt1"));
            Assert.IsFalse(commandLine.UnmatchedArguments.Contains("opt2"));
            Assert.IsTrue(commandLine.UnmatchedArguments.Contains("opt3"));
            Assert.IsTrue(commandLine.UnmatchedArguments.Contains("opt5"));
        }
    }
}