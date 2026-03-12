using System;
using System.Collections.Generic;
using System.Linq;

namespace ByG_Backend.src.Helpers
{
    /// <summary>
    /// Provee constantes de solo lectura para los estados del ciclo de vida de una Solicitud de Compra (Purchase).
    /// Centraliza las etiquetas utilizadas para el seguimiento del flujo desde el requerimiento inicial 
    /// hasta la formalización del pedido.
    /// </summary>
    public static class PurchaseStatuses
    {
        /// <summary>
        /// Estado inicial del requerimiento. La solicitud ha sido creada y el sistema está a la espera 
        /// de que se asignen o seleccionen proveedores para iniciar el proceso de cotización.
        /// </summary>
        public const string Received = "Esperando proveedores";

        /// <summary>
        /// Indica que se ha generado y enviado el documento de Solicitud de Cotización (RFQ) a los proveedores seleccionados.
        /// </summary>
        public const string QuoteSent = "Solicitud de cotización enviada";

        /// <summary>
        /// Las cotizaciones han sido recibidas o el proceso está listo para que un administrador 
        /// revise las ofertas y seleccione la más adecuada.
        /// </summary>
        public const string WaitingReview = "Esperando revisión";

        /// <summary>
        /// Se ha seleccionado una oferta y se ha generado la Orden de Compra (OC), pero esta 
        /// aún requiere una firma o aprobación jerárquica para ser válida.
        /// </summary>
        public const string OrderAuthorized = "OC esperando aprobación";

        /// <summary>
        /// Estado final del flujo de adquisición. La Orden de Compra ha sido aprobada y enviada formalmente al proveedor.
        /// </summary>
        public const string OrderSent = "OC enviada";

        /// <summary>
        /// Indica que la solicitud de compra ha sido descartada o denegada en alguna de las etapas del proceso.
        /// </summary>
        public const string Rejected = "Rechazada";
    }
}