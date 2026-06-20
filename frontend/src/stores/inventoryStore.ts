import { defineStore } from 'pinia'
import { ref } from 'vue'
import { inventoryApi } from '@/api/inventoryApi'
import type { Product } from '@/types/product'

export const useInventoryStore = defineStore('inventory', () => {
  const items = ref<Product[]>([])
  const loading = ref(false)

  async function fetchAll() {
    loading.value = true
    try {
      const { data } = await inventoryApi.getAll()
      items.value = data
    } finally {
      loading.value = false
    }
  }

  async function updateStock(productId: number, stock: number): Promise<Product> {
    const { data } = await inventoryApi.updateStock(productId, stock)
    const index = items.value.findIndex(i => i.id === productId)
    if (index !== -1) items.value[index] = data
    return data
  }

  return { items, loading, fetchAll, updateStock }
})
