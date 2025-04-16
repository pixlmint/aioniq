<template>
    <div class="callback-container">
        <div v-if="loading">
            <p>Logging you in...</p>
        </div>
        <div v-else-if="error">
            <p>Error during authentication: {{ error }}</p>
        </div>
    </div>
</template>

<script>
export default {
    name: 'AuthCallback',
    data() {
        return {
            loading: true,
            error: null
        };
    },
    async mounted() {
        try {
            // Get the tokens from URL params
            const params = new URLSearchParams(window.location.search);
            const token = params.get('token');
            const googleAccessToken = params.get('googleAccessToken');

            if (!token) {
                throw new Error('No authentication token received');
            }

            // Process the authentication
            await this.$auth.handleCallback(token, googleAccessToken);

            // Redirect to home or dashboard
            this.$router.push('/dashboard');
        } catch (err) {
            this.error = err.message;
        } finally {
            this.loading = false;
        }
    }
}
</script>

<style scoped>
.callback-container {
    max-width: 400px;
    margin: 100px auto;
    padding: 20px;
    text-align: center;
}
</style>
