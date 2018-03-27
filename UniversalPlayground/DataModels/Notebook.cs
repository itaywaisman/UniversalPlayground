using System.Collections.Generic;

namespace UniversalPlayground.DataModels
{
    public class Notebook
    {
        public string Name { get; set; }

        public IReadOnlyList<Section> Sections { get; set; }
    }
}