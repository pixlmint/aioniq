import { createRouter, createWebHistory } from 'vue-router';
import HomeView from '../views/HomeView.vue';
import LoginView from '../components/Login.vue';
import AuthCallback from '../components/AuthCallback.vue';
import CalendarView from '../components/Calendar.vue';
// import DashboardView from '../views/DashboardView.vue';

const router = createRouter({
    history: createWebHistory(import.meta.env.BASE_URL),
    routes: [
        {
            path: '/',
            name: 'home',
            component: HomeView
        },
        {
            path: '/login',
            name: 'login',
            component: LoginView
        },
        {
            path: '/auth-callback',
            name: 'auth-callback',
            component: AuthCallback
        },
        {
            path: '/calendar',
            name: 'calendar',
            component: CalendarView,
            meta: { requiresAuth: true }
        },
        // {
        //     path: '/dashboard',
        //     name: 'dashboard',
        //     component: DashboardView,
        //     meta: { requiresAuth: true }
        // },
    ]
});

// Navigation guard for protected routes
router.beforeEach((to, from, next) => {
    const isAuthenticated = localStorage.getItem('jwt_token') !== null;

    if (to.matched.some(record => record.meta.requiresAuth) && !isAuthenticated) {
        next('/login');
    } else {
        next();
    }
});

export default router
