using System.Threading.Tasks;

namespace Brighid.Identity
{
    public interface IEntityService<TEntity, TPrimaryKey>
    {
        TPrimaryKey GetPrimaryKey(TEntity entity);

        Task<TEntity> Create(TEntity entity);

        Task<TEntity> UpdateById(TPrimaryKey id, TEntity entity);

        Task<TEntity> DeleteById(TPrimaryKey id);
    }
}
