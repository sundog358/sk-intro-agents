﻿using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using SKDemo;
using SKDemo.Helpers;

class Program
{
    static async Task Main(string[] args)
    {
        IConfiguration config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        string deploymentName = config["AzureOpenAI:DeploymentName"] 
            ?? throw new ArgumentNullException("DeploymentName", "Missing deployment name in configuration");
        string endpoint = config["AzureOpenAI:Endpoint"] 
            ?? throw new ArgumentNullException("Endpoint", "Missing endpoint in configuration");
        string apiKey = config["AzureOpenAI:ApiKey"] 
            ?? throw new ArgumentNullException("ApiKey", "Missing API key in configuration");

        var builder = Kernel.CreateBuilder();
        builder.AddAzureOpenAIChatCompletion(deploymentName, endpoint, apiKey);
        builder.Plugins.AddFromType<NewsPlugin>();
        builder.Plugins.AddFromType<ArchivePlugin>();
        Kernel kernel = builder.Build();

        var chatHistory = new ChatHistory();
        var newsPlugin = new NewsPlugin();
        var archivePlugin = new ArchivePlugin();

        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔══════════════════════════════════════╗");
        Console.WriteLine("║      🤖 AI News Assistant v1.0       ║");
        Console.WriteLine("╚══════════════════════════════════════╝");
        Console.ResetColor();
        Console.WriteLine("\nAvailable commands:");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("1. news <category>    - Get latest news (e.g., 'news Technology')");
        Console.WriteLine("2. get <category> news- Alternative way to get news");
        Console.WriteLine("3. save              - Save last fetched news to archive");
        Console.WriteLine("4. chat              - Have a conversation about the news");
        Console.WriteLine("5. help              - Show this help message");
        Console.WriteLine("6. exit              - Quit the application");
        Console.WriteLine("7. clear             - Clear the console");
        Console.ResetColor();
        Console.WriteLine("\nSupported categories: Technology, Business, World, Science, Health");
        Console.WriteLine("──────────────────────────────────────────");

        // Store last fetched news content
        string? lastFetchedNews = null;
        string? lastFetchedCategory = null;

        while (true)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("\nYou: ");
            Console.ResetColor();
            var input = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(input))
                continue;

            if (input.ToLower() == "exit")
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("\nGoodbye! 👋");
                Console.ResetColor();
                break;
            }

            try
            {
                if (input.ToLower() == "clear")
                {
                    Console.Clear();
                    continue;
                }

                if (input.ToLower() == "help")
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("\nAvailable commands:");
                    Console.WriteLine("1. news <category>    - Get latest news (e.g., 'news Technology')");
                    Console.WriteLine("2. get <category> news- Alternative way to get news");
                    Console.WriteLine("3. save              - Save last fetched news to archive");
                    Console.WriteLine("4. chat              - Have a conversation about the news");
                    Console.WriteLine("5. help              - Show this help message");
                    Console.WriteLine("6. exit              - Quit the application");
                    Console.WriteLine("7. clear             - Clear the console");
                    Console.ResetColor();
                    Console.WriteLine("\nSupported categories: Technology, Business, World, Science, Health");
                    continue;
                }

                if (input.ToLower() == "save")
                {
                    if (lastFetchedNews == null || lastFetchedCategory == null)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("\n❌ No news content to save. Please fetch news first.");
                        Console.ResetColor();
                        continue;
                    }

                    string fileName = $"{lastFetchedCategory.ToLower()}_news_{DateTime.UtcNow:yyyy-MM-dd}";
                    var archiveResult = await archivePlugin.ArchiveContentAsync(lastFetchedNews, fileName);
                    
                    if (archiveResult.StartsWith("✅"))
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                    }
                    Console.WriteLine($"\n{archiveResult}");
                    Console.ResetColor();
                    continue;
                }

                // Check for both "news <category>" and "get <category> news" patterns
                bool isNewsCommand = input.ToLower().StartsWith("news ");
                bool isGetNewsCommand = input.ToLower().StartsWith("get ") && input.ToLower().EndsWith(" news");
                
                if (isNewsCommand || isGetNewsCommand)
                {
                    string category;
                    if (isNewsCommand)
                    {
                        category = input.Substring(5).Trim();
                    }
                    else
                    {
                        category = input.ToLower()
                            .Replace("get ", "")
                            .Replace(" news", "")
                            .Trim();
                    }

                    Console.WriteLine($"\n📡 Fetching {category} news...");
                    var newsContent = await newsPlugin.GetNews(kernel, category);
                    
                    lastFetchedNews = newsContent;
                    lastFetchedCategory = category;

                    using var jsonDoc = JsonDocument.Parse(newsContent);
                    var articles = jsonDoc.RootElement.GetProperty("articles");
                    
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"\n📰 Latest {category} News:");
                    Console.WriteLine("──────────────────────────────────────────");
                    Console.ResetColor();
                    
                    int articleCount = 1;
                    foreach (var article in articles.EnumerateArray())
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine($"\n{articleCount}. {article.GetProperty("title").GetString()}");
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine($"   🔗 {article.GetProperty("link").GetString()}");
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.WriteLine($"   📅 {article.GetProperty("publishDate").GetDateTime():g}");
                        Console.ResetColor();
                        articleCount++;
                    }
                    
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("\n💾 Type 'save' to archive these news articles.");
                    Console.ResetColor();
                    continue;
                }

                // Handle chat about the news
                chatHistory.AddUserMessage(input);
                Console.WriteLine("\n🤔 Thinking...");
                
                var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
                var chatResponse = await chatCompletionService.GetChatMessageContentAsync(
                    chatHistory: chatHistory,
                    executionSettings: new OpenAIPromptExecutionSettings 
                    { 
                        MaxTokens = 2000,
                        Temperature = 0.7
                    }
                );

                if (chatResponse?.Content is not null)
                {
                    chatHistory.AddAssistantMessage(chatResponse.Content);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"\n🤖 Assistant: {chatResponse.Content}");
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\n❌ Error: No response received from the AI.");
                    Console.ResetColor();
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n❌ Error: {ex.Message}");
                Console.ResetColor();
            }
        }
    }
}
