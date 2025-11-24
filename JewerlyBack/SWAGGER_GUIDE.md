# Jewerly API - Swagger Documentation Guide

## Доступ к Swagger UI

### Development
```
http://localhost:5000/swagger
```

### Production
```
https://your-api-domain.com/swagger
```

⚠️ **Production Note**: В production Swagger рекомендуется защитить (IP whitelist, Basic Auth) или отключить.

---

## Как использовать Swagger для интеграции Flutter приложения

### 1. Аутентификация в Swagger UI

#### Шаг 1: Получить JWT токен
Используйте один из endpoints:
- **Регистрация**: `POST /api/account/register`
- **Вход**: `POST /api/account/login`
- **Google Sign-In**: `POST /api/account/google`
- **Apple Sign-In**: `POST /api/account/apple`

Пример запроса (регистрация):
```json
POST /api/account/register
{
  "email": "user@example.com",
  "password": "SecurePassword123",
  "name": "John Doe"
}
```

Ответ:
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": 1234567890,
  "tokenType": "Bearer",
  "userId": "123e4567-e89b-12d3-a456-426614174000",
  "email": "user@example.com",
  "name": "John Doe",
  "provider": "local"
}
```

#### Шаг 2: Авторизация в Swagger UI
1. Нажмите кнопку **🔒 Authorize** в правом верхнем углу
2. Введите **только токен** (без слова "Bearer"):
   ```
   eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
   ```
3. Нажмите **Authorize**
4. Нажмите **Close**

Теперь все защищенные endpoints будут автоматически использовать этот токен.

---

### 2. Интеграция во Flutter приложении

#### Установка зависимостей
```yaml
dependencies:
  http: ^1.1.0
  # или
  dio: ^5.4.0
```

#### Пример кода (http package)

```dart
import 'dart:convert';
import 'package:http/http.dart' as http;

class JewerlyApiClient {
  static const String baseUrl = 'http://localhost:5000'; // Dev
  // static const String baseUrl = 'https://api.jewerly.com'; // Production

  String? _accessToken;

  // Сохранить токен после аутентификации
  void setToken(String token) {
    _accessToken = token;
  }

  // Получить заголовки с токеном
  Map<String, String> _getHeaders() {
    final headers = {
      'Content-Type': 'application/json',
      'Accept': 'application/json',
    };

    if (_accessToken != null) {
      headers['Authorization'] = 'Bearer $_accessToken';
    }

    return headers;
  }

  // Регистрация
  Future<AuthResponse> register({
    required String email,
    required String password,
    String? name,
  }) async {
    final response = await http.post(
      Uri.parse('$baseUrl/api/account/register'),
      headers: _getHeaders(),
      body: jsonEncode({
        'email': email,
        'password': password,
        'name': name,
      }),
    );

    if (response.statusCode == 201) {
      final data = jsonDecode(response.body);
      setToken(data['accessToken']);
      return AuthResponse.fromJson(data);
    } else {
      throw Exception('Registration failed: ${response.body}');
    }
  }

  // Вход
  Future<AuthResponse> login({
    required String email,
    required String password,
  }) async {
    final response = await http.post(
      Uri.parse('$baseUrl/api/account/login'),
      headers: _getHeaders(),
      body: jsonEncode({
        'email': email,
        'password': password,
      }),
    );

    if (response.statusCode == 200) {
      final data = jsonDecode(response.body);
      setToken(data['accessToken']);
      return AuthResponse.fromJson(data);
    } else {
      throw Exception('Login failed: ${response.body}');
    }
  }

  // Получить профиль (защищенный endpoint)
  Future<UserProfile> getProfile() async {
    final response = await http.get(
      Uri.parse('$baseUrl/api/account/profile'),
      headers: _getHeaders(),
    );

    if (response.statusCode == 200) {
      return UserProfile.fromJson(jsonDecode(response.body));
    } else if (response.statusCode == 401) {
      throw Exception('Unauthorized - token expired or invalid');
    } else {
      throw Exception('Failed to load profile: ${response.body}');
    }
  }

  // Получить каталог (публичный endpoint)
  Future<List<Category>> getCategories() async {
    final response = await http.get(
      Uri.parse('$baseUrl/api/catalog/categories'),
      headers: _getHeaders(),
    );

    if (response.statusCode == 200) {
      final List<dynamic> data = jsonDecode(response.body);
      return data.map((json) => Category.fromJson(json)).toList();
    } else {
      throw Exception('Failed to load categories: ${response.body}');
    }
  }

  // Загрузить файл (multipart/form-data)
  Future<AssetUploadResponse> uploadAsset({
    required String filePath,
    required String fileType,
    String? configurationId,
  }) async {
    var request = http.MultipartRequest(
      'POST',
      Uri.parse('$baseUrl/api/assets/upload'),
    );

    // Добавляем заголовки
    if (_accessToken != null) {
      request.headers['Authorization'] = 'Bearer $_accessToken';
    }

    // Добавляем файл
    request.files.add(
      await http.MultipartFile.fromPath('file', filePath),
    );

    // Добавляем поля формы
    request.fields['fileType'] = fileType;
    if (configurationId != null) {
      request.fields['configurationId'] = configurationId;
    }

    final streamedResponse = await request.send();
    final response = await http.Response.fromStream(streamedResponse);

    if (response.statusCode == 201) {
      return AssetUploadResponse.fromJson(jsonDecode(response.body));
    } else {
      throw Exception('Upload failed: ${response.body}');
    }
  }
}

// Model classes
class AuthResponse {
  final String accessToken;
  final int expiresAt;
  final String tokenType;
  final String userId;
  final String email;
  final String? name;
  final String? provider;

