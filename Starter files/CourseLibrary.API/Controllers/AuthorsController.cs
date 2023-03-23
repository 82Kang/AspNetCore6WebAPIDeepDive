
using System.Text.Json;
using AutoMapper;
using CourseLibrary.API.Helpers;
using CourseLibrary.API.Models;
using CourseLibrary.API.ResourceParameters;
using CourseLibrary.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using CourseLibrary.API.ActionConstraints;

namespace CourseLibrary.API.Controllers;

[ApiController] 
[Route("api/authors")]
public class AuthorsController : ControllerBase
{
  private readonly ICourseLibraryRepository _courseLibraryRepository;
  private readonly IMapper _mapper;
  private readonly IPropertyMappingService _propertyMappingService;
  private readonly IPropertyCheckerService _propertyCheckerService;
  private readonly ProblemDetailsFactory _problemDetailsFactory;
  //ApiDescriptionGroupCollection  descr;

  public AuthorsController(
      ICourseLibraryRepository courseLibraryRepository,
      IMapper mapper,
      IPropertyMappingService propertyMappingService,
      IPropertyCheckerService propertyCheckerService,
      ProblemDetailsFactory problemDetailsFactory)
  {
    _courseLibraryRepository = courseLibraryRepository ??
      throw new ArgumentNullException(nameof(courseLibraryRepository));
    _mapper = mapper ??
      throw new ArgumentNullException(nameof(mapper));
    _propertyMappingService = propertyMappingService ??
      throw new ArgumentNullException(nameof(propertyMappingService));
    _propertyCheckerService = propertyCheckerService ??
      throw new ArgumentNullException(nameof(propertyCheckerService));
    _problemDetailsFactory = problemDetailsFactory ??
      throw new ArgumentNullException(nameof(problemDetailsFactory));
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

      case ResourceUriType.ThisPage:
      default:
        pageShift = 0;
        break;
    }

