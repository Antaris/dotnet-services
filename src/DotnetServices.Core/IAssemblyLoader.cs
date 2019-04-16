namespace DotnetServices
{
    using System.Reflection;

    /// <summary>
    /// Defines the required contract for implementing an assembly loader
    /// </summary>
    public interface IAssemblyLoader
    {
        /// <summary>
        /// Loads the assembly for the given input
        /// </summary>
        /// <param name="input">The input file</param>
        /// <returns>The loaded assembly</returns>
        Assembly LoadAssembly(string input);
    }
}
