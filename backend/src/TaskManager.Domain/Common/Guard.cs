namespace TaskManager.Domain.Common;

internal static class Guard
{
    public static string RequiredText(string? value, int maxLength, string field)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException($"{field} is required.");

        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
            throw new DomainException($"{field} must be at most {maxLength} characters.");

        return trimmed;
    }

    public static string OptionalText(string? value, int maxLength, string field)
    {
        var trimmed = value?.Trim() ?? string.Empty;
        if (trimmed.Length > maxLength)
            throw new DomainException($"{field} must be at most {maxLength} characters.");

        return trimmed;
    }

    public static TEnum Defined<TEnum>(TEnum value, string field) where TEnum : struct, Enum
    {
        if (!Enum.IsDefined(value))
            throw new DomainException($"{field} has unsupported value '{value}'.");

        return value;
    }
}
