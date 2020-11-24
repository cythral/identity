using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

namespace Brighid.Identity
{
    public interface INormalizeable<in TDbContext> where TDbContext : DbContext
    {
        Task Normalize(TDbContext dbContext, CancellationToken cancellationToken = default);
    }
}
