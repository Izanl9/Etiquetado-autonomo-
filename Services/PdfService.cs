using System;
using System.Collections.Generic;
using System.IO;
using EtiquetadoAuto.Models;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Layout.Borders;

namespace EtiquetadoAuto.Services
{
    public class PdfService
    {
        public string GenerarEtiquetas(List<Producto> productos, double anchoMm = 80, double altoMm = 50)
        {
            // Blindaje de seguridad para que no desborde el folio A4
            if (anchoMm > 190) anchoMm = 190;
            if (altoMm > 277) altoMm = 277;
            if (anchoMm < 20) anchoMm = 20; 
            if (altoMm < 15) altoMm = 15;

            PageSize tamanoHoja = PageSize.A4;
            string nombreArchivo = $"Etiquetas_Consolidadas_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            string rutaCompleta = System.IO.Path.Combine(FileSystem.CacheDirectory, nombreArchivo);

            using (PdfWriter writer = new PdfWriter(rutaCompleta))
            {
                using (PdfDocument pdf = new PdfDocument(writer))
                {
                    Document documento = new Document(pdf, tamanoHoja);
                    
                    // Margen de 10mm alrededor del folio
                    float margenHojaPuntos = (float)(10 * 2.83465);
                    documento.SetMargins(margenHojaPuntos, margenHojaPuntos, margenHojaPuntos, margenHojaPuntos);

                    // Calcular cuántas columnas caben según el ancho solicitado
                    double anchoUtilMm = 210 - 20; 
                    int columnas = (int)(anchoUtilMm / anchoMm);
                    if (columnas < 1) columnas = 1;

                    float[] anchosColumnas = new float[columnas];
                    for (int i = 0; i < columnas; i++)
                    {
                        anchosColumnas[i] = (float)(anchoMm * 2.83465);
                    }

                    Table tablaGrid = new Table(anchosColumnas);
                    tablaGrid.SetHorizontalAlignment(iText.Layout.Properties.HorizontalAlignment.CENTER);

                    float altoPuntosEtiqueta = (float)(altoMm * 2.83465);
                    float anchoPuntosEtiqueta = (float)(anchoMm * 2.83465);

                    foreach (var prod in productos)
                    {
                        for (int k = 0; k < prod.Cantidad; k++)
                        {
                            // 1. Contenedor principal de la pegatina (Alineado superior para simular la foto)
                            iText.Layout.Element.Cell celdaEtiqueta = new iText.Layout.Element.Cell()
                                .SetWidth(anchoPuntosEtiqueta)
                                .SetHeight(altoPuntosEtiqueta)
                                .SetPadding(6) // Un poco más de aire en los bordes
                                .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.TOP)
                                .SetKeepTogether(true)
                                .SetBorder(new SolidBorder(iText.Kernel.Colors.ColorConstants.LIGHT_GRAY, 0.5f));

                            // 2. TÍTULO: Nombre del producto arriba del todo, en mayúsculas y alineado a la izquierda
                            float tamanoNombre = (float)(altoMm * 0.16);
                            if (tamanoNombre > 14) tamanoNombre = 14; // Techo visual para que sea elegante
                            if (tamanoNombre < 9)  tamanoNombre = 9;

                            Paragraph pNombre = new Paragraph(prod.Nombre.ToUpper())
                                .SetFontColor(iText.Kernel.Colors.ColorConstants.BLACK)
                                .SetFontSize(tamanoNombre)
                                .SetBold()
                                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                                .SetMarginBottom(2); // Pegado al texto de abajo

                            // 3. SUBTÍTULO: Código en gris y pequeñito justo debajo
                            float tamanoCodigo = (float)(altoMm * 0.10);
                            if (tamanoCodigo > 9) tamanoCodigo = 9;
                            if (tamanoCodigo < 6) tamanoCodigo = 6;

                            Paragraph pCodigo = new Paragraph($"CÓDIGO: {prod.Codigo}")
                                .SetFontColor(iText.Kernel.Colors.ColorConstants.GRAY)
                                .SetFontSize(tamanoCodigo)
                                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT);

                            // 4. CONTADOR (1 de 1): Abajo a la derecha
                            // Para mandarlo al fondo de la etiqueta de forma limpia, usamos margen superior dinámico
                            Paragraph pContador = new Paragraph($"{k + 1} de {prod.Cantidad}")
                                .SetFontColor(iText.Kernel.Colors.ColorConstants.DARK_GRAY)
                                .SetFontSize(tamanoCodigo * 0.9f) // Un pelín más pequeño que el código
                                .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT);

                            // Añadimos los tres bloques en orden a la pegatina
                            celdaEtiqueta.Add(pNombre);
                            celdaEtiqueta.Add(pCodigo);
                            celdaEtiqueta.Add(pContador);

                            tablaGrid.AddCell(celdaEtiqueta);
                        }
                    }

                    documento.Add(tablaGrid);
                    documento.Close();
                }
            }

            return rutaCompleta;
        }
    }
}