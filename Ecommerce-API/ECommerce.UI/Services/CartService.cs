using System.Text.Json;
using ECommerce.Shared.Models;
using ECommerce.UI.Configuration;
using ECommerce.UI.Helpers;
using ECommerce.UI.Interfaces;
using Microsoft.Extensions.Configuration;

namespace ECommerce.UI.Services;

public class CartService(IConfiguration configuration) : ICartService
{
    private readonly string _cartPath = Path.Combine(configuration["AppDataPath"] ?? throw new InvalidOperationException("Could not extract AppDataPath from appSettings.json"), "userCart.json");

    public async Task AddToCartAsync(ItemDto item)
    {
        var cart = await GetCartAsync();
        cart.Add(item);
        await SaveCartAsync(cart);
    }

    public async Task<List<ItemDto>> GetCartAsync()
    {
        if (!File.Exists(_cartPath))
            return new List<ItemDto>();

        var json = await File.ReadAllTextAsync(_cartPath);
        return JsonSerializer.Deserialize<List<ItemDto>>(json, Utils.GetJsonSerializerOptions()) ?? new List<ItemDto>();
    }

    public async Task RemoveFromCartAsync(int itemId)
    {
        var cart = await GetCartAsync();
        cart.RemoveAll(i => i.ItemId == itemId);
        await SaveCartAsync(cart);
    }

    public void ClearCart()
    {
        if (File.Exists(_cartPath))
            File.Delete(_cartPath);
    }

    private async Task SaveCartAsync(List<ItemDto> items)
    {
        var directory = Path.GetDirectoryName(_cartPath);
        Directory.CreateDirectory(directory!);

        var json = JsonSerializer.Serialize(items, Utils.GetJsonSerializerOptions());
        await File.WriteAllTextAsync(_cartPath, json);
    }
}