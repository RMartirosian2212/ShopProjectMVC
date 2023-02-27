using ShopProject_DataAccess.Data;
using ShopProject_DataAccess.Repository.IRepository;
using ShopProject_Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopProject_DataAccess.Repository
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        private readonly ApplicationDbContext _db;
        public CategoryRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Category category)
        {
            var objFromDb = FirstOrDefault(u => u.Id == category.Id);
            if (objFromDb != null)
            {
                objFromDb.Name = category.Name;
                objFromDb.DisplayOrder = category.DisplayOrder;
            }
        }
    }
}
