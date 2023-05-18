using System;
using System.Reflection;

/// <summary>
/// Attribute to denote the project root directory.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly)]
internal class ProjectRootDirectoryAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectRootDirectoryAttribute" /> class.
    /// </summary>
    /// <param name="projectRootDirectory">The directory of the project root.</param>
    public ProjectRootDirectoryAttribute(string projectRootDirectory)
    {
        ProjectRootDirectory = projectRootDirectory;
    }

    /// <summary>
    /// Gets the project root directory for the executing assembly.
    /// </summary>
    public static string ThisAssemblyProjectRootDirectory
    {
        get
        {
            var thisAssembly = Assembly.GetExecutingAssembly();
            var projectRootDirectoryAttribute = thisAssembly.GetCustomAttribute<ProjectRootDirectoryAttribute>();
            return projectRootDirectoryAttribute!.ProjectRootDirectory;
        }
    }

    /// <summary>
    /// Gets the project root directory.
    /// </summary>
    private string ProjectRootDirectory { get; }
}
