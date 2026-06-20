using ECommerceApi.Data;
using ECommerceApi.DTOs;
using ECommerceApi.Models.Enums;
using ECommerceApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ECommerceApi.Services;

public class DashboardService : IDashboardService
{
    private readonly AppDbContext _context;

    public DashboardService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardStatsDto> GetStatsAsync()
    {
        var now = DateTime.UtcNow;
        var todayStart = now.Date;
        var todayEnd = todayStart.AddDays(1);

        // 本周一 00:00:00
        var weekStart = todayStart.AddDays(-(int)now.DayOfWeek);
        if (now.DayOfWeek == DayOfWeek.Sunday) weekStart = todayStart.AddDays(-6);

        // 本月 1 日 00:00:00
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        // 已完成订单（排除 Cancelled 状态）
        var completedOrders = _context.Orders
            .Where(o => o.Status != OrderStatus.Cancelled);

        // ===== 当天统计 =====
        var todayOrders = await completedOrders
            .Where(o => o.CreatedAt >= todayStart && o.CreatedAt < todayEnd)
            .ToListAsync();

        var todayRevenue = todayOrders.Sum(o => o.TotalAmount);
        var todayCount = todayOrders.Count;
        var todayQuantity = await _context.OrderItems
            .Where(oi => oi.Order.CreatedAt >= todayStart && oi.Order.CreatedAt < todayEnd)
            .Where(oi => oi.Order.Status != OrderStatus.Cancelled)
            .SumAsync(oi => (int?)oi.Quantity) ?? 0;

        // ===== 当周统计 =====
        var weekOrders = await completedOrders
            .Where(o => o.CreatedAt >= weekStart && o.CreatedAt < todayEnd)
            .ToListAsync();

        var weekRevenue = weekOrders.Sum(o => o.TotalAmount);
        var weekCount = weekOrders.Count;
        var weekQuantity = await _context.OrderItems
            .Where(oi => oi.Order.CreatedAt >= weekStart && oi.Order.CreatedAt < todayEnd)
            .Where(oi => oi.Order.Status != OrderStatus.Cancelled)
            .SumAsync(oi => (int?)oi.Quantity) ?? 0;

        // ===== 当月统计 =====
        var monthOrders = await completedOrders
            .Where(o => o.CreatedAt >= monthStart && o.CreatedAt < todayEnd)
            .ToListAsync();

        var monthRevenue = monthOrders.Sum(o => o.TotalAmount);
        var monthCount = monthOrders.Count;
        var monthQuantity = await _context.OrderItems
            .Where(oi => oi.Order.CreatedAt >= monthStart && oi.Order.CreatedAt < todayEnd)
            .Where(oi => oi.Order.Status != OrderStatus.Cancelled)
            .SumAsync(oi => (int?)oi.Quantity) ?? 0;

        // ===== 最近7天每日营收和订单（用于图表） =====
        var sevenDaysAgo = todayStart.AddDays(-6);
        var recentOrders = await completedOrders
            .Where(o => o.CreatedAt >= sevenDaysAgo && o.CreatedAt < todayEnd)
            .ToListAsync();

        var dailyRevenues = new List<DailyRevenueDto>();
        var dailyOrders = new List<DailyOrderDto>();

        for (int i = 6; i >= 0; i--)
        {
            var date = todayStart.AddDays(-i);
            var dayOrders = recentOrders.Where(o => o.CreatedAt.Date == date).ToList();
            dailyRevenues.Add(new DailyRevenueDto
            {
                Date = date.ToString("MM-dd"),
                Revenue = dayOrders.Sum(o => o.TotalAmount)
            });
            dailyOrders.Add(new DailyOrderDto
            {
                Date = date.ToString("MM-dd"),
                Count = dayOrders.Count
            });
        }

        // ===== 商品销量排行 Top5 =====
        var topProducts = await _context.OrderItems
            .Where(oi => oi.Order.Status != OrderStatus.Cancelled)
            .GroupBy(oi => oi.Product.Name)
            .Select(g => new ProductSalesDto
            {
                ProductName = g.Key,
                TotalQuantity = g.Sum(oi => oi.Quantity),
                TotalRevenue = g.Sum(oi => oi.SubTotal)
            })
            .OrderByDescending(p => p.TotalQuantity)
            .Take(5)
            .ToListAsync();

        return new DashboardStatsDto
        {
            TodayRevenue = todayRevenue,
            TodayOrders = todayCount,
            TodayQuantity = todayQuantity,
            WeekRevenue = weekRevenue,
            WeekOrders = weekCount,
            WeekQuantity = weekQuantity,
            MonthRevenue = monthRevenue,
            MonthOrders = monthCount,
            MonthQuantity = monthQuantity,
            DailyRevenues = dailyRevenues,
            DailyOrders = dailyOrders,
            TopProducts = topProducts
        };
    }
}