    return Url.Link("GetAuthors", new
        {
        fields = authorResParam.fields,
        orderBy = authorResParam.OrderBy,
        pageNumber = authorResParam.PageNumber + pageShift,
        pageSize = authorResParam.PageSize, 
        mainCategory = authorResParam.MainCategory,
        searchQuery = authorResParam.SearchQuery,
        });
  }

  private IEnumerable<LinkDto> CreateLinksForAuthor(string authorEndpoint, Guid authorId
                                , string? fields, string contentType)
  {
    var authorLinks = new List<LinkDto>();
    /*
     * We create links for the author once it is successfully created. Those links basically
     * are the actions which the consumer of the API can take on author upon it's successfull
     * creation.
     * This is the place where we decide whether to allow consumer of the API to have knowledge
     * of some functionality or not.
     */
    // generate self link as the newly created author can be fetched from the database
    if (string.IsNullOrWhiteSpace(fields))
    {
      authorLinks.Add(new LinkDto(
            href: Url.Link (authorEndpoint, new { authorId }),
            rel: "self",
            method: "GET",
            type: contentType));
    }
    else
    {
      authorLinks.Add(new LinkDto(
            href: Url.Link (authorEndpoint, new { authorId, fields }),
            rel: "self",
            method: "GET",
            type: contentType));
    }

    authorLinks.Add(new LinkDto(
          href: Url.Link ("CreateCourseForAuthor", new { authorId }),
          rel: "create_course_for_author",
          method: "POST",
          type: contentType));
    
    authorLinks.Add(new LinkDto(
          href: Url.Link ("GetCoursesForAuthor", new { authorId }),
          rel: "courses",
          method: "GET",
          type: contentType));

    return authorLinks;
  }

  private IEnumerable<LinkDto> CreateLinksForAuthors(AuthorsResourceParameters resourceParam,
      bool hasPrevious, bool hasNext)
  {
    var curPageLink = CreateAuthorsResourceUri(resourceParam, ResourceUriType.ThisPage);
    var authorLinks = new List<LinkDto>();
    /*
     * We create links for the author once it is successfully created. Those links basically
     * are the actions which the consumer of the API can take on author upon it's successfull
     * creation.
     * This is the place where we decide whether to allow consumer of the API to have knowledge
     * of some functionality or not.
     */

    // generate self link as the newly created author can be fetched from the database

    authorLinks.Add(new LinkDto(
          href: curPageLink,
          rel: "self",
          method: "GET"));

    if (hasPrevious)
    {
      var previousPageLink = CreateAuthorsResourceUri(
          resourceParam
          ,ResourceUriType.PreviousPage);

      authorLinks.Add(new LinkDto(
            href: previousPageLink,
            rel: "previou_page_link",
            method: "GET"));
    }

    if (hasNext)
    {
      var nextPageLink = CreateAuthorsResourceUri(
          resourceParam
          ,ResourceUriType.NextPage);

      authorLinks.Add(new LinkDto(
            href: nextPageLink,
            rel: "previou_page_link",
            method: "GET"));
    }

    return authorLinks;
  }

  [HttpGet(Name = "GetAuthors")]
  [HttpHead]
  public async Task<IActionResult> GetAuthors(
      [FromQuery] AuthorsResourceParameters resourceParam)
  { 
    if (!_propertyMappingService.ValidMappingExistsFor<AuthorDto, Entities.Author>
        (resourceParam.OrderBy != null ? resourceParam.OrderBy : ""))
      return BadRequest();

    /*
     * A more sane option is to return some extra data with the failed Bad request 
     */
    if (!_propertyCheckerService.TypeHasProperties<AuthorDto>(resourceParam.fields))
      return BadRequest(_problemDetailsFactory.CreateProblemDetails(HttpContext,
            statusCode: 400,
            detail: "Not all requested data shaping fields are present"
                    + $"on the resource {resourceParam.fields}"
            ));

    // get authors from repo
    var authorsPagedFromRepo = await _courseLibraryRepository
      .GetAuthorsAsync(resourceParam); 

    // create the object for custom header
    var paginationMetadata = new 
    {
      totalCount = authorsPagedFromRepo.Count,
      pageSize = authorsPagedFromRepo.PageSize,
      currentPage = authorsPagedFromRepo.CurrentPage,
      totalPages = authorsPagedFromRepo.TotalPages,
    };

    Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(paginationMetadata));
    // return them
    var shapedAuthors = _mapper.Map<IEnumerable<AuthorDto>>(authorsPagedFromRepo).ShapeData(resourceParam.fields);

    shapedAuthors = shapedAuthors.Select( authorExpando => {
        var authorExpandoDict = authorExpando as IDictionary<string, object>;
        authorExpandoDict["links"] = CreateLinksForAuthor("GetAuthorWithoutLinks"
                                      ,(Guid) authorExpandoDict["Id"], null
                                      , Request.ContentType ?? "application/json");
        return authorExpando;
        });

    var shapedAuthorsWithLink = new 
            { 
              value = shapedAuthors,
              links = CreateLinksForAuthors(resourceParam, authorsPagedFromRepo.HasPrevious,
                        authorsPagedFromRepo.HasNext)
            };

    return Ok(shapedAuthorsWithLink);
  }

  [RequestHeaderMatchesMediaTypeAttribute("Accept", "application/json",
                          "application/vnd.marvin.author.friendly+json", "*/*")]
  [Produces("application/vnd.marvin.author.friendly+json")]
  [HttpGet("{authorId}", Name = "GetAuthorWithoutLinks")]
  public async Task<ActionResult<AuthorDto>> GetAuthorWithoutLinks(Guid authorId, string? fields)
  {
    if (!_propertyCheckerService.TypeHasProperties<AuthorDto>(fields)) {
      return BadRequest(_problemDetailsFactory.CreateProblemDetails(HttpContext,
            statusCode: 400,
            detail: "Not all requested data shaping fields are present"
                    + $"on the resource {fields}"
            ));
    }

    IDictionary<string, object?> authorToReturn;
    // get author from repo
    var authorFromRepo = await _courseLibraryRepository.GetAuthorAsync(authorId);

    if (authorFromRepo == null)
    {
      return NotFound();
    }

    authorToReturn = _mapper.Map<AuthorDto>(authorFromRepo)
                                .ShapeData(fields) as IDictionary<string, object?>;

    return Ok(authorToReturn);
  }

  [HttpGet("{authorId}", Name = "GetAuthorWithLinks")]
  [Produces("application/vnd.marvin.hateoas+json",
            "application/vnd.marvin.author.friendly.hateoas+json")]
  [RequestHeaderMatchesMediaTypeAttribute("Accept", "application/vnd.marvin.hateoas+json",
            "application/vnd.marvin.author.friendly.hateoas+json")]
  public async Task<ActionResult<AuthorDto>> GetAuthorWithLinks(Guid authorId, string? fields)
  {

    if (!_propertyCheckerService.TypeHasProperties<AuthorDto>(fields))
    {
      return BadRequest(_problemDetailsFactory.CreateProblemDetails(HttpContext,
            statusCode: 400,
            detail: "Not all requested data shaping fields are present"
                    + $"on the resource {fields}"
            ));
    }

    IDictionary<string, object?> authorToReturn;
    // get author from repo
    var authorFromRepo = await _courseLibraryRepository.GetAuthorAsync(authorId);

    if (authorFromRepo == null)
    {
      return NotFound();
    }

    authorToReturn = _mapper.Map<AuthorDto>(authorFromRepo)
                                .ShapeData(fields) as IDictionary<string, object?>;

    authorToReturn.Add("links", CreateLinksForAuthor("GetAuthorWithoutLinks", authorId, fields
                                , Request.ContentType ?? "application/json"));

    return Ok(authorToReturn);
  }

  [HttpGet("{authorId}", Name = "GetFullAuthorWithoutLinks")]
  [Produces("application/vnd.marvin.author.full+json")]
  [RequestHeaderMatchesMediaTypeAttribute("Accept", "application/vnd.marvin.author.full+json")]
  public async Task<ActionResult<AuthorDto>> GetFullAuthorWithoutLinks(Guid authorId
                                          , string? fields)
  {

    if (!_propertyCheckerService.TypeHasProperties<AuthorFullDto>(fields))
    {
      return BadRequest(_problemDetailsFactory.CreateProblemDetails(HttpContext,
            statusCode: 400,
            detail: "Not all requested data shaping fields are present"
                    + $"on the resource {fields}"
            ));
    }

    IDictionary<string, object?> authorToReturn;
    // get author from repo
    var authorFromRepo = await _courseLibraryRepository.GetAuthorAsync(authorId);

    if (authorFromRepo == null)
    {
      return NotFound();
    }

    authorToReturn = _mapper.Map<AuthorFullDto>(authorFromRepo)
                                .ShapeData(fields) as IDictionary<string, object?>;

    return Ok(authorToReturn);
  }

  [HttpGet("{authorId}", Name = "GetAuthorWithoutLinks")]
  [Produces("application/vnd.marvin.author.full.hateoas+json")]
  [RequestHeaderMatchesMediaTypeAttribute("Accept",
                                    "application/vnd.marvin.author.full.hateoas+json")]
  public async Task<ActionResult<AuthorDto>> GetFullAuthorWithLinks(Guid authorId
                                            , string? fields)
  {

    if (!_propertyCheckerService.TypeHasProperties<AuthorFullDto>(fields))
    {
      return BadRequest(_problemDetailsFactory.CreateProblemDetails(HttpContext,
            statusCode: 400,
            detail: "Not all requested data shaping fields are present"
                    + $"on the resource {fields}"
            ));
    }

    IDictionary<string, object?> authorToReturn;
    // get author from repo
    var authorFromRepo = await _courseLibraryRepository.GetAuthorAsync(authorId);

    if (authorFromRepo == null)
    {
      return NotFound();
    }

    authorToReturn = _mapper.Map<AuthorFullDto>(authorFromRepo)
                                .ShapeData(fields) as IDictionary<string, object?>;

    authorToReturn.Add("links", CreateLinksForAuthor("GetFullAuthorWithLinks", authorId, fields
                              , Request.ContentType ?? "application/json"));

    return Ok(authorToReturn);
  }

  [HttpPost]
  [RequestHeaderMatchesMediaTypeAttribute("Content-Type",
                          "application/vnd.marvin.authorforcreationwithdateofdeathdto+json")]
  [Consumes("application/vnd.marvin.authorforcreationwithdateofdeathdto+json")]
  // only consumes also would work here, keeping the other attribute for educational purpose.
  public async Task<ActionResult<AuthorDto>> CreateAuthor(AuthorForCreationWithDateOfDeathDto author)
  {
    var authorEntity = _mapper.Map<Entities.Author>(author);

    _courseLibraryRepository.AddAuthor(authorEntity);
    await _courseLibraryRepository.SaveAsync();

    var authorDto = _mapper.Map<AuthorDto>(authorEntity);
    var links = CreateLinksForAuthor("GetAuthorWithoutLinks", authorEntity.Id, null,
                     Request.ContentType ?? "application/json");
    var authorExpando = authorDto.ShapeData(null);

    authorExpando.TryAdd("links", links);

    return CreatedAtRoute("GetAuthorWithoutLinks",
        new { authorId = authorDto.Id },
        authorExpando);
  }

  [HttpPost]
  [RequestHeaderMatchesMediaTypeAttribute("Content-Type", "application/json",
                         "application/vnd.marvin.authorforcreation+json")]
  //[Consumes("application/vnd.marvin.authorforcreation+json")]
  // only consumes also would work here, keeping the other attribute for educational purpose.
  public async Task<ActionResult<AuthorDto>> CreateAuthor(AuthorForCreationDto author)
  {
    var authorEntity = _mapper.Map<Entities.Author>(author);

    _courseLibraryRepository.AddAuthor(authorEntity);
    await _courseLibraryRepository.SaveAsync();

    var authorDto = _mapper.Map<AuthorDto>(authorEntity);
    var links = CreateLinksForAuthor("GetAuthorWithoutLinks", authorEntity.Id, null
                  , Request.ContentType ?? "application/json");
    var authorExpando = authorDto.ShapeData(null);

    authorExpando.TryAdd("links", links);

    return CreatedAtRoute("GetAuthorWithoutLinks",
        new { authorId = authorDto.Id },
        authorExpando);
  }

  [HttpOptions]
  public IActionResult Options()
  {
    Response.Headers.Add("Allow", "GET, HEAD, POST, OPTIONS");
    return Ok();
  }

  [HttpGet("getit")]
  public ActionResult<string> GetIt(Guid authorId)
  {

    var list = Queryable.AsQueryable(new [] {new { name = "def", occ = "cricketer" }, new { name = "abc", occ = "hockey" }});
    var sorted = list.DoSort("name");
    return Ok(list);
  }
}
  /*
  //[ValidateMediaTypeFilter(_problemDetailsFactory)]
  [HttpGet("{authorId}", Name = "GetAuthor")]
  [Produces("application/json",
            "application/vnd.marvin.hateoas+json",
            "application/vnd.marvin.author.full+json",
            "application/vnd.marvin.author.full.hateoas+json",
            "application/vnd.marvin.author.friendly+json",
            "application/vnd.marvin.author.friendly.hateoas+json")]
  public async Task<ActionResult<AuthorDto>> GetAuthor(Guid authorId, string? fields
                                          , [FromHeader(Name = "Accept")] string? mediaType)
  {
     * TODO: Support multiple media type parsing with TryParseList
    var actionProducesAttribute = ControllerContext.ActionDescriptor.
      EndpointMetadata.OfType<ProducesAttribute>().FirstOrDefault();
    var ContentTypes = actionProducesAttribute?.ContentTypes;

    if (!MediaTypeHeaderValue.TryParse(mediaType, out var parsedMediaType))
    {
      return BadRequest(_problemDetailsFactory.CreateProblemDetails(HttpContext,
                             statusCode: 400,
                             detail: $"Accept header media type is not a valid media type"));
    }

    var includeLinks = parsedMediaType.SubTypeWithoutSuffix.
      EndsWith("hateoas", StringComparison.InvariantCultureIgnoreCase);

    ignore the suffix hateos if supplied
    var primaryMediaType = includeLinks ? parsedMediaType.SubTypeWithoutSuffix.Substring(
                              0, parsedMediaType.SubTypeWithoutSuffix.Length - 8)
                            : parsedMediaType.SubTypeWithoutSuffix;

    var fullAuthor = primaryMediaType == "vnd.marvin.author.full";

    if ((fullAuthor && !_propertyCheckerService.TypeHasProperties<AuthorFullDto>(fields)) ||
        (!fullAuthor && !_propertyCheckerService.TypeHasProperties<AuthorDto>(fields)))

      return BadRequest(_problemDetailsFactory.CreateProblemDetails(HttpContext,
            statusCode: 400,
            detail: "Not all requested data shaping fields are present"
                    + $"on the resource {fields}"
            ));

    IDictionary<string, object?> authorToReturn;
    // get author from repo
    var authorFromRepo = await _courseLibraryRepository.GetAuthorAsync(authorId);

    if (authorFromRepo == null)
    {
      return NotFound();
    }

    if (fullAuthor)
    {
      authorToReturn = _mapper.Map<AuthorFullDto>(authorFromRepo)
                                .ShapeData(fields) as IDictionary<string, object?>;
    } 
    else 
    {
      authorToReturn = _mapper.Map<AuthorDto>(authorFromRepo)
                                .ShapeData(fields) as IDictionary<string, object?>;
    }

    if (includeLinks) authorToReturn.Add("links", CreateLinksForAuthor(authorId, fields));

    return Ok(authorToReturn);
  }
*/
