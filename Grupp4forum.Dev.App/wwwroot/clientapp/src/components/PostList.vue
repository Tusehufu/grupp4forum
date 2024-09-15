<template>
    <div class="row">
        <div v-for="post in posts" :key="post.postId" class="post">
            <div class="border">
                <h3>{{ post.title }}</h3>
                <p>{{ post.content }}</p>
                <p>{{ post.createdAt }}</p>
                <p>{{ post.updatedAt }}</p>
                <p>Skrivet av: {{ post.author }}</p>
                <p>Antal gillningar {{post.likes}}</p>
            </div>
            <button class="btn btn-danger" @click="showConfirmDeleteModal(post.postId)">Radera</button>
            <button class="btn btn-success" @click="likePost(post.postId)">Gilla</button>
            <ConfirmDeletePostModal :isVisible="isConfirmDeleteModalVisible" :postId="selectedPostIdToDelete" @confirm="deletePost" @cancel="closeConfirmDeleteModal" />
        </div>
    </div>
</template>

<script setup lang="ts">
    import { ref, onMounted, watch } from 'vue';
    import axios from 'axios';
    import ConfirmDeletePostModal from './ConfirmDeletePostModal.vue';

    // Typdefinition för Post
    interface Post {
        postId: number;
        title: string;
        content: string;
        createdAt: string;
        updatedAt: string;
        author: string;
        likes: number;
    }

    // Reaktiva variabler
    const isConfirmDeleteModalVisible = ref(false);
    const selectedPostIdToDelete = ref<number | null>(null);
    const posts = ref<Post[]>([]);

    // Funktion för att visa modalen för att bekräfta borttagning av ett inlägg
    const showConfirmDeleteModal = (postId: number) => {
        selectedPostIdToDelete.value = postId;
        isConfirmDeleteModalVisible.value = true;
    };

    // Funktion för att stänga bekräftelsemodalen för borttagning
    const closeConfirmDeleteModal = () => {
        selectedPostIdToDelete.value = null;
        isConfirmDeleteModalVisible.value = false;
    };

    // Funktion för att radera ett inlägg
    const deletePost = () => {
        closeConfirmDeleteModal();
        // Här kan du lägga till logik för att faktiskt radera inlägget från backend om det behövs
    };

    // Funktion för att hämta alla inlägg
    const fetchPosts = async () => {
        try {
            const response = await axios.get<Post[]>('https://localhost:7147/api/Post');
            posts.value = response.data;
        } catch (error) {
            console.error('Fel vid hämtning av inlägg:', error);
        }
    };

    const likePost = () => {


    }

    // Lifecycle hook för att hämta alla inlägg när komponenten monteras
    onMounted(async () => {
        await fetchPosts();
    });

    // Reagerar på ändringar i posts (om nödvändigt)
    watch(
        () => posts.value,
        async (newPosts) => {
            posts.value = newPosts;
        }
    );
</script>

<style scoped>
    /* Lägg till din CSS här om du vill */
</style>
