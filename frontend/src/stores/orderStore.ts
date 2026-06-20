import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { orderApi } from '@/api/orderApi'
import type { Order, CreateOrderItem } from '@/types/order'
import { ElMessage } from 'element-plus'

export const useOrderStore = defineStore('order', () => {
  const orders = ref<Order[]>([])
  const currentOrder = ref<Order | null>(null)
  const loading = ref(false)
  const cartItems = ref<(CreateOrderItem & { productName?: string; price?: number })[]>([])

  const cartTotal = computed(() => {
    return cartItems.value.reduce((sum, item) => {
      return sum + (item.price || 0) * item.quantity
    }, 0)
  })

  const cartItemCount = computed(() => {
    return cartItems.value.reduce((sum, item) => sum + item.quantity, 0)
  })

  function addToCart(productId: number, productName: string, price: number, quantity: number = 1) {
    const existing = cartItems.value.find(i => i.productId === productId)
    if (existing) {
      existing.quantity += quantity
    } else {
      cartItems.value.push({ productId, quantity, productName, price })
    }
    ElMessage.success(`Added "${productName}" to cart`)
  }

  function removeFromCart(productId: number) {
    cartItems.value = cartItems.value.filter(i => i.productId !== productId)
  }

  function updateCartQuantity(productId: number, quantity: number) {
    const item = cartItems.value.find(i => i.productId === productId)
    if (item) {
      if (quantity <= 0) {
        removeFromCart(productId)
      } else {
        item.quantity = quantity
      }
    }
  }

  function clearCart() {
    cartItems.value = []
  }

  async function fetchAll(status?: string) {
    loading.value = true
    try {
      const { data } = await orderApi.getAll(status)
      orders.value = data
    } finally {
      loading.value = false
    }
  }

  async function fetchById(id: number): Promise<Order> {
    const { data } = await orderApi.getById(id)
    currentOrder.value = data
    return data
  }

  async function placeOrder(): Promise<Order | null> {
    if (cartItems.value.length === 0) {
      ElMessage.warning('Cart is empty')
      return null
    }

    const dto = {
      items: cartItems.value.map(i => ({
        productId: i.productId,
        quantity: i.quantity
      }))
    }

    try {
      const { data } = await orderApi.create(dto)
      orders.value.unshift(data)
      ElMessage.success(`Order ${data.orderNumber} placed successfully!`)
      clearCart()
      return data
    } catch (error: any) {
      const msg = error.response?.data?.error || 'Failed to place order'
      ElMessage.error(msg)
      throw error
    }
  }

  return {
    orders, currentOrder, loading,
    cartItems, cartTotal, cartItemCount,
    addToCart, removeFromCart, updateCartQuantity, clearCart,
    fetchAll, fetchById, placeOrder
  }
})
