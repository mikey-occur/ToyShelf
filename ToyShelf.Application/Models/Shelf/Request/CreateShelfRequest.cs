namespace ToyShelf.Application.Models.Shelf.Request
{
    public class CreateShelfRequest
    {
        public Guid InventoryLocationId { get; set; }

		public Guid ShelfTypeId { get; set; }
		public string Code { get; set; } = string.Empty;
   
    }
}