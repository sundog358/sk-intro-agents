# 🌟 Semantic Kernel Intro Agents 🌟

Welcome to the **Semantic Kernel Intro Agents** project! This application demonstrates how to build interactive, intelligent agents using **Semantic Kernel** and **Azure OpenAI**. 🎉 With plugins like `NewsPlugin` for fetching the latest news and `ArchivePlugin` for data storage, this project showcases extensible and powerful AI functionality for real-world applications. 🚀

---

## 🚀 Features

- 🤖 **Interactive AI Chat**: Engage with an AI agent in real-time, powered by Azure OpenAI's GPT models.
- 📰 **News Retrieval**: Stay informed with the latest news headlines using the `NewsPlugin`.
- 💾 **Data Archival**: Save conversation history or other data locally with the `ArchivePlugin`.
- 🔧 **Extensibility**: Designed for flexibility—easily add custom plugins or extend existing ones.
- 📜 **Real-Time Streaming Responses**: Enjoy smooth, streaming responses during chat interactions.

---

## 📋 Requirements

Before running the project, ensure you have the following:

- ✅ **.NET 8.0 SDK**: [Download here](https://dotnet.microsoft.com/download).
- ✅ **Azure OpenAI Credentials**:
  - **Deployment Name**: The name of your deployed Azure OpenAI model.
  - **Endpoint URL**: The endpoint for your Azure OpenAI service.
  - **API Key**: The authentication key for accessing Azure OpenAI.
- ✅ **Configuration**:
  - Populate the `appsettings.json` file with your Azure OpenAI credentials.

---

## 🛠️ How It Works

### **Program Overview**

1. **Configuration Loading**:

   - The application reads Azure OpenAI credentials (`DeploymentName`, `Endpoint`, `ApiKey`) from `appsettings.json` for secure configuration management.

2. **Semantic Kernel Setup**:

   - A **Kernel Builder** is initialized to configure the Semantic Kernel environment.
   - The Azure OpenAI service is integrated as a chat completion backend.

3. **Plugins**:

   - The application integrates **plugins** to expand functionality:
     - `NewsPlugin`: Retrieves news headlines from the New York Times RSS feeds.
     - `ArchivePlugin`: Saves runtime data to local storage for later use.

4. **Interactive Loop**:

   - Users input prompts, and the AI agent responds in real-time.
   - Chat history is maintained to ensure context-aware conversations.

5. **Streaming Responses**:
   - The AI responses are streamed for a smoother user experience, displaying the output as it is generated.

---

## 📂 Project Structure

```
C:\Users\rsung\OneDrive\Desktop\AIResearch\semantickernel\sk-intro-agents
├── .gitignore                       # Git ignore rules
├── README.md                        # Project documentation
├── sk-intro-agents.sln              # Solution file
├── notebooks/                       # Jupyter notebooks (optional)
├── src/                             # Source code directory
│   ├── appsettings.Development.json # Environment-specific config
│   ├── appsettings.json             # Main configuration file
│   ├── ArchivePlugin.cs             # Plugin for data archival
│   ├── NewsPlugin.cs                # Plugin for fetching news
│   ├── Program.cs                   # Main entry point
│   ├── sk-intro-agents.csproj       # Project configuration
│   ├── archives/                    # Archived runtime-generated data
│   ├── bin/                         # Build output (auto-generated)
│   └── obj/                         # Build object files (auto-generated)
```

# 🔌 Plugins

## 📰 **NewsPlugin**

- **Purpose**: Fetches top news articles from the New York Times RSS feed for a specified category.
- **Key Features**:
  - Retrieves up to 10 news items.
  - Uses the **SimpleFeedReader** library for RSS feed parsing.
- **Sample Input**: `tech`
- **Sample Output**:

```
  1. TikTok Faces U.S. Ban After Losing Bid to Overturn New Law
  2. Bitcoin Price Surges to a Milestone: $100,000
```

### 💾 ArchivePlugin

- **Purpose:** Saves conversation data or any runtime information to local storage.

- **Key Features:**

- Writes data into text files in the archives/ folder.

- Useful for maintaining logs or preserving user interactions.

### Output Example:

Data is saved as C:\archives\<filename>.txt.

### 🔧 Configuration Details:

Edit the appsettings.json file in the src/ directory to include your Azure OpenAI credentials:

```
{
  "AzureOpenAI": {
    "DeploymentName": "your-deployment-name",
    "Endpoint": "https://your-endpoint.azure.com",
    "ApiKey": "your-api-key"
  }
}
```

### 🖥️ How to Run

1. Clone the Repository

`git clone <repository-url>`
`cd sk-intro-agents`

2. Restore Dependencies

`dotnet restore`

3. Build the Project

`dotnet build`

4. Run the Application

`dotnet run`

### 🖱️ Interact with the AI

- **Prompt:** Enter `hi` or `tech news please`.

- **Response:** The AI will respond in real-time with interactive, context-aware replies.

```
Prompt: hi
Response: Hello! How can I assist you today?

Prompt: tech news please
Response: Here are some of the latest updates in the technology sector:
1. TikTok Faces U.S. Ban After Losing Bid to Overturn New Law
2. Bitcoin Price Surges to a Milestone: $100,000
```

### 🎉 Enjoy exploring the capabilities of Semantic Kernel Intro Agents! 🚀
