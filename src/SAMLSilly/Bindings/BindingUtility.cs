using SAMLSilly.Config;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Primitives;
using System.Linq;
using System.Text;

namespace SAMLSilly.Bindings
{
    /// <summary>
    /// Utility functions for use in binding implementations.
    /// </summary>
    public class BindingUtility
    {
        /// <summary>
        /// Validates the SAML20Federation configuration.
        /// </summary>
        /// <returns>True if validation passes, false otherwise</returns>
        public static bool ValidateConfiguration(Saml2Configuration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config", ErrorMessages.ConfigMissingSaml2Element);
            }

            if (config.ServiceProvider == null)
            {
                throw new ArgumentOutOfRangeException("config", ErrorMessages.ConfigMissingServiceProviderElement);
            }

            if (string.IsNullOrEmpty(config.ServiceProvider.Id))
            {
                throw new ArgumentOutOfRangeException("config", ErrorMessages.ConfigMissingServiceProviderIdAttribute);
            }

            if (config.ServiceProvider.SigningCertificate == null)
            {
                throw new ArgumentOutOfRangeException("config", ErrorMessages.ConfigMissingSigningCertificateElement);
            }

            // This will throw if no certificate or multiple certificates are found
            var certificate = config.ServiceProvider.SigningCertificate;
            if (!certificate.HasPrivateKey)
            {
                throw new ArgumentOutOfRangeException("config", ErrorMessages.ConfigSigningCertificateMissingPrivateKey);
            }

            if (config.IdentityProviders == null)
            {
                throw new ArgumentOutOfRangeException("config", ErrorMessages.ConfigMissingIdentityProvidersElement);
            }

            return true;
        }

        public static IEnumerable<KeyValuePair<string, StringValues>> QueryStringToKeyValuePair(string query)
        {
            return ParseQuery(query);
        }

        //Below code from Microsoft `Microsoft.AspNetCore.WebUtilities`
        //https://github.com/aspnet/HttpAbstractions/blob/11c8a7666091145d11c68359283c4f9268cf2dcf/src/Microsoft.AspNetCore.WebUtilities/QueryHelpers.cs#L110

        /// <summary>
        /// Parse a query string into its component key and value parts.
        /// </summary>
        /// <param name="queryString">The raw query string value, with or without the leading '?'.</param>
        /// <returns>A collection of parsed keys and values.</returns>
        internal static Dictionary<string, StringValues> ParseQuery(string queryString)
        {
            var result = ParseNullableQuery(queryString);

            if (result == null)
            {
                return new Dictionary<string, StringValues>();
            }

            return result;
        }


        /// <summary>
        /// Parse a query string into its component key and value parts.
        /// </summary>
        /// <param name="queryString">The raw query string value, with or without the leading '?'.</param>
        /// <returns>A collection of parsed keys and values, null if there are no entries.</returns>
        internal static Dictionary<string, StringValues> ParseNullableQuery(string queryString)
        {
            var accumulator = new KeyValueAccumulator();

            if (string.IsNullOrEmpty(queryString) || queryString == "?")
            {
                return null;
            }

            int scanIndex = 0;
            if (queryString[0] == '?')
            {
                scanIndex = 1;
            }

            int textLength = queryString.Length;
            int equalIndex = queryString.IndexOf('=');
            if (equalIndex == -1)
            {
                equalIndex = textLength;
            }
            while (scanIndex < textLength)
            {
                int delimiterIndex = queryString.IndexOf('&', scanIndex);
                if (delimiterIndex == -1)
                {
                    delimiterIndex = textLength;
                }
                if (equalIndex < delimiterIndex)
                {
                    while (scanIndex != equalIndex && char.IsWhiteSpace(queryString[scanIndex]))
                    {
                        ++scanIndex;
                    }
                    string name = queryString.Substring(scanIndex, equalIndex - scanIndex);
                    string value = queryString.Substring(equalIndex + 1, delimiterIndex - equalIndex - 1);
                    accumulator.Append(
                        Uri.UnescapeDataString(name.Replace('+', ' ')),
                        Uri.UnescapeDataString(value.Replace('+', ' ')));
                    equalIndex = queryString.IndexOf('=', delimiterIndex);
                    if (equalIndex == -1)
                    {
                        equalIndex = textLength;
                    }
                }
                else
                {
                    if (delimiterIndex > scanIndex)
                    {
                        accumulator.Append(queryString.Substring(scanIndex, delimiterIndex - scanIndex), string.Empty);
                    }
                }
                scanIndex = delimiterIndex + 1;
            }

            if (!accumulator.HasValues)
            {
                return null;
            }

            return accumulator.GetResults();
        }
    }

    internal struct KeyValueAccumulator
    {
        private Dictionary<string, StringValues> _accumulator;
        private Dictionary<string, List<string>> _expandingAccumulator;

        public void Append(string key, string value)
        {
            if (_accumulator == null)
            {
                _accumulator = new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase);
            }

            StringValues values;
            if (_accumulator.TryGetValue(key, out values))
            {
                if (values.Count == 0)
                {
                    // Marker entry for this key to indicate entry already in expanding list dictionary
                    _expandingAccumulator[key].Add(value);
                }
                else if (values.Count == 1)
                {
                    // Second value for this key
                    _accumulator[key] = new string[] { values[0], value };
                }
                else
                {
                    // Third value for this key
                    // Add zero count entry and move to data to expanding list dictionary
                    _accumulator[key] = default(StringValues);

                    if (_expandingAccumulator == null)
                    {
                        _expandingAccumulator = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
                    }

                    // Already 3 entries so use starting allocated as 8; then use List's expansion mechanism for more
                    var list = new List<string>(8);
                    var array = values.ToArray();

                    list.Add(array[0]);
                    list.Add(array[1]);
                    list.Add(value);

                    _expandingAccumulator[key] = list;
                }
            }
            else
            {
                // First value for this key
                _accumulator[key] = new StringValues(value);
            }

            ValueCount++;
        }

        public bool HasValues => ValueCount > 0;

        public int KeyCount => _accumulator?.Count ?? 0;

        public int ValueCount { get; private set; }

        public Dictionary<string, StringValues> GetResults()
        {
            if (_expandingAccumulator != null)
            {
                // Coalesce count 3+ multi-value entries into _accumulator dictionary
                foreach (var entry in _expandingAccumulator)
                {
                    _accumulator[entry.Key] = new StringValues(entry.Value.ToArray());
                }
            }

            return _accumulator ?? new Dictionary<string, StringValues>(0, StringComparer.OrdinalIgnoreCase);
        }
    }
}