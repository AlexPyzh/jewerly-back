# Pagination Guide для Flutter разработчиков

## Обзор

Все списковые эндпоинты API теперь поддерживают пагинацию для эффективной работы с большими объемами данных.

## PagedResult<T> Response Format

Все пагинированные endpoints возвращают объект `PagedResult<T>`:

```json
{
  "items": [...],           // Массив элементов текущей страницы
  "page": 1,                // Номер текущей страницы (начинается с 1)
  "pageSize": 20,           // Размер страницы
  "totalCount": 150,        // Общее количество элементов
  "totalPages": 8,          // Общее количество страниц (вычисляется автоматически)
  "hasNextPage": true,      // Есть ли следующая страница
  "hasPreviousPage": false  // Есть ли предыдущая страница
}
```

## Query Parameters

### Параметры пагинации

- `page` - Номер страницы (по умолчанию: 1, минимум: 1)
- `pageSize` - Количество элементов на странице (по умолчанию: 20, максимум: 100)

### Примеры запросов

```bash
# Первая страница с размером по умолчанию (20)
GET /api/configurations

# Вторая страница с размером 10
GET /api/configurations?page=2&pageSize=10

# Максимальный размер страницы
GET /api/configurations?page=1&pageSize=100
```

## Пагинированные Endpoints

### 1. Configurations
```
GET /api/configurations?page={page}&pageSize={pageSize}
```

**Response Type**: `PagedResult<JewelryConfigurationListItemDto>`

### 2. Orders
```
GET /api/orders?page={page}&pageSize={pageSize}
```

**Response Type**: `PagedResult<OrderListItemDto>`

### 3. Assets
```
GET /api/assets?page={page}&pageSize={pageSize}
```

**Response Type**: `PagedResult<UploadedAssetDto>`

### 4. Catalog Base Models
```
GET /api/catalog/base-models?categoryId={categoryId}&page={page}&pageSize={pageSize}
```

**Response Type**: `PagedResult<JewelryBaseModelDto>`

## Flutter Implementation Examples

### 1. Базовая модель PagedResult

```dart
class PagedResult<T> {
  final List<T> items;
  final int page;
  final int pageSize;
  final int totalCount;
  final int totalPages;
  final bool hasNextPage;
  final bool hasPreviousPage;

  PagedResult({
    required this.items,
    required this.page,
    required this.pageSize,
    required this.totalCount,
    required this.totalPages,
    required this.hasNextPage,
    required this.hasPreviousPage,
  });

  factory PagedResult.fromJson(
    Map<String, dynamic> json,
    T Function(Map<String, dynamic>) fromJsonT,
  ) {
    return PagedResult(
      items: (json['items'] as List)
          .map((item) => fromJsonT(item as Map<String, dynamic>))
          .toList(),
      page: json['page'] as int,
      pageSize: json['pageSize'] as int,
      totalCount: json['totalCount'] as int,
      totalPages: json['totalPages'] as int,
      hasNextPage: json['hasNextPage'] as bool,
      hasPreviousPage: json['hasPreviousPage'] as bool,
    );
  }
}
```

### 2. Простая пагинация (Load More)

```dart
class ConfigurationsScreen extends StatefulWidget {
  @override
  _ConfigurationsScreenState createState() => _ConfigurationsScreenState();
}

class _ConfigurationsScreenState extends State<ConfigurationsScreen> {
  final JewerlyApiClient _api = JewerlyApiClient();

  List<Configuration> _configurations = [];
  int _currentPage = 1;
  int _pageSize = 20;
  bool _hasNextPage = true;
  bool _isLoading = false;

  @override
  void initState() {
    super.initState();
    _loadConfigurations();
  }

  Future<void> _loadConfigurations() async {
    if (_isLoading || !_hasNextPage) return;

    setState(() => _isLoading = true);

    try {
      final pagedResult = await _api.getConfigurations(
        page: _currentPage,
        pageSize: _pageSize,
      );

      setState(() {
        _configurations.addAll(pagedResult.items);
        _currentPage++;
        _hasNextPage = pagedResult.hasNextPage;
        _isLoading = false;
      });
    } catch (e) {
      setState(() => _isLoading = false);
      // Handle error
      print('Error loading configurations: $e');
    }
  }

  @override
  Widget build(BuildContext context) {
    return ListView.builder(
      itemCount: _configurations.length + (_hasNextPage ? 1 : 0),
      itemBuilder: (context, index) {
        if (index == _configurations.length) {
          // Load more indicator
          _loadConfigurations();
          return Center(child: CircularProgressIndicator());
        }

        return ConfigurationListItem(
          configuration: _configurations[index],
        );
      },
    );
  }
}
```

### 3. Infinite Scroll с NotificationListener

