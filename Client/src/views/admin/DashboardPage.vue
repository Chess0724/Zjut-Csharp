<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import Card from '@/components/ui/Card.vue'
import CardHeader from '@/components/ui/CardHeader.vue'
import CardTitle from '@/components/ui/CardTitle.vue'
import CardContent from '@/components/ui/CardContent.vue'
import Button from '@/components/ui/Button.vue'
import { adminApi, seedApi } from '@/api'
import type { AdminStatistics } from '@/types'
import { classificationNames } from '@/lib/utils'
import { useToastStore } from '@/stores/toast'
import { 
  BookOpen, 
  Users, 
  TrendingUp, 
  Clock, 
  MessageSquare,
  ArrowUpRight,
  ArrowDownRight,
  RefreshCw,
  DollarSign
} from 'lucide-vue-next'
import * as echarts from 'echarts'

const toastStore = useToastStore()
const statistics = ref<AdminStatistics | null>(null)
const loading = ref(true)
const updatingPrices = ref(false)

const monthlyChartRef = ref<HTMLDivElement | null>(null)
const classificationChartRef = ref<HTMLDivElement | null>(null)

async function fetchStatistics() {
  loading.value = true
  try {
    const response = await adminApi.getStatistics()
    if (response.data.code === 0 && response.data.data) {
      statistics.value = response.data.data
      initCharts()
    }
  } catch (error) {
    console.error('Failed to fetch statistics:', error)
  } finally {
    loading.value = false
  }
}

function initCharts() {
  if (!statistics.value) return
  
  // 月度借阅趋势图
  if (monthlyChartRef.value) {
    const monthlyChart = echarts.init(monthlyChartRef.value)
    const monthlyData = Object.entries(statistics.value.monthlyBorrowedBooks)
    
    monthlyChart.setOption({
      tooltip: {
        trigger: 'axis',
        axisPointer: { type: 'shadow' }
      },
      grid: {
        left: '3%',
        right: '4%',
        bottom: '3%',
        containLabel: true
      },
      xAxis: {
        type: 'category',
        data: monthlyData.map(([month]) => month),
        axisLine: { lineStyle: { color: '#e5e7eb' } },
        axisLabel: { color: '#6b7280' }
      },
      yAxis: {
        type: 'value',
        axisLine: { show: false },
        axisTick: { show: false },
        splitLine: { lineStyle: { color: '#f3f4f6' } },
        axisLabel: { color: '#6b7280' }
      },
      series: [{
        name: '借阅数',
        type: 'bar',
        data: monthlyData.map(([, count]) => count),
        itemStyle: {
          color: new echarts.graphic.LinearGradient(0, 0, 0, 1, [
            { offset: 0, color: '#14b8a6' },
            { offset: 1, color: '#0d9488' }
          ]),
          borderRadius: [4, 4, 0, 0]
        },
        emphasis: {
          itemStyle: {
            color: new echarts.graphic.LinearGradient(0, 0, 0, 1, [
              { offset: 0, color: '#2dd4bf' },
              { offset: 1, color: '#14b8a6' }
            ])
          }
        }
      }]
    })
    
    // 响应式
    window.addEventListener('resize', () => monthlyChart.resize())
  }
  
  // 分类占比图
  if (classificationChartRef.value) {
    const classChart = echarts.init(classificationChartRef.value)
    const classData = Object.entries(statistics.value.borrowedBooksByClassification)
      .map(([key, value]) => ({
        name: classificationNames[key] || key,
        value
      }))
      .sort((a, b) => b.value - a.value)
      .slice(0, 10)
    
    classChart.setOption({
      tooltip: {
        trigger: 'item',
        formatter: '{b}: {c} ({d}%)'
      },
      legend: {
        type: 'scroll',
        orient: 'vertical',
        right: 10,
        top: 20,
        bottom: 20,
        textStyle: { color: '#6b7280' }
      },
      series: [{
        name: '借阅分布',
        type: 'pie',
        radius: ['45%', '70%'],
        center: ['35%', '50%'],
        avoidLabelOverlap: false,
        itemStyle: {
          borderRadius: 4,
          borderColor: '#fff',
          borderWidth: 2
        },
        label: { show: false },
        emphasis: {
          label: {
            show: true,
            fontSize: 14,
            fontWeight: 'bold'
          }
        },
        labelLine: { show: false },
        data: classData,
        color: [
          '#14b8a6', '#3b82f6', '#8b5cf6', '#ec4899', '#f97316',
          '#eab308', '#22c55e', '#06b6d4', '#6366f1', '#ef4444'
        ]
      }]
    })
    
    window.addEventListener('resize', () => classChart.resize())
  }
}

// 更新所有书籍价格
async function updateBookPrices() {
  if (updatingPrices.value) return
  
  updatingPrices.value = true
  try {
    const response = await seedApi.updateBookPrices()
    if (response.data) {
      toastStore.success(`成功更新 ${response.data.data?.updatedBooks || 0} 本书籍价格`)
    }
  } catch (error) {
    console.error('更新价格失败:', error)
    toastStore.error('更新价格失败，请重试')
  } finally {
    updatingPrices.value = false
  }
}

