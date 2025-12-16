<script setup lang="ts">
import { ref, onMounted } from 'vue'
import ReaderLayout from '@/layouts/ReaderLayout.vue'
import Card from '@/components/ui/Card.vue'
import CardHeader from '@/components/ui/CardHeader.vue'
import CardTitle from '@/components/ui/CardTitle.vue'
import CardContent from '@/components/ui/CardContent.vue'
import { userApi } from '@/api'
import type { BorrowDto, UserStatistics } from '@/types'
import { getBookCoverColor, formatDate } from '@/lib/utils'
import { BookOpen, Clock, BarChart3, Calendar } from 'lucide-vue-next'

const history = ref<BorrowDto[]>([])
const statistics = ref<UserStatistics | null>(null)
const loading = ref(true)

async function fetchData() {
  loading.value = true
  try {
    const [historyRes, statsRes] = await Promise.all([
      userApi.getBorrowHistory({ pageSize: 100 }),
      userApi.getStatistics()
    ])
    
    if (historyRes.data.code === 0) {
      history.value = historyRes.data.data || []
    }
    
    if (statsRes.data.code === 0 && statsRes.data.data) {
      statistics.value = statsRes.data.data
    }
  } catch (error) {
    console.error('Failed to fetch data:', error)
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  fetchData()
})
</script>

<template>
  <ReaderLayout>
    <div class="container px-4 py-8">
      <!-- 页面标题 -->
      <div class="mb-8">
        <h1 class="text-3xl font-bold mb-2">借阅历史</h1>
        <p class="text-muted-foreground">查看您的借阅记录和阅读统计</p>
      </div>
      
      <!-- 统计卡片 -->
      <div v-if="statistics" class="grid gap-4 md:grid-cols-4 mb-8">
        <Card>
          <CardContent class="p-6">
            <div class="flex items-center gap-4">
              <div class="w-12 h-12 rounded-full bg-primary/10 flex items-center justify-center">
                <BookOpen class="h-6 w-6 text-primary" />
              </div>
              <div>
                <p class="text-2xl font-bold">{{ statistics.totalBorrowedBooks }}</p>
                <p class="text-sm text-muted-foreground">总借阅</p>
              </div>
            </div>
          </CardContent>
        </Card>
        
        <Card>
          <CardContent class="p-6">
            <div class="flex items-center gap-4">
              <div class="w-12 h-12 rounded-full bg-green-100 dark:bg-green-900 flex items-center justify-center">
                <BarChart3 class="h-6 w-6 text-green-600" />
              </div>
              <div>
                <p class="text-2xl font-bold">{{ statistics.totalReturnedBooks }}</p>
                <p class="text-sm text-muted-foreground">已归还</p>
              </div>
            </div>
          </CardContent>
        </Card>
        
        <Card>
          <CardContent class="p-6">
            <div class="flex items-center gap-4">
              <div class="w-12 h-12 rounded-full bg-amber-100 dark:bg-amber-900 flex items-center justify-center">
                <Calendar class="h-6 w-6 text-amber-600" />
              </div>
              <div>
                <p class="text-2xl font-bold">{{ statistics.currentBorrowedBooks }}</p>
                <p class="text-sm text-muted-foreground">当前借阅</p>
              </div>
            </div>
          </CardContent>
        </Card>
        
        <Card>
          <CardContent class="p-6">
            <div class="flex items-center gap-4">
              <div class="w-12 h-12 rounded-full bg-purple-100 dark:bg-purple-900 flex items-center justify-center">
                <Clock class="h-6 w-6 text-purple-600" />
              </div>
              <div>
                <p class="text-2xl font-bold">{{ statistics.averageBorrowDuration }}</p>
                <p class="text-sm text-muted-foreground">平均借阅天数</p>
              </div>
            </div>
          </CardContent>
        </Card>
      </div>
      
      <!-- 时间轴 -->
      <Card>
        <CardHeader>
          <CardTitle>借阅记录</CardTitle>
        </CardHeader>
        <CardContent>
          <!-- 加载状态 -->
          <div v-if="loading" class="space-y-4">
            <div v-for="i in 5" :key="i" class="flex gap-4 animate-pulse">
              <div class="w-16 h-20 bg-muted rounded" />
              <div class="flex-1 space-y-2">
                <div class="h-4 bg-muted rounded w-1/2" />
                <div class="h-3 bg-muted rounded w-1/3" />
              </div>
            </div>
          </div>
          
          <!-- 时间轴列表 -->
          <div v-else-if="history.length > 0" class="relative">
            <!-- 时间轴线 -->
            <div class="absolute left-7 top-0 bottom-0 w-px bg-border" />
            
            <div class="space-y-6">
              <div 
                v-for="(item, index) in history" 
                :key="index"
                class="relative flex gap-6"
              >
                <!-- 时间点 -->
                <div class="relative z-10 flex-shrink-0">
                  <div 
                    :class="[
                      'w-14 h-14 rounded-lg bg-gradient-to-br flex items-center justify-center shadow-sm',
                      getBookCoverColor(item.title)
                    ]"
                  >
                    <BookOpen class="h-6 w-6 text-white" />
                  </div>
                </div>
                
                <!-- 内容 -->
                <div class="flex-1 pb-6">
                  <div class="bg-muted/50 rounded-lg p-4">
                    <h4 class="font-semibold mb-1">{{ item.title }}</h4>
                    <p class="text-sm text-muted-foreground mb-2">{{ item.author }}</p>
                    <div class="flex flex-wrap gap-4 text-xs text-muted-foreground">
                      <span class="flex items-center gap-1">
                        <Calendar class="h-3 w-3" />
                        借阅：{{ item.borrowDate }}
                      </span>
                      <span v-if="item.returnDate" class="flex items-center gap-1">
                        <Clock class="h-3 w-3" />
                        归还：{{ item.returnDate }}
                      </span>
                      <span class="flex items-center gap-1">
                        借阅时长：{{ item.borrowDuration }} 天
                      </span>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
          
          <!-- 空状态 -->
          <div v-else class="text-center py-12 text-muted-foreground">
            暂无借阅记录
          </div>
        </CardContent>
      </Card>
    </div>
  </ReaderLayout>
</template>
