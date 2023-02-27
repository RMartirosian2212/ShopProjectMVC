using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopProject_Models
{
    public class Product
    {
        public Product()
        {
            TempSqFt = 1;
        }
        [Key]
        public int Id { get; set; }
        [Required]
        public string? Name { get; set; }
        public string? Description { get; set; }
        [Range(1, int.MaxValue)]
        public double Price { get; set; }
        public string ShortDesc { get; set; } = "Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since the 1500s, when an unknown printer took a galley of type and scrambled it to make a type specimen book.";
        public string? Image { get; set; }

        [Display(Name = "Category Type")]
        public int CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        public Category? Category{ get; set; }
        
        [Display(Name = "Application Type")]
        public int ApplicationTypeId { get; set; }
        [ForeignKey("ApplicationTypeId")]
        public ApplicationType? ApplicationType{ get; set; }
        
        [NotMapped]
        [Range(1, 10000)]
        public int TempSqFt { get; set; } 
    }
}
