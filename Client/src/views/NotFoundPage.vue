<script setup lang="ts">
import { useRouter } from 'vue-router'
import Button from '@/components/ui/Button.vue'
import { Home, ArrowLeft } from 'lucide-vue-next'

const router = useRouter()

function goHome() {
  router.push('/')
}

function goBack() {
  router.back()
}
</script>

<template>
  <div class="min-h-screen flex items-center justify-center bg-background px-4">
    <div class="text-center">
      <h1 class="text-9xl font-bold text-muted-foreground/30">404</h1>
      <h2 class="text-2xl font-semibold mt-4">页面未找到</h2>
      <p class="text-muted-foreground mt-2 max-w-md mx-auto">
        抱歉，您访问的页面不存在或已被移除。
      </p>
      <div class="flex justify-center gap-4 mt-8">
        <Button variant="outline" @click="goBack">
          <ArrowLeft class="h-4 w-4 mr-2" />
          返回上页
        </Button>
        <Button @click="goHome">
          <Home class="h-4 w-4 mr-2" />
          返回首页
        </Button>
      </div>
    </div>
  </div>
</template>
