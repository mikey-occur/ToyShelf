namespace ToyCabin.Application.Models.Cabin.Request
{
    public class UpdateCabinRequest
    {
		public Guid? StoreId { get; set; }
		public string Code { get; set; } = string.Empty;
		public string Name { get; set; } = string.Empty;
		public string? LocationDescription { get; set; }
		public bool? IsActive { get; set; }
		public bool? IsOnline { get; set; }
    }
}