
namespace MyChat.Common.Interfaces
{
    public interface IRepositoryFactory
    {
        Task<T?> CreateAsync<T>() where T : class;
    }
}
