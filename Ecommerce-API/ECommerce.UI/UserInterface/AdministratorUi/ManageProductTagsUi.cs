using System.Net;
using ECommerce.Shared.Models;
using ECommerce.UI.Enums;
using ECommerce.UI.Helpers;
using ECommerce.UI.Interfaces;
using Spectre.Console;
using static ECommerce.UI.Helpers.DisplayHelper;

namespace ECommerce.UI.UserInterface.AdministratorUi;

internal class ManageProductTagsUi(ITagService tagService)
{
    private readonly UiHelper _uiHelper = new(tagService);

    //------- Menu Methods -------
    internal async Task ManageProductTags()
    {
        while (true)
        {
            Console.Clear();

            var option = DisplayMenu<ManageProductTagsMenu>();

            switch (option)
            {
                case ManageProductTagsMenu.ReviewTags:
                    await ReviewTags();
                    break;
                case ManageProductTagsMenu.SearchTags:
                    await SearchTags();
                    break;
                case ManageProductTagsMenu.CreateNewTag:
                    await CreateNewTag();
                    break;
                case ManageProductTagsMenu.DeleteTag:
                    await DeleteTag();
                    break;
                case ManageProductTagsMenu.Back:
                    return;
            }
        }
    }

    //------- CRUD Menus -------
    /// <summary>
    ///     Presents a list of tags to the user and handles pagination
    /// </summary>
    /// <param name="searchTerm">optional search term to filter results</param>
    /// <param name="returnTagSelection">returns a list of tags selected by the user if set to true</param>
    internal async Task<List<TagDto>?> ReviewTags(string? searchTerm = null, bool returnTagSelection = false)
    {
        var pageNumber = 1;
        while (true)
            try
            {
                Console.Clear();
                var response = await tagService.GetTagsAsync(pageNumber, searchTerm: searchTerm);

                if (!returnTagSelection)
                {
                    var table = _uiHelper.BuildTagTable(response);
                    DisplayTable(table);
                }
                else
                {
                    var selection = DisplayMultiPrompt(response.Data.Select(t => t.TagName).ToList());
                    var selectedTags = response.Data
                        .Where(t => selection.Contains(t.TagName))
                        .ToList();
                    return selectedTags;
                }

                var option = UiHelper.DisplayPaginationController(response.PageNumber, response.TotalPages);
                switch (option)
                {
                    case PaginationController.LastPage:
                        pageNumber = pageNumber == 1 ? 1 : pageNumber - 1;
                        break;
                    case PaginationController.NextPage:
                        pageNumber += 1;
                        break;
                    case PaginationController.Back:
                        return null;
                }
            }
            catch (HttpRequestException ex)
            {
                if (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    DisplayWarning("No results were found when searching, please try a different search or make sure the database isn't empty.");
                    UiHelper.WaitForUser();
                    return null;
                }
                UiHelper.DisplayCaughtException(ex);
                return null;
            }
    }

    /// <summary>
    /// Searches the database for tags matching a search term
    /// </summary>
    /// <param name="returnSearchTags">Optional parameter to return a list of tags selected from search</param>
    /// <returns></returns>
    internal async Task<List<TagDto>?> SearchTags(bool returnSearchTags = false)
    {
        while (true)
        {
            List<TagDto>? selectedTags = null;
            Console.Clear();

            var searchTerm = UiHelper.GetArgument("Please enter a term to search by:");
            if (searchTerm is null) return null;

            selectedTags = await ReviewTags(searchTerm, returnTagSelection: returnSearchTags);

            if (!await AnsiConsole.ConfirmAsync("Would you like to perform another search?"))
                return returnSearchTags ? selectedTags : null;
        }
    }

    private async Task CreateNewTag()
    {
        while (true)
        {
            var name = UiHelper.GetArgument("Please enter a name for the new tag:");
            if (name is null) return;

            if (!await AnsiConsole.ConfirmAsync($"are you sure you want to create a new tag with the name {name}?"))
                continue;

            try
            {
                await tagService.PostTagAsync(name);
                DisplaySuccess("Successfully  created a new tag");
                UiHelper.WaitForUser();
            }
            catch (HttpRequestException ex)
            {
                UiHelper.DisplayCaughtException(ex);
            }

            return;
        }
    }

    private async Task DeleteTag()
    {
        while (true)
        {
            var id = UiHelper.GetArgument("Please enter the ID of the tag you want to delete:");
            if (id is null) return;

            if (!int.TryParse(id, out var parsedId))
            {
                DisplayWarning("Please enter a valid ID containing only numbers.");
                UiHelper.WaitForUser();
                continue;
            }

            if (!await AnsiConsole.ConfirmAsync($"Are you sure you want to delete the tag ID {id}?"))
                continue;

            try
            {
                await tagService.DeleteTagAsync(parsedId);
                DisplaySuccess("Successfully deleted tag.");
                UiHelper.WaitForUser();
            }
            catch (HttpRequestException ex)
            {
                UiHelper.DisplayCaughtException(ex);
            }

            return;
        }
    }
}