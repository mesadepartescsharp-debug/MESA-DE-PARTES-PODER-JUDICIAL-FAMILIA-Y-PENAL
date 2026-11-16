using System;
using System.IO;
using static Avances.Program;

namespace Avances
{
   
    internal static class Persistencia
    {
        // ==========================================================
        // 0. CARPETAS Y ARCHIVOS INICIALES
        // ==========================================================
        // Crea Datos, Adjuntos y PDF_Generados, y los TXT vacíos si todavía no existen.
        internal static void PrepararCarpetas()
        {
            Directory.CreateDirectory(carpetaDatos);
            Directory.CreateDirectory(carpetaAdjuntos);
            Directory.CreateDirectory(carpetaPDF);
            MoverArchivoAnterior("usuarios.txt", archivoUsuarios);
            MoverArchivoAnterior("expedientes.txt", archivoDatos);
            if (!File.Exists(archivoUsuarios)) File.Create(archivoUsuarios).Close();
            if (!File.Exists(archivoDatos)) File.Create(archivoDatos).Close();
        }

        // Conserva los registros de versiones anteriores (TXT que estaban junto al .exe).
        static void MoverArchivoAnterior(string nombre, string destino)
        {
            string anterior = Path.Combine(carpetaExe, nombre);
            if (File.Exists(anterior) && !File.Exists(destino)) File.Move(anterior, destino);
        }

        // ==========================================================
        // 1. ARCHIVO DE USUARIOS (usuarios.txt)
        // ==========================================================
        internal static void GuardarUsuarios()
        {
            using (var sw = new StreamWriter(archivoUsuarios, false))
                for (int i = 0; i < cantidadUsuarios; i++)
                    sw.WriteLine($"{usuariosNombres[i]}|{usuariosDnis[i]}|{usuariosTelefonos[i]}|" +
                                 $"{usuariosCorreos[i]}|{usuariosContrasenas[i]}");
        }

        internal static void CargarUsuarios()
        {
            if (!File.Exists(archivoUsuarios)) return;
            foreach (var linea in File.ReadAllLines(archivoUsuarios))
            {
                var d = linea.Split('|');
                if (d.Length == 5 && cantidadUsuarios < 100)
                {
                    usuariosNombres[cantidadUsuarios] = d[0];
                    usuariosDnis[cantidadUsuarios] = d[1];
                    usuariosTelefonos[cantidadUsuarios] = d[2];
                    usuariosCorreos[cantidadUsuarios] = d[3];
                    usuariosContrasenas[cantidadUsuarios] = d[4];
                    cantidadUsuarios++;
                }
            }
        }

        // ==========================================================
        // 2. ARCHIVO DE EXPEDIENTES (expedientes.txt)
        // ==========================================================
        internal static void GuardarExpedientes()
        {
            using (var sw = new StreamWriter(archivoDatos, false))
                for (int i = 0; i < cantidad; i++)
                {
                    sw.WriteLine($"{codigos[i]}|{nombres[i]}|{dnis[i]}|{telefonos[i]}|{correos[i]}|" +
                                 $"{tiposDocumento[i]}|{especialidades[i]}|{jueces[i]}|{documentos[i]}|" +
                                 $"{descripcionDocumento[i]}|{cantidadMedios[i]}|{propietariosDni[i]}|" +
                                 $"{fechasRegistro[i]}|{horasRegistro[i]}|{estados[i]}|{foliosDocumentos[i]}|" +
                                 $"{requiereComplemento[i]}|{tipoRequerimiento[i]}|{observacionesControl[i]?.Replace("|", "/").Replace("\r", " ").Replace("\n", " ")}");
                    for (int j = 0; j < cantidadMedios[i]; j++)
                        sw.WriteLine($"MEDIO|{tiposMedios[i, j]}|{archivosMedios[i, j]}|{descripcionMedios[i, j]}");
                    sw.WriteLine($"COMPLEMENTARIOS|{cantidadComplementarios[i]}");

                    for (int j = 0; j < cantidadComplementarios[i]; j++)
                    {
                        sw.WriteLine($"COMPLEMENTO|{archivosComplementarios[i, j]}|{descripcionesComplementarios[i, j]}");
                    }
                }
        }

