using MyChat.Data.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyChat.Data.Interfaces
{
    public interface IDataStoreCollection
    {
        Task<DataStoreRepository<T>> GetRepositoryAsync<T>() where T : DataStoreRepository<T>;
    }
}
