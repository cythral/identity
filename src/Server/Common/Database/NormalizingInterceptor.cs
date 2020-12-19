using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Brighid.Identity
{
    public class NormalizingInterceptor : SaveChangesInterceptor
    {
        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            var context = eventData.Context as DatabaseContext;
            var tracker = eventData.Context.ChangeTracker;
            var entries = tracker.Entries();

            foreach (var entry in entries)
            {
                if (entry.Entity is INormalizeable normalizeable && !normalizeable.IsNormalized)
                {
                    await normalizeable.Normalize(context!, cancellationToken);
                }
            }

            return result;
        }

        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            return SavingChangesAsync(eventData, result).AsTask().GetAwaiter().GetResult();
        }
    }
}
