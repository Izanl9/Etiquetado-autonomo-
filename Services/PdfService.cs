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

            // 1. Creamos una lista emparejando cada etiqueta con su índice de copia actual y el total
            var listaPlanaEtiquetas = new List<(Producto Prod, int CopiaActual, int TotalCopias)>();
            foreach (var prod in productos)
            {
                for (int i = 0; i < prod.Cantidad; i++)
                {
                    listaPlanaEtiquetas.Add((prod, i + 1, prod.Cantidad));
                }
            }

            // 2. Cálculo de columnas reales según el ancho de la página A4 (210mm)
            int columnasQueCaben = (int)Math.Floor(210.0 / anchoMm);
            if (columnasQueCaben < 1) columnasQueCaben = 1;

            double espacioSobrante = 210.0 - (columnasQueCaben * anchoMm);
            float margenHorizontalMm = (float)(espacioSobrante / 2.0);

            // 3. Generación del Layout del PDF
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    
                    // Centramos el bloque de etiquetas dinámicamente en el folio
                    page.MarginLeft(margenHorizontalMm, Unit.Millimetre);
                    page.MarginRight(margenHorizontalMm, Unit.Millimetre);
                    page.MarginTop(10, Unit.Millimetre);
                    page.MarginBottom(10, Unit.Millimetre);
                    page.PageColor(QuestColors.White);

                    page.Content().Column(mainColumn =>
                    {
                        // Separación vertical entre las filas de etiquetas
                        mainColumn.Spacing(2, Unit.Millimetre);

                        int index = 0;
                        while (index < listaPlanaEtiquetas.Count)
                        {
                            mainColumn.Item().Row(row =>
                            {
                                for (int c = 0; c < columnasQueCaben && index < listaPlanaEtiquetas.Count; c++)
                                {
                                    var item = listaPlanaEtiquetas[index];
                                    index++;

                                    row.ConstantItem((float)anchoMm, Unit.Millimetre)
                                       .Height((float)altoMm, Unit.Millimetre)
                                       .Background(QuestColors.White)
                                       .Border(0.5f, Unit.Point)
                                       .BorderColor(QuestColors.Grey.Lighten1) // Borde fino limpio
                                       .Padding(8) 
                                       .Column(col =>
                                       {
                                           // Separación uniforme entre textos internos
                                           col.Spacing(2);

                                           // TEXTO 1: Nombre del producto en Mayúsculas
                                           col.Item().Text(item.Prod.Nombre.ToUpper())
                                               .FontSize(8)
                                               .Bold()
                                               .LineHeight(1.1f);

                                           // TEXTO 2: Código identificador debajo
                                           col.Item().Text($"CÓDIGO: {item.Prod.Codigo}")
                                               .FontSize(6)
                                               .FontColor(QuestColors.Grey.Darken1);

                                           // CORRECCIÓN SOLUCIONADA: 
                                           // col.Item().Expand() ahora sí pertenece al contenedor de ítems de la columna.
                                           // Absorbe el espacio vertical restante y empuja de forma limpia el contador al fondo.
                                           col.Item().Expand();

                                           // TEXTO 3: Contador "X de Y" alineado abajo a la derecha
                                           col.Item().AlignRight().Text($"{item.CopiaActual} de {item.TotalCopias}")
                                               .FontSize(7)
                                               .FontColor(QuestColors.Grey.Darken2);
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
