using System;
using System.Reflection;

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
