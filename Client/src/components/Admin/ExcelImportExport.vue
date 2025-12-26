<script setup lang="ts">
import { ref, computed } from 'vue'
import { useToastStore } from '@/stores/toast'
import { useAuthStore } from '@/stores/auth'
import { excelApi, type BookImportResult } from '@/api'
import Button from '@/components/ui/Button.vue'
import Dialog from '@/components/ui/Dialog.vue'
import { Download, Upload, FileSpreadsheet, AlertCircle, CheckCircle } from 'lucide-vue-next'

interface Props {
  type: 'books' | 'users' | 'orders' | 'borrows'
}

const props = defineProps<Props>()
const toastStore = useToastStore()
const authStore = useAuthStore()

// 状态
const isExporting = ref(false)
const isImporting = ref(false)
const isDownloadingTemplate = ref(false)
const showImportDialog = ref(false)
const importResult = ref<BookImportResult | null>(null)
const fileInputRef = ref<HTMLInputElement | null>(null)

// 计算属性
const typeLabel = computed(() => {
  const labels = {
    books: '图书',
    users: '用户',
    orders: '订单',
    borrows: '借阅记录'
  }
  return labels[props.type]
})

const canImport = computed(() => props.type === 'books')
const canExportUsers = computed(() => authStore.hasRole('Admin'))

// 下载 Blob 文件
function downloadBlob(blob: Blob, filename: string) {
  const url = window.URL.createObjectURL(blob)
  const link = document.createElement('a')
  link.href = url
  link.download = filename
  document.body.appendChild(link)
  link.click()
  document.body.removeChild(link)
  window.URL.revokeObjectURL(url)
}

// 导出数据
async function handleExport() {
  if (props.type === 'users' && !canExportUsers.value) {
    toastStore.error('仅管理员可导出用户数据')
    return
  }

  isExporting.value = true
  try {
    let response
    switch (props.type) {
      case 'books':
        response = await excelApi.exportBooks()
        break
      case 'users':
        response = await excelApi.exportUsers()
        break
      case 'orders':
        response = await excelApi.exportOrders()
        break
      case 'borrows':
        response = await excelApi.exportBorrows()
        break
    }
    
    const blob = new Blob([response.data], { 
      type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' 
    })
    const filename = `${typeLabel.value}列表_${new Date().toISOString().slice(0, 10)}.xlsx`
    downloadBlob(blob, filename)
    toastStore.success(`${typeLabel.value}数据导出成功`)
  } catch (error: any) {
    console.error('导出失败:', error)
    toastStore.error(error.response?.data || '导出失败')
  } finally {
    isExporting.value = false
  }
}

// 下载导入模板
async function handleDownloadTemplate() {
  isDownloadingTemplate.value = true
  try {
    const response = await excelApi.downloadTemplate()
    const blob = new Blob([response.data], { 
      type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' 
    })
    downloadBlob(blob, '图书导入模板.xlsx')
    toastStore.success('模板下载成功')
  } catch (error: any) {
    console.error('下载模板失败:', error)
    toastStore.error('下载模板失败')
  } finally {
    isDownloadingTemplate.value = false
  }
}

// 触发文件选择
function triggerFileSelect() {
  fileInputRef.value?.click()
}

// 处理文件选择
async function handleFileSelect(event: Event) {
  const target = event.target as HTMLInputElement
  const file = target.files?.[0]
  if (!file) return

  if (!file.name.endsWith('.xlsx')) {
    toastStore.error('仅支持 .xlsx 格式的 Excel 文件')
    return
  }

  isImporting.value = true
  importResult.value = null

  try {
    const response = await excelApi.importBooks(file)
    importResult.value = response.data
    showImportDialog.value = true
    
    if (response.data.failCount === 0) {
      toastStore.success(`成功导入 ${response.data.successCount} 本图书`)
    } else {
      toastStore.warning(`导入完成：成功 ${response.data.successCount}，失败 ${response.data.failCount}`)
    }
  } catch (error: any) {
    console.error('导入失败:', error)
    toastStore.error(error.response?.data || '导入失败')
  } finally {
    isImporting.value = false
    // 重置 input 以允许重新选择相同文件
    target.value = ''
  }
}

