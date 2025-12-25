<template>
  <v-container>
    <v-row>
      <v-form class="form" :fast-fail="true" @submit.prevent>
        <v-container>
          <v-row>
            <v-col class="icon" cols="1">
              <v-icon icon="mdi-account" color="#4CAF50"></v-icon>
            </v-col>
            <v-col cols="11">
              <v-text-field 
                v-model="userAccount" 
                :rules="[notNullRule]" 
                label="管理员账号"
                variant="outlined"
                density="comfortable"
                bg-color="rgba(255,255,255,0.05)"
                color="#4CAF50"
                base-color="rgba(255,255,255,0.5)"
              />
            </v-col>
          </v-row>
          <v-row>
            <v-col class="icon" cols="1">
              <v-icon icon="mdi-lock" color="#4CAF50"></v-icon>
            </v-col>
            <v-col cols="11">
              <v-text-field 
                v-model="password" 
                :rules="[notNullRule]" 
                label="密码"
                variant="outlined"
                density="comfortable"
                bg-color="rgba(255,255,255,0.05)"
                color="#4CAF50"
                base-color="rgba(255,255,255,0.5)"
                :append-inner-icon="showPassword ? 'mdi-eye' : 'mdi-eye-off'"
                :type="showPassword ? 'text' : 'password'"
                @click:append-inner="showPassword = !showPassword" 
              />
            </v-col>
          </v-row>
          <v-row>
            <v-btn 
              color="#4CAF50" 
              class="w-75 mx-auto login-btn" 
              type="submit" 
              :disabled="submitButtonDisabled"
              :loading="submitButtonLoading" 
              @click="onLoginSubmit"
              size="large"
            >
              <v-icon icon="mdi-login" class="mr-2"></v-icon>
              登录管理后台
            </v-btn>
          </v-row>
          <v-row class="mt-4">
            <v-col class="text-center">
              <router-link to="/" class="user-link">
                <v-icon icon="mdi-arrow-left" size="small"></v-icon>
                返回用户登录
              </router-link>
            </v-col>
          </v-row>
        </v-container>
      </v-form>
    </v-row>
    <v-snackbar v-model="snackbar" timeout="5000" rounded="pill" :color="snackbarColor">
      {{ loginPrompt }}
    </v-snackbar>
  </v-container>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router';
import axiosInstance from '@/plugins/util/axiosInstance';

const userAccount = ref<string>('')
const password = ref<string>('')

const snackbar = ref<boolean>(false)
const snackbarColor = ref<string>('#4CAF50')
const loginPrompt = ref<string>('')

const router = useRouter()

const submitButtonDisabled = ref(true)
const submitButtonLoading = ref(false)
const showPassword = ref(false)

const notNullRule = (value: string | any[]) => {
  if (value !== '') {
    submitButtonDisabled.value = false;
    return true;
  } else {
    submitButtonDisabled.value = true;
    return '请填入信息';
  }
}

function onLoginSubmit() {
  submitButtonLoading.value = true;

  const url = '/Account/Login'
  axiosInstance.post(url, {
    account: userAccount.value,
    password: password.value,
    loginType: 'admin'  // 管理员登录类型
  }).then((response) => {
    const { userName: name, token: token, role: userType, avatar: avatar } = response.data;
    window.localStorage.setItem("name", name);
    window.localStorage.setItem("token", token);
    window.localStorage.setItem("avatar", avatar);

    if (userType === 'Admin' || userType === 'Moderator') {
      router.push('/admin');
    } else {
      loginPrompt.value = "您不是管理员，请使用用户入口登录";
      snackbarColor.value = '#e74c3c';
      snackbar.value = true;
      window.localStorage.removeItem("token");
    }
  }).catch((error) => {
    console.error(error);
    let message;
    if (error.response?.status === 403) {
      message = error.response.data || "您不是管理员，请使用用户入口登录";
    } else if (error.response?.status === 401) {
      message = "密码错误";
    } else if (error.response?.status === 404) {
      message = "账号不存在";
    } else {
      message = error.message;
    }
    loginPrompt.value = message;
    snackbarColor.value = '#e74c3c';
    snackbar.value = true;
  }).finally(() => {
    submitButtonLoading.value = false;
  });
}
</script>

<style scoped>
.form {
  width: 100%;
  margin: 0 auto;
  text-align: center;
}

.icon {
  display: flex;
  margin-top: 15px;
  justify-content: center;
  vertical-align: center;
}

.login-btn {
  font-weight: bold;
  letter-spacing: 2px;
}

.user-link {
  color: rgba(255, 255, 255, 0.6);
  text-decoration: none;
  font-size: 0.9rem;
  transition: color 0.3s;
}

.user-link:hover {
  color: #4CAF50;
}
</style>