onMounted(() => {
  fetchStatistics()
})
</script>

<template>
  <div class="space-y-6">
    <!-- 页面标题 -->
    <div class="flex items-center justify-between">
      <div>
        <h1 class="text-2xl font-bold">仪表盘</h1>
        <p class="text-muted-foreground">图书馆运营数据概览</p>
      </div>
      <div class="flex gap-2">
        <Button 
          variant="outline" 
          size="sm"
          @click="updateBookPrices"
          :disabled="updatingPrices"
        >
          <DollarSign class="w-4 h-4 mr-1" />
          {{ updatingPrices ? '更新中...' : '初始化书籍价格' }}
        </Button>
        <Button 
          variant="outline" 
          size="sm"
          @click="fetchStatistics"
          :disabled="loading"
        >
          <RefreshCw :class="['w-4 h-4 mr-1', { 'animate-spin': loading }]" />
          刷新数据
        </Button>
      </div>
    </div>
    
    <!-- KPI 卡片 -->
    <div class="grid gap-4 md:grid-cols-2 lg:grid-cols-5">
      <!-- 总用户数 -->
      <Card>
        <CardContent class="p-6">
          <div class="flex items-center justify-between">
            <div>
              <p class="text-sm text-muted-foreground">注册用户</p>
              <p class="text-2xl font-bold mt-1">
                {{ loading ? '-' : statistics?.totalUsers }}
              </p>
            </div>
            <div class="w-12 h-12 rounded-full bg-blue-100 dark:bg-blue-900 flex items-center justify-center">
              <Users class="h-6 w-6 text-blue-600" />
            </div>
          </div>
        </CardContent>
      </Card>
      
      <!-- 当前借阅 -->
      <Card>
        <CardContent class="p-6">
          <div class="flex items-center justify-between">
            <div>
              <p class="text-sm text-muted-foreground">当前借阅</p>
              <p class="text-2xl font-bold mt-1">
                {{ loading ? '-' : statistics?.totalCurrentBorrowedBooks }}
              </p>
            </div>
            <div class="w-12 h-12 rounded-full bg-amber-100 dark:bg-amber-900 flex items-center justify-center">
              <BookOpen class="h-6 w-6 text-amber-600" />
            </div>
          </div>
        </CardContent>
      </Card>
      
      <!-- 历史借阅 -->
      <Card>
        <CardContent class="p-6">
          <div class="flex items-center justify-between">
            <div>
              <p class="text-sm text-muted-foreground">历史借阅</p>
              <p class="text-2xl font-bold mt-1">
                {{ loading ? '-' : statistics?.totalHistoryBorrowedBooks }}
              </p>
            </div>
            <div class="w-12 h-12 rounded-full bg-green-100 dark:bg-green-900 flex items-center justify-center">
              <TrendingUp class="h-6 w-6 text-green-600" />
            </div>
          </div>
        </CardContent>
      </Card>
      
      <!-- 平均借阅时长 -->
      <Card>
        <CardContent class="p-6">
          <div class="flex items-center justify-between">
            <div>
              <p class="text-sm text-muted-foreground">平均借阅时长</p>
              <p class="text-2xl font-bold mt-1">
                {{ loading ? '-' : statistics?.averageBorrowDuration }}
                <span class="text-sm font-normal text-muted-foreground">天</span>
              </p>
            </div>
            <div class="w-12 h-12 rounded-full bg-purple-100 dark:bg-purple-900 flex items-center justify-center">
              <Clock class="h-6 w-6 text-purple-600" />
            </div>
          </div>
        </CardContent>
      </Card>
      
      <!-- 待处理荐购 -->
      <Card>
        <CardContent class="p-6">
          <div class="flex items-center justify-between">
            <div>
              <p class="text-sm text-muted-foreground">待处理荐购</p>
              <p class="text-2xl font-bold mt-1">
                {{ loading ? '-' : statistics?.unhandledRecommends }}
              </p>
            </div>
            <div class="w-12 h-12 rounded-full bg-rose-100 dark:bg-rose-900 flex items-center justify-center">
              <MessageSquare class="h-6 w-6 text-rose-600" />
            </div>
          </div>
        </CardContent>
      </Card>
    </div>
    
    <!-- 图表区域 -->
    <div class="grid gap-6 lg:grid-cols-2">
      <!-- 月度借阅趋势 -->
      <Card>
        <CardHeader>
          <CardTitle>月度借阅趋势</CardTitle>
        </CardHeader>
        <CardContent>
          <div 
            ref="monthlyChartRef" 
            class="h-80"
            :class="{ 'animate-pulse bg-muted rounded': loading }"
          />
        </CardContent>
      </Card>
      
      <!-- 分类借阅分布 -->
      <Card>
        <CardHeader>
          <CardTitle>借阅分类分布</CardTitle>
        </CardHeader>
        <CardContent>
          <div 
            ref="classificationChartRef" 
            class="h-80"
            :class="{ 'animate-pulse bg-muted rounded': loading }"
          />
        </CardContent>
      </Card>
    </div>
  </div>
</template>
