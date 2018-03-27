using System;
using System.Collections.Generic;

namespace UniversalPlayground.DataModels
{
    public class Section
    {
        private string _name;

        public Section(IList<NotebookEntry> notebookEntries)
        {
            NotebookEntries = notebookEntries;
        }

        public string Name
        {
            get => _name;
            set => _name = value ?? throw new ArgumentNullException("Name", "Name value cannot be null");
        }

        public IList<NotebookEntry> NotebookEntries { get; }
    }
}