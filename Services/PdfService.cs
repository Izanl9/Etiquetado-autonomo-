using System;
using System.Collections.Generic;
using System.IO;
using EtiquetadoAuto.Models;
using Microsoft.Maui.Storage; 
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

// SOLUCIÓN AL ERROR CS0104: Creamos un alias exclusivo para evitar conflictos con MAUI Graphics
using QuestColors = QuestPDF.Helpers.Colors;

namespace EtiquetadoAuto.Services
{
    public class PdfService
    {
        public PdfService()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public string GenerarEtiquetas(List<Producto> productos, double anchoMm, double altoMm)
        {
            string nombreArchivo = $"Etiquetas_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            string rutaCarpeta = FileSystem.CacheDirectory; 
            string rutaCompleta = Path.Combine(rutaCarpeta, nombreArchivo);

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(10, Unit.Millimetre);
                    page.PageColor(QuestColors.White); // Corregido con el alias

                    page.Content().Grid(grid =>
                    {
                        grid.Columns(3);
                        grid.Spacing(4, Unit.Millimetre); 

                        foreach (var prod in productos)
                        {
                            for (int i = 0; i < prod.Cantidad; i++)
                            {
                                grid.Item(1) 
                                    .Background(QuestColors.White) // Corregido con el alias
                                    .Width((float)anchoMm, Unit.Millimetre)
                                    .Height((float)altoMm, Unit.Millimetre)
                                    .Border(0.5f, Unit.Point)
                                    .BorderColor(QuestColors.Grey.Medium) // Corregido con el alias
                                    .Padding(5) 
                                    .Column(col =>
                                    {
                                        // CORRECCIÓN CS1061: El espaciado se asigna de forma global a la columna aquí
                                        col.Spacing(3);

                                        // FILA 1: Código de Referencia
                                        col.Item().Row(row =>
                                        {
                                            row.RelativeItem().Text($"REF: {prod.Codigo}")
                                                .FontSize(7)
                                                .FontColor(QuestColors.Grey.Darken2) // Corregido con el alias
                                                .Bold();
                                        });

                                        // FILA 2: Nombre del Producto
                                        // CORRECCIÓN CS1061: Cambiado 'col.RelativeItem()' por 'col.Item()'
                                        col.Item().Text(prod.Nombre)
                                            .FontSize(9)
                                            .Bold()
                                            .LineHeight(1.1f);

                                        // FILA 3: Código de Barras ficticio
                                        col.Item().AlignBottom().Row(row =>
                                        {
                                            // CORRECCIÓN CS1929: .AlignCenter() debe ir ANTES de .Text()
                                            row.RelativeItem().AlignCenter().Text("||||||  |||||  |||||  |||  ||||")
                                                .FontFamily("Courier New")
                                                .FontSize(11);
                                        });
                                    });
                            }
                        }
                    });
                });
            }).GeneratePdf(rutaCompleta);

            return rutaCompleta;
        }
    }
}