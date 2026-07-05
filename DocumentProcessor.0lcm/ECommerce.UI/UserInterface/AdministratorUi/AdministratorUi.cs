using ECommerce.UI.Enums;
using ECommerce.UI.Helpers;
using static ECommerce.UI.Helpers.DisplayHelper;

namespace ECommerce.UI.UserInterface.AdministratorUi;

internal class AdministratorUi(
    ManageProductsUi manageProductsUi,
    ManageProductTagsUi manageProductTagsUi,
    ManageSalesUi manageSalesUi,
    ExportDataUi exportDataUi)
{
    //------- Main Menu Methods -------
    internal async Task MainMenu()
    {
        while (true)
            try
            {
                Console.Clear();
                var option = DisplayMenu<AdminMainMenu>();
                await HandleMainMenuOption(option);
            }
            catch (Exception ex)
            {
                UiHelper.DisplayCaughtException(ex);
            }
    }

    private async Task HandleMainMenuOption(AdminMainMenu option)
    {
        switch (option)
        {
            case AdminMainMenu.ManageProducts:
                await manageProductsUi.ManageProductsMenu();
                break;
            case AdminMainMenu.ManageProductTags:
                await manageProductTagsUi.ManageProductTags();
                break;
            case AdminMainMenu.ManageSales:
                await manageSalesUi.ManageSales();
                break;
            case AdminMainMenu.ExportData:
                await exportDataUi.ExportMenu();
                break;
            case AdminMainMenu.ExitApplication:
                ExitApplication();
                break;
        }
    }

    private static void ExitApplication()
    {
        Environment.Exit(0);
    }
}