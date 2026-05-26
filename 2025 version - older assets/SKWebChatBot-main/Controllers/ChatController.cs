using Microsoft.AspNetCore.Mvc;
using SKWebChatBot.Services;

namespace SKWebChatBot.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ChatController(SemanticKernelService _semanticKernelService, ChatService _chatService) : ControllerBase
{
    [HttpGet("{sessionId}")]
    public async Task<ActionResult> GetMessagesAsync(string sessionId)
    {
        var messages = await _chatService.GetMessagesAsync(sessionId);
        return Ok(messages);
    }

    [HttpPost]
    public async Task<ActionResult> GetChatResponseAsync([FromBody] ChatRequest chatRequest)
    {
        var sessionId = chatRequest.SessionId;
        var messages = await _chatService.GetMessagesAsync(sessionId); 
        await _chatService.AddMessageAsync(sessionId, chatRequest.UserMessage, "User");
        // var response = await _semanticKernelService.GetChatResponseAsync(chatRequest.UserMessage);
        // var response = await _semanticKernelService.GetChatResponseWithHistoryAsync(chatRequest.UserMessage, messages);
        var response = await _semanticKernelService.GetChatResponseWithRagAsync(chatRequest.UserMessage);
        await _chatService.AddMessageAsync(sessionId, response, "Bot");
        return Ok(response);
    }
}

public record ChatRequest(string UserMessage, string SessionId);
