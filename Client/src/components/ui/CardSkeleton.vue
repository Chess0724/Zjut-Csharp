<script setup lang="ts">
import { cn } from '@/lib/utils'

interface Props {
  class?: string
}

const props = defineProps<Props>()
</script>

<template>
  <div :class="cn('rounded-xl border bg-card text-card-foreground shadow animate-pulse', props.class)">
    <div class="p-6 space-y-4">
      <div class="space-y-2">
        <div class="h-4 bg-muted rounded w-1/2" />
        <div class="h-3 bg-muted rounded w-1/3" />
      </div>
      <div class="space-y-2">
        <div class="h-3 bg-muted rounded" />
        <div class="h-3 bg-muted rounded w-5/6" />
        <div class="h-3 bg-muted rounded w-4/6" />
      </div>
    </div>
  </div>
</template>
