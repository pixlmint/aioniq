import gAuthPlugin from 'vue3-google-login';
import axios from 'axios';

const apiBaseUrl = 'https://localhost:5001'; // Your .NET backend URL

export default {
    install(app, options) {
        // Configure Google OAuth plugin
        app.use(gAuthPlugin, {
            clientId: '804363148845-4mui1b168btsdojprj0ddsf0tfttkmlc.apps.googleusercontent.com',
            scope: 'email profile https://www.googleapis.com/auth/calendar',
            prompt: 'consent',
        });

        // Add authentication service to the app
        app.config.globalProperties.$auth = {
            async login() {
                try {
                    // Redirect to the backend for authentication
                    window.location.href = `${apiBaseUrl}/api/auth/signin-google`;
                } catch (error) {
                    console.error('Google login error:', error);
                    throw error;
                }
            },

            async handleCallback(token, googleAccessToken) {
                // Store the tokens
                localStorage.setItem('jwt_token', token);
                localStorage.setItem('google_token', googleAccessToken);

                // Configure axios for future requests
                axios.defaults.headers.common['Authorization'] = `Bearer ${token}`;

                // Fetch user data
                try {
                    const response = await axios.get(`${apiBaseUrl}/api/auth/user`);
                    return response.data;
                } catch (error) {
                    console.error('Error fetching user data:', error);
                    throw error;
                }
            },

            isAuthenticated() {
                return !!localStorage.getItem('jwt_token');
            },

            logout() {
                localStorage.removeItem('jwt_token');
                localStorage.removeItem('google_token');
                axios.defaults.headers.common['Authorization'] = '';
            }
        };
    }
};
