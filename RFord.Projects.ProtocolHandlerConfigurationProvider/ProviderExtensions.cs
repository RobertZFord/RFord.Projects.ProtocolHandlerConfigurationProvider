using Microsoft.Extensions.Configuration;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Web;

namespace RFord.Projects.ProtocolHandlerConfigurationProvider
{
    public static class ProviderExtensions
    {
        /// <summary>
        /// Register the protocol handler configuration source.
        /// </summary>
        /// <param name="builder">The <seealso cref="IConfigurationBuilder"/> configuration builder.</param>
        /// <param name="protocolHandlerData">The URL provided from the calling process.</param>
        /// <param name="keyMappings">Dictionary for remapping URL components to user-specified keys.</param>
        /// <param name="segmentNames">The names that will be applied to the route segments.</param>
        /// <returns></returns>
        public static IConfigurationBuilder AddProtocolHandler(
            this IConfigurationBuilder builder,
            string protocolHandlerData,
            IDictionary<string, string> keyMappings,
            params string[] segmentNames
        )
        {
            builder.Add<ProtocolHandlerConfigurationSource>(
                source =>
                {
                    source.SetSource(protocolHandlerData);
                    source.SetSegmentNames(segmentNames);
                    source.SetRemapping(keyMappings);
                }
            );
            return builder;
        }

        /// <summary>
        /// Register the protocol handler configuration source.
        /// </summary>
        /// <param name="builder">The <seealso cref="IConfigurationBuilder"/> configuration builder.</param>
        /// <param name="protocolHandlerData">The URL provided from the calling process.</param>
        /// <param name="segmentNames">The names that will be applied to the route segments.</param>
        /// <returns></returns>
        public static IConfigurationBuilder AddProtocolHandler(
            this IConfigurationBuilder builder,
            string protocolHandlerData,
            params string[] segmentNames
        )
            => AddProtocolHandler(
                builder,
                protocolHandlerData,
                new Dictionary<string, string> { },
                segmentNames
            );
    }
}