        // Lectura secuencial con StreamReader: cada expediente seguido de sus líneas MEDIO.
        internal static void CargarExpedientes()
        {
            if (!File.Exists(archivoDatos)) return;
            using (StreamReader lector = new StreamReader(archivoDatos))
            {
                string linea;
                string pendiente = null;

                while (cantidad < 100 &&
                       (linea = pendiente ?? lector.ReadLine()) != null)
                {
                    pendiente = null;
                    
                    string[] d = linea.Split('|');
                    if (d.Length == 0 || d[0] == "MEDIO" ||
                    (d.Length != 11 && d.Length != 15 && d.Length != 16 && d.Length != 19)) continue;
                    int medios;
                    if (!int.TryParse(d[10], out medios) || medios < 0 || medios > 3) break;
                    codigos[cantidad] = d[0];
                    nombres[cantidad] = d[1];
                    dnis[cantidad] = d[2];
                    telefonos[cantidad] = d[3];
                    correos[cantidad] = d[4];
                    tiposDocumento[cantidad] = d[5];
                    especialidades[cantidad] = d[6];
                    jueces[cantidad] = d[7];
                    documentos[cantidad] = d[8];
                    descripcionDocumento[cantidad] = d[9];
                    propietariosDni[cantidad] = d.Length >= 15 ? d[11] : "";
                    fechasRegistro[cantidad] = d.Length >= 15 ? d[12] : "SIN FECHA";
                    horasRegistro[cantidad] = d.Length >= 15 ? d[13] : "--:--";
                    estados[cantidad] = d.Length >= 15 ? d[14] : "REGISTRADO";
                    int folios;
                    foliosDocumentos[cantidad] = d.Length >= 16 && int.TryParse(d[15], out folios)
                        ? folios
                        : (File.Exists(documentos[cantidad])
                            ? Validaciones.ContarFoliosPDF(documentos[cantidad])
                            : 0);
                    requiereComplemento[cantidad] = d.Length == 19 ? d[16] : "";
                    tipoRequerimiento[cantidad] = d.Length == 19 ? d[17] : "";
                    observacionesControl[cantidad] = d.Length == 19 ? d[18] : "";
                    cantidadMedios[cantidad] = 0;
                    for (int j = 0; j < medios; j++)
                    {
                        string medio = lector.ReadLine();
                        if (medio == null) break;
                        string[] m = medio.Split('|');
                        if (m.Length != 4 || m[0] != "MEDIO") break;
                        tiposMedios[cantidad, j] = m[1];
                        archivosMedios[cantidad, j] = m[2];
                        descripcionMedios[cantidad, j] = m[3];
                        cantidadMedios[cantidad]++;
                    }
                    cantidadComplementarios[cantidad] = 0;

                    string siguiente = lector.ReadLine();

                    if (siguiente != null && siguiente.StartsWith("COMPLEMENTARIOS|"))
                    {
                        string[] datos = siguiente.Split('|');
                        int total;

                        if (datos.Length == 2 &&
                            int.TryParse(datos[1], out total) &&
                            total >= 0 && total <= 10)
                        {
                            for (int j = 0; j < total; j++)
                            {
                                string complemento = lector.ReadLine();
                                if (complemento == null) break;

                                string[] c = complemento.Split('|');

                                if (c.Length != 3 || c[0] != "COMPLEMENTO")
                                {
                                    pendiente = complemento;
                                    break;
                                }

                                archivosComplementarios[cantidad, j] = c[1];
                                descripcionesComplementarios[cantidad, j] = c[2];
                                cantidadComplementarios[cantidad]++;
                            }
                        }
                    }
                    else
                    {
                        pendiente = siguiente;
                    }
                    cantidad++;
                }
            }
        }
    }
}
