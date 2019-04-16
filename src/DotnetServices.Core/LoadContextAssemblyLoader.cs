namespace DotnetServices
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Runtime.Loader;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Loads assemblies using a load context
    /// </summary>
    public class LoadContextAssemblyLoader : IAssemblyLoader
    {
        private ILogger _logger;

        /// <summary>
        /// Initialises a new instance of <see cref="LoadContextAssemblyLoader"/>
        /// </summary>
        /// <param name="loggerFactory">The logger factory</param>
        public LoadContextAssemblyLoader(ILoggerFactory loggerFactory)
        {
            loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _logger = loggerFactory.CreateLogger<LoadContextAssemblyLoader>();
        }

        /// <inheritdoc />
        public Assembly LoadAssembly(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                _logger.LogError("Project was not provided");
                return null;
            }

            if (!File.Exists(input))
            {
                _logger.LogError("Project does not exist");
                return null;
            }

            _logger.LogInformation($"Loading assembly {input}");

            return new LoadContext().Load(input);
        }

        private class LoadContext : AssemblyLoadContext
        {
            public Assembly Load(string assemblyFile)
            {
                AssemblyName an = AssemblyName.GetAssemblyName(assemblyFile);

                return Load(an);
            }

            protected override Assembly Load(AssemblyName assemblyName)
                => Default.LoadFromAssemblyName(assemblyName);
        }
    }
}
