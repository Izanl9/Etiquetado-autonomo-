using System;
using System.Collections.Generic;
using System.IO;
using EtiquetadoAuto.Models;
using Microsoft.Maui.Storage; 
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

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

            // 1. Desglosar todas las unidades físicas de las etiquetas en una lista plana
            var listaPlanaEtiquetas = new List<Producto>();
            foreach (var prod in productos)
            {
                for (int i = 0; i < prod.Cantidad; i++)
                {
                    listaPlanaEtiquetas.Add(prod);
                }
            }

            // 2. CÁLCULO GEOMÉTRICO DINÁMICO (Para evitar deformaciones)
            // El ancho de un papel A4 es fijo: 210mm.
            // Calculamos cuántas columnas reales caben físicamente según el ancho configurado.
            int columnasQueCaben = (int)Math.Floor(210.0 / anchoMm);
            if (columnasQueCaben < 1) columnasQueCaben = 1;

            // Calculamos el espacio sobrante en horizontal para repartirlo a los lados (centrado perfecto)
            double espacioSobrante = 210.0 - (columnasQueCaben * anchoMm);
            float margenHorizontalMm = (float)(espacioSobrante / 2.0);

            // 3. GENERACIÓN DEL DOCUMENTO
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    
                    // Aplicamos el margen dinámico para que queden centradas en la plantilla A4
                    page.MarginLeft(margenHorizontalMm, Unit.Millimetre);
                    page.MarginRight(margenHorizontalMm, Unit.Millimetre);
                    page.MarginTop(10, Unit.Millimetre);
                    page.MarginBottom(10, Unit.Millimetre);
                    page.PageColor(QuestColors.White);

                    page.Content().Column(mainColumn =>
                    {
                        // Espacio de separación vertical entre filas de etiquetas (3mm de margen de corte)
                        mainColumn.Spacing(3, Unit.Millimetre);

                        int index = 0;
                        while (index < listaPlanaEtiquetas.Count)
                        {
                            // Creamos una fila horizontal manual
                            mainColumn.Item().Row(row =>
                            {
                                // Agrupamos las etiquetas según las columnas calculadas
                                for (int c = 0; c < columnasQueCaben && index < listaPlanaEtiquetas.Count; c++)
                                {
                                    var prod = listaPlanaEtiquetas[index];
                                    index++;

                                    // CONGELAMOS EL TAMAÑO REAL REQUERIDO AQUÍ (.ConstantItem)
                                    row.ConstantItem((float)anchoMm, Unit.Millimetre)
                                       .Height((float)altoMm, Unit.Millimetre)
                                       .Background(QuestColors.White)
                                       .Border(0.5f, Unit.Point)
                                       .BorderColor(QuestColors.Grey.Medium)
                                       .Padding(5) 
                                       .Column(col =>
                                       {
                                           col.Spacing(2);

                                           // Fila 1: Código de Referencia
                                           col.Item().Row(r =>
                                           {
                                               r.RelativeItem().Text($"REF: {prod.Codigo}")
                                                   .FontSize(7)
                                                   .FontColor(QuestColors.Grey.Darken2)
                                                   .Bold();
                                           });

                                           // Fila 2: Nombre del Producto (se ajustará al tamaño de la celda)
                                           col.Item().Text(prod.Nombre)
                                               .FontSize(8)
                                               .Bold()
                                               .LineHeight(1.0f);

                                           // Fila 3: Simulación visual del Código de Barras (Abajo del todo)
                                           col.Item().AlignBottom().Row(r =>
                                           {
                                               r.RelativeItem().AlignCenter().Text("||||||  |||||  |||||  |||  ||||")
                                                   .FontFamily("Courier New")
                                                   .FontSize(10);
                                           });
                                       });
                                }
                            });
                        }
                    });
                });
            }).GeneratePdf(rutaCompleta);

            return rutaCompleta;
        }
    }
}
