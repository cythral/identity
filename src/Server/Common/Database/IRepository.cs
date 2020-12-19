using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Brighid.Identity
{
    public interface IRepository<TEntity, TPrimaryKeyType> where TEntity : class
    {
        IQueryable<TEntity> All { get; }

        Task<TEntity> Add(TEntity entity);

        Task<TEntity?> TryAdd(TEntity entity);

        Task<TEntity> GetById(TPrimaryKeyType primaryKey, params string[] embeds);

        Task<bool> Exists(TPrimaryKeyType primaryKey);

        Task<TEntity> Save(TEntity entity);

        Task<TEntity> Remove(TPrimaryKeyType primaryKey);

        Task<TEntity> Remove(TEntity entity);

        void TrackAsDeleted(TEntity entity);
    }
}
