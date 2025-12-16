<script setup lang="ts">
import { ref, onMounted } from 'vue'
import Card from '@/components/ui/Card.vue'
import CardHeader from '@/components/ui/CardHeader.vue'
import CardTitle from '@/components/ui/CardTitle.vue'
import CardContent from '@/components/ui/CardContent.vue'
import DataTable from '@/components/ui/DataTable.vue'
import Pagination from '@/components/ui/Pagination.vue'
import Input from '@/components/ui/Input.vue'
import Button from '@/components/ui/Button.vue'
import Badge from '@/components/ui/Badge.vue'
import { bookApi } from '@/api'
import type { BorrowDto } from '@/types'
import { Search, BookMarked, History } from 'lucide-vue-next'

const activeTab = ref<'current' | 'history'>('current')
const borrows = ref<BorrowDto[]>([])
const loading = ref(true)
const currentPage = ref(1)
const totalPages = ref(1)
const totalItems = ref(0)
const pageSize = ref(15)
const searchQuery = ref('')
const sortColumn = ref('Title')
const sortOrder = ref<'ASC' | 'DESC'>('ASC')

const currentColumns = [
  { key: 'title', label: '书名', sortable: true },
  { key: 'author', label: '作者', sortable: true },
  { key: 'publisher', label: '出版社' },
  { key: 'borrowDate', label: '借阅日期', sortable: true },
  { key: 'borrowDuration', label: '已借天数' },
]

const historyColumns = [
  { key: 'title', label: '书名', sortable: true },
  { key: 'author', label: '作者', sortable: true },
  { key: 'publisher', label: '出版社' },
  { key: 'borrowDate', label: '借阅日期', sortable: true },
  { key: 'returnDate', label: '归还日期' },
  { key: 'borrowDuration', label: '借阅天数' },
]

async function fetchBorrows() {
  loading.value = true
  try {
    const api = activeTab.value === 'current' 
      ? bookApi.getCurrentBorrows 
      : bookApi.getBorrowHistory
    
    const response = await api({
      pageIndex: currentPage.value - 1,
      pageSize: pageSize.value,
      sortColumn: sortColumn.value,
      sortOrder: sortOrder.value,
      filterQuery: searchQuery.value || undefined
    })
    
    if (response.data.code === 0) {
      borrows.value = response.data.data || []
      totalItems.value = response.data.recordCount
      totalPages.value = Math.ceil(response.data.recordCount / pageSize.value)
    }
  } catch (error) {
    console.error('Failed to fetch borrows:', error)
  } finally {
    loading.value = false
  }
}

function handleSort(column: string) {
  if (sortColumn.value === column) {
    sortOrder.value = sortOrder.value === 'ASC' ? 'DESC' : 'ASC'
  } else {
    sortColumn.value = column
    sortOrder.value = 'ASC'
  }
  fetchBorrows()
}

function handleSearch() {
  currentPage.value = 1
  fetchBorrows()
}

function switchTab(tab: 'current' | 'history') {
  activeTab.value = tab
  currentPage.value = 1
  fetchBorrows()
}

onMounted(() => {
  fetchBorrows()
})
</script>

<template>
  <div class="space-y-6">
    <!-- 页面标题 -->
    <div>
      <h1 class="text-2xl font-bold">借阅管理</h1>
      <p class="text-muted-foreground">管理图书借阅记录</p>
    </div>
    
    <!-- 标签切换 -->
    <div class="flex gap-2">
      <Button
        :variant="activeTab === 'current' ? 'default' : 'outline'"
        @click="switchTab('current')"
      >
        <BookMarked class="h-4 w-4 mr-2" />
        当前借阅
      </Button>
      <Button
        :variant="activeTab === 'history' ? 'default' : 'outline'"
        @click="switchTab('history')"
      >
        <History class="h-4 w-4 mr-2" />
        借阅历史
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
              placeholder="搜索书名、作者或出版社..."
              class="pl-10"
              @keyup.enter="handleSearch"
            />
          </div>
          <Button @click="handleSearch">搜索</Button>
        </div>
      </CardContent>
    </Card>
    
    <!-- 数据表格 -->
    <Card>
      <CardContent class="p-0">
        <DataTable
          :columns="activeTab === 'current' ? currentColumns : historyColumns"
          :data="borrows"
          :loading="loading"
          :sort-column="sortColumn"
          :sort-order="sortOrder"
          @sort="handleSort"
        >
          <template #cell-borrowDuration="{ value }">
            <Badge 
              :variant="Number(value) > 25 ? 'destructive' : Number(value) > 20 ? 'warning' : 'secondary'"
            >
              {{ value }} 天
            </Badge>
          </template>
          
          <template #actions>
            <!-- 预留操作列 -->
          </template>
        </DataTable>
        
        <Pagination
          v-if="totalPages > 1"
          v-model:current-page="currentPage"
          :total-pages="totalPages"
          :total-items="totalItems"
          :page-size="pageSize"
          @update:current-page="fetchBorrows"
        />
      </CardContent>
    </Card>
  </div>
</template>
