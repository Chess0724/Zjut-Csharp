<script setup lang="ts">
import { ref, computed } from 'vue'
import { useToastStore } from '@/stores/toast'
import { aiApi, bookApi, type BookInfoFromImage } from '@/api'
import Button from '@/components/ui/Button.vue'
import Dialog from '@/components/ui/Dialog.vue'
import Input from '@/components/ui/Input.vue'
import { Camera, Upload, Loader2, CheckCircle, X } from 'lucide-vue-next'

const toastStore = useToastStore()

// 状态
const showDialog = ref(false)
const isExtracting = ref(false)
const isSaving = ref(false)
const imagePreview = ref<string | null>(null)
const extractedInfo = ref<BookInfoFromImage | null>(null)
const fileInputRef = ref<HTMLInputElement | null>(null)

// 编辑表单
const formData = ref({
  title: '',
  author: '',
  publisher: '',
  publishedDate: '',
  identifier: '',
  inventory: 10,
  saleInventory: 100,
  price: 39.90
})

// 是否可以保存
const canSave = computed(() => {
  return formData.value.title.trim() && 
         formData.value.author.trim() && 
         formData.value.publisher.trim()
})

// 打开对话框
function openDialog() {
  showDialog.value = true
  resetForm()
}

// 关闭对话框
function closeDialog() {
  showDialog.value = false
  resetForm()
}

