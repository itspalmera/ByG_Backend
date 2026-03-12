using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ByG_Backend.src.Helpers
{
    /// <summary>
    /// Clase genérica que estandariza todas las respuestas de la API.
    /// Proporciona una estructura uniforme que facilita el manejo de respuestas exitosas 
    /// y de errores en el lado del cliente (Frontend).
    /// </summary>
    /// <typeparam name="T">El tipo de dato que se devuelve en la propiedad 'Data'.</typeparam>
    /// <param name="success">Indica si la operación se completó con éxito.</param>
    /// <param name="message">Mensaje informativo sobre el resultado de la operación.</param>
    /// <param name="data">El contenido de la respuesta (opcional).</param>
    /// <param name="errors">Lista de mensajes de error detallados en caso de falla (opcional).</param>
    public class ApiResponse<T>(bool success, string message, T? data = default, List<string>? errors = null)
    {
        /// <summary>
        /// Determina el éxito o fracaso de la solicitud HTTP.
        /// Útil para que el frontend decida si mostrar un mensaje de éxito o una alerta de error.
        /// </summary>
        public bool Success { get; set; } = success;

        /// <summary>
        /// Breve descripción del resultado (ej: "Usuario autenticado", "Error de validación").
        /// </summary>
        public string Message { get; set; } = message;

        /// <summary>
        /// Carga útil de la respuesta. Puede ser un objeto simple, una lista o un PagedResponse.
        /// </summary>
        public T? Data { get; set; } = data;

        /// <summary>
        /// Colección de errores específicos, ideal para mostrar validaciones de formularios 
        /// o excepciones detalladas en el entorno de desarrollo.
        /// </summary>
        public List<string>? Errors { get; set; } = errors;
    }
}