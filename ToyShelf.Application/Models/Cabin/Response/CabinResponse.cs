namespace ToyShelf.Application.Models.Cabin.Response
{
    public class CabinResponse
    {
        public Guid Id { get; set; }
		public Guid? StoreId { get; set; }
		public string Code { get; set; } = string.Empty;
		public string Name { get; set; } = string.Empty;
		public string? LocationDescription { get; set; }
		public bool IsOnline { get; set; }
		public bool IsActive { get; set; } = true;
		public DateTime CreatedAt { get; set; }
		public DateTime? LastHeartbeatAt { get; set; }
    }
}