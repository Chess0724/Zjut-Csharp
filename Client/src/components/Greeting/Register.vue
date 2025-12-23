<template>
  <v-container>
    <v-row>
      <span class="subheader">注册</span>
    </v-row>
    <v-row>
      <v-form class="form" :fast-fail="true" @submit.prevent>
        <v-container>
          <v-row>
            <v-col class="icon" cols="1">
              <v-icon icon="mdi-email"></v-icon>
            </v-col>
            <v-col cols="7">
              <v-text-field v-model="email" :rules="emailRules" label="邮箱" type="email" />
            </v-col>
            <v-col cols="4">
              <v-btn 
                :color="skyColor" 
                :disabled="!canSendCode || countdown > 0" 
                :loading="sendingCode"
                @click="onSendCode"
                class="send-code-btn"
              >
                {{ countdown > 0 ? `${countdown}秒后重发` : '发送验证码' }}
              </v-btn>
            </v-col>
          </v-row>
          <v-row>
            <v-col class="icon" cols="1">
              <v-icon icon="mdi-shield-check"></v-icon>
            </v-col>
            <v-col cols="11">
              <v-text-field v-model="verificationCode" :rules="[notNullRule]" label="验证码" maxlength="6" />
            </v-col>
          </v-row>
          <v-row>
            <v-col class="icon" cols="1">
              <v-icon icon="mdi-account"></v-icon>
            </v-col>
            <v-col cols="11">
              <v-text-field v-model="userAccount" :rules="[notNullRule]" label="用户名" />
            </v-col>
          </v-row>
          <v-row>
            <v-col class="icon" cols="1">
              <v-icon icon="mdi-lock"></v-icon>
            </v-col>
            <v-col cols="11">
              <v-text-field v-model="password" :rules="passwordLengthRule" label="密码"
                :append-inner-icon="showPassword ? 'mdi-eye' : 'mdi-eye-off'"
                :type="showPassword ? 'text' : 'password'"
                @click:append-inner="showPassword = !showPassword"  />
            </v-col>
          </v-row>
          <v-row>
            <v-col class="icon" cols="1">
              <v-icon icon="mdi-lock"></v-icon>
            </v-col>
            <v-col cols="11">
              <v-text-field v-model="passwordConfirmed" :rules="passwordRule" label="确认密码"
                :append-inner-icon="showPassword ? 'mdi-eye' : 'mdi-eye-off'"
                :type="showPassword ? 'text' : 'password'"
                @click:append-inner="showPassword = !showPassword" />
            </v-col>
          </v-row>
          <v-row>
            <v-btn :color="skyColor" class="w-25 mx-auto" type="submit" @click="onRegisterSubmit"
              :disabled="submitButtonDisabled" :loading="submitButtonLoading">注册</v-btn>
          </v-row>
        </v-container>
      </v-form>
    </v-row>
    <v-snackbar v-model="snackbar" timeout="5000" rounded="pill" :color="snackbarColor">
      {{ registerPrompt }}
    </v-snackbar>
  </v-container>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { getSkyColor } from '@/plugins/util/color';
import axiosInstance from '@/plugins/util/axiosInstance';
import { notNullRule } from '@/plugins/util/rules';

const skyColor = getSkyColor()

const email = ref<string>('')
const verificationCode = ref<string>('')
const userAccount = ref<string>('')
const password = ref<string>('')
const passwordConfirmed = ref<string>('')

const snackbar = ref<boolean>(false)
const snackbarColor = ref<string>(skyColor)
const registerPrompt = ref<string>('')

const submitButtonDisabled = ref(true)
const submitButtonLoading = ref(false)
const showPassword = ref(false)

// 验证码相关
const sendingCode = ref(false)
const countdown = ref(0)
let countdownTimer: ReturnType<typeof setInterval> | null = null

// 邮箱格式验证
const emailRules = [
  (value: string) => {
    if (!value) return '请输入邮箱'
    const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
    if (!emailPattern.test(value)) return '邮箱格式不正确'
    return true
  }
]

// 密码长度验证
const passwordLengthRule = [
  (value: string) => {
    if (!value) return '请输入密码'
    if (value.length < 6) return '密码长度不能少于6位'
    return true
  }
]

// 是否可以发送验证码
const canSendCode = computed(() => {
  const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
  return emailPattern.test(email.value)
})

const passwordRule = [
  (value: string | any[]) => {
    if (password.value === passwordConfirmed.value) {
      if (password.value !== '' && password.value.length >= 6 && 
          userAccount.value !== '' && verificationCode.value !== '' && email.value !== '') {
        submitButtonDisabled.value = false;
        return true;
      } else {
        submitButtonDisabled.value = true;
        return value ? true : '请填入信息';
      }
    } else {
      submitButtonDisabled.value = true;
      return '两次输入的密码不一致';
    }
  },
]

// 发送验证码
async function onSendCode() {
  if (!canSendCode.value || countdown.value > 0) return
  
  sendingCode.value = true
  
  try {
    await axiosInstance.post('/Account/SendVerificationCode', {
      email: email.value
    })
    
    snackbarColor.value = skyColor
    registerPrompt.value = '验证码已发送，请查收邮箱'
    snackbar.value = true
    
    // 开始倒计时
    countdown.value = 60
    countdownTimer = setInterval(() => {
      countdown.value--
      if (countdown.value <= 0) {
        if (countdownTimer) {
          clearInterval(countdownTimer)
          countdownTimer = null
        }
      }
    }, 1000)
    
  } catch (error: any) {
    snackbarColor.value = 'error'
    if (error.response?.status === 400) {
      registerPrompt.value = error.response.data || '该邮箱已被注册'
    } else if (error.response?.status === 429) {
      registerPrompt.value = '发送过于频繁，请稍后重试'
    } else {
      registerPrompt.value = '发送验证码失败，请稍后重试'
    }
    snackbar.value = true
    console.error('发送验证码失败：', error)
  }
  
  sendingCode.value = false
}

// 注册提交
async function onRegisterSubmit() {
  submitButtonLoading.value = true;

  try {
    await axiosInstance.post('/Account/Register', {
      userName: userAccount.value,
      password: password.value,
      email: email.value,
      verificationCode: verificationCode.value
    })
    
    snackbarColor.value = skyColor
    registerPrompt.value = '注册成功，请登录'
    snackbar.value = true
    
    // 清空表单
    email.value = ''
    verificationCode.value = ''
    userAccount.value = ''
    password.value = ''
    passwordConfirmed.value = ''
    
  } catch (error: any) {
    snackbarColor.value = 'error'
    let message = '注册失败'
    
    if (error.response?.data) {
      if (typeof error.response.data === 'string') {
        message = error.response.data
      } else if (error.response.data.detail) {
        message = error.response.data.detail
      }
    }
    
    registerPrompt.value = message
    snackbar.value = true
    console.error('注册失败：', error)
  }
  
  submitButtonLoading.value = false
}

</script>

<style scoped>
.subheader {
  font-size: 1.5rem;
  font-weight: bold;
  margin: 0 auto;
  display: block;
  text-align: center;
}

.form {
  width: 90%;
  margin: 20px auto;
  text-align: center;
}

.icon {
  display: flex;
  margin-top: 15px;
  justify-content: center;
  vertical-align: center;
}

.send-code-btn {
  margin-top: 12px;
  height: 40px;
  font-size: 12px;
}
</style>

