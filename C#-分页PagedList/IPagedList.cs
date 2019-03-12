using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportService.Test
{
   public interface IPagedList
    {
        /// <summary>
        /// 记录数
        /// </summary>
        int TotalCount { get; set; }
        /// <summary>
        /// 页数
        /// </summary>
        int TotalPages { get; set; }
        /// <summary>
        /// 当前页
        /// </summary>
        int PageIndex { get; set; }
        /// <summary>
        /// 页面大小
        /// </summary>
        int PageSize { get; set; }
        /// <summary>
        /// 是否上一页
        /// </summary>
        bool IsPreviousPage { get; }
        /// <summary>
        /// 是否下一页
        /// </summary>
        bool IsNextPage { get; }
    }
}
