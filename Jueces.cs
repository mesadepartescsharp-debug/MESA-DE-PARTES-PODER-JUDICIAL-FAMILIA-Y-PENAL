using System;
using System.IO;
using static Avances.Program;

namespace Avances
{
    // ==============================================================
    // MÓDULO: JUECES                                   
    // Acceso del juez, expedientes asignados, consulta, actualización
    // de estados, ordenamiento por fecha y estadísticas.
    // ==============================================================
    internal static class Jueces
    {
        // ==========================================================
        // 1. ACCESO DEL JUEZ
        // ==========================================================
        internal static void AccesoJuez()
        {
            string op = "";
            while (op != "2")
            {
                Console.Clear();
                Titulo("ACCESO DEL JUEZ");
                Console.WriteLine("1. Iniciar sesión\n2. Volver\n");
                op = Leer("Opción: ");
                if (op == "1") IniciarSesionJuez();
                else if (op != "2")
                {
                    Console.WriteLine("Opción inválida.");
                    Console.ReadKey();
                }
            }
        }

        static void IniciarSesionJuez()
        {
            Console.Clear();
            Titulo("INICIAR SESIÓN - JUEZ");
            string correo = Leer("Correo institucional: ").ToLower();
            string pass = Leer("Contraseña: ");
            int pos = -1;
            for (int i = 0; i < juecesCorreos.Length; i++)
                if (juecesCorreos[i].ToLower() == correo && juecesContrasenas[i] == pass) pos = i;

            if (pos == -1)
            {
                Console.WriteLine("Incorrecto.");
                Console.ReadKey();
                return;
            }
            MenuJuez(juecesNombres[pos], juecesEspecialidades[pos]);
        }

        // ==========================================================
        // 2. PANEL DEL JUEZ
        // ==========================================================
        static void MenuJuez(string juez, string especialidad)
        {
            string op = "";
            while (op != "5")
            {
                Console.Clear();
                Titulo("PANEL DEL JUEZ");
                Console.WriteLine($"Juez         : {juez}\nEspecialidad : {especialidad}");
                Seccion("RESUMEN DE EXPEDIENTES");
                Console.WriteLine($"Asignados    : {ContarExpedientesJuez(juez)}");
                Console.WriteLine($"Registrados  : {ContarEstadoJuez(juez, "REGISTRADO")}");
                Console.WriteLine($"En revisión  : {ContarEstadoJuez(juez, "EN REVISIÓN")}");
                Console.WriteLine($"Control      : {ContarEstadoJuez(juez, "CONTROL")}");
                Console.WriteLine($"Gestión      : {ContarEstadoJuez(juez, "GESTIÓN")}");
                Console.WriteLine($"Atendidos    : {ContarEstadoJuez(juez, "ATENDIDO")}");
                Seccion("OPCIONES");
                Console.WriteLine("1. Expedientes asignados\n2. Consultar expediente\n3. Expedientes para dar prioridad\n" +
                                  "4. Estadísticas\n5. Cerrar sesión\n");
                op = Leer("Opción: ");
                if (op == "1") MostrarExpedientesJuez(juez);
                else if (op == "2") ConsultarExpedienteJuez(juez);
                else if (op == "3") OrdenarExpedientesJuez(juez);
                else if (op == "4") MostrarEstadisticasJuez(juez, especialidad);
                else if (op != "5")
                {
                    Console.WriteLine("Opción inválida.");
                    Console.ReadKey();
                }
            }
        }

        static int ContarExpedientesJuez(string juez)
        {
            int t = 0;
            for (int i = 0; i < cantidad; i++)
                if (jueces[i] == juez) t++;
            return t;
        }

        static int ContarEstadoJuez(string juez, string est)
        {
            int t = 0;
            for (int i = 0; i < cantidad; i++)
                if (jueces[i] == juez && estados[i] == est) t++;
            return t;
        }

        static void MostrarExpedientesJuez(string juez)
        {
            Console.Clear();
            Titulo("EXPEDIENTES ASIGNADOS");
            bool enc = false;
            for (int i = 0; i < cantidad; i++)
                if (jueces[i] == juez)
                {
                    enc = true;
                    Console.WriteLine($"{codigos[i]} | {tiposDocumento[i]} | " +
                                      $"{fechasRegistro[i]} {MostrarHora12(horasRegistro[i])} | {estados[i]}");
                }
            if (!enc) Console.WriteLine("Ninguno asignado.");
            Console.ReadKey();
        }

