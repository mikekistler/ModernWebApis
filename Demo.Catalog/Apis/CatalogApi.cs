using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Catalog.Data;
using Catalog.Models;

namespace Catalog.API;

public static class CatalogApi
{
    public static IEndpointRouteBuilder MapCatalogApi(this IEndpointRouteBuilder app)
    {
        // RouteGroupBuilder for catalog endpoints
        var api = app.MapGroup("catalog");

        // Routes for querying catalog items.
        api.MapGet("/items", GetAllItems);

        api.MapGet("/items/by", GetItemsByIds);

        api.MapGet("/items/{id:int}", GetItemById);

        api.MapGet("/items/{id:int}/pic", GetItemPictureById);

        // Routes for modifying catalog items.
        api.MapPut("/items/{id:int}", UpdateItem);

        api.MapPost("/items", CreateItem);

        api.MapDelete("/items/{id:int}", DeleteItemById);

        return app;
    }

    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    public static async Task<Ok<PaginatedItems<CatalogItem>>> GetAllItems(
        HttpRequest httpRequest,
        [AsParameters] CatalogServices services,
        [AsParameters] PaginationRequest paginationRequest,
        string? name,
        string? type,
        string? brand
    )
    {
        // workaround for https://github.com/dotnet/aspnetcore/issues/61770
        var Context = httpRequest.HttpContext.RequestServices.GetRequiredService<CatalogContext>();

        var pageSize = paginationRequest.PageSize ?? 10;
        var pageIndex = paginationRequest.PageIndex ?? 0;

        var root = (IQueryable<CatalogItem>)Context.CatalogItems;

        if (name is not null)
        {
            root = root.Where(c => c.Name!.StartsWith(name));
        }
        if (type is not null)
        {
            root = root.Where(c => c.CatalogType == type);
        }
        if (brand is not null)
        {
            root = root.Where(c => c.CatalogBrand == brand);
        }

        var totalItems = await root
            .LongCountAsync();

        var itemsOnPage = await root
            .OrderBy(c => c.Name)
            .Skip(pageSize * pageIndex)
            .Take(pageSize)
            .ToListAsync();

        return TypedResults.Ok(new PaginatedItems<CatalogItem>(pageIndex, pageSize, totalItems, itemsOnPage));
    }

    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    public static async Task<Ok<List<CatalogItem>>> GetItemsByIds(
        HttpRequest httpRequest,
        [AsParameters] CatalogServices services,
        int[] ids)
    {
        // workaround for https://github.com/dotnet/aspnetcore/issues/61770
        var Context = httpRequest.HttpContext.RequestServices.GetRequiredService<CatalogContext>();

        var items = await Context.CatalogItems.Where(item => ids.Contains(item.Id)).ToListAsync();
        return TypedResults.Ok(items);
    }

    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    public static async Task<Results<Ok<CatalogItem>, NotFound, BadRequest<ProblemDetails>>> GetItemById(
        HttpRequest httpRequest,
        [AsParameters] CatalogServices services,
        int id)
    {
        if (id <= 0)
        {
            return TypedResults.BadRequest<ProblemDetails>(new (){
                Detail = "Id is not valid"
            });
        }

        // workaround for https://github.com/dotnet/aspnetcore/issues/61770
        var Context = httpRequest.HttpContext.RequestServices.GetRequiredService<CatalogContext>();

        var item = await Context.CatalogItems.SingleOrDefaultAsync(ci => ci.Id == id);

        if (item == null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(item);
    }

    [ProducesResponseType<byte[]>(StatusCodes.Status200OK, "application/octet-stream",
        [ "image/png", "image/gif", "image/jpeg", "image/bmp", "image/tiff",
          "image/wmf", "image/jp2", "image/svg+xml", "image/webp" ])]
    public static async Task<Results<PhysicalFileHttpResult,NotFound>> GetItemPictureById(
        HttpRequest httpRequest,
        IWebHostEnvironment environment,
        int id)
    {
        // workaround for https://github.com/dotnet/aspnetcore/issues/61770
        var Context = httpRequest.HttpContext.RequestServices.GetRequiredService<CatalogContext>();

        var item = await Context.CatalogItems.FindAsync(id);

        if (item is null)
        {
            return TypedResults.NotFound();
        }

        var path = GetFullPath(environment.ContentRootPath, item.PictureFileName!);

        string imageFileExtension = Path.GetExtension(item.PictureFileName!);
        string mimetype = GetImageMimeTypeFromImageFileExtension(imageFileExtension);
        DateTime lastModified = File.GetLastWriteTimeUtc(path);

        return TypedResults.PhysicalFile(path, mimetype, lastModified: lastModified);
    }

    public static async Task<Results<Created, BadRequest<ProblemDetails>, NotFound<ProblemDetails>>> UpdateItemV1(
        HttpRequest httpRequest,
        [AsParameters] CatalogServices services,
        CatalogItem productToUpdate)
    {
        if (productToUpdate?.Id == null)
        {
            return TypedResults.BadRequest<ProblemDetails>(new (){
                Detail = "Item id must be provided in the request body."
            });
        }
        return await UpdateItem(httpRequest.HttpContext, productToUpdate.Id, services, productToUpdate);
    }

    public static async Task<Results<Created, BadRequest<ProblemDetails>, NotFound<ProblemDetails>>> UpdateItem(
        HttpContext httpContext,
        int id,
        [AsParameters] CatalogServices services,
        CatalogItem productToUpdate)
    {
        // workaround for https://github.com/dotnet/aspnetcore/issues/61770
        var Context = httpContext.RequestServices.GetRequiredService<CatalogContext>();

        var catalogItem = await Context.CatalogItems.SingleOrDefaultAsync(i => i.Id == id);

        if (catalogItem == null)
        {
            return TypedResults.NotFound<ProblemDetails>(new (){
                Detail = $"Item with id {id} not found."
            });
        }

        // Update current product
        var catalogEntry = Context.Entry(catalogItem);
        catalogEntry.CurrentValues.SetValues(productToUpdate);

        var priceEntry = catalogEntry.Property(i => i.Price);

        await Context.SaveChangesAsync();

        return TypedResults.Created($"/api/catalog/items/{id}");
    }

    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    public static async Task<Created> CreateItem(
        HttpRequest httpRequest,
        [AsParameters] CatalogServices services,
        CatalogItem product)
    {
        // workaround for https://github.com/dotnet/aspnetcore/issues/61770
        var Context = httpRequest.HttpContext.RequestServices.GetRequiredService<CatalogContext>();

        var item = new CatalogItem
        {
            Id = product.Id,
            CatalogBrand = product.CatalogBrand,
            CatalogType = product.CatalogType,
            Description = product.Description,
            Name = product.Name,
            PictureFileName = product.PictureFileName,
            Price = product.Price,
            AvailableStock = product.AvailableStock,
            RestockThreshold = product.RestockThreshold,
            MaxStockThreshold = product.MaxStockThreshold
        };

        Context.CatalogItems.Add(item);
        await Context.SaveChangesAsync();

        return TypedResults.Created($"/api/catalog/items/{item.Id}");
    }

    public static async Task<Results<NoContent, NotFound>> DeleteItemById(
        HttpRequest httpRequest,
        [AsParameters] CatalogServices services,
        int id)
    {
        // workaround for https://github.com/dotnet/aspnetcore/issues/61770
        var Context = httpRequest.HttpContext.RequestServices.GetRequiredService<CatalogContext>();

        var item = await Context.CatalogItems.SingleOrDefaultAsync(x => x.Id == id);

        if (item is null)
        {
            return TypedResults.NotFound();
        }

        Context.CatalogItems.Remove(item);
        await Context.SaveChangesAsync();
        return TypedResults.NoContent();
    }

    private static string GetImageMimeTypeFromImageFileExtension(string extension) => extension switch
    {
        ".png" => "image/png",
        ".gif" => "image/gif",
        ".jpg" or ".jpeg" => "image/jpeg",
        ".bmp" => "image/bmp",
        ".tiff" => "image/tiff",
        ".wmf" => "image/wmf",
        ".jp2" => "image/jp2",
        ".svg" => "image/svg+xml",
        ".webp" => "image/webp",
        _ => "application/octet-stream",
    };

    public static string GetFullPath(string contentRootPath, string pictureFileName) =>
        Path.Combine(contentRootPath, "Pics", pictureFileName);
}