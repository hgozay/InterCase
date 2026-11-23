using System.Globalization;

namespace InterCase.Common.Helpers
{
    public static class Localization
    {
        public const string English = "en";
        public const string Turkish = "tr";
        public const string DefaultLanguage = English;

        public static readonly string[] SupportedLanguages = [English, Turkish];

        public static class Keys
        {
            public const string UserIdRequired = "UserIdRequired";
            public const string UserIdMaxLength = "UserIdMaxLength";
            public const string ProductIdRequired = "ProductIdRequired";
            public const string ProductIdMaxLength = "ProductIdMaxLength";
            public const string QuantityRequired = "QuantityRequired";
            public const string QuantityRange = "QuantityRange";
            public const string PaymentMethodRequired = "PaymentMethodRequired";
            public const string PaymentMethodInvalid = "PaymentMethodInvalid";
            public const string UsernameRequired = "UsernameRequired";
            public const string PasswordRequired = "PasswordRequired";
            public const string LanguageRequired = "LanguageRequired";
            public const string LanguageNotSupported = "LanguageNotSupported";
            public const string InvalidValue = "InvalidValue";
            public const string InvalidRequestBody = "InvalidRequestBody";
        }

        private static readonly Dictionary<string, Dictionary<string, string>> Messages = new()
        {
            [Keys.UserIdRequired] = new() { [English] = "userId is required.", [Turkish] = "userId zorunludur." },
            [Keys.UserIdMaxLength] = new() { [English] = "userId must be at most 64 characters.", [Turkish] = "userId en fazla 64 karakter olabilir." },
            [Keys.ProductIdRequired] = new() { [English] = "productId is required.", [Turkish] = "productId zorunludur." },
            [Keys.ProductIdMaxLength] = new() { [English] = "productId must be at most 64 characters.", [Turkish] = "productId en fazla 64 karakter olabilir." },
            [Keys.QuantityRequired] = new() { [English] = "quantity is required.", [Turkish] = "quantity zorunludur." },
            [Keys.QuantityRange] = new() { [English] = "quantity must be between 1 and 1000.", [Turkish] = "quantity 1 ile 1000 arasında olmalıdır." },
            [Keys.PaymentMethodRequired] = new() { [English] = "paymentMethod is required.", [Turkish] = "paymentMethod zorunludur." },
            [Keys.PaymentMethodInvalid] = new() { [English] = "paymentMethod is not a valid payment method.", [Turkish] = "paymentMethod geçerli bir ödeme yöntemi değil." },
            [Keys.UsernameRequired] = new() { [English] = "username is required.", [Turkish] = "username zorunludur." },
            [Keys.PasswordRequired] = new() { [English] = "password is required.", [Turkish] = "password zorunludur." },
            [Keys.LanguageRequired] = new() { [English] = "language is required.", [Turkish] = "language zorunludur." },
            [Keys.LanguageNotSupported] = new() { [English] = "language must be one of: en, tr.", [Turkish] = "language şu değerlerden biri olmalıdır: en, tr." },
            [Keys.InvalidValue] = new() { [English] = "{0} has an invalid value.", [Turkish] = "{0} için geçersiz bir değer gönderildi." },
            [Keys.InvalidRequestBody] = new() { [English] = "The request body is missing or is not valid JSON.", [Turkish] = "İstek gövdesi eksik ya da geçerli bir JSON değil." }
        };

        public static string CurrentLanguage => Normalize(CultureInfo.CurrentUICulture.TwoLetterISOLanguageName);

        public static bool IsSupported(string? language) =>
            language is not null && SupportedLanguages.Contains(language, StringComparer.OrdinalIgnoreCase);

        public static string Normalize(string? language) =>
            IsSupported(language) ? language!.ToLowerInvariant() : DefaultLanguage;

        public static bool HasKey(string key) => Messages.ContainsKey(key);

        public static string Get(string key) => Get(key, CurrentLanguage);

        public static string Format(string key, params object[] arguments) =>
            string.Format(CultureInfo.CurrentCulture, Get(key), arguments);

        public static string Get(string key, string language)
        {
            if (!Messages.TryGetValue(key, out var translations))
                return key;

            return translations.TryGetValue(Normalize(language), out var message)
                ? message
                : translations[DefaultLanguage];
        }

        public static IEnumerable<KeyValuePair<string, string>> GetAll(string language) =>
            Messages.Select(m => new KeyValuePair<string, string>(m.Key, Get(m.Key, language)));
    }
}
