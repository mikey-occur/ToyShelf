namespace ToyShelf.Application.Models.Shelf.Request
{
    public class CreateShelfRequest
    {
        public Guid? PartnerId { get; set; }
        public Guid? StoreId { get; set; }
        public string Code { get; set; } = string.Empty;
        public int Level { get; set; }
    }
}