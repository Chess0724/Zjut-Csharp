<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import ReaderLayout from '@/layouts/ReaderLayout.vue'
import Button from '@/components/ui/Button.vue'
import Badge from '@/components/ui/Badge.vue'
import Card from '@/components/ui/Card.vue'
import CardHeader from '@/components/ui/CardHeader.vue'
import CardTitle from '@/components/ui/CardTitle.vue'
import CardContent from '@/components/ui/CardContent.vue'
import { bookApi, userApi, commentApi } from '@/api'
import { useToastStore } from '@/stores/toast'
import { useAuthStore } from '@/stores/auth'
import { getBookCoverColor, getClassificationName } from '@/lib/utils'
import type { Book, BookComment } from '@/types'
import { 
  ArrowLeft, 
  BookOpen, 
  User, 
  Building2, 
  Calendar,
  Tag,
  Package,
  Star
} from 'lucide-vue-next'

const route = useRoute()
const router = useRouter()
const toastStore = useToastStore()
const authStore = useAuthStore()

const book = ref<Book | null>(null)
const comments = ref<BookComment[]>([])
const loading = ref(true)
const borrowing = ref(false)
const newComment = ref('')
const newRating = ref(5)
const submittingComment = ref(false)

const bookId = computed(() => Number(route.params.id))
const coverColor = computed(() => book.value ? getBookCoverColor(book.value.title) : '')
const isAvailable = computed(() => (book.value?.inventory || 0) > 0)
const classification = computed(() => book.value ? getClassificationName(book.value.identifier) : '')

async function fetchBook() {
  loading.value = true
  try {
    const response = await bookApi.getBooks({ filterQuery: route.params.id as string })
    if (response.data.code === 0 && response.data.data?.length) {
      book.value = response.data.data.find(b => b.id === bookId.value) || null
    }
    await fetchComments()
  } catch (error) {
    console.error('Failed to fetch book:', error)
  } finally {
    loading.value = false
  }
}

async function fetchComments() {
  try {
    const response = await commentApi.getComments(bookId.value, { pageSize: 50 })
    if (response.data.code === 0) {
      comments.value = response.data.data || []
    }
  } catch (error) {
    console.error('Failed to fetch comments:', error)
  }
}

async function handleBorrow() {
  if (!book.value) return
  borrowing.value = true
  try {
    const response = await userApi.borrowBook(book.value.id)
    if (response.data.code === 0) {
      toastStore.success(`《${book.value.title}》借阅成功！`)
      fetchBook()
    } else {
      toastStore.error(response.data.message)
    }
  } catch {
    toastStore.error('借阅失败，请稍后再试')
  } finally {
    borrowing.value = false
  }
}

async function submitComment() {
  if (!newComment.value.trim()) {
    toastStore.warning('请输入评论内容')
    return
  }
  
  submittingComment.value = true
  try {
    const response = await commentApi.addComment({
      bookId: bookId.value,
      content: newComment.value,
      rating: newRating.value
    })
    if (response.data.code === 0) {
      toastStore.success('评论发表成功')
      newComment.value = ''
      newRating.value = 5
      fetchComments()
    } else {
      toastStore.error(response.data.message)
    }
  } catch {
    toastStore.error('评论发表失败')
  } finally {
    submittingComment.value = false
  }
}

function goBack() {
  router.back()
}

onMounted(() => {
  fetchBook()
})
</script>

