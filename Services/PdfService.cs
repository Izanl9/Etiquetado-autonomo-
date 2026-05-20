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
            // Blindaje de seguridad para el ancho horizontal en el folio A4
            if (anchoMm > 190) anchoMm = 190;
            if (anchoMm < 20) anchoMm = 20; 

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

                    // Calcular cuántas columnas reales caben a lo ancho según el Entry de la pantalla
                    double anchoUtilMm = 210 - 20; 
                    int columnas = (int)(anchoUtilMm / anchoMm);
                    if (columnas < 1) columnas = 1;

                    float[] anchosColumnas = new float[columnas];
                    for (int i = 0; i < columnas; i++)
                    {
                        anchosColumnas[i] = (float)(anchoMm * 2.83465);
                    }

                    // Cuadrícula principal transparente
                    Table tablaGrid = new Table(anchosColumnas);
                    tablaGrid.SetHorizontalAlignment(iText.Layout.Properties.HorizontalAlignment.CENTER);

                    float anchoPuntosEtiqueta = (float)(anchoMm * 2.83465);

                    foreach (var prod in productos)
                    {
                        for (int k = 0; k < prod.Cantidad; k++)
                        {
                            // 🌟 CLAVE 1: Eliminamos altoMm de la celda base. 
                            // Ahora la cuadrícula solo controla el Ancho estricto. El alto será libre.
                            iText.Layout.Element.Cell celdaGrid = new iText.Layout.Element.Cell()
                                .SetWidth(anchoPuntosEtiqueta)
                                .SetPadding(4) // Espacio de separación entre pegatinas vecinas
                                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                                .SetKeepTogether(true);

                            // 🌟 CLAVE 2: Creamos la sub-tabla interna para maquetar la pegatina real (Sin SetHeight fijo)
                            // Al no ponerle un alto fijo, la caja gris se encoge verticalmente eliminando todo el espacio en blanco.
                            Table tarjetaEtiqueta = new Table(1);
                            tarjetaEtiqueta.SetWidth(UnitValue.CreatePercentValue(100));
                            tarjetaEtiqueta.SetBorder(new SolidBorder(iText.Kernel.Colors.ColorConstants.LIGHT_GRAY, 0.5f));

                            // Tamaño de letra basado en el ancho disponible para que no se corte hacia los lados
                            float tamanoNombre = (float)(anchoMm * 0.13);
                            if (tamanoNombre > 11) tamanoNombre = 11; // Forzamos el tamaño exacto de tu foto buena
                            if (tamanoNombre < 8)  tamanoNombre = 8;

                            float tamanoCodigo = tamanoNombre * 0.75f;

                            // Creamos un único párrafo contenedor para que el texto y el contador estén compactados
                            Paragraph pContenido = new Paragraph()
                                .SetMargin(0)
                                .SetPadding(0);

                            // 1. Nombre del producto (Negrita, Izquierda, Mayúsculas)
                            pContenido.Add(new Text(prod.Nombre.ToUpper() + "\n")
                                .SetFontColor(iText.Kernel.Colors.ColorConstants.BLACK)
                                .SetFontSize(tamanoNombre)
                                .SetBold());

                            // 2. Código justo debajo en gris
                            pContenido.Add(new Text($"CÓDIGO: {prod.Codigo}")
                                .SetFontColor(iText.Kernel.Colors.ColorConstants.GRAY)
                                .SetFontSize(tamanoCodigo));

                            // 3. Contador "X de Y" integrado de forma compacta (Alineado a la derecha abajo)
                            Paragraph pContador = new Paragraph($"{k + 1} de {prod.Cantidad}")
                                .SetFontColor(iText.Kernel.Colors.ColorConstants.DARK_GRAY)
                                .SetFontSize(tamanoCodigo * 0.9f)
                                .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                                .SetMarginTop(2) // Separación mínima con el código de arriba
                                .SetMarginBottom(0);

                            // Metemos los textos en la celda de la pegatina
                            iText.Layout.Element.Cell celdaContenido = new iText.Layout.Element.Cell()
                                .Add(pContenido)
                                .Add(pContador)
                                .SetPaddingTop(6)
                                .SetPaddingBottom(4)
                                .SetPaddingLeft(6)
                                .SetPaddingRight(6)
                                .SetBorder(iText.Layout.Borders.Border.NO_BORDER);

                            tarjetaEtiqueta.AddCell(celdaContenido);

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