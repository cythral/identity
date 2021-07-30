using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

namespace Brighid.Identity
{
    public abstract class EntityController<TEntity, TEntityRequest, TPrimaryKey, TRepository, TMapper, TService> : Controller
        where TEntity : class
        where TMapper : IRequestToEntityMapper<TEntityRequest, TEntity>
        where TRepository : IRepository<TEntity, TPrimaryKey>
        where TService : IEntityService<TEntity, TPrimaryKey>
    {
        public EntityController(
            string baseAddress,
            TMapper mapper,
            TService service,
            TRepository repository
        )
        {
            Mapper = mapper;
            BaseAddress = baseAddress;
            Service = service;
            Repository = repository;
        }

        protected string BaseAddress { get; }

        protected TService Service { get; }

        protected TRepository Repository { get; }

        protected TMapper Mapper { get; }

        [HttpPost]
        public virtual async Task<ActionResult<TEntity>> Create([FromBody] TEntityRequest request)
        {
            try
            {
                await Validate(request);
                var entity = await Mapper.MapRequestToEntity(request, HttpContext.RequestAborted);
                var result = await Service.Create(entity);
                var primaryKey = Service.GetPrimaryKey(entity);
                var destination = new Uri($"{BaseAddress}/{primaryKey}", UriKind.Relative);
                return Created(destination, result);
            }
            catch (Exception e) when (e is IValidationException)
            {
                return UnprocessableEntity(new { e.Message });
            }
            catch (AggregateException e)
            {
                return UnprocessableEntity(new
                {
                    Message = "Multiple validation errors occurred.",
                    ValidationErrors = from innerException in e.InnerExceptions
                                       where innerException is IValidationException
                                       select innerException.Message,
                });
            }
        }

        [HttpGet("{id}")]
        public virtual async Task<ActionResult<TEntity>> GetById(TPrimaryKey id)
        {
            var result = await Repository.FindById(id);
            return result == null ? NotFound() : Ok(result);
        }

        [HttpPut("{id}")]
        public virtual async Task<ActionResult<TEntity>> UpdateById(TPrimaryKey id, [FromBody] TEntityRequest request)
        {
            try
            {
                await Validate(request);
                var entity = await Mapper.MapRequestToEntity(request, HttpContext.RequestAborted);
                var result = await Service.UpdateById(id, entity);
                return Ok(result);
            }
            catch (Exception e) when (e is IValidationException)
            {
                return UnprocessableEntity(new { e.Message });
            }
            catch (AggregateException e)
            {
                return UnprocessableEntity(new
                {
                    Message = "Multiple validation errors occurred.",
                    ValidationErrors = from innerException in e.InnerExceptions
                                       where innerException is IValidationException
                                       select innerException.Message,
                });
            }
        }

        [HttpDelete("{id}")]
        public virtual async Task<ActionResult<TEntity>> DeleteById(TPrimaryKey id)
        {
            var result = await Service.DeleteById(id);
            return Ok(result);
        }

        protected virtual Task Validate(TEntityRequest request)
        {
            return Task.CompletedTask;
        }
    }
}
