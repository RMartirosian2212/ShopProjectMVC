using Microsoft.AspNetCore.Mvc.Rendering;
using ShopProject_Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopProject_DataAccess.Repository.IRepository
{
    public interface IProductRepository : IRepository<Product>
    {
        void Update(Product product);
        IEnumerable<SelectListItem> GetAllDropdownList(string obj);
    }
}
