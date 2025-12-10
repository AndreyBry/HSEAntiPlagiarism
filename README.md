# HSE Anti-Plagiarism

Учебная система проверки работ на плагиат с микросервисной архитектурой.

## Технологии
- **ASP.NET Core (.NET 8)**
- **PostgreSQL** (отдельная БД для каждого сервиса)
- **S3 хранилище** (MinIO)
- **gRPC** для связи между микросервисами
- **Docker / Docker Compose**
- Принципы **SOLID**, разделение на слои **Domain / Application / Infrastructure / Abstractions**

---

## 1. Краткое описание архитектуры

Система состоит из трёх основных микросервисов и инфраструктурных компонентов:

### 1.1 Микросервисы

#### ApiGateway
Единственная точка входа для клиента.
- Внешний API: **HTTP REST**
- Внутренние вызовы: **gRPC** к FileStoring и FileAnalysis
- Не содержит бизнес-логики анализа или хранения — только оркестрация

#### FileStoringService
Сервис хранения файлов и метаданных.
- Файлы: **S3 (MinIO/AWS S3)**
- Метаданные: **PostgreSQL**
- Вычисляет и хранит:
  - `Checksum` файла
  - `Size`, `ContentType`, `StorageKey`
- Предоставляет gRPC методы:
  - `Upload`
  - `Download`
  - `GetMeta`

#### FileAnalysisService
Сервис анализа работ.
- Хранит:
  - `Work` (сдача работы)
  - `Report` (результаты анализа)
- Использует **gRPC клиент** к FileStoringService для доступа к файлам/метаданным
- Сценарии:
  - регистрация сдачи
  - проверка на плагиат по checksum
  - генерация облака слов

### 1.2 Хранилища и инфраструктура
- `storing-postgres` — БД FileStoringService
- `analysis-postgres` — БД FileAnalysisService
- `minio` — S3 хранилище

### 1.3 Общие контракты
gRPC контракты вынесены в отдельный проект:
```
src/BuildingBlocks/GrpcContracts/Protos
```
Этот проект хранит **единый источник .proto**, а генерация клиент/сервер кода выполняется в конкретных сервисах.

---

## 2. Пользовательские сценарии

Ниже описаны ключевые сценарии взаимодействия сервисов.

---

### 2.1 Сценарий: Сдача работы (Upload + Analysis)

**Цель:** студент отправляет файл, система сохраняет его и выполняет анализ.

#### Шаги обмена данными

1) **Клиент → ApiGateway**
- `POST /works`
- `multipart/form-data`:
  - `studentId`
  - `assignmentId`
  - `file`

2) **ApiGateway → FileStoringService (gRPC)**
- `Upload(fileName, contentType, data)`
- Результат:
  - `fileId`
  - `checksum`
  - `size`
  - `storageKey`

3) **ApiGateway → FileAnalysisService (gRPC)**
- `SubmitWork(studentId, assignmentId, fileId)`

4) **FileAnalysisService → FileStoringService (gRPC)**
- `GetMeta(fileId)`
- Получение `checksum`, `size`

5) **FileAnalysisService → Analysis DB**
- создаёт `Work`
- выполняет проверку плагиата:
  - ищет существующие `Work` по `assignmentId + checksum`
  - если есть более ранняя сдача **другим** студентом → `isPlagiarism = true`

6) **FileAnalysisService → Analysis DB**
- создаёт `Report`

7) **FileAnalysisService → ApiGateway**
- возвращает:
  - `workId`
  - `status`
  - `isPlagiarism`

8) **ApiGateway → Клиент**
- синхронный ответ результата сдачи

---

### 2.2 Сценарий: Получение отчётов по работе

**Цель:** клиент запрашивает историю/результаты анализа.

1) **Клиент → ApiGateway**
- `GET /works/{workId}/reports`

2) **ApiGateway → FileAnalysisService (gRPC)**
- `GetReports(workId)`

3) **FileAnalysisService → Analysis DB**
- читает отчёты

4) **ApiGateway → Клиент**
- возвращает список отчётов (JSON)

---

### 2.3 Сценарий: Генерация облака слов

**Цель:** получить визуализацию ключевых слов работы.

1) **Клиент → ApiGateway**
- `GET /works/{workId}/wordcloud`

2) **ApiGateway → FileAnalysisService (gRPC)**
- `BuildWordCloud(workId)`

3) **FileAnalysisService → Analysis DB**
- получает `Work` и соответствующий `fileId`

4) **FileAnalysisService → FileStoringService (gRPC)**
- `Download(fileId)`

5) **FileAnalysisService**
- извлекает текст
- нормализует токены
- фильтрует стоп-слова
- ограничивает объём текста (`WordCloud:MaxTextLength`)
- формирует частотный словарь

6) **FileAnalysisService**
- генерирует изображение облака слов

7) **FileAnalysisService → ApiGateway**
- возвращает `PNG` байты

8) **ApiGateway → Клиент**
- отдаёт `image/png`

---

## 3. Запуск системы

### 3.1 Требования
- .NET 8 SDK
- Docker Desktop

### 3.2 Структура Docker
`docker-compose.yml` расположен в папке:
```
docker/docker-compose.yml
```

### 3.3 Запуск всех сервисов
```powershell
cd docker
docker compose up --build
```

### 3.4 Проверка
Swagger:
- `http://localhost:8080/swagger`

---

## 4. Особенности архитектуры
- Каждый сервис имеет независимую БД.
- Межсервисное взаимодействие — gRPC.
- Слои:
  - **Domain** — сущности и бизнес-инварианты
  - **Application** — сценарии/юзкейсы
  - **Infrastructure** — EF Core, S3, внешние клиенты
  - **Abstractions** — контракты зависимостей
- ApiGateway не содержит доменной логики анализа/хранения.

---

## 5. Основные эндпоинты Gateway

- `POST /works` — сдать работу
- `GET /works/{workId}/reports` — получить отчёты
- `GET /works/{workId}/wordcloud` — получить облако слов (PNG)