```dart
class InfiniteScrollList extends StatefulWidget {
  @override
  _InfiniteScrollListState createState() => _InfiniteScrollListState();
}

class _InfiniteScrollListState extends State<InfiniteScrollList> {
  final ScrollController _scrollController = ScrollController();
  final JewerlyApiClient _api = JewerlyApiClient();

  List<Order> _orders = [];
  int _currentPage = 1;
  int _pageSize = 20;
  bool _hasNextPage = true;
  bool _isLoading = false;

  @override
  void initState() {
    super.initState();
    _scrollController.addListener(_onScroll);
    _loadOrders();
  }

  @override
  void dispose() {
    _scrollController.dispose();
    super.dispose();
  }

  void _onScroll() {
    if (_scrollController.position.pixels >=
        _scrollController.position.maxScrollExtent * 0.8) {
      _loadOrders();
    }
  }

  Future<void> _loadOrders() async {
    if (_isLoading || !_hasNextPage) return;

    setState(() => _isLoading = true);

    try {
      final pagedResult = await _api.getOrders(
        page: _currentPage,
        pageSize: _pageSize,
      );

      setState(() {
        _orders.addAll(pagedResult.items);
        _currentPage++;
        _hasNextPage = pagedResult.hasNextPage;
        _isLoading = false;
      });
    } catch (e) {
      setState(() => _isLoading = false);
      print('Error loading orders: $e');
    }
  }

  Future<void> _refreshOrders() async {
    setState(() {
      _orders.clear();
      _currentPage = 1;
      _hasNextPage = true;
    });

    await _loadOrders();
  }

  @override
  Widget build(BuildContext context) {
    return RefreshIndicator(
      onRefresh: _refreshOrders,
      child: ListView.builder(
        controller: _scrollController,
        itemCount: _orders.length + (_isLoading ? 1 : 0),
        itemBuilder: (context, index) {
          if (index == _orders.length) {
            return Center(child: CircularProgressIndicator());
          }

          return OrderListItem(order: _orders[index]);
        },
      ),
    );
  }
}
```

### 4. API Client методы с пагинацией

```dart
class JewerlyApiClient {
  static const String baseUrl = 'http://localhost:5000';
  String? _accessToken;

  void setToken(String token) {
    _accessToken = token;
  }

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

  // GET Configurations с пагинацией
  Future<PagedResult<Configuration>> getConfigurations({
    int page = 1,
    int pageSize = 20,
  }) async {
    final uri = Uri.parse('$baseUrl/api/configurations')
        .replace(queryParameters: {
      'page': page.toString(),
      'pageSize': pageSize.toString(),
    });

    final response = await http.get(uri, headers: _getHeaders());

    if (response.statusCode == 200) {
      final json = jsonDecode(response.body);
      return PagedResult.fromJson(
        json,
        (item) => Configuration.fromJson(item),
      );
    } else {
      throw Exception('Failed to load configurations: ${response.body}');
    }
  }

  // GET Orders с пагинацией
  Future<PagedResult<Order>> getOrders({
    int page = 1,
    int pageSize = 20,
  }) async {
    final uri = Uri.parse('$baseUrl/api/orders')
        .replace(queryParameters: {
      'page': page.toString(),
      'pageSize': pageSize.toString(),
    });

    final response = await http.get(uri, headers: _getHeaders());

    if (response.statusCode == 200) {
      final json = jsonDecode(response.body);
      return PagedResult.fromJson(
        json,
        (item) => Order.fromJson(item),
      );
    } else {
      throw Exception('Failed to load orders: ${response.body}');
    }
  }

  // GET Assets с пагинацией
  Future<PagedResult<Asset>> getAssets({
    int page = 1,
    int pageSize = 20,
  }) async {
    final uri = Uri.parse('$baseUrl/api/assets')
        .replace(queryParameters: {
      'page': page.toString(),
      'pageSize': pageSize.toString(),
    });

    final response = await http.get(uri, headers: _getHeaders());

    if (response.statusCode == 200) {
      final json = jsonDecode(response.body);
      return PagedResult.fromJson(
        json,
        (item) => Asset.fromJson(item),
      );
    } else {
      throw Exception('Failed to load assets: ${response.body}');
    }
  }

  // GET Base Models с пагинацией
  Future<PagedResult<BaseModel>> getBaseModels({
    required int categoryId,
    int page = 1,
    int pageSize = 20,
  }) async {
    final uri = Uri.parse('$baseUrl/api/catalog/base-models')
        .replace(queryParameters: {
      'categoryId': categoryId.toString(),
      'page': page.toString(),
      'pageSize': pageSize.toString(),
    });

    final response = await http.get(uri, headers: _getHeaders());

    if (response.statusCode == 200) {
      final json = jsonDecode(response.body);
      return PagedResult.fromJson(
        json,
        (item) => BaseModel.fromJson(item),
      );
    } else {
      throw Exception('Failed to load base models: ${response.body}');
    }
  }
}
```

