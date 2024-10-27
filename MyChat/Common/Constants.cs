using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyChat.Common
{
    public static class Constants
    {
#if DEBUG
        public const string UserAppFolderName = "MyChat_Debug";
        public const string ApplicationInstanceId = "{E026C9F8-9B5D-44E7-BA40-07C8F39E47F1}";
        public const string ApplicationPipeName = "MyChatDebugNamedPipe";
#else
        public const string UserAppFolderName = "MyChat";
        public const string ApplicationInstanceId = "{ABA1841A-9E7D-4524-897F-FA022F9EC4C7}";
        public const string ApplicationPipeName = "MyChatApplicationNamedPipe";
#endif

        public const string UserSettingsFileName = "usersettings.json";
        public const string ChatTemplatesFileName = "templates.json";
        public const string ImageMetadataFileName = "imagemetadata.json";
    }
}
