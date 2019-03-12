

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
namespace ReportService.Test
{
    /// <summary>
    /// 分页通用类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PagedList<T> : List<T>, IPagedList
    {
        /// <summary>
        /// 数据源为IQueryable的范型
        /// </summary>
        /// <param name="source">数据源</param>
        /// <param name="index">当前页</param>
        /// <param name="pageSize">每页显示多少条记录</param>
        public PagedList(IQueryable<T> source, int index, int pageSize)
        {
            if (source != null) //判断传过来的实体集是否为空
            {
                int total = source.Count();
                this.TotalCount = total;
                this.TotalPages = total / pageSize;

                if (total % pageSize > 0)
                    TotalPages++;

                this.PageSize = pageSize;
                if (index > this.TotalPages)
                {
                    index = this.TotalPages;
                }
                if (index < 1)
                {
                    index = 1;
                }
                this.PageIndex = index;
                this.AddRange(source.Skip((index - 1) * pageSize).Take(pageSize).ToList()); //Skip是跳到第几页，Take返回多少条
            }
        }


        public PagedList(IQueryable<T> source, PageCtl pageCtl)
        {
            if (pageCtl == null)
            {
                this.AddRange(source.ToList());
                this.ToList();
                return;
            }

            if (pageCtl.PageSize == 0) pageCtl.PageSize = 30;
            if (pageCtl.PageIndex == 0) pageCtl.PageIndex = 1;


            int total = source.Count();
            this.TotalCount = total;
            this.TotalPages = total / pageCtl.PageSize;

            if (total % pageCtl.PageSize > 0)
                TotalPages++;

            string OrderByField = pageCtl.OrderByField;
            bool Ascending = !pageCtl.Dscending;

            Type type = typeof(T);
            if (string.IsNullOrEmpty(OrderByField))
            {
                PropertyInfo[] propertys = type.GetProperties();
                if (propertys.Length == 0)
                    throw new Exception("当前实体未包含属性！");
                //OrderByField = propertys[0].Name;
                foreach (var item in propertys.Where(x => x.GetCustomAttributes(false).Count() > 0))
                {
                    if (item.CustomAttributes.Where(x => x.AttributeType == new System.ComponentModel.DataAnnotations.KeyAttribute().GetType()).FirstOrDefault() != null)
                    {
                        OrderByField = item.Name;
                        break;
                    }
                }
            }

            PropertyInfo property = type.GetProperty(OrderByField);
            if (property == null)
                throw new Exception("错误：\r\n当前实体中不存在指定的排序字段：" + OrderByField);


            ParameterExpression param = Expression.Parameter(type, "p");
            Expression propertyAccessExpression = Expression.MakeMemberAccess(param, property);
            LambdaExpression orderByExpression = Expression.Lambda(propertyAccessExpression, param);

            string methodName = Ascending ? "OrderBy" : "OrderByDescending";

            MethodCallExpression resultExp = Expression.Call(
                typeof(Queryable), methodName, new Type[] { type, property.PropertyType },
            source.Expression,
            Expression.Quote(orderByExpression)
            );

            var query = source.Provider.CreateQuery<T>(resultExp);



            this.PageSize = pageCtl.PageSize;
            if (pageCtl.PageIndex > this.TotalPages)
            {
                pageCtl.PageIndex = this.TotalPages;
            }
            if (pageCtl.PageIndex < 1)
            {
                pageCtl.PageIndex = 1;
            }
            this.PageIndex = pageCtl.PageIndex;
            pageCtl.TotalPages = this.TotalPages;
            pageCtl.TotalCount = this.TotalCount;

            var list = query.Skip((pageCtl.PageIndex - 1) * pageCtl.PageSize).Take(pageCtl.PageSize).ToList();
            this.AddRange(list);
            this.ToList();
        }


        public PagedList(IEnumerable<T> source, PageCtl pageCtl)
        {
            if (pageCtl == null)
            {
                this.AddRange(source.ToList());
                this.ToList();
                return;
            }
            var query = source.AsQueryable();

            if (pageCtl.PageSize == 0) pageCtl.PageSize = 30;
            if (pageCtl.PageIndex == 0) pageCtl.PageIndex = 1;


            int total = source.Count();
            this.TotalCount = total;
            this.TotalPages = total / pageCtl.PageSize;

            if (total % pageCtl.PageSize > 0)
                TotalPages++;

            string OrderByField = pageCtl.OrderByField;
            bool Ascending = !pageCtl.Dscending;

            Type type = typeof(T);
            if (string.IsNullOrEmpty(OrderByField))
            {
                PropertyInfo[] propertys = type.GetProperties();
                if (propertys.Length == 0)
                    throw new Exception("当前实体未包含属性！");
                //OrderByField = propertys[0].Name;
                foreach (var item in propertys.Where(x => x.GetCustomAttributes(false).Count() > 0))
                {
                    if (item.CustomAttributes.Where(x => x.AttributeType == new System.ComponentModel.DataAnnotations.KeyAttribute().GetType()).FirstOrDefault() != null)
                    {
                        OrderByField = item.Name;
                        break;
                    }
                }
            }

            PropertyInfo property = type.GetProperty(OrderByField);
            if (property == null)
                throw new Exception("错误：\r\n当前实体中不存在指定的排序字段：" + OrderByField);

            ParameterExpression param = Expression.Parameter(type, "p");
            Expression propertyAccessExpression = Expression.MakeMemberAccess(param, property);
            LambdaExpression orderByExpression = Expression.Lambda(propertyAccessExpression, param);

            string methodName = Ascending ? "OrderBy" : "OrderByDescending";


            MethodCallExpression resultExp = Expression.Call(
                typeof(Queryable), methodName, new Type[] { type, property.PropertyType },
            query.Expression,
            Expression.Quote(orderByExpression)
            );

            query = query.Provider.CreateQuery<T>(resultExp);

            this.PageSize = pageCtl.PageSize;
            if (pageCtl.PageIndex > this.TotalPages)
            {
                pageCtl.PageIndex = this.TotalPages;
            }
            if (pageCtl.PageIndex < 1)
            {
                pageCtl.PageIndex = 1;
            }
            this.PageIndex = pageCtl.PageIndex;
            pageCtl.TotalPages = this.TotalPages;
            pageCtl.TotalCount = this.TotalCount;

            var list = query.Skip((pageCtl.PageIndex - 1) * pageCtl.PageSize).Take(pageCtl.PageSize).ToList();
            this.AddRange(list);
            this.ToList();
        }

        #region 属性

        /// <summary>
        /// 总页数
        /// </summary>
        public int TotalPages { get; set; }
        /// <summary>
        /// 总记录数
        /// </summary>
        public int TotalCount { get; set; }
        /// <summary>
        /// 当前页
        /// </summary>
        public int PageIndex { get; set; }
        /// <summary>
        /// 每页显示多少条记录
        /// </summary>
        public int PageSize { get; set; }
        /// <summary>
        /// 是否有上一页
        /// </summary>
        public bool IsPreviousPage { get { return (PageIndex > 0); } }
        /// <summary>
        /// 是否有下一页
        /// </summary>
        public bool IsNextPage { get { return (PageIndex * PageSize) <= TotalCount; } }


        #endregion


    }
}
