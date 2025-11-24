namespace JewerlyBack.Application.Models;

/// <summary>
/// Generic модель для пагинированных результатов
/// </summary>
/// <typeparam name="T">Тип элементов в списке</typeparam>
/// <remarks>
/// Используется для всех списковых эндпоинтов, которые возвращают большое количество данных.
/// Позволяет клиенту получать данные порциями и строить infinite scroll / pagination UI.
/// </remarks>
public class PagedResult<T>
{
    /// <summary>
    /// Список элементов текущей страницы
    /// </summary>
    public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();

    /// <summary>
    /// Номер текущей страницы (начинается с 1)
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// Размер страницы (количество элементов на странице)
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Общее количество элементов во всей коллекции
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Общее количество страниц
    /// </summary>
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;

    /// <summary>
    /// Есть ли следующая страница
    /// </summary>
    public bool HasNextPage => Page < TotalPages;

    /// <summary>
    /// Есть ли предыдущая страница
    /// </summary>
    public bool HasPreviousPage => Page > 1;
}
