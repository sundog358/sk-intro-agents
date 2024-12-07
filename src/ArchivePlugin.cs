using Microsoft.SemanticKernel;
using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Text;
using System.Text.Json;

namespace SKDemo
{
    /// <summary>
    /// ArchivePlugin saves structured JSON content to a file.
    /// </summary>
    public class ArchivePlugin
    {
        [KernelFunction("archive_content")]
        [Description("Archives content to a file.")]
        public async Task<string> ArchiveContentAsync(
            [Description("The content to archive")] string content,
            [Description("The filename to use")] string fileName)
        {
            if (string.IsNullOrEmpty(content))
            {
                return "❌ Error: Content is empty";
            }

            try
            {
                // Ensure absolute path to archives directory
                var baseDir = Path.Combine(Directory.GetCurrentDirectory(), "archives");
                Directory.CreateDirectory(baseDir);
                
                var filePath = Path.Combine(baseDir, $"{fileName}.json");
                
                // Ensure the content is valid JSON before saving
                using (JsonDocument.Parse(content)) { }
                
                // Write file with UTF8 encoding without BOM
                await File.WriteAllTextAsync(
                    filePath, 
                    content, 
                    new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)
                );
                
                // Verify file was created
                if (File.Exists(filePath))
                {
                    return $"✅ Content archived successfully to {filePath}";
                }
                else
                {
                    return "❌ Error: File was not created successfully";
                }
            }
            catch (JsonException ex)
            {
                return $"❌ Error: Invalid JSON content - {ex.Message}";
            }
            catch (Exception ex)
            {
                return $"❌ Error archiving content: {ex.Message}";
            }
        }
    }
}
