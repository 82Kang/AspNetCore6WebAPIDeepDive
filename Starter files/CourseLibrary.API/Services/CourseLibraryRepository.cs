using CourseLibrary.API.DbContexts;
using CourseLibrary.API.Entities;
using CourseLibrary.API.Helpers;
using CourseLibrary.API.Models;
using CourseLibrary.API.ResourceParameters;
using Microsoft.EntityFrameworkCore;

namespace CourseLibrary.API.Services;

public class CourseLibraryRepository : ICourseLibraryRepository 
{
    private readonly CourseLibraryContext _context;
    private IPropertyMappingService _propertyMappingService;

    public CourseLibraryRepository(CourseLibraryContext context
        , IPropertyMappingService propertyMappingService)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _propertyMappingService = propertyMappingService 
          ?? throw new ArgumentNullException(nameof(propertyMappingService));
    }

    public void AddCourse(Guid authorId, Course course)
    {
        if (authorId == Guid.Empty)
        {
            throw new ArgumentNullException(nameof(authorId));
        }

        if (course == null)
        {
            throw new ArgumentNullException(nameof(course));
        }

        // always set the AuthorId to the passed-in authorId
        course.AuthorId = authorId;
        _context.Courses.Add(course);
    }

    public void DeleteCourse(Course course)
    {
        _context.Courses.Remove(course);
    }

    public async Task<Course> GetCourseAsync(Guid authorId, Guid courseId)
    {
        if (authorId == Guid.Empty)
        {
            throw new ArgumentNullException(nameof(authorId));
        }

        if (courseId == Guid.Empty)
        {
            throw new ArgumentNullException(nameof(courseId));
        }

#pragma warning disable CS8603 // Possible null reference return.
        return await _context.Courses
          .Where(c => c.AuthorId == authorId && c.Id == courseId).FirstOrDefaultAsync();
#pragma warning restore CS8603 // Possible null reference return.
    }

    public async Task<IEnumerable<Course>> GetCoursesAsync(Guid authorId)
    {
        if (authorId == Guid.Empty)
        {
            throw new ArgumentNullException(nameof(authorId));
        }

        return await _context.Courses
                    .Where(c => c.AuthorId == authorId)
                    .OrderBy(c => c.Title).ToListAsync();
    }

    public void UpdateCourse(Course course)
    {
        // no code in this implementation
    }

    public void AddAuthor(Author author)
    {
        if (author == null)
        {
            throw new ArgumentNullException(nameof(author));
        }

        // the repository fills the id (instead of using identity columns)
        author.Id = Guid.NewGuid();

        foreach (var course in author.Courses)
        {
            course.Id = Guid.NewGuid();
        }

        _context.Authors.Add(author);
    }

    public async Task<bool> AuthorExistsAsync(Guid authorId)
    {
        if (authorId == Guid.Empty)
        {
            throw new ArgumentNullException(nameof(authorId));
        }

        return await _context.Authors.AnyAsync(a => a.Id == authorId);
    }

    public void DeleteAuthor(Author author)
    {
        if (author == null)
        {
            throw new ArgumentNullException(nameof(author));
        }

        _context.Authors.Remove(author);
    }

    public async Task<Author> GetAuthorAsync(Guid authorId)
    {
        if (authorId == Guid.Empty)
        {
            throw new ArgumentNullException(nameof(authorId));
        }

#pragma warning disable CS8603 // Possible null reference return.
        return await _context.Authors.FirstOrDefaultAsync(a => a.Id == authorId);
#pragma warning restore CS8603 // Possible null reference return.
    }

   
    public async Task<IEnumerable<Author>> GetAuthorsAsync()
    {
        return await _context.Authors.ToListAsync();
    }

    public async Task<PagedList<Author>> GetAuthorsAsync(AuthorsResourceParameters authorResourceParam)
    {
      if (authorResourceParam == null)
      {
        throw new ArgumentNullException("Resource parameter is null");
      }

      /*
       * Implement paging no matter what 
      if (string.IsNullOrWhiteSpace(authorResourceParam.MainCategory) || string.IsNullOrWhiteSpace(authorResourceParam.SearchQuery))
      {
        return await GetAuthorsAsync();
      }
      */

      var allAuthors = _context.Authors as IQueryable<Author>;

      if (!string.IsNullOrWhiteSpace(authorResourceParam.MainCategory))
      {
        var mainCategory = authorResourceParam.MainCategory.Trim();
        allAuthors = allAuthors.Where( author => author.MainCategory == mainCategory);
      }

      if (!string.IsNullOrWhiteSpace(authorResourceParam.SearchQuery))
      {
        var searchQuery = authorResourceParam.SearchQuery.Trim();
        allAuthors = allAuthors.Where( author => author.FirstName.Contains(searchQuery)
                                         || author.LastName.Contains(searchQuery)
                                         || author.MainCategory.Contains(searchQuery));
      }

      if (!string.IsNullOrWhiteSpace(authorResourceParam.OrderBy))
      {
        var authorPropMapping = _propertyMappingService.GetPropertyMapping<AuthorDto, Author>();
        allAuthors = allAuthors.ApplySort(authorResourceParam.OrderBy, authorPropMapping);
      }

      return await PagedList<Author>.CreateAsync(allAuthors, authorResourceParam.PageNumber, authorResourceParam.PageSize); 
    }

    public async Task<IEnumerable<Author>> GetAuthorsAsync(IEnumerable<Guid> authorIds)
    {
        if (authorIds == null)
        {
            throw new ArgumentNullException(nameof(authorIds));
        }

        return await _context.Authors.Where(a => authorIds.Contains(a.Id))
            .OrderBy(a => a.FirstName)
            .OrderBy(a => a.LastName)
            .ToListAsync();
    }

    public void UpdateAuthor(Author author)
    {
        // no code in this implementation
    }

    public async Task<bool> SaveAsync()
    {
        return (await _context.SaveChangesAsync() >= 0);
    }
}

