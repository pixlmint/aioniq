<template>
    <template v-if="initDone">
        <Header />
        <!--<header>
        <div class="wrapper">
            <nav>
                <RouterLink to="/">Home</RouterLink>
            </nav>
        </div>
    </header>-->
        <RouterView />
    </template>
</template>

<script setup lang="ts">
import { computed, onMounted } from 'vue';
import Header from './components/Header.vue';
import { useUserStore } from './stores/userStore';
import { ElLoading } from 'element-plus';

const userStore = useUserStore();

onMounted(() => {
    console.log("mounted...");
    const loading = ElLoading.service({
        lock: true,
        text: 'Loading',
        background: 'rgba(0, 0, 0, 0.7)',
    });
    userStore.init();

    const loadingCheckInterval = window.setInterval(function() {
        if (userStore.initDone) {
            loading.close();
            window.clearInterval(loadingCheckInterval);
        }
    }, 100);
});

const initDone = computed(() => {
    return userStore.initDone;
});
</script>
