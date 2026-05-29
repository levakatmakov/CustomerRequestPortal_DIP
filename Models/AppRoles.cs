namespace CustomerRequestPortal.Models
{
    public static class AppRoles
    {
        public const string Customer = "Customer";
        public const string Dispatcher = "Dispatcher";
        public const string Executor = "Executor";
        public const string Administrator = "Administrator";

        public static string GetDisplayName(string role)
        {
            return role switch
            {
                Customer => "Клиент",
                Dispatcher => "Диспетчер",
                Executor => "Исполнитель",
                Administrator => "Администратор",
                _ => role
            };
        }
    }
}
