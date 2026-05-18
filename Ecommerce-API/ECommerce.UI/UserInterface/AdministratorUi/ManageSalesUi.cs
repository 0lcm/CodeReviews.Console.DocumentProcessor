using System.Net;
using ECommerce.UI.Enums;
using ECommerce.UI.Helpers;
using ECommerce.UI.Interfaces;
using Spectre.Console;
using static ECommerce.UI.Helpers.DisplayHelper;

namespace ECommerce.UI.UserInterface.AdministratorUi;

internal class ManageSalesUi(ISaleService saleService, IVerificationService verificationService)
{
    //------- Menu Methods -------
    internal async Task ManageSales()
    {
        while (true)
        {
            Console.Clear();
            var option = DisplayMenu<ManageSalesMenu>();

            switch (option)
            {
                case ManageSalesMenu.ReviewSales:
                    await ReviewSales();
                    break;
                case ManageSalesMenu.CreateNewSale:
                    await CreateNewSale();
                    break;
                case ManageSalesMenu.DeleteSale:
                    await DeleteSale();
                    break;
                case ManageSalesMenu.Back:
                    return;
            }
        }
    }

    //------- CRUD Menus -------
    private async Task ReviewSales()
    {
        var pageNumber = 1;
        while (true)
            try
            {
                Console.Clear();
                var response = await saleService.GetSalesAsync(pageNumber);
                var table = UiHelper.BuildSaleTable(response);


                DisplayTable(table);

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
                        return;
                }
            }
            catch (HttpRequestException ex)
            {
                if (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    DisplayWarning("No results were found when searching, please try a different search or make sure the database isn't empty.");
                    UiHelper.WaitForUser();
                    return;
                }
                UiHelper.DisplayCaughtException(ex);
                return;
            }
    }

    private async Task CreateNewSale()
    {
        Dictionary<int, int> idQuantityPair = [];
        while (true)
        {
            Console.Clear();
            var id = UiHelper.GetArgument("Please enter the ID of the sold item:");
            if (id is null) return;

            var quantity = UiHelper.GetArgument("Please enter the quantity of the sold item:");
            if (quantity is null) return;

            if (!verificationService.TryParseValidQuantity(quantity, out var parsedQuantity)
                || !int.TryParse(id, out var parsedId))
            {
                DisplayWarning("Please enter a valid number that is greater than or equal to 1.");
                UiHelper.WaitForUser();
                continue;
            }

            idQuantityPair.Add(parsedId, parsedQuantity);

            if (!await AnsiConsole.ConfirmAsync("Would you like to add another item to the sale?"))
                break;
        }

        try
        {
            await saleService.PostSaleAsync(idQuantityPair);
            DisplaySuccess("Successfully created a new sale record.");
            UiHelper.WaitForUser();
        }
        catch (HttpRequestException ex)
        {
            UiHelper.DisplayCaughtException(ex);
        }
    }

    private async Task DeleteSale()
    {
        while (true)
        {
            var id = UiHelper.GetArgument("Please enter the ID of the sale to delete:");
            if (id is null) return;

            if (!int.TryParse(id, out var parsedId))
            {
                DisplayWarning("Please enter a valid number that is greater than or equal to 1.");
                UiHelper.WaitForUser();
                continue;
            }

            if (!await AnsiConsole.ConfirmAsync($"Are you sure you want to delete the sale ID {parsedId}?"))
            {
                DisplaySuccess("Deletion was cancelled.");
                UiHelper.WaitForUser();
                return;
            }

            try
            {
                await saleService.DeleteSaleAsync(parsedId);
                DisplaySuccess("Successfully deleted the sale.");
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