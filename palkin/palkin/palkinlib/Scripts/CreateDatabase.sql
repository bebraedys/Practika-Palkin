-- Скрипт создания базы данных Palkin
-- СУБД: PostgreSQL
-- Студент: Palkin

-- Создание роли app с паролем
DO $$
BEGIN
    IF NOT EXISTS (SELECT FROM pg_catalog.pg_roles WHERE rolname = 'app') THEN
        CREATE ROLE app WITH LOGIN PASSWORD '123456789';
    END IF;
END
$$;

-- Создание табличного пространства (если требуется)
-- CREATE TABLESPACE palkin_ts LOCATION '/path/to/data';

-- Создание базы данных (выполняется отдельно от скрипта)
-- CREATE DATABASE "Palkin" TABLESPACE palkin_ts;

-- Подключение к базе данных Palkin
-- \c "Palkin"

-- Создание схемы app
CREATE SCHEMA IF NOT EXISTS app AUTHORIZATION app;

-- Таблица типов партнеров
CREATE TABLE app.partner_types (
    id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL UNIQUE
);

-- Таблица партнеров
CREATE TABLE app.partners (
    id SERIAL PRIMARY KEY,
    partner_type_id INTEGER NOT NULL REFERENCES app.partner_types(id) ON DELETE RESTRICT,
    name VARCHAR(255) NOT NULL,
    legal_address VARCHAR(500),
    inn VARCHAR(20),
    director_name VARCHAR(255),
    phone VARCHAR(50),
    email VARCHAR(100),
    rating INTEGER NOT NULL DEFAULT 0 CHECK (rating >= 0),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Таблица продаж (история реализации продукции)
CREATE TABLE app.sales (
    id SERIAL PRIMARY KEY,
    partner_id INTEGER NOT NULL REFERENCES app.partners(id) ON DELETE CASCADE,
    product_name VARCHAR(255) NOT NULL,
    quantity INTEGER NOT NULL CHECK (quantity > 0),
    sale_date DATE NOT NULL DEFAULT CURRENT_DATE,
    amount NUMERIC(12, 2) NOT NULL CHECK (amount >= 0)
);

-- Индексы для улучшения производительности
CREATE INDEX idx_partners_partner_type ON app.partners(partner_type_id);
CREATE INDEX idx_sales_partner ON app.sales(partner_id);
CREATE INDEX idx_sales_date ON app.sales(sale_date);

-- Представление для расчета общей суммы продаж партнера
CREATE OR REPLACE VIEW app.partner_sales_summary AS
SELECT 
    p.id AS partner_id,
    COALESCE(SUM(s.amount), 0) AS total_sales
FROM app.partners p
LEFT JOIN app.sales s ON p.id = s.partner_id
GROUP BY p.id;

-- Функция для обновления updated_at
CREATE OR REPLACE FUNCTION app.update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Триггер для автоматического обновления updated_at
CREATE TRIGGER update_partners_updated_at
    BEFORE UPDATE ON app.partners
    FOR EACH ROW
    EXECUTE FUNCTION app.update_updated_at_column();

-- Начальные данные для типов партнеров
INSERT INTO app.partner_types (name) VALUES 
    ('Розничный магазин'),
    ('Оптовый магазин'),
    ('Интернет-магазин'),
    ('Торговая компания'),
    ('Сервисная компания');

-- Предоставление прав роли app
GRANT ALL PRIVILEGES ON SCHEMA app TO app;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA app TO app;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA app TO app;
GRANT EXECUTE ON ALL FUNCTIONS IN SCHEMA app TO app;

-- Комментарии к таблицам
COMMENT ON TABLE app.partners IS 'Таблица партнеров компании Мастер пол';
COMMENT ON TABLE app.partner_types IS 'Справочник типов партнеров';
COMMENT ON TABLE app.sales IS 'История продаж продукции партнерам';
