using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FlitBit.Dapper.Infrastructure.Dto
{
    
    public interface IConstructorFinder
    {
        ConstructorInfo GetConstructor(Type type);
    }

    public class DefaultConstructorFinder
    {
        
    }
}
