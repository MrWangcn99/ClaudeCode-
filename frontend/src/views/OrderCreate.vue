<template>
  <div class="page-container">
    <div class="page-header">
      <div class="page-title">
        <el-icon :size="24" color="#fa8c16"><ShoppingCart /></el-icon>
        <h2>创建订单</h2>
      </div>
    </div>

    <el-row :gutter="24">
      <!-- 左侧：商品选择 -->
      <el-col :span="15">
        <el-card class="content-card" shadow="never">
          <template #header>
            <div class="card-header">
              <span class="card-title">
                <el-icon><Goods /></el-icon>
                选择商品
              </span>
              <el-input
                v-model="searchQuery"
                placeholder="搜索商品..."
                clearable
                style="width: 260px"
              />
            </div>
          </template>

          <div v-loading="loading" class="product-list">
            <el-empty v-if="!loading && filteredProducts.length === 0" description="没有找到匹配的商品" />

            <div v-for="product in filteredProducts" :key="product.id" class="product-item">
              <!-- 图片 -->
              <div class="product-img-wrap">
                <img v-if="product.imageUrl" :src="product.imageUrl" class="product-img" @error="onImgError" />
                <div v-else class="img-placeholder">
                  <el-icon :size="24"><Picture /></el-icon>
                </div>
              </div>

              <!-- 信息 -->
              <div class="product-info">
                <div class="product-name">{{ product.name }}</div>
                <div class="product-meta">
                  <span class="product-price">¥{{ product.price.toFixed(2) }}</span>
                  <span class="meta-sep">|</span>
                  <span>
                    库存：
                    <el-tag :type="product.stock > 0 ? 'success' : 'danger'" size="small" effect="plain">
                      {{ product.stock }}
                    </el-tag>
                  </span>
                </div>
              </div>

              <!-- 操作 -->
              <div class="product-actions">
                <template v-if="product.stock > 0">
                  <el-input-number
                    v-model="quantities[product.id]"
                    :min="1"
                    :max="product.stock"
                    size="small"
                    style="width: 100px"
                  />
                  <el-button type="primary" size="small" round @click="handleAddToCart(product)">
                    <el-icon><Plus /></el-icon>
                    加入购物车
                  </el-button>
                </template>
                <el-tag v-else type="danger" size="default">已售罄</el-tag>
              </div>
            </div>
          </div>
        </el-card>
      </el-col>

      <!-- 右侧：购物车 -->
      <el-col :span="9">
        <el-card class="content-card cart-card" shadow="never">
          <template #header>
            <div class="card-header">
              <span class="card-title">
                <el-icon><ShoppingCart /></el-icon>
                购物车
                <el-badge v-if="orderStore.cartItemCount > 0" :value="orderStore.cartItemCount" :max="99" />
              </span>
              <el-button
                size="small"
                type="danger"
                text
                :disabled="orderStore.cartItems.length === 0"
                @click="orderStore.clearCart()"
              >
                清空
              </el-button>
            </div>
          </template>

          <el-empty
            v-if="orderStore.cartItems.length === 0"
            description="购物车为空，请从左侧选择商品"
            :image-size="80"
          />

          <template v-else>
            <div class="cart-list">
              <div v-for="item in orderStore.cartItems" :key="item.productId" class="cart-item">
                <div class="cart-item-info">
                  <div class="cart-item-name">{{ item.productName }}</div>
                  <div class="cart-item-price">¥{{ (item.price || 0).toFixed(2) }} / 件</div>
                </div>
                <div class="cart-item-actions">
                  <el-input-number
                    v-model="item.quantity"
                    :min="1"
                    size="small"
                    style="width: 90px"
                    @change="(val: number | undefined) => orderStore.updateCartQuantity(item.productId, val || 1)"
                  />
                  <span class="cart-item-subtotal">¥{{ ((item.price || 0) * item.quantity).toFixed(2) }}</span>
                  <el-button
                    size="small"
                    type="danger"
                    :icon="DeleteIcon"
                    circle
                    @click="orderStore.removeFromCart(item.productId)"
                  />
                </div>
              </div>
            </div>

            <el-divider />

            <div class="cart-total">
              <span>合计金额</span>
              <span class="total-amount">¥{{ orderStore.cartTotal.toFixed(2) }}</span>
            </div>

            <el-button
              type="primary"
              size="large"
              round
              class="place-order-btn"
              :loading="placingOrder"
              @click="handlePlaceOrder"
            >
              <el-icon><Check /></el-icon>
              提交订单
            </el-button>
          </template>
        </el-card>
      </el-col>
    </el-row>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { Delete as DeleteIcon } from '@element-plus/icons-vue'
