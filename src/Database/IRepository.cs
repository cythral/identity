using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Brighid.Identity
{
    public interface IRepository<TEntity, TPrimaryKeyType>
        where TEntity : class
    {
        IQueryable<TEntity> All { get; }

        Task<IEnumerable<TEntity>> List();

        Task LoadCollection(TEntity entity, string collection, CancellationToken cancellationToken = default);

        Task<TEntity> Add(TEntity entity);

        Task<TEntity?> TryAdd(TEntity entity);

        Task<TEntity?> FindById(TPrimaryKeyType primaryKey, CancellationToken cancellationToken = default);

        Task<bool> Exists(TPrimaryKeyType primaryKey, CancellationToken cancellationToken = default);

        Task<TEntity> Save(TEntity entity, CancellationToken cancellationToken = default);

        Task<TEntity> Remove(TPrimaryKeyType primaryKey);

        Task<TEntity> Remove(TEntity entity);

        void TrackAsDeleted(TEntity entity);

        EntityState GetState(TEntity? entity);
    }
}
