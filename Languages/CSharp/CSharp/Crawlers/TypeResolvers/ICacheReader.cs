using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharp.Crawlers.TypeResolvers
{
    public interface ICacheReader
    {
        void ResolveMatchingType(params PartialType[] types);
    }
}
