using System;
using System.Threading.Tasks;

using Brighid.Identity.Sns;

using Microsoft.AspNetCore.Mvc;

namespace Brighid.Identity
{
    public abstract class EntityController<TEntity, TPrimaryKey, TRepository, TService> : Controller
        where TEntity : class
        where TRepository : IRepository<TEntity, TPrimaryKey>
        where TService : IEntityService<TEntity, TPrimaryKey>
    {
        protected string BaseAddress { get; }
        protected TService Service { get; }
        protected TRepository Repository { get; }

        public EntityController(
            string baseAddress,
            TService service,
            TRepository repository
        )
        {
            BaseAddress = baseAddress;
            Service = service;
            Repository = repository;
        }

        private void TrySetSnsContextItems(TPrimaryKey id, TEntity data)
        {
            var requestType = HttpContext.Items[Constants.RequestSource] as IdentityRequestSource?;
            if (requestType == IdentityRequestSource.Sns)
            {
                SetSnsContextItems(id, data);
            }
        }

        protected virtual void SetSnsContextItems(TPrimaryKey id, TEntity data)
        {
            HttpContext.Items[CloudFormationConstants.Id] = id;
            HttpContext.Items[CloudFormationConstants.Data] = data;
        }

        [HttpPost]
        public virtual async Task<ActionResult<TEntity>> Create([FromBody] TEntity entity)
        {
            var result = await Service.Create(entity);
            var primaryKey = Service.GetPrimaryKey(entity);
            var destination = new Uri($"{BaseAddress}/{primaryKey}", UriKind.Relative);
            TrySetSnsContextItems(primaryKey, result);
            return Created(destination, result);
        }

        [HttpGet("{id}")]
        public virtual async Task<ActionResult<TEntity>> GetById(TPrimaryKey id)
        {
            var result = await Repository.FindById(id);
            return result == null ? NotFound() : Ok(result);
        }

        [HttpPut("{id}")]
        public virtual async Task<ActionResult<TEntity>> UpdateById(TPrimaryKey id, [FromBody] TEntity entity)
        {
            var result = await Service.UpdateById(id, entity);
            TrySetSnsContextItems(id, result);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public virtual async Task<ActionResult<TEntity>> DeleteById(TPrimaryKey id)
        {
            var result = await Service.DeleteById(id);
            TrySetSnsContextItems(id, result);
            return Ok(result);
        }
    }
}
