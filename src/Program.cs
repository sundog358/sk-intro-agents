using Microsoft.SemanticKernel; // Core Semantic Kernel functionality
using Microsoft.SemanticKernel.ChatCompletion; // For chat completion services
using Microsoft.SemanticKernel.Connectors.OpenAI; // For OpenAI service connectors
using SKDemo; // Namespace for plugins (e.g., NewsPlugin)
using Microsoft.Extensions.Configuration; // For configuration management

class Program
{
    static async Task Main(string[] args)
    {
        // Load configuration from appsettings.json
        IConfiguration config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory()) // Set the base directory
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true) // Load appsettings.json
            .Build();

        // Retrieve configuration values from the "AzureOpenAI" section
        string deploymentName = config["AzureOpenAI:DeploymentName"] ?? throw new ArgumentNullException("DeploymentName", "⚠️ DeploymentName is missing in appsettings.json.");
        string endpoint = config["AzureOpenAI:Endpoint"] ?? throw new ArgumentNullException("Endpoint", "⚠️ Endpoint is missing in appsettings.json.");
        string apiKey = config["AzureOpenAI:ApiKey"] ?? throw new ArgumentNullException("ApiKey", "⚠️ ApiKey is missing in appsettings.json.");

        // Create the kernel builder
        var builder = Kernel.CreateBuilder();

        // Add Azure OpenAI service to the builder
        builder.AddAzureOpenAIChatCompletion(deploymentName, endpoint, apiKey);

        // Add plugins (e.g., NewsPlugin)
        builder.Plugins.AddFromType<NewsPlugin>();

        // Build the kernel
        Kernel kernel = builder.Build();

        // Retrieve the chat completion service
        var chatService = kernel.GetRequiredService<IChatCompletionService>();

        // Initialize chat history for maintaining context
        ChatHistory chatMessages = new ChatHistory();

        // Main interaction loop
        while (true)
        {
            // Prompt the user for input
            Console.Write("Prompt: ");
            string? userInput = Console.ReadLine();

            // Validate user input
            if (string.IsNullOrWhiteSpace(userInput))
            {
                Console.WriteLine("⚠️ Please provide a valid input.");
                continue; // Skip iteration if input is invalid
            }

            // Add user input to the chat history
            chatMessages.AddUserMessage(userInput);

            // Retrieve streaming chat message contents asynchronously
            var completion = chatService.GetStreamingChatMessageContentsAsync(
                chatMessages,
                executionSettings: new OpenAIPromptExecutionSettings
                {
                    ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions // Automatically invoke kernel functions
                },
                kernel: kernel
            );

            string fullMessage = ""; // To store the assistant's full response

            // Stream and display the assistant's response
            await foreach (var content in completion)
            {
                Console.Write(content.Content); // Print the response content
                fullMessage += content.Content; // Accumulate the full response
            }

            // Add the assistant's response to the chat history
            chatMessages.AddAssistantMessage(fullMessage);
            Console.WriteLine(); // Add a newline for better readability
        }
    }
}
