namespace UI.Config;

public class AppConfig
{
    public string ApiBaseUrl { get; set; } = "";
    public string RealtimeUrl { get; set; } = "";

    // на будущее
    public int TimeoutSeconds { get; set; } = 30;
}