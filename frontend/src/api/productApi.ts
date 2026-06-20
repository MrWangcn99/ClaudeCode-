import http from './http'
import type { Product, CreateProduct, UpdateProduct } from '@/types/product'

export const productApi = {
  getAll: () => http.get<Product[]>('/products'),
  getById: (id: number) => http.get<Product>(`/products/${id}`),
  create: (data: CreateProduct) => http.post<Product>('/products', data),
  update: (id: number, data: UpdateProduct) => http.put<Product>(`/products/${id}`, data),
  delete: (id: number) => http.delete(`/products/${id}`)
}
