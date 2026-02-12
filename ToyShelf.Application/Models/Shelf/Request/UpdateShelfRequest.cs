using ToyShelf.Domain.Entities;

namespace ToyShelf.Application.Models.Shelf.Request
{
    public class UpdateShelfRequest
    {
        public Guid? PartnerId { get; set; }
        public Guid? StoreId { get; set; }
        public string Code { get; set; } = string.Empty;
        public int Level { get; set; }
        public ShelfStatus Status { get; set; }
    }
}