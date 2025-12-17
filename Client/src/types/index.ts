// API 响应类型
export interface ResultDto<T> {
  data: T | null
  code: number
  message: string
  pageIndex: number
  pageSize: number
  recordCount: number
}

// 书籍类型
export interface Book {
  id: number
  title: string
  author: string
  publisher: string
  publishedDate: string
  identifier: string
  inboundDate: string
  inventory: number
  borrowed: number
  price: number        // 售价
  originalPrice?: number  // 原价（用于显示折扣）
}

export interface BookDto {
  id?: number
  title: string
  author: string
  publisher: string
  publishedDate: string
  identifier: string
  inventory: number
  price: number
}

// 借阅类型
export interface BorrowDto {
  id: number
  title: string
  author: string
  publisher: string
  borrowDate: string
  borrowDuration: string
  returnDate: string | null
}

// 用户类型
export interface User {
  id: string
  userName: string
  email: string
  phone: string
  roles: string[]
  avatar: string | null
}

export interface LoginRequest {
  account: string
  password: string
}

export interface LoginResponse {
  token: string
  userName: string
  role: string
  avatar: string
}

export interface RegisterRequest {
  userName: string
  password: string
}

// 用户信息响应
export interface GetInfoResponse {
  email: string
  phoneNumber: string
  avatar: string
}

export interface UpdateInfoRequest {
  email?: string
  phone?: string
  avatar?: string
  oldPassword?: string
  newPassword?: string
}

// 用户统计
export interface UserStatistics {
  totalBorrowedBooks: number
  totalReturnedBooks: number
  currentBorrowedBooks: number
  averageBorrowDuration: string
  monthlyBorrowedBooks: Record<string, number>
}

// 管理员统计
export interface AdminStatistics {
  totalHistoryBorrowedBooks: number
  totalCurrentBorrowedBooks: number
  totalUsers: number
  monthlyBorrowedBooks: Record<string, number>
  borrowedBooksByClassification: Record<string, number>
  averageBorrowDuration: string
  unhandledRecommends: number
}

// 推荐类型
export interface Recommend {
  id: number
  title: string
  author: string
  publisher: string
  isbn: string
  userRemark: string
  isProcessed: string
  adminRemark: string | null
  createTime: string
  updateTime: string
  userName: string | null
  adminName: string | null
}

export interface RecommendRequest {
  title: string
  author: string
  publisher: string
  isbn: string
  remark: string
}

export interface RecommendUpdateRequest {
  id: number
  isProcessed: boolean
  adminRemark: string
}

// 书评类型
export interface BookComment {
  id: number
  bookId: number
  userId: string
  userName: string
  content: string
  rating: number
  createTime: string
  avatar: string | null
}

export interface BookCommentRequest {
  bookId: number
  content: string
  rating: number
}

// 日志类型
export interface LogEvent {
  id: number
  timestamp: string
  level: string
  message: string
  exception: string | null
}

// 分页参数
export interface PaginationParams {
  pageIndex?: number
  pageSize?: number
  sortColumn?: string
  sortOrder?: 'ASC' | 'DESC'
  filterQuery?: string
}

// 用户角色
export enum Role {
  User = 'User',
  Moderator = 'Moderator',
  Admin = 'Admin'
}

// ==================== 购物相关类型 ====================

// 购物车商品（本地存储用）
export interface CartItem {
  book: Book
  quantity: number
}

// 购物车商品（后端返回）
export interface CartItemDto {
  bookId: number
  title: string
  author: string
  publisher: string
  identifier: string
  price: number
  originalPrice?: number
  quantity: number
  inventory: number
  addedDate: string
}

// 添加购物车请求
export interface AddToCartRequest {
  bookId: number
  quantity: number
}

// 更新购物车数量请求
export interface UpdateCartQuantityRequest {
  bookId: number
  quantity: number
}

// 订单状态
export enum OrderStatus {
  Pending = 'Pending',           // 待支付
  Paid = 'Paid',                 // 已支付
  Shipped = 'Shipped',           // 已发货
  Delivered = 'Delivered',       // 已送达
  Completed = 'Completed',       // 已完成
  Cancelled = 'Cancelled'        // 已取消
}

// 订单项
export interface OrderItem {
  bookId: number
  title: string
  author: string
  price: number
  quantity: number
}

// 订单
export interface Order {
  id: number
  userId: string
  userName: string
  items: OrderItem[]
  totalAmount: number
  status: OrderStatus
  shippingAddress: string
  contactPhone: string
  createTime: string
  updateTime: string
  payTime: string | null
  shipTime: string | null
  deliverTime: string | null
}

// 创建订单请求
export interface CreateOrderRequest {
  items?: { bookId: number; quantity: number }[]
  shippingAddress?: string
  receiverName?: string
  receiverPhone?: string
  remark?: string
}

// 订单统计（管理员）
export interface OrderStatistics {
  totalOrders: number
  pendingOrders: number
  paidOrders: number
  shippedOrders: number
  completedOrders: number
  cancelledOrders: number
  totalSales: number
  todaySales: number
}
