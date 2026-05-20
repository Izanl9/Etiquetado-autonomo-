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
            // Blindaje de seguridad para que no se desborde la hoja A4
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
                    
                    // Margen de 10mm alrededor del folio A4
                    float margenHojaPuntos = (float)(10 * 2.83465);
                    documento.SetMargins(margenHojaPuntos, margenHojaPuntos, margenHojaPuntos, margenHojaPuntos);

                    // Calcular cuántas columnas reales caben a lo ancho
                    double anchoUtilMm = 210 - 20; 
                    int columnas = (int)(anchoUtilMm / anchoMm);
                    if (columnas < 1) columnas = 1;

                    float[] anchosColumnas = new float[columnas];
                    for (int i = 0; i < columnas; i++)
                    {
                        anchosColumnas[i] = (float)(anchoMm * 2.83465);
                    }

                    // Cuadrícula principal transparente (actúa como soporte de las etiquetas)
                    Table tablaGrid = new Table(anchosColumnas);
                    tablaGrid.SetHorizontalAlignment(iText.Layout.Properties.HorizontalAlignment.CENTER);

                    float altoPuntosEtiqueta = (float)(altoMm * 2.83465);
                    float anchoPuntosEtiqueta = (float)(anchoMm * 2.83465);

                    foreach (var prod in productos)
                    {
                        for (int k = 0; k < prod.Cantidad; k++)
                        {
                            // Celda base invisible de la cuadrícula
                            iText.Layout.Element.Cell celdaGrid = new iText.Layout.Element.Cell()
                                .SetWidth(anchoPuntosEtiqueta)
                                .SetHeight(altoPuntosEtiqueta)
                                .SetPadding(4) // Espacio de separación entre pegatinas vecinas
                                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                                .SetKeepTogether(true);

                            // 🌟 AQUÍ ESTÁ EL TRUCO: Creamos una sub-tabla interna para maquetar la pegatina real
                            // Tiene 1 columna y estructuramos las filas para forzar las posiciones de la foto
                            Table tarjetaEtiqueta = new Table(1);
                            tarjetaEtiqueta.SetWidth(UnitValue.CreatePercentValue(100));
                            tarjetaEtiqueta.SetHeight(UnitValue.CreatePointValue(altoPuntosEtiqueta - 8)); // Descontamos paddings
                            tarjetaEtiqueta.SetBorder(new SolidBorder(iText.Kernel.Colors.ColorConstants.LIGHT_GRAY, 0.5f));

                            // --- FILA Superior (Nombre + Código) ---
                            float tamanoNombre = (float)(altoMm * 0.15);
                            if (tamanoNombre > 13) tamanoNombre = 13;
                            if (tamanoNombre < 9)  tamanoNombre = 9;

                            float tamanoCodigo = (float)(altoMm * 0.09);
                            if (tamanoCodigo > 8) tamanoCodigo = 8;
                            if (tamanoCodigo < 6) tamanoCodigo = 6;

                            Paragraph pContenidoSuperior = new Paragraph()
                                .SetMargin(0)
                                .SetPadding(0);

                            // Añadimos Nombre (Negrita, Izquierda, Mayúsculas)
                            pContenidoSuperior.Add(new Text(prod.Nombre.ToUpper() + "\n")
                                .SetFontColor(iText.Kernel.Colors.ColorConstants.BLACK)
                                .SetFontSize(tamanoNombre)
                                .SetBold());

                            // Añadimos Código justo debajo
                            pContenidoSuperior.Add(new Text($"CÓDIGO: {prod.Codigo}")
                                .SetFontColor(iText.Kernel.Colors.ColorConstants.GRAY)
                                .SetFontSize(tamanoCodigo));

                            iText.Layout.Element.Cell celdaSuperior = new iText.Layout.Element.Cell()
                                .Add(pContenidoSuperior)
                                .SetPadding(5)
                                .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.TOP)
                                .SetBorder(iText.Layout.Borders.Border.NO_BORDER);

                            // --- FILA INFERIOR (Contador "X de Y") ---
                            Paragraph pContador = new Paragraph($"{k + 1} de {prod.Cantidad}")
                                .SetFontColor(iText.Kernel.Colors.ColorConstants.DARK_GRAY)
                                .SetFontSize(tamanoCodigo * 0.9f)
                                .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                                .SetMargin(0)
                                .SetPadding(0);

                            iText.Layout.Element.Cell celdaInferior = new iText.Layout.Element.Cell()
                                .Add(pContador)
                                .SetPadding(5)
                                .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.BOTTOM) // Empuja el contador abajo del todo
                                .SetBorder(iText.Layout.Borders.Border.NO_BORDER);

                            // Montamos la estructura de la pegatina
                            tarjetaEtiqueta.AddCell(celdaSuperior);
                            tarjetaEtiqueta.AddCell(celdaInferior);

                            // Metemos la pegatina dentro de la cuadrícula general
                            celdaGrid.Add(tarjetaEtiqueta);
                            tablaGrid.AddCell(celdaGrid);
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