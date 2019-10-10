using Dora.Interception;
using System;
using System.Collections.Generic;
using System.Text;

namespace Interception
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CacheReturnValueAttribute : InterceptorAttribute
    {
        public override void Use(IInterceptorChainBuilder builder)
        {
            builder.Use<CacheInterceptor>(this.Order);
        }
    }
}