// 关闭导入结果对话框
function closeImportDialog() {
  showImportDialog.value = false
  importResult.value = null
}

// 发出刷新事件
const emit = defineEmits<{
  refresh: []
}>()

// 导入成功后刷新列表
function handleImportComplete() {
  closeImportDialog()
  emit('refresh')
}
</script>

<template>
  <div class="flex items-center gap-2">
    <!-- 导出按钮 -->
    <Button
      variant="outline"
      size="sm"
      :loading="isExporting"
      :disabled="type === 'users' && !canExportUsers"
      @click="handleExport"
    >
      <Download class="w-4 h-4 mr-1" />
      导出{{ typeLabel }}
    </Button>

    <!-- 导入相关按钮（仅图书支持导入） -->
    <template v-if="canImport">
      <Button
        variant="outline"
        size="sm"
        :loading="isDownloadingTemplate"
        @click="handleDownloadTemplate"
      >
        <FileSpreadsheet class="w-4 h-4 mr-1" />
        下载模板
      </Button>

      <Button
        variant="default"
        size="sm"
        :loading="isImporting"
        @click="triggerFileSelect"
      >
        <Upload class="w-4 h-4 mr-1" />
        导入图书
      </Button>

      <!-- 隐藏的文件输入 -->
      <input
        ref="fileInputRef"
        type="file"
        accept=".xlsx"
        class="hidden"
        @change="handleFileSelect"
      />
    </template>

    <!-- 导入结果对话框 -->
    <Dialog
      :open="showImportDialog"
      title="导入结果"
      description="图书数据导入完成"
      class="max-w-lg"
      @update:open="closeImportDialog"
    >
      <div v-if="importResult" class="space-y-4">
        <!-- 统计摘要 -->
        <div class="grid grid-cols-3 gap-4 text-center">
          <div class="p-3 rounded-lg bg-muted">
            <div class="text-2xl font-bold">{{ importResult.totalRows }}</div>
            <div class="text-sm text-muted-foreground">总行数</div>
          </div>
          <div class="p-3 rounded-lg bg-green-100 dark:bg-green-900/30">
            <div class="text-2xl font-bold text-green-600">{{ importResult.successCount }}</div>
            <div class="text-sm text-green-600">成功</div>
          </div>
          <div class="p-3 rounded-lg bg-red-100 dark:bg-red-900/30">
            <div class="text-2xl font-bold text-red-600">{{ importResult.failCount }}</div>
            <div class="text-sm text-red-600">失败</div>
          </div>
        </div>

        <!-- 错误详情 -->
        <div v-if="importResult.errors.length > 0" class="space-y-2">
          <h4 class="font-medium flex items-center gap-1 text-red-600">
            <AlertCircle class="w-4 h-4" />
            错误详情
          </h4>
          <div class="max-h-48 overflow-y-auto space-y-1">
            <div
              v-for="(error, index) in importResult.errors"
              :key="index"
              class="text-sm p-2 rounded bg-red-50 dark:bg-red-900/20 text-red-700 dark:text-red-300"
            >
              第 {{ error.row }} 行 - {{ error.field }}: {{ error.message }}
            </div>
          </div>
        </div>

        <!-- 成功提示 -->
        <div v-else class="flex items-center justify-center gap-2 text-green-600 py-4">
          <CheckCircle class="w-6 h-6" />
          <span class="text-lg">所有数据导入成功！</span>
        </div>

        <!-- 操作按钮 -->
        <div class="flex justify-end">
          <Button @click="handleImportComplete">确定</Button>
        </div>
      </div>
    </Dialog>
  </div>
</template>
