using SQLite;
using System.Application.Models;
using System.Linq.Expressions;

namespace System;

public static class AsyncTableQueryExtensions
{
    #region WhereOr

    /// <inheritdoc cref="QueryableExtensions.WhereOr{T}(IReadOnlyList{Expression{Func{T, bool}}})"/>
    public static AsyncTableQuery<T> WhereOr<T>(this AsyncTableQuery<T> source, IReadOnlyList<Expression<Func<T, bool>>> predicates) where T : new()
    {
        var predicate = ExpressionHelper.WhereOr(predicates);
        return source.Where(predicate);
    }

    /// <inheritdoc cref="QueryableExtensions.WhereOr{T}(IReadOnlyList{Expression{Func{T, bool}}})"/>
    public static AsyncTableQuery<T> WhereOr<T>(this AsyncTableQuery<T> source, IEnumerable<Expression<Func<T, bool>>> predicates) where T : new()
        => source.WhereOr(predicates.ToArray());

    /// <inheritdoc cref="QueryableExtensions.WhereOr{T}(IReadOnlyList{Expression{Func{T, bool}}})"/>
    public static AsyncTableQuery<T> WhereOr<T>(this AsyncTableQuery<T> source, params Expression<Func<T, bool>>[] predicates) where T : new()
    {
        IReadOnlyList<Expression<Func<T, bool>>> predicates_ = predicates;
        return source.WhereOr(predicates_);
    }

    #endregion

    #region Paging

    /// <summary>
    /// 根据页码进行分页查询，调用此方法前 必须 进行排序
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="source"></param>
    /// <param name="current"></param>
    /// <param name="pageSize"></param>
    /// <returns></returns>
    public static async Task<PagedModel<TEntity>> PagingAsync<TEntity>(
        this AsyncTableQuery<TEntity> source,
        int current = IPagedModel.DefaultCurrent,
        int pageSize = IPagedModel.DefaultPageSize) where TEntity : new()
    {
        var skipCount = (current - 1) * pageSize;
        var total = await source.CountAsync();
        var dataSource = await source.Skip(skipCount).Take(pageSize).ToListAsync();
        var pagedModel = new PagedModel<TEntity>
        {
            Current = current,
            PageSize = pageSize,
            Total = total,
            DataSource = dataSource,
        };
        return pagedModel;
    }

    #endregion
}