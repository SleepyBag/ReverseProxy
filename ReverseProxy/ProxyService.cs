// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Proxy
{
    public class ProxyService
    {
        public ProxyService(IOptions<SharedProxyOptions> options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            Options = options.Value;

            var sslOptions = new SslClientAuthenticationOptions {
                // Leave certs unvalidated for debugging
                RemoteCertificateValidationCallback = delegate { return true; },
            };

            var handler = new SocketsHttpHandler {
                UseProxy = false,
                AllowAutoRedirect = false,
                AutomaticDecompression = DecompressionMethods.None,
                UseCookies = false,
                SslOptions = sslOptions,
                MaxConnectionsPerServer = 10,
                EnableMultipleHttp2Connections = true,
            };

            Client = new HttpMessageInvoker(Options.MessageHandler ?? handler, disposeHandler: true);
        }

        public SharedProxyOptions Options { get; private set; }
        internal HttpMessageInvoker Client { get; private set; }
    }
}

