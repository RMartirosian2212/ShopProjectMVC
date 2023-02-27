using Microsoft.AspNetCore.Mvc.Rendering;
using ShopProject_DataAccess.Data;
using ShopProject_DataAccess.Repository.IRepository;
using ShopProject_Models;
using ShopProject_Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopProject_DataAccess.Repository
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private readonly ApplicationDbContext _db;
        public ProductRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public IEnumerable<SelectListItem> GetAllDropdownList(string obj)
        {
            if (obj == WebConstant.CategoryName)
            {
                return _db.Categories.Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                });
            }
            if (obj == WebConstant.ApplicationTypeName)
            {
                return _db.ApplicationTypes.Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                });
            }
            return null;
        }

        public void Update(Product product)
        {
            _db.Products.Update(product);
        }
    }
}
