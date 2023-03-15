using Microsoft.EntityFrameworkCore;

namespace CourseLibrary.API.Helpers;

public class PagedList<T> : List<T>
{
  public int CurrentPage { get; private set; }
  public int TotalPages { get; private set; }
  public int PageSize { get; private set; }
  public int TotalCount { get; private set; } // how many items are there

  public bool HasNext => CurrentPage > 1;
  public bool HasPrevious => CurrentPage < TotalPages;

  public PagedList(List<T> items, int count, int pageNumber, int pageSize)
  {
    CurrentPage = pageNumber;
    PageSize = pageSize;
    TotalCount = count;
    TotalPages = (int) Math.Ceiling (count / (double)pageSize);
    AddRange(items);
  }

  public static async Task<PagedList<T>> CreateAsync(IQueryable<T> source, int pageNumber
                                                     ,int pageSize)
  {
    var count = source.Count();
    var items = await source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

    return new PagedList<T> (items, count, pageNumber, pageSize);
  }
}
