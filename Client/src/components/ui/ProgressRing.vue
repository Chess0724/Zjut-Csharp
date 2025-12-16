<script setup lang="ts">
import { computed } from 'vue'
import { cn } from '@/lib/utils'

interface Props {
  value: number
  max?: number
  size?: 'sm' | 'md' | 'lg'
  showLabel?: boolean
  class?: string
}

const props = withDefaults(defineProps<Props>(), {
  max: 100,
  size: 'md',
  showLabel: false
})

const percentage = computed(() => Math.min(100, Math.max(0, (props.value / props.max) * 100)))

const sizeClasses = {
  sm: 'w-12 h-12',
  md: 'w-20 h-20',
  lg: 'w-28 h-28'
}

const strokeWidth = {
  sm: 4,
  md: 6,
  lg: 8
}

const radius = computed(() => {
  const sizes = { sm: 20, md: 34, lg: 48 }
  return sizes[props.size]
})

const circumference = computed(() => 2 * Math.PI * radius.value)
const strokeDashoffset = computed(() => circumference.value - (percentage.value / 100) * circumference.value)

const colorClass = computed(() => {
  if (percentage.value <= 20) return 'text-red-500'
  if (percentage.value <= 40) return 'text-amber-500'
  return 'text-green-500'
})
</script>

<template>
  <div :class="cn('relative inline-flex items-center justify-center', sizeClasses[size], props.class)">
    <svg class="transform -rotate-90 w-full h-full">
      <!-- 背景圆环 -->
      <circle
        :cx="size === 'sm' ? 24 : size === 'md' ? 40 : 56"
        :cy="size === 'sm' ? 24 : size === 'md' ? 40 : 56"
        :r="radius"
        fill="none"
        stroke="currentColor"
        :stroke-width="strokeWidth[size]"
        class="text-muted"
      />
      <!-- 进度圆环 -->
      <circle
        :cx="size === 'sm' ? 24 : size === 'md' ? 40 : 56"
        :cy="size === 'sm' ? 24 : size === 'md' ? 40 : 56"
        :r="radius"
        fill="none"
        stroke="currentColor"
        :stroke-width="strokeWidth[size]"
        :stroke-dasharray="circumference"
        :stroke-dashoffset="strokeDashoffset"
        stroke-linecap="round"
        :class="colorClass"
        style="transition: stroke-dashoffset 0.5s ease-in-out"
      />
    </svg>
    <span 
      v-if="showLabel"
      :class="cn('absolute text-sm font-semibold', colorClass)"
    >
      {{ Math.round(percentage) }}%
    </span>
    <span v-else class="absolute">
      <slot />
    </span>
  </div>
</template>