### 5. State Management с Provider

```dart
class ConfigurationsProvider with ChangeNotifier {
  final JewerlyApiClient _api;

  List<Configuration> _configurations = [];
  int _currentPage = 1;
  int _pageSize = 20;
  bool _hasNextPage = true;
  bool _isLoading = false;

  ConfigurationsProvider(this._api);

  List<Configuration> get configurations => _configurations;
  bool get isLoading => _isLoading;
  bool get hasNextPage => _hasNextPage;

  Future<void> loadMore() async {
    if (_isLoading || !_hasNextPage) return;

    _isLoading = true;
    notifyListeners();

    try {
      final pagedResult = await _api.getConfigurations(
        page: _currentPage,
        pageSize: _pageSize,
      );

      _configurations.addAll(pagedResult.items);
      _currentPage++;
      _hasNextPage = pagedResult.hasNextPage;
    } catch (e) {
      print('Error: $e');
    } finally {
      _isLoading = false;
      notifyListeners();
    }
  }

  Future<void> refresh() async {
    _configurations.clear();
    _currentPage = 1;
    _hasNextPage = true;
    await loadMore();
  }
}

// В Widget:
class ConfigurationsScreen extends StatelessWidget {
  @override
  Widget build(BuildContext context) {
    return Consumer<ConfigurationsProvider>(
      builder: (context, provider, child) {
        return RefreshIndicator(
          onRefresh: provider.refresh,
          child: ListView.builder(
            itemCount: provider.configurations.length +
                      (provider.hasNextPage ? 1 : 0),
            itemBuilder: (context, index) {
              if (index == provider.configurations.length) {
                provider.loadMore();
                return Center(child: CircularProgressIndicator());
              }
              return ConfigurationListItem(
                configuration: provider.configurations[index],
              );
            },
          ),
        );
      },
    );
  }
}
```

## Best Practices

### 1. Оптимальный PageSize
- **Списки карточек**: 20-30 элементов
- **Таблицы**: 10-15 элементов
- **Галереи**: 30-50 элементов

### 2. Кэширование
```dart
// Кэшировать первую страницу для быстрого отображения
SharedPreferences prefs = await SharedPreferences.getInstance();
String? cachedData = prefs.getString('configurations_page_1');
if (cachedData != null) {
  // Показать кэшированные данные
  setState(() {
    _configurations = json.decode(cachedData);
  });
}
// Затем загрузить свежие данные
```

### 3. Обработка ошибок
```dart
try {
  final result = await _api.getConfigurations(page: _currentPage);
  // Success
} on SocketException {
  // No internet connection
  showSnackBar('No internet connection');
} on HttpException {
  // Server error
  showSnackBar('Server error, please try again');
} catch (e) {
  // Unknown error
  showSnackBar('An error occurred: $e');
}
```

### 4. Pull-to-Refresh
Всегда реализуйте pull-to-refresh для обновления списка:
```dart
RefreshIndicator(
  onRefresh: () async {
    await provider.refresh();
  },
  child: ListView.builder(...),
)
```

### 5. Индикаторы загрузки
- Показывайте индикатор в конце списка при загрузке следующей страницы
- Используйте shimmer-эффект для первой загрузки
- Добавляйте pull-to-refresh индикатор

## Полезные поля PagedResult

```dart
// Показать общее количество элементов
Text('Total: ${pagedResult.totalCount} items')

// Показать текущую страницу
Text('Page ${pagedResult.page} of ${pagedResult.totalPages}')

// Отключить кнопку "Load More" если нет следующей страницы
ElevatedButton(
  onPressed: pagedResult.hasNextPage ? _loadMore : null,
  child: Text('Load More'),
)

// Показать прогресс
LinearProgressIndicator(
  value: pagedResult.page / pagedResult.totalPages,
)
```

## Тестирование пагинации

### Проверьте edge cases:
1. ✅ Первая страница (page=1)
2. ✅ Последняя страница (hasNextPage=false)
3. ✅ Пустой результат (items=[])
4. ✅ Большой pageSize (100)
5. ✅ Маленький pageSize (1)
6. ✅ Невалидные параметры (page=0, pageSize=0)

## Troubleshooting

### Problem: Items duplicating
**Solution**: Очищайте список при refresh:
```dart
Future<void> refresh() async {
  _items.clear();
  _currentPage = 1;
  await loadMore();
}
```

### Problem: Infinite loading loop
**Solution**: Проверяйте `hasNextPage` перед загрузкой:
```dart
if (!_hasNextPage || _isLoading) return;
```

### Problem: Slow performance with large lists
**Solution**: Используйте `ListView.builder` вместо `ListView` и рассмотрите lazy loading с небольшим `pageSize`.

---

## Дополнительная информация

Для получения дополнительной информации об API см. `SWAGGER_GUIDE.md` и Swagger UI (`/swagger`).
