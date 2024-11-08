namespace MyChat.Data.Interfaces
{
    public interface IDataStoreClient
    {
        Task<IDataStoreCollection> GetCollectionAsync(string databaseName);
    }
}
