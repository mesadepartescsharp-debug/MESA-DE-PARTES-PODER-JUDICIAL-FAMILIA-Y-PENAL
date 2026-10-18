# MESA DE PARTES DIGITAL DEL PODER JUDICIAL

## Descripción del proyecto

El presente proyecto consiste en el desarrollo de un prototipo académico de Mesa de Partes Digital, elaborado en C# mediante Visual Studio y .NET Framework 4.7.2.

Su finalidad es facilitar el registro, consulta y seguimiento de expedientes correspondientes a las especialidades Penal y Familia, mediante la digitalización de procedimientos y la organización de información.

El sistema contempla dos tipos de acceso: usuarios y jueces, cada uno con funcionalidades específicas.

## Funcionalidades principales

- Registro e inicio de sesión de usuarios.
- Validación de datos personales y contraseñas.
- Recuperación de contraseñas mediante correo electrónico.
- Registro de expedientes de las especialidades Penal y Familia.
- Incorporación de documentos principales y medios probatorios.
- Búsqueda y consulta de expedientes.
- Ordenamiento de información mediante el algoritmo de burbuja.
- Acceso de jueces y actualización de estados de expedientes.
- Generación de constancias y comunicaciones en formato PDF.
- Envío de notificaciones mediante correo electrónico.
- Persistencia de información mediante archivos TXT.

## Organización del código

El proyecto se encuentra organizado en ocho archivos principales:

| Módulo | Funcionalidad |
|---|---|
| `Program.cs` | Estructura principal y menús del sistema. |
| `Usuarios.cs` | Registro, autenticación y gestión de usuarios. |
| `Validaciones.cs` | Verificación de datos y documentos. |
| `Expedientes.cs` | Registro, búsqueda y consulta de expedientes. |
| `Jueces.cs` | Acceso de jueces y actualización de estados. |
| `Persistencia.cs` | Guardado y recuperación de información. |
| `DocumentosPDF.cs` | Generación de constancias y comunicaciones PDF. |
| `Correos.cs` | Envío de notificaciones electrónicas. |

## Estados de los expedientes

El sistema permite gestionar tres estados:

- **Registrado:** el expediente fue incorporado al sistema.
- **En revisión:** el expediente se encuentra en proceso de evaluación.
- **Atendido:** el expediente ha sido atendido.

Cada estado cuenta con una plantilla gráfica que sirve como base para la generación de documentos PDF. El módulo `DocumentosPDF.cs` complementa estas plantillas mediante la incorporación dinámica de información y ajustes de presentación.

## Tecnologías utilizadas

- Lenguaje C#.
- Visual Studio.
- .NET Framework 4.7.2.
- PDFsharp para la generación de documentos PDF.
- Archivos TXT para la persistencia de datos.
- Git y GitHub para el control de versiones.

## Temas académicos aplicados

El desarrollo del sistema permite aplicar conocimientos de programación, entre ellos:

1. Estructuras condicionales y repetitivas.
2. Funciones, métodos y parámetros.
3. Arreglos, búsqueda y ordenamiento burbuja.
4. Cadenas de caracteres y arreglos paralelos.
5. Lectura, escritura y recuperación de información en archivos.

## Finalidad académica

Este proyecto tiene fines exclusivamente académicos y busca representar el funcionamiento de una Mesa de Partes Digital mediante la aplicación de conocimientos de programación y organización modular.

**Nota:** Se trata de un prototipo académico independiente. No constituye una plataforma oficial del Poder Judicial del Perú.
