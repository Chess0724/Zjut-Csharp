<script setup lang="ts">
import { ref, watch } from 'vue'
import Input from '@/components/ui/Input.vue'
import Button from '@/components/ui/Button.vue'
import { Search, X } from 'lucide-vue-next'
import { debounce } from '@/lib/utils'

interface Props {
  modelValue: string
  placeholder?: string
  debounceMs?: number
}

const props = withDefaults(defineProps<Props>(), {
  placeholder: '搜索书名、作者或出版社...',
  debounceMs: 300
})

const emit = defineEmits<{
  'update:modelValue': [value: string]
  'search': [value: string]
}>()

const localValue = ref(props.modelValue)

const debouncedEmit = debounce((value: string) => {
  emit('update:modelValue', value)
  emit('search', value)
}, props.debounceMs)

watch(() => props.modelValue, (val) => {
  localValue.value = val
})

function onInput(value: string) {
  localValue.value = value
  debouncedEmit(value)
}

function clear() {
  localValue.value = ''
  emit('update:modelValue', '')
  emit('search', '')
}

function handleSubmit() {
  emit('update:modelValue', localValue.value)
  emit('search', localValue.value)
}
</script>

<template>
  <form 
    class="relative flex items-center gap-2"
    @submit.prevent="handleSubmit"
  >
    <div class="relative flex-1">
      <Search class="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
      <Input
        :model-value="localValue"
        type="search"
        :placeholder="placeholder"
        class="pl-10 pr-10 h-11"
        @update:model-value="onInput"
      />
      <button
        v-if="localValue"
        type="button"
        class="absolute right-3 top-1/2 -translate-y-1/2 text-muted-foreground hover:text-foreground"
        @click="clear"
      >
        <X class="h-4 w-4" />
      </button>
    </div>
    <Button type="submit" size="lg">
      搜索
    </Button>
  </form>
</template>
