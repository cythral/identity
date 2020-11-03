using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

namespace Brighid.Identity
{
    public abstract class Repository<TEntity, TPrimaryKeyType> : IRepository<TEntity, TPrimaryKeyType> where TEntity : class, new() where TPrimaryKeyType : class
    {
        protected DatabaseContext Context { get; private set; }
        protected DbSet<TEntity> Set { get; private set; }
        protected static string? PrimaryKeyName { get; private set; }
        protected static Action<TEntity, TPrimaryKeyType> SetPrimaryKey { get; private set; } = null!;

        public Repository(DatabaseContext context)
        {
            Context = context;
            Set = context.Set<TEntity>();

            if (PrimaryKeyName == null)
            {
                PrimaryKeyName = Context.Model.FindEntityType(typeof(TEntity)).FindPrimaryKey().Properties[0].Name;

                var prop = typeof(TEntity).GetProperties().Where(prop => prop.Name == PrimaryKeyName).First();

                var setterMethod = prop.GetSetMethod()!;
                var setPrimaryKeyType = Type.GetType($"System.Action`2[{typeof(TEntity).FullName}, {typeof(TPrimaryKeyType).FullName}]")!;

                SetPrimaryKey = (Action<TEntity, TPrimaryKeyType>)Delegate.CreateDelegate(setPrimaryKeyType, setterMethod);
            }
        }

        public IQueryable<TEntity> All => Set.AsQueryable();

        public async Task<TEntity> Add(TEntity entity)
        {
            var result = await Set.AddAsync(entity);

            try
            {
                await Context.SaveChangesAsync();
                return result.Entity;
            }
            catch (DbUpdateException e)
            {
                result.State = EntityState.Detached;
                throw new DbUpdateException(e.Message);
            }
        }

        public async Task<TEntity?> TryAdd(TEntity entity)
        {
            try
            {
                return await Add(entity);
            }
#pragma warning disable CA1031
            catch (Exception) { return null; }
#pragma warning restore CA1031
        }

        public async Task<TEntity> GetById(TPrimaryKeyType primaryKey, params Expression<Func<TEntity, object?>>[] embeds)
        {
            var queryable = embeds.Aggregate(All, (query, embed) => query.Include(embed));

            return await queryable.FirstOrDefaultAsync(entity =>
                EF.Property<TPrimaryKeyType>(entity, PrimaryKeyName).Equals(primaryKey)
            );
        }

        public async Task<bool> Exists(TPrimaryKeyType primaryKey)
        {
            var query = from obj in All where EF.Property<TPrimaryKeyType>(obj, PrimaryKeyName).Equals(primaryKey) select obj;
            var count = await query.CountAsync();
            return count > 0;
        }

        public async Task<TEntity> Save(TEntity entity)
        {
            Set.Attach(entity);
            await Context.SaveChangesAsync();
            return entity;
        }

        public async Task<TEntity> Remove(TPrimaryKeyType primaryKey)
        {
            var entity = new TEntity();
            SetPrimaryKey(entity, primaryKey);

            Set.Attach(entity);
            Set.Remove(entity);
            await Context.SaveChangesAsync();

            return entity;
        }

        public TEntity Track(TPrimaryKeyType primaryKey)
        {
            var entity = new TEntity();
            SetPrimaryKey(entity, primaryKey);
            Set.Attach(entity);
            return entity;
        }
    }
}
