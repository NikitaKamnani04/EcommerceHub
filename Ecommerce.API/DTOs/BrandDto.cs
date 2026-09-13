namespace Ecommerce.API.DTOs
{
    public class BrandCreateDto
    {
        public string BrandName { get; set; }
    }

    public class BrandUpdateDto
    {
        public string? BrandName { get; set; }
        public bool? IsActive {  get; set; }
    }


}
