<script setup lang="ts">
import { cn } from '@/lib/utils'

interface Props {
  class?: string
  rows?: number
}

const props = withDefaults(defineProps<Props>(), {
  rows: 3
})
</script>

<template>
  <div :class="cn('animate-pulse', props.class)">
    <div class="space-y-3">
      <div 
        v-for="i in rows" 
        :key="i"
        class="h-4 bg-muted rounded"
        :style="{ width: `${Math.random() * 40 + 60}%` }"
      />
    </div>
  </div>
</template>
