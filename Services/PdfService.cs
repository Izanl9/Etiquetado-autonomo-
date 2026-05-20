using System;
using System.Collections.Generic;
using System.IO;
using EtiquetadoAuto.Models;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;

namespace EtiquetadoAuto.Services
{
    public class PdfService
    {
        public string GenerarEtiquetas(List<Producto> productos, double anchoMm = 80, double altoMm = 50)
        {
            // 1. BLINDAJE DE MEDIDAS: El espacio máximo útil de un A4 con márgenes de 10mm es 190x277mm
            if (anchoMm > 190) anchoMm = 190;
            if (altoMm > 277) altoMm = 277;
            if (anchoMm < 20) anchoMm = 20; // Tamaño mínimo seguro para que quepa texto
            if (altoMm < 15) altoMm = 15;

            PageSize tamanoHoja = PageSize.A4;
            string nombreArchivo = $"Etiquetas_Hoja_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            string rutaCompleta = System.IO.Path.Combine(FileSystem.CacheDirectory, nombreArchivo);

            using (PdfWriter writer = new PdfWriter(rutaCompleta))
            {
                using (PdfDocument pdf = new PdfDocument(writer))
                {
                    Document documento = new Document(pdf, tamanoHoja);
                    
                    // Margen perimetral del folio A4 (10 mm)
                    float margenHojaPuntos = (float)(10 * 2.83465);
                    documento.SetMargins(margenHojaPuntos, margenHojaPuntos, margenHojaPuntos, margenHojaPuntos);

                    // Calcular cuántas columnas caben exactamente en el ancho útil (190 mm)
                    double anchoUtilMm = 210 - 20; 
                    int columnas = (int)(anchoUtilMm / anchoMm);
                    if (columnas < 1) columnas = 1;

                    // Configurar el ancho real de las columnas en puntos
                    float[] anchosColumnas = new float[columnas];
                    for (int i = 0; i < columnas; i++)
                    {
                        anchosColumnas[i] = (float)(anchoMm * 2.83465);
                    }

                    Table tablaGrid = new Table(anchosColumnas);
                    tablaGrid.SetHorizontalAlignment(iText.Layout.Properties.HorizontalAlignment.CENTER);

                    // Factor de conversión a puntos
                    float altoPuntosEtiqueta = (float)(altoMm * 2.83465);
                    float anchoPuntosEtiqueta = (float)(anchoMm * 2.83465);

                    foreach (var prod in productos)
                    {
                        for (int i = 0; i < prod.Cantidad; i++)
                        {
                            // Crear la celda con tamaño fijo estricto para que actúe como contenedor cerrado
                            iText.Layout.Element.Cell celdaEtiqueta = new iText.Layout.Element.Cell()
                                .SetWidth(anchoPuntosEtiqueta)
                                .SetHeight(altoPuntosEtiqueta)
                                .SetPadding(4) // Margen interno pequeño para aprovechar el espacio
                                .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                                .SetKeepTogether(true); // Evita que una etiqueta se parta entre dos hojas

                            Paragraph contenedor = new Paragraph()
                                .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);

                            // --- CALCULO INTELIGENTE DE FUENTES (ANTI-CORTE) ---
                            // El código de barras/REF ocupa un tamaño pequeño proporcional al alto
                            float tamanoRef = (float)(altoMm * 0.18);
                            if (tamanoRef > 11) tamanoRef = 11; // Techo máximo para que no sea gigante
                            if (tamanoRef < 7)  tamanoRef = 7;  // Suelo mínimo para que sea legible

                            Text txtCodigo = new Text($"REF: {prod.Codigo}\n")
                                .SetFontSize(tamanoRef)
                                .SetBold()
                                .SetFontColor(iText.Kernel.Colors.ColorConstants.DARK_GRAY);
                            contenedor.Add(txtCodigo);

                            // Separador dinámico (se acorta si la etiqueta es estrecha)
                            string separador = anchoMm < 50 ? "---------\n" : "---------------------------\n";
                            contenedor.Add(new Text(separador).SetFontSize(6));

                            // Ajuste extremo del nombre del producto
                            // Base teórica: el texto ocupará el 25% del alto de la pegatina
                            float tamanoLetraNombre = (float)(altoMm * 0.25);

                            // Reducción por longitud de caracteres (si el texto es un párrafo, encoge la letra)
                            if (prod.Nombre.Length > 15 && prod.Nombre.Length <= 30)
                                tamanoLetraNombre *= 0.8f; 
                            else if (prod.Nombre.Length > 30)
                                tamanoLetraNombre *= 0.6f;

                            // Reducción extra si la etiqueta es muy estrecha horizontalmente
                            if (anchoMm < 60 && tamanoLetraNombre > 12)
                                tamanoLetraNombre = 12;

                            // Límites absolutos de lectura
                            if (tamanoLetraNombre > 24) tamanoLetraNombre = 24; // No sobredimensionar en etiquetas gigantes
                            if (tamanoLetraNombre < 8)  tamanoLetraNombre = 8;  // No encoger a modo invisible

                            Text txtNombre = new Text(prod.Nombre)
                                .SetFontSize(tamanoLetraNombre)
                                .SetBold()
                                .SetFontColor(iText.Kernel.Colors.ColorConstants.BLACK);
                            contenedor.Add(txtNombre);

                            // Meter contenido en la celda
                            celdaEtiqueta.Add(contenedor);
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