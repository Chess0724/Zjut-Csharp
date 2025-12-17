import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import type { Book, CartItem, CartItemDto } from '@/types'
import { cartApi } from '@/api'
import { useToastStore } from './toast'
import { useAuthStore } from './auth'

export const useCartStore = defineStore('cart', () => {
  const toastStore = useToastStore()
  const authStore = useAuthStore()
  
  // 购物车数据 - 本地模式（未登录）和服务端模式（已登录）
  const localItems = ref<CartItem[]>(
    JSON.parse(localStorage.getItem('cart') || '[]')
  )
  const serverItems = ref<CartItemDto[]>([])
  const loading = ref(false)

  // 判断是否使用服务端购物车
  const useServerCart = computed(() => authStore.isLoggedIn)

  // 购物车商品数量
  const itemCount = computed(() => {
    if (useServerCart.value) {
      return serverItems.value.reduce((sum, item) => sum + item.quantity, 0)
    }
    return localItems.value.reduce((sum, item) => sum + item.quantity, 0)
  })

  // 购物车总金额
  const totalAmount = computed(() => {
    if (useServerCart.value) {
      return serverItems.value.reduce((sum, item) => sum + item.price * item.quantity, 0)
    }
    return localItems.value.reduce((sum, item) => sum + (item.book.price || 39.9) * item.quantity, 0)
  })

  // 获取购物车列表（用于展示）
  const items = computed(() => {
    if (useServerCart.value) {
      return serverItems.value
    }
    return localItems.value
  })

  // 保存到 localStorage（本地模式）
  function saveToStorage() {
    localStorage.setItem('cart', JSON.stringify(localItems.value))
  }

  // 从服务端获取购物车
  async function fetchCart() {
    if (!useServerCart.value) return
    
    loading.value = true
    try {
      const response = await cartApi.getCart()
      if (response.data.code === 0) {
        serverItems.value = response.data.data || []
      }
    } catch (error) {
      console.error('获取购物车失败:', error)
    } finally {
      loading.value = false
    }
  }

  // 添加商品到购物车
  async function addItem(book: Book, quantity: number = 1) {
    if (useServerCart.value) {
      // 服务端模式
      try {
        const response = await cartApi.addToCart({ bookId: book.id, quantity })
        if (response.data.code === 0) {
          toastStore.success(`已添加 "${book.title}" 到购物车`)
          await fetchCart()
          return true
        } else {
          toastStore.error(response.data.message)
          return false
        }
      } catch {
        toastStore.error('添加失败，请稍后再试')
        return false
      }
    } else {
      // 本地模式
      const existingItem = localItems.value.find(item => item.book.id === book.id)
      
      if (existingItem) {
        if (existingItem.quantity + quantity > book.inventory - book.borrowed) {
          toastStore.error('库存不足')
          return false
        }
        existingItem.quantity += quantity
      } else {
        if (quantity > book.inventory - book.borrowed) {
          toastStore.error('库存不足')
          return false
        }
        localItems.value.push({ book, quantity })
      }
      
      saveToStorage()
      toastStore.success(`已添加 "${book.title}" 到购物车`)
      return true
    }
  }

  // 更新商品数量
  async function updateQuantity(bookId: number, quantity: number) {
    if (useServerCart.value) {
      try {
        const response = await cartApi.updateQuantity({ bookId, quantity })
        if (response.data.code === 0) {
          await fetchCart()
        } else {
          toastStore.error(response.data.message)
        }
      } catch {
        toastStore.error('更新失败')
      }
    } else {
      const item = localItems.value.find(item => item.book.id === bookId)
      if (item) {
        if (quantity <= 0) {
          removeItem(bookId)
        } else if (quantity > item.book.inventory - item.book.borrowed) {
          toastStore.error('库存不足')
        } else {
          item.quantity = quantity
          saveToStorage()
        }
      }
    }
  }

  // 移除商品
  async function removeItem(bookId: number) {
    if (useServerCart.value) {
      try {
        const response = await cartApi.removeFromCart(bookId)
        if (response.data.code === 0) {
          toastStore.success('已从购物车移除')
          await fetchCart()
        } else {
          toastStore.error(response.data.message)
        }
      } catch {
        toastStore.error('移除失败')
      }
    } else {
      const index = localItems.value.findIndex(item => item.book.id === bookId)
      if (index > -1) {
        const item = localItems.value[index]
        localItems.value.splice(index, 1)
        saveToStorage()
        toastStore.success(`已从购物车移除 "${item.book.title}"`)
      }
    }
  }

  // 清空购物车
  async function clearCart() {
    if (useServerCart.value) {
      try {
        await cartApi.clearCart()
        serverItems.value = []
      } catch {
        toastStore.error('清空失败')
      }
    } else {
      localItems.value = []
      saveToStorage()
    }
  }

  // 检查商品是否在购物车中
  function isInCart(bookId: number) {
    if (useServerCart.value) {
      return serverItems.value.some(item => item.bookId === bookId)
    }
    return localItems.value.some(item => item.book.id === bookId)
  }

  // 获取商品在购物车中的数量
  function getQuantity(bookId: number) {
    if (useServerCart.value) {
      const item = serverItems.value.find(item => item.bookId === bookId)
      return item?.quantity || 0
    }
    const item = localItems.value.find(item => item.book.id === bookId)
    return item?.quantity || 0
  }

  return {
    items,
    localItems,
    serverItems,
    loading,
    itemCount,
    totalAmount,
    useServerCart,
    fetchCart,
    addItem,
    updateQuantity,
    removeItem,
    clearCart,
    isInCart,
    getQuantity
  }
})
