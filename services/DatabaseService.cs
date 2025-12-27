using Npgsql;
using QualityAppWPF.Models;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using QualityAppWPF.Models;

namespace QualityAppWPF.Services
{
    public class DatabaseService
    {
        private readonly string _connectionString;

        public DatabaseService()
        {
            _connectionString = "Host=localhost;Port=5432;Database=Check;Username=postgres;Password=1";
        }

        // Инициализация БД с таблицей пользователей
        public async Task<bool> InitializeDatabaseAsync()
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    var createTablesSql = @"
               -- Создаем основные таблицы
                CREATE TABLE IF NOT EXISTS products (
                    id SERIAL PRIMARY KEY,
                    barcode VARCHAR(50) UNIQUE NOT NULL,
                    name VARCHAR(255) NOT NULL,
                    category VARCHAR(100),
                    manufacturer VARCHAR(255),
                    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                );

                -- Добавляем колонки для БЖУ если их нет
                DO $$ 
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns 
                                   WHERE table_name = 'products' AND column_name = 'protein') THEN
                        ALTER TABLE products 
                        ADD COLUMN protein DECIMAL(5,2);
                    END IF;
                    
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns 
                                   WHERE table_name = 'products' AND column_name = 'fat') THEN
                        ALTER TABLE products 
                        ADD COLUMN fat DECIMAL(5,2);
                    END IF;
                    
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns 
                                   WHERE table_name = 'products' AND column_name = 'carbs') THEN
                        ALTER TABLE products 
                        ADD COLUMN carbs DECIMAL(5,2);
                    END IF;
                    
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns 
                                   WHERE table_name = 'products' AND column_name = 'calories') THEN
                        ALTER TABLE products 
                        ADD COLUMN calories DECIMAL(6,2);
                    END IF;
                END $$;

                CREATE TABLE IF NOT EXISTS quality_checks (
                    id SERIAL PRIMARY KEY,
                    product_id INTEGER REFERENCES products(id),
                    product_barcode VARCHAR(50),
                    product_name VARCHAR(255),
                    check_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    rating INTEGER CHECK (rating >= 1 AND rating <= 5),
                    comment TEXT,
                    inspector VARCHAR(255),
                    location VARCHAR(255)
                );

                CREATE TABLE IF NOT EXISTS users (
                    id SERIAL PRIMARY KEY,
                    username VARCHAR(100) UNIQUE NOT NULL,
                    password_hash VARCHAR(255) NOT NULL,
                    full_name VARCHAR(255),
                    role VARCHAR(50) DEFAULT 'User',
                    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    is_active BOOLEAN DEFAULT true
                );

                -- Создаем новые таблицы для расширенной функциональности
                CREATE TABLE IF NOT EXISTS manufacturers (
                    manufacturing_id SERIAL PRIMARY KEY,
                    manufacturing_name VARCHAR(255) NOT NULL,
                    contact_person VARCHAR(255)
                );

                CREATE TABLE IF NOT EXISTS categories (
                    category_id SERIAL PRIMARY KEY,
                    category_name VARCHAR(255) NOT NULL
                );

                CREATE TABLE IF NOT EXISTS quality_parameters (
                    parameter_id SERIAL PRIMARY KEY,
                    parameter_name VARCHAR(255) NOT NULL,
                    parameter_description TEXT,
                    unit VARCHAR(50)
                );

                CREATE TABLE IF NOT EXISTS detailed_checks (
                    check_id SERIAL PRIMARY KEY,
                    quality_check_id INTEGER REFERENCES quality_checks(id) ON DELETE CASCADE,
                    parameter_id INTEGER REFERENCES quality_parameters(parameter_id),
                    parameter_value DECIMAL(10,2),
                    is_passed BOOLEAN DEFAULT true,
                    notes TEXT,
                    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                );

                -- Добавляем новые столбцы в существующие таблицы
                DO $$ 
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns 
                                   WHERE table_name = 'products' AND column_name = 'manufacturing_id') THEN
                        ALTER TABLE products 
                        ADD COLUMN manufacturing_id INTEGER REFERENCES manufacturers(manufacturing_id);
                    END IF;
                    
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns 
                                   WHERE table_name = 'products' AND column_name = 'category_id') THEN
                        ALTER TABLE products 
                        ADD COLUMN category_id INTEGER REFERENCES categories(category_id);
                    END IF;
                    
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns 
                                   WHERE table_name = 'products' AND column_name = 'product_description') THEN
                        ALTER TABLE products 
                        ADD COLUMN product_description TEXT;
                    END IF;
                END $$;

                -- Добавляем тестовые данные
                INSERT INTO products (barcode, name, category, manufacturer)
                SELECT '4601234567890', 'Молоко 3.2%', 'Молочные', 'Простоквашино'
                WHERE NOT EXISTS (SELECT 1 FROM products WHERE barcode = '4601234567890');

                INSERT INTO products (barcode, name, category, manufacturer)
                SELECT '4601234567891', 'Хлеб Бородинский', 'Хлебобулочные', 'Хлебзавод №1'
                WHERE NOT EXISTS (SELECT 1 FROM products WHERE barcode = '4601234567891');

                -- Добавляем тестовых пользователей
                INSERT INTO users (username, password_hash, full_name, role) 
                SELECT 'user', 'n4bQgYhMfWWaL+qgxVrQFaO/TxsrC4Is0V1sFbDwCgg=', 'Оператор проверки', 'User'
                WHERE NOT EXISTS (SELECT 1 FROM users WHERE username = 'user');

                INSERT INTO users (username, password_hash, full_name, role) 
                SELECT 'admin', 'jGl25bVBBBW96Qi9Te4V37Fnqchz/Eu4qB9vKrRIqRg=', 'Администратор системы', 'Admin'
                WHERE NOT EXISTS (SELECT 1 FROM users WHERE username = 'admin');

                -- Добавляем тестовых производителей
                INSERT INTO manufacturers (manufacturing_name, contact_person) 
                SELECT 'Простоквашино', 'Иванов И.И.'
                WHERE NOT EXISTS (SELECT 1 FROM manufacturers WHERE manufacturing_name = 'Простоквашино');

                INSERT INTO manufacturers (manufacturing_name, contact_person) 
                SELECT 'Хлебзавод №1', 'Петров П.П.'
                WHERE NOT EXISTS (SELECT 1 FROM manufacturers WHERE manufacturing_name = 'Хлебзавод №1');

                -- Добавляем тестовые категории
                INSERT INTO categories (category_name) 
                SELECT 'Молочные продукты'
                WHERE NOT EXISTS (SELECT 1 FROM categories WHERE category_name = 'Молочные продукты');

                INSERT INTO categories (category_name) 
                SELECT 'Хлебобулочные изделия'
                WHERE NOT EXISTS (SELECT 1 FROM categories WHERE category_name = 'Хлебобулочные изделия');

                -- Добавляем тестовые параметры контроля
                INSERT INTO quality_parameters (parameter_name, parameter_description, unit) 
                SELECT 'Вес', 'Масса продукта', 'г'
                WHERE NOT EXISTS (SELECT 1 FROM quality_parameters WHERE parameter_name = 'Вес');

                INSERT INTO quality_parameters (parameter_name, parameter_description, unit) 
                SELECT 'Температура', 'Температура хранения', '°C'
                WHERE NOT EXISTS (SELECT 1 FROM quality_parameters WHERE parameter_name = 'Температура');

                INSERT INTO quality_parameters (parameter_name, parameter_description, unit) 
                SELECT 'Срок годности', 'Остаток срока годности', 'дней'
                WHERE NOT EXISTS (SELECT 1 FROM quality_parameters WHERE parameter_name = 'Срок годности');

                -- Обновляем продукты с новыми связями
                UPDATE products 
                SET manufacturing_id = (SELECT manufacturing_id FROM manufacturers WHERE manufacturing_name = 'Простоквашино' LIMIT 1),
                    category_id = (SELECT category_id FROM categories WHERE category_name = 'Молочные продукты' LIMIT 1)
                WHERE name LIKE '%Молоко%' AND manufacturing_id IS NULL;

                UPDATE products 
                SET manufacturing_id = (SELECT manufacturing_id FROM manufacturers WHERE manufacturing_name = 'Хлебзавод №1' LIMIT 1),
                    category_id = (SELECT category_id FROM categories WHERE category_name = 'Хлебобулочные изделия' LIMIT 1)
                WHERE name LIKE '%Хлеб%' AND manufacturing_id IS NULL;
