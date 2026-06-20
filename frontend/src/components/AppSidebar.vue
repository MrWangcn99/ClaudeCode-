<template>
  <el-aside width="220px" class="app-sidebar">
    <div class="sidebar-logo">
      <div class="logo-icon">
        <el-icon :size="22"><ShoppingBag /></el-icon>
      </div>
      <span class="logo-text">电商管理平台</span>
    </div>

    <el-menu
      :default-active="route.path"
      router
      background-color="#001529"
      text-color="#ffffffb3"
      active-text-color="#fff"
      class="sidebar-menu"
    >
      <el-menu-item index="/dashboard">
        <el-icon><DataAnalysis /></el-icon>
        <span>数据仪表盘</span>
      </el-menu-item>
      <el-menu-item index="/products">
        <el-icon><Goods /></el-icon>
        <span>商品管理</span>
      </el-menu-item>
      <el-menu-item index="/inventory">
        <el-icon><Box /></el-icon>
        <span>库存管理</span>
      </el-menu-item>
      <el-menu-item index="/orders">
        <el-icon><Document /></el-icon>
        <span>订单管理</span>
      </el-menu-item>
      <el-menu-item index="/orders/new">
        <el-icon><ShoppingCart /></el-icon>
        <span>创建订单</span>
      </el-menu-item>
    </el-menu>
  </el-aside>
</template>

<script setup lang="ts">
import { useRoute } from 'vue-router'
const route = useRoute()
</script>

<style scoped>
.app-sidebar {
  background: #001529 !important;
  overflow: hidden;
}

.sidebar-logo {
  height: 64px;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 10px;
  background: #002140;
  border-bottom: 1px solid rgba(255, 255, 255, 0.06);
}

.logo-icon {
  width: 36px;
  height: 36px;
  background: linear-gradient(135deg, #1890ff, #36cfc9);
  border-radius: 8px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #fff;
  flex-shrink: 0;
}

.logo-text {
  color: #fff;
  font-size: 16px;
  font-weight: 600;
  letter-spacing: 1px;
  white-space: nowrap;
}

.sidebar-menu {
  border-right: none !important;
}

.sidebar-menu :deep(.el-menu-item) {
  height: 50px;
  line-height: 50px;
  margin: 4px 8px;
  border-radius: 8px;
  transition: all 0.3s;
  color: #ffffffb3 !important;
}

.sidebar-menu :deep(.el-menu-item:hover) {
  background-color: rgba(255, 255, 255, 0.06) !important;
  color: #fff !important;
}

.sidebar-menu :deep(.el-menu-item.is-active) {
  background: linear-gradient(135deg, #1890ff, #36cfc9) !important;
  color: #fff !important;
  box-shadow: 0 4px 12px rgba(24, 144, 255, 0.35);
}
</style>
