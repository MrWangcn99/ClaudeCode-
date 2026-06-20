<template>
  <div class="page-container">
    <!-- 页面标题栏 -->
    <div class="page-header">
      <div class="page-title">
        <el-icon :size="24" class="title-icon"><Document /></el-icon>
        <h2>订单管理</h2>
        <el-tag type="info" size="small" round>共 {{ store.orders.length }} 条订单</el-tag>
      </div>
      <el-button type="primary" size="large" round @click="$router.push('/orders/new')">
        <el-icon><Plus /></el-icon>
        新建订单
      </el-button>
    </div>

    <!-- 筛选 + 表格 -->
    <el-card class="content-card" shadow="never">
      <!-- 状态筛选 -->
      <div class="filter-bar">
        <span class="filter-label">订单状态：</span>
        <el-radio-group v-model="filterStatus" @change="handleFilterChange" size="default">
          <el-radio-button value="">全部</el-radio-button>
          <el-radio-button value="Pending">待处理</el-radio-button>
          <el-radio-button value="Processing">处理中</el-radio-button>
          <el-radio-button value="Completed">已完成</el-radio-button>
          <el-radio-button value="Cancelled">已取消</el-radio-button>
        </el-radio-group>
      </div>

      <!-- 订单表格 -->
      <el-table
        :data="store.orders"
        v-loading="store.loading"
        stripe
        style="width: 100%"
        :header-cell-style="{ background: '#fafafa', color: '#303133', fontWeight: 600 }"
        @row-click="handleRowClick"
        highlight-current-row
      >
        <el-table-column type="expand">
          <template #default="{ row }">
            <div class="expand-content">
              <h4>订单明细</h4>
              <el-table :data="row.items || []" size="small" style="width: 100%">
                <el-table-column prop="productName" label="商品名称" min-width="220" />
                <el-table-column prop="unitPrice" label="单价" width="120" align="center">
                  <template #default="{ row: item }">¥{{ item.unitPrice.toFixed(2) }}</template>
                </el-table-column>
                <el-table-column prop="quantity" label="数量" width="80" align="center" />
                <el-table-column prop="subTotal" label="小计" width="120" align="center">
                  <template #default="{ row: item }">
                    <span class="subtotal-text">¥{{ item.subTotal.toFixed(2) }}</span>
                  </template>
                </el-table-column>
              </el-table>
              <div v-if="!row.items || row.items.length === 0" class="empty-items">
                <el-button size="small" type="primary" text @click.stop="loadOrderItems(row.id)">
                  点击加载订单明细
                </el-button>
              </div>
            </div>
          </template>
        </el-table-column>

        <el-table-column prop="orderNumber" label="订单编号" width="210">
          <template #default="{ row }">
            <span class="order-number">{{ row.orderNumber }}</span>
          </template>
        </el-table-column>

        <el-table-column prop="status" label="状态" width="110" align="center">
          <template #default="{ row }">
            <OrderStatusTag :status="row.status" />
          </template>
        </el-table-column>

        <el-table-column prop="totalAmount" label="订单金额" width="140" align="center">
          <template #default="{ row }">
            <span class="order-amount">¥{{ row.totalAmount.toFixed(2) }}</span>
          </template>
        </el-table-column>

        <el-table-column prop="createdAt" label="创建时间" width="180" align="center">
          <template #default="{ row }">
            {{ new Date(row.createdAt).toLocaleString('zh-CN') }}
          </template>
        </el-table-column>

        <el-table-column label="操作" width="100" fixed="right" align="center">
          <template #default="{ row }">
            <el-button size="small" type="primary" link @click.stop="viewOrderDetail(row.id)">
              <el-icon><View /></el-icon>
              详情
            </el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>

    <!-- 订单详情弹窗 -->
    <el-dialog v-model="detailVisible" title="订单详情" width="680px" destroy-on-close>
      <template v-if="detailOrder">
        <el-descriptions :column="2" border size="large">
          <el-descriptions-item label="订单编号">
            <span class="detail-order-num">{{ detailOrder.orderNumber }}</span>
          </el-descriptions-item>
          <el-descriptions-item label="订单状态">
            <OrderStatusTag :status="detailOrder.status" />
          </el-descriptions-item>
          <el-descriptions-item label="订单金额">
            <span class="detail-amount">¥{{ detailOrder.totalAmount.toFixed(2) }}</span>
          </el-descriptions-item>
          <el-descriptions-item label="创建时间">
            {{ new Date(detailOrder.createdAt).toLocaleString('zh-CN') }}
          </el-descriptions-item>
        </el-descriptions>

        <h4 class="detail-items-title">商品明细</h4>
        <el-table :data="detailOrder.items || []" size="small" style="width: 100%">
          <el-table-column prop="productName" label="商品名称" min-width="200" />
          <el-table-column prop="unitPrice" label="单价" width="100" align="center">
            <template #default="{ row }">¥{{ row.unitPrice.toFixed(2) }}</template>
          </el-table-column>
          <el-table-column prop="quantity" label="数量" width="60" align="center" />
          <el-table-column prop="subTotal" label="小计" width="110" align="center">
            <template #default="{ row }">
              <span style="font-weight: 700; color: #f5222d">¥{{ row.subTotal.toFixed(2) }}</span>
            </template>
          </el-table-column>
        </el-table>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useOrderStore } from '@/stores/orderStore'
