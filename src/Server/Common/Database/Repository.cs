using System;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

namespace Brighid.Identity
{
    public abstract class Repository<TEntity, TPrimaryKeyType> : IRepository<TEntity, TPrimaryKeyType> where TEntity : class where TPrimaryKeyType : notnull
    {
        protected DatabaseContext Context { get; private set; }
        protected DbSet<TEntity> Set { get; private set; }
        protected static string? PrimaryKeyName { get; private set; }
        protected static Action<TEntity, TPrimaryKeyType>? SetPrimaryKey { get; private set; } = null!;

        public Repository(DatabaseContext context)
        {
            Context = context;
            Set = context.Set<TEntity>();

            if (PrimaryKeyName == null)
            {
                PrimaryKeyName = Context.Model.FindEntityType(typeof(TEntity)).FindPrimaryKey().Properties[0].Name;

                var prop = typeof(TEntity).GetProperties().Where(prop => prop.Name == PrimaryKeyName).First();
                var setterMethod = prop.GetSetMethod();

                if (prop != null && setterMethod != null)
                {
                    var setPrimaryKeyType = Type.GetType($"System.Action`2[{typeof(TEntity).FullName}, {typeof(TPrimaryKeyType).FullName}]")!;
                    SetPrimaryKey = (Action<TEntity, TPrimaryKeyType>)Delegate.CreateDelegate(setPrimaryKeyType, setterMethod);
                }
            }
        }

        public virtual IQueryable<TEntity> All => Set.AsQueryable();

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

        public async Task<TEntity> GetById(TPrimaryKeyType primaryKey, params string[] embeds)
        {
            var entitySet = embeds
                .Aggregate(All, (query, embed) => query.Include(embed))
                .AsQueryable();

            var query = from entity in entitySet
                        where EF.Property<TPrimaryKeyType>(entity, PrimaryKeyName).Equals(primaryKey)
                        select entity;

            return await query.FirstOrDefaultAsync();
        }

        public async Task<bool> Exists(TPrimaryKeyType primaryKey)
        {
            var query = from obj in All where EF.Property<TPrimaryKeyType>(obj, PrimaryKeyName).Equals(primaryKey) select obj;
            var count = await query.CountAsync();
            return count > 0;
        }

        public async Task<TEntity> Save(TEntity entity)
        {
            Context.Attach(entity);
            await Context.SaveChangesAsync();
            return entity;
        }

        public async Task<TEntity> Remove(TPrimaryKeyType primaryKey)
        {
            if (SetPrimaryKey == null)
            {
                throw new NotSupportedException();
            }

            var entity = (TEntity)FormatterServices.GetUninitializedObject(typeof(TEntity));
            SetPrimaryKey(entity, primaryKey);

            Set.Attach(entity);
            return await Remove(entity);
        }

        public async Task<TEntity> Remove(TEntity entity)
        {
            Set.Remove(entity);
            await Context.SaveChangesAsync();
            return entity;
        }

        public void TrackAsDeleted(TEntity entity)
        {
            Context.Entry(entity).State = EntityState.Deleted;
        }
    }
}
