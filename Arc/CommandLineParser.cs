namespace Arc
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class CommandLineSwitchAttribute : Attribute
    {
        private readonly string _name;

        public string Name { get { return _name; } }
        public string HelpText { get; set; }
        public object DefaultValue { get; set; }

        public CommandLineSwitchAttribute(string name)
        {
            _name = name;
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class CommandLineArgumentAttribute : Attribute
    {
        private readonly string _name;
        private readonly int _position;

        public string Name { get { return _name; } }
        public string HelpText { get; set; }
        public object DefaultValue { get; set; }
        public int Position { get { return _position; } }

        public CommandLineArgumentAttribute(string name, int position)
        {
            _name = name;
            _position = position;
        }
    }

    public class CommandLineParser
    {
        internal struct SwitchInfo
        {
            public string Name;
            public PropertyInfo PropertyInfo;
            public object DefaultValue;
            public string HelpText;
        }

        internal struct ArgumentInfo
        {
            public string Name;
            public PropertyInfo PropertyInfo;
            public object DefaultValue;
            public string HelpText;
            public int Position;
        }

        #region Switch handlers

        private abstract class SwitchHandler
        {
            protected readonly string SwitchName;
            protected readonly PropertyInfo SwitchProperty;
            protected readonly object DefaultValue;

            protected SwitchHandler(SwitchInfo switchInfo)
            {
                SwitchName = switchInfo.Name;
                SwitchProperty = switchInfo.PropertyInfo;
                DefaultValue = switchInfo.DefaultValue;
            }

            public static SwitchHandler GetSwitchHandler(SwitchInfo switchInfo)
            {
                var switchType = switchInfo.PropertyInfo.PropertyType;

                if (switchType == typeof(bool))
                {
                    return new ToggleSwitchHandler(switchInfo);
                }

                if (switchType == typeof(int))
                {
                    return new IntSwitchHandler(switchInfo);
                }

                if (switchType == typeof(string))
                {
                    return new ValueSwitchHandler(switchInfo);
                }

                if (switchType.IsSubclassOf(typeof(Enum)))
                {
                    return new EnumSwitchHandler(switchInfo);

                }
                throw new InvalidOperationException(string.Format("Unhandled type for switch {0}", switchInfo.Name));
            }

            public abstract bool Match(string argument);
            public abstract void SetValue(object target, string argument);
            public void SetDefaultValue(object target)
            {
                SwitchProperty.SetValue(target, GetDefaultValue(), null);
            }

            protected abstract object GetDefaultValue();
        }

        private class ToggleSwitchHandler : SwitchHandler
        {
            public ToggleSwitchHandler(SwitchInfo switchInfo)
                : base(switchInfo)
            {
            }

            public override bool Match(string argument)
            {
                return argument.ToLower() == string.Format("-{0}", SwitchName.ToLower());
            }

            public override void SetValue(object target, string argument)
            {
                SwitchProperty.SetValue(target, true, null);
            }

            protected override object GetDefaultValue()
            {
                return DefaultValue != null && DefaultValue is bool ? DefaultValue : false;
            }
        }

        private class ValueSwitchHandler : SwitchHandler
        {
            private readonly string _pattern;

            public ValueSwitchHandler(SwitchInfo switchInfo)
                : base(switchInfo)
            {
                _pattern = string.Format(@"^-{0}:((""(.+)"")|([^""]\S*))", switchInfo.Name);
            }

            public override bool Match(string argument)
            {
                var match = Regex.Match(argument, _pattern, RegexOptions.IgnoreCase);

                return match.Success;
            }

            public override void SetValue(object target, string argument)
            {
                var match = Regex.Match(argument, _pattern, RegexOptions.IgnoreCase);

                if (match.Groups.Count == 5)
                {
                    var value = match.Groups[3].Captures.Count == 1 ? match.Groups[3].Captures[0].Value : match.Groups[4].Captures[0].Value;

                    SwitchProperty.SetValue(target, ConvertValue(value), null);
                }
            }

            protected virtual object ConvertValue(string value)
            {
                return value;
            }

            protected override object GetDefaultValue()
            {
                return DefaultValue != null ? DefaultValue.ToString() : string.Empty;
            }
        }

        private class IntSwitchHandler : ValueSwitchHandler
        {
            public IntSwitchHandler(SwitchInfo switchInfo)
                : base(switchInfo)
            {
            }

            protected override object ConvertValue(string value)
            {
                return int.Parse(value);
            }

            protected override object GetDefaultValue()
            {
                return DefaultValue != null && DefaultValue is int ? DefaultValue : -1;
            }
        }

        private class EnumSwitchHandler : ValueSwitchHandler
        {
            public EnumSwitchHandler(SwitchInfo switchInfo)
                : base(switchInfo)
            {
            }

            protected override object ConvertValue(string value)
            {
                return Enum.Parse(SwitchProperty.PropertyType, value, true);
            }

            protected override object GetDefaultValue()
            {
                return DefaultValue != null && DefaultValue.GetType() == SwitchProperty.PropertyType ? DefaultValue : Enum.ToObject(SwitchProperty.PropertyType, 0);
            }
        }

        #endregion

        #region Argument handlers

        private abstract class ArgumentHandler
        {
            protected readonly string ArgumentName;
            protected readonly PropertyInfo ArgumentProperty;
            protected readonly object DefaultValue;

            protected ArgumentHandler(ArgumentInfo argumentInfo)
            {
                ArgumentName = argumentInfo.Name;
                ArgumentProperty = argumentInfo.PropertyInfo;
                DefaultValue = argumentInfo.DefaultValue;
            }

            public static ArgumentHandler GetArgumentHandler(ArgumentInfo argumentInfo)
            {
                var argumentType = argumentInfo.PropertyInfo.PropertyType;

                if (argumentType == typeof(int))
                {
                    return new IntArgumentHandler(argumentInfo);
                }

                if (argumentType == typeof(string))
                {
                    return new ValueArgumentHandler(argumentInfo);
                }

                if (argumentType.IsSubclassOf(typeof(Enum)))
                {
                    return new EnumArgumentHandler(argumentInfo);

                }
                throw new InvalidOperationException(string.Format("Unhandled type for argument {0}", argumentInfo.Name));
            }

            public abstract void SetValue(object target, string argument);
            public void SetDefaultValue(object target)
            {
                ArgumentProperty.SetValue(target, GetDefaultValue(), null);
            }

            protected abstract object GetDefaultValue();
        }

        private class ValueArgumentHandler : ArgumentHandler
        {
            public ValueArgumentHandler(ArgumentInfo argumentInfo)
                : base(argumentInfo)
            {
            }

            public override void SetValue(object target, string argument)
            {
                var match = Regex.Match(argument, @"^((""(.+)"")|([^""]\S*))", RegexOptions.IgnoreCase);

                if (match.Groups.Count == 5)
                {
                    var value = match.Groups[3].Captures.Count == 1 ? match.Groups[3].Captures[0].Value : match.Groups[4].Captures[0].Value;

                    ArgumentProperty.SetValue(target, ConvertValue(value), null);
                }
            }

            protected virtual object ConvertValue(string value)
            {
                return value;
            }

            protected override object GetDefaultValue()
            {
                return DefaultValue != null ? DefaultValue.ToString() : string.Empty;
            }
        }

        private class IntArgumentHandler : ValueArgumentHandler
        {
            public IntArgumentHandler(ArgumentInfo argumentInfo)
                : base(argumentInfo)
            {
            }

            protected override object ConvertValue(string value)
            {
                return int.Parse(value);
            }

            protected override object GetDefaultValue()
            {
                return DefaultValue != null && DefaultValue is int ? DefaultValue : -1;
            }
        }

        private class EnumArgumentHandler : ValueArgumentHandler
        {
            public EnumArgumentHandler(ArgumentInfo argumentInfo)
                : base(argumentInfo)
            {
            }

            protected override object ConvertValue(string value)
            {
                try
                {
                    return Enum.Parse(ArgumentProperty.PropertyType, value, true);
                }
                catch (ArgumentException)
                {
                    throw new ArgumentException(string.Format("Invalid value for '{0}': {1}", ArgumentName, value));
                }
            }

            protected override object GetDefaultValue()
            {
                return DefaultValue != null && DefaultValue.GetType() == ArgumentProperty.PropertyType ? DefaultValue : Enum.ToObject(ArgumentProperty.PropertyType, 0);
            }
        }

        #endregion

        private readonly string _commandLine;
        private readonly string[] _parameters;
        private readonly List<string> _unmatchedParameters;
        private readonly List<string> _unmatchedSwitches;
        private readonly List<string> _unmatchedArguments;

        public CommandLineParser(string commandLine)
        {
            _commandLine = commandLine;
            _parameters = SplitCommandLine(_commandLine);
            _unmatchedSwitches = new List<string>();
            _unmatchedArguments = new List<string>();

            var switchInfos = GetSwitchInfos(GetType());

            var unmatchedParameters = HandleSwitches(switchInfos, new List<string>(_parameters));

            var argumentInfos = GetArgumentInfos(GetType());

            unmatchedParameters = HandleArguments(argumentInfos, unmatchedParameters);

            _unmatchedParameters = unmatchedParameters;
        }

        private List<string> HandleArguments(IEnumerable<ArgumentInfo> argumentInfos, List<string> parameters)
        {
            foreach (var argumentInfo in argumentInfos)
            {
                var argumentHandler = ArgumentHandler.GetArgumentHandler(argumentInfo);

                if (parameters.Count > 0)
                {
                    var parameter = parameters.First();

                    argumentHandler.SetValue(this, parameter);

                    parameters.RemoveAt(0);
                }
                else
                {
                    argumentHandler.SetDefaultValue(this);

                    UnmatchedArguments.Add(argumentInfo.Name);
                }
            }
            return parameters;
        }

        private List<string> HandleSwitches(Dictionary<string, SwitchInfo> switchInfos, List<string> parameters)
        {
            foreach (var switchName in switchInfos.Keys)
            {
                var switchInfo = switchInfos[switchName];

                var switchHandler = SwitchHandler.GetSwitchHandler(switchInfo);

                var index = parameters.FindIndex(switchHandler.Match);

                if (index >= 0)
                {
                    switchHandler.SetValue(this, parameters[index]);

                    parameters.RemoveAt(index);
                }
                else
                {
                    switchHandler.SetDefaultValue(this);

                    UnmatchedSwitches.Add(switchName);
                }
            }
            return parameters;
        }

        internal static Dictionary<string, SwitchInfo> GetSwitchInfos(Type parserType)
        {
            var switches = new Dictionary<string, SwitchInfo>();

            var properties = parserType.GetProperties();

            foreach (var property in properties)
            {
                var attributes = property.GetCustomAttributes(true);

                var switchAttribute = (CommandLineSwitchAttribute)attributes.FirstOrDefault(attribute => attribute is CommandLineSwitchAttribute);

                if (switchAttribute != null)
                {
                    switches.Add(switchAttribute.Name, new SwitchInfo { DefaultValue = switchAttribute.DefaultValue, HelpText = switchAttribute.HelpText, Name = switchAttribute.Name, PropertyInfo = property });
                }
            }

            return switches;
        }

        internal static List<ArgumentInfo> GetArgumentInfos(Type parserType)
        {
            var arguments = new List<ArgumentInfo>();

            var properties = parserType.GetProperties();

            foreach (var property in properties)
            {
                var attributes = property.GetCustomAttributes(true);

                var argumentAttribute = (CommandLineArgumentAttribute)attributes.FirstOrDefault(attribute => attribute is CommandLineArgumentAttribute);

                if (argumentAttribute != null)
                {
                    arguments.Add(new ArgumentInfo { DefaultValue = argumentAttribute.DefaultValue, HelpText = argumentAttribute.HelpText, Name = argumentAttribute.Name, PropertyInfo = property, Position = argumentAttribute.Position});
                }
            }

            return arguments.OrderBy(argumentInfo => argumentInfo.Position).ToList();
        }

        internal static string[] SplitCommandLine(string commandLine)
        {
            var match = Regex.Match(commandLine, @"(\S+)(\s+([^""\s]*(""[^""]*"")?))+");

            if (match.Groups.Count != 5) return new string[0];

            var captures = match.Groups[3].Captures.Cast<Capture>().ToList();

            return captures.ConvertAll(capture => capture.Value).ToArray();
        }

        public string CommandLine
        {
            get { return _commandLine; }
        }

        public string[] Parameters
        {
            get { return _parameters; }
        }

        public List<string> UnmatchedParameters
        {
            get { return _unmatchedParameters; }
        }

        public List<string> UnmatchedSwitches
        {
            get { return _unmatchedSwitches; }
        }

        public List<string> UnmatchedArguments
        {
            get { return _unmatchedArguments; }
        }
    }
}