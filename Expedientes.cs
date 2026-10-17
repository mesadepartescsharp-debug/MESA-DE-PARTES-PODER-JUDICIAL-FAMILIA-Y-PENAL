using System;
using System.IO;
using System.Windows.Forms;
using static Avances.Program;

namespace Avances
{
    // ==============================================================
    // MÓDULO: EXPEDIENTES                             (TEMAS 2, 3 y 4)
    // Registro de expedientes, medios probatorios, consultas,
    // listados y ordenamiento burbuja.
    // ==============================================================
    internal static class Expedientes
    {
        // ==========================================================
        // 1. REGISTRO DE EXPEDIENTE
        // ==========================================================
        internal static void RegistrarExpediente()
        {
            if (usuarioActual < 0)
            {
                Console.WriteLine("Debe iniciar sesión.");
                Console.ReadKey();
                return;
            }
            if (cantidad >= codigos.Length)
            {
                Console.WriteLine("Capacidad máxima de expedientes.");
                Console.ReadKey();
                return;
            }
            LimpiarMedios(cantidad);
            Console.Clear();
            Titulo("REGISTRO DE EXPEDIENTE");
            Console.WriteLine($"Usuario: {usuariosNombres[usuarioActual]}\nDNI: {usuariosDnis[usuarioActual]}");

            nombres[cantidad] = usuariosNombres[usuarioActual];
            dnis[cantidad] = usuariosDnis[usuarioActual];
            telefonos[cantidad] = usuariosTelefonos[usuarioActual];
            correos[cantidad] = usuariosCorreos[usuarioActual];
            propietariosDni[cantidad] = usuariosDnis[usuarioActual];

            Console.WriteLine("\nTIPO DE DOCUMENTO\n1. Demanda\n2. Escrito\n3. Solicitud");
            string opDoc = LeerHasta("Opción: ", Validaciones.ValidarTipoDocumento);
            tiposDocumento[cantidad] = opDoc == "1" ? "DEMANDA" : opDoc == "2" ? "ESCRITO" : "SOLICITUD";

            Console.WriteLine("\nESPECIALIDAD\n1. Penal\n2. Familia");
            string opEsp = LeerHasta("Opción: ", Validaciones.ValidarEspecialidad);
            especialidades[cantidad] = opEsp == "1" ? "PENAL" : "FAMILIA";

            documentos[cantidad] = SeleccionarDocumento(especialidades[cantidad], dnis[cantidad]);
            foliosDocumentos[cantidad] = Validaciones.ContarFoliosPDF(documentos[cantidad]);
            descripcionDocumento[cantidad] = LeerHasta("Descripción del documento: ",
                Validaciones.ValidarDescripcion);
            RegistrarMedios(cantidad, especialidades[cantidad]);
            RevisarMedios(cantidad);

            string codigoTemp = "EXP-" + (cantidad + 1).ToString("D4");
            string juezTemp = especialidades[cantidad] == "PENAL"
                ? "Dr. Carlos Mendoza Rojas"
                : "Dra. Patricia Salazar Torres";
            string fechaTemp = DateTime.Now.ToString("dd/MM/yyyy");
            string horaTemp = DateTime.Now.ToString("HH:mm");

            Titulo("CONFIRMACIÓN DE REGISTRO");
            Console.WriteLine("¿Desea confirmar el registro?\n1. Sí\n2. No");
            string conf = Leer("Opción: ");
            while (conf != "1" && conf != "2") conf = Leer("1 o 2: ");
            if (conf == "2")
            {
                LimpiarMedios(cantidad);
                documentos[cantidad] = descripcionDocumento[cantidad] = null;
                Console.WriteLine("Registro cancelado.");
                Console.ReadKey();
                return;
            }

            codigos[cantidad] = codigoTemp;
            jueces[cantidad] = juezTemp;
            fechasRegistro[cantidad] = fechaTemp;
            horasRegistro[cantidad] = horaTemp;
            estados[cantidad] = "REGISTRADO";
            documentos[cantidad] = CopiarAdjunto(documentos[cantidad], codigoTemp);
            for (int i = 0; i < cantidadMedios[cantidad]; i++) archivosMedios[cantidad,
                i] = CopiarAdjunto(archivosMedios[cantidad, i], codigoTemp);
            cantidad++;
            Persistencia.GuardarExpedientes();

            string rutaPdf = DocumentosPDF.GenerarPDFRegistro(cantidad - 1);
            Console.WriteLine("Registrado correctamente.");
            if (rutaPdf != "")
            {
                Console.WriteLine("PDF: " + rutaPdf);
                Correos.EnviarCorreoExpediente(cantidad - 1, rutaPdf, "REGISTRADO");
            }
            Console.ReadKey();
        }

