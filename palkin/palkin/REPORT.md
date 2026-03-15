# Пояснительная записка
## Проект: Подсистема управления партнерами компании «Мастер пол»
### Студент: Palkin

---

## Содержание

1. [Введение](#введение)
2. [Проектирование базы данных](#проектирование-базы-данных)
3. [Архитектура приложения](#архитектура-приложения)
4. [Реализация функционала](#реализация-функционала)
5. [Тестирование](#тестирование)
6. [Заключение](#заключение)

---

## Введение

### Цель работы
Разработка подсистемы для работы с партнерами компании, обеспечивающей:
- Просмотр списка партнеров
- Добавление/редактирование данных о партнере
- Просмотр истории реализации продукции партнером
- Автоматический расчет скидки на основе объема продаж

### Используемые технологии
- **СУБД**: PostgreSQL 12+
- **Язык программирования**: C# (.NET 8.0)
- **Фреймворк**: WPF (Windows Presentation Foundation)
- **ORM**: Entity Framework Core 8.0
- **Тестирование**: xUnit, Moq

---

## Проектирование базы данных

### Схема базы данных

База данных `Palkin` содержит схему `app` со следующими таблицами:

#### 1. partner_types - Справочник типов партнеров

| Поле | Тип | Описание |
|------|-----|----------|
| id | SERIAL | Первичный ключ |
| name | VARCHAR(100) | Название типа (уникальное) |

#### 2. partners - Партнеры компании

| Поле | Тип | Описание |
|------|-----|----------|
| id | SERIAL | Первичный ключ |
| partner_type_id | INTEGER | Внешний ключ на partner_types |
| name | VARCHAR(255) | Наименование компании |
| legal_address | VARCHAR(500) | Юридический адрес |
| inn | VARCHAR(20) | ИНН организации |
| director_name | VARCHAR(255) | ФИО директора |
| phone | VARCHAR(50) | Контактный телефон |
| email | VARCHAR(100) | Электронная почта |
| rating | INTEGER | Рейтинг (≥ 0) |
| created_at | TIMESTAMP | Дата создания |
| updated_at | TIMESTAMP | Дата обновления |

#### 3. sales - История продаж

| Поле | Тип | Описание |
|------|-----|----------|
| id | SERIAL | Первичный ключ |
| partner_id | INTEGER | Внешний ключ на partners |
| product_name | VARCHAR(255) | Наименование продукции |
| quantity | INTEGER | Количество (> 0) |
| sale_date | DATE | Дата продажи |
| amount | NUMERIC(12,2) | Сумма (≥ 0) |

### Связи между таблицами

```
partner_types (1) ──────< partners (N)
partners (1) ──────< sales (N)
```

### Нормализация

База данных приведена к **третьей нормальной форме (3NF)**:

1. **1NF**: Все поля атомарны, нет повторяющихся групп
2. **2NF**: Все неключевые поля зависят от первичного ключа целиком
3. **3NF**: Нет транзитивных зависимостей между неключевыми полями

### Ссылочная целостность

- **ON DELETE RESTRICT** для partner_types → partners (нельзя удалить тип, если есть партнеры)
- **ON DELETE CASCADE** для partners → sales (при удалении партнера удаляются продажи)

---

## Архитектура приложения

### Структура проекта

```
palkin/
├── palkinlib/              # Библиотека классов
│   ├── Data/               # PalkinDbContext
│   ├── Models/             # Модели данных
│   ├── Repositories/       # Репозитории
│   ├── Services/           # Бизнес-логика
│   └── Scripts/            # SQL скрипты
├── palkinprog/             # WPF приложение
│   └── Dialogs/            # Окна
└── palkinlibtests/         # Модульные тесты
```

### Диаграмма классов

```
┌─────────────────────────────────────────────────────────────────┐
│                        Models                                    │
├─────────────────┐  ┌─────────────────┐  ┌─────────────────┐    │
│  PartnerType    │  │    Partner      │  │     Sale        │    │
├─────────────────┤  ├─────────────────┤  ├─────────────────┤    │
│ +Id: int        │  │ +Id: int        │  │ +Id: int        │    │
│ +Name: string   │  │ +PartnerTypeId  │  │ +PartnerId      │    │
│ +Partners       │  │ +Name: string   │  │ +ProductName    │    │
└─────────────────┘  │ +PartnerType    │  │ +Quantity: int  │    │
                     │ +Rating: int    │  │ +SaleDate: date │    │
                     │ +GetDiscount()  │  │ +Amount: decimal│    │
                     └─────────────────┘  └─────────────────┘    │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│                      Repositories                                │
├──────────────────────┐  ┌──────────────────┐  ┌──────────────┐ │
│IPartnerTypeRepository│  │IPartnerRepository│  │ISaleRepository│ │
├──────────────────────┤  ├──────────────────┤  ├──────────────┤ │
│+GetAllAsync()        │  │+GetAllAsync()    │  │+GetByPartnerId││
│+GetByIdAsync()       │  │+GetByIdAsync()   │  │+AddAsync()   │ │
│+AddAsync()           │  │+AddAsync()       │  │+UpdateAsync()│ │
│+UpdateAsync()        │  │+UpdateAsync()    │  │+DeleteAsync()│ │
│+DeleteAsync()        │  │+DeleteAsync()    │  └──────────────┘ │
└──────────────────────┘  │+GetTotalSales()  │
                          └──────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│                        Services                                  │
├─────────────────────────────────────────────────────────────────┤
│  DiscountService (static)        PartnerService                 │
├─────────────────────────────────├───────────────────────────────┤
│+CalculateDiscountPercent()      │+GetAllPartnersAsync()         │
│+CalculateDiscountAmount()       │+GetPartnerByIdAsync()         │
│+GetDiscountDescription()        │+AddPartnerAsync()             │
└─────────────────────────────────│+UpdatePartnerAsync()          │
                                  │+DeletePartnerAsync()          │
                                  │+GetPartnerTypesAsync()        │
                                  │+GetPartnerSalesAsync()        │
                                  └───────────────────────────────┘
```

### Внедрение зависимостей

Приложение использует контейнер внедрения зависимостей `Microsoft.Extensions.DependencyInjection`:

```csharp
services.AddTransient<Func<PalkinDbContext>>(...);
services.AddTransient<IPartnerRepository, PartnerRepository>();
services.AddTransient<IPartnerService, PartnerService>();
services.AddTransient<MainWindow>();
```

---

## Реализация функционала

### 1. Главное окно (MainWindow)

**Функционал:**
- Отображение списка партнеров в виде таблицы
- Сортировка по колонкам
- Контекстное меню (редактировать, удалить, история продаж)
- Кнопки добавления и обновления
- Статусная строка

**XAML:** `palkinprog/MainWindow.xaml`

### 2. Окно редактирования партнера (PartnerEditWindow)

**Функционал:**
- Добавление нового партнера
- Редактирование существующего
- Валидация данных:
  - Наименование - обязательно
  - Тип партнера - обязательно
  - Рейтинг - неотрицательное целое число
  - Email - проверка формата
- Выпадающий список типов партнеров

**XAML:** `palkinprog/Dialogs/PartnerEditWindow.xaml`

### 3. Окно истории продаж (SalesHistoryWindow)

**Функционал:**
- Просмотр всех продаж партнера
- Отображение общей суммы продаж
- Расчет и отображение текущей скидки
- Информация о следующем уровне скидки

**XAML:** `palkinprog/Dialogs/SalesHistoryWindow.xaml`

### 4. Расчет скидки (DiscountService)

**Алгоритм:**
```
ЕСЛИ сумма_продаж >= 300000 ТО скидка = 15%
ИНАЧЕ ЕСЛИ сумма_продаж >= 50000 ТО скидка = 10%
ИНАЧЕ ЕСЛИ сумма_продаж >= 10000 ТО скидка = 5%
ИНАЧЕ скидка = 0%
```

**Реализация:** `palkinlib/Services/DiscountService.cs`

### 5. Обработка исключений

Приложение обрабатывает следующие ситуации:
- Ошибка подключения к БД - сообщение с инструкцией
- Ошибка валидации - подсветка полей, информативные сообщения
- Ошибка удаления - подтверждение необратимого действия
- Пустой выбор - предупреждение о необходимости выбора

---

## Тестирование

### Модульные тесты

**Файмы тестов:**
- `DiscountServiceTests.cs` - тесты сервиса скидок (26 тестов)
- `PartnerServiceTests.cs` - тесты сервиса партнеров

**Покрытие:**
- Расчет скидок для всех диапазонов
- CRUD операции с партнерами
- Работа с репозиториями (моки)

### Результаты тестирования

```
Сводка теста: всего: 26; сбой: 0; успешно: 26; пропущено: 0
```

### Интеграционное тестирование

Проверена работа приложения:
- ✓ Загрузка списка партнеров из БД
- ✓ Добавление нового партнера
- ✓ Редактирование существующего партнера
- ✓ Удаление партнера с подтверждением
- ✓ Просмотр истории продаж
- ✓ Корректный расчет скидки
- ✓ Валидация входных данных

---

## Заключение

### Выполненные требования

| Требование | Статус |
|------------|--------|
| База данных PostgreSQL с 3NF | ✓ |
| Ссылочная целостность | ✓ |
| ER-диаграмма | ✓ |
| WPF интерфейс | ✓ |
| Entity Framework Core | ✓ |
| Добавление/редактирование/удаление | ✓ |
| Просмотр истории продаж | ✓ |
| Расчет скидки | ✓ |
| Обработка исключений | ✓ |
| Валидация данных | ✓ |
| Модульные тесты | ✓ |
| CamelCase идентификаторы | ✓ |
| Комментарии в коде | ✓ |

### Структура файлов

```
palkin/
├── palkin.sln                          # Решение
├── README.md                           # Инструкция
├── palkinlib/
│   ├── palkinlib.csproj
│   ├── Data/
│   │   └── PalkinDbContext.cs
│   ├── Models/
│   │   ├── Partner.cs
│   │   ├── PartnerType.cs
│   │   └── Sale.cs
│   ├── Repositories/
│   │   ├── Interfaces.cs
│   │   ├── PartnerRepository.cs
│   │   ├── PartnerTypeRepository.cs
│   │   └── SaleRepository.cs
│   ├── Services/
│   │   ├── DiscountService.cs
│   │   └── PartnerService.cs
│   └── Scripts/
│       ├── CreateDatabase.sql
│       └── ER_Diagram.sql
├── palkinprog/
│   ├── palkinprog.csproj
│   ├── App.xaml.cs
│   ├── MainWindow.xaml
│   ├── MainWindow.xaml.cs
│   └── Dialogs/
│       ├── PartnerEditWindow.xaml
│       ├── PartnerEditWindow.xaml.cs
│       ├── SalesHistoryWindow.xaml
│       └── SalesHistoryWindow.xaml.cs
└── palkinlibtests/
    ├── palkinlibtests.csproj
    ├── DiscountServiceTests.cs
    └── PartnerServiceTests.cs
```

### Запуск приложения

```bash
# Сборка
dotnet build palkin.sln

# Запуск
dotnet run --project palkinprog

# Тесты
dotnet test palkinlibtests
```

---

**Дата выполнения:** 13 марта 2026 г.

**Студент:** Palkin
