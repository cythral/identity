using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using EFEntityState = Microsoft.EntityFrameworkCore.EntityState;

namespace Brighid.Identity
{
    public abstract class Repository<TEntity, TPrimaryKeyType> : IRepository<TEntity, TPrimaryKeyType>
        where TEntity : class
        where TPrimaryKeyType : notnull
    {
        private static readonly Func<DatabaseContext, TPrimaryKeyType, IAsyncEnumerable<TEntity>> FindByIdCompiledQuery = EF.CompileAsyncQuery<DatabaseContext, TPrimaryKeyType, TEntity>(
            (context, primaryKey) => from entity in context.Set<TEntity>()
                                     where EF.Property<TPrimaryKeyType>(entity, PrimaryKeyName!).Equals(primaryKey)
                                     select entity
        );

        public Repository(DatabaseContext context)
        {
            Context = context;
            Set = context.Set<TEntity>();

            if (PrimaryKeyName == null)
            {
                PrimaryKeyName = Context.Model.FindEntityType(typeof(TEntity))!.FindPrimaryKey()!.Properties[0].Name;

                var prop = typeof(TEntity).GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Where(prop => prop.Name == PrimaryKeyName).First();
                var setterMethod = prop.GetSetMethod(true);

                if (prop != null && setterMethod != null)
                {
                    var setPrimaryKeyType = typeof(Action<,>).MakeGenericType(typeof(TEntity), typeof(TPrimaryKeyType));
                    SetPrimaryKey = (Action<TEntity, TPrimaryKeyType>)Delegate.CreateDelegate(setPrimaryKeyType, setterMethod);
                }
            }
        }

        public virtual IQueryable<TEntity> All => Set.AsQueryable();

        protected static string? PrimaryKeyName { get; private set; }

        protected static Action<TEntity, TPrimaryKeyType>? SetPrimaryKey { get; private set; } = null!;

        protected DatabaseContext Context { get; private set; }

        protected DbSet<TEntity> Set { get; private set; }

        public virtual async Task<IEnumerable<TEntity>> List()
        {
            return await All.ToListAsync();
        }

        public virtual async Task LoadCollection(TEntity entity, string collection, CancellationToken cancellationToken = default)
        {
            var entry = Context.Entry(entity);
            await entry.Collection(collection).LoadAsync(cancellationToken);
        }

        public virtual async Task<TEntity> Add(TEntity entity)
        {
            var result = await Set.AddAsync(entity);

            try
            {
                await Context.SaveChangesAsync();
                return result.Entity;
            }
            catch (DbUpdateException e)
            {
                result.State = EFEntityState.Detached;
                throw new DbUpdateException(e.Message, e.InnerException);
            }
        }

        public virtual async Task<TEntity?> TryAdd(TEntity entity)
        {
            try
            {
                return await Add(entity);
            }
#pragma warning disable CA1031
            catch (Exception)
            {
                return null;
            }
#pragma warning restore CA1031
        }

        public virtual async Task<TEntity?> FindById(TPrimaryKeyType primaryKey, CancellationToken cancellationToken = default)
        {
            return await FindByIdCompiledQuery(Context, primaryKey).FirstOrDefaultAsync(cancellationToken);
        }

        public virtual async Task<bool> Exists(TPrimaryKeyType primaryKey, CancellationToken cancellationToken = default)
        {
            return await FindByIdCompiledQuery(Context, primaryKey).AnyAsync(cancellationToken);
        }

        public virtual async Task<TEntity> Save(TEntity entity, CancellationToken cancellationToken = default)
        {
            Context.Attach(entity);
            await Context.SaveChangesAsync(cancellationToken);
            return entity;
        }

        public virtual async Task<TEntity> Remove(TPrimaryKeyType primaryKey)
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

        public virtual async Task<TEntity> Remove(TEntity entity)
        {
            Set.Remove(entity);
            await Context.SaveChangesAsync();
            return entity;
        }

        public virtual void TrackAsDeleted(TEntity entity)
        {
            Context.Entry(entity).State = EFEntityState.Deleted;
        }

        public virtual EntityState GetState(TEntity? entity)
        {
            if (entity == null)
            {
                return EntityState.Invalid;
            }

            var state = Context.Entry(entity).State;
            return state switch
            {
                EFEntityState.Added => EntityState.Added,
                EFEntityState.Deleted => EntityState.Deleted,
                EFEntityState.Detached => EntityState.Detached,
                EFEntityState.Unchanged => EntityState.Unchanged,
                EFEntityState.Modified => EntityState.Modified,
                _ => EntityState.Invalid,
            };
        }
    }
}