        static string SeleccionarDocumento(string especialidad, string dni)
        {
            string nombreEsperado = especialidad + "-" + dni + ".pdf";
            string ruta = "";
            while (ruta == "")
            {
                Console.WriteLine($"\n=== DOCUMENTO PRINCIPAL ===\n" +
                                  $"Formato: PDF | Máx: 30 MB | Nombre: {nombreEsperado}\n" +
                                  $"Presione ENTER para seleccionar...");
                Console.ReadLine();
                var ventana = new OpenFileDialog
                {
                    Title = "Seleccionar documento",
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    Filter = "Todos (*.*)|*.*"
                };
                if (ventana.ShowDialog() == DialogResult.OK && Validaciones.ValidarDocumento(ventana.FileName,
                    nombreEsperado))
                {
                    int folios = Validaciones.ContarFoliosPDF(ventana.FileName);
                    if (folios < 1 || folios > 100)
                        Console.WriteLine($"ERROR: Debe tener 1-100 folios. Detectados: {folios}");
                    else
                    {
                        ruta = ventana.FileName;
                        Console.WriteLine($"Documento validado. Folios: {folios}");
                    }
                }
                else Console.WriteLine("AVISO: Debe seleccionar un documento.");
            }
            return ruta;
        }

        // ==========================================================
        // 2. MEDIOS PROBATORIOS (arreglos bidimensionales)
        // ==========================================================
        static void RegistrarMedios(int posicion, string especialidad)
        {
            Titulo("MEDIOS PROBATORIOS");
            Console.WriteLine("¿Adjuntar medio?\n1. Sí\n2. No");
            string resp = Leer("Opción: ");
            while (resp != "1" && resp != "2") resp = Leer("ERROR: 1 o 2. Opción: ");

            for (int i = 0; i < 3 && resp == "1"; i++)
            {
                Console.WriteLine("\nTIPO DE MEDIO\n1. Imagen\n2. Audio\n3. Video");
                string op = Leer("Opción: ");
                while (op != "1" && op != "2" && op != "3") op = Leer("ERROR: 1-3. Opción: ");

                string tipo = op == "1" ? "IMAGEN" : op == "2" ? "AUDIO" : "VIDEO";
                int max = op == "1" ? 5 : op == "2" ? 10 : 15;
                string ruta = SeleccionarMedio(tipo, max);
                MostrarEjemplos(especialidad, tipo);
                string desc = LeerHasta("Descripción: ", Validaciones.ValidarDescripcion);

                tiposMedios[posicion, i] = tipo;
                archivosMedios[posicion, i] = ruta;
                descripcionMedios[posicion, i] = desc;
                cantidadMedios[posicion]++;
                Console.WriteLine("Medio agregado.");

                if (i == 2) Console.WriteLine("AVISO: Máximo 3 medios.");
                else
                {
                    Console.WriteLine("¿Otro medio?\n1. Sí\n2. No");
                    resp = Leer("Opción: ");
                    while (resp != "1" && resp != "2") resp = Leer("ERROR: 1 o 2. Opción: ");
                }
            }
        }

        static string SeleccionarMedio(string tipo, int maximo)
        {
            string ruta = "";
            while (ruta == "")
            {
                Console.WriteLine($"\n=== ADJUNTAR {tipo} ===");
                if (tipo == "IMAGEN") Console.WriteLine("Formatos: JPG, JPEG");
                else if (tipo == "AUDIO") Console.WriteLine("Formato: MP3");
                else Console.WriteLine("Formato: MP4");
                Console.WriteLine($"Máximo: {maximo} MB\nPresione ENTER...");
                Console.ReadLine();
                var ventana = new OpenFileDialog
                {
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    Filter = "Todos (*.*)|*.*"
                };
                if (ventana.ShowDialog() == DialogResult.OK && Validaciones.ValidarMedio(ventana.FileName, tipo,
                    maximo))
                    ruta = ventana.FileName;
                else Console.WriteLine("AVISO: Debe seleccionar un archivo.");
            }
            return ruta;
        }

        static void MostrarEjemplos(string especialidad, string tipo)
        {
            Console.WriteLine("\nDescriba brevemente el archivo y su relación con el caso.\nEjemplos:");
            if (especialidad == "FAMILIA")
            {
                if (tipo == "IMAGEN") Console.WriteLine("- Fotografía del domicilio familiar.\n" +
                                                        "- Fotografía de bienes en controversia.");
                else if (tipo == "AUDIO") Console.WriteLine("- Audio de conversación del caso.\n" +
                                                            "- Audio entre las partes.");
                else Console.WriteLine("- Video del lugar de los hechos.\n- Video de situación familiar.");
            }
            else
            {
                if (tipo == "IMAGEN") Console.WriteLine("- Fotografía del lugar.\n" +
                                                        "- Fotografía de objeto relacionado.");
                else if (tipo == "AUDIO") Console.WriteLine("- Audio de conversación.\n" +
                                                            "- Audio de comunicación del caso.");
                else Console.WriteLine("- Video del lugar.\n- Video de situación del caso.");
            }
        }

