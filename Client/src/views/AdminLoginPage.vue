<script setup lang="ts">
import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { useToastStore } from '@/stores/toast'
import Input from '@/components/ui/Input.vue'
import Button from '@/components/ui/Button.vue'
import Card from '@/components/ui/Card.vue'
import CardHeader from '@/components/ui/CardHeader.vue'
import CardTitle from '@/components/ui/CardTitle.vue'
import CardDescription from '@/components/ui/CardDescription.vue'
import CardContent from '@/components/ui/CardContent.vue'
import { Lock, User, ShieldCheck, ArrowLeft } from 'lucide-vue-next'

const router = useRouter()
const authStore = useAuthStore()
const toastStore = useToastStore()

const account = ref('')
const password = ref('')
const isLoading = ref(false)

const isFormValid = computed(() => {
  return account.value.trim() && password.value
})

async function handleLogin() {
  if (!isFormValid.value) return
  
  isLoading.value = true
  
  try {
    const success = await authStore.login(account.value, password.value)
    
    if (success) {
      // 检查是否为管理员或版主
      if (authStore.isAdmin || authStore.isModerator) {
        toastStore.success('登录成功')
        // login 方法会自动跳转到管理后台
      } else {
        // 普通用户不允许从管理员入口登录
        toastStore.error('您不是管理员，请使用用户入口登录')
        authStore.logout()
      }
    } else {
      toastStore.error('账号或密码错误')
    }
  } catch (error) {
    toastStore.error('登录失败，请稍后重试')
  } finally {
    isLoading.value = false
  }
}

function goToUserLogin() {
  router.push('/')
}
</script>

<template>
  <div class="min-h-screen bg-gradient-to-br from-library-900 via-library-800 to-library-950 flex items-center justify-center p-4">
    <!-- 装饰背景 -->
    <div class="absolute inset-0 overflow-hidden">
      <div class="absolute top-20 left-20 w-72 h-72 bg-library-500/20 rounded-full blur-3xl" />
      <div class="absolute bottom-20 right-20 w-96 h-96 bg-library-600/20 rounded-full blur-3xl" />
    </div>
    
    <Card class="w-full max-w-md relative z-10 bg-card/95 backdrop-blur shadow-2xl">
      <CardHeader class="text-center pb-2">
        <div class="w-16 h-16 mx-auto mb-4 rounded-full bg-primary/10 flex items-center justify-center">
          <ShieldCheck class="h-8 w-8 text-primary" />
        </div>
        <CardTitle class="text-2xl">管理后台</CardTitle>
        <CardDescription>Administrator Portal</CardDescription>
      </CardHeader>
      
      <CardContent>
        <form @submit.prevent="handleLogin" class="space-y-4">
          <!-- 账号 -->
          <div class="space-y-2">
            <label class="text-sm font-medium">管理员账号</label>
            <div class="relative">
              <User class="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
              <Input
                v-model="account"
                placeholder="请输入用户名或邮箱"
                class="pl-10"
                autocomplete="username"
              />
            </div>
          </div>
          
          <!-- 密码 -->
          <div class="space-y-2">
            <label class="text-sm font-medium">密码</label>
            <div class="relative">
              <Lock class="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
              <Input
                v-model="password"
                type="password"
                placeholder="请输入密码"
                class="pl-10"
                autocomplete="current-password"
              />
            </div>
          </div>
          
          <!-- 登录按钮 -->
          <Button
            type="submit"
            class="w-full"
            :loading="isLoading"
            :disabled="!isFormValid"
          >
            登录管理后台
          </Button>
          
          <!-- 返回用户登录 -->
          <div class="text-center">
            <button
              type="button"
              class="inline-flex items-center gap-1 text-sm text-muted-foreground hover:text-primary transition-colors"
              @click="goToUserLogin"
            >
              <ArrowLeft class="h-4 w-4" />
              返回用户登录
            </button>
          </div>
        </form>
      </CardContent>
    </Card>
  </div>
</template>
