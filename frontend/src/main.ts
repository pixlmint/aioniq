import 'bootstrap/dist/css/bootstrap-utilities.css'
import './assets/main.css'
import 'element-plus/dist/index.css'

import { createApp } from 'vue'
import { createPinia } from 'pinia'
import ElementPlus from 'element-plus'

import App from './App.vue'
import router from './router'
import axios from 'axios'

import localstorageWrapper from './services/localstorageWrapper'

const app = createApp(App)

axios.interceptors.response.use(
    response => response,
    error => {
        if (error.response && error.response.status === 401) {
            router.push('/login');
        }
        return Promise.reject(error);
    }
)

axios.interceptors.request.use(config => {
    const token = localstorageWrapper.getItem('jwt');
    if (token) {
        config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
})

app.use(ElementPlus)
app.use(createPinia())
app.use(router)

app.mount('#app')
