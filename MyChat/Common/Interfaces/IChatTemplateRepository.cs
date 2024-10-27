using MyChat.Common.Model;

namespace MyChat.Common.Interfaces
{
    public interface IChatTemplateRepository
    {
        void Delete(ChatTemplate template);
        List<ChatTemplate> Fetch();
        ChatTemplate? Get(Guid identifier);
        void Save(ChatTemplate template);
        void Update(List<ChatTemplate> templateList);
    }
}