using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CodeWalker.Cli.Helpers;

internal abstract record BaseResult
{
    [JsonPropertyName("success")]
    [JsonPropertyOrder(-1)]
    public required bool Success { get; init; }

    [JsonPropertyName("errorMessages")]
    [JsonPropertyOrder(100)]
    public required IReadOnlyList<string> ErrorMessages { get; init; }
}

internal static class Output
{
    public static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        WriteIndented = true,
    };

    /// <summary>
    /// Reports an error in JSON or text format and returns exit code 1.
    /// The <c>with</c> expression preserves the runtime (derived) type, and
    /// <see cref="JsonSerializer"/> serialises using that type so all properties are included.
    /// </summary>
    public static int ReportError(
        string message,
        bool json,
        BaseResult result,
        string? stackTrace = null
    )
    {
        if (json)
        {
            BaseResult errorResult = result with
            {
                Success = false,
                ErrorMessages = [.. result.ErrorMessages, message],
            };
            Console.WriteLine(
                JsonSerializer.Serialize(errorResult, errorResult.GetType(), JsonSerializerOptions)
            );
        }
        else
        {
            Console.Error.WriteLine($"Error: {message}");
            if (stackTrace != null)
                Console.Error.WriteLine(stackTrace);
        }
        return 1;
    }
}
