using Microsoft.AspNetCore.DataProtection;

namespace J4JSoftware.DependencyInjection
{
    public interface IDataProtection
    {
        IDataProtector Protector { get; }
    }
}