namespace Ecommerce.API.DTOs
{
    public class CategoryCreateDto
    {
        public string CategoryName { get; set; }
        public string? CategoryDescription { get; set; }
        public int? ParentCategoryID { get; set; }
    }

    public class CategoryUpdateDto
    {
        public string? CategoryName { get; set; }
        public string? CategoryDescription { get; set; }
        public int? ParentCategoryID { get; set; }
        public bool? IsActive { get; set; }
    }
}