// 重置表单
function resetForm() {
  imagePreview.value = null
  extractedInfo.value = null
  formData.value = {
    title: '',
    author: '',
    publisher: '',
    publishedDate: '',
    identifier: '',
    inventory: 10,
    saleInventory: 100,
    price: 39.90
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

  // 验证文件类型
  if (!file.type.startsWith('image/')) {
    toastStore.error('请选择图片文件')
    return
  }

  // 压缩并转换为 base64
  const base64 = await fileToBase64(file)
  imagePreview.value = base64

  // 调用 AI 识别
  await extractBookInfo(base64)

  // 重置 input
  target.value = ''
}

// 文件转 base64
function fileToBase64(file: File): Promise<string> {
  return new Promise((resolve, reject) => {
    const reader = new FileReader()
    reader.onload = () => resolve(reader.result as string)
    reader.onerror = reject
    reader.readAsDataURL(file)
  })
}

// 调用 AI 识别图书信息
async function extractBookInfo(base64Image: string) {
  isExtracting.value = true
  extractedInfo.value = null

  try {
    const response = await aiApi.extractBookFromImage(base64Image)
    extractedInfo.value = response.data

    if (response.data.success) {
      // 填充表单
      formData.value.title = response.data.title || ''
      formData.value.author = response.data.author || ''
      formData.value.publisher = response.data.publisher || ''
      formData.value.publishedDate = response.data.publishedDate || ''
      formData.value.identifier = response.data.isbn || ''
      formData.value.price = response.data.price || 39.90

      if (response.data.error) {
        toastStore.warning(response.data.error)
      } else {
        toastStore.success('图书信息识别成功，请确认后保存')
      }
    } else {
      toastStore.error(response.data.error || '识别失败')
    }
  } catch (error: any) {
    console.error('识别失败:', error)
    toastStore.error(error.response?.data?.error || '识别失败，请重试')
  } finally {
    isExtracting.value = false
  }
}

// 保存图书
async function saveBook() {
  if (!canSave.value) {
    toastStore.warning('请填写必要的图书信息')
    return
  }

  isSaving.value = true

  try {
    const response = await bookApi.addBook({
      title: formData.value.title,
      author: formData.value.author,
      publisher: formData.value.publisher,
      publishedDate: formData.value.publishedDate,
      identifier: formData.value.identifier,
      inventory: formData.value.inventory,
      price: formData.value.price
    })

    if (response.data.code === 0) {
      toastStore.success('图书添加成功')
      closeDialog()
      emit('refresh')
    } else {
      toastStore.error(response.data.message || '添加失败')
    }
  } catch (error: any) {
    console.error('保存失败:', error)
    toastStore.error('保存失败，请重试')
  } finally {
    isSaving.value = false
  }
}

// 发出刷新事件
const emit = defineEmits<{
  refresh: []
}>()
</script>

<template>
  <div>
    <!-- 拍照导入按钮 -->
    <Button variant="outline" size="sm" @click="openDialog">
      <Camera class="w-4 h-4 mr-1" />
      拍照导入
    </Button>

    <!-- 对话框 -->
    <Dialog
      :open="showDialog"
      title="拍照导入图书"
      description="拍摄或上传图书封面/版权页，AI 自动识别图书信息"
      class="max-w-2xl"
      @update:open="closeDialog"
    >
      <div class="space-y-4">
        <!-- 图片上传区域 -->
        <div 
          class="border-2 border-dashed rounded-lg p-6 text-center cursor-pointer hover:border-primary transition-colors"
          :class="{ 'border-primary bg-primary/5': imagePreview }"
          @click="triggerFileSelect"
        >
          <input
            ref="fileInputRef"
            type="file"
            accept="image/*"
            capture="environment"
            class="hidden"
            @change="handleFileSelect"
          />

          <template v-if="imagePreview">
            <div class="relative inline-block">
              <img
                :src="imagePreview"
                alt="预览"
                class="max-h-48 rounded-lg mx-auto"
              />
              <button
                class="absolute -top-2 -right-2 p-1 rounded-full bg-destructive text-white hover:bg-destructive/90"
                @click.stop="resetForm"
              >
                <X class="w-4 h-4" />
              </button>
            </div>
          </template>

          <template v-else>
            <Upload class="w-12 h-12 mx-auto text-muted-foreground mb-2" />
            <p class="text-muted-foreground">点击上传或拍摄图书封面/版权页</p>
            <p class="text-sm text-muted-foreground mt-1">支持 JPG、PNG 格式</p>
          </template>
        </div>

        <!-- 识别状态 -->
        <div v-if="isExtracting" class="flex items-center justify-center gap-2 py-4">
          <Loader2 class="w-5 h-5 animate-spin text-primary" />
          <span class="text-muted-foreground">AI 正在识别图书信息...</span>
        </div>

        <!-- 识别结果表单 -->
        <template v-if="extractedInfo && !isExtracting">
          <div class="flex items-center gap-2 text-green-600 mb-2">
            <CheckCircle class="w-5 h-5" />
            <span>识别完成，请确认并编辑以下信息</span>
          </div>

          <div class="grid grid-cols-2 gap-4">
            <div class="space-y-2">
              <label class="text-sm font-medium">书名 *</label>
              <Input v-model="formData.title" placeholder="请输入书名" />
            </div>
            <div class="space-y-2">
              <label class="text-sm font-medium">作者 *</label>
              <Input v-model="formData.author" placeholder="请输入作者" />
            </div>
            <div class="space-y-2">
              <label class="text-sm font-medium">出版社 *</label>
              <Input v-model="formData.publisher" placeholder="请输入出版社" />
            </div>
            <div class="space-y-2">
              <label class="text-sm font-medium">出版日期</label>
              <Input v-model="formData.publishedDate" placeholder="如：2023-06" />
            </div>
            <div class="space-y-2">
              <label class="text-sm font-medium">ISBN / 分类号</label>
              <Input v-model="formData.identifier" placeholder="请输入ISBN或分类号" />
            </div>
            <div class="space-y-2">
              <label class="text-sm font-medium">售价</label>
              <Input v-model.number="formData.price" type="number" step="0.01" min="0" />
            </div>
            <div class="space-y-2">
              <label class="text-sm font-medium">借阅库存</label>
              <Input v-model.number="formData.inventory" type="number" min="0" />
            </div>
            <div class="space-y-2">
              <label class="text-sm font-medium">销售库存</label>
              <Input v-model.number="formData.saleInventory" type="number" min="0" />
            </div>
          </div>

          <!-- 原始识别结果（可展开） -->
          <details v-if="extractedInfo.rawText" class="text-sm">
            <summary class="cursor-pointer text-muted-foreground hover:text-foreground">
              查看 AI 原始识别结果
            </summary>
            <pre class="mt-2 p-3 bg-muted rounded-lg text-xs overflow-x-auto whitespace-pre-wrap">{{ extractedInfo.rawText }}</pre>
          </details>
        </template>

        <!-- 操作按钮 -->
        <div class="flex justify-end gap-2 pt-4">
          <Button variant="outline" @click="closeDialog">取消</Button>
          <Button
            :disabled="!canSave || isSaving"
            :loading="isSaving"
            @click="saveBook"
          >
            添加图书
          </Button>
        </div>
      </div>
    </Dialog>
  </div>
</template>
