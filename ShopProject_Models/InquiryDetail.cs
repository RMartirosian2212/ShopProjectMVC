using ShopProject_Models.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopProject_Models
{
    public class InquiryDetail
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int InquiryHeaderId { get; set; }
        [ForeignKey("InquiryHeaderId")]
        public InquiryHeader inquiryHeader { get; set; }
        [Required]
        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public Product Product { get; set; }
    }
}
