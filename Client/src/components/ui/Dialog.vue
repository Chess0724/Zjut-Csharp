<script setup lang="ts">
import { ref, watch } from 'vue'
import { cn } from '@/lib/utils'
import Button from './Button.vue'
import { X } from 'lucide-vue-next'

interface Props {
  open: boolean
  title?: string
  description?: string
  class?: string
}

const props = withDefaults(defineProps<Props>(), {
  open: false
})

const emit = defineEmits<{
  'update:open': [value: boolean]
  'close': []
}>()

const isVisible = ref(props.open)

watch(() => props.open, (val) => {
  isVisible.value = val
})

function close() {
  emit('update:open', false)
  emit('close')
}

function onBackdropClick(e: MouseEvent) {
  if (e.target === e.currentTarget) {
    close()
  }
}
</script>

<template>
  <Teleport to="body">
    <Transition
      enter-active-class="transition-opacity duration-200"
      enter-from-class="opacity-0"
      enter-to-class="opacity-100"
      leave-active-class="transition-opacity duration-200"
      leave-from-class="opacity-100"
      leave-to-class="opacity-0"
    >
      <div
        v-if="isVisible"
        class="fixed inset-0 z-50 flex items-center justify-center"
      >
        <!-- Backdrop -->
        <div
          class="fixed inset-0 bg-black/50 backdrop-blur-sm"
          @click="onBackdropClick"
        />
        
        <!-- Dialog Content -->
        <div
          :class="cn(
            'relative z-50 w-full max-w-lg bg-background rounded-lg shadow-lg border p-6 animate-fade-in',
            props.class
          )"
        >
          <!-- Close Button -->
          <Button
            variant="ghost"
            size="icon"
            class="absolute right-4 top-4"
            @click="close"
          >
            <X class="h-4 w-4" />
          </Button>
          
          <!-- Header -->
          <div v-if="title || description" class="mb-4">
            <h2 v-if="title" class="text-lg font-semibold">{{ title }}</h2>
            <p v-if="description" class="text-sm text-muted-foreground mt-1">
              {{ description }}
            </p>
          </div>
          
          <!-- Content -->
          <slot />
        </div>
      </div>
    </Transition>
  </Teleport>
</template>
