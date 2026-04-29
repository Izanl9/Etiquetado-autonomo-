namespace EtiquetadoAuto.Models;

public class Producto
{
    public string Id { get; set; } = string.Empty; // Añadido
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public int Quantity { get; set; } // Añadido para compatibilidad con tu Service
    public DateTime LastUpdate { get; set; } // Añadido
}