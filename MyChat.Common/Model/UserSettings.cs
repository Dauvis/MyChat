
using System.Drawing;

namespace MyChat.Common.Model
{
    public class UserSettings
    {
        public string DefaultInstructions { get; set; } = "";
        public string SelectedChatModel { get; set; } = "";
        public string DefaultTone { get; set; } = "";
        public List<string> LastOpenFiles { get; set; } = [];
        public MainWindowInfo MainWindow { get; set; } = new();
        public ImageToolWindowInfo ImageToolWindow { get; set; } = new();
        public bool IsTemplateTreeView { get; set; } = true;
        public List<TemplateMRUEntry> RecentTemplates { get; set; } = [];

        public void UpdateTemplateMRU(ChatTemplate template)
        {
            var existing = RecentTemplates.Where(r => r.Identifier == template.Identifier).FirstOrDefault();

            if (existing is not null)
            {
                RecentTemplates.Remove(existing);
            }

            RecentTemplates.Insert(0, new(template.Identifier, template.Name));

            while (RecentTemplates.Count > 10)
            {
                RecentTemplates.RemoveAt(10);
            }
        }

        public void UpdateTemplateMRU(Guid templateId, List<ChatTemplate> templates)
        {
            var existing = RecentTemplates.Where(r => r.Identifier == templateId).FirstOrDefault();

            if (existing is not null)
            {
                RecentTemplates.Remove(existing);
            }

            var template = templates.Where(t => t.Identifier == templateId).FirstOrDefault();

            if (template is not null)
            {
                RecentTemplates.Insert(0, new(template.Identifier, template.Name));
            }

            while (RecentTemplates.Count > 10)
            {
                RecentTemplates.RemoveAt(10);
            }
        }

        public class MainWindowInfo
        {
            public Rectangle Rectangle { get; set; }
            public double ChatColumnWidth { get; set; }
            public double MessageColumnWidth { get; set; }
        }

        public class ImageToolWindowInfo
        {
            public Rectangle Rectangle { get; set; }
            public double PromptColumnWidth { get; set; }
        }
    }
}
