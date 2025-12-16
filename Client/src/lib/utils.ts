import { type ClassValue, clsx } from 'clsx'
import { twMerge } from 'tailwind-merge'

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs))
}

// 生成书籍封面背景色（根据书名生成一致的颜色）
export function getBookCoverColor(title: string): string {
  const colors = [
    'from-teal-500 to-teal-700',
    'from-blue-500 to-blue-700',
    'from-indigo-500 to-indigo-700',
    'from-purple-500 to-purple-700',
    'from-rose-500 to-rose-700',
    'from-amber-500 to-amber-700',
    'from-emerald-500 to-emerald-700',
    'from-cyan-500 to-cyan-700',
    'from-violet-500 to-violet-700',
    'from-fuchsia-500 to-fuchsia-700',
  ]
  
  let hash = 0
  for (let i = 0; i < title.length; i++) {
    hash = title.charCodeAt(i) + ((hash << 5) - hash)
  }
  
  return colors[Math.abs(hash) % colors.length]
}

// 格式化日期
export function formatDate(date: string | Date): string {
  const d = new Date(date)
  return d.toLocaleDateString('zh-CN', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit'
  })
}

// 格式化相对时间
export function formatRelativeTime(date: string | Date): string {
  const d = new Date(date)
  const now = new Date()
  const diff = now.getTime() - d.getTime()
  
  const minutes = Math.floor(diff / 60000)
  const hours = Math.floor(diff / 3600000)
  const days = Math.floor(diff / 86400000)
  
  if (minutes < 1) return '刚刚'
  if (minutes < 60) return `${minutes}分钟前`
  if (hours < 24) return `${hours}小时前`
  if (days < 30) return `${days}天前`
  
  return formatDate(date)
}

// 计算借阅剩余天数（假设借阅期限为30天）
export function getBorrowRemainingDays(borrowDate: string, maxDays: number = 30): number {
  const borrow = new Date(borrowDate)
  const now = new Date()
  const diff = now.getTime() - borrow.getTime()
  const daysPassed = Math.floor(diff / 86400000)
  return Math.max(0, maxDays - daysPassed)
}

// 获取借阅状态颜色
export function getBorrowStatusColor(remainingDays: number): string {
  if (remainingDays <= 3) return 'text-red-500'
  if (remainingDays <= 7) return 'text-amber-500'
  return 'text-green-500'
}

// 截断文本
export function truncate(text: string, length: number): string {
  if (text.length <= length) return text
  return text.slice(0, length) + '...'
}

// 防抖函数
export function debounce<T extends (...args: unknown[]) => unknown>(
  fn: T,
  delay: number
): (...args: Parameters<T>) => void {
  let timeoutId: ReturnType<typeof setTimeout>
  
  return function (...args: Parameters<T>) {
    clearTimeout(timeoutId)
    timeoutId = setTimeout(() => fn(...args), delay)
  }
}

// 中图法分类名称映射
export const classificationNames: Record<string, string> = {
  'A': '马克思主义、列宁主义、毛泽东思想、邓小平理论',
  'B': '哲学、宗教',
  'C': '社会科学总论',
  'D': '政治、法律',
  'E': '军事',
  'F': '经济',
  'G': '文化、科学、教育、体育',
  'H': '语言、文字',
  'I': '文学',
  'J': '艺术',
  'K': '历史、地理',
  'N': '自然科学总论',
  'O': '数理科学和化学',
  'P': '天文学、地球科学',
  'Q': '生物科学',
  'R': '医药、卫生',
  'S': '农业科学',
  'T': '工业技术',
  'U': '交通运输',
  'V': '航空、航天',
  'X': '环境科学、安全科学',
  'Z': '综合性图书'
}

// 根据索书号获取分类名
export function getClassificationName(identifier: string): string {
  const firstChar = identifier.charAt(0).toUpperCase()
  return classificationNames[firstChar] || '其他'
}
