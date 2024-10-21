using MyChat.Model;

namespace MyChat.Data
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