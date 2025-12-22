import api from './client'
import type {
  ResultDto,
  Book,
  BookDto,
  BorrowDto,
  User,
  LoginRequest,
  LoginResponse,
  RegisterRequest,
  GetInfoResponse,
  UpdateInfoRequest,
  UserStatistics,
  AdminStatistics,
  Recommend,
  RecommendRequest,
  RecommendUpdateRequest,
  BookComment,
  BookCommentRequest,
  LogEvent,
  PaginationParams,
  Order,
  CreateOrderRequest,
  OrderStatistics,
  CartItemDto,
  AddToCartRequest,
  UpdateCartQuantityRequest,
  BookRecommendation,
  UserPurchaseStats
} from '@/types'

// ==================== 认证相关 ====================

export const authApi = {
  login: (data: LoginRequest) =>
    api.post<LoginResponse>('/Account/Login', data),
  
  register: (data: RegisterRequest) =>
    api.post('/Account/Register', data),
  
  getInfo: () =>
    api.get<GetInfoResponse>('/Account/GetInfo'),
  
  updateInfo: (data: UpdateInfoRequest) =>
    api.put('/Account/Update', data),
  
  uploadAvatar: (file: File) => {
    const formData = new FormData()
    formData.append('file', file)
    return api.post('/Account/Avatar', formData, {
      headers: { 'Content-Type': 'multipart/form-data' }
    })
  }
}

// ==================== 书籍相关 ====================

export const bookApi = {
  getBooks: (params: PaginationParams = {}) =>
    api.get<ResultDto<Book[]>>('/book', { params }),
  
  getBook: (id: number) =>
    api.get<ResultDto<Book>>(`/book/${id}`),
  
  addBook: (data: BookDto) =>
    api.post<ResultDto<Book>>('/book', data),
  
  updateBook: (data: BookDto) =>
    api.put<ResultDto<Book>>('/book', data),
  
  deleteBook: (id: number) =>
    api.delete<ResultDto<Book>>('/book', { params: { id } }),
  
  // 当前借阅列表（管理员）
  getCurrentBorrows: (params: PaginationParams = {}) =>
    api.get<ResultDto<BorrowDto[]>>('/book/currentborrow', { params }),
  
  // 借阅历史（管理员）
  getBorrowHistory: (params: PaginationParams = {}) =>
    api.get<ResultDto<BorrowDto[]>>('/book/borrowhistory', { params }),
}

// ==================== 用户相关 ====================

export const userApi = {
  // 获取当前用户的借阅
  getCurrentBorrow: (params: PaginationParams = {}) =>
    api.get<ResultDto<BorrowDto[]>>('/user/currentborrow', { params }),
  
  // 获取当前用户的借阅历史
  getBorrowHistory: (params: PaginationParams = {}) =>
    api.get<ResultDto<BorrowDto[]>>('/user/borrowhistory', { params }),
  
  // 借阅书籍
  borrowBook: (bookId: number) =>
    api.post<ResultDto<null>>('/user/borrow', null, { params: { bookId } }),
  
  // 归还书籍
  returnBook: (bookId: number) =>
    api.post<ResultDto<null>>('/user/return', null, { params: { bookId } }),
  
  // 获取用户统计
  getStatistics: () =>
    api.get<ResultDto<UserStatistics>>('/user/statistics'),
  
  // 添加推荐
  addRecommend: (data: RecommendRequest) =>
    api.post<ResultDto<null>>('/user/recommend', data),
}

// ==================== 管理员相关 ====================

export const adminApi = {
  // 获取所有用户
  getUsers: (params: PaginationParams = {}) =>
    api.get<ResultDto<User[]>>('/admin/users', { params }),
  
  // 删除用户
  removeUser: (userId: string) =>
    api.delete<ResultDto<null>>('/admin/user', { params: { userId } }),
  
  // 更新用户角色
  updateUserRole: (userId: string, roles: string[]) =>
    api.put<ResultDto<null>>('/admin/user/role', { userId, roles }),
  
  // 获取管理员统计
  getStatistics: () =>
    api.get<ResultDto<AdminStatistics>>('/admin/statistics'),
  
  // 获取系统设置
  getSettings: () =>
    api.get<ResultDto<Record<string, string>>>('/admin/settings'),
  
  // 更新系统设置
  updateSettings: (settings: Record<string, string>) =>
    api.put<ResultDto<null>>('/admin/settings', settings),
}

// ==================== 推荐相关 ====================

export const recommendApi = {
  // 获取推荐列表
  getRecommends: (params: PaginationParams = {}) =>
    api.get<ResultDto<Recommend[]>>('/Recommend', { params }),
  
  // 处理推荐
  updateRecommend: (data: RecommendUpdateRequest) =>
    api.put<ResultDto<null>>('/Recommend', data),
}

