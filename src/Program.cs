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

        Console.WriteLine("Welcome to the AI News Assistant! Type 'exit' to quit.");
        Console.WriteLine("Available commands:");
        Console.WriteLine("- news <category>: Get latest news for a category (e.g., 'news Technology')");
        Console.WriteLine("- get <category> news: Alternative way to get news");
        Console.WriteLine("- save: Save the last fetched news to archive");
        Console.WriteLine("- chat: Have a conversation about the news");
        Console.WriteLine("- help: Show this help message");
        Console.WriteLine();

        // Store last fetched news content
        string? lastFetchedNews = null;
        string? lastFetchedCategory = null;

        while (true)
        {
            Console.Write("You: ");
            var input = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(input) || input.ToLower() == "exit")
                break;

            try
            {
                if (input.ToLower() == "help")
                {
                    Console.WriteLine("Available commands:");
                    Console.WriteLine("- news <category>: Get latest news for a category");
                    Console.WriteLine("- get <category> news: Alternative way to get news");
                    Console.WriteLine("- save: Save the last fetched news to archive");
                    Console.WriteLine("- chat: Have a conversation about the news");
                    Console.WriteLine("- help: Show this help message");
                    Console.WriteLine("- exit: Quit the application");
                    continue;
                }

                if (input.ToLower() == "save")
                {
                    if (lastFetchedNews == null || lastFetchedCategory == null)
                    {
                        Console.WriteLine("\n❌ No news content to save. Please fetch news first.");
                        continue;
                    }

                    string fileName = $"{lastFetchedCategory.ToLower()}_news_{DateTime.UtcNow:yyyy-MM-dd}";
                    var archiveResult = await archivePlugin.ArchiveContentAsync(lastFetchedNews, fileName);
                    Console.WriteLine(archiveResult);
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

                    Console.WriteLine($"\nFetching {category} news...");
                    var newsContent = await newsPlugin.GetNews(kernel, category);
                    
                    // Store the fetched news for potential saving later
                    lastFetchedNews = newsContent;
                    lastFetchedCategory = category;

                    // Parse and display news in a readable format
                    using var jsonDoc = JsonDocument.Parse(newsContent);
                    var articles = jsonDoc.RootElement.GetProperty("articles");
                    Console.WriteLine($"\nLatest {category} News:");
                    foreach (var article in articles.EnumerateArray())
                    {
                        Console.WriteLine($"\n📰 {article.GetProperty("title").GetString()}");
                        Console.WriteLine($"🔗 {article.GetProperty("link").GetString()}");
                        Console.WriteLine($"📅 {article.GetProperty("publishDate").GetDateTime():g}");
                    }
                    Console.WriteLine("\nType 'save' to archive these news articles.");
                    continue;
                }

                // Handle chat about the news
                chatHistory.AddUserMessage(input);
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
                    Console.WriteLine($"\nAssistant: {chatResponse.Content}");
                }
                else
                {
                    Console.WriteLine("\n❌ Error: No response received from the AI.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ Error: {ex.Message}");
            }

            Console.WriteLine(); // Add a blank line for readability
        }

        Console.WriteLine("\nGoodbye! 👋");
    }
}
