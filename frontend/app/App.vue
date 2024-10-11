<template>
    <el-button @click="signInWithGoogle">Login</el-button>
</template>

<script lang="ts" setup>
import { decodeCredential, googleSdkLoaded } from 'vue3-google-login';
import { useUserStore } from '@/store/userStore'
import { storeToRefs } from 'pinia';

const userStore = useUserStore();
const {clientId} = storeToRefs(userStore)
const { fetchUserDataFrom } = userStore

const callback = function (response) {
    console.log(response);
    const userData = decodeCredential(response.credential)
    console.log("Handle the userData", userData)
}

const signInWithGoogle = () => {
    googleSdkLoaded(google => {
        google.accounts.oauth2
            .initCodeClient({
                client_id: clientId.value,
                scope: 'email profile openid https://www.googleapis.com/auth/tasks.readonly',
                redirect_uri: location.origin,
                callback: response => {
                    console.log(response);
                    if (response.code)
                        fetchUserDataFrom(response.code)
                },
            })
            .requestCode()
    })
}
</script>
