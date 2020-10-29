﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Toolkit.Uwp.Notifications
{
    /// <summary>
    /// A portable string serializer/deserializer for .NET.
    /// </summary>
    public sealed class ToastArguments : IEnumerable<KeyValuePair<string, string>>
    {
        private Dictionary<string, string> _dictionary = new Dictionary<string, string>();

#if !WINRT
        /// <summary>
        /// Gets the value of the specified key. Throws <see cref="KeyNotFoundException"/> if the key could not be found.
        /// </summary>
        /// <param name="key">The key to find.</param>
        /// <returns>The value of the specified key.</returns>
        public string this[string key]
        {
            get
            {
                if (key == null)
                {
                    throw new ArgumentNullException(nameof(key));
                }

                if (TryGetValue(key, out string value))
                {
                    return value;
                }

                throw new KeyNotFoundException($"A key with name '{key}' could not be found.");
            }

            set
            {
                if (key == null)
                {
                    throw new ArgumentNullException(nameof(key));
                }

                _dictionary[key] = value;
            }
        }
#endif

        /// <summary>
        /// Attempts to get the value of the specified key. If no key exists, returns false.
        /// </summary>
        /// <param name="key">The key to find.</param>
        /// <param name="value">The key's value will be written here if found.</param>
        /// <returns>True if found the key and set the value, otherwise false.</returns>
#if WINRT
        [return: System.Runtime.InteropServices.WindowsRuntime.ReturnValueName("found")]
#endif
        public bool TryGetValue(string key, out string value)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            return _dictionary.TryGetValue(key, out value);
        }

        /// <summary>
        /// Gets the number of key/value pairs contained in the toast arguments.
        /// </summary>
        public int Count => _dictionary.Count;

        /// <summary>
        /// Adds a key (without a value) to the arguments. If the key already exists, throws an exception.
        /// </summary>
        /// <param name="key">The name of the parameter.</param>
        public void Add(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            _dictionary.Add(key, null);
        }

        /// <summary>
        /// Adds a key and optional value to the arguments. If the key already exists, throws an exception.
        /// </summary>
        /// <param name="key">The name of the parameter.</param>
        /// <param name="value">The optional value of the key.</param>
        public void Add(string key, string value)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            _dictionary.Add(key, value);
        }

        /// <summary>
        /// Sets a key. If there is an existing key, it is replaced.
        /// </summary>
        /// <param name="key">The key.</param>
        public void Set(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            _dictionary[key] = null;
        }

        /// <summary>
        /// Sets a key and optional value. If there is an existing key, it is replaced.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The optional value of the parameter.</param>
        public void Set(string key, string value)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            _dictionary[key] = value;
        }

        /// <summary>
        /// Determines if the specified key is present.
        /// </summary>
        /// <param name="key">The key to look for.</param>
        /// <returns>True if the key is present, otherwise false.</returns>
        public bool Contains(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            return _dictionary.ContainsKey(key);
        }

        /// <summary>
        /// Determines if specified key and value are present.
        /// </summary>
        /// <param name="key">The key to look for.</param>
        /// <param name="value">The value to look for when the key has been matched.</param>
        /// <returns>True if the key and value were found, else false.</returns>
#if WINRT
        [return: System.Runtime.InteropServices.WindowsRuntime.ReturnValueName("doesContain")]
#endif
        public bool Contains(string key, string value)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            return _dictionary.TryGetValue(key, out string actualValue) && actualValue == value;
        }

        /// <summary>
        /// Removes the specified key and its associated value.
        /// </summary>
        /// <param name="key">The key to remove.</param>
        /// <returns>True if the key was removed, else false.</returns>
        public bool Remove(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            return _dictionary.Remove(key);
        }

        private static string UrlEncode(string str)
        {
            return Uri.EscapeDataString(str)

                // It incorrectly encodes spaces as %20, should use +
                .Replace("%20", "+");
        }

        private static string UrlDecode(string str)
        {
            // Doesn't handle decoding the +, so we manually do that
            return Uri.UnescapeDataString(str.Replace('+', ' '));
        }

        /// <summary>
        /// Parses a string that was generated using ToastArguments into a <see cref="ToastArguments"/> object.
        /// </summary>
        /// <param name="toastArgumentsStr">The toast arguments string to deserialize.</param>
        /// <returns>The parsed toast arguments.</returns>
        public static ToastArguments Parse(string toastArgumentsStr)
        {
            if (string.IsNullOrWhiteSpace(toastArgumentsStr))
            {
                return new ToastArguments();
            }

            string[] pairs = toastArgumentsStr.Split('&');

            ToastArguments answer = new ToastArguments();

            foreach (string pair in pairs)
            {
                string name;
                string value;

                int indexOfEquals = pair.IndexOf('=');

                if (indexOfEquals == -1)
                {
                    name = UrlDecode(pair);
                    value = null;
                }
                else
                {
                    name = UrlDecode(pair.Substring(0, indexOfEquals));
                    value = UrlDecode(pair.Substring(indexOfEquals + 1));
                }

                answer.Add(name, value);
            }

            return answer;
        }

        /// <summary>
        /// Serializes the key-value pairs into a string that can be used within a toast notification.
        /// </summary>
        /// <returns>A string that can be used within a toast notification.</returns>
        public sealed override string ToString()
        {
            return string.Join("&", this.Select(pair =>

                    // Key
                    UrlEncode(pair.Key) +

                    // Write value if not null
                    ((pair.Value == null) ? string.Empty : ("=" + UrlEncode(pair.Value)))));
        }

        /// <summary>
        /// Gets an enumerator to enumerate the arguments. Note that order of the arguments is NOT preserved.
        /// </summary>
        /// <returns>An enumeartor of the key/value pairs.</returns>
        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }

        /// <summary>
        /// Gets an enumerator to enumerate the query string parameters.
        /// </summary>
        /// <returns>An enumeartor of the key/value pairs.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
