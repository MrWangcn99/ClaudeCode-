namespace ECommerceApi.DTOs;

/// <summary>
/// 仪表盘统计数据
/// </summary>
public class DashboardStatsDto
{
    /// <summary>
    /// 当天营收额
    /// </summary>
    public decimal TodayRevenue { get; set; }

    /// <summary>
    /// 当天订单数
    /// </summary>
    public int TodayOrders { get; set; }

    /// <summary>
    /// 当天出货量（商品件数）
    /// </summary>
    public int TodayQuantity { get; set; }

    /// <summary>
    /// 当周营收额
    /// </summary>
    public decimal WeekRevenue { get; set; }

    /// <summary>
    /// 当周订单数
    /// </summary>
    public int WeekOrders { get; set; }

    /// <summary>
    /// 当周出货量
    /// </summary>
    public int WeekQuantity { get; set; }

    /// <summary>
    /// 当月营收额
    /// </summary>
    public decimal MonthRevenue { get; set; }

    /// <summary>
    /// 当月订单数
    /// </summary>
    public int MonthOrders { get; set; }

    /// <summary>
    /// 当月出货量
    /// </summary>
    public int MonthQuantity { get; set; }

    /// <summary>
    /// 最近7天每日营收（用于折线图）
    /// </summary>
    public List<DailyRevenueDto> DailyRevenues { get; set; } = new();

    /// <summary>
    /// 最近7天每日订单数（用于柱状图）
    /// </summary>
    public List<DailyOrderDto> DailyOrders { get; set; } = new();

    /// <summary>
    /// 商品销量排行 Top5
    /// </summary>
    public List<ProductSalesDto> TopProducts { get; set; } = new();
}

public class DailyRevenueDto
{
    public string Date { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
}

public class DailyOrderDto
{
    public string Date { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class ProductSalesDto
{
    public string ProductName { get; set; } = string.Empty;
    public int TotalQuantity { get; set; }
    public decimal TotalRevenue { get; set; }
}
