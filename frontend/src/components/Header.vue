<template>
    <el-page-header>
        <template #content>
            <h1>Aioniq</h1>
        </template>

        <template #extra>
            <template v-if="isLoggedIn">
                <div class="d-flex align-items-center">
                    <span class="text-large">{{ userData.name }}</span>
                    <el-avatar class="ms-2" :src="userData.picture" />
                </div>
            </template>
            <template v-else>
                <div ref="signinButtonRef" @click="signInWithGoogle"></div>
                <el-button @click="hackLogin">Hack login</el-button>
            </template>
        </template>
    </el-page-header>
</template>

<script lang="ts" setup>
import { useUserStore } from '@/stores/userStore';
import { onMounted, ref } from 'vue';
import { computed } from 'vue';
import { googleSdkLoaded } from 'vue3-google-login';
import axios from 'axios';

const userStore = useUserStore();

const isLoggedIn = computed(() => {
    return userStore.user !== null;
});

const userData = computed(() => {
    return userStore.user!;
})

const signinButtonRef = ref();

const hackLogin = () => {
    axios.post("/api/Auth/HackLogMeIn");
}

onMounted(() => {
    googleSdkLoaded(google => {
        google.accounts.id.renderButton(signinButtonRef.value, {});
    });
})

const signInWithGoogle = () => {
    const redirectUri = location.href;
    googleSdkLoaded(google => {
        google.accounts.oauth2
            .initCodeClient({
                client_id: userStore.clientId,
                scope: 'email profile openid https://www.googleapis.com/auth/tasks.readonly',
                redirect_uri: redirectUri,
                callback: response => {
                    if (response.code) {
                        userStore.loginOrRegister(response.code, redirectUri)
                    }
                },
            })
            .requestCode()
    })
}
</script>
