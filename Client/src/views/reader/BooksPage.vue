<script setup lang="ts">
import { ref, onMounted, watch } from 'vue'
import { useRoute } from 'vue-router'
import ReaderLayout from '@/layouts/ReaderLayout.vue'
import BookGrid from '@/components/book/BookGrid.vue'
import SearchBar from '@/components/book/SearchBar.vue'
import Pagination from '@/components/ui/Pagination.vue'
import Button from '@/components/ui/Button.vue'
import { bookApi, userApi } from '@/api'
import { useToastStore } from '@/stores/toast'
import type { Book } from '@/types'
import { SlidersHorizontal } from 'lucide-vue-next'

const route = useRoute()
const toastStore = useToastStore()

const books = ref<Book[]>([])
const loading = ref(true)
const currentPage = ref(1)
const totalPages = ref(1)
const totalItems = ref(0)
const pageSize = ref(12)
const searchQuery = ref((route.query.q as string) || '')
const sortColumn = ref('Title')
const sortOrder = ref<'ASC' | 'DESC'>('ASC')
const showFilters = ref(false)

const sortOptions = [
  { value: 'Title', label: '书名' },
  { value: 'Author', label: '作者' },
  { value: 'PublishedDate', label: '出版日期' },
  { value: 'InboundDate', label: '入库日期' },
]

async function fetchBooks() {
  loading.value = true
  try {
    const response = await bookApi.getBooks({
      pageIndex: currentPage.value - 1,
      pageSize: pageSize.value,
      sortColumn: sortColumn.value,
      sortOrder: sortOrder.value,
      filterQuery: searchQuery.value || undefined
    })
    
    if (response.data.code === 0) {
      books.value = response.data.data || []
      totalItems.value = response.data.recordCount
      totalPages.value = Math.ceil(response.data.recordCount / pageSize.value)
    }
  } catch (error) {
    console.error('Failed to fetch books:', error)
  } finally {
    loading.value = false
  }
}

async function handleBorrow(book: Book) {
  try {
    const response = await userApi.borrowBook(book.id)
    if (response.data.code === 0) {
      toastStore.success(`《${book.title}》借阅成功！`)
      fetchBooks() // 刷新库存
    } else {
      toastStore.error(response.data.message)
    }
  } catch {
    toastStore.error('借阅失败，请稍后再试')
  }
}

function handleSearch(query: string) {
  searchQuery.value = query
  currentPage.value = 1
  fetchBooks()
}

function handleSort(column: string) {
  if (sortColumn.value === column) {
    sortOrder.value = sortOrder.value === 'ASC' ? 'DESC' : 'ASC'
  } else {
    sortColumn.value = column
    sortOrder.value = 'ASC'
  }
  fetchBooks()
}

watch(currentPage, () => {
  fetchBooks()
})

onMounted(() => {
  fetchBooks()
})
</script>

<template>
  <ReaderLayout>
    <div class="container px-4 py-8">
      <!-- 页面标题 -->
      <div class="mb-8">
        <h1 class="text-3xl font-bold mb-2">图书搜索</h1>
        <p class="text-muted-foreground">浏览和搜索馆藏图书</p>
      </div>
      
      <!-- 搜索和筛选 -->
      <div class="mb-8 space-y-4">
        <div class="flex flex-col sm:flex-row gap-4">
          <div class="flex-1">
            <SearchBar
              :model-value="searchQuery"
              @search="handleSearch"
            />
          </div>
          <Button 
            variant="outline" 
            class="flex items-center gap-2"
            @click="showFilters = !showFilters"
          >
            <SlidersHorizontal class="h-4 w-4" />
            筛选排序
          </Button>
        </div>
        
        <!-- 筛选选项 -->
        <Transition
          enter-active-class="transition-all duration-200 ease-out"
          enter-from-class="opacity-0 -translate-y-2"
          enter-to-class="opacity-100 translate-y-0"
          leave-active-class="transition-all duration-150 ease-in"
          leave-from-class="opacity-100 translate-y-0"
          leave-to-class="opacity-0 -translate-y-2"
        >
          <div v-if="showFilters" class="p-4 bg-muted/50 rounded-lg">
            <div class="flex flex-wrap items-center gap-4">
              <span class="text-sm font-medium">排序方式：</span>
              <div class="flex flex-wrap gap-2">
                <Button
                  v-for="option in sortOptions"
                  :key="option.value"
                  :variant="sortColumn === option.value ? 'default' : 'outline'"
                  size="sm"
                  @click="handleSort(option.value)"
                >
                  {{ option.label }}
                  <span v-if="sortColumn === option.value" class="ml-1">
                    {{ sortOrder === 'ASC' ? '↑' : '↓' }}
                  </span>
                </Button>
              </div>
            </div>
          </div>
        </Transition>
      </div>
      
      <!-- 结果统计 -->
      <div class="mb-4 text-sm text-muted-foreground">
        共找到 {{ totalItems }} 本图书
      </div>
      
      <!-- 图书网格 -->
      <BookGrid
        :books="books"
        :loading="loading"
        @borrow="handleBorrow"
      />
      
      <!-- 分页 -->
      <Pagination
        v-if="totalPages > 1"
        v-model:current-page="currentPage"
        :total-pages="totalPages"
        :total-items="totalItems"
        :page-size="pageSize"
        class="mt-8"
      />
    </div>
  </ReaderLayout>
</template>
