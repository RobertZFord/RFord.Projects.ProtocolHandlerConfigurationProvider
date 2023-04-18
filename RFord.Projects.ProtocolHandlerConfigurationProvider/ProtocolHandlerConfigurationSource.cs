using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace RFord.Projects.ProtocolHandlerConfigurationProvider
{
    public class ProtocolHandlerConfigurationSource : IConfigurationSource
    {
        private string source = "";
        private string[] segmentNames = new string[0];
        private IDictionary<string, string> remapping = new Dictionary<string, string>();

        public void SetSource(string source)
        {
            this.source = source;
        }

        public void SetSegmentNames(string[] segmentNames)
        {
            this.segmentNames = segmentNames;
        }

        public void SetRemapping(IDictionary<string, string> remapping)
        {
            this.remapping = remapping;
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
            => new ProtocolHandlerConfigurationProvider(source, remapping, segmentNames);
    }
}
