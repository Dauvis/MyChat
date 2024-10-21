using MyChat.Model;
using MyChat.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MyChat.Data
{
    public class ChatTemplateRepository : IChatTemplateRepository
    {
        private string _chatTemplatesPath = "";

        public List<ChatTemplate> Fetch()
        {
            string templatesFilePath = GetChatTemplatesPath();

            if (File.Exists(templatesFilePath))
            {
                string templatesJson = File.ReadAllText(templatesFilePath);
                return JsonSerializer.Deserialize<List<ChatTemplate>>(templatesJson) ?? [];
            }

            return [];
        }

        public ChatTemplate? Get(Guid identifier)
        {
            var templates = Fetch();

            var template = templates.Where(t => t.Identifier == identifier).FirstOrDefault();

            return template;
        }

        public void Save(ChatTemplate template)
        {
            var templateList = Fetch();
            var existingTemplate = templateList.Where(t => t.Identifier == template.Identifier).FirstOrDefault();

            if (existingTemplate is not null)
            {
                templateList.Remove(existingTemplate);
            }

            templateList.Add(template);
            Update(templateList);
        }

        public void Delete(ChatTemplate template)
        {
            var templateList = Fetch();
            var existingTemplate = templateList.Where(t => t.Identifier == template.Identifier).FirstOrDefault();

            if (existingTemplate is not null)
            {
                templateList.Remove(existingTemplate);
            }

            Update(templateList);
        }

        public void Update(List<ChatTemplate> templateList)
        {
            string templatesFilePath = GetChatTemplatesPath();
            string templatesJson = JsonSerializer.Serialize(templateList);
            File.WriteAllText(templatesFilePath, templatesJson);
        }

        private string GetChatTemplatesPath()
        {
            if (string.IsNullOrEmpty(_chatTemplatesPath))
            {
                string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string userAppFolder = Path.Combine(appDataPath, Constants.UserAppFolderName);
                Directory.CreateDirectory(userAppFolder);
                _chatTemplatesPath = Path.Combine(userAppFolder, Constants.ChatTemplatesFileName);
            }

            return _chatTemplatesPath;
        }
    }
}
