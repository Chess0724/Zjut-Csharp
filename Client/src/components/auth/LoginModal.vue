<script setup lang="ts">
import { ref, computed } from 'vue'
import { useAuthStore } from '@/stores/auth'
import { useToastStore } from '@/stores/toast'
import Dialog from '@/components/ui/Dialog.vue'
import Input from '@/components/ui/Input.vue'
import Button from '@/components/ui/Button.vue'
import { Lock, User } from 'lucide-vue-next'

const authStore = useAuthStore()
const toastStore = useToastStore()

const mode = ref<'login' | 'register'>('login')
const userName = ref('')
const password = ref('')
const confirmPassword = ref('')

const isFormValid = computed(() => {
  if (mode.value === 'login') {
    return userName.value.trim() && password.value
  }
  return userName.value.trim() && password.value && password.value === confirmPassword.value
})

async function handleSubmit() {
  if (!isFormValid.value) return
  
  if (mode.value === 'login') {
    const success = await authStore.login(userName.value, password.value)
    if (success) {
      toastStore.success('登录成功')
      resetForm()
    }
  } else {
    if (password.value !== confirmPassword.value) {
      toastStore.error('两次输入的密码不一致')
      return
    }
    const success = await authStore.register(userName.value, password.value)
    if (success) {
      toastStore.success('注册成功，请登录')
      mode.value = 'login'
      password.value = ''
    }
  }
}

function switchMode() {
  mode.value = mode.value === 'login' ? 'register' : 'login'
  resetForm()
}

function resetForm() {
  userName.value = ''
  password.value = ''
  confirmPassword.value = ''
}

function handleClose() {
  authStore.closeLoginModal()
  resetForm()
  mode.value = 'login'
}
</script>

<template>
  <Dialog
    :open="authStore.showLoginModal"
    :title="mode === 'login' ? '登录' : '注册'"
    :description="mode === 'login' ? '登录以继续使用图书馆服务' : '创建新账户以使用图书馆服务'"
    class="max-w-md"
    @update:open="handleClose"
  >
    <form @submit.prevent="handleSubmit" class="space-y-4">
      <!-- 用户名 -->
      <div class="space-y-2">
        <label class="text-sm font-medium">用户名</label>
        <div class="relative">
          <User class="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
          <Input
            v-model="userName"
            placeholder="请输入用户名"
            class="pl-10"
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
          />
        </div>
      </div>
      
      <!-- 确认密码（仅注册） -->
      <div v-if="mode === 'register'" class="space-y-2">
        <label class="text-sm font-medium">确认密码</label>
        <div class="relative">
          <Lock class="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
          <Input
            v-model="confirmPassword"
            type="password"
            placeholder="请再次输入密码"
            class="pl-10"
          />
        </div>
      </div>
      
      <!-- 提交按钮 -->
      <Button
        type="submit"
        class="w-full"
        :loading="authStore.isLoading"
        :disabled="!isFormValid"
      >
        {{ mode === 'login' ? '登录' : '注册' }}
      </Button>
      
      <!-- 切换模式 -->
      <div class="text-center text-sm">
        <span class="text-muted-foreground">
          {{ mode === 'login' ? '还没有账户？' : '已有账户？' }}
        </span>
        <button
          type="button"
          class="text-primary hover:underline ml-1"
          @click="switchMode"
        >
          {{ mode === 'login' ? '立即注册' : '立即登录' }}
        </button>
      </div>
    </form>
  </Dialog>
</template>
