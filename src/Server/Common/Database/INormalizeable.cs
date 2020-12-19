using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

namespace Brighid.Identity
{
    public interface INormalizeable
    {
        bool IsNormalized { get; }
        Task Normalize(DatabaseContext dbContext, CancellationToken cancellationToken = default);
    }
}
