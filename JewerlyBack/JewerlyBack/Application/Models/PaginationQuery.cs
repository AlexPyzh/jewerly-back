using System.ComponentModel.DataAnnotations;

namespace JewerlyBack.Application.Models;

/// <summary>
/// Параметры пагинации для списковых эндпоинтов
/// </summary>
/// <remarks>
/// Используется в query string: ?page=1&pageSize=20
///
/// Ограничения:
/// - Page должен быть >= 1
/// - PageSize должен быть от 1 до MaxPageSize (100)
///
/// Значения по умолчанию:
/// - Page = 1
/// - PageSize = 20
/// </remarks>
public class PaginationQuery
{
    /// <summary>
    /// Максимальный размер страницы (защита от слишком больших запросов)
    /// </summary>
    public const int MaxPageSize = 100;

    /// <summary>
    /// Размер страницы по умолчанию
    /// </summary>
    public const int DefaultPageSize = 20;

    private int _page = 1;
    private int _pageSize = DefaultPageSize;

    /// <summary>
    /// Номер страницы (начинается с 1)
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Page must be greater than or equal to 1")]
    public int Page
    {
        get => _page;
        set => _page = value < 1 ? 1 : value;
    }

    /// <summary>
    /// Размер страницы (количество элементов)
    /// </summary>
    /// <remarks>
    /// Максимальное значение: 100 элементов
    /// Минимальное значение: 1 элемент
    /// По умолчанию: 20 элементов
    /// </remarks>
    [Range(1, MaxPageSize, ErrorMessage = "PageSize must be between 1 and 100")]
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value switch
        {
            < 1 => DefaultPageSize,
            > MaxPageSize => MaxPageSize,
            _ => value
        };
    }

    /// <summary>
    /// Вычислить количество элементов для пропуска (для Skip в LINQ)
    /// </summary>
    public int Skip => (Page - 1) * PageSize;
}
