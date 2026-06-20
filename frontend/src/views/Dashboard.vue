<template>
  <div class="page-container">
    <div class="page-header">
      <div class="page-title">
        <el-icon :size="24" class="title-icon"><DataAnalysis /></el-icon>
        <h2>数据仪表盘</h2>
      </div>
      <el-button size="large" round @click="loadStats" :loading="loading">
        <el-icon><Refresh /></el-icon>
        刷新数据
      </el-button>
    </div>

    <!-- 统计卡片 -->
    <el-row :gutter="16" class="stat-row">
      <el-col :span="8">
        <el-card class="stat-card today-card" shadow="never">
          <div class="stat-label">今日营收</div>
          <div class="stat-value">¥{{ formatMoney(stats?.todayRevenue) }}</div>
          <div class="stat-sub">
            <span>订单 {{ stats?.todayOrders ?? 0 }} 笔</span>
            <el-divider direction="vertical" />
            <span>出货 {{ stats?.todayQuantity ?? 0 }} 件</span>
          </div>
        </el-card>
      </el-col>
      <el-col :span="8">
        <el-card class="stat-card week-card" shadow="never">
          <div class="stat-label">本周营收</div>
          <div class="stat-value">¥{{ formatMoney(stats?.weekRevenue) }}</div>
          <div class="stat-sub">
            <span>订单 {{ stats?.weekOrders ?? 0 }} 笔</span>
            <el-divider direction="vertical" />
            <span>出货 {{ stats?.weekQuantity ?? 0 }} 件</span>
          </div>
        </el-card>
      </el-col>
      <el-col :span="8">
        <el-card class="stat-card month-card" shadow="never">
          <div class="stat-label">本月营收</div>
          <div class="stat-value">¥{{ formatMoney(stats?.monthRevenue) }}</div>
          <div class="stat-sub">
            <span>订单 {{ stats?.monthOrders ?? 0 }} 笔</span>
            <el-divider direction="vertical" />
            <span>出货 {{ stats?.monthQuantity ?? 0 }} 件</span>
          </div>
        </el-card>
      </el-col>
    </el-row>

    <!-- 图表区 -->
    <el-row :gutter="16" style="margin-top: 16px">
      <!-- 近7日营收趋势 -->
      <el-col :span="12">
        <el-card class="chart-card" shadow="never">
          <template #header>
            <span class="chart-title">近7日营收趋势</span>
          </template>
          <v-chart :option="revenueChartOption" style="height: 320px" autoresize />
        </el-card>
      </el-col>

      <!-- 近7日订单量 -->
      <el-col :span="12">
        <el-card class="chart-card" shadow="never">
          <template #header>
            <span class="chart-title">近7日订单量</span>
          </template>
          <v-chart :option="orderChartOption" style="height: 320px" autoresize />
        </el-card>
      </el-col>
    </el-row>

    <!-- 商品销量排行 -->
    <el-row :gutter="16" style="margin-top: 16px">
      <el-col :span="24">
        <el-card class="chart-card" shadow="never">
          <template #header>
            <span class="chart-title">商品销量排行 Top5</span>
          </template>
          <v-chart :option="topProductsOption" style="height: 300px" autoresize />
        </el-card>
      </el-col>
    </el-row>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { use } from 'echarts/core'
import { CanvasRenderer } from 'echarts/renderers'
import { LineChart, BarChart } from 'echarts/charts'
import {
  TitleComponent,
  TooltipComponent,
  LegendComponent,
  GridComponent
} from 'echarts/components'
import VChart from 'vue-echarts'
import { dashboardApi } from '@/api/dashboardApi'
import type { DashboardStats } from '@/types/dashboard'

// 注册 ECharts 组件
use([CanvasRenderer, LineChart, BarChart, TitleComponent, TooltipComponent, LegendComponent, GridComponent])

const stats = ref<DashboardStats | null>(null)
const loading = ref(false)

function formatMoney(val?: number): string {
  return (val ?? 0).toLocaleString('zh-CN', { minimumFractionDigits: 2, maximumFractionDigits: 2 })
}

