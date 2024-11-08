using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyChat.Data.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class DataStoreRepositoryAttribute : Attribute
    {
        public DataStoreRepositoryAttribute(string partitionKey, string collectionName, string repositoryName = "")
        {
            PartitionKey = partitionKey;
            CollectionName = collectionName;
            RepositoryName = repositoryName;
        }

        public string PartitionKey { get; }
        public string CollectionName { get; }
        public string RepositoryName { get; }
    }
}
