namespace Ecommerce.Web.Models
{
    // Product list page ke liye - jyada details chahiye dikhane ke liye

    public class ProductListItemModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = "";
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public int CategoryId { get; set; }
        public int SubCategoryId { get; set; }
        public int BrandId { get; set; }
    }

    // Naya product add karne ke form ke liye
    public class ProductCreateFormModel
    {
        public int CategoryID { get; set; }
        public int SubCategoryID { get; set; }
        public int BrandID { get; set; }
        public string ProductName { get; set; } = "";
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
    }

    // Existing product edit karne ke form ke liye
    public class ProductEditFormModel
    {
        public int ProductId { get; set; }
        public int CategoryID { get; set; }
        public int SubCategoryID { get; set; }
        public int BrandID { get; set; }
        public string ProductName { get; set; } = "";
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
    }

    // Dropdown mein dikhane ke liye - sirf ID aur Naam chahiye
    public class DropdownItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }

        // SubCategory ke liye - CategoryID bhi chahiye (JS filter karne ke liye)
        public class SubCategoryDropdownItem
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
            public int CategoryId { get; set; }
        }

        // Create form + saare dropdowns ka data - ek saath bhejne ke liye
        public class ProductCreateViewModel
        {
            public ProductCreateFormModel Product { get; set; } = new();
            public List<DropdownItem> Categories { get; set; } = new();
            public List<SubCategoryDropdownItem> SubCategories { get; set; } = new();
            public List<DropdownItem> Brands { get; set; } = new();
        }

        // Edit form ke liye bhi same tarah
        public class ProductEditViewModel
        {
            public ProductEditFormModel Product { get; set; } = new();
            public List<DropdownItem> Categories { get; set; } = new();
            public List<SubCategoryDropdownItem> SubCategories { get; set; } = new();
            public List<DropdownItem> Brands { get; set; } = new();
        }
    }
