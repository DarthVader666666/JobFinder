using HtmlAgilityPack;

using JobFinders.BLL.Models;

namespace JobFinders.BLL.Interfaces
{
    public interface IJobParser
    {
        Job? Parse(JobFinderSetting? setting, HtmlNode? node, string? url);
    }
}
