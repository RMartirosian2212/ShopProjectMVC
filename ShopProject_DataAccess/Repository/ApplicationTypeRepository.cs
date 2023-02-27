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
    public class ApplicationTypeRepository : Repository<ApplicationType>, IApplicationTypeRepository
    {
        private readonly ApplicationDbContext _db;
        public ApplicationTypeRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(ApplicationType applicationType)
        {
            var objFromDb = FirstOrDefault(u => u.Id == applicationType.Id);
            if (objFromDb != null)
            {
                objFromDb.Name = applicationType.Name;
            }
        }
    }
}
