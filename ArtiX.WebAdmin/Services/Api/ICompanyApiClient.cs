namespace ArtiX.WebAdmin.Services.Api;

public interface ICompanyApiClient
{
    // TODO: Define company endpoints when API contract is available.
}

public class CompanyApiClient : ApiClientBase, ICompanyApiClient
{
    public CompanyApiClient(HttpClient httpClient) : base(httpClient)
    {
    }
}