  AuthResponse({
    required this.accessToken,
    required this.expiresAt,
    required this.tokenType,
    required this.userId,
    required this.email,
    this.name,
    this.provider,
  });

  factory AuthResponse.fromJson(Map<String, dynamic> json) {
    return AuthResponse(
      accessToken: json['accessToken'],
      expiresAt: json['expiresAt'],
      tokenType: json['tokenType'],
      userId: json['userId'],
      email: json['email'],
      name: json['name'],
      provider: json['provider'],
    );
  }
}

class UserProfile {
  final String id;
  final String email;
  final String? name;
  final String? provider;
  final bool isEmailConfirmed;
  final String createdAt;
  final String? lastLoginAt;

  UserProfile({
    required this.id,
    required this.email,
    this.name,
    this.provider,
    required this.isEmailConfirmed,
    required this.createdAt,
    this.lastLoginAt,
  });

  factory UserProfile.fromJson(Map<String, dynamic> json) {
    return UserProfile(
      id: json['id'],
      email: json['email'],
      name: json['name'],
      provider: json['provider'],
      isEmailConfirmed: json['isEmailConfirmed'],
      createdAt: json['createdAt'],
      lastLoginAt: json['lastLoginAt'],
    );
  }
}

class Category {
  final int id;
  final String name;
  final String? description;

  Category({
    required this.id,
    required this.name,
    this.description,
  });

  factory Category.fromJson(Map<String, dynamic> json) {
    return Category(
      id: json['id'],
      name: json['name'],
      description: json['description'],
    );
  }
}

class AssetUploadResponse {
  final String id;
  final String url;
  final String? originalFileName;
  final String fileType;
  final String message;

  AssetUploadResponse({
    required this.id,
    required this.url,
    this.originalFileName,
    required this.fileType,
    required this.message,
  });

  factory AssetUploadResponse.fromJson(Map<String, dynamic> json) {
    return AssetUploadResponse(
      id: json['id'],
      url: json['url'],
      originalFileName: json['originalFileName'],
      fileType: json['fileType'],
      message: json['message'],
    );
  }
}
```

#### Использование в приложении

```dart
void main() async {
  final api = JewerlyApiClient();

  try {
    // Регистрация
    final authResponse = await api.register(
      email: 'user@example.com',
      password: 'SecurePassword123',
      name: 'John Doe',
    );

    print('Logged in! Token: ${authResponse.accessToken}');

    // Получить профиль
    final profile = await api.getProfile();
    print('User profile: ${profile.email}');

    // Получить каталог (публичный)
    final categories = await api.getCategories();
    print('Categories count: ${categories.length}');

  } catch (e) {
    print('Error: $e');
  }
}
```

---

### 3. CORS для Flutter Web

Если вы разрабатываете Flutter Web приложение, убедитесь, что ваш origin добавлен в `appsettings.json`:

```json
"Cors": {
  "AllowedOrigins": [
    "http://localhost:4200",
    "http://localhost:5000",
    "http://localhost:8080",
    "http://localhost:3000"
  ]
}
```

Для production замените на реальные домены:
```json
"Cors": {
  "AllowedOrigins": [
    "https://app.jewerly.com",
    "https://jewerly.com"
  ]
}
```

---

### 4. Основные Endpoints

#### Публичные (не требуют авторизации)
- `POST /api/account/register` — регистрация
- `POST /api/account/login` — вход
- `POST /api/account/google` — Google Sign-In
- `POST /api/account/apple` — Apple Sign-In
- `GET /api/catalog/categories` — список категорий
- `GET /api/catalog/materials` — список материалов
- `GET /api/catalog/stone-types` — типы камней
- `GET /api/catalog/base-models` — базовые модели
- `GET /api/health/live` — liveness probe
- `GET /api/health/ready` — readiness probe

#### Защищенные (требуют JWT токен)
- `GET /api/account/profile` — получить профиль
- `GET /api/configurations` — список конфигураций
- `POST /api/configurations` — создать конфигурацию
- `GET /api/orders` — список заказов
- `POST /api/orders` — создать заказ
- `POST /api/assets/upload` — загрузить файл
- `GET /api/assets` — список ассетов

---

### 5. Обработка ошибок

API возвращает стандартизированные ошибки:

```json
{
  "status": 400,
  "message": "Invalid credentials",
  "correlationId": "0HMVEK5L3NBBJ:00000001",
  "timestamp": "2024-01-15T10:30:00Z"
}
```

В Flutter обрабатывайте их так:

```dart
try {
  final response = await api.login(email: email, password: password);
} on http.ClientException catch (e) {
  // Сетевая ошибка
  print('Network error: $e');
} catch (e) {
  // Ошибка API
  if (e.toString().contains('401')) {
    print('Invalid credentials');
  } else if (e.toString().contains('500')) {
    print('Server error');
  } else {
    print('Unknown error: $e');
  }
}
```

---

## Дополнительная информация

- **API Base URL (Dev)**: `http://localhost:5000`
- **API Base URL (Prod)**: `https://your-api-domain.com`
- **Swagger URL**: `/swagger`
- **JWT Token Lifetime**: 60 минут (по умолчанию)
- **Refresh Token**: TODO (будет добавлено позже)

## Полезные команды

### Сгенерировать Dart модели из Swagger
```bash
# Установить swagger_to_openapi
dart pub global activate openapi_generator_cli

# Сгенерировать модели
openapi-generator-cli generate \
  -i http://localhost:5000/swagger/v1/swagger.json \
  -g dart \
  -o lib/api
```

### Проверить доступность API
```bash
curl http://localhost:5000/api/health/live
```

### Получить токен через curl
```bash
curl -X POST http://localhost:5000/api/account/login \
  -H "Content-Type: application/json" \
  -d '{"email":"user@example.com","password":"password123"}'
```
