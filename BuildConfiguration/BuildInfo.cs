using System;

namespace BuildConfiguration
{
    public class BuildInfo
    {
        public BuildInfo() {}
        public BuildInfo(DateTimeOffset date, string buildId)
        {
            Date = date;
            BuildId = buildId;
        }

        public DateTimeOffset Date { get; set; }
        public string BuildId { get; set; }
    }
}
