using Google.GenAI;
using Google.GenAI.Types;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SehhaTech.Core.DTOs.ChatBot;

namespace SehhaTech.Infrastructure.Services;

public class ChatBotService
{
    private readonly string _apiKey;
    private readonly ILogger<ChatBotService> _logger;

    private static readonly Dictionary<string, (int Count, DateTime Window)> _rateLimits = new();
    private static readonly SemaphoreSlim _lock = new(1, 1);

    public ChatBotService(IConfiguration config, ILogger<ChatBotService> logger)
    {
        _logger = logger;
        _apiKey = config["Gemini:ApiKey"]
            ?? throw new InvalidOperationException("Gemini API Key is missing.");
    }

    public async Task<ChatResponseDto> AskLandingAsync(ChatRequestDto request, string clientIp)
    {
        if (!await CheckRateLimitAsync(clientIp))
            return Fail("لقد تجاوزت الحد المسموح به. حاول مرة أخرى بعد دقيقة. / Rate limit exceeded.");

        return await SendAsync(request, BotRole.Landing);
    }

    public async Task<ChatResponseDto> AskInternalAsync(ChatRequestDto request, string userRole, string clinicName)
    {
        return await SendAsync(request, BotRole.Internal, userRole, clinicName);
    }

    private async Task<ChatResponseDto> SendAsync(
        ChatRequestDto request,
        BotRole botRole,
        string? userRole = null,
        string? clinicName = null)
    {
        try
        {
            var client = new Client(apiKey: _apiKey);
            var systemPrompt = GetSystemPrompt(botRole, userRole, clinicName);

            // Build conversation history
            var contents = new List<Content>();

            // System prompt as first exchange
            contents.Add(new Content
            {
                Role = "user",
                Parts = [new Part { Text = systemPrompt }]
            });
            contents.Add(new Content
            {
                Role = "model",
                Parts = [new Part { Text = "مفهوم، أنا جاهز. / Understood, ready to help." }]
            });

            // Add history (last 10 messages)
            if (request.History != null && request.History.Count > 0)
            {
                foreach (var msg in request.History.TakeLast(10))
                {
                    contents.Add(new Content
                    {
                        Role = msg.Role == "user" ? "user" : "model",
                        Parts = [new Part { Text = msg.Text }]
                    });
                }
            }

            // Add current message
            contents.Add(new Content
            {
                Role = "user",
                Parts = [new Part { Text = request.Message.Trim() }]
            });

            var response = await client.Models.GenerateContentAsync(
                model: "gemini-2.5-flash",
                contents: contents,
                config: new GenerateContentConfig
                {
                    Temperature = 0.7f,
                    MaxOutputTokens = 500
                }
            );

            var reply = response.Candidates?[0].Content?.Parts?[0].Text;

            return string.IsNullOrWhiteSpace(reply)
                ? Fail("لم أتمكن من الرد. / Could not generate a response.")
                : new ChatResponseDto(true, reply.Trim());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ChatBotService error: {Message}", ex.Message);
            return Fail("حدث خطأ غير متوقع. / An unexpected error occurred.");
        }
    }

    private static string GetSystemPrompt(BotRole role, string? userRole = null, string? clinicName = null) => role switch
    {
        BotRole.Landing => """
            You are SehhaTech's intelligent assistant — a clinic management system for Egyptian healthcare.

            LANGUAGE RULE (CRITICAL):
            - Arabic input → respond ONLY in Arabic
            - English input → respond ONLY in English
            - Never mix languages

            ABOUT SEHHATECH:
            - Multi-tenant SaaS clinic management system
            - Built with ASP.NET Core + SQL Server
            - Pricing: 500 EGP/year per clinic
            - Payment via Paymob

            FEATURES:
            ✓ Patient management & medical records
            ✓ Appointment booking & scheduling
            ✓ Doctor profiles & specializations
            ✓ Receptionist tools & queue management
            ✓ Clinic Admin dashboard
            ✓ Role-based access (ClinicAdmin, Doctor, Reception)
            ✓ Secure JWT authentication

            PERSONALITY:
            - You are warm, friendly, and conversational — like a helpful colleague, not a robot
            - Use simple everyday language, avoid technical jargon
            - Keep responses short (3-5 lines max) unless the user asks for details
            - Use occasional emojis to feel human 😊
            - If someone asks something unrelated to SehhaTech, kindly say you can only help with SehhaTech topics
            - Never use bullet points unless the user explicitly asks for a list
            - Talk naturally, like you're chatting with someone, not writing a report
            """,

        BotRole.Internal => $"""
            You are an internal assistant for SehhaTech.
            You are helping a {userRole ?? "staff member"} at {clinicName ?? "the clinic"}.

            LANGUAGE RULE (CRITICAL):
            - Arabic input → respond ONLY in Arabic
            - English input → respond ONLY in English

            Help based on role:
            - Doctor: patient info, appointment status
            - Reception: booking, scheduling, queue management
            - ClinicAdmin: manage doctors, receptionists, settings

            Be concise and professional.
            You provide guidance only — not direct database changes.
            """,

        _ => "You are a helpful assistant. Match the user's language."
    };

    private static async Task<bool> CheckRateLimitAsync(string ip)
    {
        await _lock.WaitAsync();
        try
        {
            var now = DateTime.UtcNow;
            if (_rateLimits.TryGetValue(ip, out var entry))
            {
                if ((now - entry.Window).TotalMinutes >= 1)
                {
                    _rateLimits[ip] = (1, now);
                    return true;
                }
                if (entry.Count >= 10) return false;
                _rateLimits[ip] = (entry.Count + 1, entry.Window);
                return true;
            }
            _rateLimits[ip] = (1, now);
            return true;
        }
        finally { _lock.Release(); }
    }

    private static ChatResponseDto Fail(string msg) => new(false, msg, msg);
}

internal enum BotRole { Landing, Internal }