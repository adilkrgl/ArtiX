namespace ArtiX.WebAdmin.Services.Api;

public interface IInvoiceApiClient
{
    // TODO: Define invoice endpoints when API contract is available.
}

public class InvoiceApiClient : ApiClientBase, IInvoiceApiClient
{
    public InvoiceApiClient(HttpClient httpClient) : base(httpClient)
    {
    }
}
