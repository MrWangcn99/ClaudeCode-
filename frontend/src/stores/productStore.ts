import { defineStore } from 'pinia'
import { ref } from 'vue'
import { productApi } from '@/api/productApi'
import type { Product, CreateProduct, UpdateProduct } from '@/types/product'

export const useProductStore = defineStore('product', () => {
  const products = ref<Product[]>([])
  const loading = ref(false)

  async function fetchAll() {
    loading.value = true
    try {
      const { data } = await productApi.getAll()
      products.value = data
    } finally {
      loading.value = false
    }
  }

  async function create(dto: CreateProduct): Promise<Product> {
    const { data } = await productApi.create(dto)
    products.value.push(data)
    return data
  }

  async function update(id: number, dto: UpdateProduct): Promise<Product> {
    const { data } = await productApi.update(id, dto)
    const index = products.value.findIndex(p => p.id === id)
    if (index !== -1) products.value[index] = data
    return data
  }

  async function remove(id: number) {
    await productApi.delete(id)
    products.value = products.value.filter(p => p.id !== id)
  }

  return { products, loading, fetchAll, create, update, remove }
})
