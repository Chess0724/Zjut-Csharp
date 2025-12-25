<template>
  <div class="d-flex justify-center align-center h-screen" ref="vantaRef">
    <v-sheet class="d-flex align-center mx-auto" elevation="12" rounded="xl" 
             style="width: 420px; background: rgba(10, 30, 20, 0.95); border: 1px solid rgba(76, 175, 80, 0.3);">
      <v-container>
        <v-row class="mb-4">
          <v-col class="text-center">
            <v-icon icon="mdi-shield-account" size="64" color="#4CAF50" class="mb-2"></v-icon>
            <div class="admin-header">管理后台</div>
            <div class="admin-subtitle">Administrator Portal</div>
          </v-col>
        </v-row>
        <v-row>
          <AdminLogin></AdminLogin>
        </v-row>
      </v-container>
    </v-sheet>
  </div>
</template>

<script setup lang="ts">
import { onMounted, onBeforeUnmount, ref } from 'vue';
import * as THREE from 'three';
import GLOBE from "vanta/dist/vanta.globe.min";
import AdminLogin from './AdminLogin.vue';

const vantaRef = ref(null)
let vantaEffect: any = null

onMounted(() => {
  vantaEffect = GLOBE({
    el: vantaRef.value,
    THREE: THREE,
    minHeight: 200.00,
    minWidth: 200.00,
    scale: 1.00,
    scaleMobile: 1.00,
    mouseControls: true,
    touchControls: true,
    gyroControls: false,
    backgroundColor: 0x0d1117,
    color: 0x4CAF50,
    color2: 0x2E7D32
  })
})

onBeforeUnmount(() => {
  if (vantaEffect) {
    vantaEffect.destroy()
  }
})
</script>

<style scoped>
.admin-header {
  font-size: 2rem;
  font-weight: bold;
  color: #4CAF50;
  text-shadow: 0 0 10px rgba(76, 175, 80, 0.3);
}

.admin-subtitle {
  font-size: 0.9rem;
  color: rgba(255, 255, 255, 0.6);
  letter-spacing: 2px;
  text-transform: uppercase;
}
</style>
