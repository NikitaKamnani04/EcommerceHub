namespace Ecommerce.API.DTOs
{
    public class SubCategoryCreateDto
    {
        public int CategoryID { get; set; }
        public string SubCategoryName { get; set; }
        public string? Description { get; set; }
    }

    public class SubCategoryUpdateDto
    {
        public int? CategoryID { get; set; }
        public string? SubCategoryName { get; set; }
        public string? Description { get; set; }
        public bool? IsActive { get; set; }
    }
}
