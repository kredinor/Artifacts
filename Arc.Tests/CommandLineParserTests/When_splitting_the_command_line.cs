namespace Arc.Tests.CommandLineParserTests
{
    using System.Linq;
    using Arc;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class When_splitting_the_command_line
    {
        private const string TheCommandLine = @"exe opt1  2 -opt3 -opt4:""c:\temp 1\sub folder"" opt5:""grr argh""";
        
        [TestMethod]
        public void Then_the_exe_name_should_not_be_included()
        {
            var parts = CommandLineParser.SplitCommandLine(TheCommandLine);

            Assert.IsTrue(parts.All(part => part != "exe"));
        }

        [TestMethod]
        public void Then_all_params_be_should_included()
        {
            var parts = CommandLineParser.SplitCommandLine(TheCommandLine);

            Assert.IsTrue(parts.Any(part => part == "opt1"), "opt1 not found");
            Assert.IsTrue(parts.Any(part => part == "2"), "2 not found");
            Assert.IsTrue(parts.Any(part => part == "-opt3"), "-opt3 not found");
            Assert.IsTrue(parts.Any(part => part == @"-opt4:""c:\temp 1\sub folder"""), @"-opt4:""c:\temp 1\sub folder"" not found");
            Assert.IsTrue(parts.Any(part => part == @"opt5:""grr argh"""), @"-opt5:""grr argh"" not found");
        }

        [TestMethod]
        public void Then_an_empty_string_should_produce_an_empty_array()
        {
            var parts = CommandLineParser.SplitCommandLine(string.Empty);

            Assert.AreEqual(0, parts.Length);            
        }

        [TestMethod]
        public void Then_no_arguments_should_produce_an_empty_array()
        {
            var parts = CommandLineParser.SplitCommandLine("exe");

            Assert.AreEqual(0, parts.Length);
        }
    }
}