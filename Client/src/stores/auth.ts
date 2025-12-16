import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import type { Role } from '@/types'
import { authApi } from '@/api'
import router from '@/router'

// 用户信息类型
interface UserInfo {
  userName: string
  role: string
  avatar: string
  email?: string
  phoneNumber?: string
}

export const useAuthStore = defineStore('auth', () => {
  const token = ref<string | null>(localStorage.getItem('token'))
  const user = ref<UserInfo | null>(JSON.parse(localStorage.getItem('user') || 'null'))
  const isLoading = ref(false)
  const showLoginModal = ref(false)

  const isLoggedIn = computed(() => !!token.value)
  
  const isAdmin = computed(() => 
    user.value?.role === 'Admin'
  )
  
  const isModerator = computed(() => 
    user.value?.role === 'Moderator' || user.value?.role === 'Admin'
  )

  const hasRole = (role: Role | string) => {
    if (!user.value?.role) return false
    // Admin 拥有所有权限
    if (user.value.role === 'Admin') return true
    // Moderator 拥有 Moderator 和 User 权限
    if (user.value.role === 'Moderator') {
      return role === 'Moderator' || role === 'User'
    }
    return user.value.role === role
  }

  async function login(account: string, password: string) {
    isLoading.value = true
    try {
      const response = await authApi.login({ account, password })
      const { token: newToken, userName, role, avatar } = response.data
      
      token.value = newToken
      user.value = { userName, role, avatar }
      
      localStorage.setItem('token', newToken)
      localStorage.setItem('user', JSON.stringify(user.value))
      
      showLoginModal.value = false
      return true
    } catch {
      return false
    } finally {
      isLoading.value = false
    }
  }

  async function register(userName: string, password: string) {
    isLoading.value = true
    try {
      await authApi.register({ userName, password })
      return true
    } catch {
      return false
    } finally {
      isLoading.value = false
    }
  }

  async function fetchUserInfo() {
    if (!token.value) return
    try {
      const response = await authApi.getInfo()
      // 合并额外的用户信息（如 email, phoneNumber）
      if (user.value) {
        user.value = { ...user.value, ...response.data }
        localStorage.setItem('user', JSON.stringify(user.value))
      }
    } catch {
      // 如果获取失败，可能token过期，但不强制登出
      console.warn('Failed to fetch user info')
    }
  }

  function logout() {
    token.value = null
    user.value = null
    localStorage.removeItem('token')
    localStorage.removeItem('user')
    router.push('/')
  }

  function openLoginModal() {
    showLoginModal.value = true
  }

  function closeLoginModal() {
    showLoginModal.value = false
  }

  return {
    token,
    user,
    isLoading,
    isLoggedIn,
    isAdmin,
    isModerator,
    showLoginModal,
    hasRole,
    login,
    register,
    logout,
    fetchUserInfo,
    openLoginModal,
    closeLoginModal
  }
})
