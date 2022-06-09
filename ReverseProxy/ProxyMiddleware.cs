// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Proxy
{
    /// <summary>
    /// Proxy Middleware
    /// </summary>
    public class ProxyMiddleware
    {
        private const int DefaultWebSocketBufferSize = 4096;

        private readonly RequestDelegate _next;
        private readonly ProxyOptions _options;

        private static readonly string[] NotForwardedWebSocketHeaders = new[] { "Connection", "Host", "Upgrade", "Sec-WebSocket-Key", "Sec-WebSocket-Version" };

        public ProxyMiddleware(RequestDelegate next, IOptions<ProxyOptions> options)
        {
            if (next == null)
            {
                throw new ArgumentNullException(nameof(next));
            }
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }
            if (options.Value.Scheme == null)
            {
                throw new ArgumentException("Options parameter must specify scheme.", nameof(options));
            }

            _next = next;
            _options = options.Value;
        }

        async public Task Invoke(HttpContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var buffer = new byte[Convert.ToInt32(context.Request.ContentLength)];
            await context.Request.Body.ReadAsync(buffer, 0, buffer.Length);
            var content = Encoding.UTF8.GetString(buffer);
            var uriStrings = content.Split(";");

            var uris = new Uri[uriStrings.Length];
            int i = 0;
            foreach (var uriString in uriStrings) { 
                var uri = new Uri(UriHelper.BuildAbsolute(_options.Scheme, new HostString(uriString), _options.PathBase, context.Request.Path, context.Request.QueryString.Add(_options.AppendQuery)));
                uris[i++] = uri;
            }

            // broadcast request
            await context.ProxyRequest(uris);

            // reponse before broadcasting
            var bytes = Encoding.UTF8.GetBytes("Hello World");
            context.Response.StatusCode = StatusCodes.Status200OK;
            await context.Response.Body.WriteAsync(bytes, 0, bytes.Length);
            await context.Response.CompleteAsync();

        }
    }
}
