<script setup lang="ts">
import { ref, onMounted } from 'vue'
import Card from '@/components/ui/Card.vue'
import CardContent from '@/components/ui/CardContent.vue'
import DataTable from '@/components/ui/DataTable.vue'
import Pagination from '@/components/ui/Pagination.vue'
import Input from '@/components/ui/Input.vue'
import Button from '@/components/ui/Button.vue'
import Badge from '@/components/ui/Badge.vue'
import Dialog from '@/components/ui/Dialog.vue'
import { recommendApi } from '@/api'
import { useToastStore } from '@/stores/toast'
import type { Recommend } from '@/types'
import { Search, Check, Clock, MessageSquare } from 'lucide-vue-next'

const toastStore = useToastStore()

const recommends = ref<Recommend[]>([])
const loading = ref(true)
const currentPage = ref(1)
const totalPages = ref(1)
const totalItems = ref(0)
const pageSize = ref(15)
const searchQuery = ref('')
const sortColumn = ref('CreateTime')
const sortOrder = ref<'ASC' | 'DESC'>('DESC')

// 处理对话框
const showProcessDialog = ref(false)
const processingRecommend = ref<Recommend | null>(null)
const adminRemark = ref('')
const processing = ref(false)

const columns = [
  { key: 'title', label: '书名', sortable: true },
  { key: 'author', label: '作者' },
  { key: 'publisher', label: '出版社' },
  { key: 'isbn', label: 'ISBN' },
  { key: 'userName', label: '推荐人' },
  { key: 'createTime', label: '提交时间', sortable: true },
  { key: 'isProcessed', label: '状态' },
]

async function fetchRecommends() {
  loading.value = true
  try {
    const response = await recommendApi.getRecommends({
      pageIndex: currentPage.value - 1,
      pageSize: pageSize.value,
      sortColumn: sortColumn.value,
      sortOrder: sortOrder.value,
      filterQuery: searchQuery.value || undefined
    })
    
    if (response.data.code === 0) {
      recommends.value = response.data.data || []
      totalItems.value = response.data.recordCount
      totalPages.value = Math.ceil(response.data.recordCount / pageSize.value)
    }
  } catch (error) {
    console.error('Failed to fetch recommends:', error)
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
  fetchRecommends()
}

function handleSearch() {
  currentPage.value = 1
  fetchRecommends()
}

function openProcessDialog(recommend: Recommend) {
  processingRecommend.value = recommend
  adminRemark.value = recommend.adminRemark || ''
  showProcessDialog.value = true
}

async function handleProcess() {
  if (!processingRecommend.value) return
  
  processing.value = true
  try {
    const response = await recommendApi.updateRecommend({
      id: processingRecommend.value.id,
      isProcessed: true,
      adminRemark: adminRemark.value
    })
    
    if (response.data.code === 0) {
      toastStore.success('处理成功')
      showProcessDialog.value = false
      fetchRecommends()
    } else {
      toastStore.error(response.data.message)
    }
  } catch {
    toastStore.error('处理失败')
  } finally {
    processing.value = false
  }
}

onMounted(() => {
  fetchRecommends()
})
</script>

<template>
  <div class="space-y-6">
    <!-- 页面标题 -->
    <div>
      <h1 class="text-2xl font-bold">荐购管理</h1>
      <p class="text-muted-foreground">处理读者荐购请求</p>
    </div>
    
    <!-- 搜索栏 -->
    <Card>
      <CardContent class="p-4">
        <div class="flex gap-4">
          <div class="relative flex-1">
            <Search class="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
            <Input
              v-model="searchQuery"
              placeholder="搜索书名、作者、出版社或ISBN..."
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
          :data="recommends"
          :loading="loading"
          :sort-column="sortColumn"
          :sort-order="sortOrder"
          @sort="handleSort"
        >
          <template #cell-isProcessed="{ value }">
            <Badge :variant="value === '是' ? 'success' : 'secondary'">
              <component 
                :is="value === '是' ? Check : Clock" 
                class="h-3 w-3 mr-1" 
              />
              {{ value === '是' ? '已处理' : '待处理' }}
            </Badge>
          </template>
          
          <template #actions="{ row }">
            <Button
              v-if="(row as unknown as Recommend).isProcessed !== '是'"
              variant="outline"
              size="sm"
              @click.stop="openProcessDialog(row as unknown as Recommend)"
            >
              <MessageSquare class="h-3 w-3 mr-1" />
              处理
            </Button>
            <Button
              v-else
              variant="ghost"
              size="sm"
              @click.stop="openProcessDialog(row as unknown as Recommend)"
            >
              查看
            </Button>
          </template>
        </DataTable>
        
        <Pagination
          v-if="totalPages > 1"
          v-model:current-page="currentPage"
          :total-pages="totalPages"
          :total-items="totalItems"
          :page-size="pageSize"
          @update:current-page="fetchRecommends"
        />
      </CardContent>
    </Card>
    
    <!-- 处理对话框 -->
    <Dialog
      v-model:open="showProcessDialog"
      title="处理荐购"
    >
      <div v-if="processingRecommend" class="space-y-4">
        <!-- 荐购信息 -->
        <div class="p-4 bg-muted/50 rounded-lg space-y-2">
          <p><strong>书名：</strong>{{ processingRecommend.title }}</p>
          <p v-if="processingRecommend.author"><strong>作者：</strong>{{ processingRecommend.author }}</p>
          <p v-if="processingRecommend.publisher"><strong>出版社：</strong>{{ processingRecommend.publisher }}</p>
          <p v-if="processingRecommend.isbn"><strong>ISBN：</strong>{{ processingRecommend.isbn }}</p>
          <p v-if="processingRecommend.userRemark"><strong>读者留言：</strong>{{ processingRecommend.userRemark }}</p>
          <p class="text-sm text-muted-foreground">
            由 {{ processingRecommend.userName }} 于 {{ processingRecommend.createTime }} 提交
          </p>
        </div>
        
        <!-- 管理员回复 -->
        <div class="space-y-2">
          <label class="text-sm font-medium">管理员回复</label>
          <textarea
            v-model="adminRemark"
            placeholder="请填写处理说明..."
            class="w-full h-24 px-3 py-2 border rounded-lg resize-none focus:outline-none focus:ring-1 focus:ring-ring"
            :disabled="processingRecommend.isProcessed === '是'"
          />
        </div>
        
        <!-- 操作按钮 -->
        <div class="flex justify-end gap-2">
          <Button variant="outline" @click="showProcessDialog = false">
            取消
          </Button>
          <Button
            v-if="processingRecommend.isProcessed !== '是'"
            :loading="processing"
            @click="handleProcess"
          >
            <Check class="h-4 w-4 mr-2" />
            标记为已处理
          </Button>
        </div>
      </div>
    </Dialog>
  </div>
</template>
