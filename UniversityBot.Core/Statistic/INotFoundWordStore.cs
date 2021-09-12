using System.Collections.Generic;
using System.Threading.Tasks;

namespace UniversityBot.Core.Statistic
{
    public interface INotFoundWordStore
    {
        Task Save(NotFoundStoreEntity notFoundStoreEntity);
    }
}