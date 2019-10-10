using Dora.DynamicProxy;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Interception
{
    public class CacheInterceptor
    {
        private readonly InterceptDelegate _next;
        private readonly IMemoryCache _cache;
        private readonly MemoryCacheEntryOptions _options;
        public CacheInterceptor(InterceptDelegate next, IMemoryCache cache,
            IOptions<MemoryCacheEntryOptions> optionsAccessor)

        {

            _next = next;

            _cache = cache;

            _options = optionsAccessor.Value;

        }
        public async Task InvokeAsync(InvocationContext context)
        {
            var key = new CacheKey(context.Method, context.Arguments);
            if (_cache.TryGetValue(key, out object value))
                context.ReturnValue = value;
            else
            {
                await _next(context);
                _cache.Set(key, context.ReturnValue, _options);
            }
        }
        private class CacheKey
        {
            public MethodBase Method { get; }
            public object[] InputArguments { get; }
            public CacheKey(MethodBase method, object[] arguments)
            {
                Method = method;
                InputArguments = arguments;
            }
            public override bool Equals(object obj)
            {
                if (!(obj is CacheKey another))
                    return false;
                if (!Method.Equals(another.Method))
                    return false;
                for (int index = 0; index < InputArguments.Length; index++)
                {
                    var argument1 = InputArguments[index];
                    var argument2 = another.InputArguments[index];
                    if (argument1 == null && argument2 == null)
                    {
                        continue;
                    }

                    if (argument1 == null || argument2 == null)
                    {
                        return false;
                    }

                    if (!argument1.Equals(argument2))
                    {
                        return false;
                    }
                }
                return true;
            }
            public override int GetHashCode()
            {
                int hashCode = Method.GetHashCode();
                foreach (var argument in InputArguments)
                {
                    hashCode = hashCode ^ argument.GetHashCode();
                }
                return hashCode;
            }
        }
    }
}
