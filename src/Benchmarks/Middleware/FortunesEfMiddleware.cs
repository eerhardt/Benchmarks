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
    public class FortunesEfMiddleware
    {
        private static readonly PathString _path = new PathString(Scenarios.GetPath(s => s.DbFortunesEf));

        private readonly RequestDelegate _next;
        private readonly HtmlEncoder _htmlEncoder;
        private readonly SliceFactory<IEnumerable<Fortune>> _fortunesFactory;

        public FortunesEfMiddleware(RequestDelegate next, HtmlEncoder htmlEncoder)
        {
            _next = next;
            _htmlEncoder = htmlEncoder;
            _fortunesFactory = RazorSlice.ResolveSliceFactory<IEnumerable<Fortune>>("/Templates/Fortunes.cshtml");
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if (httpContext.Request.Path.StartsWithSegments(_path, StringComparison.Ordinal))
            {
                var db = httpContext.RequestServices.GetService<EfDb>();
                var rows = await db.LoadFortunesRows();

                await MiddlewareHelpers.RenderFortunesHtml(rows, httpContext, _htmlEncoder, _fortunesFactory);

                return;
            }

            await _next(httpContext);
        }
    }

    public static class FortunesEfMiddlewareExtensions
    {
        public static IApplicationBuilder UseFortunesEf(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<FortunesEfMiddleware>();
        }
    }
}