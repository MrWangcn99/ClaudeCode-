<template>
  <el-tag :type="tagType" effect="plain" round size="small">
    {{ displayText }}
  </el-tag>
</template>

<script setup lang="ts">
import { computed } from 'vue'

const props = defineProps<{ status: string }>()

const statusMap: Record<string, { text: string; type: 'warning' | 'primary' | 'success' | 'danger' | 'info' }> = {
  Pending: { text: '待处理', type: 'warning' },
  Processing: { text: '处理中', type: 'primary' },
  Completed: { text: '已完成', type: 'success' },
  Cancelled: { text: '已取消', type: 'danger' }
}

const tagType = computed(() => statusMap[props.status]?.type || 'info')
const displayText = computed(() => statusMap[props.status]?.text || props.status)
</script>
