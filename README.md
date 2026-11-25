# SportsCenter API

Web API для керування спортивним центром: види занять, розклад, реєстрація відвідувачів, скасування занять, облік відвідуваності та звіт про завантаженість залів.

Навчальний проєкт з дисципліни МРПЗ (лабораторна робота №2, команда 7, група ТА-42). Вимоги взято з декомпозиції в Jira (проєкт `SCRUM`).

## Стек

| Що | Чим |
| --- | --- |
| Мова і платформа | C#, .NET 8 |
| Тип застосунку | ASP.NET Core Web API (контролери) |
| Доступ до даних | Entity Framework Core, PostgreSQL (Npgsql); у тестах — InMemory |
| Документація API | Swagger (Swashbuckle) + XML-коментарі |
| Тести | xUnit |
| Контейнеризація | Docker, Docker Compose, pgAdmin |

## Швидкий старт

Потрібен [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).

```bash
git clone https://github.com/AfricaEE112/MZPR
cd MZPR

# тести (база даних не потрібна — використовується InMemory)
dotnet test
```

Запуск самого API потребує PostgreSQL, тому найпростіше запускати його через Docker Compose (див. нижче).

## Запуск у Docker (лабораторна робота №3)

Потрібен [Docker Desktop](https://docs.docker.com/desktop/). Застосунок складається з трьох контейнерів:

| Сервіс | Образ | Призначення | Адреса |
| --- | --- | --- | --- |
| `api` | збирається з `Dockerfile` | ASP.NET Core Web API | http://localhost:8080/swagger |
| `postgres` | `postgres:16-alpine` | база даних | `localhost:5432` |
| `pgadmin` | `dpage/pgadmin4` | веб-інтерфейс для БД | http://localhost:5050 |

```bash
cp .env.example .env        # задайте свої логіни й паролі в .env
docker compose up --build -d
```

- Усі контейнери працюють в одній мережі `sportscenter_net`; API підключається до БД за іменем сервісу `postgres`.
- Дані PostgreSQL зберігаються в Docker Volume `postgres_data` і переживають перезапуск (`docker compose down` / `up`). Щоб видалити й дані: `docker compose down -v`.
- Параметри підключення та паролі передаються через змінні середовища з файлу `.env` (він у `.gitignore`).
- Таблиці створюються автоматично під час першого запуску API.

Підключення в pgAdmin: *Register → Server*, host `postgres`, port `5432`, логін і пароль із `.env` (`POSTGRES_USER`, `POSTGRES_PASSWORD`).

## Структура

```
SportsCenter.sln
├── SportsCenter.Api/
│   ├── Program.cs              # реєстрація сервісів (DI), PostgreSQL, Swagger
│   ├── Data/                   # SportsCenterDbContext (EF Core)
│   ├── Models/                 # сутності та константи статусів
│   └── Controllers/            # ендпоінти та бізнес-правила
└── SportsCenter.Tests/         # xUnit-тести, по одному класу на контролер
Dockerfile                      # образ Web API
docker-compose.yaml             # api + postgres + pgadmin
.env.example                    # шаблон змінних середовища
```

## Ендпоінти

| Метод і маршрут | Призначення | Jira |
| --- | --- | --- |
| `GET /api/ActivityTypes` | Список видів занять | SCRUM-10 |
| `POST /api/ActivityTypes` | Створити вид заняття | SCRUM-10 |
| `PUT /api/ActivityTypes/{id}` | Оновити вид заняття | SCRUM-10 |
| `DELETE /api/ActivityTypes/{id}` | Видалити вид заняття | SCRUM-10 |
| `POST /api/Schedule` | Додати заняття до розкладу | SCRUM-11 |
| `POST /api/Registration` | Записати відвідувача на заняття | SCRUM-12 |
| `GET /api/Registration/{visitorName}` | Записи відвідувача | SCRUM-12 |
| `PUT /api/ClassSession/{id}/cancel` | Скасувати заняття | SCRUM-13 |
| `POST /api/ClassSession/{id}/attendance` | Відмітити присутність | SCRUM-13 |
| `GET /api/ClassSession/report?startDate=&endDate=` | Звіт про завантаженість за період | SCRUM-13 |

## Основні бізнес-правила

- Вид заняття потребує назви, тривалості та ліміту місць більших за нуль.
- Заняття в розкладі не може накладатися за часом з іншим у тій самій аудиторії або з тим самим тренером. Скасовані заняття не блокують час.
- Реєстрація неможлива на скасоване заняття, при вичерпаному ліміті місць та повторно для того самого відвідувача (`409 Conflict`).
- Повторна відмітка присутності оновлює наявний запис, а не створює дублікат.
- Звіт не враховує скасовані заняття в місткості та відвідуваності, а показує їх окремо.

## Розробка

- Підхід TDD: тест → мінімальна реалізація → рефакторинг (приклад: коміти `2ffa121` → `ee93441` → `c4f58b7`).
- Нова функціональність розробляється в окремій гілці і зливається в `main` через Pull Request.
- Повідомлення комітів: `Feat:`, `Test:`, `Refactor:`, `Fix:`, `Docs:` + номер задачі Jira.
