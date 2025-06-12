<template>
    <div class="login-container">
        <h1>Sign In</h1>
        <!--<a :href="loginUrl">Login</a>
        <button @click="signInWithGoogle" class="google-btn">
            Sign in with Google
        </button>
        <GoogleLogin :callback="googleLoginCallback" />
        <button :disabled="!isReady" @click="() => login()">Login using Google</button>-->
    </div>
</template>

<script lang="ts" setup>
/*import { computed } from "vue";
import {
    useCodeClient,
    decodeCredential,
    type ImplicitFlowSuccessResponse,
    type ImplicitFlowErrorResponse,
} from "vue3-google-signin";

const googleLoginSuccess = function(response: ImplicitFlowSuccessResponse) {
    console.log(response);
    const { credential } = response;
    const decodedCredential = decodeCredential(credential);
    console.log("decoded:", decodedCredential);
}

const googleLoginError = function(response: ImplicitFlowErrorResponse) {
    console.log(response);
}

const { isReady, login } = useCodeClient({
    onSuccess: googleLoginSuccess,
    onError: googleLoginError,
    scope: "https://www.googleapis.com/auth/calendar.readonly profile"
})*/
</script>

<style scoped>
.login-container {
    max-width: 400px;
    margin: 0 auto;
    padding: 20px;
    text-align: center;
}

.google-btn {
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 10px;
    background-color: white;
    border: 1px solid #ccc;
    border-radius: 4px;
    padding: 10px 15px;
    font-size: 16px;
    cursor: pointer;
    transition: background-color 0.3s;
}

.google-btn:hover {
    background-color: #f5f5f5;
}

.google-btn img {
    width: 20px;
    height: 20px;
}
</style>
