using ArtCommissions.BLL;
using ArtCommissions.Common.DTOs;
using ArtCommissions.Common.Exceptions;
using ArtCommissions.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace ArtCommissions.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class TagsController : BaseController
    {
        public TagsController(IServicesManager servicesManager, IConfiguration configuration) : base(servicesManager, configuration) { }

        public async Task<IActionResult> Index(int page = 1)
        {
            var tags = await _servicesManager.TagsService.GetAllAsync(page, _paginationSize);
            return View(tags);
        }

        [HttpPost("tags/create")]
        public async Task<IActionResult> Create(Tag tag)
        {
            return await TryExecuteAsync<IActionResult>(
                async () =>
                {
                    if (!ModelState.IsValid)
                    {
                        Response.StatusCode = StatusCodes.Status400BadRequest;
                        return BadRequest(ModelState.Values.SelectMany(v => v.Errors.Select(b => b.ErrorMessage)).First());
                    }

                    var newModel = await _servicesManager.TagsService.AddAsync(GetCurrentUserId(), tag);
                    return PartialView("_TagsTableRowPartial", newModel);
                },
                async (Exception exception) =>
                {
                    return await Task.Run(() =>
                    {
                        if (exception is UnauthorizedException)
                            return Unauthorized();
                        else
                            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
                    });
                });
        }

        [HttpDelete("tags/{id:int}/delete")]
        public async Task<IActionResult> Remove(int id)
        {
            return await TryExecuteAsync(
                async () =>
                {
                    await _servicesManager.TagsService.RemoveAsync(GetCurrentUserId(), id);
                    return (IActionResult)NoContent();
                },
                async (Exception exception) => await Task.Run(() =>
                    {
                        if (exception is UnauthorizedException)
                            return (IActionResult)Unauthorized();
                        else
                            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
                    })
                );
        }

        [HttpPatch("tags/{id:int}/edit")]
        public async Task<IActionResult> Edit(Tag tag)
        {
            return await TryExecuteAsync(
                async () =>
                {
                    await _servicesManager.TagsService.EditAsync(GetCurrentUserId(), tag);
                    return (IActionResult)Ok();
                },
                async (Exception exception) => await Task.Run(() =>
                {
                    if (exception is SqlException && ((SqlException)exception).Number == 2627)
                        return (IActionResult)BadRequest("This tag name already exists");
                    else
                        return new StatusCodeResult(StatusCodes.Status500InternalServerError);
                }));
        }
    }
}
