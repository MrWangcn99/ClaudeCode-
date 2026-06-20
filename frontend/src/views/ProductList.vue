<template>
  <div class="page-container">
    <!-- 页面标题栏 -->
    <div class="page-header">
      <div class="page-title">
        <el-icon :size="24" class="title-icon"><Goods /></el-icon>
        <h2>商品管理</h2>
        <el-tag type="info" size="small" round>共 {{ store.products.length }} 件商品</el-tag>
      </div>
      <el-button type="primary" size="large" round @click="$router.push('/products/new')">
        <el-icon><Plus /></el-icon>
        添加商品
      </el-button>
    </div>

    <!-- 搜索栏 -->
    <div class="search-bar">
      <el-input
        v-model="searchQuery"
        placeholder="搜索商品名称..."
        clearable
        size="large"
        :prefix-icon="Search"
        class="search-input"
      />
    </div>

    <!-- 商品表格 -->
    <el-card class="content-card" shadow="never">
      <el-table
        :data="filteredProducts"
        v-loading="store.loading"
        stripe
        style="width: 100%"
        :header-cell-style="{ background: '#fafafa', color: '#303133', fontWeight: 600 }"
      >
        <el-table-column prop="id" label="ID" width="60" align="center" />
        <el-table-column label="商品图片" width="90" align="center">
          <template #default="{ row }">
            <el-image
              v-if="row.imageUrl"
              :src="row.imageUrl"
              style="width: 50px; height: 50px; border-radius: 8px"
              fit="cover"
            >
              <template #error>
                <div class="img-placeholder">
                  <el-icon><Picture /></el-icon>
                </div>
              </template>
            </el-image>
            <div v-else class="img-placeholder">
              <el-icon><Picture /></el-icon>
            </div>
          </template>
        </el-table-column>
        <el-table-column prop="name" label="商品名称" min-width="200">
          <template #default="{ row }">
            <span class="product-name">{{ row.name }}</span>
          </template>
        </el-table-column>
        <el-table-column prop="price" label="价格" width="130" align="center">
          <template #default="{ row }">
            <span class="price-text">¥{{ row.price.toFixed(2) }}</span>
          </template>
        </el-table-column>
        <el-table-column prop="stock" label="库存" width="100" align="center">
          <template #default="{ row }">
            <el-tag
              :type="row.stock > 10 ? 'success' : row.stock > 3 ? 'warning' : 'danger'"
              effect="plain"
              round
            >
              {{ row.stock }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="description" label="商品描述" min-width="220" show-overflow-tooltip />
        <el-table-column prop="createdAt" label="创建时间" width="170" align="center">
          <template #default="{ row }">
            {{ new Date(row.createdAt).toLocaleString('zh-CN') }}
          </template>
        </el-table-column>
        <el-table-column label="操作" width="180" fixed="right" align="center">
          <template #default="{ row }">
            <el-button size="small" type="primary" link @click="$router.push(`/products/${row.id}/edit`)">
              <el-icon><Edit /></el-icon>
              编辑
            </el-button>
            <el-divider direction="vertical" />
            <el-popconfirm
              title="确定要删除该商品吗？"
              confirm-button-text="确定"
              cancel-button-text="取消"
              @confirm="handleDelete(row.id)"
            >
              <template #reference>
                <el-button size="small" type="danger" link>
                  <el-icon><Delete /></el-icon>
                  删除
                </el-button>
              </template>
            </el-popconfirm>
          </template>
        </el-table-column>
      </el-table>
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { Search } from '@element-plus/icons-vue'
import { useProductStore } from '@/stores/productStore'
import { ElMessage } from 'element-plus'

const store = useProductStore()
const searchQuery = ref('')

const filteredProducts = computed(() => {
  if (!searchQuery.value) return store.products
  const q = searchQuery.value.toLowerCase()
  return store.products.filter(p => p.name.toLowerCase().includes(q))
})

async function handleDelete(id: number) {
  try {
    await store.remove(id)
    ElMessage.success('商品已删除')
  } catch {
    ElMessage.error('删除失败，该商品可能存在关联订单')
  }
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
  color: #1890ff;
}

.search-bar {
  margin-bottom: 16px;
}

.search-input {
  max-width: 360px;
}

.content-card {
  border-radius: 12px;
  overflow: hidden;
  border: 1px solid #f0f0f0;
}

.product-name {
  font-weight: 500;
  color: #1d2129;
}

.price-text {
  color: #f5222d;
  font-weight: 700;
  font-size: 15px;
}

.img-placeholder {
  width: 50px;
  height: 50px;
  border-radius: 8px;
  background: #f5f7fa;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #c0c4cc;
}
</style>
