<script setup lang="ts">
import { RouterView } from 'vue-router'
import { onMounted } from 'vue'
import ToastContainer from '@/components/ui/ToastContainer.vue'
import { useAuthStore } from '@/stores/auth'

const authStore = useAuthStore()

// 应用启动时，如果有 token 则获取用户信息
onMounted(async () => {
  if (authStore.token) {
    try {
      await authStore.fetchUserInfo()
    } catch {
      // Token 失效，清除状态
      authStore.logout()
    }
  }
})
</script>

<template>
  <RouterView />
  <ToastContainer />
</template>

<style>
/* 页面切换过渡动画 */
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.2s ease;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}
</style>
