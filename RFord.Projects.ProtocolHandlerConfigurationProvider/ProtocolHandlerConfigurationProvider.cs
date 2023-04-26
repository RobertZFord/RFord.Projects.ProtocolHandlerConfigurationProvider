using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace RFord.Projects.ProtocolHandlerConfigurationProvider
{
    public class ProtocolHandlerConfigurationProvider : ConfigurationProvider
    {
        private readonly string _source;
        private readonly IDictionary<string, string> _remapping;
        private readonly string[] _segmentNames;
        private readonly IDictionary<string, string?> _parts;

        public const string ConfigurationPrefix = "ProtocolHandler";

        public ProtocolHandlerConfigurationProvider(
            string source,
            IDictionary<string, string> remapping,
            string[] segmentNames
        )
        {
            _source = source;
            _remapping = remapping;
            _segmentNames = segmentNames;
            _parts = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        }

        public override void Load()
        {
            // the base object .Load() is actually a NOP, so we don't bother calling it here.

            if (!Uri.TryCreate(
                uriString: _source,
                uriKind: UriKind.Absolute,
                result: out Uri parsedSource
            ))
            {
                // throw new ArgumentOutOfRangeException(paramName: nameof(_source));
                return;
            }
            AddOrUpdatePart(nameof(parsedSource.OriginalString), parsedSource.OriginalString);

            AddOrUpdatePart(nameof(parsedSource.Scheme), parsedSource.Scheme);
            if (!string.IsNullOrWhiteSpace(parsedSource.UserInfo))
            {
                AddOrUpdatePart(nameof(parsedSource.UserInfo), parsedSource.UserInfo);
            }
            if (!string.IsNullOrWhiteSpace(parsedSource.Host))
            {
                AddOrUpdatePart(nameof(parsedSource.Host), parsedSource.Host);
            }

            if (parsedSource.Port != -1)
            {
                AddOrUpdatePart(nameof(parsedSource.Port), parsedSource.Port.ToString());
            }

            // if the user provided names, let's use them
            // but if the user didn't, and we do have a path, we should give it a default Path key
            Queue<string> segmentNameQueue = new Queue<string>(
                _segmentNames.Length == 0 && parsedSource.Segments.Length == 1 ? new string[] { "Path" } : _segmentNames
            );
            foreach (string segment in processSegments(parsedSource.Segments))
            {
                // if segmentnamequeue is empty
                if (segmentNameQueue.Count == 0)
                {
                    throw new InvalidOperationException("Segment name and argument count mismatch!");
                }
                AddOrUpdatePart(segmentNameQueue.Dequeue(), segment);
            }

            // if segment name queue is populated
            if (segmentNameQueue.Count > 0)
            {
                throw new InvalidOperationException("Segment name and argument count mismatch!");
            }

            //// experimental hierarchical segments
            //// this would let us bind X segments to an array in some options object
            //int segmentCount = 0;
            //foreach (string segment in processSegments(parsedSource.Segments))
            //{
            //    _parts.Add(formattedKeyName($"Segments:{segmentCount}"), segment);
            //    segmentCount++;
            //}

            string queryString = parsedSource.Query.TrimStart('?');
            foreach (string queryPart in queryString.Split('&', StringSplitOptions.RemoveEmptyEntries))
            {
                int separator = queryPart.IndexOf('=');

                string key;
                string value;
                if (separator != -1)
                {
                    key = queryPart.Substring(0, separator);
                    value = queryPart.Substring(separator + 1);
                }
                else
                {
                    // if there's no name in the query KVP, what should we do?
                    // just name it "Query"?  Leave it unnamed?  not sure.
                    key = "Query";
                    value = queryPart;
                }

                AddOrUpdatePart(key, value);
            }

            string fragment = parsedSource.Fragment.TrimStart('#');

            if (!string.IsNullOrWhiteSpace(fragment))
            {
                AddOrUpdatePart(nameof(parsedSource.Fragment), fragment);
            }

            //  ?foxtrot=golf=aaa&hotel=india
            //      {foxtrot=golf%3daaa&hotel=india}
            //      .GetValues("foxtrot") => "golf%3daaa"
            //  ?foxtrot=golf&test&hotel=india
            //      .GetValues(null) => "test"
            //      .GetValues("foxtrot") => "golf"
            //  so just &test& is a 'null' key with a 'test' value

            Data = _parts;
        }

        private void AddOrUpdatePart(string key, string value)
        {
            string processedKey = processKey(key);

            if (_parts.ContainsKey(processedKey))
            {
                _parts[processedKey] = value;
            }
            else
            {
                _parts.Add(processedKey, value);
            }
        }

        private string processKey(string unprocessedKey)
        {
            // do remapping and key processing in here
            string processedKey = unprocessedKey;

            if (_remapping.ContainsKey(unprocessedKey))
            {
                processedKey = _remapping[unprocessedKey];
            }

            processedKey = applyConfigurationPrefix(processedKey);

            return processedKey;
        }

        // we do this up front because the TryGet with the full path DOES get
        // called, but the GetChildKeys with the configuration prefix as the
        // parent path has not been called yet, and this makes the call that
        // does occur slightly less computationally expensive
        private string applyConfigurationPrefix(string key)
            => string.Format(
                    string.IsNullOrWhiteSpace(key) ? "{0}" : "{0}:{1}",
                    ConfigurationPrefix,
                    key
                );

        /*
        private string stripConfigurationPrefix(string s)
        {
            if (s.StartsWith(ConfigurationPrefix))
            {
                return s.Replace(ConfigurationPrefix, "").TrimStart(':');
            }
            return s;
        }
        */

        private IEnumerable<string> processSegments(string[] segments)
        {
            foreach (var segment in segments)
            {
                string trimmed = segment.TrimEnd('/');

                if (string.IsNullOrWhiteSpace(trimmed))
                {
                    continue;
                }

                yield return trimmed;
            }
        }
    }
}
