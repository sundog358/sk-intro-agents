using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text;

namespace SKDemo.Helpers
{
    public static class JsonOutputHelper
    {
        private static readonly JsonSerializerOptions DefaultOptions = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(System.Text.Unicode.UnicodeRanges.All),
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            PropertyNameCaseInsensitive = true,
            NumberHandling = JsonNumberHandling.Strict,
            ReferenceHandler = ReferenceHandler.IgnoreCycles
        };

        public static async Task SaveJsonToFile(string json, string category, string baseDirectory = "archives")
        {
            try
            {
                // First decode any HTML entities in the JSON string
                json = System.Web.HttpUtility.HtmlDecode(json);
                
                // Parse the JSON to ensure it's valid
                using var jsonDoc = JsonDocument.Parse(json);
                
                // Format with proper indentation
                var formattedJson = JsonSerializer.Serialize(jsonDoc.RootElement, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(System.Text.Unicode.UnicodeRanges.All),
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                // Ensure directory exists
                var outputDir = Path.Combine(Directory.GetCurrentDirectory(), baseDirectory);
                Directory.CreateDirectory(outputDir);

                // Create filename
                var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd");
                var filename = Path.Combine(outputDir, $"{category.ToLower()}_news_{timestamp}.json");

                // Write the file with UTF-8 encoding (no BOM)
                await File.WriteAllTextAsync(
                    filename, 
                    formattedJson, 
                    new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving JSON output: {ex.Message}");
                throw;
            }
        }

        public static async Task<T?> LoadJsonFromFile<T>(string filepath)
        {
            if (!File.Exists(filepath))
                return default;

            var json = await File.ReadAllTextAsync(filepath);
            return JsonSerializer.Deserialize<T>(json, DefaultOptions);
        }
    }
} 