﻿using KeeCloud.WebRequests;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KeeCloud
{
    public class ProviderRegistry
    {
        static readonly object registrationSync = new object();
        static bool isRegistered = false;

        public static IEnumerable<ProviderItem> SupportedWebRequests
        {
            get
            {
                // add a new yield return with a protocol prefix and a delegate to create for each supported handler in the plugin
                yield return new ProviderItem("dropbox", () => new KeeCloud.Providers.Dropbox.DropboxProvider());
                yield return new ProviderItem("s3", () => new KeeCloud.Providers.Amazon.AmazonS3Provider());


                // this is a dummy provider used mainly for testing the UI of the credential configuration wizard
                // as well as the base web request logic
#if DUMMY
                yield return new ProviderItem("dummy", () => new KeeCloud.Providers.Dummy.DummyProvider());
#endif
            }
        }

        /// <summary>
        /// Register all supported prefixes with the .net framework
        /// </summary>
        public static void RegisterAllIFRequired()
        {
            lock (registrationSync)
            {
                if (!isRegistered)
                {
                    isRegistered = true;
                    foreach (var supported in SupportedWebRequests)
                    {
                        ProviderWebRequest.RegisterPrefix(supported.Protocol + ":", ProviderWebRequestCreator.Instance);
                    }
                }
            }
        }

        /// <summary>
        /// Get a specific provider for the URI provided
        /// </summary>
        /// <param name="uri">URI for which to find a provider</param>
        /// <returns>The matching provider</returns>
        internal static ProviderItem GetProviderForUri(Uri uri)
        {
            ProviderItem item;

            if (!TryGetProviderForScheme(uri.Scheme, out item))
                throw new ArgumentException("uri");

            return item;
        }

        /// <summary>
        /// Get a specific provider for the scheme or protocol provided
        /// </summary>
        /// <param name="uri">URI for which to find a provider</param>
        /// <returns>The matching provider</returns>
        internal static bool TryGetProviderForScheme(string scheme, out ProviderItem provider)
        {
            provider = ProviderRegistry.SupportedWebRequests
                .FirstOrDefault(w => w.Protocol.ToLowerInvariant() == scheme.ToLowerInvariant());

            return (provider != null);
        }
    }
}
