namespace Arc
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;

    class Program
    {
        private const string RepositoryDriverLetter = "a";
        private const string SubstCommand = "subst";
        private const string TfCommand = "tf";

        public enum Command { Help, Publish, Promote, Subst, CheckIn, CheckOut, Label, Get } ;

        // ReSharper disable UnusedAutoPropertyAccessor.Local
        class CommandLine : CommandLineParser
        {
            [CommandLineArgument("command", 1, DefaultValue = Command.Help)]
            public Command Command { get; set; }

            [CommandLineSwitch("command", DefaultValue = Command.Help)]
            public Command CommandHelp { get; set; }

            [CommandLineSwitch("include")]
            public string IncludeFile { get; set; }

            [CommandLineSwitch("source")]
            public string SourceFolder { get; set; }

            [CommandLineSwitch("target")]
            public string TargetFolder { get; set; }

            [CommandLineSwitch("repository")]
            public string RepositoryFolder { get; set; }

            [CommandLineSwitch("tf", DefaultValue = TfCommand)]
            public string PathToTfCommand { get; set; }

            [CommandLineSwitch("comment")]
            public string Comment { get; set; }

            [CommandLineSwitch("label")]
            public string Label { get; set; }

            [CommandLineSwitch("drive", DefaultValue = RepositoryDriverLetter)]
            public string RepositoryDriveLetter { get; set; }

            public CommandLine(string commandLine)
                : base(commandLine)
            {
            }
        }
        // ReSharper restore UnusedAutoPropertyAccessor.Local

        static int Main()
        {
            var version = typeof(Program).Assembly.GetName().Version;

            Console.WriteLine(string.Format("Artifacts Repository Control tool (arc.exe v{0})\n", version));

            CommandLine commandLine;

            try
            {
                commandLine = new CommandLine(Environment.CommandLine);
            }
            catch (ArgumentException argumentException)
            {
                Console.WriteLine("Syntax error: {0}", argumentException.Message);

                return -1;
            }

            switch (commandLine.Command)
            {
                case Command.Help:

                    return Help(commandLine.CommandHelp);

                case Command.Publish:

                    if (string.IsNullOrEmpty(commandLine.IncludeFile)) return ShowError("Missing parameter: -include");
                    if (string.IsNullOrEmpty(commandLine.SourceFolder)) return ShowError("Missing parameter: -source");
                    if (string.IsNullOrEmpty(commandLine.TargetFolder)) return ShowError("Missing parameter: -target");

                    return Publish(commandLine.IncludeFile, commandLine.SourceFolder, commandLine.TargetFolder);

                case Command.Subst:

                    if (commandLine.RepositoryDriveLetter.Length != 1) return ShowError("Bad parameter: -drive (must be a single letter)");

                    return Subst(commandLine.RepositoryFolder, commandLine.RepositoryDriveLetter);

                case Command.CheckOut:

                    if (string.IsNullOrEmpty(commandLine.RepositoryFolder)) return ShowError("Missing parameter: -repository");

                    return CheckOut(commandLine.RepositoryFolder, commandLine.PathToTfCommand);

                case Command.CheckIn:

                    if (string.IsNullOrEmpty(commandLine.RepositoryFolder)) return ShowError("Missing parameter: -repository");

                    return CheckIn(commandLine.RepositoryFolder, commandLine.Comment, commandLine.PathToTfCommand);

                case Command.Label:

                    if (string.IsNullOrEmpty(commandLine.RepositoryFolder)) return ShowError("Missing parameter: -repository");
                    if (string.IsNullOrEmpty(commandLine.Label)) return ShowError("Missing parameter: -label");

                    return Label(commandLine.RepositoryFolder, commandLine.Label, commandLine.Comment, commandLine.PathToTfCommand);

                case Command.Promote:

                    if (string.IsNullOrEmpty(commandLine.RepositoryFolder)) return ShowError("Missing parameter: -repository");

                    return Promote(commandLine.RepositoryFolder, commandLine.Label, commandLine.Comment, commandLine.PathToTfCommand);

                case Command.Get:

                    if (string.IsNullOrEmpty(commandLine.RepositoryFolder)) return ShowError("Missing parameter: -repository");

                    return Get(commandLine.RepositoryFolder, commandLine.PathToTfCommand);
            }
            return -1;
        }

        private static int Help(Command command)
        {
            switch (command)
            {
                case Command.Get:
                    Console.WriteLine(@"get - gets the latest version of the repository from TFS.");
                    Console.WriteLine(@"");
                    Console.WriteLine(@"NOTE: This will overwrite any writeable files!");
                    Console.WriteLine(@"");
                    Console.WriteLine(@"Syntax: arc get -repository:<root> [-tf:<tf path>]");
                    Console.WriteLine(@"");
                    Console.WriteLine(@"  root - Artifacts repository root, e.g. c:\projects\artifacts");
                    Console.WriteLine(@"  tf   - Path to tf.exe command (default assumes tf.exe is in path)");
                    return 0;

            }
            Help();
            return 0;
        }

        private static void Help()
        {
            Console.WriteLine("Avalable commands:");
            Console.WriteLine(@"arc help [-command:<command>]");
            Console.WriteLine(@"arc publish -include:<artifacts file> -source:<folder> -target:<folder>");
            Console.WriteLine(@"arc subst [-repository:<root>] [-drive:<drive letter>]");
            Console.WriteLine(@"arc checkout -repository:<folder> [-tf:<tf path>]");
            Console.WriteLine(@"arc checkin -repository:<folder> [-comment:""""<checkin>""""] [-tf:<tf path>]");
            Console.WriteLine(@"arc label -repository:<folder> -label:""<label>"" [-comment:""""<checkin>""""] [-tf:<tf path>]");
            Console.WriteLine(@"arc promote -repository:<folder> [-label:""<label>""] [-comment:""""<checkin>""""] [-tf:<tf path>]");
            Console.WriteLine(@"arc get -repository:<root> [-tf:<tf path>]");
        }

        private static int ShowError(string message)
        {
            Console.WriteLine(message);

            return -1;
        }

        private static int Get(string repository, string pathToTfCommand)
        {
            try
            {
                Console.WriteLine("Getting latest from {0}...\n", repository);

                var itemSpec = Path.Combine(repository, "*");

                var result = ExecuteCommand(pathToTfCommand, string.Format("get /overwrite /recursive {0}", itemSpec));

                Console.WriteLine(result.Output);

                if (result.ExitCode != 0)
                {
                    Console.WriteLine("Get failed!");

                    return -1;
                }
                return 0;
            }
            catch (Exception exception)
            {
                Console.WriteLine("Get failed: {0}", exception.Message);

                return -1;
            }
        }

        private static int Promote(string repository, string label, string comment, string pathToTfCommand)
        {
            Console.WriteLine(string.Format("Promoting {0}", repository));

            if (CheckOut(repository, pathToTfCommand) != 0)
            {
                Console.WriteLine("Promote failed!");
                return -1;
            }
            if (CheckIn(repository, comment, pathToTfCommand) != 0)
            {
                Console.WriteLine("Promote failed!");
                return -1;
            }

            if (!string.IsNullOrEmpty(label))
            {
                if (Label(repository, label, comment, pathToTfCommand) != 0)
                {
                    Console.WriteLine("Promote failed!");
                    return -1;
                }
            }
            return 0;
        }

        private static int Label(string repository, string label, string comment, string pathToTfCommand)
        {
            try
            {
                Console.WriteLine("Labeling {0} => {1}...\n", repository, label);

                var itemSpec = Path.Combine(repository, "*");
                var commentParameter = !string.IsNullOrEmpty(comment) ? string.Format(@"/comment:""{0}""", comment) : string.Empty;

                var result = ExecuteCommand(pathToTfCommand, string.Format(@"label ""{0}"" {1} {2}", label, itemSpec, commentParameter));

                Console.WriteLine(result.Output);

                if (result.ExitCode != 0)
                {
                    Console.WriteLine("Label failed!");

                    return -1;
                }
                return 0;
            }
            catch (Exception exception)
            {
                Console.WriteLine("Label failed: {0}", exception.Message);

                return -1;
            }

        }

        private static int CheckOut(string repository, string pathToTfCommand)
        {
            try
            {
                Console.WriteLine("Checing out {0}...\n", repository);

                var itemSpec = Path.Combine(repository, "*");

                var result = ExecuteCommand(pathToTfCommand, string.Format("checkout /lock:checkout /recursive {0}", itemSpec));

                Console.WriteLine(result.Output);

                if (result.ExitCode != 0)
                {
                    Console.WriteLine("Check out failed!");

                    return -1;
                }
                return 0;
            }
            catch (Exception exception)
            {
                Console.WriteLine("Check out failed: {0}", exception.Message);

                return -1;
            }
        }

        private static int CheckIn(string repository, string comment, string pathToTfCommand)
        {
            try
            {
                Console.WriteLine("Checing in {0}...\n", repository);

                var itemSpec = Path.Combine(repository, "*");
                var commentParameter = !string.IsNullOrEmpty(comment) ? string.Format(@"/comment:""{0}""", comment) : string.Empty;

                var result = ExecuteCommand(pathToTfCommand, string.Format("checkin /noprompt /recursive {0} {1}", commentParameter, itemSpec));

                Console.WriteLine(result.Output);

                if (result.ExitCode != 0)
                {
                    Console.WriteLine("Check in failed!");

                    return -1;
                }
                return 0;
            }
            catch (Exception exception)
            {
                Console.WriteLine("Check in failed: {0}", exception.Message);

                return -1;
            }

        }

        private static int Subst(string repository, string driveLetter)
        {
            try
            {
                if (!string.IsNullOrEmpty(repository))
                {
                    if (!string.IsNullOrEmpty(GetCurrentSubst(driveLetter[0])))
                    {
                        DeleteSubst(driveLetter[0]);
                    }
                }
                AddSubst(repository, driveLetter[0]);

                Console.WriteLine(GetCurrentSubst(driveLetter[0]));

                return 0;
            }
            catch (Exception exception)
            {
                Console.WriteLine(string.Format("Subst failed: {0}", exception.Message));

                return -1;
            }
        }

        private static void AddSubst(string repository, char driverLetter)
        {
            var commandParameters = string.Format("{0}: {1}", driverLetter, repository);

            ExecuteCommand(SubstCommand, commandParameters);
        }

        private static void DeleteSubst(char driveLetter)
        {
            var commandParameters = string.Format("{0}: /d", driveLetter);

            ExecuteCommand(SubstCommand, commandParameters);
        }

        private static string GetCurrentSubst(char driverLetter)
        {
            var result = ExecuteCommand(SubstCommand);

            var stringReader = new StringReader(result.Output);

            while (true)
            {
                var line = stringReader.ReadLine();

                if (line == null) break;

                var pattern = string.Format("{0}:\\", driverLetter);

                if (line.StartsWith(pattern, StringComparison.InvariantCultureIgnoreCase))
                {
                    return line;
                }
            }

            return string.Format("{0}:\\: => (not mapped)", driverLetter);
        }

        private struct ExecuteCommandResult
        {
            public int ExitCode { get; private set; }
            public string Output { get; private set; }

            public ExecuteCommandResult(int exitCode, string output)
                : this()
            {
                ExitCode = exitCode;
                Output = output;
            }
        }

        private static ExecuteCommandResult ExecuteCommand(string command)
        {
            return ExecuteCommand(command, string.Empty);
        }

        private static ExecuteCommandResult ExecuteCommand(string command, string commandParameters)
        {
            try
            {
                var pinfo = new ProcessStartInfo(command, commandParameters)
                                {
                                    CreateNoWindow = true,
                                    RedirectStandardOutput = true,
                                    RedirectStandardError = true,
                                    UseShellExecute = false
                                };


                var process = Process.Start(pinfo);

                process.WaitForExit();


                var standardOutput = process.StandardOutput.ReadToEnd();
                var standardError = process.StandardError.ReadToEnd();

                var output = new List<string>();

                if (!string.IsNullOrEmpty(standardError)) output.Add(standardError);
                if (!string.IsNullOrEmpty(standardOutput)) output.Add(standardOutput);

                return new ExecuteCommandResult(process.ExitCode, string.Join("\n\n", output.ToArray()));
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException(string.Format("{0} {1}: ({2})", command, commandParameters, exception.Message));
            }
        }

        private static int Publish(string promotionFile, string sourceFolder, string targetFolder)
        {
            try
            {
                Console.WriteLine(string.Format("Publishing from {0} to {1} using {2}", sourceFolder, targetFolder, promotionFile));

                if (!File.Exists(promotionFile))
                {
                    Console.WriteLine(string.Format("Promotion file note found: {0}", promotionFile));
                    return -1;
                }
                if (!Directory.Exists(sourceFolder))
                {
                    Console.WriteLine(string.Format("Source folder note found: {0}", sourceFolder));
                    return -1;
                }
                if (!Directory.Exists(targetFolder))
                {
                    Directory.CreateDirectory(targetFolder);
                    Console.WriteLine(string.Format("Target folder created: {0}", targetFolder));
                }

                var fileCount = 0;

                using (var reader = new StreamReader(promotionFile))
                {
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();

                        if (line != null && (!line.StartsWith("#") && line.Trim() != string.Empty))
                        {
                            var source = Path.Combine(sourceFolder, line);
                            var target = Path.Combine(targetFolder, line);

                            if (File.Exists(target))
                            {
                                var attributes = File.GetAttributes(target);

                                if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                                {
                                    Console.WriteLine("-r {0}", line);
                                    File.SetAttributes(target, attributes ^ FileAttributes.ReadOnly);
                                }
                            }
                            Console.WriteLine("copy {0}", line);

                            File.Copy(source, target, true);
                            ++fileCount;
                        }
                    }
                }
                Console.WriteLine("{0} files published", fileCount);

                return 0;
            }
            catch (Exception exception)
            {
                Console.WriteLine(string.Format("Publishing failed: {0}", exception.Message));

                return -1;
            }
        }
    }
}
