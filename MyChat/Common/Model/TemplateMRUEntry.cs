using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyChat.Common.Model
{
    public class TemplateMRUEntry
    {
        public Guid Identifier { get; set; }
        public string Name { get; set; }

        public TemplateMRUEntry(Guid identifier, string name)
        {
            Identifier = identifier;
            Name = name;
        }
    }
}
