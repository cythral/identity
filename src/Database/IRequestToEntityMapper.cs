using System.Threading;
using System.Threading.Tasks;

namespace Brighid.Identity
{
    public interface IRequestToEntityMapper<TRequest, TEntity>
    {
        Task<TEntity> MapRequestToEntity(TRequest request, CancellationToken cancellationToken = default);
    }
}
