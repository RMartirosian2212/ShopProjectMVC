using Microsoft.AspNetCore.Mvc.Rendering;
using ShopProject_Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopProject_DataAccess.Repository.IRepository
{
    public interface IInquiryHeaderRepository : IRepository<InquiryHeader>
    {
        void Update(InquiryHeader inquiryHeader);
    }
}