import OrderStatusTag from '@/components/OrderStatusTag.vue'
import type { Order } from '@/types/order'

const store = useOrderStore()
const filterStatus = ref('')
const detailVisible = ref(false)
const detailOrder = ref<Order | null>(null)

function handleFilterChange() {
  store.fetchAll(filterStatus.value || undefined)
}

async function handleRowClick(row: Order) {
  if (!row.items || row.items.length === 0) {
    await loadOrderItems(row.id)
  }
}

async function loadOrderItems(orderId: number) {
  try {
    const order = await store.fetchById(orderId)
    const index = store.orders.findIndex(o => o.id === orderId)
    if (index !== -1) {
      store.orders[index] = { ...store.orders[index], items: order.items }
    }
  } catch {
    // 错误已在拦截器中处理
  }
}

async function viewOrderDetail(orderId: number) {
  try {
    detailOrder.value = await store.fetchById(orderId)
    detailVisible.value = true
  } catch {
    // 错误已在拦截器中处理
  }
}

onMounted(() => {
  store.fetchAll()
})
</script>

<style scoped>
.page-container {
  max-width: 1400px;
  margin: 0 auto;
}

.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
}

.page-title {
  display: flex;
  align-items: center;
  gap: 12px;
}

.page-title h2 {
  margin: 0;
  font-size: 22px;
  font-weight: 700;
  color: #1d2129;
}

.title-icon {
  color: #722ed1;
}

.content-card {
  border-radius: 12px;
  overflow: hidden;
  border: 1px solid #f0f0f0;
}

.filter-bar {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-bottom: 20px;
  padding: 4px 0;
}

.filter-label {
  font-weight: 500;
  color: #303133;
  white-space: nowrap;
}

.order-number {
  font-weight: 600;
  color: #1890ff;
  font-family: monospace;
  font-size: 13px;
}

.order-amount {
  font-weight: 700;
  font-size: 15px;
  color: #f5222d;
}

.expand-content {
  padding: 12px 24px;
}

.expand-content h4 {
  margin: 0 0 12px 0;
  font-size: 14px;
  color: #303133;
}

.subtotal-text {
  font-weight: 700;
  color: #f5222d;
}

.empty-items {
  text-align: center;
  padding: 16px;
}

.detail-order-num {
  font-weight: 600;
  color: #1890ff;
  font-family: monospace;
}

.detail-amount {
  font-weight: 700;
  color: #f5222d;
  font-size: 16px;
}

.detail-items-title {
  margin: 20px 0 10px 0;
  font-size: 15px;
  color: #303133;
}
</style>
