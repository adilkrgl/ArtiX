namespace ArtiX.WebAdmin.Services.Api;

public abstract class ApiClientBase
{
    protected ApiClientBase(HttpClient httpClient)
    {
        HttpClient = httpClient;
    }

    protected HttpClient HttpClient { get; }
}
