using ToyShelf.Application.Models.ShelfType.Response;
using ToyShelf.Domain.Entities;

namespace ToyShelf.Application.Models.Shelf.Response
{
    public class ShelfResponse
    {
        public Guid Id { get; set; }
        public Guid InventoryLocationId { get; set; }
		public Guid ShelfTypeId { get; set; }
		public string Code { get; set; } = string.Empty;
        public ShelfStatus Status { get; set; }
        public DateTime? AssignedAt { get; set; }
        public DateTime? UnassignedAt { get; set; }

		public ShelfTypeResponse? ShelfType { get; set; }
	}
}