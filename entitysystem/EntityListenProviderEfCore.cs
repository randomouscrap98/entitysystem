//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Logging;
//
//namespace entitysystem
//{
//    public class EntityListenProviderEfCore : EntityProviderEfCore, IEntityListenProvider
//    {
//        public EntityListenProviderEfCore(ILogger<EntityListenProviderEfCore> logger, IEntitySearcher searcher, DbContext context) : 
//            base(logger, searcher, context)
//        {
//
//        }
//
//        public override async Task WriteEntityRelationsAsync(IEnumerable<EntityRelation> relations)
//        {
//            await base.WriteEntityRelationsAsync(relations);
//        }
//
//        public Task<EntityRelation> ListenPrimaryRelationAsync(long id, TimeSpan maxWait)
//        {
//            throw new NotImplementedException();
//        }
//
//        public Task<EntityRelation> ListenSecondaryRelationAsync(long id, TimeSpan maxWait)
//        {
//            throw new NotImplementedException();
//        }
//    }
//}