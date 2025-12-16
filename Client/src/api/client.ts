import axios, { type AxiosInstance, type AxiosError, type InternalAxiosRequestConfig } from 'axios'
import { useAuthStore } from '@/stores/auth'
import { useToastStore } from '@/stores/toast'

const API_BASE_URL = '/api'

// 创建 axios 实例
const api: AxiosInstance = axios.create({
  baseURL: API_BASE_URL,
  timeout: 15000,
  headers: {
    'Content-Type': 'application/json',
  },
})

// 请求拦截器 - 添加 JWT Token
api.interceptors.request.use(
  (config: InternalAxiosRequestConfig) => {
    const authStore = useAuthStore()
    if (authStore.token) {
      config.headers.Authorization = `Bearer ${authStore.token}`
    }
    return config
  },
  (error: AxiosError) => {
    return Promise.reject(error)
  }
)

// 响应拦截器 - 处理错误
api.interceptors.response.use(
  (response) => {
    return response
  },
  (error: AxiosError) => {
    const toastStore = useToastStore()
    
    if (error.response) {
      const status = error.response.status
      
      switch (status) {
        case 401:
          // Token 过期或无效
          const authStore = useAuthStore()
          authStore.logout()
          toastStore.error('登录已过期，请重新登录')
          break
        case 403:
          toastStore.error('没有权限执行此操作')
          break
        case 404:
          toastStore.error('请求的资源不存在')
          break
        case 500:
          toastStore.error('服务器内部错误')
          break
        default:
          const message = (error.response.data as string) || '请求失败'
          toastStore.error(message)
      }
    } else if (error.request) {
      toastStore.error('网络连接失败，请检查网络')
    } else {
      toastStore.error('请求配置错误')
    }
    
    return Promise.reject(error)
  }
)

export default api