DO $$ 
BEGIN
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns 
                   WHERE table_name = 'products' AND column_name = 'protein') THEN
        ALTER TABLE products 
        ADD COLUMN protein DECIMAL(5,2);
    END IF;
    
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns 
                   WHERE table_name = 'products' AND column_name = 'fat') THEN
        ALTER TABLE products 
        ADD COLUMN fat DECIMAL(5,2);
    END IF;
    
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns 
                   WHERE table_name = 'products' AND column_name = 'carbs') THEN
        ALTER TABLE products 
        ADD COLUMN carbs DECIMAL(5,2);
    END IF;
    
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns 
                   WHERE table_name = 'products' AND column_name = 'calories') THEN
        ALTER TABLE products 
        ADD COLUMN calories DECIMAL(6,2);
    END IF;
END $$;
            ";

                    using (var command = new NpgsqlCommand(createTablesSql, connection))
                    {
                        await command.ExecuteNonQueryAsync();
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка инициализации БД: {ex.Message}");
            }
        }
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }

        public async Task<User> AuthenticateUserAsync(string username, string passwordHash)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var sql = "SELECT * FROM users WHERE username = @username AND " +
                    "password_hash = @passwordHash AND is_active = true";
                using (var command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("username", username);
                    command.Parameters.AddWithValue("passwordHash", passwordHash);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new User
                            {
                                Id = reader.GetInt32(0),
                                Username = reader.GetString(1),
                                PasswordHash = reader.GetString(2),
                                FullName = reader.IsDBNull(3) ? "" : reader.GetString(3),
                                Role = reader.GetString(4),
                                CreatedAt = reader.GetDateTime(5),
                                IsActive = reader.GetBoolean(6)
                            };
                        }
                    }
                }
            }
            
        }

        // Получение всех пользователей
        public async Task<List<User>> GetAllUsersAsync()
        {
            var users = new List<User>();

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var sql = "SELECT * FROM users ORDER BY created_at DESC";
                using (var command = new NpgsqlCommand(sql, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            users.Add(new User
                            {
                                Id = reader.GetInt32(0),
                                Username = reader.GetString(1),
                                PasswordHash = reader.GetString(2),
                                FullName = reader.IsDBNull(3) ? "" : reader.GetString(3),
                                Role = reader.GetString(4),
                                CreatedAt = reader.GetDateTime(5),
                                IsActive = reader.GetBoolean(6)
                            });
                        }
                    }
                }
            }

            return users;
        }

        // Добавление пользователя (С ИСПРАВЛЕННЫМ ХЭШИРОВАНИЕМ!)
        public async Task<bool> AddUserAsync(User user)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var sql = @"
                    INSERT INTO users (username, password_hash, full_name, role, is_active)
                    VALUES (@username, @passwordHash, @fullName, @role, @isActive)
                    RETURNING id";

                using (var command = new NpgsqlCommand(sql, connection))
                {
                    // ВАЖНО: Если пароль уже хэширован, используем его как есть
                    // Если нет - хэшируем
                    string passwordHash = user.PasswordHash;
                    if (!passwordHash.Contains("==")) // Простая проверка на base64
                    {
                        passwordHash = HashPassword(user.PasswordHash);
                    }

                    command.Parameters.AddWithValue("username", user.Username);
                    command.Parameters.AddWithValue("passwordHash", passwordHash);
                    command.Parameters.AddWithValue("fullName", user.FullName);
                    command.Parameters.AddWithValue("role", user.Role);
                    command.Parameters.AddWithValue("isActive", user.IsActive);

                    var result = await command.ExecuteScalarAsync();
                    return result != null;
                }
            }
        }

        // Добавление продукта (для админа)
        public async Task<bool> AddProductAsync(Product product)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var sql = @"
            INSERT INTO products 
            (barcode, name, category, manufacturer, protein, fat, carbs, calories)
            VALUES (@barcode, @name, @category, @manufacturer, @protein, @fat, @carbs, @calories)
            RETURNING id";

                using (var command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("barcode", product.Barcode);
                    command.Parameters.AddWithValue("name", product.Name);
                    command.Parameters.AddWithValue("category", product.Category ?? "");
                    command.Parameters.AddWithValue("manufacturer", product.Manufacturer ?? "");

                    // Добавляем параметры БЖУ (может быть NULL)
                    command.Parameters.AddWithValue("protein",
                        product.Protein.HasValue ? (object)product.Protein.Value : DBNull.Value);
                    command.Parameters.AddWithValue("fat",
                        product.Fat.HasValue ? (object)product.Fat.Value : DBNull.Value);
                    command.Parameters.AddWithValue("carbs",
                        product.Carbs.HasValue ? (object)product.Carbs.Value : DBNull.Value);
                    command.Parameters.AddWithValue("calories",
                        product.Calories.HasValue ? (object)product.Calories.Value : DBNull.Value);

                    var result = await command.ExecuteScalarAsync();
                    return result != null;
                }
            }
        }

        public async Task<Product> GetProductByBarcodeAsync(string barcode)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var sql = "SELECT * FROM products WHERE barcode = @barcode";
                using (var command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("barcode", barcode);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            var product = new Product
                            {
                                Id = reader.GetInt32(0),
                                Barcode = reader.GetString(1),
                                Name = reader.GetString(2),
                                Category = reader.IsDBNull(3) ? "" : reader.GetString(3),
                                Manufacturer = reader.IsDBNull(4) ? "" : reader.GetString(4),
                                CreatedAt = reader.GetDateTime(5)
                            };
                            if (!reader.IsDBNull(6)) product.Protein = reader.GetDouble(6);
                            if (!reader.IsDBNull(7)) product.Fat = reader.GetDouble(7);
                            if (!reader.IsDBNull(8)) product.Carbs = reader.GetDouble(8);
                            if (!reader.IsDBNull(9)) product.Calories = reader.GetDouble(9);

                            return product;
                        }
                    }
                }
            }
            return null;
        }

        // Удаление продукта (для админа)
        public async Task<bool> DeleteProductAsync(int productId)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var sql = "DELETE FROM products WHERE id = @id";
                using (var command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("id", productId);

                    var rowsAffected = await command.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
        }
        public async Task<List<Product>> GetAllProductsAsync()
        {
            var products = new List<Product>();

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var sql = "SELECT * FROM products ORDER BY created_at DESC";
                using (var command = new NpgsqlCommand(sql, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var product = new Product
                            {
                                Id = reader.GetInt32(0),
                                Barcode = reader.GetString(1),
                                Name = reader.GetString(2),
                                Category = reader.IsDBNull(3) ? "" : reader.GetString(3),
                                Manufacturer = reader.IsDBNull(4) ? "" : reader.GetString(4),
                                CreatedAt = reader.GetDateTime(5)
                            };
                            if (!reader.IsDBNull(6)) product.Protein = reader.GetDouble(6);
                            if (!reader.IsDBNull(7)) product.Fat = reader.GetDouble(7);
                            if (!reader.IsDBNull(8)) product.Carbs = reader.GetDouble(8);
                            if (!reader.IsDBNull(9)) product.Calories = reader.GetDouble(9);

                            products.Add(product);
                        }
                    }
                }
            }

            return products;
        }

        public async Task<Statistics> GetStatisticsAsync()
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var sql = @"
                    SELECT 
                        (SELECT COUNT(*) FROM products) as total_products,
                        (SELECT COUNT(*) FROM quality_checks WHERE DATE(check_date) = CURRENT_DATE) as checks_today,
                        (SELECT AVG(rating) FROM quality_checks) as avg_rating";

                using (var command = new NpgsqlCommand(sql, connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new Statistics
                        {
                            TotalProducts = reader.GetInt32(0),
                            ChecksToday = reader.GetInt32(1),
                            AverageRating = reader.IsDBNull(2) ? 0 : reader.GetDouble(2)
                        };
                    }
                }
            }

            return new Statistics();
        }

        public async Task<List<QualityCheck>> GetQualityChecksAsync(string barcode = null)
        {
            var checks = new List<QualityCheck>();

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string sql = "SELECT * FROM quality_checks ";
                if (!string.IsNullOrEmpty(barcode))
                {
                    sql += "WHERE product_barcode = @barcode ";
                }
                sql += "ORDER BY check_date DESC";

                using (var command = new NpgsqlCommand(sql, connection))
                {
                    if (!string.IsNullOrEmpty(barcode))
                    {
                        command.Parameters.AddWithValue("barcode", barcode);
                    }

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            checks.Add(new QualityCheck
                            {
                                Id = reader.GetInt32(0),
                                ProductId = reader.GetInt32(1),
                                ProductBarcode = reader.GetString(2),
                                ProductName = reader.IsDBNull(3) ? "" : reader.GetString(3),
                                CheckDate = reader.GetDateTime(4),
                                Rating = reader.GetInt32(5),
                                Comment = reader.IsDBNull(6) ? "" : reader.GetString(6),
                                Inspector = reader.IsDBNull(7) ? "" : reader.GetString(7),
                                Location = reader.IsDBNull(8) ? "" : reader.GetString(8)
                            });
                        }
                    }
                }
            }

            return checks;
        }

        public async Task<bool> AddQualityCheckAsync(QualityCheck check)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var sql = @"
                    INSERT INTO quality_checks 
                    (product_id, product_barcode, product_name, rating, comment, inspector, location)
                    VALUES (@productId, @barcode, @productName, @rating, @comment, @inspector, @location)";

                using (var command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("productId", check.ProductId);
                    command.Parameters.AddWithValue("barcode", check.ProductBarcode);
                    command.Parameters.AddWithValue("productName", check.ProductName ?? "");
                    command.Parameters.AddWithValue("rating", check.Rating);
                    command.Parameters.AddWithValue("comment", check.Comment ?? "");
                    command.Parameters.AddWithValue("inspector", check.Inspector);
                    command.Parameters.AddWithValue("location", check.Location ?? "");

                    var rowsAffected = await command.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
        }

        public async Task<List<Manufacturer>> GetAllManufacturersAsync()
        {
            var manufacturers = new List<Manufacturer>();

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var sql = "SELECT * FROM manufacturers ORDER BY manufacturing_name";
                using (var command = new NpgsqlCommand(sql, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            manufacturers.Add(new Manufacturer
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                ContactPerson = reader.IsDBNull(2) ? "" : reader.GetString(2)
                            });
                        }
                    }
                }
            }

            return manufacturers;
        }

        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            var categories = new List<Category>();

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var sql = "SELECT * FROM categories ORDER BY category_name";
                using (var command = new NpgsqlCommand(sql, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            categories.Add(new Category
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1)
                            });
                        }
                    }
                }
            }

            return categories;
        }

        public async Task<List<QualityParameter>> GetAllQualityParametersAsync()
        {
            var parameters = new List<QualityParameter>();

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var sql = "SELECT * FROM quality_parameters ORDER BY parameter_name";
                using (var command = new NpgsqlCommand(sql, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            parameters.Add(new QualityParameter
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Description = reader.IsDBNull(2) ? "" : reader.GetString(2),
                                Unit = reader.IsDBNull(3) ? "" : reader.GetString(3)
                            });
                        }
                    }
                }
            }

            return parameters;
        }

        public async Task<bool> AddDetailedCheckAsync(DetailedCheck detailedCheck)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var sql = @"
            INSERT INTO detailed_checks 
            (quality_check_id, parameter_id, parameter_value, is_passed, notes)
            VALUES (@qualityCheckId, @parameterId, @value, @isPassed, @notes)";

                using (var command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("qualityCheckId", detailedCheck.QualityCheckId);
                    command.Parameters.AddWithValue("parameterId", detailedCheck.ParameterId);
                    command.Parameters.AddWithValue("value", detailedCheck.Value);
                    command.Parameters.AddWithValue("isPassed", detailedCheck.IsPassed);
                    command.Parameters.AddWithValue("notes", detailedCheck.Notes ?? "");

                    var rowsAffected = await command.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
        }

        public async Task<List<DetailedCheck>> GetDetailedChecksAsync(int qualityCheckId)
        {
            var checks = new List<DetailedCheck>();

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var sql = @"
            SELECT dc.*, qp.parameter_name, qp.unit 
            FROM detailed_checks dc
            JOIN quality_parameters qp ON dc.parameter_id = qp.parameter_id
            WHERE dc.quality_check_id = @qualityCheckId
            ORDER BY dc.created_at";

                using (var command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("qualityCheckId", qualityCheckId);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            checks.Add(new DetailedCheck
                            {
                                Id = reader.GetInt32(0),
                                QualityCheckId = reader.GetInt32(1),
                                ParameterId = reader.GetInt32(2),
                                Value = reader.GetDecimal(3),
                                IsPassed = reader.GetBoolean(4),
                                Notes = reader.IsDBNull(5) ? "" : reader.GetString(5),
                                CreatedAt = reader.GetDateTime(6),
                                ParameterName = reader.GetString(7),
                                Unit = reader.IsDBNull(8) ? "" : reader.GetString(8)
                            });
                        }
                    }
                }
            }

            return checks;
        }
        // В класс DatabaseService добавляем:

        public async Task<bool> AddManufacturerAsync(Manufacturer manufacturer)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var sql = @"
            INSERT INTO manufacturers (manufacturing_name, contact_person)
            VALUES (@name, @contactPerson)
            RETURNING manufacturing_id";

                using (var command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("name", manufacturer.Name);
                    command.Parameters.AddWithValue("contactPerson", manufacturer.ContactPerson ?? "");

                    var result = await command.ExecuteScalarAsync();
                    return result != null;
                }
            }
        }

        public async Task<bool> AddCategoryAsync(Category category)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var sql = @"
            INSERT INTO categories (category_name)
            VALUES (@name)
            RETURNING category_id";

                using (var command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("name", category.Name);

                    var result = await command.ExecuteScalarAsync();
                    return result != null;
                }
            }
        }

        public async Task<bool> AddQualityParameterAsync(QualityParameter parameter)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var sql = @"
            INSERT INTO quality_parameters (parameter_name, parameter_description, unit)
            VALUES (@name, @description, @unit)
            RETURNING parameter_id";

                using (var command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("name", parameter.Name);
                    command.Parameters.AddWithValue("description", parameter.Description ?? "");
                    command.Parameters.AddWithValue("unit", parameter.Unit ?? "");

                    var result = await command.ExecuteScalarAsync();
                    return result != null;
                }
            }
        }

        public async Task<bool> DeleteManufacturerAsync(int id)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var sql = "DELETE FROM manufacturers WHERE manufacturing_id = @id";
                using (var command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("id", id);
                    var rowsAffected = await command.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var sql = "DELETE FROM categories WHERE category_id = @id";
                using (var command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("id", id);
                    var rowsAffected = await command.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
        }

        public async Task<bool> DeleteQualityParameterAsync(int id)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var sql = "DELETE FROM quality_parameters WHERE parameter_id = @id";
                using (var command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("id", id);
                    var rowsAffected = await command.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
        }
    }
}