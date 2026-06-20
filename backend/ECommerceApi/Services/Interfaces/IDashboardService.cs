using ECommerceApi.DTOs;

namespace ECommerceApi.Services.Interfaces;

public interface IDashboardService
{
    Task<DashboardStatsDto> GetStatsAsync();
}
