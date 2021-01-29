using System;
using Microsoft.AspNetCore.DataProtection;

namespace J4JSoftware.DependencyInjection
{
    public class DataProtection : IDataProtection
    {
        private readonly IDataProtectionProvider _provider;

        private IDataProtector? _protector;

        public DataProtection( IDataProtectionProvider provider )
        {
            _provider = provider;
        }

        public string Purpose { get; set; } = Guid.NewGuid().ToString();

        public IDataProtector Protector
        {
            get
            {
                _protector ??= _provider.CreateProtector( Purpose );

                return _protector;
            }
        }
    }
}
