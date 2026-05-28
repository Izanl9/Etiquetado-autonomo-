using System;
using System.Collections.Generic;
using System.IO;
using EtiquetadoAuto.Models;
using Microsoft.Maui.Storage; // Necesario para FileSystem.CacheDirectory
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace EtiquetadoAuto.Services
{
    public class PdfService
    {
        public PdfService()
        {
            // Requerido en las versiones modernas de QuestPDF (Gratuito para desarrollo/comunidad)
            QuestPDF.Settings.License = LicenseType.Community;
        }

        /// <summary>
        /// Genera un archivo PDF con etiquetas distribuidas en 3 columnas por fila respetando el tamaño personalizado.
        /// </summary>
        /// <param name="productos">Lista de productos detectados con sus cantidades</param>
        /// <param name="anchoMm">Ancho personalizado en milímetros</param>
        /// <param name="altoMm">Alto personalizado en milímetros</param>
        /// <returns>La ruta física del archivo PDF generado en el dispositivo</returns>
        public string GenerarEtiquetas(List<Producto> productos, double anchoMm, double altoMm)
        {
            // 1. Definir la ruta de guardado temporal segura dentro de Android/iOS
            string nombreArchivo = $"Etiquetas_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            string rutaCarpeta = FileSystem.CacheDirectory; 
            string rutaCompleta = Path.Combine(rutaCarpeta, nombreArchivo);

            // 2. Crear la estructura del PDF
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    // Configuramos un folio estándar A4 (210mm x 297mm) donde se imprimirán las planchas de etiquetas
                    page.Size(PageSizes.A4);
                    page.Margin(10, Unit.Millimetre);
                    page.PageColor(Colors.White);

                    // Creamos una cuadrícula (Grid) interna
                    page.Content().Grid(grid =>
                    {
                        // CONFIGURACIÓN CRUCIAL: Forzamos exactamente 3 columnas por fila
                        grid.Columns(3);
                        
                        // Separación en milímetros entre etiquetas (margen de corte/separación)
                        grid.Spacing(4, Unit.Millimetre); 

                        // 3. Iterar los productos escaneados
                        foreach (var prod in productos)
                        {
                            // Bucle para duplicar la etiqueta física según la cantidad asignada
                            for (int i = 0; i < prod.Cantidad; i++)
                            {
                                // Cada item ocupa 1 de las 3 columnas disponibles del Grid
                                grid.Item(1) 
                                    .Background(Colors.White)
                                    // Aplicamos el tamaño exacto introducido en la pantalla de Blazor
                                    .Width((float)anchoMm, Unit.Millimetre)
                                    .Height((float)altoMm, Unit.Millimetre)
                                    // Línea fina de borde gris para saber por dónde recortar o despegar
                                    .Border(0.5f, Unit.Point)
                                    .BorderColor(Colors.Grey.Medium)
                                    .Padding(5) 
                                    .Column(col =>
                                    {
                                        // FILA 1: Código de Referencia (Arriba)
                                        col.Item().Row(row =>
                                        {
                                            row.RelativeItem().Text($"REF: {prod.Codigo}")
                                                .FontSize(7)
                                                .FontColor(Colors.Grey.Darken2)
                                                .Bold();
                                        });

                                        col.Item().Spacing(3);

                                        // FILA 2: Nombre del Producto (Centro)
                                        // Usamos RelativeItem para que ocupe todo el espacio central disponible
                                        col.RelativeItem().Text(prod.Nombre)
                                            .FontSize(9)
                                            .Bold()
                                            .LineHeight(1.1f);

                                        col.Item().Spacing(3);

                                        // FILA 3: Simulación visual de Código de Barras (Abajo)
                                        col.Item().AlignBottom().Row(row =>
                                        {
                                            // Usamos una fuente monoespaciada para simular líneas de código de barras
                                            row.RelativeItem().Text("||||||  |||||  |||||  |||  ||||")
                                                .FontFamily("Courier New")
                                                .FontSize(11)
                                                .AlignCenter();
                                        });
                                    });
                            }
                        }
                    });
                });
            }).GeneratePdf(rutaCompleta);

            // 4. Devolvemos la ruta para que 'Launcher.Default.OpenAsync' la abra inmediatamente en el móvil
            return rutaCompleta;
        }
    }
}