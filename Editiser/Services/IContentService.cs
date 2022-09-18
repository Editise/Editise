using System;
namespace Editiser.Services
{
	public interface IContentService
	{
        public Dictionary<string, Content> Blocks { get; set; }
        bool DevMode { get; set; }

        public void ClearCache();
        public bool BlockPresent(List<(string Docname, string BlockName)> testBlocks);
    }
}

