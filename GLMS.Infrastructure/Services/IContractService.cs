using System;
using System.Collections.Generic;
using System.Text;

namespace GLMS.Infrastructure.Services
{
    public interface IContractService
    {
        Task AutoUpdateExpiryAsync();
    }
}
