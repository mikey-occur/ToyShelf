using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.IServices
{
    public interface IProductBroadcaster
    {
        Task NotifyProductSelectedAsync(Guid productId);
    }
}
