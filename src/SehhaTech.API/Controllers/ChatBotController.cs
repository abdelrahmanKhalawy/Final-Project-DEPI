using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SehhaTech.Core.DTOs.ChatBot;
using SehhaTech.Infrastructure.Services;

namespace SehhaTech.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatBotController : ControllerBase
{
    private readonly ChatBotService _chatBot;

    public ChatBotController(ChatBotService chatBot)
    {
        _chatBot = chatBot;
    }

    // ─────────────────────────────────────────────
    //  LANDING PAGE BOT — No Auth Required
    // ─────────────────────────────────────────────

    /// <summary>
    /// Public chatbot for the landing page.
    /// Answers questions about SehhaTech features, pricing, and how to get started.
    /// Supports Arabic and English automatically.
    /// </summary>
    [HttpPost("ask")]
    [AllowAnonymous]
    public async Task<IActionResult> Ask([FromBody] ChatRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
            return BadRequest(new ChatResponseDto(false, "", "الرسالة لا يمكن أن تكون فارغة. / Message cannot be empty."));

        if (request.Message.Length > 500)
            return BadRequest(new ChatResponseDto(false, "", "الرسالة طويلة جداً. / Message is too long."));

        var clientIp = GetClientIp();
        var result = await _chatBot.AskLandingAsync(request, clientIp);

        return result.Success ? Ok(result) : StatusCode(429, result);
    }

    // ─────────────────────────────────────────────
    //  INTERNAL BOT — JWT Required
    // ─────────────────────────────────────────────

    /// <summary>
    /// Internal chatbot for authenticated users (Doctor, Reception, ClinicAdmin).
    /// Supports Arabic and English automatically.
    /// Context-aware based on user role.
    /// </summary>
    [HttpPost("internal")]
    [Authorize(Roles = "Doctor,Reception,ClinicAdmin")]
    public async Task<IActionResult> Internal([FromBody] ChatRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
            return BadRequest(new ChatResponseDto(false, "", "الرسالة لا يمكن أن تكون فارغة. / Message cannot be empty."));

        if (request.Message.Length > 1000)
            return BadRequest(new ChatResponseDto(false, "", "الرسالة طويلة جداً. / Message is too long."));

        // Extract role and clinic info from JWT claims
        var userRole = User.FindFirst("Role")?.Value ?? "Staff";
        var clinicName = User.FindFirst("ClinicName")?.Value ?? "the clinic";

        var result = await _chatBot.AskInternalAsync(request, userRole, clinicName);

        return result.Success ? Ok(result) : StatusCode(500, result);
    }

    // ─────────────────────────────────────────────
    //  HELPER
    // ─────────────────────────────────────────────

    private string GetClientIp()
    {
        // Works behind reverse proxy (nginx, etc.)
        var forwarded = Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwarded))
            return forwarded.Split(',')[0].Trim();

        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}