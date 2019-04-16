namespace DotnetServices
{
    using System;
    using System.Collections.Immutable;
    using System.ComponentModel.DataAnnotations;
    using System.IO;
    using System.Linq;
    using McMaster.Extensions.CommandLineUtils;
    using Microsoft.Extensions.Logging;
    using Terminal.Gui;

    using static System.Console;

    public class Program
    {
        static void Main(string[] args)
            => CommandLineApplication.Execute<Program>(args);

        /// <summary>
        /// Gets or sets the input
        /// </summary>
        [Argument(0, Description = "The project, application, or library to analyse")]
        public string Input { get; set; }

        /// <summary>
        /// Gets or sets the logging level verbosity
        /// </summary>
        [Option("-v|--verbosity <LEVEL>", Description = "Sets the verbosity level for logging")]
        public LogLevel Verbosity { get; set; }

        /// <summary>
        /// Gets or sets the project input framework
        /// </summary>
        [Option("-f|--framework <FRAMEWORK>", Description = "Sets the target framework for compiling project input")]
        public string Framework { get; set; }

        /// <summary>
        /// Executes the application
        /// </summary>
        public void OnExecute()
        {
            var loggerFactory = new LoggerFactory()
                .AddConsole(Verbosity);

            IAssemblyLoader loader = null;
            if (string.Equals(".csproj", Path.GetExtension(Input), StringComparison.OrdinalIgnoreCase))
            {
                loader = new CompilationAssemblyLoader(loggerFactory, Framework);
            }
            else if (string.Equals(".exe", Path.GetExtension(Input), StringComparison.OrdinalIgnoreCase)
                || string.Equals(".dll", Path.GetExtension(Input), StringComparison.OrdinalIgnoreCase))
            {
                loader = new LoadContextAssemblyLoader(loggerFactory);
            }

            if (loader == null)
            {
                throw new InvalidOperationException($"The input {Input} is not supported");
            }

            var assembly = loader.LoadAssembly(Input);

            if (assembly == null)
            {
                throw new InvalidOperationException($"Unable to load assembly from {Input}");
            }

            Application.Init();

            var top = new CustomWindow();

            var left = new FrameView("Services")
            {
                Width = Dim.Percent(50),
                Height = Dim.Fill(1)
            };
            var right = new View()
            {
                X = Pos.Right(left),
                Width = Dim.Fill(),
                Height = Dim.Fill(1)
            };
            var helpText = new Label("Use arrow keys and Tab to move around, Esc to quit")
            {
                Y = Pos.AnchorEnd(1)
            };

            top.Add(left, right, helpText);
            Application.Top.Add(top);

            Application.Run();
        }

        /// <summary>
        /// Validates the input values
        /// </summary>
        /// <returns>The validation result</returns>
        public ValidationResult OnValidate()
        {
            if (File.Exists(Input))
            {
                return ValidationResult.Success;
            }

            if (!Directory.Exists(Input))
            {
                return new ValidationResult("Project path does not exist");
            }

            var projects = Directory.GetFiles(Input, "*.csproj", SearchOption.TopDirectoryOnly);

            if (!projects.Any())
            {
                return new ValidationResult("Unable to find any project files in the current directory");
            }

            if (projects.Length > 1)
            {
                return new ValidationResult("More than one project exists in the current directory");
            }

            Input = projects[0];
            return ValidationResult.Success;
        }

        private class CustomWindow : Window
        {
            public CustomWindow()
                : base("Services", 0)
            { }

            public override bool ProcessKey(KeyEvent keyEvent)
            {
                if (keyEvent.Key != Key.Esc)
                {
                    return base.ProcessKey(keyEvent);
                }

                Application.RequestStop();
                return true;
            }
        }
    }
}
