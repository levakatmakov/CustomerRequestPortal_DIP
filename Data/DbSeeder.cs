using Microsoft.EntityFrameworkCore;
using CustomerRequestPortal.Models;

namespace CustomerRequestPortal.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext db)
        {
            if (db.Products.Any())
            {
                return; // Товары уже есть
            }

            var products = new List<Product>
            {
                // Цемент и сухие смеси
                new Product { Name = "Цемент М500", Description = "Портландцемент для бетона", Price = 450, Unit = "мешок 50кг", Category = "Сухие смеси", StockQuantity = 500, IsActive = true },
                new Product { Name = "Цемент М400", Description = "Портландцемент универсальный", Price = 380, Unit = "мешок 50кг", Category = "Сухие смеси", StockQuantity = 600, IsActive = true },
                new Product { Name = "Штукатурка гипсовая", Description = "Для внутренних работ", Price = 350, Unit = "мешок 30кг", Category = "Сухие смеси", StockQuantity = 400, IsActive = true },
                new Product { Name = "Шпаклёвка финишная", Description = "Для окончательной отделки", Price = 420, Unit = "мешок 25кг", Category = "Сухие смеси", StockQuantity = 350, IsActive = true },
                new Product { Name = "Клей для плитки", Description = "Усиленный, для керамики", Price = 280, Unit = "мешок 25кг", Category = "Сухие смеси", StockQuantity = 500, IsActive = true },
                new Product { Name = "Затирка для швов", Description = "Влагостойкая", Price = 150, Unit = "кг", Category = "Сухие смеси", StockQuantity = 200, IsActive = true },
                
                // Кирпич и блоки
                new Product { Name = "Кирпич красный полнотелый", Description = "Облицовочный М150", Price = 25, Unit = "шт", Category = "Кирпич", StockQuantity = 10000, IsActive = true },
                new Product { Name = "Кирпич силикатный", Description = "Строительный белый", Price = 18, Unit = "шт", Category = "Кирпич", StockQuantity = 8000, IsActive = true },
                new Product { Name = "Блок газобетонный", Description = "D500, 600x300x200", Price = 180, Unit = "шт", Category = "Блоки", StockQuantity = 2000, IsActive = true },
                new Product { Name = "Блок пенобетонный", Description = "D600, 600x300x200", Price = 160, Unit = "шт", Category = "Блоки", StockQuantity = 1800, IsActive = true },
                new Product { Name = "Блок керамзитобетонный", Description = "Полнотелый 400x200x200", Price = 95, Unit = "шт", Category = "Блоки", StockQuantity = 3000, IsActive = true },
                
                // Пиломатериалы
                new Product { Name = "Доска обрезная 50х150", Description = "Сосна, 1 сорт", Price = 18500, Unit = "м³", Category = "Пиломатериалы", StockQuantity = 100, IsActive = true },
                new Product { Name = "Брус 100х100", Description = "Сосна, строганый", Price = 19000, Unit = "м³", Category = "Пиломатериалы", StockQuantity = 80, IsActive = true },
                new Product { Name = "Брус 150х150", Description = "Сосна, для строительства", Price = 19500, Unit = "м³", Category = "Пиломатериалы", StockQuantity = 60, IsActive = true },
                new Product { Name = "Вагонка деревянная", Description = "Сосна, класс А", Price = 450, Unit = "м²", Category = "Пиломатериалы", StockQuantity = 500, IsActive = true },
                new Product { Name = "Фанера ФК 15мм", Description = "Влагостойкая", Price = 850, Unit = "лист 1.5м²", Category = "Пиломатериалы", StockQuantity = 300, IsActive = true },
                new Product { Name = "ОСБ плита 9мм", Description = "Для обшивки", Price = 550, Unit = "лист 2.8м²", Category = "Пиломатериалы", StockQuantity = 400, IsActive = true },
                
                // Кровельные материалы
                new Product { Name = "Профнастил С8", Description = "Полиэстер, 0.5мм", Price = 450, Unit = "м²", Category = "Кровля", StockQuantity = 1000, IsActive = true },
                new Product { Name = "Металлочерепица", Description = "Монтеррей, 0.5мм", Price = 580, Unit = "м²", Category = "Кровля", StockQuantity = 800, IsActive = true },
                new Product { Name = "Шифер 8-волновой", Description = "Асбестоцементный", Price = 650, Unit = "лист", Category = "Кровля", StockQuantity = 500, IsActive = true },
                new Product { Name = "Рубероид", Description = "Гидроизоляционный", Price = 450, Unit = "рулон 15м", Category = "Кровля", StockQuantity = 300, IsActive = true },
                new Product { Name = "Ондулин", Description = "Битумный лист", Price = 520, Unit = "лист", Category = "Кровля", StockQuantity = 400, IsActive = true },
                
                // Утеплители
                new Product { Name = "Минеральная вата", Description = "Базальтовая, 50мм", Price = 850, Unit = "упаковка 6м²", Category = "Утеплители", StockQuantity = 500, IsActive = true },
                new Product { Name = "Пенопласт ПСБ-С-25", Description = "1000х1000х50", Price = 120, Unit = "лист", Category = "Утеплители", StockQuantity = 1000, IsActive = true },
                new Product { Name = "Экструдированный пенополистирол", Description = "Пеноплэкс 50мм", Price = 180, Unit = "лист", Category = "Утеплители", StockQuantity = 800, IsActive = true },
                new Product { Name = "Керамзит", Description = "Фракция 10-20", Price = 2500, Unit = "м³", Category = "Утеплители", StockQuantity = 200, IsActive = true },
                
                // Бетон и сыпучие
                new Product { Name = "Бетон М200", Description = "Готовый раствор", Price = 4200, Unit = "м³", Category = "Бетон", StockQuantity = 100, IsActive = true },
                new Product { Name = "Бетон М300", Description = "Для фундамента", Price = 4500, Unit = "м³", Category = "Бетон", StockQuantity = 80, IsActive = true },
                new Product { Name = "Песок строительный", Description = "Мытый, сеяный", Price = 850, Unit = "м³", Category = "Сыпучие", StockQuantity = 500, IsActive = true },
                new Product { Name = "Щебень 20-40", Description = "Гранитный", Price = 2200, Unit = "м³", Category = "Сыпучие", StockQuantity = 300, IsActive = true }
            };

            db.Products.AddRange(products);
            await db.SaveChangesAsync();
        }
    }
}