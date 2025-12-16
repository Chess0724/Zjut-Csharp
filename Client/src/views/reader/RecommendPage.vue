<script setup lang="ts">
import { ref, onMounted } from 'vue'
import ReaderLayout from '@/layouts/ReaderLayout.vue'
import Card from '@/components/ui/Card.vue'
import CardHeader from '@/components/ui/CardHeader.vue'
import CardTitle from '@/components/ui/CardTitle.vue'
import CardDescription from '@/components/ui/CardDescription.vue'
import CardContent from '@/components/ui/CardContent.vue'
import Input from '@/components/ui/Input.vue'
import Button from '@/components/ui/Button.vue'
import Badge from '@/components/ui/Badge.vue'
import { userApi, recommendApi } from '@/api'
import { useToastStore } from '@/stores/toast'
import type { Recommend } from '@/types'
import { BookPlus, Send, Clock, CheckCircle, XCircle } from 'lucide-vue-next'

const toastStore = useToastStore()

const myRecommends = ref<Recommend[]>([])
const loading = ref(true)
const submitting = ref(false)

// 表单数据
const form = ref({
  title: '',
  author: '',
  publisher: '',
  isbn: '',
  remark: ''
})

async function fetchRecommends() {
  loading.value = true
  try {
    const response = await recommendApi.getRecommends({ pageSize: 100 })
    if (response.data.code === 0) {
      myRecommends.value = response.data.data || []
    }
  } catch (error) {
    console.error('Failed to fetch recommends:', error)
  } finally {
    loading.value = false
  }
}

async function handleSubmit() {
  if (!form.value.title.trim()) {
    toastStore.warning('请填写书名')
    return
  }
  
  submitting.value = true
  try {
    const response = await userApi.addRecommend({
      title: form.value.title,
      author: form.value.author,
      publisher: form.value.publisher,
      isbn: form.value.isbn,
      remark: form.value.remark
    })
    
    if (response.data.code === 0) {
      toastStore.success('荐购请求提交成功')
      // 重置表单
      form.value = {
        title: '',
        author: '',
        publisher: '',
        isbn: '',
        remark: ''
      }
      fetchRecommends()
    } else {
      toastStore.error(response.data.message)
    }
  } catch {
    toastStore.error('提交失败，请稍后再试')
  } finally {
    submitting.value = false
  }
}

function getStatusBadge(isProcessed: string) {
  if (isProcessed === '是') {
    return { variant: 'success' as const, text: '已处理', icon: CheckCircle }
  }
  return { variant: 'secondary' as const, text: '待处理', icon: Clock }
}

onMounted(() => {
  fetchRecommends()
})
</script>

<template>
  <ReaderLayout>
    <div class="container px-4 py-8">
      <!-- 页面标题 -->
      <div class="mb-8">
        <h1 class="text-3xl font-bold mb-2">荐购图书</h1>
        <p class="text-muted-foreground">找不到想要的书？向图书馆推荐购买</p>
      </div>
      
      <div class="grid gap-8 lg:grid-cols-5">
        <!-- 荐购表单 -->
        <Card class="lg:col-span-2">
          <CardHeader>
            <CardTitle class="flex items-center gap-2">
              <BookPlus class="h-5 w-5" />
              提交荐购
            </CardTitle>
            <CardDescription>
              填写您想推荐购买的图书信息
            </CardDescription>
          </CardHeader>
          <CardContent>
            <form @submit.prevent="handleSubmit" class="space-y-4">
              <div class="space-y-2">
                <label class="text-sm font-medium">书名 *</label>
                <Input
                  v-model="form.title"
                  placeholder="请输入书名"
                />
              </div>
              
              <div class="space-y-2">
                <label class="text-sm font-medium">作者</label>
                <Input
                  v-model="form.author"
                  placeholder="请输入作者"
                />
              </div>
              
              <div class="space-y-2">
                <label class="text-sm font-medium">出版社</label>
                <Input
                  v-model="form.publisher"
                  placeholder="请输入出版社"
                />
              </div>
              
              <div class="space-y-2">
                <label class="text-sm font-medium">ISBN</label>
                <Input
                  v-model="form.isbn"
                  placeholder="请输入ISBN号"
                />
              </div>
              
              <div class="space-y-2">
                <label class="text-sm font-medium">推荐理由</label>
                <textarea
                  v-model="form.remark"
                  placeholder="说说您为什么推荐这本书..."
                  class="w-full h-24 px-3 py-2 border rounded-lg resize-none focus:outline-none focus:ring-1 focus:ring-ring"
                />
              </div>
              
              <Button type="submit" class="w-full" :loading="submitting">
                <Send class="h-4 w-4 mr-2" />
                提交荐购
              </Button>
            </form>
          </CardContent>
        </Card>
        
        <!-- 我的荐购记录 -->
        <Card class="lg:col-span-3">
          <CardHeader>
            <CardTitle>荐购记录</CardTitle>
            <CardDescription>
              您提交的荐购请求
            </CardDescription>
          </CardHeader>
          <CardContent>
            <!-- 加载状态 -->
            <div v-if="loading" class="space-y-4">
              <div v-for="i in 3" :key="i" class="p-4 border rounded-lg animate-pulse">
                <div class="h-5 bg-muted rounded w-1/3 mb-2" />
                <div class="h-4 bg-muted rounded w-1/2" />
              </div>
            </div>
            
            <!-- 荐购列表 -->
            <div v-else-if="myRecommends.length > 0" class="space-y-4">
              <div
                v-for="rec in myRecommends"
                :key="rec.id"
                class="p-4 border rounded-lg hover:bg-muted/50 transition-colors"
              >
                <div class="flex items-start justify-between gap-4 mb-2">
                  <h4 class="font-semibold">{{ rec.title }}</h4>
                  <Badge :variant="getStatusBadge(rec.isProcessed).variant">
                    <component :is="getStatusBadge(rec.isProcessed).icon" class="h-3 w-3 mr-1" />
                    {{ getStatusBadge(rec.isProcessed).text }}
                  </Badge>
                </div>
                
                <div class="text-sm text-muted-foreground space-y-1">
                  <p v-if="rec.author">作者：{{ rec.author }}</p>
                  <p v-if="rec.publisher">出版社：{{ rec.publisher }}</p>
                  <p v-if="rec.isbn">ISBN：{{ rec.isbn }}</p>
                  <p v-if="rec.userRemark">推荐理由：{{ rec.userRemark }}</p>
                </div>
                
                <div class="mt-3 flex items-center gap-4 text-xs text-muted-foreground">
                  <span>提交时间：{{ rec.createTime }}</span>
                  <span v-if="rec.updateTime !== '尚未处理'">处理时间：{{ rec.updateTime }}</span>
                </div>
                
                <div v-if="rec.adminRemark" class="mt-3 p-3 bg-muted rounded-lg">
                  <p class="text-sm">
                    <span class="font-medium">管理员回复：</span>
                    {{ rec.adminRemark }}
                  </p>
                </div>
              </div>
            </div>
            
            <!-- 空状态 -->
            <div v-else class="text-center py-12 text-muted-foreground">
              <Clock class="h-12 w-12 mx-auto mb-4 opacity-50" />
              <p>暂无荐购记录</p>
            </div>
          </CardContent>
        </Card>
      </div>
    </div>
  </ReaderLayout>
</template>
