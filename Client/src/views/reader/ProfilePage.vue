<script setup lang="ts">
import { ref, onMounted } from 'vue'
import ReaderLayout from '@/layouts/ReaderLayout.vue'
import Card from '@/components/ui/Card.vue'
import CardHeader from '@/components/ui/CardHeader.vue'
import CardTitle from '@/components/ui/CardTitle.vue'
import CardContent from '@/components/ui/CardContent.vue'
import Input from '@/components/ui/Input.vue'
import Button from '@/components/ui/Button.vue'
import Avatar from '@/components/ui/Avatar.vue'
import Badge from '@/components/ui/Badge.vue'
import { authApi } from '@/api'
import { useAuthStore } from '@/stores/auth'
import { useToastStore } from '@/stores/toast'
import { User, Mail, Phone, Lock, Camera } from 'lucide-vue-next'

const authStore = useAuthStore()
const toastStore = useToastStore()

const loading = ref(false)
const form = ref({
  email: '',
  phone: '',
  oldPassword: '',
  newPassword: '',
  confirmPassword: ''
})

async function handleUpdate() {
  // 验证密码
  if (form.value.newPassword && form.value.newPassword !== form.value.confirmPassword) {
    toastStore.error('两次输入的密码不一致')
    return
  }
  
  loading.value = true
  try {
    const updateData: Record<string, string> = {}
    if (form.value.email) updateData.email = form.value.email
    if (form.value.phone) updateData.phone = form.value.phone
    if (form.value.oldPassword && form.value.newPassword) {
      updateData.oldPassword = form.value.oldPassword
      updateData.newPassword = form.value.newPassword
    }
    
    await authApi.updateInfo(updateData)
    toastStore.success('信息更新成功')
    await authStore.fetchUserInfo()
    
    // 清空密码字段
    form.value.oldPassword = ''
    form.value.newPassword = ''
    form.value.confirmPassword = ''
  } catch {
    toastStore.error('更新失败，请稍后再试')
  } finally {
    loading.value = false
  }
}

async function handleAvatarUpload(event: Event) {
  const input = event.target as HTMLInputElement
  if (!input.files?.length) return
  
  const file = input.files[0]
  if (file.size > 2 * 1024 * 1024) {
    toastStore.error('头像文件不能超过2MB')
    return
  }
  
  try {
    await authApi.uploadAvatar(file)
    toastStore.success('头像更新成功')
    await authStore.fetchUserInfo()
  } catch {
    toastStore.error('头像上传失败')
  }
}

onMounted(() => {
  if (authStore.user) {
    form.value.email = authStore.user.email || ''
    form.value.phone = authStore.user.phoneNumber || ''
  }
})
</script>

<template>
  <ReaderLayout>
    <div class="container px-4 py-8 max-w-4xl">
      <!-- 页面标题 -->
      <div class="mb-8">
        <h1 class="text-3xl font-bold mb-2">个人中心</h1>
        <p class="text-muted-foreground">管理您的账户信息</p>
      </div>
      
      <div class="grid gap-6">
        <!-- 头像和基本信息 -->
        <Card>
          <CardContent class="p-6">
            <div class="flex flex-col sm:flex-row items-center gap-6">
              <!-- 头像 -->
              <div class="relative">
                <Avatar 
                  :src="authStore.user?.avatar" 
                  :fallback="authStore.user?.userName || 'U'"
                  size="xl"
                  class="h-24 w-24"
                />
                <label 
                  class="absolute bottom-0 right-0 w-8 h-8 bg-primary rounded-full flex items-center justify-center cursor-pointer hover:bg-primary/90 transition-colors"
                >
                  <Camera class="h-4 w-4 text-primary-foreground" />
                  <input 
                    type="file" 
                    accept="image/*" 
                    class="hidden"
                    @change="handleAvatarUpload"
                  />
                </label>
              </div>
              
              <!-- 用户信息 -->
              <div class="text-center sm:text-left">
                <h2 class="text-2xl font-bold">{{ authStore.user?.userName }}</h2>
                <p class="text-muted-foreground">{{ authStore.user?.email }}</p>
                <div class="flex flex-wrap gap-2 mt-2 justify-center sm:justify-start">
                  <Badge 
                    :variant="authStore.user?.role === 'Admin' ? 'destructive' : authStore.user?.role === 'Moderator' ? 'default' : 'secondary'"
                  >
                    {{ authStore.user?.role }}
                  </Badge>
                </div>
              </div>
            </div>
          </CardContent>
        </Card>
        
        <!-- 编辑信息 -->
        <Card>
          <CardHeader>
            <CardTitle>编辑资料</CardTitle>
          </CardHeader>
          <CardContent>
            <form @submit.prevent="handleUpdate" class="space-y-6">
              <div class="grid gap-4 sm:grid-cols-2">
                <div class="space-y-2">
                  <label class="text-sm font-medium">用户名</label>
                  <div class="relative">
                    <User class="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
                    <Input
                      :model-value="authStore.user?.userName"
                      disabled
                      class="pl-10"
                    />
                  </div>
                  <p class="text-xs text-muted-foreground">用户名不可修改</p>
                </div>
                
                <div class="space-y-2">
                  <label class="text-sm font-medium">邮箱</label>
                  <div class="relative">
                    <Mail class="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
                    <Input
                      v-model="form.email"
                      type="email"
                      placeholder="请输入邮箱"
                      class="pl-10"
                    />
                  </div>
                </div>
                
                <div class="space-y-2 sm:col-span-2">
                  <label class="text-sm font-medium">电话</label>
                  <div class="relative">
                    <Phone class="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
                    <Input
                      v-model="form.phone"
                      placeholder="请输入电话号码"
                      class="pl-10"
                    />
                  </div>
                </div>
              </div>
              
              <hr />
              
              <div>
                <h3 class="text-lg font-medium mb-4">修改密码</h3>
                <div class="grid gap-4 sm:grid-cols-3">
                  <div class="space-y-2">
                    <label class="text-sm font-medium">当前密码</label>
                    <div class="relative">
                      <Lock class="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
                      <Input
                        v-model="form.oldPassword"
                        type="password"
                        placeholder="当前密码"
                        class="pl-10"
                      />
                    </div>
                  </div>
                  
                  <div class="space-y-2">
                    <label class="text-sm font-medium">新密码</label>
                    <div class="relative">
                      <Lock class="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
                      <Input
                        v-model="form.newPassword"
                        type="password"
                        placeholder="新密码"
                        class="pl-10"
                      />
                    </div>
                  </div>
                  
                  <div class="space-y-2">
                    <label class="text-sm font-medium">确认密码</label>
                    <div class="relative">
                      <Lock class="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
                      <Input
                        v-model="form.confirmPassword"
                        type="password"
                        placeholder="确认新密码"
                        class="pl-10"
                      />
                    </div>
                  </div>
                </div>
              </div>
              
              <div class="flex justify-end">
                <Button type="submit" :loading="loading">
                  保存更改
                </Button>
              </div>
            </form>
          </CardContent>
        </Card>
      </div>
    </div>
  </ReaderLayout>
</template>
