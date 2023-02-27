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
    public class OrderHeaderRepository : Repository<OrderHeader>, IOrderHeaderRepository
    {
        private readonly ApplicationDbContext _db;
        public OrderHeaderRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(OrderHeader orderHeader)
        {
            _db.OrderHeader.Update(orderHeader);
        }
    }
}
