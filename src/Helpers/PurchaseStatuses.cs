namespace ByG_Backend.src.Helpers
{
    public static class PurchaseStatuses
    {
        public const string Received = "Esperando proveedores";
        public const string QuoteSent = "Solicitud de cotización enviada";
        public const string WaitingReview = "Esperando revisión";
        

        public const string OrderAuthorized = "OC esperando aprobación"; // Cuando se crea la OC (pero no se ha enviado)
        public const string OrderSent = "OC enviada";          // Cuando se aprueba la OC
        public const string Rejected = "Rechazada";
    }
}