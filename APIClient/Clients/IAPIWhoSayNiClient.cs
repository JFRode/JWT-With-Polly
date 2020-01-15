using System.Threading;
using System.Threading.Tasks;

namespace APIClient.Clients
{
    public interface IAPIWhoSayNiClient
    {
        Task<string> Get(CancellationToken cancellationToken);

        Task<string> GetAuthenticationToken(CancellationToken cancellationToken);

        Task<string> RefreshAuthenticationToken(CancellationToken cancellationToken);
    }
}