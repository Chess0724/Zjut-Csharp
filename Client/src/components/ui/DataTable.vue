<script setup lang="ts">
import { computed } from 'vue'
import { cn } from '@/lib/utils'

interface Column {
  key: string
  label: string
  sortable?: boolean
  width?: string
  class?: string
}

interface Props {
  columns: Column[]
  data: Record<string, unknown>[]
  loading?: boolean
  sortColumn?: string
  sortOrder?: 'ASC' | 'DESC'
  class?: string
}

const props = withDefaults(defineProps<Props>(), {
  loading: false,
  sortOrder: 'ASC'
})

const emit = defineEmits<{
  'sort': [column: string]
  'row-click': [row: Record<string, unknown>]
}>()

function handleSort(column: Column) {
  if (column.sortable) {
    emit('sort', column.key)
  }
}

const sortIcon = computed(() => (column: string) => {
  if (props.sortColumn !== column) return '↕'
  return props.sortOrder === 'ASC' ? '↑' : '↓'
})
</script>

<template>
  <div :class="cn('w-full overflow-auto', props.class)">
    <table class="w-full caption-bottom text-sm">
      <thead class="border-b">
        <tr>
          <th
            v-for="col in columns"
            :key="col.key"
            :class="cn(
              'h-10 px-4 text-left align-middle font-medium text-muted-foreground',
              col.sortable && 'cursor-pointer hover:text-foreground select-none',
              col.class
            )"
            :style="col.width ? { width: col.width } : undefined"
            @click="handleSort(col)"
          >
            <span class="flex items-center gap-1">
              {{ col.label }}
              <span v-if="col.sortable" class="text-xs opacity-50">
                {{ sortIcon(col.key) }}
              </span>
            </span>
          </th>
          <th class="h-10 px-4 text-right align-middle font-medium text-muted-foreground w-24">
            操作
          </th>
        </tr>
      </thead>
      <tbody>
        <!-- Loading State -->
        <template v-if="loading">
          <tr v-for="i in 5" :key="i">
            <td
              v-for="col in columns"
              :key="col.key"
              class="p-4"
            >
              <div class="h-4 bg-muted rounded animate-pulse" />
            </td>
            <td class="p-4">
              <div class="h-4 bg-muted rounded animate-pulse w-16 ml-auto" />
            </td>
          </tr>
        </template>
        
        <!-- Data -->
        <template v-else-if="data.length > 0">
          <tr
            v-for="(row, index) in data"
            :key="index"
            class="border-b transition-colors hover:bg-muted/50"
            @click="emit('row-click', row)"
          >
            <td
              v-for="col in columns"
              :key="col.key"
              class="p-4 align-middle"
            >
              <slot :name="`cell-${col.key}`" :value="row[col.key]" :row="row">
                {{ row[col.key] }}
              </slot>
            </td>
            <td class="p-4 text-right">
              <slot name="actions" :row="row" />
            </td>
          </tr>
        </template>
        
        <!-- Empty State -->
        <tr v-else>
          <td :colspan="columns.length + 1" class="h-32 text-center text-muted-foreground">
            暂无数据
          </td>
        </tr>
      </tbody>
    </table>
  </div>
</template>