        // ==========================================================
        // 3. CONSULTA DE EXPEDIENTE ASIGNADO
        // ==========================================================
        static void ConsultarExpedienteJuez(string juez)
        {
            Titulo("CONSULTAR EXPEDIENTE ASIGNADO");
            string codigo = Leer("Código: ").ToUpper();
            int pos = -1;
            for (int i = 0; i < cantidad; i++)
                if (codigos[i] == codigo && jueces[i] == juez) pos = i;

            if (pos == -1)
            {
                Console.WriteLine("No existe o no asignado.");
                Console.ReadKey();
                return;
            }

            string op = "";
            while (op != "5")
            {
                Console.Clear();
                MostrarDetalleExpedienteJuez(pos);
                Console.WriteLine("1. Abrir PDF principal\n2. Abrir medio probatorio\n3. Abrir archivo complementario\n4. Actualizar estado\n5. Volver");
                op = Leer("Opción: ");
                if (op == "1") Expedientes.AbrirArchivo(documentos[pos]);
                else if (op == "2") AbrirMedioProbatorio(pos);
                else if (op == "3") AbrirArchivoComplementario(pos);
                else if (op == "4") ActualizarEstadoExpediente(pos);
                else if (op == "5") break;
                else
                {
                    Console.WriteLine("Opción inválida.");
                    Console.ReadKey();
                }
            }
        }
        static void AbrirArchivoComplementario(int p)
        {
            Console.Clear();
            Titulo("ARCHIVOS COMPLEMENTARIOS");

            int total = cantidadComplementarios[p];
            if (total == 0)
            {
                Console.WriteLine("No se han adjuntado archivos complementarios a este expediente.");
                Console.ReadKey();
                return;
            }
            for (int i = 0; i < total; i++)
            {
                Console.WriteLine($"{i + 1}. " +
                    Path.GetFileName(archivosComplementarios[p, i]));
            }

            Console.WriteLine("0. Volver");
            string opcion = Leer("\nSeleccione el archivo que desea abrir: ");

            if (opcion == "0")
                return;
            if (int.TryParse(opcion, out int numero) &&
                numero >= 1 && numero <= total)
            {
                string ruta = archivosComplementarios[p, numero - 1];

                if (File.Exists(ruta))
                {
                    Expedientes.AbrirArchivo(ruta);
                }
                else
                {
                    Console.WriteLine("El archivo seleccionado no se encuentra disponible.");
                }
            }
            else
            {
                Console.WriteLine("Opción inválida.");
            }
            Console.WriteLine("\nPresione una tecla para continuar...");
            Console.ReadKey();
        }
        static void MostrarDetalleExpedienteJuez(int p)
        {
            Titulo("DETALLE DEL EXPEDIENTE " + codigos[p]);
            Console.WriteLine("Estado actual        : " + estados[p]);
            Seccion("DATOS DEL USUARIO");
            Console.WriteLine($"Nombre               : {nombres[p]}\nDNI                  : {dnis[p]}");
            Console.WriteLine($"Teléfono             : {telefonos[p]}\nCorreo               : {correos[p]}");
            Seccion("DATOS DEL EXPEDIENTE");
            Console.WriteLine($"Documento            : {tiposDocumento[p]}\n" +
                              $"Especialidad         : {especialidades[p]}");
            Console.WriteLine($"Fecha de registro    : {fechasRegistro[p]}\n" +
                              $"Hora de registro     : {MostrarHora12(horasRegistro[p])}");
            Console.WriteLine($"Descripción          : {descripcionDocumento[p]}");
            Console.WriteLine($"Archivo principal    : {Path.GetFileName(documentos[p])}\n" +
                              $"Folios               : {foliosDocumentos[p]}");
            Seccion("MEDIOS PROBATORIOS (" + cantidadMedios[p] + ")");
            if (cantidadMedios[p] == 0) Console.WriteLine("No se adjuntaron medios adicionales.");
            for (int i = 0; i < cantidadMedios[p]; i++)
            {
                Console.WriteLine($"\nMedio {i + 1}: {tiposMedios[p, i]}");
                Console.WriteLine("Archivo              : " + Path.GetFileName(archivosMedios[p, i]));
                Console.WriteLine("Descripción          : " + descripcionMedios[p, i]);
            }
            Seccion("DOCUMENTACIÓN COMPLEMENTARIA");

            Console.WriteLine("Archivos adicionales presentados: " + cantidadComplementarios[p]);

            if (cantidadComplementarios[p] == 0)
            {
                Console.WriteLine("No se han presentado archivos complementarios.");
            }
            else
            {
                for (int i = 0; i < cantidadComplementarios[p]; i++)
                {
                    Console.WriteLine($"\nArchivo {i + 1}: " +
                        Path.GetFileName(archivosComplementarios[p, i]));

                    Console.WriteLine("Descripción: " +
                        descripcionesComplementarios[p, i]);
                }
            }
            Seccion("OPCIONES DEL JUEZ");
        }

