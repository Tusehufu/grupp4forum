<template>
    <div class="home">
        <h1 class="text-center">Forum</h1>
        <div class="container">
            <!-- Knappen som öppnar modalen -->
            <button v-if="isLoggedIn" class="btn btn-primary me-1" @click="openCreatePostModal">Skapa inlägg</button>

            <!-- Modalen styrs av isCreatePostModalVisible och lyssnar på close-eventet -->
            <CreatePostModal :isVisible="isCreatePostModalVisible" @close="closeCreatePostModal" @postCreated="fetchPosts" />

            <!-- Lista över inlägg -->
            <PostList :posts="posts" />
        </div>
    </div>
</template>

<script setup lang="ts">
    import { ref, onMounted } from 'vue';
    import axios from 'axios';
    import PostList from '../components/PostList.vue';
    import CreatePostModal from '../components/CreatePostModal.vue';

    // Typdefinition för en Post
    interface Post {
        id: number;
        title: string;
        content: string;
    }

    const posts = ref<Post[]>([]);
    const isCreatePostModalVisible = ref(false);
    const isLoggedIn = ref(false);

    // Kontrollera om användaren är inloggad genom att kontrollera om JWT-token finns i localStorage
    const checkLoginStatus = () => {
        const token = localStorage.getItem('jwtToken');
        isLoggedIn.value = !!token;  // Sätt true om token finns, annars false
    };


    // Hämtar inlägg från API
    const fetchPosts = async () => {
        try {
            const response = await axios.get('https://grupp4forumdevapp20240923094105.azurewebsites.net/api/Post');
            posts.value = response.data;
        } catch (error) {
            console.error('Fel vid hämtning av inlägg:', error);
        }
    };

    // Öppnar modalen för att skapa inlägg
    const openCreatePostModal = () => {
        isCreatePostModalVisible.value = true;
    };

    // Stänger modalen för att skapa inlägg
    const closeCreatePostModal = () => {
        isCreatePostModalVisible.value = false;
    };

    // Körs när komponenten monteras
    onMounted(() => {
        fetchPosts();
        checkLoginStatus(); 
    });
</script>

<style scoped>
</style>
