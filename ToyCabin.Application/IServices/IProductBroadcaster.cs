using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyCabin.Application.IServices
{
    public interface IProductBroadcaster
    {
        Task NotifyProductSelectedAsync(Guid productId);
    }
}
