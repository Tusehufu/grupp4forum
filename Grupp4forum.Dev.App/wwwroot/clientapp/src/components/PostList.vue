<template>
    <div class="row">
        <div v-for="post in posts" :key="post.postId" class="post">
            <div class="border">
                <h3>{{ post.title }}</h3>
                <p>{{ post.content }}</p>
                <p>Skapat: {{ post.createdAt }}</p>
                <p>Uppdaterades senast: {{ post.updatedAt }}</p>
                <p>Skrivet av: {{ post.author }}</p>
                <p>Antal gillningar: {{ post.likes }}</p>
            </div>

            <!-- Radera-knapp -->
            <button class="btn btn-danger" @click="showConfirmDeleteModal(post.postId)">Radera</button>

            <!-- Gilla-knapp -->
            <button class="btn btn-success" @click="likePost(post.postId)">Gilla</button>

            <!-- Modal för att bekräfta borttagning av inlägg -->
            <ConfirmDeletePostModal v-if="isConfirmDeleteModalVisible && selectedPostIdToDelete !== null"
                                    :isVisible="isConfirmDeleteModalVisible"
                                    :postId="selectedPostIdToDelete"
                                    @confirm="fetchPosts"
        @cancel="closeConfirmDeleteModal"/>

      <!-- Svara-knapp -->
      <button class="btn btn-primary" @click="openCreateReplyModal(post.postId, null)">Svara</button>

      <!-- Visa alla replies under posten -->
      <div class="replies" v-if="post.replies && post.replies.length > 0">
        <h4>Svar:</h4>
        <div v-for="reply in post.replies" :key="reply.replyId" class="reply">
          <p>{{ reply.content }} - <em>{{ reply.author }}</em> ({{ reply.createdAt }})</p>
        </div>
      </div>

      <!-- Modal för att skapa svar -->
      <CreateReplyModal
        :isVisible="isCreateReplyModalVisible"
        :postId="selectedPostId"
        :parentReplyId="selectedParentReplyId"
        @close="closeCreateReplyModal"
      />
        </div>
    </div>
</template>

<script setup lang="ts">
    import { ref, onMounted } from 'vue';
    import axios from 'axios';
    import ConfirmDeletePostModal from './ConfirmDeletePostModal.vue';
    import CreateReplyModal from './CreateReplyModal.vue';

    // Typdefinition för Post och Reply
    interface Post {
        postId: number;
        title: string;
        content: string;
        createdAt: string;
        updatedAt: string;
        author: string;
        likes: number;
        replies?: Reply[];
    }

    interface Reply {
        replyId: number;
        content: string;
        author: string;
        timestamp: string;
        postId: number;
    }

    // Reaktiva variabler
    const isConfirmDeleteModalVisible = ref(false);
    const isCreateReplyModalVisible = ref(false);
    const selectedPostIdToDelete = ref<number | null>(null);
    const selectedPostId = ref<number | null>(null);
    const selectedParentReplyId = ref<number | null>(null);
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

    // Funktion för att hämta alla inlägg och deras replies
    const fetchPosts = async () => {
        try {
            const response = await axios.get<Post[]>('https://localhost:7147/api/Post');
            const postsWithReplies = await Promise.all(
                response.data.map(async (post) => {
                    const replies = await fetchRepliesForPost(post.postId);
                    return { ...post, replies };
                })
            );
            posts.value = postsWithReplies;
        } catch (error) {
            console.error('Fel vid hämtning av inlägg:', error);
        }
    };

    // Funktion för att hämta alla replies för ett specifikt postId
    const fetchRepliesForPost = async (postId: number) => {
        try {
            const response = await axios.get<Reply[]>(`https://localhost:7147/api/replies/post/${postId}`);
            return response.data;
        } catch (error) {
            console.error('Fel vid hämtning av replies:', error);
            return [];
        }
    };

    // Funktion för att gilla ett inlägg
    const likePost = async (postId: number) => {
        try {
            await axios.post(`https://localhost:7147/api/Post/${postId}/like`);
            await fetchPosts(); // Uppdatera listan efter att ha gillat
        } catch (error) {
            console.error('Fel vid gillning av inlägg:', error);
        }
    };

    // Funktion för att öppna modal för att svara på ett inlägg eller svar
    const openCreateReplyModal = (postId: number | null, parentReplyId: number | null) => {
        selectedPostId.value = postId;
        selectedParentReplyId.value = parentReplyId;
        isCreateReplyModalVisible.value = true;
    };

    // Funktion för att stänga modal för att skapa svar
    const closeCreateReplyModal = () => {
        isCreateReplyModalVisible.value = false;
        selectedPostId.value = null;
        selectedParentReplyId.value = null;
    };

    // Lifecycle hook för att hämta alla inlägg när komponenten monteras
    onMounted(async () => {
        await fetchPosts();
    });
</script>

<style scoped>
    /* Din CSS här */
</style>
