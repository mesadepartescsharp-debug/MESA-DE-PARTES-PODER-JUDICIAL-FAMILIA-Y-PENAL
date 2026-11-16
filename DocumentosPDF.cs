using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
using System.IO;
using static Avances.Program;

namespace Avances
{
    // ==============================================================
    // MÓDULO: DOCUMENTOS PDF                                 
    // Generación de las tres constancias PDF con PDFsharp
    // (REGISTRADO, EN REVISIÓN y ATENDIDO) sobre las plantillas PNG.
    // ==============================================================
    internal static class DocumentosPDF
    {
        // ==========================================================
        // 1. RUTAS Y PLANTILLAS
        // ==========================================================
        static string CarpetaPDF()
        {
            if (!Directory.Exists(carpetaPDF)) Directory.CreateDirectory(carpetaPDF);
            return carpetaPDF;
        }

        static string RutaPlantilla(string archivo)
        {
            string r = Path.Combine(carpetaExe, "Plantillas", archivo);
            if (File.Exists(r)) return r;
            return Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..",
            "Plantillas", archivo));
        }

        static void ColocarPlantilla(XGraphics g, string archivo)
        {
            string r = RutaPlantilla(archivo);
            if (!File.Exists(r)) throw new Exception("Plantilla no encontrada: " + r);
            using (var img = XImage.FromFile(r))
                g.DrawImage(img, 0, 0, 595, 842);
        }

        static void EscribirDato(XGraphics g, string texto, double x, double y, double ancho, double tam, bool bold)
        {
            if (texto == null) texto = "";
            g.DrawString(texto, new XFont("Arial", tam, bold ? XFontStyle.Bold : XFontStyle.Regular), XBrushes.Black,
                new XRect(x, y, ancho, 16), XStringFormats.CenterLeft);
        }

        static void EscribirObservacionControl(XGraphics g, string texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return;

            double ancho = 315;
            double alto = 48;
            double inicioX = 235;
            double inicioY = 657;

            for (double tam = 9; tam >= 5; tam -= 0.5)
            {
                var fuente = new XFont("Arial", tam, XFontStyle.Regular);
                var palabras = texto.Split(new[] { ' ' },
                    StringSplitOptions.RemoveEmptyEntries);

                var renglones = new System.Collections.Generic.List<string>();
                string linea = "";

                foreach (string palabra in palabras)
                {
                    string prueba = linea == "" ? palabra : linea + " " + palabra;

                    if (g.MeasureString(prueba, fuente).Width > ancho && linea != "")
                    {
                        renglones.Add(linea);
                        linea = palabra;
                    }
                    else
                    {
                        linea = prueba;
                    }
                }

                if (linea != "") renglones.Add(linea);

                double salto = tam + 4;

                if (renglones.Count * salto <= alto)
                {
                    for (int i = 0; i < renglones.Count; i++)
                    {
                        g.DrawString(renglones[i], fuente, XBrushes.Black,
                            new XRect(inicioX, inicioY + i * salto, ancho, salto),
                            XStringFormats.CenterLeft);
                    }

                    return;
                }
            }

            // Si el texto supera el espacio disponible, se conserva completo
            // en los datos del expediente y se muestra un aviso en el PDF.
            g.DrawString("Observación extensa. Consulte el detalle del expediente.",
                new XFont("Arial", 8, XFontStyle.Regular),
                XBrushes.Black,
                new XRect(inicioX, inicioY, ancho, 16),
                XStringFormats.CenterLeft);
        }


        // ==========================================================
        // 2. ESCRITURA DE DATOS SOBRE LA PLANTILLA
        // ==========================================================
        static void EscribirDatosUsuario(XGraphics g, int p, string est)
        {
            double x = est == "ATENDIDO" ? 242 : 232;
            double y = est == "REGISTRADO" ? 299 : est == "EN REVISIÓN" ? 310 : 329;
            double s = est == "REGISTRADO" ? 25.5 : 22.5;
            EscribirDato(g, nombres[p], x, est == "REGISTRADO" ? y + 5 : y, 310, 10, false);
            EscribirDato(g, dnis[p], x, y + s, 310, 10, false);
            EscribirDato(g, telefonos[p], x, y + s * 2, 310, 10, false);
            EscribirDato(g, correos[p], x, y + s * 3, 310, 9.4, false);
        }

        static void EscribirDatosExpediente(XGraphics g, int p, string est)
        {
            double x = est == "ATENDIDO" ? 242 : 232;
            double y = est == "REGISTRADO" ? 426 : est == "EN REVISIÓN" ? 433 : 453;
            double s = 20.4;
            EscribirDato(g, tiposDocumento[p], x, y, 310, 9.8, false);
            EscribirDato(g, especialidades[p], x, y + s, 310, 9.8, false);
            EscribirDato(g, Path.GetFileName(documentos[p]), x, y + s * 2, 310, 9.2, false);
            EscribirDato(g, descripcionDocumento[p], x, y + s * 3, 310, 9.2, false);
            EscribirDato(g, fechasRegistro[p], x, y + s * 4, 310, 9.6, false);
            EscribirDato(g, MostrarHora12(horasRegistro[p]), x, y + s * 5, 310, 9.6, false);

            if (est == "EN REVISIÓN")
            {
                EscribirDato(g, est, x + 15, y + s * 6 + 5, 130, 9.3, true);
                EscribirDato(g, jueces[p], x, y + s * 7 + 5, 310, 9.3, false);
            }
            else if (est == "ATENDIDO")
            {
                g.DrawString(est, new XFont("Arial", 9.3, XFontStyle.Bold), XBrushes.Black,
                    new XRect(252, y + s * 6, 107, 16), XStringFormats.Center);
                EscribirDato(g, jueces[p], x, y + s * 7 + 7, 310, 9.3, false);
            }
            else if (est == "REGISTRADO")
            {
                g.DrawString(est, new XFont("Arial", 9.3, XFontStyle.Bold), XBrushes.Black,
                    new XRect(245, y + s * 6 + 5, 126, 16), XStringFormats.Center);
                EscribirDato(g, jueces[p], x, y + s * 7 + 5, 310, 9.3, false);
            }
            else
            {
                if (est != "GESTIÓN")
                    EscribirDato(g, est, x, y + s * 6, 310, 9.3, true);

                EscribirDato(g, jueces[p], x, y + s * 7, 310, 9.3, false);
            }
        }

        static void EscribirMediosProbatorios(XGraphics g, int p, string est)
        {
            double y0 = est == "REGISTRADO" ? 660 : est == "EN REVISIÓN" ? 661 : est == "ATENDIDO" ? 686 : 646;
            double s = 20.8;
            if (cantidadMedios[p] == 0)
            {
                EscribirDato(g, "Sin medios probatorios adicionales", 110, y0, 420, 8.6, false);
                return;
            }
            for (int i = 0; i < cantidadMedios[p] && i < 3; i++)
            {
                double y = y0 + s * i;
                string desc = descripcionMedios[p, i] ?? "";
                if (desc.Length > 42) desc = desc.Substring(0, 42) + "...";
                var f = new XFont("Arial", 8.2, XFontStyle.Regular);

                if (est == "EN REVISIÓN")
                {
                    g.DrawString((i + 1).ToString(), f, XBrushes.Black, new XRect(47, y, 61, 16),
                        XStringFormats.Center);
                    g.DrawString(tiposMedios[p, i], f, XBrushes.Black, new XRect(108, y, 104, 16),
                        XStringFormats.Center);
                    g.DrawString(Path.GetFileName(archivosMedios[p, i]), f, XBrushes.Black, new XRect(212, y, 177, 16),
                        XStringFormats.Center);
                    g.DrawString(desc, f, XBrushes.Black, new XRect(389, y, 185, 16), XStringFormats.Center);
                }
                else if (est == "ATENDIDO")
                {
                    g.DrawString((i + 1).ToString(), f, XBrushes.Black, new XRect(70, y, 29, 16),
                        XStringFormats.Center);
                    g.DrawString(tiposMedios[p, i], f, XBrushes.Black, new XRect(99, y, 119, 16),
                        XStringFormats.Center);
                    g.DrawString(Path.GetFileName(archivosMedios[p, i]), f, XBrushes.Black, new XRect(218, y, 172, 16),
                        XStringFormats.Center);
                    g.DrawString(desc, f, XBrushes.Black, new XRect(390, y, 189, 16), XStringFormats.Center);
                }
                else
                {
                    g.DrawString((i + 1).ToString(), f, XBrushes.Black, new XRect(78, y, 29, 16),
                        XStringFormats.Center);
                    g.DrawString(tiposMedios[p, i], f, XBrushes.Black, new XRect(107, y, 115, 16),
                        XStringFormats.Center);
                    g.DrawString(Path.GetFileName(archivosMedios[p, i]), f, XBrushes.Black, new XRect(208, y, 182, 16),
                        XStringFormats.Center);
                    g.DrawString(desc, f, XBrushes.Black, new XRect(390, y, 216, 16), XStringFormats.Center);
                }
            }
        }

        // ==========================================================
        // 3. CONSTANCIAS PDF
        // ==========================================================
        internal static string GenerarPDFRegistro(int p) => CrearPDF(p, "REGISTRADO", "01_REGISTRADO_VINO_A4.png",
            "CONSTANCIA_REGISTRO_");

        internal static string GenerarPDFRevision(int p) => CrearPDF(p, "EN REVISIÓN", "02_EN_REVISION_AZUL_A4.png",
            "ACTUALIZACION_REVISION_");

        internal static string GenerarPDFAtendido(int p) => CrearPDF(p, "ATENDIDO", "03_ATENDIDO_VERDE_A4.png",
            "ACTUALIZACION_ATENDIDO_");
        internal static string GenerarPDFControl(int p)
        {
            return CrearPDFNuevo(p, "CONTROL", "04_CONTROL_MORADO_A4.png", "ACTUALIZACION_CONTROL_");
        }

        internal static string GenerarPDFGestion(int p)
        {
            return CrearPDFNuevo(p, "GESTIÓN", "05_GESTION_MARRON_A4.png", "ACTUALIZACION_GESTION_");
        }
        static string CrearPDFNuevo(int p, string est, string plantilla, string prefijo)
        {
            try
            {
                var pdf = new PdfDocument();
                var pag = pdf.AddPage();
                pag.Size = PdfSharp.PageSize.A4;

                var g = XGraphics.FromPdfPage(pag);
                ColocarPlantilla(g, plantilla);

                string fechaDoc = DateTime.Now.ToString("dd/MM/yyyy hh:mm tt");

                // Código y fecha de actualización
                g.DrawString(codigos[p], new XFont("Arial", 10, XFontStyle.Bold),
                XBrushes.Black, new XRect(420, 180, 150, 16),
                XStringFormats.Center);

                g.DrawString(fechaDoc, new XFont("Arial", 8.5, XFontStyle.Regular),
                    XBrushes.Black, new XRect(420, 231, 150, 16),
                    XStringFormats.Center);

                // Datos del usuario
                double yUsuario = est == "CONTROL" ? 291 : 299;
                double saltoUsuario = est == "CONTROL" ? 20.5 : 22.5;

                EscribirDato(g, nombres[p], 235, yUsuario, 325, 9.5, false);
                EscribirDato(g, dnis[p], 235, yUsuario + saltoUsuario, 325, 9.5, false);
                EscribirDato(g, telefonos[p], 235, yUsuario + saltoUsuario * 2, 325, 9.5, false);
                EscribirDato(g, correos[p], 235, yUsuario + saltoUsuario * 3, 325, 9, false);

                // Datos del expediente
                double yExp = est == "CONTROL" ? 405 : 420;
                double saltoExp = est == "CONTROL" ? 20.5 : 22.5;

                EscribirDato(g, tiposDocumento[p], 235, yExp, 325, 9.5, false);
                EscribirDato(g, especialidades[p], 235, yExp + saltoExp, 325, 9.5, false);
                EscribirDato(g, Path.GetFileName(documentos[p]), 235, yExp + saltoExp * 2, 325, 9, false);
                EscribirDato(g, jueces[p], 235, yExp + saltoExp * 3, 325, 9, false);

                // Estado actual
                if (est != "GESTIÓN")
                {
                    g.DrawString(est, new XFont("Arial", 10, XFontStyle.Bold),
                        XBrushes.Black,
                        new XRect(245, yExp + saltoExp * 4, 130, 18),
                        XStringFormats.Center);
                }
                // Información del control documental
                if (est == "CONTROL")
                {
                    // Marcar la casilla correspondiente
                    var fuenteMarca = new XFont("Arial", 13, XFontStyle.Bold);

                    if (requiereComplemento[p] == "SÍ")
                        g.DrawString("X", fuenteMarca, XBrushes.Black,
                            new XRect(307, 555, 14, 16), XStringFormats.Center);

                    else if (requiereComplemento[p] == "NO")
                        g.DrawString("X", fuenteMarca, XBrushes.Black,
                            new XRect(435, 555, 14, 16), XStringFormats.Center);

                    // Tipo de requerimiento
                    EscribirDato(g, tipoRequerimiento[p] ?? "",
                        235, 630, 325, 9, false);

                    // Detalle u observación del juez
                    EscribirObservacionControl(g, observacionesControl[p] ?? "");


                    // Cubrir únicamente el texto original de indicaciones
                    // Fondo lila para cubrir las indicaciones originales
                    var fondoLila = new XSolidBrush(XColor.FromArgb(243, 232, 248));

                    g.DrawRectangle(fondoLila, 70, 740, 515, 43);

                    // Mensaje del resultado del control
                    var fuenteMensaje = new XFont("Arial", 9, XFontStyle.Regular);
                    var colorMensaje = new XSolidBrush(XColor.FromArgb(75, 35, 95));

                    string linea1 = "La documentación presentada se encuentra conforme para continuar con el trámite.";

                    string linea2 = requiereComplemento[p] == "SÍ"
                        ? "Se solicita información adicional para fortalecer la evaluación del caso."
                        : "No se solicita información complementaria.";

                    g.DrawString(linea1, fuenteMensaje, colorMensaje,
                        new XRect(85, 745, 485, 15), XStringFormats.CenterLeft);

                    g.DrawString(linea2, fuenteMensaje, colorMensaje,
                        new XRect(85, 760, 485, 15), XStringFormats.CenterLeft);
                }
                // Pie de página
                EscribirDato(g, codigos[p], 70, 805, 120, 12, true);
                EscribirDato(g, DateTime.Now.ToString("dd/MM/yyyy"), 490, 805, 85, 9, false);


                string ruta = Path.Combine(CarpetaPDF(), prefijo + codigos[p] + ".pdf");

                pdf.Save(ruta);
                pdf.Close();

                return ruta;
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR PDF: " + ex.Message);
                return "";
            }
        }
        static string CrearPDF(int p, string est, string plantilla, string prefijo)
        {
            try
            {
                var pdf = new PdfDocument();
                var pag = pdf.AddPage();
                pag.Size = PdfSharp.PageSize.A4;
                var g = XGraphics.FromPdfPage(pag);
                ColocarPlantilla(g, plantilla);

                string fechaDoc = est == "REGISTRADO"
                    ? fechasRegistro[p] + "   " + MostrarHora12(horasRegistro[p])
                    : DateTime.Now.ToString("dd/MM/yyyy") + "   " + DateTime.Now.ToString("hh:mm tt").Replace("AM",
                        "a. m.").Replace("PM", "p. m.");

                if (est == "EN REVISIÓN")
                {
                    g.DrawString(codigos[p], new XFont("Arial", 10, XFontStyle.Bold), XBrushes.Black,
                        new XRect(430, 205, 138, 16), XStringFormats.Center);
                    g.DrawString(fechaDoc, new XFont("Arial", 8.2), XBrushes.Black, new XRect(430, 254, 138, 16),
                        XStringFormats.Center);
                }
                else if (est == "ATENDIDO")
                {
                    g.DrawString(codigos[p], new XFont("Arial", 10, XFontStyle.Bold), XBrushes.Black,
                        new XRect(430, 207, 138, 16), XStringFormats.Center);
                    g.DrawString(fechaDoc, new XFont("Arial", 8.2), XBrushes.Black, new XRect(430, 258, 138, 16),
                        XStringFormats.Center);
                }
                else
                {
                    g.DrawString(codigos[p], new XFont("Arial", 10, XFontStyle.Bold), XBrushes.Black,
                        new XRect(430, 194, 138, 16), XStringFormats.Center);
                    g.DrawString(fechaDoc, new XFont("Arial", 8.2), XBrushes.Black, new XRect(430, 239, 138, 16),
                        XStringFormats.Center);
                }

                EscribirDatosUsuario(g, p, est);
                EscribirDatosExpediente(g, p, est);
                EscribirMediosProbatorios(g, p, est);

                if (est == "EN REVISIÓN")
                {
                    g.DrawRectangle(XBrushes.White, 20, 705, 555, 78);
                    g.DrawRectangle(new XSolidBrush(XColor.FromArgb(232, 244, 255)), 68, 711, 500, 58);
                    g.DrawRectangle(new XPen(XColor.FromArgb(180, 210, 235), 0.6), 68, 711, 500, 58);
                    var fa = new XFont("Arial", 11);
                    g.DrawString("Su documentación presentada está siendo revisada por el órgano", fa, XBrushes.Black,
                        new XRect(83, 717, 470, 16), XStringFormats.CenterLeft);
                    g.DrawString("jurisdiccional correspondiente. En caso de requerirse información", fa,
                        XBrushes.Black, new XRect(83, 734, 470, 16), XStringFormats.CenterLeft);
                    g.DrawString("adicional, se le comunicará oportunamente a través de este medio.", fa,
                        XBrushes.Black, new XRect(83, 751, 470, 16), XStringFormats.CenterLeft);
                    var pen = new XPen(XColor.FromArgb(40, 110, 190), 2);
                    g.DrawRectangle(pen, 505, 724, 22, 28);
                    g.DrawLine(pen, 510, 733, 521, 733);
                    g.DrawLine(pen, 510, 739, 521, 739);
                    g.DrawLine(pen, 509, 745, 514, 750);
                    g.DrawLine(pen, 514, 750, 525, 738);
                }
                else if (est == "ATENDIDO")
                {
                    g.DrawRectangle(XBrushes.White, 20, 705, 555, 78);
                    g.DrawRectangle(new XSolidBrush(XColor.FromArgb(235, 248, 238)), 68, 711, 500, 58);
                    g.DrawRectangle(new XPen(XColor.FromArgb(155, 205, 165), 0.6), 68, 711, 500, 58);
                    var fv = new XFont("Arial", 11);
                    g.DrawString("La atención correspondiente a su expediente ha sido registrada", fv, XBrushes.Black,
                        new XRect(83, 717, 400, 16), XStringFormats.CenterLeft);
                    g.DrawString("en el sistema. Puede consultar la información actualizada", fv, XBrushes.Black,
                        new XRect(83, 734, 400, 16), XStringFormats.CenterLeft);
                    g.DrawString("ingresando a la Mesa de Partes Digital.", fv, XBrushes.Black,
                        new XRect(83, 751, 400, 16), XStringFormats.CenterLeft);
                    var pen = new XPen(XColor.FromArgb(35, 130, 65), 2);
                    g.DrawRectangle(pen, 505, 724, 22, 28);
                    g.DrawLine(pen, 510, 733, 521, 733);
                    g.DrawLine(pen, 510, 739, 521, 739);
                    g.DrawLine(pen, 509, 745, 514, 750);
                    g.DrawLine(pen, 514, 750, 525, 738);
                }
                else if (est == "REGISTRADO")
                    g.DrawRectangle(XBrushes.White, 20, 753, 555, 27);

                string prefijoCom = est == "REGISTRADO" ? "CR-" : est == "EN REVISIÓN" ? "AR-" : "AA-";
                string codCom = prefijoCom + codigos[p] + "-" + DateTime.Now.Year;
                g.DrawRectangle(XBrushes.White, 20, 780, 555, 55);
                EscribirDato(g, codCom, 30, 795, 250, 10, true);

                var fechaPie = est == "REGISTRADO" ? ObtenerFechaHora(fechasRegistro[p],
                    horasRegistro[p]) : DateTime.Now;
                if (fechaPie == DateTime.MaxValue) fechaPie = DateTime.Now;
                string[] meses =
                {
                    "enero", "febrero", "marzo", "abril", "mayo", "junio",
                    "julio", "agosto", "septiembre", "octubre", "noviembre", "diciembre"
                };
                EscribirDato(g, "Lima, " + fechaPie.Day + " de " + meses[fechaPie.Month - 1] + " de " + fechaPie.Year,
                    400, 795, 175, 9.5, false);

                string ruta = Path.Combine(CarpetaPDF(), prefijo + codigos[p] + ".pdf");
                pdf.Save(ruta);
                pdf.Close();
                return ruta;
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR PDF: " + ex.Message);
                return "";
            }
        }
    }
}
