export interface Product {
  id: number
  name: string
  price: number
  stock: number
  description?: string
  imageUrl?: string
  createdAt: string
  updatedAt: string
}

export interface CreateProduct {
  name: string
  price: number
  stock: number
  description?: string
  imageUrl?: string
}

export type UpdateProduct = CreateProduct
