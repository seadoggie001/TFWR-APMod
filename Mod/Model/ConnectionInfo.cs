namespace com.seadoggie.TFWRArchipelago.Model;

public class ConnectionInfo(string url, int port, string username, string password)
{
    public string Url { get; set; } = url;
    public int Port { get; set; } = port;
    public string Username { get; set; } = username;
    public string Password { get; set; } = password;
}