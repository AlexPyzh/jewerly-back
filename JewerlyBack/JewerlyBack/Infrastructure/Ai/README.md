# OpenAI Integration Configuration

## Обзор

Модуль для интеграции с OpenAI API для генерации AI-превью ювелирных изделий.

## Структура

- **Configuration/OpenAiOptions.cs** — класс конфигурации для OpenAI API
- **OpenAiImageProvider.cs** — реализация сервиса для генерации изображений (заглушка)
- **IAiImageProvider** (в Application/Ai/) — интерфейс сервиса

## Настройка API-ключа

### ⚠️ ВАЖНО: Безопасность

**НИКОГДА** не храните реальный API-ключ OpenAI в файлах конфигурации (`appsettings.json`)!

Ключ должен передаваться через:

1. **Переменные окружения** (для Production/Docker):
   ```bash
   export Ai__OpenAi__ApiKey="sk-YOUR_OPENAI_API_KEY"
   ```

2. **User Secrets** (для локальной разработки):
   ```bash
   # В директории проекта JewerlyBack
   dotnet user-secrets set "Ai:OpenAi:ApiKey" "sk-YOUR_OPENAI_API_KEY"
   ```

### Валидация при старте

Приложение автоматически валидирует наличие `ApiKey` при старте:
- Если ключ не задан → приложение упадёт с ошибкой валидации
- Это предотвращает случайный запуск без настроенного ключа

### Структура конфигурации

В `appsettings.json`:

```json
{
  "Ai": {
    "OpenAi": {
      "ApiKey": "",                                  // ОСТАВИТЬ ПУСТЫМ!
      "Model": "dall-e-3",                           // Модель для генерации
      "BaseUrl": "https://api.openai.com/v1",        // OpenAI API endpoint
      "TimeoutSeconds": 120,                          // Таймаут HTTP-запросов
      "MaxRetryAttempts": 3                           // Количество повторных попыток
    }
  }
}
```

## Использование в коде

### Dependency Injection

Сервис `IAiImageProvider` зарегистрирован в DI как `Scoped`:

```csharp
public class YourService
{
    private readonly IAiImageProvider _aiImageProvider;

    public YourService(IAiImageProvider aiImageProvider)
    {
        _aiImageProvider = aiImageProvider;
    }

    public async Task GeneratePreviewAsync(string prompt)
    {
        var imageUrl = await _aiImageProvider.GenerateSinglePreviewAsync(prompt);
        // imageUrl содержит публичный URL из S3
    }
}
```

### Доступ к настройкам (если нужно)

```csharp
using JewerlyBack.Infrastructure.Ai.Configuration;
using Microsoft.Extensions.Options;

public class YourService
{
    private readonly OpenAiOptions _openAiOptions;

    public YourService(IOptions<OpenAiOptions> options)
    {
        _openAiOptions = options.Value;
    }
}
```

## Roadmap

### Текущий статус
✅ Конфигурация настроена
✅ DI зарегистрирован
⏳ Реализация API-интеграции (следующий шаг)

### Следующие шаги
1. Реализовать HTTP-клиент для OpenAI API
2. Интеграция с S3 для загрузки сгенерированных изображений
3. Добавить методы для 360-превью (множественные изображения)
4. Error handling и retry logic
5. Логирование и мониторинг API-вызовов

## Получение API-ключа OpenAI

1. Зарегистрируйтесь на https://platform.openai.com/
2. Перейдите в раздел "API Keys"
3. Создайте новый ключ
4. Сохраните ключ в user-secrets (локально) или переменных окружения (production)

## Troubleshooting

### Ошибка: "OpenAI ApiKey must be provided"

**Причина:** API-ключ не задан при старте приложения.

**Решение:**
```bash
# Локально (Development)
dotnet user-secrets set "Ai:OpenAi:ApiKey" "sk-YOUR_KEY"

# Production (через переменные окружения)
export Ai__OpenAi__ApiKey="sk-YOUR_KEY"
```

### Проверка user-secrets

```bash
# Показать все secrets
dotnet user-secrets list

# Удалить конкретный secret
dotnet user-secrets remove "Ai:OpenAi:ApiKey"

# Очистить все secrets
dotnet user-secrets clear
```
