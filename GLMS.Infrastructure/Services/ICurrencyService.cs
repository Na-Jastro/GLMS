using System;
using System.Collections.Generic;
using System.Text;

namespace GLMS.Infrastructure.Services
{
    public interface ICurrencyService
    {
        Task<decimal> GetUsdToZarRate();
    }
}
