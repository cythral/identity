using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

using Brighid.Identity.Applications;
using Brighid.Identity.Roles;
using Brighid.Identity.Users;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

#pragma warning disable IDE0060, CA1801, CA1819

namespace Brighid.Identity
{
    public class EntityController<TEntity, TPrimaryKey, TRepository> : Controller
        where TEntity : class
        where TRepository : IRepository<TEntity, TPrimaryKey>
    {
        private readonly TRepository repository;

        protected virtual string[] Embeds => Array.Empty<string>();

        public EntityController(
            TRepository repository
        )
        {
            this.repository = repository;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TEntity>> GetById(TPrimaryKey id)
        {
            return await repository.GetById(id, Embeds);
        }
    }
}
