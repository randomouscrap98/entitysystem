using System.Collections.Generic;
using System.Threading.Tasks;

namespace entitysystem 
{
    public interface IEntityListenProvider : IEntityProvider
    {
        Task<EntityRelation> ListenPrimaryRelationAsync(long id);
        Task<EntityRelation> ListenSecondaryRelationAsync(long id);
    }
}