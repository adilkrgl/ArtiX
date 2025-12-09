namespace ArtiX.WebAdmin.Services.Api;

public interface ICustomerApiClient
{
    // TODO: Define customer endpoints when API contract is available.
}

public class CustomerApiClient : ApiClientBase, ICustomerApiClient
{
    public CustomerApiClient(HttpClient httpClient) : base(httpClient)
    {
    }
}
