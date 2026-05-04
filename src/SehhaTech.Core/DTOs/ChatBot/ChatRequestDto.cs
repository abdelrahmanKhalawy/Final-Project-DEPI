namespace SehhaTech.Core.DTOs.ChatBot;

public record ChatRequestDto(
    string Message,
    List<ChatMessageDto>? History = null
);

public record ChatMessageDto(
    string Role,   // "user" or "model"
    string Text
);

public record ChatResponseDto(
    bool Success,
    string Reply,
    string? Error = null
);