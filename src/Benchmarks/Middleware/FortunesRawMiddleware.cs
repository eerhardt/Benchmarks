// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Benchmarks.Configuration;
using Benchmarks.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using RazorSlices;

namespace Benchmarks.Middleware
{
    public class FortunesRawMiddleware
    {
        private static readonly PathString _path = new PathString(Scenarios.GetPath(s => s.DbFortunesRaw));

        private readonly RequestDelegate _next;
        private readonly HtmlEncoder _htmlEncoder;
        private readonly SliceFactory<IEnumerable<Fortune>> _fortunesFactory;

        public FortunesRawMiddleware(RequestDelegate next, HtmlEncoder htmlEncoder)
        {
            _next = next;
            _htmlEncoder = htmlEncoder;
            _fortunesFactory = RazorSlice.ResolveSliceFactory<IEnumerable<Fortune>>("/Templates/Fortunes.cshtml");
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if (httpContext.Request.Path.StartsWithSegments(_path, StringComparison.Ordinal))
            {
                var db = httpContext.RequestServices.GetService<RawDb>();
                var rows = await db.LoadFortunesRows();

                await MiddlewareHelpers.RenderFortunesHtml(rows, httpContext, _htmlEncoder, _fortunesFactory);

                return;
            }

            await _next(httpContext);
        }
    }

    public static class FortunesRawMiddlewareExtensions
    {
        public static IApplicationBuilder UseFortunesRaw(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<FortunesRawMiddleware>();
        }
    }
}
