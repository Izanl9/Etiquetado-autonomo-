using Firebase.Database;
using Firebase.Database.Query;
using EtiquetadoAuto.Models;

namespace EtiquetadoAuto.Services
{
    public class StockService
    {
        private readonly FirebaseClient _firebase = new FirebaseClient("TU_URL_DE_FIREBASE_AQUI");

        // ESTE ES EL NOMBRE QUE EL COMPILADOR NO ENCONTRABA:
        public async Task<List<Producto>> GetProductosAsync()
        {
            try
            {
                var Productos = await _firebase
                    .Child("Productos")
                    .OnceAsync<Producto>();

                return Productos.Select(item => new Producto
                {
                    Id = item.Object.Id,
                    Quantity = item.Object.Quantity,
                    LastUpdate = item.Object.LastUpdate
                }).ToList();
            }
            catch
            {
                return new List<Producto>();
            }
        }
    }
}