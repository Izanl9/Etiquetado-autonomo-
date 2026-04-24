using Firebase.Database;
using Firebase.Database.Query;
using EtiquetadoAuto.Models;

namespace EtiquetadoAuto.Services
{
    public class StockService
    {
        private readonly FirebaseClient _firebase = new FirebaseClient("TU_URL_DE_FIREBASE_AQUI");

        // ESTE ES EL NOMBRE QUE EL COMPILADOR NO ENCONTRABA:
        public async Task<List<Product>> GetProductsAsync()
        {
            try
            {
                var products = await _firebase
                    .Child("productos")
                    .OnceAsync<Product>();

                return products.Select(item => new Product
                {
                    Id = item.Object.Id,
                    Quantity = item.Object.Quantity,
                    LastUpdate = item.Object.LastUpdate
                }).ToList();
            }
            catch
            {
                return new List<Product>();
            }
        }
    }
}