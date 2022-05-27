// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Proxy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Builder
{
    public static class ProxyExtensions
    {
        /// <summary>
        /// Runs proxy forwarding requests to the server specified by base uri.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="baseUri">Destination base uri</param>
        public static void RunProxy(this IApplicationBuilder app, Uri baseUri)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }
            if (baseUri == null)
            {
                throw new ArgumentNullException(nameof(baseUri));
            }

            var options = new ProxyOptions
            {
                Scheme = baseUri.Scheme,
                Host = new HostString(baseUri.Authority),
                PathBase = baseUri.AbsolutePath,
                AppendQuery = new QueryString(baseUri.Query)
            };
            app.UseMiddleware<ProxyMiddleware>(Options.Create(options));
        }

        public static void RunProxy(this IApplicationBuilder app, string[] uris)
        {
            var options = new List<IOptions<ProxyOptions>>();
            foreach (string uriString in uris) {
                try
                {
                    var baseUri = new Uri(uriString);
                    if (app == null)
                    {
                        throw new ArgumentNullException(nameof(app));
                    }
                    if (baseUri == null)
                    {
                        throw new ArgumentNullException(nameof(baseUri));
                    }

                    var option = new ProxyOptions
                    {
                        Scheme = baseUri.Scheme,
                        Host = new HostString(baseUri.Authority),
                        PathBase = baseUri.AbsolutePath,
                        AppendQuery = new QueryString(baseUri.Query)
                    };
                    options.Add(Options.Create(option));
                }
                catch (Exception e)
                {
                    break;
                }
            }
            app.UseMiddleware<ProxyMiddleware>(options);
        }

        /// <summary>
        /// Runs proxy forwarding requests to the server specified by options.
        /// </summary>
        /// <param name="app"></param>
        public static void RunProxy(this IApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            app.UseMiddleware<ProxyMiddleware>();
        }

        /// <summary>
        /// Runs proxy forwarding requests to the server specified by options.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="options">Proxy options</param>
        public static void RunProxy(this IApplicationBuilder app, ProxyOptions options)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            app.UseMiddleware<ProxyMiddleware>(Options.Create(options));
        }

        /// <summary>
        /// Forwards current request to the specified destination uri.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="destinationUri">Destination Uri</param>
        public static async Task ProxyRequest(this HttpContext context, Uri[] destinationUris)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            foreach (var destinationUri in destinationUris)
            {
                if (destinationUri == null)
                {
                    throw new ArgumentNullException(nameof(destinationUri));
                }
            }

            var proxyService = context.RequestServices.GetRequiredService<ProxyService>();

            var sendTasks = new List<Task<HttpResponseMessage>>();
            foreach (var destinationUri in destinationUris)
            {
                using (var requestMessage = context.CreateProxyHttpRequest(destinationUri))
                {
                    var sendTask = context.SendProxyHttpRequest(requestMessage);
                    sendTasks.Add(sendTask);
                }
            }
            var responseMessages = await Task.WhenAll(sendTasks);
            var statusCode = System.Net.HttpStatusCode.OK;
            foreach (var responseMessage in responseMessages)
            {
                if (responseMessage.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    statusCode = responseMessage.StatusCode;
                }
            }
            var bytes = statusCode == System.Net.HttpStatusCode.OK ? Encoding.UTF8.GetBytes("Hello World") : Encoding.UTF8.GetBytes("Goodbye World");
            context.Response.StatusCode = (int)statusCode;
            await context.Response.Body.WriteAsync(bytes, 0, bytes.Length);
            // await context.CopyProxyHttpResponse(responseMessage);
            await context.Response.CompleteAsync();
        }
    }
}

