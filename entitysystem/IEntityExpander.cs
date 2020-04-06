using System.Collections.Generic;
using System.Threading.Tasks;

namespace Randomous.EntitySystem
{
    public interface IEntityExpander
    {
        //void AddValuesEasy(EntityPackage package, Dictionary<string, string> values); //params EntityValue[] values);

        void AddValues(EntityPackage package, params EntityValue[] values);
        void AddRelations(EntityPackage package, params EntityRelation[] relations);

        Task<List<EntityPackage>> ExpandAsync(params Entity[] entities);
    }
}