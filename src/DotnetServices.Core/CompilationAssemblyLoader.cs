namespace DotnetServices
{
    using System;
    using System.IO;
    using System.Reflection;
    using Buildalyzer;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Loads compilation assemblies
    /// </summary>
    public class CompilationAssemblyLoader : IAssemblyLoader
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger _logger;
        private readonly string _framework;

        /// <summary>
        /// Initialises a new instance of <see cref="CompilationAssemblyLoader"/>
        /// </summary>
        /// <param name="loggerFactory">The logger factory</param>
        public CompilationAssemblyLoader(ILoggerFactory loggerFactory, string framework = null)
        {
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _logger = _loggerFactory.CreateLogger<CompilationAssemblyLoader>();
            _framework = framework;
        }

        /// <inheritdoc />
        public Assembly LoadAssembly(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                throw new ArgumentException("Project not provided", nameof(input));
            }

            if (!File.Exists(input))
            {
                throw new ArgumentException("Project does not exist", nameof(input));
            }

            var manager = new AnalyzerManager(new AnalyzerManagerOptions
            {
                LoggerFactory = _loggerFactory
            });
            var project = manager.GetProject(input);

            var analyzeResult = string.IsNullOrEmpty(_framework)
                ? project.Build() : project.Build(_framework);
            var buildResult = analyzeResult.BuildResult;

            if (!analyzeResult.OverallSuccess)
            {
                throw new InvalidOperationException("Unable to compile project");
            }

            var output = buildResult.ResultsByTarget["Build"].Items[0].ItemSpec;

            // Load the assembly using the load context
            return new LoadContextAssemblyLoader(_loggerFactory).LoadAssembly(output);
        }
    }
}
