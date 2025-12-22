<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import ReaderLayout from '@/layouts/ReaderLayout.vue'
import Button from '@/components/ui/Button.vue'
import Input from '@/components/ui/Input.vue'
import Card from '@/components/ui/Card.vue'
import CardContent from '@/components/ui/CardContent.vue'
import SmartRecommendation from '@/components/book/SmartRecommendation.vue'
import { 
  Search, 
  BookOpen, 
  Users, 
  Clock, 
  TrendingUp,
  ArrowRight
} from 'lucide-vue-next'

const router = useRouter()
const searchQuery = ref('')

const features = [
  {
    icon: BookOpen,
    title: '海量图书',
    description: '涵盖各类学科领域，新书好书一网打尽'
  },
  {
    icon: Clock,
    title: '借阅购买',
    description: '支持借阅和购买，满足不同阅读需求'
  },
  {
    icon: Users,
    title: '荐购服务',
    description: '找不到想要的书？向我们推荐购买'
  },
  {
    icon: TrendingUp,
    title: '订单跟踪',
    description: '实时查看订单状态，物流信息一目了然'
  }
]

function handleSearch() {
  if (searchQuery.value.trim()) {
    router.push({ name: 'Books', query: { q: searchQuery.value } })
  }
}
</script>

<template>
  <ReaderLayout>
    <!-- Hero Section -->
    <section class="relative overflow-hidden bg-gradient-to-br from-library-600 via-library-700 to-library-900">
      <!-- 装饰背景 -->
      <div class="absolute inset-0 opacity-10">
        <div class="absolute top-20 left-20 w-72 h-72 bg-white rounded-full blur-3xl" />
        <div class="absolute bottom-20 right-20 w-96 h-96 bg-white rounded-full blur-3xl" />
      </div>
      
      <div class="relative container px-4 py-24 lg:py-32">
        <div class="max-w-3xl mx-auto text-center">
          <h1 class="text-4xl lg:text-6xl font-bold text-white mb-6 font-serif">
            新华书店 · 阅读新体验
          </h1>
          <p class="text-lg lg:text-xl text-library-100 mb-10">
            提供图书购买与借阅一站式服务，让阅读触手可及
          </p>
          
          <!-- 搜索框 -->
          <form 
            class="flex flex-col sm:flex-row gap-4 max-w-2xl mx-auto"
            @submit.prevent="handleSearch"
          >
            <div class="relative flex-1">
              <Search class="absolute left-4 top-1/2 -translate-y-1/2 h-5 w-5 text-muted-foreground" />
              <Input
                v-model="searchQuery"
                type="search"
                placeholder="搜索书名、作者或出版社..."
                class="h-14 pl-12 pr-4 text-base bg-white/95 border-0 shadow-lg"
              />
            </div>
            <Button type="submit" size="lg" class="h-14 px-8 shadow-lg">
              搜索图书
            </Button>
          </form>
          
          <!-- 快捷入口 -->
          <div class="mt-8 flex flex-wrap justify-center gap-4 text-library-100">
            <RouterLink 
              to="/books" 
              class="flex items-center gap-2 hover:text-white transition-colors"
            >
              浏览全部图书
              <ArrowRight class="h-4 w-4" />
            </RouterLink>
          </div>
        </div>
      </div>
      
      <!-- 波浪装饰 -->
      <div class="absolute bottom-0 left-0 right-0">
        <svg 
          viewBox="0 0 1440 120" 
          fill="none" 
          xmlns="http://www.w3.org/2000/svg"
          class="w-full h-auto"
          preserveAspectRatio="none"
        >
          <path 
            d="M0 120L60 110C120 100 240 80 360 70C480 60 600 60 720 65C840 70 960 80 1080 85C1200 90 1320 90 1380 90L1440 90V120H1380C1320 120 1200 120 1080 120C960 120 840 120 720 120C600 120 480 120 360 120C240 120 120 120 60 120H0Z" 
            class="fill-background"
          />
        </svg>
      </div>
    </section>
    
    <!-- 特色功能 -->
    <section class="container px-4 py-20">
      <div class="text-center mb-12">
        <h2 class="text-3xl font-bold mb-4">我们的服务</h2>
        <p class="text-muted-foreground max-w-2xl mx-auto">
          新华书店致力于为读者提供优质的阅读体验和便捷的购书借书服务
        </p>
      </div>
      
      <div class="grid md:grid-cols-2 lg:grid-cols-4 gap-6">
        <Card 
          v-for="feature in features" 
          :key="feature.title"
          class="text-center hover:shadow-lg transition-shadow"
        >
          <CardContent class="pt-8">
            <div class="w-14 h-14 mx-auto mb-4 rounded-full bg-primary/10 flex items-center justify-center">
              <component :is="feature.icon" class="h-7 w-7 text-primary" />
            </div>
            <h3 class="font-semibold text-lg mb-2">{{ feature.title }}</h3>
            <p class="text-sm text-muted-foreground">{{ feature.description }}</p>
          </CardContent>
        </Card>
      </div>
    </section>

    <!-- 智能推荐：猜你喜欢 -->
    <SmartRecommendation />
    
    <!-- CTA Section -->
    <section class="bg-muted/50">
      <div class="container px-4 py-20">
        <div class="max-w-3xl mx-auto text-center">
          <h2 class="text-3xl font-bold mb-4">开始您的阅读之旅</h2>
          <p class="text-muted-foreground mb-8">
            注册账户即可购买或借阅图书，追踪阅读进度，发现更多好书
          </p>
          <div class="flex flex-wrap justify-center gap-4">
            <RouterLink to="/books">
              <Button size="lg">
                浏览图书
              </Button>
            </RouterLink>
            <RouterLink to="/cart">
              <Button variant="outline" size="lg">
                我的购物车
              </Button>
            </RouterLink>
          </div>
        </div>
      </div>
    </section>
  </ReaderLayout>
</template>
