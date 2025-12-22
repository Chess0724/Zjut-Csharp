<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { smartRecommendApi, userApi } from '@/api'
import { useAuthStore } from '@/stores/auth'
import { useToastStore } from '@/stores/toast'
import type { Book } from '@/types'
import BookCard from '@/components/book/BookCard.vue'
import BookCardSkeleton from '@/components/book/BookCardSkeleton.vue'
import Button from '@/components/ui/Button.vue'
import { 
  Sparkles, 
  BookOpen, 
  ChevronRight
} from 'lucide-vue-next'

const authStore = useAuthStore()
const toastStore = useToastStore()

const recommendations = ref<Book[]>([])
const loading = ref(false)
const error = ref<string | null>(null)

// 获取推荐书籍
async function fetchRecommendations() {
  if (!authStore.isLoggedIn) return
  
  loading.value = true
  error.value = null
  
  try {
    const response = await smartRecommendApi.getRecommendations(8)
    if (response.data.code === 0 && response.data.data) {
      // 将推荐数据转换为 Book 类型
      recommendations.value = response.data.data.map(item => ({
        id: item.id,
        title: item.title,
        author: item.author,
        publisher: item.publisher,
        publishedDate: item.publishedDate,
        identifier: item.identifier,
        inboundDate: '',
        inventory: item.inventory,
        borrowed: 0,
        price: item.price,
        originalPrice: item.originalPrice
      }))
    } else {
      error.value = response.data.message || '获取推荐失败'
    }
  } catch (e) {
    console.error('获取推荐书籍失败:', e)
    error.value = '网络错误，请稍后重试'
  } finally {
    loading.value = false
  }
}

// 处理借阅
async function handleBorrow(book: Book) {
  try {
    const response = await userApi.borrowBook(book.id)
    if (response.data.code === 0) {
      toastStore.success(`《${book.title}》借阅成功！`)
      fetchRecommendations() // 刷新推荐
    } else {
      toastStore.error(response.data.message)
    }
  } catch {
    toastStore.error('借阅失败，请稍后再试')
  }
}

onMounted(() => {
  fetchRecommendations()
})
</script>

<template>
  <section v-if="authStore.isLoggedIn" class="py-12 bg-muted/30">
    <div class="container px-4">
      <!-- 标题 -->
      <div class="flex items-center justify-between mb-8">
        <div class="flex items-center gap-3">
          <div class="p-2 bg-primary/10 rounded-lg">
            <Sparkles class="h-6 w-6 text-primary" />
          </div>
          <div>
            <h2 class="text-2xl font-bold">猜你喜欢</h2>
            <p class="text-sm text-muted-foreground">基于您的购买历史智能推荐</p>
          </div>
        </div>
        <RouterLink to="/books" class="text-primary hover:underline flex items-center gap-1">
          查看更多
          <ChevronRight class="h-4 w-4" />
        </RouterLink>
      </div>

      <!-- 加载状态 - 骨架屏 -->
      <div v-if="loading" class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
        <BookCardSkeleton v-for="i in 8" :key="i" />
      </div>

      <!-- 错误状态 -->
      <div v-else-if="error" class="text-center py-12 text-muted-foreground">
        <p>{{ error }}</p>
        <Button variant="outline" class="mt-4" @click="fetchRecommendations">
          重试
        </Button>
      </div>

      <!-- 无推荐数据 -->
      <div v-else-if="recommendations.length === 0" class="text-center py-12">
        <BookOpen class="h-12 w-12 mx-auto mb-4 text-muted-foreground/50" />
        <p class="text-muted-foreground">暂无推荐，购买更多书籍后将为您生成个性化推荐</p>
        <RouterLink to="/books">
          <Button class="mt-4">
            去逛逛书城
          </Button>
        </RouterLink>
      </div>

      <!-- 推荐列表 - 使用 BookCard 组件 -->
      <div v-else class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
        <BookCard
          v-for="book in recommendations"
          :key="book.id"
          :book="book"
          @borrow="handleBorrow"
        />
      </div>
    </div>
  </section>
</template>