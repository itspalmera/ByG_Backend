using System;
using System.Collections.Generic;
using System.Linq;

namespace ByG_Backend.src.Helpers
{
    /// <summary>
    /// Provee constantes de solo lectura para los estados de las Órdenes de Compra (OC).
    /// Centralizar estos valores previene el uso de "Magic Strings" y asegura la consistencia 
    /// en las validaciones de flujo de trabajo en controladores y servicios.
    /// </summary>
    public static class PurchaseOrderStatuses
    {
        /// <summary>
        /// Estado inicial de la orden de compra. 
        /// Indica que la OC ha sido generada pero aún permite modificaciones en sus ítems o datos generales.
        /// </summary>
        public const string WaitingApproval = "Esperando Aprobación";

        /// <summary>
        /// Indica que la OC ha sido aprobada y enviada al proveedor.
        /// En este estado, el documento se considera bloqueado (no editable) y suele disparar la generación del PDF legal.
        /// </summary>
        public const string Sent = "Enviada";

        /// <summary>
        /// Indica que el proceso de compra ha sido desistido o rechazado.
        /// Este estado cierra el ciclo de vida de la OC sin concretar la transacción.
        /// </summary>
        public const string Cancelled = "Cancelada";
    }
}