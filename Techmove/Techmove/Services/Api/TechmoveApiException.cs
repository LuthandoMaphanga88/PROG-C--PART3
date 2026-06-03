namespace Techmove.Services.Api;

public class TechmoveApiException : Exception
{
    public TechmoveApiException(string message, int statusCode, string? responseBody = null, Exception? innerException = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
        ResponseBody = responseBody;
    }

    public int StatusCode { get; }

    public string? ResponseBody { get; }
}
