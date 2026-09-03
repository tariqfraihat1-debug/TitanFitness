namespace TitanFitness.Common.Responses;

public sealed class ErrorResponse
{
    public object? Data { get; set; }
    public string[] Errors { get; set; } = Array.Empty<string>();
}