        static void AbrirMedioProbatorio(int p)
        {
            if (cantidadMedios[p] == 0)
            {
                Console.WriteLine("Sin medios.");
                Console.ReadKey();
                return;
            }
            Seccion("SELECCIONAR MEDIO PROBATORIO");
            for (int i = 0; i < cantidadMedios[p]; i++)
                Console.WriteLine($"{i + 1}. {tiposMedios[p, i]} - {Path.GetFileName(archivosMedios[p, i])}");
            int n;
            if (!int.TryParse(Leer("Opción: "), out n) || n < 1 || n > cantidadMedios[p])
            {
                Console.WriteLine("Inválido.");
                Console.ReadKey();
                return;
            }
            Expedientes.AbrirArchivo(archivosMedios[p, n - 1]);
        }

        // ==========================================================
        // 4. ACTUALIZACIÓN DE ESTADO
        // ==========================================================
        static void ActualizarEstadoExpediente(int p)
        {
            Console.Clear();
            Titulo("ACTUALIZAR ESTADO");
            Console.WriteLine($"Expediente  : {codigos[p]}\nEstado actual: {estados[p]}");
            Seccion("SELECCIONE EL NUEVO ESTADO");
            Console.WriteLine("1. EN REVISIÓN\n2. CONTROL\n3. GESTIÓN\n4. ATENDIDO\n5. Cancelar\n");
            string op = Leer("Opción: ");
            if (op == "1") estados[p] = "EN REVISIÓN";
            else if (op == "2") estados[p] = "CONTROL";
            else if (op == "3") estados[p] = "GESTIÓN";
            else if (op == "4") estados[p] = "ATENDIDO";
            else if (op == "5") return;
            else
            {
                Console.WriteLine("Inválido.");
                Console.ReadKey();
                return;
            }

            if (estados[p] == "CONTROL")
            {
                Console.WriteLine("\nCONTROL DOCUMENTAL");
                Console.WriteLine("¿Requiere documentación complementaria?");
                Console.WriteLine("1. Sí");
                Console.WriteLine("2. No");

                string respuesta = Leer("Opción: ");
                requiereComplemento[p] = respuesta == "1" ? "SÍ" : "NO";
                tipoRequerimiento[p] = "";
                observacionesControl[p] = "";

                if (respuesta == "1")
                {
                    Console.WriteLine("\nTIPO DE REQUERIMIENTO");
                    Console.WriteLine("1. Documento complementario");
                    Console.WriteLine("2. Medio probatorio adicional");
                    Console.WriteLine("3. Ambos");

                    string tipo = Leer("Opción: ");
                    tipoRequerimiento[p] = tipo == "1" ? "DOCUMENTO COMPLEMENTARIO"
                    : tipo == "2" ? "MEDIO PROBATORIO ADICIONAL"
                    : tipo == "3" ? "AMBOS" : "";

                    Console.WriteLine("\nINDICACIONES DEL JUEZ");
                    string observacion = Leer("Especifique lo solicitado: ");
                    observacionesControl[p] = observacion;
                }
            }


            Persistencia.GuardarExpedientes();
            string pdf = estados[p] == "EN REVISIÓN"
            ? DocumentosPDF.GenerarPDFRevision(p)
            : estados[p] == "CONTROL"
            ? DocumentosPDF.GenerarPDFControl(p)
            : estados[p] == "GESTIÓN"
            ? DocumentosPDF.GenerarPDFGestion(p)
            : estados[p] == "ATENDIDO"
            ? DocumentosPDF.GenerarPDFAtendido(p)
            : "";
            Console.WriteLine("Estado actualizado.");
            if (pdf != "")
            {
                Console.WriteLine("PDF: " + pdf);
                Correos.EnviarCorreoExpediente(p, pdf, estados[p]);
            }
            Console.ReadKey();
        }

