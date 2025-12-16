<script setup lang="ts">
import { cn } from '@/lib/utils'

interface Props {
  src?: string | null
  fallback?: string
  size?: 'sm' | 'md' | 'lg' | 'xl'
  class?: string
}

const props = withDefaults(defineProps<Props>(), {
  size: 'md',
  fallback: 'U'
})

const sizeClasses = {
  sm: 'h-8 w-8 text-xs',
  md: 'h-10 w-10 text-sm',
  lg: 'h-12 w-12 text-base',
  xl: 'h-16 w-16 text-lg'
}
</script>

<template>
  <div
    :class="cn(
      'relative inline-flex items-center justify-center rounded-full bg-muted overflow-hidden',
      sizeClasses[size],
      props.class
    )"
  >
    <img
      v-if="src"
      :src="src"
      alt="Avatar"
      class="h-full w-full object-cover"
    />
    <span v-else class="font-medium text-muted-foreground uppercase">
      {{ fallback.charAt(0) }}
    </span>
  </div>
</template>
