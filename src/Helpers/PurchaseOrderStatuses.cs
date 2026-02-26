namespace ByG_Backend.src.Helpers
{
    public static class PurchaseOrderStatuses
    {
        public const string WaitingApproval = "Esperando Aprobación"; // Estado Inicial (Editable)
        public const string Sent = "Enviada";                         // Aprobada (No Editable, PDF generado)
        public const string Cancelled = "Cancelada";                  // Rechazada
    }
}