<template>
  <el-container style="height: 100vh">
    <AppSidebar />

    <el-container>
      <!-- 顶部导航栏 -->
      <el-header class="app-header">
        <div class="header-left">
          <el-breadcrumb separator="/">
            <el-breadcrumb-item :to="{ path: '/' }">首页</el-breadcrumb-item>
            <el-breadcrumb-item v-if="route.matched[1]">
              {{ getPageTitle(route.path) }}
            </el-breadcrumb-item>
          </el-breadcrumb>
        </div>

        <div class="header-right">
          <el-badge :value="orderStore.cartItemCount" :hidden="orderStore.cartItemCount === 0" class="cart-badge">
            <el-button type="primary" size="default" round @click="$router.push('/orders/new')">
              <el-icon><ShoppingCart /></el-icon>
              购物车
            </el-button>
          </el-badge>

          <el-divider direction="vertical" />

          <el-dropdown trigger="click">
            <span class="user-info">
              <el-avatar :size="32" class="user-avatar">
                <el-icon :size="18"><UserFilled /></el-icon>
              </el-avatar>
              <span class="user-name">{{ authStore.username || '管理员' }}</span>
              <el-icon><ArrowDown /></el-icon>
            </span>
            <template #dropdown>
              <el-dropdown-menu>
                <el-dropdown-item disabled>
                  <el-icon><User /></el-icon>
                  {{ authStore.username || '管理员' }}
                </el-dropdown-item>
                <el-dropdown-item divided @click="handleLogout">
                  <el-icon><SwitchButton /></el-icon>
                  退出登录
                </el-dropdown-item>
              </el-dropdown-menu>
            </template>
          </el-dropdown>
        </div>
      </el-header>

      <!-- 主内容区 -->
      <el-main class="app-main">
        <slot />
      </el-main>
    </el-container>
  </el-container>
</template>

<script setup lang="ts">
import { useRoute, useRouter } from 'vue-router'
import { useOrderStore } from '@/stores/orderStore'
import { useAuthStore } from '@/stores/authStore'
import { ElMessageBox } from 'element-plus'
import AppSidebar from './AppSidebar.vue'

const route = useRoute()
const router = useRouter()
const orderStore = useOrderStore()
const authStore = useAuthStore()

function getPageTitle(path: string): string {
  const map: Record<string, string> = {
    '/products': '商品管理',
    '/products/new': '添加商品',
    '/inventory': '库存管理',
    '/orders': '订单管理',
    '/orders/new': '创建订单'
  }
  if (path.startsWith('/products/') && path.endsWith('/edit')) return '编辑商品'
  if (path === '/dashboard') return '数据仪表盘'
  return map[path] || ''
}

async function handleLogout() {
  try {
    await ElMessageBox.confirm('确定要退出登录吗？', '提示', {
      confirmButtonText: '确定',
      cancelButtonText: '取消',
      type: 'warning'
    })
    authStore.logout()
    router.push('/login')
  } catch {
    // 用户取消
  }
}
</script>

<style scoped>
.app-header {
  height: 64px;
  background: #fff;
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 24px;
  box-shadow: 0 1px 4px rgba(0, 0, 0, 0.06);
  z-index: 10;
}

.header-left {
  display: flex;
  align-items: center;
}

.header-right {
  display: flex;
  align-items: center;
  gap: 12px;
}

.cart-badge {
  margin-right: 4px;
}

.user-info {
  display: flex;
  align-items: center;
  gap: 8px;
  cursor: pointer;
  padding: 4px 8px;
  border-radius: 8px;
  transition: background 0.3s;
}

.user-info:hover {
  background: #f5f7fa;
}

.user-avatar {
  background: linear-gradient(135deg, #1890ff, #36cfc9);
}

.user-name {
  font-size: 14px;
  font-weight: 500;
  color: #303133;
}

.app-main {
  background: #f0f2f5;
  padding: 24px;
  overflow-y: auto;
}
</style>
