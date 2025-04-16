<template>
    <div class="login-container">
        <h1>Sign In</h1>
        <button @click="signInWithGoogle" class="google-btn">
            <!--<img src="@/assets/google-icon.svg" alt="Google" />-->
            Sign in with Google
        </button>
    </div>
</template>

<script>
export default {
    name: 'LoginView',
    methods: {
        signInWithGoogle() {
            this.$auth.login();
        }
    }
}
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