// 营收折线图配置
const revenueChartOption = computed(() => ({
  tooltip: {
    trigger: 'axis',
    formatter: (params: any) => `${params[0].axisValue}<br/>营收：<b>¥${params[0].value.toLocaleString()}</b>`
  },
  grid: { left: '3%', right: '8%', bottom: '3%', top: '8%', containLabel: true },
  xAxis: {
    type: 'category',
    data: stats.value?.dailyRevenues?.map(d => d.date) ?? [],
    axisLine: { lineStyle: { color: '#ddd' } }
  },
  yAxis: {
    type: 'value',
    axisLabel: { formatter: (v: number) => '¥' + (v / 1000).toFixed(0) + 'k' },
    splitLine: { lineStyle: { color: '#f0f0f0' } }
  },
  series: [{
    name: '营收',
    type: 'line',
    data: stats.value?.dailyRevenues?.map(d => d.revenue) ?? [],
    smooth: true,
    symbol: 'circle',
    symbolSize: 8,
    lineStyle: { color: '#1890ff', width: 3 },
    itemStyle: { color: '#1890ff' },
    areaStyle: {
      color: {
        type: 'linear',
        x: 0, y: 0, x2: 0, y2: 1,
        colorStops: [
          { offset: 0, color: 'rgba(24,144,255,0.25)' },
          { offset: 1, color: 'rgba(24,144,255,0.02)' }
        ]
      }
    }
  }]
}))

// 订单柱状图配置
const orderChartOption = computed(() => ({
  tooltip: {
    trigger: 'axis',
    formatter: (params: any) => `${params[0].axisValue}<br/>订单数：<b>${params[0].value} 笔</b>`
  },
  grid: { left: '3%', right: '8%', bottom: '3%', top: '8%', containLabel: true },
  xAxis: {
    type: 'category',
    data: stats.value?.dailyOrders?.map(d => d.date) ?? [],
    axisLine: { lineStyle: { color: '#ddd' } }
  },
  yAxis: {
    type: 'value',
    minInterval: 1,
    splitLine: { lineStyle: { color: '#f0f0f0' } }
  },
  series: [{
    name: '订单数',
    type: 'bar',
    data: stats.value?.dailyOrders?.map(d => d.count) ?? [],
    barWidth: '50%',
    itemStyle: {
      color: {
        type: 'linear',
        x: 0, y: 0, x2: 0, y2: 1,
        colorStops: [
          { offset: 0, color: '#52c41a' },
          { offset: 1, color: '#237804' }
        ]
      },
      borderRadius: [6, 6, 0, 0]
    }
  }]
}))

// 商品销量排行横向柱状图
const topProductsOption = computed(() => {
  const data = stats.value?.topProducts ?? []
  return {
    tooltip: {
      trigger: 'axis',
      formatter: (params: any) => {
        const d = data[params[0].dataIndex]
        return `<b>${d.productName}</b><br/>销量：${d.totalQuantity} 件<br/>营收：¥${d.totalRevenue.toFixed(2)}`
      }
    },
    grid: { left: '3%', right: '15%', bottom: '3%', top: '3%', containLabel: true },
    xAxis: {
      type: 'value',
      name: '销量（件）',
      splitLine: { lineStyle: { color: '#f0f0f0' } }
    },
    yAxis: {
      type: 'category',
      data: data.map(d => d.productName).reverse(),
      axisLine: { lineStyle: { color: '#ddd' } },
      axisLabel: { width: 120, overflow: 'truncate' }
    },
    series: [{
      name: '销量',
      type: 'bar',
      data: data.map(d => d.totalQuantity).reverse(),
      barWidth: '55%',
      itemStyle: {
        color: {
          type: 'linear',
          x: 0, y: 0, x2: 1, y2: 0,
          colorStops: [
            { offset: 0, color: '#1890ff' },
            { offset: 1, color: '#36cfc9' }
          ]
        },
        borderRadius: [0, 6, 6, 0]
      },
      label: {
        show: true,
        position: 'right',
        formatter: '{c} 件',
        color: '#666'
      }
    }]
  }
})

async function loadStats() {
  loading.value = true
  try {
    const { data } = await dashboardApi.getStats()
    stats.value = data
  } catch {
    // 错误由拦截器处理
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  loadStats()
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

/* 统计卡片 */
.stat-row {
  margin-bottom: 0;
}

.stat-card {
  border-radius: 12px;
  border: 1px solid #f0f0f0;
  position: relative;
  overflow: hidden;
}

.stat-card::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  width: 4px;
  height: 100%;
}

.today-card::before { background: linear-gradient(180deg, #1890ff, #36cfc9); }
.week-card::before { background: linear-gradient(180deg, #722ed1, #b37feb); }
.month-card::before { background: linear-gradient(180deg, #fa8c16, #ffc069); }

.stat-label {
  font-size: 14px;
  color: #86909c;
  margin-bottom: 8px;
}

.stat-value {
  font-size: 30px;
  font-weight: 800;
  color: #1d2129;
  letter-spacing: -1px;
  margin-bottom: 8px;
}

.stat-sub {
  font-size: 13px;
  color: #86909c;
  display: flex;
  align-items: center;
}

/* 图表卡片 */
.chart-card {
  border-radius: 12px;
  border: 1px solid #f0f0f0;
}

.chart-title {
  font-weight: 600;
  font-size: 15px;
  color: #1d2129;
}
</style>
