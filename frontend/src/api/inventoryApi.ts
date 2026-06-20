import http from './http'
import type { Product } from '@/types/product'

export const inventoryApi = {
  getAll: () => http.get<Product[]>('/inventory'),
  updateStock: (productId: number, stock: number) =>
    http.put<Product>(`/inventory/${productId}/stock`, { stock })
}
