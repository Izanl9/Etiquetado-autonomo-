using System.Text.Json;
using EtiquetadoAuto.Models;

namespace EtiquetadoAuto.Services;

public class StockService
{
    private string filePath = Path.Combine(FileSystem.AppDataDirectory, "inventario.json");

    public async Task<List<Producto>> GetStockAsync()
    {
        if (!File.Exists(filePath)) return new List<Producto>();
        
        var json = await File.ReadAllTextAsync(filePath);
        return JsonSerializer.Deserialize<List<Producto>>(json) ?? new List<Producto>();
    }

    public async Task ActualizarDesdeAlbaran(List<Producto> nuevosProductos)
    {
        var stockActual = await GetStockAsync();

        foreach (var nuevo in nuevosProductos)
        {
            var existente = stockActual.FirstOrDefault(p => p.Nombre.ToLower() == nuevo.Nombre.ToLower());
            if (existente != null)
                existente.Cantidad += nuevo.Cantidad;
            else
                stockActual.Add(nuevo);
        }

        var json = JsonSerializer.Serialize(stockActual);
        await File.WriteAllTextAsync(filePath, json);
    }
}