export interface DailyRevenue {
  date: string
  revenue: number
}

export interface DailyOrder {
  date: string
  count: number
}

export interface ProductSales {
  productName: string
  totalQuantity: number
  totalRevenue: number
}

export interface DashboardStats {
  todayRevenue: number
  todayOrders: number
  todayQuantity: number
  weekRevenue: number
  weekOrders: number
  weekQuantity: number
  monthRevenue: number
  monthOrders: number
  monthQuantity: number
  dailyRevenues: DailyRevenue[]
  dailyOrders: DailyOrder[]
  topProducts: ProductSales[]
}
