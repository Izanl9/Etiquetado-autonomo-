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
            // 1. EL SECRETO: Convertir milímetros a puntos tipográficos (1 mm = 2.83465 points)
            float anchoPuntos = (float)(anchoMm * 2.83465);
            float altoPuntos = (float)(altoMm * 2.83465);

            // 2. Definir la ruta temporal donde se guardará el PDF en el dispositivo
            string nombreArchivo = $"Etiquetas_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            string rutaCompleta = System.IO.Path.Combine(FileSystem.CacheDirectory, nombreArchivo);

            // 3. Inicializar el escritor de iText
            using (PdfWriter writer = new PdfWriter(rutaCompleta))
            {
                // Crear el tamaño de página personalizado usando los puntos calculados
                PageSize tamanoPersonalizado = new PageSize(anchoPuntos, altoPuntos);
                
                using (PdfDocument pdf = new PdfDocument(writer))
                {
                    // Forzar a que el documento use nuestro tamaño por defecto
                    pdf.SetDefaultPageSize(tamanoPersonalizado);

                    // Configurar márgenes pequeños (ej. 5mm de margen) para aprovechar el espacio de la pegatina
                    float margenPuntos = (float)(5 * 2.83465);
                    Document documento = new Document(pdf);
                    documento.SetMargins(margenPuntos, margenPuntos, margenPuntos, margenPuntos);

                    bool esPrimeraPagina = true;

                    // 4. Bucle principal: Generar tantas páginas como "Cantidad" pida cada producto
                    foreach (var prod in productos)
                    {
                        for (int i = 0; i < prod.Cantidad; i++)
                        {
                            // Si no es la primera etiqueta del PDF, añadimos una nueva página física
                            if (!esPrimeraPagina)
                            {
                                documento.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));
                            }
                            esPrimeraPagina = false;

                            // --- DISEÑO INTERNO DE LA ETIQUETA ---
                            // Puedes adaptar este diseño según la estética que busques

                            // Contenedor principal centrado
                            Paragraph contenedor = new Paragraph()
                                .SetTextAlignment(TextAlignment.CENTER)
                                .SetVerticalAlignment(VerticalAlignment.MIDDLE);

                            // Código de barras o Referencia (Texto secundario arriba)
                            Text txtCodigo = new Text($"REF: {prod.Codigo}\n")
                                .SetFontSize(Math.Max(8, (float)(altoMm * 0.2))) // Escala el texto según el alto
                                .SetBold()
                                .SetFontColor(iText.Kernel.Colors.ColorConstants.DARK_GRAY);
                            contenedor.Add(txtCodigo);

                            // Línea de separación estética (opcional)
                            contenedor.Add(new Text("-----------------------------------------\n").SetFontSize(8));

                            // Nombre del producto (Texto principal grande)
                            // Calculamos un tamaño de letra dinámico para que no se desborde en etiquetas enanas
                            float tamanoLetraNombre = (float)(altoMm * 0.3); 
                            if (prod.Nombre.Length > 20) tamanoLetraNombre = (float)(altoMm * 0.22); // Si el texto es largo, lo encogemos un poco

                            Text txtNombre = new Text(prod.Nombre)
                                .SetFontSize(Math.Max(10, tamanoLetraNombre))
                                .SetBold()
                                .SetFontColor(iText.Kernel.Colors.ColorConstants.BLACK);
                            contenedor.Add(txtNombre);

                            // Inyectar el bloque de texto en la página actual de la etiqueta
                            documento.Add(contenedor);
                        }
                    }

                    documento.Close();
                }
            }

            // Devolvemos la ruta del archivo generado para que 'Launcher.OpenAsync' pueda abrirlo
            return rutaCompleta;
        }
    }
}