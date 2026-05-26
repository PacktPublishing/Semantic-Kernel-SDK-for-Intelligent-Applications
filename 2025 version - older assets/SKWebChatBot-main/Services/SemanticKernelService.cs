using System.Text;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using SKWebChatBot.Data;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace SKWebChatBot.Services;

public class SemanticKernelService
{
    private readonly Kernel _kernel;

    public SemanticKernelService(IConfiguration configuration)
    {
        string apiKey = configuration["SemanticKernel:ApiKey"];
        string endpoint = configuration["SemanticKernel:Endpoint"];
        string deploymentName = configuration["SemanticKernel:DeploymentName"];

        var builder = Kernel.CreateBuilder();
        builder.AddAzureOpenAIChatCompletion(
            deploymentName: deploymentName,
            endpoint: endpoint,
            apiKey: apiKey
        );
        _kernel = builder.Build();
    }

    public async Task<string> GetChatResponseAsync(string userMessage)
    {
        var result = await _kernel.InvokePromptAsync(userMessage);
        Console.WriteLine(result);
        return result.ToString();
    }

    public async Task<string> GetChatResponseWithHistoryAsync(string userMessage, IEnumerable<ChatMessage> messages)
    {
        var chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();
        ChatHistory chatHistory = [];
        foreach (var message in messages)
        {
            if (message.Sender == "User")
            {
                chatHistory.AddUserMessage(message.Message);
            }
            else
            {
                chatHistory.AddAssistantMessage(message.Message);
            }
        }
        chatHistory.AddUserMessage(userMessage);

        var result = chatCompletionService.GetStreamingChatMessageContentsAsync(
            chatHistory: chatHistory,
            kernel: _kernel
        );

        var response = new StringBuilder();
        await foreach (var chunk in result)
        {
            response.Append(chunk);
        }
        return response.ToString();
    }
    public async Task<string> GetChatResponseWithRagAsync(string userMessage)
    {
        string pdfPath = Path.Combine(Directory.GetCurrentDirectory(), "Documents", "career-profiles.pdf");
        string content = ExtractTextFromPdf(pdfPath);

        var prompt = $"""
                You are a career guidance assistant. You will be provided with a list of career profiles, each containing a title and a description.

                Your task is to help users find suitable career options based on their interests and skills. When a user provides their interests or skills, identify and list the careers that align with the provided information. If no matching careers are found, respond with, "I couldn't find any careers matching your interests and skills."

                Here is the list of career profiles:

                {content}

                User's interests and skills: {userMessage}
                """;
        
        var result = await _kernel.InvokePromptAsync(prompt);
        Console.WriteLine(result);
        return result.ToString();
    }

    private string ExtractTextFromPdf(string pdfPath)
    {
        var text = new StringBuilder();

        using (var document = PdfDocument.Open(pdfPath))
        {
            foreach (var page in document.GetPages())
            {
                var pageText = ContentOrderTextExtractor.GetText(page);
                text.AppendLine(pageText);
            }
        }

        return text.ToString();
    }
}
