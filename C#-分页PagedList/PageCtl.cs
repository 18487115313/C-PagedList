using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportService.Test
{
    public class PageCtl 
    {
        /// <summary>
        /// 当前页
        /// </summary>
        public int PageIndex { get; set; }
        /// <summary>
        /// 每页数据大小
        /// </summary>
        public int PageSize { get; set; }
        /// <summary>
        /// 总页数
        /// </summary>
        public int TotalPages { get; set; }
        /// <summary>
        /// 总记录数
        /// </summary>
        public int TotalCount { get; set; }
        /// <summary>
        /// 排序字段
        /// </summary>
        public string OrderByField { get; set; }
        /// <summary>
        /// 是否倒序
        /// </summary>
        public bool Dscending { get; set; }
    }
}
