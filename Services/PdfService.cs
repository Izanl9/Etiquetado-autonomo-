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
            // 1. Forzamos a que el lienzo siempre sea una hoja A4 completa
            PageSize tamanoHoja = PageSize.A4;
            
            string nombreArchivo = $"Etiquetas_Hoja_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            string rutaCompleta = System.IO.Path.Combine(FileSystem.CacheDirectory, nombreArchivo);

            using (PdfWriter writer = new PdfWriter(rutaCompleta))
            {
                using (PdfDocument pdf = new PdfDocument(writer))
                {
                    Document documento = new Document(pdf, tamanoHoja);
                    
                    // Margen de seguridad perimetral del folio (10 mm a cada lado)
                    float margenHojaPuntos = (float)(10 * 2.83465);
                    documento.SetMargins(margenHojaPuntos, margenHojaPuntos, margenHojaPuntos, margenHojaPuntos);

                    // 2. CÁLCULO AUTOMÁTICO DE CUÁNTAS CABEN POR FILA
                    // El espacio útil horizontal de un A4 (210mm) menos los márgenes (20mm) es de 190mm
                    double anchoUtilMm = 210 - 20; 
                    int columnas = (int)(anchoUtilMm / anchoMm);
                    if (columnas < 1) columnas = 1; // Como mínimo, una por fila

                    // Definimos los anchos exactos de las columnas en puntos basados en tu elección
                    float[] anchosColumnas = new float[columnas];
                    for (int i = 0; i < columnas; i++)
                    {
                        anchosColumnas[i] = (float)(anchoMm * 2.83465);
                    }

                    // Creamos la tabla/cuadrícula donde se encajarán las etiquetas
                    Table tablaGrid = new Table(anchosColumnas);
                    tablaGrid.SetHorizontalAlignment(HorizontalAlignment.CENTER);

                    // 3. BUCLE: Creamos tantas celdas como copias hayamos pedido
                    foreach (var prod in productos)
                    {
                        for (int i = 0; i < prod.Cantidad; i++)
                        {
                            // Cada celda es una pegatina individual con el tamaño exacto que elegiste
                            Cell celdaEtiqueta = new Cell()
                                .SetWidth((float)(anchoMm * 2.83465))
                                .SetHeight((float)(altoMm * 2.83465))
                                .SetPadding(5)
                                .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE);

                            // Contenedor del texto centrado dentro de la pegatina
                            Paragraph contenido = new Paragraph()
                                .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);

                            // Código / Referencia arriba
                            Text txtCodigo = new Text($"REF: {prod.Codigo}\n")
                                .SetFontSize(9)
                                .SetBold()
                                .SetFontColor(iText.Kernel.Colors.ColorConstants.DARK_GRAY);
                            contenido.Add(txtCodigo);

                            // Línea divisoria fina
                            contenido.Add(new Text("-----------------------------------\n").SetFontSize(7));

                            // Nombre del producto (Tamaño adaptable según el alto de la etiqueta)
                            float tamanoLetra = (float)(altoMm * 0.22);
                            if (prod.Nombre.Length > 20) tamanoLetra = (float)(altoMm * 0.16);

                            Text txtNombre = new Text(prod.Nombre)
                                .SetFontSize(Math.Max(9, tamanoLetra)) // Nunca baja de tamaño 9 para que sea legible
                                .SetBold()
                                .SetFontColor(iText.Kernel.Colors.ColorConstants.BLACK);
                            contenido.Add(txtNombre);

                            // Metemos el texto en la celda y la celda en la cuadrícula
                            celdaEtiqueta.Add(contenido);
                            tablaGrid.AddCell(celdaEtiqueta);
                        }
                    }

                    // 4. Inyectamos la cuadrícula en el folio.
                    // iText es inteligente: si la cuadrícula supera el alto del A4,
                    // crea una página nueva y sigue dibujando las etiquetas ahí de forma nativa.
                    documento.Add(tablaGrid);
                    documento.Close();
                }
            }

            return rutaCompleta;
        }
    }
}