        static void RevisarMedios(int p)
        {
            if (cantidadMedios[p] == 0) return;
            Titulo("REVISIÓN DE MEDIOS PROBATORIOS");
            Console.WriteLine("¿Desea revisar sus medios probatorios?\n1. Sí\n2. No");
            string respuesta = Leer("Opción: ");
            while (respuesta != "1" && respuesta != "2") respuesta = Leer("Ingrese 1 o 2: ");
            if (respuesta == "2") return;
            while (cantidadMedios[p] > 0)
            {
                Titulo("MEDIOS PROBATORIOS ADJUNTADOS");
                for (int i = 0; i < cantidadMedios[p]; i++)
                    Console.WriteLine($"{i + 1}. {tiposMedios[p, i]} - {Path.GetFileName(archivosMedios[p, i])}");
                Console.WriteLine("\n1. Abrir y revisar un medio probatorio\n2. Eliminar un medio probatorio\n" +
                                  "3. Finalizar revisión");
                string op = Leer("Opción: ");
                if (op == "3") break;
                if (op != "1" && op != "2")
                {
                    Console.WriteLine("Opción inválida.");
                    continue;
                }
                int n;
                if (!int.TryParse(Leer("Número de medio: "), out n) || n < 1 || n > cantidadMedios[p])
                {
                    Console.WriteLine("Medio inválido.");
                    continue;
                }
                int k = n - 1;
                if (op == "1")
                {
                    AbrirArchivo(archivosMedios[p, k]);
                    continue;
                }
                string nombre = Path.GetFileName(archivosMedios[p, k]);
                Console.WriteLine($"¿Retirar {nombre}?\n1. Sí\n2. No");
                if (Leer("Opción: ") != "1") continue;
                for (int i = k; i < cantidadMedios[p] - 1; i++)
                {
                    tiposMedios[p, i] = tiposMedios[p, i + 1];
                    archivosMedios[p, i] = archivosMedios[p, i + 1];
                    descripcionMedios[p, i] = descripcionMedios[p, i + 1];
                }
                cantidadMedios[p]--;
                tiposMedios[p, cantidadMedios[p]] = null;
                archivosMedios[p, cantidadMedios[p]] = null;
                descripcionMedios[p, cantidadMedios[p]] = null;
                Console.WriteLine($"Medio probatorio retirado correctamente: {nombre}");
                Console.Write("Presione ENTER para continuar...");
                Console.ReadLine();
                break; // Tras eliminar un medio se pasa a la confirmación final.
            }
        }

        // Guarda una copia del archivo en Adjuntos\<código>\ y devuelve la nueva ruta.
        static string CopiarAdjunto(string ruta, string codigo)
        {
            try
            {
                string carpeta = Path.Combine(carpetaAdjuntos, codigo);
                Directory.CreateDirectory(carpeta);
                string destino = Path.Combine(carpeta, Path.GetFileName(ruta));
                int n = 2;
                while (File.Exists(destino))
                {
                    destino = Path.Combine(carpeta, n + "_" + Path.GetFileName(ruta));
                    n++;
                }
                File.Copy(ruta, destino);
                return destino;
            }
            catch (Exception ex)
            {
                Console.WriteLine("AVISO: no se pudo copiar el adjunto: " + ex.Message);
                return ruta;
            }
        }

        // Elimina solo las referencias en memoria; no borra el archivo físico.
        static void LimpiarMedios(int p)
        {
            for (int i = 0; i < 3; i++)
            {
                tiposMedios[p, i] = archivosMedios[p, i] = descripcionMedios[p, i] = null;
            }
            cantidadMedios[p] = 0;
        }

        internal static void AbrirArchivo(string ruta)
        {
            if (string.IsNullOrEmpty(ruta) || !File.Exists(ruta))
            {
                Console.WriteLine("Archivo no disponible.");
                Console.ReadKey();
                return;
            }
            try
            {
                System.Diagnostics.Process.Start(ruta);
                Console.WriteLine("Abierto.");
            }
            catch
            {
                Console.WriteLine("Error al abrir.");
            }
            Console.ReadKey();
        }

