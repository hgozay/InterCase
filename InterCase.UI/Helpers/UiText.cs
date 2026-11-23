using InterCase.Common;
using InterCase.Common.Helpers;

namespace InterCase.UI.Helpers
{
    public static class UiText
    {
        public static bool IsTurkish => Localization.CurrentLanguage == Localization.Turkish;

        private static string Pick(string english, string turkish) => IsTurkish ? turkish : english;

        public static string AppTagline => Pick("Order console", "Sipariş konsolu");
        public static string Language => Pick("Language", "Dil");
        public static string SignOut => Pick("Sign out", "Çıkış");
        public static string ApiLinks => Pick("API tools", "API araçları");

        public static string LoginTitle => Pick("Sign in", "Giriş yap");
        public static string LoginSubtitle => Pick(
            "Sign in to place orders and follow them as the worker processes them.",
            "Sipariş vermek ve worker'ın işleyişini izlemek için giriş yapın.");
        public static string Username => Pick("Username", "Kullanıcı adı");
        public static string Password => Pick("Password", "Şifre");
        public static string SignIn => Pick("Sign in", "Giriş yap");
        public static string SigningIn => Pick("Signing in…", "Giriş yapılıyor…");
        public static string DemoAccounts => Pick("Demo accounts", "Demo hesapları");
        public static string DemoAccountsHint => Pick(
            "Pick an account to fill in its username and password.",
            "Kullanıcı adı ve şifresini doldurmak için bir hesap seçin.");
        public static string FillDemo => Pick("Fill in", "Doldur");
        public static string SessionExpired => Pick(
            "Your session has expired. Please sign in again.",
            "Oturumunuzun süresi doldu. Lütfen yeniden giriş yapın.");

        public static string NewOrder => Pick("New order", "Yeni sipariş");
        public static string NewOrderHint => Pick(
            "The order is queued right away; the worker processes it in the background.",
            "Sipariş hemen kuyruğa alınır; worker arka planda işler.");
        public static string UserId => Pick("User ID", "Kullanıcı ID");
        public static string SignedInUserHint => Pick(
            "Orders are placed for the signed-in user.",
            "Siparişler giriş yapan kullanıcı adına verilir.");
        public static string ProductId => Pick("Product ID", "Ürün ID");
        public static string Quantity => Pick("Quantity", "Adet");
        public static string QuantityHint => Pick("Between 1 and 1000", "1 ile 1000 arasında");
        public static string PaymentMethodLabel => Pick("Payment method", "Ödeme yöntemi");
        public static string GenerateId => Pick("New", "Yeni");
        public static string GenerateIdLabel => Pick("Generate a new ID", "Yeni ID üret");
        public static string PlaceOrder => Pick("Place order", "Sipariş ver");
        public static string Sending => Pick("Sending…", "Gönderiliyor…");
        public static string OrderAccepted => Pick(
            "Order received. Its status will change once the worker processes it.",
            "Sipariş alındı. Worker işleyince durumu değişecek.");
        public static string CorrelationId => "Correlation ID";

        public static string OrdersTitle => Pick("Orders", "Siparişler");
        public static string Refresh => Pick("Refresh", "Yenile");
        public static string LiveUpdating => Pick("Updating automatically", "Otomatik güncelleniyor");
        public static string EnterUserId => Pick(
            "Enter a user ID to see that user's orders.",
            "Siparişleri görmek için bir kullanıcı ID girin.");
        public static string NoOrders => Pick("This user has no orders yet.", "Bu kullanıcının henüz siparişi yok.");
        public static string OrderColumn => Pick("Order", "Sipariş");
        public static string ProductColumn => Pick("Product", "Ürün");
        public static string PaymentColumn => Pick("Payment", "Ödeme");
        public static string StatusColumn => Pick("Status", "Durum");
        public static string CreatedColumn => Pick("Created", "Oluşturuldu");
        public static string ProcessedColumn => Pick("Processed", "İşlendi");
        public static string NewBadge => Pick("New", "Yeni");
        public static string TimeZoneNote => Pick("Times are in Turkey time (UTC+3).", "Saatler Türkiye saatiyle (UTC+3).");

        public static string ApiUnavailable => Pick(
            "Could not reach the API. Make sure InterCase.API is running.",
            "API'ye ulaşılamadı. InterCase.API'nin çalıştığından emin olun.");
        public static string UnexpectedApiResponse => Pick(
            "The API returned an unexpected response.",
            "API beklenmeyen bir yanıt döndü.");

        public static string ErrorTitle => Pick("Something went wrong", "Bir şeyler ters gitti");
        public static string ErrorText => Pick(
            "The page could not be displayed. Please try again.",
            "Sayfa görüntülenemedi. Lütfen yeniden deneyin.");
        public static string NotFoundTitle => Pick("Page not found", "Sayfa bulunamadı");
        public static string NotFoundText => Pick(
            "The page you are looking for does not exist.",
            "Aradığınız sayfa mevcut değil.");
        public static string BackToOrders => Pick("Back to orders", "Siparişlere dön");

        public static string OrderCount(int count) => Pick(
            count == 1 ? "1 order" : $"{count} orders",
            $"{count} sipariş");

        public static string Status(OrderStatus status) => status switch
        {
            OrderStatus.Pending => Pick("Pending", "Beklemede"),
            OrderStatus.Processing => Pick("Processing", "İşleniyor"),
            OrderStatus.Processed => Pick("Processed", "İşlendi"),
            OrderStatus.Failed => Pick("Failed", "Başarısız"),
            _ => status.ToString()
        };

        public static string Payment(PaymentMethod method) => method switch
        {
            PaymentMethod.CreditCard => Pick("Credit card", "Kredi kartı"),
            PaymentMethod.BankTransfer => Pick("Bank transfer", "Havale / EFT"),
            PaymentMethod.Cash => Pick("Cash", "Nakit"),
            _ => method.ToString()
        };
    }
}
