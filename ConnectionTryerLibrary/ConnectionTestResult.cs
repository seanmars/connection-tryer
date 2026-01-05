namespace ConnectionTryerLibrary;

public class ConnectionTestResult
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public Exception? Exception { get; set; }
    public TimeSpan ElapsedTime { get; set; }
}