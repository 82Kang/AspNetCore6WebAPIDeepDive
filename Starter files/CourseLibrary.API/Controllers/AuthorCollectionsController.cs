using AutoMapper;
using CourseLibrary.API.Helpers;
using CourseLibrary.API.Entities;
using CourseLibrary.API.Models;
using CourseLibrary.API.Services;
using Microsoft.AspNetCore.Mvc;
namespace CourseLibrary.API.Controllers;

[ApiController]
[Route("api/authorcollections")]
public class AuthorCollectionsController : ControllerBase
{
  private readonly ICourseLibraryRepository _courseLibraryRepository;
  private readonly IMapper _mapper;

  public AuthorCollectionsController(ICourseLibraryRepository courseLibraryRepository,
      IMapper mapper)
  {
    _courseLibraryRepository = courseLibraryRepository;
    _mapper = mapper;
  }


  /*
   * Two ways to pass the query parameters:
   * 1. Key value pair
   */
  [HttpGet("({authorIds})", Name = "GetAuthorCollection")]
  public async Task<ActionResult<IEnumerable<AuthorDto>>>
    GetAuthorCollection(
        [ModelBinder(BinderType = typeof(ArrayModelBinder))]
        [FromRoute] IEnumerable<Guid> authorIds)
  {
    /* query from the model that is binded */
    var authorEntities = await _courseLibraryRepository.GetAuthorsAsync(authorIds);
    if (authorEntities.Count() != authorIds.Count()) 
    {
      return NotFound();
    }
    
    return Ok(_mapper.Map<IEnumerable<AuthorDto>>(authorEntities));
  }

  [HttpPost]
  public async Task<ActionResult<IEnumerable<AuthorDto>>> CreateAuthorCollection
    (IEnumerable<AuthorForCreationDto> authorCollection)
  {
    var authorEntities = _mapper.Map<IEnumerable<Author>>(authorCollection);
    foreach (var author in authorEntities)
    {
      _courseLibraryRepository.AddAuthor(author);
    }

    await _courseLibraryRepository.SaveAsync();

    return CreatedAtRoute("GetAuthorCollection", new { authorIds = string.Join(",", authorEntities.Select(a => a.Id)).ToString() },
        _mapper.Map<IEnumerable<AuthorDto>>(authorEntities));
  }
}
