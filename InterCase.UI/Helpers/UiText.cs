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
        public static string DevTools => Pick("Developer tools", "Geliştirici araçları");
        public static string OpeningRabbitMq => Pick("Opening the RabbitMQ management panel…", "RabbitMQ yönetim paneli açılıyor…");
        public static string OpenRabbitMq => Pick("Open RabbitMQ panel", "RabbitMQ panelini aç");
        public static string MainNavigation => Pick("Main navigation", "Ana gezinme");
        public static string ActivityTitle => Pick("Notifications & logs", "Bildirimler ve loglar");

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
        public static string OrderInProgress => Pick(
            "The worker is processing your order.",
            "Siparişiniz worker tarafından işleniyor.");
        public static string OrderCompleted => Pick(
            "Your order has been processed.",
            "Siparişiniz işlendi.");
        public static string OrderFailed => Pick(
            "Your order could not be processed.",
            "Siparişiniz işlenemedi.");
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
        public static string NotificationColumn => Pick("Notification", "Bildirim");
        public static string OrdersPagination => Pick("Order pages", "Sipariş sayfaları");

        public static string NotificationsTitle => Pick("Notifications", "Bildirimler");
        public static string NotificationsHint => Pick(
            "Sent to you when the worker finishes processing an order.",
            "Worker bir siparişi işlemeyi bitirdiğinde size gönderilir.");
        public static string NoNotifications => Pick("No notifications yet.", "Henüz bildirim yok.");
        public static string NotificationsPagination => Pick("Notification pages", "Bildirim sayfaları");
        public static string SentColumn => Pick("Sent", "Gönderildi");
        public static string ChannelColumn => Pick("Channel", "Kanal");
        public static string MessageColumn => Pick("Message", "Mesaj");

        public static string LogsTitle => Pick("Redis logs", "Redis logları");
        public static string LogsHint => Pick(
            "The record the worker writes to Redis for each processed order.",
            "Worker'ın işlenen her sipariş için Redis'e yazdığı kayıt.");
        public static string NoLogs => Pick("No processed orders yet.", "Henüz işlenmiş sipariş yok.");
        public static string LogsPagination => Pick("Log pages", "Log sayfaları");
        public static string LogColumn => Pick("Redis record", "Redis kaydı");
        public static string LogMissing => Pick("No record in Redis", "Redis'te kayıt yok");

        public static string PreviousPage => Pick("Previous", "Önceki");
        public static string NextPage => Pick("Next", "Sonraki");
        public static string PageLabel(int page) => Pick($"Page {page}", $"Sayfa {page}");
        public static string PageOf(int page, int totalPages) => $"{page} / {totalPages}";
        public static string TimeZoneNote => Pick("Times are in Turkey time (UTC+3).", "Saatler Türkiye saatiyle (UTC+3).");

        public static string WorkerDownTitle => Pick("The worker is not running", "Worker çalışmıyor");
        public static string WorkerDownText => Pick(
            "New orders wait in the queue as Pending. Once the worker starts, it processes them on its own.",
            "Yeni siparişler kuyrukta Beklemede kalır. Worker açılınca bekleyen siparişleri kendisi işler.");
        public static string WorkerStartTitle => Pick("To start it:", "Çalıştırmak için:");
        public static string WorkerStartInfrastructure => Pick(
            "PostgreSQL, Redis and RabbitMQ must be up (in the repository root):",
            "PostgreSQL, Redis ve RabbitMQ açık olmalı (repo kökünde):");
        public static string WorkerStartVisualStudio => Pick(
            "Visual Studio: right-click InterCase.Worker → Debug → Start New Instance.",
            "Visual Studio: InterCase.Worker projesine sağ tıklayın → Debug → Start New Instance.");
        public static string WorkerStartTerminal => Pick(
            "Or in a terminal (in the repository root):",
            "Ya da terminalde (repo kökünde):");
        public static string WorkerLastSeen => Pick("Last heartbeat", "Son sinyal");

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

        public static string NotificationCount(int count) => Pick(
            count == 1 ? "1 notification" : $"{count} notifications",
            $"{count} bildirim");

        public static string LogCount(int count) => Pick(
            count == 1 ? "1 record" : $"{count} records",
            $"{count} kayıt");

        public static string Channel(string channel) => channel switch
        {
            NotificationChannels.Console => Pick("Console", "Konsol"),
            _ => channel
        };

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
