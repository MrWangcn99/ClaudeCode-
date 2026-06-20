import http from './http'
import type { Order, CreateOrder } from '@/types/order'

export const orderApi = {
  getAll: (status?: string) =>
    http.get<Order[]>('/orders', { params: status ? { status } : {} }),
  getById: (id: number) => http.get<Order>(`/orders/${id}`),
  create: (data: CreateOrder) => http.post<Order>('/orders', data)
}
