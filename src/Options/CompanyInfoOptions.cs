using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ByG_Backend.src.Options
{
    /// <summary>
    /// Representa la configuración de información corporativa de la empresa.
    /// Esta clase se utiliza para mapear la sección correspondiente del archivo appsettings.json
    /// y proveer datos institucionales a los servicios de generación de documentos y correos.
    /// </summary>
    public class CompanyInfoOptions
    {
        /// <summary>
        /// Nombre o Razón Social de la empresa (ej: ByG Ingeniería SpA).
        /// </summary>
        public string BusinessName { get; set; } = "";

        /// <summary>
        /// Rol Único Tributario (RUT) de la empresa.
        /// </summary>
        public string Rut { get; set; } = "";

        /// <summary>
        /// Dirección física de la casa matriz o sucursal principal.
        /// </summary>
        public string Address { get; set; } = "";

        /// <summary>
        /// Número telefónico de contacto institucional.
        /// </summary>
        public string Phone { get; set; } = "";

        /// <summary>
        /// Correo electrónico oficial de contacto o ventas.
        /// </summary>
        public string Email { get; set; } = "";

        /// <summary>
        /// Ciudad o comuna donde reside la empresa (ej: Antofagasta).
        /// </summary>
        public string City { get; set; } = "";
    }
}