

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ReportService.Test
{
   public static class QueryExtension
    {
        /// <summary>
        /// 返回IQueryble分页数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="linq"></param>
        /// <param name="pageIndex">页号</param>
        /// <param name="pageSize">每页行数</param>
        /// <returns></returns>
        public static PagedList<T> ToPagedList<T>(this IQueryable<T> linq, int pageIndex, int pageSize)
        {
            return new PagedList<T>(linq, pageIndex, pageSize);
        }


        /// <summary>
        /// 返回Iqueryble分页数据（默认每页18条数据）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="linq">IQueryble对像</param>
        /// <param name="pageIndex">页号</param>
        /// <returns></returns>
        public static PagedList<T> ToPagedList<T>(this IQueryable<T> linq, int pageIndex)
        {
            int pageSize = 30;
            return new PagedList<T>(linq, pageIndex, pageSize);
        }

        public static List<T> ToPagedList<T>(this IQueryable<T> linq, PageCtl pcl)
        {
            return new PagedList<T>(linq, pcl).ToList();
        }

        public static List<T> ToPagedList<T>(this IEnumerable<T> linq, PageCtl pcl)
        {
            return new PagedList<T>(linq, pcl).ToList();
        }
        public static int Count(this IQueryable source)
        {
            if (source == null) throw new ArgumentNullException("source");
            return (int)source.Provider.Execute(
                Expression.Call(
                    typeof(Queryable), "Count",
                    new Type[] { source.ElementType }, source.Expression));
        }

        public static IQueryable ToPagedList(this IQueryable source, PageCtl pageCtl)
        {
            if (pageCtl == null)
            {
                return source;
            }

            if (pageCtl.PageSize == 0) pageCtl.PageSize = 30;
            if (pageCtl.PageIndex == 0) pageCtl.PageIndex = 1;


            int total = source.Count();
            pageCtl.TotalCount = total;
            pageCtl.TotalPages = total / pageCtl.PageSize;


            if (total % pageCtl.PageSize > 0)
                pageCtl.TotalPages++;

            string OrderByField = pageCtl.OrderByField;
            bool Ascending = !pageCtl.Dscending;

            Type type = source.ElementType;
            if (string.IsNullOrEmpty(OrderByField))
            {
                PropertyInfo[] propertys = type.GetProperties();
                if (propertys.Length == 0)
                    throw new Exception("当前实体未包含属性！");
                var pIs = from c in propertys where c.GetCustomAttributes(false).Count() > 0 select c;
                var pI = from c in pIs where c.CustomAttributes.Where(w => w.AttributeType == typeof(System.ComponentModel.DataAnnotations.KeyAttribute)).Count() > 0 select c;
                if (pI.Count() > 0)
                {
                    OrderByField = pI.First().Name;
                }
                else
                {
                    OrderByField = propertys[0].Name;
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

            var query = source.Provider.CreateQuery(resultExp);

            var PageSize = pageCtl.PageSize;
            if (pageCtl.PageIndex > pageCtl.TotalPages)
            {
                pageCtl.PageIndex = pageCtl.TotalPages;
            }
            if (pageCtl.PageIndex < 1)
            {
                pageCtl.PageIndex = 1;
            }
            //  this.PageIndex = pageCtl.PageIndex;
            //pageCtl.TotalPages = this.TotalPages;
            //pageCtl.TotalCount = this.TotalCount;

            query = query._Skip((pageCtl.PageIndex - 1) * pageCtl.PageSize)._Take(pageCtl.PageSize);
            return query;
        }
        public static IQueryable _Skip(this IQueryable source, int count)
        {
            if (source == null) throw new ArgumentNullException("source");
            return source.Provider.CreateQuery(
                Expression.Call(
                    typeof(Queryable), "Skip",
                    new Type[] { source.ElementType },
                    source.Expression, Expression.Constant(count)));
        }
        public static IQueryable _Take(this IQueryable source, int count)
        {
            if (source == null) throw new ArgumentNullException("source");
            return source.Provider.CreateQuery(
                Expression.Call(
                    typeof(Queryable), "Take",
                    new Type[] { source.ElementType },
                    source.Expression, Expression.Constant(count)));
        }
    }
}
