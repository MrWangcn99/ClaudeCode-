export interface OrderItem {
  id: number
  productId: number
  productName: string
  quantity: number
  unitPrice: number
  subTotal: number
}

export interface Order {
  id: number
  orderNumber: string
  status: string
  totalAmount: number
  createdAt: string
  items?: OrderItem[]
}

export interface CreateOrderItem {
  productId: number
  quantity: number
}

export interface CreateOrder {
  items: CreateOrderItem[]
}
