<script setup lang="ts">
import { ref, onMounted } from 'vue'
import Card from '@/components/ui/Card.vue'
import CardContent from '@/components/ui/CardContent.vue'
import DataTable from '@/components/ui/DataTable.vue'
import Pagination from '@/components/ui/Pagination.vue'
import Input from '@/components/ui/Input.vue'
import Button from '@/components/ui/Button.vue'
import Badge from '@/components/ui/Badge.vue'
import { logApi } from '@/api'
import type { LogEvent } from '@/types'
import { Search, RefreshCw } from 'lucide-vue-next'

const logs = ref<LogEvent[]>([])
const loading = ref(true)
const currentPage = ref(1)
const totalPages = ref(1)
const totalItems = ref(0)
const pageSize = ref(20)
const searchQuery = ref('')

const columns = [
  { key: 'timestamp', label: '时间', width: '180px' },
  { key: 'level', label: '级别', width: '100px' },
  { key: 'message', label: '消息' },
]

async function fetchLogs() {
  loading.value = true
  try {
    const response = await logApi.getLogs({
      pageIndex: currentPage.value - 1,
      pageSize: pageSize.value,
      filterQuery: searchQuery.value || undefined
    })
    
    if (response.data.code === 0) {
      logs.value = response.data.data || []
      totalItems.value = response.data.recordCount
      totalPages.value = Math.ceil(response.data.recordCount / pageSize.value)
    }
  } catch (error) {
    console.error('Failed to fetch logs:', error)
  } finally {
    loading.value = false
  }
}

function handleSearch() {
  currentPage.value = 1
  fetchLogs()
}

function getLevelVariant(level: string) {
  switch (level.toLowerCase()) {
    case 'error':
    case 'fatal':
      return 'destructive'
    case 'warning':
      return 'warning'
    case 'information':
    case 'info':
      return 'info'
    default:
      return 'secondary'
  }
}

onMounted(() => {
  fetchLogs()
})
</script>

<template>
  <div class="space-y-6">
    <!-- 页面标题 -->
    <div class="flex items-center justify-between">
      <div>
        <h1 class="text-2xl font-bold">系统日志</h1>
        <p class="text-muted-foreground">查看系统运行日志</p>
      </div>
      <Button variant="outline" @click="fetchLogs">
        <RefreshCw class="h-4 w-4 mr-2" />
        刷新
      </Button>
    </div>
    
    <!-- 搜索栏 -->
    <Card>
      <CardContent class="p-4">
        <div class="flex gap-4">
          <div class="relative flex-1">
            <Search class="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
            <Input
              v-model="searchQuery"
              placeholder="搜索日志内容..."
              class="pl-10"
              @keyup.enter="handleSearch"
            />
          </div>
          <Button @click="handleSearch">搜索</Button>
        </div>
      </CardContent>
    </Card>
    
    <!-- 日志表格 -->
    <Card>
      <CardContent class="p-0">
        <DataTable
          :columns="columns"
          :data="logs"
          :loading="loading"
        >
          <template #cell-timestamp="{ value }">
            <span class="text-xs font-mono text-muted-foreground">
              {{ value }}
            </span>
          </template>
          
          <template #cell-level="{ value }">
            <Badge :variant="getLevelVariant(value as string)">
              {{ value }}
            </Badge>
          </template>
          
          <template #cell-message="{ value, row }">
            <div class="max-w-lg">
              <p class="truncate text-sm">{{ value }}</p>
              <p 
                v-if="(row as unknown as LogEvent).exception"
                class="text-xs text-destructive mt-1 truncate"
              >
                {{ (row as unknown as LogEvent).exception }}
              </p>
            </div>
          </template>
          
          <template #actions>
            <!-- 预留 -->
          </template>
        </DataTable>
        
        <Pagination
          v-if="totalPages > 1"
          v-model:current-page="currentPage"
          :total-pages="totalPages"
          :total-items="totalItems"
          :page-size="pageSize"
          @update:current-page="fetchLogs"
        />
      </CardContent>
    </Card>
  </div>
</template>
