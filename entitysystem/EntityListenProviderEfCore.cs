using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace entitysystem
{
    public class EntityListenProviderEfCore : EntityProviderEfCore, IEntityListenProvider
    {
        public EntityListenProviderEfCore(ILogger<EntityListenProviderEfCore> logger, IEntitySearcher searcher, DbContext context) : 
            base(logger, searcher, context)
        {

        }

        public Task<EntityRelation> ListenPrimaryRelationAsync(long id)
        {
            throw new NotImplementedException();
        }

        public Task<EntityRelation> ListenSecondaryRelationAsync(long id)
        {
            throw new NotImplementedException();
        }
    }
}