// ==================== 书评相关 ====================

export const commentApi = {
  // 获取书籍评论
  getComments: (bookId: number, params: PaginationParams = {}) =>
    api.get<ResultDto<BookComment[]>>('/BookComment', { params: { ...params, bookId } }),
  
  // 添加评论
  addComment: (data: BookCommentRequest) =>
    api.post<ResultDto<null>>('/BookComment', data),
  
  // 删除评论
  deleteComment: (id: number) =>
    api.delete<ResultDto<null>>('/BookComment', { params: { id } }),
}

// ==================== 日志相关 ====================

export const logApi = {
  getLogs: (params: PaginationParams = {}) =>
    api.get<ResultDto<LogEvent[]>>('/Log', { params }),
}

// ==================== 种子数据 ====================

export const seedApi = {
  seedBooks: () =>
    api.post<ResultDto<null>>('/Seed/Books'),
  
  seedUsers: () =>
    api.post<ResultDto<null>>('/Seed/Users'),
  
  // 更新书籍价格
  updateBookPrices: () =>
    api.put<ResultDto<{ updatedBooks: number; totalBooks: number }>>('/Seed/BookPrices'),
}

// ==================== 购物车相关 ====================

export const cartApi = {
  // 获取购物车
  getCart: () =>
    api.get<ResultDto<CartItemDto[]>>('/cart'),
  
  // 添加到购物车
  addToCart: (data: AddToCartRequest) =>
    api.post<ResultDto<null>>('/cart', data),
  
  // 更新数量
  updateQuantity: (data: UpdateCartQuantityRequest) =>
    api.put<ResultDto<null>>('/cart', data),
  
  // 移除商品
  removeFromCart: (bookId: number) =>
    api.delete<ResultDto<null>>(`/cart/${bookId}`),
  
  // 清空购物车
  clearCart: () =>
    api.delete<ResultDto<null>>('/cart'),
}

// ==================== 订单相关 ====================

export const orderApi = {
  // 创建订单
  createOrder: (data: CreateOrderRequest) =>
    api.post<ResultDto<Order>>('/order', data),
  
  // 获取当前用户的订单列表
  getMyOrders: (params: PaginationParams = {}) =>
    api.get<ResultDto<Order[]>>('/order', { params }),
  
  // 获取订单详情
  getOrder: (id: number) =>
    api.get<ResultDto<Order>>(`/order/${id}`),
  
  // 取消订单
  cancelOrder: (id: number) =>
    api.post<ResultDto<null>>(`/order/${id}/cancel`),
  
  // 确认收货
  confirmOrder: (id: number) =>
    api.post<ResultDto<null>>(`/order/${id}/complete`),
  
  // 模拟支付
  payOrder: (id: number) =>
    api.post<ResultDto<null>>(`/order/${id}/pay`),
  
  // 管理员：获取所有订单
  getAllOrders: (params: PaginationParams = {}) =>
    api.get<ResultDto<Order[]>>('/order/all', { params }),
  
  // 管理员：发货
  shipOrder: (id: number) =>
    api.post<ResultDto<null>>(`/order/${id}/ship`),
  
  // 管理员：获取订单统计
  getStatistics: () =>
    api.get<ResultDto<OrderStatistics>>('/order/statistics'),
}

// ==================== 智能推荐相关 ====================

export const smartRecommendApi = {
  // 获取个性化书籍推荐（猜你喜欢）
  getRecommendations: (count: number = 10) =>
    api.get<ResultDto<BookRecommendation[]>>('/SmartRecommend', { params: { count } }),
  
  // 获取用户购买偏好统计
  getUserStats: () =>
    api.get<ResultDto<UserPurchaseStats[]>>('/SmartRecommend/stats'),
}

// ==================== AI 功能相关 ====================

export const aiApi = {
  // 生成 AI 图书简介
  generateBookSummary: (bookId: number) =>
    api.get<BookAISummary>(`/AI/summary/${bookId}`),
  
  // 分析书籍适合度（需要登录）
  analyzeBookSuitability: (bookId: number) =>
    api.get<BookSuitability>(`/AI/suitability/${bookId}`),
}

// AI 类型定义
export interface BookAISummary {
  success: boolean
  bookId: number
  bookTitle: string
  summary: string
  generatedAt: string
  hasUserContext: boolean
  error?: string
}

export interface BookSuitability {
  success: boolean
  bookId: number
  bookTitle: string
  suitabilityScore: number
  recommendation: string
  analysis: string
  reasons: string[]
  error?: string
}