import { useProductStore } from '@/stores/productStore'
import { useOrderStore } from '@/stores/orderStore'
import type { Product } from '@/types/product'

const router = useRouter()
const productStore = useProductStore()
const orderStore = useOrderStore()

const searchQuery = ref('')
const quantities = reactive<Record<number, number>>({})
const placingOrder = ref(false)
const loading = ref(false)

function onImgError(e: Event) {
  const img = e.target as HTMLImageElement
  img.style.display = 'none'
}

const filteredProducts = computed(() => {
  const list = productStore.products
  if (!searchQuery.value) return list
  const q = searchQuery.value.toLowerCase()
  return list.filter(p => p.name.toLowerCase().includes(q))
})

function handleAddToCart(product: Product) {
  const qty = quantities[product.id] || 1
  orderStore.addToCart(product.id, product.name, product.price, qty)
  quantities[product.id] = 1
}

async function handlePlaceOrder() {
  placingOrder.value = true
  try {
    const order = await orderStore.placeOrder()
    if (order) {
      router.push('/orders')
    }
  } catch {
    // 错误已在 store 中处理
  } finally {
    placingOrder.value = false
  }
}

async function loadProducts() {
  loading.value = true
  try {
    await productStore.fetchAll()
    // 初始化每个商品的数量为 1
    productStore.products.forEach(p => {
      quantities[p.id] = 1
    })
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  loadProducts()
})
</script>

<style scoped>
.page-container { max-width: 1400px; margin: 0 auto; }
.page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 20px; }
.page-title { display: flex; align-items: center; gap: 12px; }
.page-title h2 { margin: 0; font-size: 22px; font-weight: 700; color: #1d2129; }

.content-card { border-radius: 12px; border: 1px solid #f0f0f0; }
.card-header { display: flex; justify-content: space-between; align-items: center; }
.card-title { display: flex; align-items: center; gap: 6px; font-weight: 600; font-size: 15px; color: #1d2129; }

.product-list { max-height: 650px; overflow-y: auto; }
.product-item { display: flex; align-items: center; padding: 14px 4px; border-bottom: 1px solid #f5f5f5; gap: 14px; }
.product-item:hover { background: #fafafa; border-radius: 8px; }

.product-img-wrap { width: 72px; height: 72px; flex-shrink: 0; }
.product-img { width: 72px; height: 72px; border-radius: 10px; object-fit: cover; }
.img-placeholder { width: 72px; height: 72px; border-radius: 10px; background: #f5f7fa; display: flex; align-items: center; justify-content: center; color: #c0c4cc; }

.product-info { flex: 1; min-width: 0; }
.product-name { font-weight: 600; font-size: 15px; color: #1d2129; margin-bottom: 6px; }
.product-meta { display: flex; align-items: center; gap: 8px; font-size: 13px; color: #86909c; }
.product-price { color: #f5222d; font-weight: 700; font-size: 15px; }
.meta-sep { color: #ddd; }

.product-actions { display: flex; align-items: center; gap: 10px; flex-shrink: 0; }

.cart-card { position: sticky; top: 24px; }
.cart-list { max-height: 500px; overflow-y: auto; }
.cart-item { padding: 10px 0; border-bottom: 1px solid #f5f5f5; }
.cart-item-info { margin-bottom: 8px; }
.cart-item-name { font-weight: 500; font-size: 14px; color: #1d2129; }
.cart-item-price { font-size: 12px; color: #86909c; margin-top: 2px; }
.cart-item-actions { display: flex; align-items: center; gap: 10px; }
.cart-item-subtotal { font-weight: 700; font-size: 14px; color: #f5222d; min-width: 80px; text-align: right; }

.cart-total { display: flex; justify-content: space-between; align-items: center; font-size: 16px; font-weight: 500; color: #1d2129; padding: 4px 0; }
.total-amount { font-size: 24px; font-weight: 700; color: #f5222d; }

.place-order-btn { width: 100%; margin-top: 16px; height: 48px; font-size: 16px; font-weight: 600; letter-spacing: 2px; }
</style>
