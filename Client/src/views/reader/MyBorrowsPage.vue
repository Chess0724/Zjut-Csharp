<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import ReaderLayout from '@/layouts/ReaderLayout.vue'
import Card from '@/components/ui/Card.vue'
import CardHeader from '@/components/ui/CardHeader.vue'
import CardTitle from '@/components/ui/CardTitle.vue'
import CardContent from '@/components/ui/CardContent.vue'
import Button from '@/components/ui/Button.vue'
import Badge from '@/components/ui/Badge.vue'
import ProgressRing from '@/components/ui/ProgressRing.vue'
import { userApi } from '@/api'
import { useToastStore } from '@/stores/toast'
import type { BorrowDto } from '@/types'
import { getBorrowRemainingDays, getBookCoverColor } from '@/lib/utils'
import { BookOpen, Clock, RotateCcw } from 'lucide-vue-next'

const toastStore = useToastStore()

const borrows = ref<BorrowDto[]>([])
const loading = ref(true)
const returning = ref<number | null>(null)

// 借阅期限（天）
const BORROW_LIMIT = 30

async function fetchBorrows() {
  loading.value = true
  try {
    const response = await userApi.getCurrentBorrow({ pageSize: 100 })
    if (response.data.code === 0) {
      borrows.value = response.data.data || []
    }
  } catch (error) {
    console.error('Failed to fetch borrows:', error)
  } finally {
    loading.value = false
  }
}

async function handleReturn(borrow: BorrowDto) {
  returning.value = borrow.id
  try {
    const response = await userApi.returnBook(borrow.id)
    if (response.data.code === 0) {
      toastStore.success(`《${borrow.title}》归还成功！`)
      fetchBorrows()
    } else {
      toastStore.error(response.data.message)
    }
  } catch {
    toastStore.error('归还失败，请稍后再试')
  } finally {
    returning.value = null
  }
}

function getRemainingDays(borrowDate: string): number {
  return getBorrowRemainingDays(borrowDate, BORROW_LIMIT)
}

function getRemainingPercentage(borrowDate: string): number {
  const remaining = getRemainingDays(borrowDate)
  return (remaining / BORROW_LIMIT) * 100
}

onMounted(() => {
  fetchBorrows()
})
</script>

<template>
  <ReaderLayout>
    <div class="container px-4 py-8">
      <!-- 页面标题 -->
      <div class="mb-8">
        <h1 class="text-3xl font-bold mb-2">我的借阅</h1>
        <p class="text-muted-foreground">管理您当前借阅的图书</p>
      </div>
      
      <!-- 加载状态 -->
      <div v-if="loading" class="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
        <Card v-for="i in 6" :key="i" class="animate-pulse">
          <CardContent class="p-6">
            <div class="flex gap-4">
              <div class="w-20 h-28 bg-muted rounded" />
              <div class="flex-1 space-y-2">
                <div class="h-5 bg-muted rounded w-3/4" />
                <div class="h-4 bg-muted rounded w-1/2" />
                <div class="h-4 bg-muted rounded w-1/3" />
              </div>
            </div>
          </CardContent>
        </Card>
      </div>
      
      <!-- 借阅列表 -->
      <div v-else-if="borrows.length > 0" class="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
        <Card 
          v-for="borrow in borrows" 
          :key="borrow.id"
          class="overflow-hidden hover:shadow-md transition-shadow"
        >
          <CardContent class="p-0">
            <div class="flex">
              <!-- 迷你封面 -->
              <div 
                :class="[
                  'w-24 flex-shrink-0 bg-gradient-to-br flex items-center justify-center p-3',
                  getBookCoverColor(borrow.title)
                ]"
              >
                <span class="text-white text-xs font-medium text-center leading-tight line-clamp-4">
                  {{ borrow.title }}
                </span>
              </div>
              
              <!-- 信息 -->
              <div class="flex-1 p-4">
                <div class="flex items-start justify-between gap-2 mb-2">
                  <h3 class="font-semibold line-clamp-1">{{ borrow.title }}</h3>
                </div>
                <p class="text-sm text-muted-foreground mb-1">{{ borrow.author }}</p>
                <div class="flex items-center gap-2 text-xs text-muted-foreground mb-3">
                  <Clock class="h-3 w-3" />
                  <span>借阅于 {{ borrow.borrowDate }}</span>
                </div>
                
                <!-- 剩余时间 -->
                <div class="flex items-center justify-between">
                  <div class="flex items-center gap-2">
                    <ProgressRing
                      :value="getRemainingPercentage(borrow.borrowDate)"
                      size="sm"
                    >
                      <span class="text-[10px] font-bold">
                        {{ getRemainingDays(borrow.borrowDate) }}
                      </span>
                    </ProgressRing>
                    <span class="text-xs text-muted-foreground">
                      剩余 {{ getRemainingDays(borrow.borrowDate) }} 天
                    </span>
                  </div>
                  <Button
                    variant="outline"
                    size="sm"
                    :loading="returning === borrow.id"
                    @click="handleReturn(borrow)"
                  >
                    <RotateCcw class="h-3 w-3 mr-1" />
                    归还
                  </Button>
                </div>
              </div>
            </div>
          </CardContent>
        </Card>
      </div>
      
      <!-- 空状态 -->
      <div v-else class="text-center py-16">
        <div class="w-20 h-20 mx-auto mb-4 rounded-full bg-muted flex items-center justify-center">
          <BookOpen class="h-10 w-10 text-muted-foreground" />
        </div>
        <h3 class="text-lg font-medium mb-2">暂无借阅</h3>
        <p class="text-muted-foreground mb-6">去图书馆看看有什么好书吧</p>
        <RouterLink to="/books">
          <Button>浏览图书</Button>
        </RouterLink>
      </div>
    </div>
  </ReaderLayout>
</template>
