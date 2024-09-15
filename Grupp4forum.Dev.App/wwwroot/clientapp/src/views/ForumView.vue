<template>
    <div class="home">
        <h1 class="text-center">Forum</h1>
        <div class="container">
            <!-- Knappen som öppnar modalen -->
            <button class="btn btn-primary me-1" @click="openCreatePostModal">Skapa inlägg</button>

            <!-- Modalen styrs av isCreatePostModalVisible och lyssnar på close-eventet -->
            <CreatePostModal :isVisible="isCreatePostModalVisible" @close="closeCreatePostModal" />

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

    // Hämtar inlägg från API
    const fetchPosts = async () => {
        try {
            const response = await axios.get('https://localhost:7147/api/Post');
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
    });
</script>

<style scoped>
</style>