<template>
  <ReaderLayout>
    <div class="container px-4 py-8">
      <!-- 返回按钮 -->
      <Button variant="ghost" class="mb-6" @click="goBack">
        <ArrowLeft class="h-4 w-4 mr-2" />
        返回
      </Button>
      
      <!-- 加载状态 -->
      <div v-if="loading" class="animate-pulse space-y-8">
        <div class="flex flex-col lg:flex-row gap-8">
          <div class="w-full lg:w-80 h-96 bg-muted rounded-xl" />
          <div class="flex-1 space-y-4">
            <div class="h-8 bg-muted rounded w-2/3" />
            <div class="h-6 bg-muted rounded w-1/2" />
            <div class="h-4 bg-muted rounded w-1/3" />
          </div>
        </div>
      </div>
      
      <!-- 书籍详情 -->
      <div v-else-if="book" class="space-y-8">
        <div class="flex flex-col lg:flex-row gap-8">
          <!-- 封面 -->
          <div class="w-full lg:w-80 flex-shrink-0">
            <div 
              :class="[
                'relative h-96 rounded-xl bg-gradient-to-br flex items-center justify-center p-8 shadow-lg',
                coverColor
              ]"
            >
              <div class="absolute inset-0 opacity-10">
                <div class="absolute top-6 left-6 right-6 h-px bg-white" />
                <div class="absolute bottom-6 left-6 right-6 h-px bg-white" />
                <div class="absolute top-6 bottom-6 left-6 w-px bg-white" />
                <div class="absolute top-6 bottom-6 right-6 w-px bg-white" />
              </div>
              <h2 class="text-white text-center font-serif text-2xl font-bold leading-tight">
                {{ book.title }}
              </h2>
            </div>
          </div>
          
          <!-- 信息 -->
          <div class="flex-1 space-y-6">
            <div>
              <div class="flex items-start justify-between gap-4 mb-2">
                <h1 class="text-3xl font-bold font-serif">{{ book.title }}</h1>
                <Badge :variant="isAvailable ? 'success' : 'destructive'" class="flex-shrink-0">
                  {{ isAvailable ? `库存 ${book.inventory}` : '暂无库存' }}
                </Badge>
              </div>
              <p class="text-xl text-muted-foreground">{{ book.author }}</p>
            </div>
            
            <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
              <div class="flex items-center gap-3 text-muted-foreground">
                <Building2 class="h-5 w-5" />
                <span>{{ book.publisher }}</span>
              </div>
              <div class="flex items-center gap-3 text-muted-foreground">
                <Calendar class="h-5 w-5" />
                <span>{{ book.publishedDate }}</span>
              </div>
              <div class="flex items-center gap-3 text-muted-foreground">
                <Tag class="h-5 w-5" />
                <span>{{ book.identifier }}</span>
              </div>
              <div class="flex items-center gap-3 text-muted-foreground">
                <BookOpen class="h-5 w-5" />
                <span>{{ classification }}</span>
              </div>
              <div class="flex items-center gap-3 text-muted-foreground">
                <Package class="h-5 w-5" />
                <span>已借出 {{ book.borrowed }} 本</span>
              </div>
            </div>
            
            <!-- 借阅按钮 -->
            <div class="pt-4">
              <Button
                v-if="isAvailable"
                size="lg"
                :loading="borrowing"
                @click="handleBorrow"
              >
                立即借阅
              </Button>
              <Button v-else size="lg" disabled>
                暂无库存
              </Button>
            </div>
          </div>
        </div>
        
        <!-- 评论区 -->
        <Card>
          <CardHeader>
            <CardTitle>读者评论</CardTitle>
          </CardHeader>
          <CardContent>
            <!-- 发表评论 -->
            <div v-if="authStore.isLoggedIn" class="mb-6 p-4 bg-muted/50 rounded-lg">
              <div class="flex items-center gap-2 mb-3">
                <span class="text-sm font-medium">评分：</span>
                <div class="flex gap-1">
                  <button
                    v-for="i in 5"
                    :key="i"
                    @click="newRating = i"
                  >
                    <Star 
                      :class="[
                        'h-5 w-5 transition-colors',
                        i <= newRating ? 'text-amber-400 fill-amber-400' : 'text-muted-foreground'
                      ]"
                    />
                  </button>
                </div>
              </div>
              <textarea
                v-model="newComment"
                placeholder="分享您对这本书的看法..."
                class="w-full h-24 px-3 py-2 border rounded-lg resize-none focus:outline-none focus:ring-1 focus:ring-ring"
              />
              <div class="mt-2 flex justify-end">
                <Button
                  :loading="submittingComment"
                  @click="submitComment"
                >
                  发表评论
                </Button>
              </div>
            </div>
            
            <!-- 评论列表 -->
            <div v-if="comments.length > 0" class="space-y-4">
              <div
                v-for="comment in comments"
                :key="comment.id"
                class="flex gap-4 pb-4 border-b last:border-0"
              >
                <div class="flex-shrink-0">
                  <div class="w-10 h-10 rounded-full bg-muted flex items-center justify-center">
                    <User class="h-5 w-5 text-muted-foreground" />
                  </div>
                </div>
                <div class="flex-1">
                  <div class="flex items-center gap-2 mb-1">
                    <span class="font-medium">{{ comment.userName }}</span>
                    <div class="flex">
                      <Star
                        v-for="i in 5"
                        :key="i"
                        :class="[
                          'h-3 w-3',
                          i <= comment.rating ? 'text-amber-400 fill-amber-400' : 'text-muted'
                        ]"
                      />
                    </div>
                    <span class="text-xs text-muted-foreground">{{ comment.createTime }}</span>
                  </div>
                  <p class="text-sm text-muted-foreground">{{ comment.content }}</p>
                </div>
              </div>
            </div>
            <div v-else class="text-center py-8 text-muted-foreground">
              暂无评论，来发表第一条评论吧！
            </div>
          </CardContent>
        </Card>
      </div>
      
      <!-- 未找到 -->
      <div v-else class="text-center py-16">
        <p class="text-muted-foreground">未找到该图书</p>
      </div>
    </div>
  </ReaderLayout>
</template>
