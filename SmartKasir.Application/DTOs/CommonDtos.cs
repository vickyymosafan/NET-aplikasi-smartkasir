namespace SmartKasir.Application.DTOs;

/// <summary>
/// Hasil dengan pagination
/// </summary>
public record PagedResult<T>(
    List<T> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages);

/// <summary>
/// Parameter pagination
/// </summary>
public record PaginationParams(int Page = 1, int PageSize = 20);

/// <summary>
/// Response operasi generik
/// </summary>
public record OperationResult(bool Success, string? Message = null);

/// <summary>
/// Response operasi dengan data
/// </summary>
public record OperationResult<T>(bool Success, T? Data, string? Message = null);
