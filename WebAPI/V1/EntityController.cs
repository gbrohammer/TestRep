using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.OData;
using WebAPI.Models;
using WebAPI.Services;

namespace WebAPI.V1;

[ApiVersion(1.0)]
public class EntitiesController : ODataController
{
    private readonly EntityService _entityService;

    public EntitiesController(
        EntityService entityService)
    {
        _entityService = entityService;
    }

    [EnableQuery(
        AllowedQueryOptions =
            AllowedQueryOptions.Top |
            AllowedQueryOptions.Skip |
            AllowedQueryOptions.Filter |
            AllowedQueryOptions.OrderBy |
            AllowedQueryOptions.Apply,
        MaxTop = 1000)]
    [HttpGet]
    public async Task<IQueryable<Entity>> Get(
        [FromServices] ODataQueryOptions<Entity> queryOptions,
        [FromQuery(Name = "$top")] int? _ = null, // just to make swagger happy without configuring API versioning
        [FromQuery(Name = "$skip")] int? __ = null,
        [FromQuery(Name = "$filter")] string? ___  =null,
        [FromQuery(Name = "$orderby")] string? ____ = null,
        [FromQuery(Name = "$apply")] string? _____ = "groupby((date),aggregate(yieldData_yield with average as averageYield))"
        )
    {
        var result = await _entityService.GetQueryable(queryOptions);
        return result;
    }
}