        // ==========================================================
        // 3. CONSULTAS Y LISTADOS
        // ==========================================================
        static void BuscarExpediente()
        {
            string codigo = Leer("Código: ").ToUpper();
            int pos = -1;
            for (int i = 0; i < cantidad; i++)
                if (codigos[i] == codigo) pos = i;

            if (pos == -1) Console.WriteLine("No encontrado.");
            else
            {
                Console.WriteLine($"\n=== DATOS ===\nNombre: {nombres[pos]}\nDNI: {dnis[pos]}\n" +
                                  $"Teléfono: {telefonos[pos]}\nCorreo: {correos[pos]}\n" +
                                  $"Tipo: {tiposDocumento[pos]}\nEspecialidad: {especialidades[pos]}\n" +
                                  $"Juez: {jueces[pos]}\nDocumento: {Path.GetFileName(documentos[pos])}\n" +
                                  $"Folios: {foliosDocumentos[pos]}\nDescripción: {descripcionDocumento[pos]}\n" +
                                  $"Código: {codigos[pos]}\nEstado: {estados[pos]}\nMedios: {cantidadMedios[pos]}");
                for (int i = 0; i < cantidadMedios[pos]; i++)
                    Console.WriteLine($"Medio {i + 1}: {tiposMedios[pos, i]} | " +
                                      $"{Path.GetFileName(archivosMedios[pos, i])} | {descripcionMedios[pos, i]}");
            }
            Console.ReadKey();
        }

        static void MostrarExpedientes()
        {
            Console.Clear();
            Titulo("EXPEDIENTES REGISTRADOS");
            if (cantidad == 0) Console.WriteLine("Ninguno.");
            for (int i = 0; i < cantidad; i++)
                Console.WriteLine($"{codigos[i]} | {tiposDocumento[i]} | {especialidades[i]} | {nombres[i]}");
            Console.ReadKey();
        }

        // ==========================================================
        // 4. ORDENAMIENTO BURBUJA
        // ==========================================================
        // Ordenamiento burbuja ascendente por código.
        static void OrdenarExpedientes()
        {
            for (int i = 0; i < cantidad - 1; i++)
                for (int j = 0; j < cantidad - 1; j++)
                    if (string.Compare(codigos[j], codigos[j + 1]) > 0)
                        IntercambiarExpediente(j, j + 1);

            Persistencia.GuardarExpedientes();
            Console.Clear();
            Titulo("EXPEDIENTES ORDENADOS");
            for (int i = 0; i < cantidad; i++)
                Console.WriteLine($"{codigos[i]} | {tiposDocumento[i]} | {especialidades[i]} | {nombres[i]}");
            Console.WriteLine("Ordenados ascendente por código.");
            Console.ReadKey();
        }

        static void IntercambiarExpediente(int a, int b)
        {
            Cambiar(codigos, a);
            Cambiar(nombres, a);
            Cambiar(dnis, a);
            Cambiar(telefonos, a);
            Cambiar(correos, a);
            Cambiar(tiposDocumento, a);
            Cambiar(especialidades, a);
            Cambiar(jueces, a);
            Cambiar(documentos, a);
            Cambiar(descripcionDocumento, a);
            Cambiar(propietariosDni, a);
            Cambiar(fechasRegistro, a);
            Cambiar(horasRegistro, a);
            Cambiar(estados, a);

            int tf = foliosDocumentos[a]; foliosDocumentos[a] = foliosDocumentos[b]; foliosDocumentos[b] = tf;
            int tc = cantidadMedios[a]; cantidadMedios[a] = cantidadMedios[b]; cantidadMedios[b] = tc;

            for (int k = 0; k < 3; k++)
            {
                CambiarMatriz(tiposMedios, a, k);
                CambiarMatriz(archivosMedios, a, k);
                CambiarMatriz(descripcionMedios, a, k);
            }
        }

        static void Cambiar(string[] a, int p)
        {
            string t = a[p];
            a[p] = a[p + 1];
            a[p + 1] = t;
        }

        static void CambiarMatriz(string[,] m, int f, int c)
        {
            string t = m[f, c];
            m[f, c] = m[f + 1, c];
            m[f + 1, c] = t;
        }

        // ==========================================================
        // 5. MENÚ DE CONSULTA DE EXPEDIENTES
        // ==========================================================
        internal static void MenuConsultaExpedientes()
        {
            string op = "";
            while (op != "5")
            {
                Console.Clear();
                Titulo("MESA DE PARTES");
                Console.WriteLine("1. Registrar expediente\n2. Buscar expediente\n3. Mostrar expedientes\n" +
                                  "4. Ordenar expedientes\n5. Volver\n");
                op = Leer("Opción: ");
                switch (op)
                {
                    case "1":
                        RegistrarExpediente();
                        break;
                    case "2":
                        BuscarExpediente();
                        break;
                    case "3":
                        MostrarExpedientes();
                        break;
                    case "4":
                        OrdenarExpedientes();
                        break;
                }
            }
        }
    }
}
