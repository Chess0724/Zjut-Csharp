<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import Card from '@/components/ui/Card.vue'
import CardHeader from '@/components/ui/CardHeader.vue'
import CardTitle from '@/components/ui/CardTitle.vue'
import CardContent from '@/components/ui/CardContent.vue'
import DataTable from '@/components/ui/DataTable.vue'
import Pagination from '@/components/ui/Pagination.vue'
import Input from '@/components/ui/Input.vue'
import Button from '@/components/ui/Button.vue'
import Badge from '@/components/ui/Badge.vue'
import Dialog from '@/components/ui/Dialog.vue'
import { bookApi } from '@/api'
import { useToastStore } from '@/stores/toast'
import { useAuthStore } from '@/stores/auth'
import type { Book, BookDto } from '@/types'
import { Plus, Pencil, Trash2, Search } from 'lucide-vue-next'

const toastStore = useToastStore()
const authStore = useAuthStore()

const books = ref<Book[]>([])
const loading = ref(true)
const currentPage = ref(1)
const totalPages = ref(1)
const totalItems = ref(0)
const pageSize = ref(15)
const searchQuery = ref('')
const sortColumn = ref('Title')
const sortOrder = ref<'ASC' | 'DESC'>('ASC')

// 编辑对话框
const showEditDialog = ref(false)
const editingBook = ref<BookDto | null>(null)
const isNewBook = ref(false)
const saving = ref(false)

const columns = [
  { key: 'id', label: 'ID', width: '60px' },
  { key: 'title', label: '书名', sortable: true },
  { key: 'author', label: '作者', sortable: true },
  { key: 'publisher', label: '出版社', sortable: true },
  { key: 'identifier', label: '索书号' },
  { key: 'inventory', label: '库存', width: '80px' },
  { key: 'borrowed', label: '借出', width: '80px' },
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

function handleSort(column: string) {
  if (sortColumn.value === column) {
    sortOrder.value = sortOrder.value === 'ASC' ? 'DESC' : 'ASC'
  } else {
    sortColumn.value = column
    sortOrder.value = 'ASC'
  }
  fetchBooks()
}

function handleSearch() {
  currentPage.value = 1
  fetchBooks()
}

function openAddDialog() {
  isNewBook.value = true
  editingBook.value = {
    title: '',
    author: '',
    publisher: '',
    publishedDate: '',
    identifier: '',
    inventory: 1
  }
  showEditDialog.value = true
}

function openEditDialog(book: Book) {
  isNewBook.value = false
  editingBook.value = {
    id: book.id,
    title: book.title,
    author: book.author,
    publisher: book.publisher,
    publishedDate: book.publishedDate,
    identifier: book.identifier,
    inventory: book.inventory
  }
  showEditDialog.value = true
}

async function handleSave() {
  if (!editingBook.value) return
  
  if (!editingBook.value.title.trim()) {
    toastStore.warning('请填写书名')
    return
  }
  
  saving.value = true
  try {
    if (isNewBook.value) {
      const response = await bookApi.addBook(editingBook.value)
      if (response.data.code === 0) {
        toastStore.success('图书添加成功')
        showEditDialog.value = false
        fetchBooks()
      } else {
        toastStore.error(response.data.message)
      }
    } else {
      const response = await bookApi.updateBook(editingBook.value)
      if (response.data.code === 0) {
        toastStore.success('图书更新成功')
        showEditDialog.value = false
        fetchBooks()
      } else {
        toastStore.error(response.data.message)
      }
    }
  } catch {
    toastStore.error('操作失败')
  } finally {
    saving.value = false
  }
}

async function handleDelete(book: Book) {
  if (!confirm(`确定要删除《${book.title}》吗？此操作不可撤销。`)) {
    return
  }
  
  try {
    const response = await bookApi.deleteBook(book.id)
    if (response.data.code === 0) {
      toastStore.success('图书删除成功')
      fetchBooks()
    } else {
      toastStore.error(response.data.message)
    }
  } catch {
    toastStore.error('删除失败')
  }
}

onMounted(() => {
  fetchBooks()
})
</script>

<template>
  <div class="space-y-6">
    <!-- 页面标题 -->
    <div class="flex items-center justify-between">
      <div>
        <h1 class="text-2xl font-bold">图书管理</h1>
        <p class="text-muted-foreground">管理馆藏图书信息</p>
      </div>
      <Button @click="openAddDialog">
        <Plus class="h-4 w-4 mr-2" />
        添加图书
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
          :columns="columns"
          :data="books"
          :loading="loading"
          :sort-column="sortColumn"
          :sort-order="sortOrder"
          @sort="handleSort"
        >
          <template #cell-inventory="{ value }">
            <Badge :variant="(value as number) > 0 ? 'success' : 'destructive'">
              {{ value }}
            </Badge>
          </template>
          
          <template #cell-borrowed="{ value }">
            <span class="text-muted-foreground">{{ value }}</span>
          </template>
          
          <template #actions="{ row }">
            <div class="flex items-center justify-end gap-1">
              <Button
                variant="ghost"
                size="icon"
                @click.stop="openEditDialog(row as Book)"
              >
                <Pencil class="h-4 w-4" />
              </Button>
              <Button
                v-if="authStore.isAdmin"
                variant="ghost"
                size="icon"
                class="text-destructive hover:text-destructive"
                @click.stop="handleDelete(row as Book)"
              >
                <Trash2 class="h-4 w-4" />
              </Button>
            </div>
          </template>
        </DataTable>
        
        <Pagination
          v-if="totalPages > 1"
          v-model:current-page="currentPage"
          :total-pages="totalPages"
          :total-items="totalItems"
          :page-size="pageSize"
          @update:current-page="fetchBooks"
        />
      </CardContent>
    </Card>
    
    <!-- 编辑对话框 -->
    <Dialog
      v-model:open="showEditDialog"
      :title="isNewBook ? '添加图书' : '编辑图书'"
    >
      <form v-if="editingBook" @submit.prevent="handleSave" class="space-y-4">
        <div class="space-y-2">
          <label class="text-sm font-medium">书名 *</label>
          <Input v-model="editingBook.title" placeholder="请输入书名" />
        </div>
        
        <div class="grid grid-cols-2 gap-4">
          <div class="space-y-2">
            <label class="text-sm font-medium">作者</label>
            <Input v-model="editingBook.author" placeholder="请输入作者" />
          </div>
          <div class="space-y-2">
            <label class="text-sm font-medium">出版社</label>
            <Input v-model="editingBook.publisher" placeholder="请输入出版社" />
          </div>
        </div>
        
        <div class="grid grid-cols-2 gap-4">
          <div class="space-y-2">
            <label class="text-sm font-medium">出版日期</label>
            <Input v-model="editingBook.publishedDate" placeholder="如：2023-01" />
          </div>
          <div class="space-y-2">
            <label class="text-sm font-medium">索书号</label>
            <Input v-model="editingBook.identifier" placeholder="如：TP312/123" />
          </div>
        </div>
        
        <div class="space-y-2">
          <label class="text-sm font-medium">库存数量</label>
          <Input 
            v-model.number="editingBook.inventory" 
            type="number" 
            min="0"
            placeholder="请输入库存数量" 
          />
        </div>
        
        <div class="flex justify-end gap-2 pt-4">
          <Button type="button" variant="outline" @click="showEditDialog = false">
            取消
          </Button>
          <Button type="submit" :loading="saving">
            {{ isNewBook ? '添加' : '保存' }}
          </Button>
        </div>
      </form>
    </Dialog>
  </div>
</template>
