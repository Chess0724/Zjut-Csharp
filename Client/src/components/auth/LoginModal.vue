<script setup lang="ts">
import { ref, computed, onUnmounted } from 'vue'
import { useAuthStore } from '@/stores/auth'
import { useToastStore } from '@/stores/toast'
import { authApi } from '@/api'
import Dialog from '@/components/ui/Dialog.vue'
import Input from '@/components/ui/Input.vue'
import Button from '@/components/ui/Button.vue'
import { Lock, User, Mail, ShieldCheck, KeyRound } from 'lucide-vue-next'

const authStore = useAuthStore()
const toastStore = useToastStore()

const mode = ref<'login' | 'register' | 'forgot'>('login')
const userName = ref('')
const password = ref('')
const confirmPassword = ref('')
const email = ref('')
const verificationCode = ref('')
const newPassword = ref('')
const confirmNewPassword = ref('')

// 验证码倒计时
const countdown = ref(0)
let countdownTimer: ReturnType<typeof setInterval> | null = null

// 是否正在发送验证码
const sendingCode = ref(false)

// 是否正在重置密码
const resettingPassword = ref(false)

// 邮箱格式验证
const isEmailValid = computed(() => {
  const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
  return emailPattern.test(email.value)
})

// 是否可以发送验证码
const canSendCode = computed(() => {
  return isEmailValid.value && countdown.value === 0 && !sendingCode.value
})

const isFormValid = computed(() => {
  if (mode.value === 'login') {
    return userName.value.trim() && password.value
  }
  if (mode.value === 'forgot') {
    return isEmailValid.value && 
           verificationCode.value.trim() && 
           newPassword.value.length >= 6 &&
           newPassword.value === confirmNewPassword.value
  }
  return userName.value.trim() && 
         password.value && 
         password.value.length >= 6 &&
         password.value === confirmPassword.value &&
         isEmailValid.value &&
         verificationCode.value.trim()
})

const dialogTitle = computed(() => {
  switch (mode.value) {
    case 'login': return '登录'
    case 'register': return '注册'
    case 'forgot': return '重置密码'
  }
})

const dialogDescription = computed(() => {
  switch (mode.value) {
    case 'login': return '登录以继续使用图书馆服务'
    case 'register': return '创建新账户以使用图书馆服务'
    case 'forgot': return '通过邮箱验证重置您的密码'
  }
})

async function sendCode() {
  if (!canSendCode.value) return
  
  sendingCode.value = true
  
  try {
    if (mode.value === 'forgot') {
      // 发送密码重置验证码
      await authApi.sendResetCode({ email: email.value })
      toastStore.success('验证码已发送，请查收邮箱')
    } else {
      // 发送注册验证码
      const result = await authStore.sendVerificationCode(email.value)
      if (result.success) {
        toastStore.success('验证码已发送，请查收邮箱')
      } else {
        toastStore.error(result.message)
        sendingCode.value = false
        return
      }
    }
    
    // 开始倒计时
    countdown.value = 60
    countdownTimer = setInterval(() => {
      countdown.value--
      if (countdown.value <= 0) {
        if (countdownTimer) {
          clearInterval(countdownTimer)
          countdownTimer = null
        }
      }
    }, 1000)
  } catch (error: any) {
    const message = error.response?.data || '发送验证码失败'
    toastStore.error(message)
  } finally {
    sendingCode.value = false
  }
}

async function handleSubmit() {
  if (!isFormValid.value) return
  
  if (mode.value === 'login') {
    const success = await authStore.login(userName.value, password.value)
    if (success) {
      toastStore.success('登录成功')
      resetForm()
    }
  } else if (mode.value === 'forgot') {
    // 重置密码
    resettingPassword.value = true
    try {
      await authApi.resetPassword({
        email: email.value,
        verificationCode: verificationCode.value,
        newPassword: newPassword.value
      })
      toastStore.success('密码重置成功，请使用新密码登录')
      mode.value = 'login'
      resetForm()
    } catch (error: any) {
      const message = error.response?.data || '密码重置失败'
      toastStore.error(message)
    } finally {
      resettingPassword.value = false
    }
  } else {
    if (password.value !== confirmPassword.value) {
      toastStore.error('两次输入的密码不一致')
      return
    }
    if (password.value.length < 6) {
      toastStore.error('密码长度不能少于6位')
      return
    }
    const success = await authStore.register(userName.value, password.value, email.value, verificationCode.value)
    if (success) {
      toastStore.success('注册成功，请登录')
      mode.value = 'login'
      password.value = ''
      confirmPassword.value = ''
      email.value = ''
      verificationCode.value = ''
    } else {
      toastStore.error('注册失败，请检查验证码是否正确')
    }
  }
}

