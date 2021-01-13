using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Crawler.CrawlTaskManages
{
    public interface ICrawlTask
    {
        string Url { get; }

        string TaskType { get; set; }

        int Depth { get; }

        ICrawlTask CreateNextTask(string url);

        IEnumerable<ICrawlTask> Run();
    }
}
