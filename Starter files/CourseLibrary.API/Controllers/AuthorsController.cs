
using System.Text.Json;
using AutoMapper;
using CourseLibrary.API.Helpers;
using CourseLibrary.API.Models;
using CourseLibrary.API.ResourceParameters;
using CourseLibrary.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace CourseLibrary.API.Controllers;

[ApiController] 
[Route("api/authors")]
public class AuthorsController : ControllerBase
{
  private readonly ICourseLibraryRepository _courseLibraryRepository;
  private readonly IMapper _mapper;

  public AuthorsController(
      ICourseLibraryRepository courseLibraryRepository,
      IMapper mapper)
  {
    _courseLibraryRepository = courseLibraryRepository ??
      throw new ArgumentNullException(nameof(courseLibraryRepository));
    _mapper = mapper ??
      throw new ArgumentNullException(nameof(mapper));
  }

  private string? CreateAuthorsResourceUri(AuthorsResourceParameters authorResParam
                                           , ResourceUriType resourceType)
  {
    int pageShift = 0;
    switch (resourceType)
    {
      case ResourceUriType.NextPage:
        pageShift = 1;
        break;

      case ResourceUriType.PreviousPage:
        pageShift = -1;
        break;
    }

    return Url.Link("GetAuthors", new
        {
        orderBy = authorResParam.OrderBy,
        pageNumber = authorResParam.PageNumber + pageShift,
        pageSize = authorResParam.PageSize, 
        mainCategory = authorResParam.MainCategory,
        searchQuery = authorResParam.SearchQuery,
        });
  }

  [HttpGet(Name = "GetAuthors")]
  [HttpHead]
  public async Task<ActionResult<IEnumerable<AuthorDto>>> GetAuthors(
      [FromQuery] AuthorsResourceParameters resourceParam)
  { 
    // get authors from repo
    var authorsPagedFromRepo = await _courseLibraryRepository
      .GetAuthorsAsync(resourceParam); 

    var previousPageLink = authorsPagedFromRepo.HasPrevious
      ? CreateAuthorsResourceUri(
          resourceParam
          ,ResourceUriType.PreviousPage) : null;

    var nextPageLink = authorsPagedFromRepo.HasNext
      ? CreateAuthorsResourceUri(
          resourceParam
          ,ResourceUriType.NextPage) : null;

    // create the object for custom header
    var paginationMetadata = new 
    {
      totalCount = authorsPagedFromRepo.Count,
      pageSize = authorsPagedFromRepo.PageSize,
      currentPage = authorsPagedFromRepo.CurrentPage,
      totalPages = authorsPagedFromRepo.TotalPages,
      previousPageLink = previousPageLink,
      nextPageLink = nextPageLink
    };

    Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(paginationMetadata));
    // return them
    return Ok(_mapper.Map<IEnumerable<AuthorDto>>(authorsPagedFromRepo));
  }

  [HttpGet("getit")]
  public ActionResult<string> GetIt(Guid authorId)
  {

    var list = Queryable.AsQueryable(new [] {new { name = "def", occ = "cricketer" }, new { name = "abc", occ = "hockey" }});
    var sorted = list.DoSort("name");
    return Ok(list);
  }

  [HttpGet("{authorId}", Name = "GetAuthor")]
  public async Task<ActionResult<AuthorDto>> GetAuthor(Guid authorId)
  {
    // get author from repo
    var authorFromRepo = await _courseLibraryRepository.GetAuthorAsync(authorId);

    if (authorFromRepo == null)
    {
      return NotFound();
    }

    // return author
    return Ok(_mapper.Map<AuthorDto>(authorFromRepo));
  }

  [HttpPost]
  public async Task<ActionResult<AuthorDto>> CreateAuthor(AuthorForCreationDto author)
  {
    var authorEntity = _mapper.Map<Entities.Author>(author);

    _courseLibraryRepository.AddAuthor(authorEntity);
    await _courseLibraryRepository.SaveAsync();

    var authorToReturn = _mapper.Map<AuthorDto>(authorEntity);

    return CreatedAtRoute("GetAuthor",
        new { authorId = authorToReturn.Id },
        authorToReturn);
  }

  [HttpOptions]
  public IActionResult Options()
  {
    Response.Headers.Add("Allow", "GET, HEAD, POST, OPTIONS");
    return Ok();
  }
}