function switchMode() {
  mode.value = mode.value === 'login' ? 'register' : 'login'
  resetForm()
}

function goToForgotPassword() {
  mode.value = 'forgot'
  resetForm()
}

function backToLogin() {
  mode.value = 'login'
  resetForm()
}

function resetForm() {
  userName.value = ''
  password.value = ''
  confirmPassword.value = ''
  email.value = ''
  verificationCode.value = ''
  newPassword.value = ''
  confirmNewPassword.value = ''
}

function handleClose() {
  authStore.closeLoginModal()
  resetForm()
  mode.value = 'login'
}

onUnmounted(() => {
  if (countdownTimer) {
    clearInterval(countdownTimer)
  }
})
</script>

<template>
  <Dialog
    :open="authStore.showLoginModal"
    :title="dialogTitle"
    :description="dialogDescription"
    class="max-w-md"
    @update:open="handleClose"
  >
    <form @submit.prevent="handleSubmit" class="space-y-4">
      <!-- 用户名（登录和注册） -->
      <div v-if="mode !== 'forgot'" class="space-y-2">
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

      <!-- 邮箱（注册和忘记密码） -->
      <div v-if="mode === 'register' || mode === 'forgot'" class="space-y-2">
        <label class="text-sm font-medium">邮箱</label>
        <div class="flex gap-2">
          <div class="relative flex-1">
            <Mail class="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
            <Input
              v-model="email"
              type="email"
              placeholder="请输入邮箱"
              class="pl-10"
            />
          </div>
          <Button
            type="button"
            variant="outline"
            :disabled="!canSendCode"
            :loading="sendingCode"
            @click="sendCode"
            class="whitespace-nowrap min-w-[100px]"
          >
            {{ countdown > 0 ? `${countdown}s` : '发送验证码' }}
          </Button>
        </div>
      </div>

      <!-- 验证码（注册和忘记密码） -->
      <div v-if="mode === 'register' || mode === 'forgot'" class="space-y-2">
        <label class="text-sm font-medium">验证码</label>
        <div class="relative">
          <ShieldCheck class="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
          <Input
            v-model="verificationCode"
            placeholder="请输入6位验证码"
            maxlength="6"
            class="pl-10"
          />
        </div>
      </div>
      
      <!-- 密码（登录和注册） -->
      <div v-if="mode !== 'forgot'" class="space-y-2">
        <label class="text-sm font-medium">密码</label>
        <div class="relative">
          <Lock class="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
          <Input
            v-model="password"
            type="password"
            :placeholder="mode === 'register' ? '请输入密码（至少6位）' : '请输入密码'"
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

      <!-- 新密码（仅忘记密码） -->
      <div v-if="mode === 'forgot'" class="space-y-2">
        <label class="text-sm font-medium">新密码</label>
        <div class="relative">
          <KeyRound class="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
          <Input
            v-model="newPassword"
            type="password"
            placeholder="请输入新密码（至少6位）"
            class="pl-10"
          />
        </div>
      </div>

      <!-- 确认新密码（仅忘记密码） -->
      <div v-if="mode === 'forgot'" class="space-y-2">
        <label class="text-sm font-medium">确认新密码</label>
        <div class="relative">
          <KeyRound class="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
          <Input
            v-model="confirmNewPassword"
            type="password"
            placeholder="请再次输入新密码"
            class="pl-10"
          />
        </div>
      </div>
      
      <!-- 提交按钮 -->
      <Button
        type="submit"
        class="w-full"
        :loading="authStore.isLoading || resettingPassword"
        :disabled="!isFormValid"
      >
        {{ mode === 'login' ? '登录' : mode === 'register' ? '注册' : '重置密码' }}
      </Button>

      <!-- 忘记密码链接（仅登录模式） -->
      <div v-if="mode === 'login'" class="text-center">
        <button
          type="button"
          class="text-sm text-muted-foreground hover:text-primary transition-colors"
          @click="goToForgotPassword"
        >
          忘记密码？
        </button>
      </div>
      
      <!-- 切换模式 -->
      <div class="text-center text-sm">
        <template v-if="mode === 'forgot'">
          <button
            type="button"
            class="text-primary hover:underline"
            @click="backToLogin"
          >
            返回登录
          </button>
        </template>
        <template v-else>
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
        </template>
      </div>
    </form>
  </Dialog>
</template>
