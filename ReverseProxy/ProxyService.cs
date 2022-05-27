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
            // var handler = new HttpClientHandler { AllowAutoRedirect = false, UseCookies = false};
            var handler = new SocketsHttpHandler {
                UseProxy = false,
                AllowAutoRedirect = false,
                AutomaticDecompression = DecompressionMethods.None,
                UseCookies = false,
                SslOptions = sslOptions,
                EnableMultipleHttp2Connections = true,
                // NOTE: MaxResponseHeadersLength = 64, which means up to 64 KB of headers are allowed by default as of .NET Core 3.1.
            };

            // handler.ClientCertificateOptions = ClientCertificateOption.Manual;
            // handler.ServerCertificateCustomValidationCallback = 
            //     (httpRequestMessage, cert, cetChain, policyErrors) =>
            // {
            //     return true;
            // };

            Client = new HttpClient(Options.MessageHandler ?? handler, disposeHandler: true);
            // Client.DefaultRequestVersion = new Version(2, 0);
        }

        public SharedProxyOptions Options { get; private set; }
        internal HttpClient Client { get; private set; }
    }
}

