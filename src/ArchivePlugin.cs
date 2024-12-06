// File Name: ArchivePlugin.cs
// Description: A Semantic Kernel plugin to save user-provided content to a file.
// Purpose: Archives user-provided content by writing it to a text file in a designated folder.

using Microsoft.SemanticKernel;  // For KernelFunction and Semantic Kernel integration
using System;                    // For basic system utilities
using System.ComponentModel;     // For metadata annotations
using System.IO;                 // For file I/O operations
using System.Threading.Tasks;    // For async programming

namespace SKDemo // Namespace for Semantic Kernel demo plugins
{
    /// <summary>
    /// ArchivePlugin saves user-provided content to a text file.
    /// </summary>
    public class ArchivePlugin
    {
        private readonly string _archiveDirectory;

        public ArchivePlugin()
        {
            _archiveDirectory = Path.Combine(Directory.GetCurrentDirectory(), "archives");
            Directory.CreateDirectory(_archiveDirectory); // Ensure directory exists
        }

        /// <summary>
        /// Saves the provided content to a file.
        /// </summary>
        /// <param name="content">The content to save.</param>
        /// <param name="fileName">Optional file name for the saved content.</param>
        /// <returns>A message indicating the result of the operation.</returns>
        [KernelFunction("archive_data")]
        [Description("Saves content to a file in the archive folder.")]
        public async Task<string> ArchiveContentAsync(
            [Description("The content to save.")] string content,
            [Description("Optional filename (without extension).")] string? fileName = null)
        {
            try
            {
                fileName ??= $"archive_{DateTime.Now:yyyyMMdd_HHmmss}";
                fileName = string.Join("_", fileName.Split(Path.GetInvalidFileNameChars()));
                string filePath = Path.Combine(_archiveDirectory, $"{fileName}.txt");

                await File.WriteAllTextAsync(filePath, content);
                return $"✅ Content successfully archived to: {filePath}";
            }
            catch (Exception ex)
            {
                return $"❌ Error saving content: {ex.Message}";
            }
        }
    }
}
