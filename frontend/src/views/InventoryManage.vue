<template>
  <div class="page-container">
    <!-- 页面标题栏 -->
    <div class="page-header">
      <div class="page-title">
        <el-icon :size="24" class="title-icon"><Box /></el-icon>
        <h2>库存管理</h2>
        <el-tag type="info" size="small" round>共 {{ store.items.length }} 件商品</el-tag>
      </div>
      <el-button type="primary" size="large" round @click="store.fetchAll()" :loading="store.loading">
        <el-icon><Refresh /></el-icon>
        刷新数据
      </el-button>
    </div>

    <!-- 库存表格 -->
    <el-card class="content-card" shadow="never">
      <el-table
        :data="store.items"
        v-loading="store.loading"
        stripe
        style="width: 100%"
        :header-cell-style="{ background: '#fafafa', color: '#303133', fontWeight: 600 }"
      >
        <el-table-column prop="id" label="ID" width="60" align="center" />
        <el-table-column prop="name" label="商品名称" min-width="220">
          <template #default="{ row }">
            <div class="product-cell">
              <el-image
                v-if="row.imageUrl"
                :src="row.imageUrl"
                style="width: 40px; height: 40px; border-radius: 6px"
                fit="cover"
              >
                <template #error><div class="mini-placeholder"><el-icon><Picture /></el-icon></div></template>
              </el-image>
              <span class="product-name">{{ row.name }}</span>
            </div>
          </template>
        </el-table-column>
        <el-table-column prop="price" label="单价" width="120" align="center">
          <template #default="{ row }">
            <span class="price-text">¥{{ row.price.toFixed(2) }}</span>
          </template>
        </el-table-column>
        <el-table-column label="当前库存" width="120" align="center">
          <template #default="{ row }">
            <el-tag
              :type="row.stock > 10 ? 'success' : row.stock > 3 ? 'warning' : 'danger'"
              effect="plain"
              round
              size="large"
            >
              {{ row.stock }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="调整库存" min-width="320" align="center">
          <template #default="{ row }">
            <div class="adjust-cell">
              <el-input-number
                v-model="editStocks[row.id]"
                :min="0"
                :step="1"
                size="large"
                style="width: 160px"
                controls-position="right"
              />
              <el-button
                type="primary"
                :disabled="editStocks[row.id] === row.stock"
                :loading="savingIds.has(row.id)"
                round
                @click="handleUpdateStock(row.id)"
              >
                <el-icon><Check /></el-icon>
                更新库存
              </el-button>
            </div>
          </template>
        </el-table-column>
        <el-table-column prop="updatedAt" label="最后更新" width="170" align="center">
          <template #default="{ row }">
            {{ new Date(row.updatedAt).toLocaleString('zh-CN') }}
          </template>
        </el-table-column>
      </el-table>
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { useInventoryStore } from '@/stores/inventoryStore'
import { ElMessage } from 'element-plus'

const store = useInventoryStore()
const editStocks = reactive<Record<number, number>>({})
const savingIds = ref(new Set<number>())

onMounted(async () => {
  await store.fetchAll()
  store.items.forEach(item => {
    editStocks[item.id] = item.stock
  })
})

async function handleUpdateStock(productId: number) {
  savingIds.value.add(productId)
  try {
    const updated = await store.updateStock(productId, editStocks[productId])
    ElMessage.success(`「${updated.name}」库存已更新为 ${updated.stock}`)
  } catch {
    ElMessage.error('库存更新失败')
    const item = store.items.find(i => i.id === productId)
    if (item) editStocks[productId] = item.stock
  } finally {
    savingIds.value.delete(productId)
  }
}
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
  color: #52c41a;
}

.content-card {
  border-radius: 12px;
  overflow: hidden;
  border: 1px solid #f0f0f0;
}

.product-cell {
  display: flex;
  align-items: center;
  gap: 10px;
}

.product-name {
  font-weight: 500;
  color: #1d2129;
}

.price-text {
  color: #f5222d;
  font-weight: 600;
}

.adjust-cell {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 12px;
}

.mini-placeholder {
  width: 40px;
  height: 40px;
  border-radius: 6px;
  background: #f5f7fa;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #c0c4cc;
}
</style>