        // ====================================================================
        // 5. ORDENAMIENTO POR FECHA Y ESTADÍSTICAS /AHORA CAMBIADO-> PRIORIDAD
        // ====================================================================
        static void OrdenarExpedientesJuez(string juez)
        {
            int total = 0;
            for (int i = 0; i < cantidad; i++)
                if (jueces[i] == juez && estados[i] != "ATENDIDO")
                    total++;

            if (total == 0)
            {
                Console.Clear();
                Titulo("EXPEDIENTES PARA DAR PRIORIDAD");
                Console.WriteLine("No hay expedientes pendientes de atención.");
                Console.ReadKey();
                return;
            }
            string[,] matrizPrioritarios = new string[total, 5];

            var fec = new string[total];
            var hor = new string[total];
            int idx = 0;

            for (int i = 0; i < cantidad; i++)
                if (jueces[i] == juez && estados[i] != "ATENDIDO")
                {
                    matrizPrioritarios[idx, 0] = codigos[i];
                    matrizPrioritarios[idx, 1] = tiposDocumento[i];
                    matrizPrioritarios[idx, 2] = fechasRegistro[i];
                    matrizPrioritarios[idx, 3] = horasRegistro[i];
                    matrizPrioritarios[idx, 4] = estados[i];

                    fec[idx] = fechasRegistro[i];
                    hor[idx] = horasRegistro[i];
                    idx++;
                }

            for (int i = 0; i < total - 1; i++)
                for (int j = 0; j < total - 1 - i; j++)
                {
                    var f1 = ObtenerFechaHora(fec[j], hor[j]);
                    var f2 = ObtenerFechaHora(fec[j + 1], hor[j + 1]);
                    if (f1 > f2)
                    {
                        string tempFec = fec[j]; fec[j] = fec[j + 1]; fec[j + 1] = tempFec;
                        string tempHor = hor[j]; hor[j] = hor[j + 1]; hor[j + 1] = tempHor;

                        for (int c = 0; c < 5; c++)
                        {
                            string tempM = matrizPrioritarios[j, c];
                            matrizPrioritarios[j, c] = matrizPrioritarios[j + 1, c];
                            matrizPrioritarios[j + 1, c] = tempM;
                        }
                    }
                }

            Console.Clear();
            Titulo("EXPEDIENTES PARA DAR PRIORIDAD");

            for (int i = 0; i < total; i++)
            {
                Console.WriteLine($"[!] Este expediente está desde la fecha: {matrizPrioritarios[i, 2]} a las {MostrarHora12(matrizPrioritarios[i, 3])}");
                Console.WriteLine($"    Código: {matrizPrioritarios[i, 0]} | Tipo: {matrizPrioritarios[i, 1]} | Estado: {matrizPrioritarios[i, 4]}\n");
            }

            Console.ReadKey();
        }

        static void MostrarEstadisticasJuez(string juez, string esp)
        {
            int dem = 0, esc = 0, sol = 0;
            int reg = 0, rev = 0, con = 0, ges = 0, ate = 0;
            for (int i = 0; i < cantidad; i++)
                if (jueces[i] == juez)
                {
                    if (tiposDocumento[i] == "DEMANDA") dem++;
                    else if (tiposDocumento[i] == "ESCRITO") esc++;
                    else if (tiposDocumento[i] == "SOLICITUD") sol++;
                    if (estados[i] == "REGISTRADO") reg++;
                    else if (estados[i] == "EN REVISIÓN") rev++;
                    else if (estados[i] == "CONTROL") con++;
                    else if (estados[i] == "GESTIÓN") ges++;
                    else if (estados[i] == "ATENDIDO") ate++;
                }

            Console.Clear();
            Titulo("ESTADÍSTICAS - " + esp);
            Seccion("TIPOS DE DOCUMENTO");
            Console.WriteLine($"Total       : {dem + esc + sol}\nDemandas    : {dem}\nEscritos    : {esc}\n" +
                              $"Solicitudes : {sol}");
            Seccion("ESTADOS DE LOS EXPEDIENTES");
            Console.WriteLine(
            $"Registrados : {reg}\n" +
            $"En revisión : {rev}\n" +
            $"Control     : {con}\n" +
            $"Gestión     : {ges}\n" +
            $"Atendidos   : {ate}"
         );
            if (dem + esc + sol == 0) Console.WriteLine("Sin datos.");
            else if (dem > esc && dem > sol) Console.WriteLine($"Más frecuente: DEMANDA ({dem})");
            else if (esc > dem && esc > sol) Console.WriteLine($"Más frecuente: ESCRITO ({esc})");
            else if (sol > dem && sol > esc) Console.WriteLine($"Más frecuente: SOLICITUD ({sol})");
            else Console.WriteLine("Empate entre tipos.");
            Console.ReadKey();
        }
    }
}
