<template>
    <div class="row">
        <div class="d-flex justify-content-between mb-3">
            <div class="dropdown">
                <button class="btn btn-secondary dropdown-toggle"
                        type="button"
                        id="sortDropdown"
                        data-bs-toggle="dropdown"
                        aria-expanded="false">
                    Sortera Inlägg
                </button>
                <ul class="dropdown-menu" aria-labelledby="sortDropdown">
                    <!-- Dynamiskt genererade kategorier i dropdown -->
                    <li v-for="category in categories" :key="category.categoryId">
                        <a class="dropdown-item" href="#" @click.prevent="sortPostsByCategory(category.categoryId)">
                            {{ category.name }}
                        </a>
                    </li>
                    <li><hr class="dropdown-divider"></li>
                    <li><a class="dropdown-item" href="#" @click.prevent="sortPosts('likes')">Antal Gillningar</a></li>
                    <li><a class="dropdown-item" href="#" @click.prevent="sortPosts('latest')">Senaste Inlägg</a></li>
                    <li><a class="dropdown-item" href="#" @click.prevent="sortPosts('oldest')">Tidigaste Inlägg</a></li>
                    <li><a class="dropdown-item" href="#" @click.prevent="resetPosts">Återställ Filter</a></li>
                </ul>
            </div>
        </div>

        <!-- Visa endast inlägg på den aktuella sidan -->
        <div v-for="post in paginatedPosts" :key="post.postId" class="post">
            <div class="postborder">
                <h3 class="posttitle">{{ post.title }}</h3>
                <p class="postcontent">{{ post.content }}</p>
                <div v-if="post.imageBase64">
                    <img :src="'data:image/jpeg;base64,' + post.imageBase64" alt="Post Image" class="img-fluid post-image" />
                </div>
                <div class="postcontainer">
                    <div class="left-section">
                        <div class="leftsectioncontainer">
                            <p class="postcategory">Kategori: {{ post.categoryName }}</p>
                            <p class="postauthor">Skrivet av: {{ post.author }}</p>
                        </div>
                    </div>
                    <div class="middle-section">
                        <div class="middlesectioncontainer">
                            <p class="postlikes">Antal gillningar: {{ post.likes }}</p>
                        </div>
                    </div>
                    <div class="right-section">
                        <div class="rightsectioncontainer">
                            <p class="postcreatedat">Skapades: {{ formatDate(post.createdAt) }}</p>
                            <p class="postlastupdate">Uppdaterad: {{ formatDate(post.updatedAt) }}</p>
                        </div>
                    </div>
                </div>
                <div class="buttoncontainer">
                    <!-- Gilla-knapp -->
                    <button class="btn btnlike" @click="likePost(post.postId)">Gilla</button>

                    <!-- Svara-knapp för post -->
                    <button class="btn btnanswer" @click="openCreateReplyModal(post.postId, null)">Svara</button>

                    <!-- Radera-knapp -->
                    <button class="btn btn-danger" @click="showConfirmDeleteModal(post.postId)">Radera</button>

                    <!-- Gul redigera-knapp -->
                    <button class="btn btn-warning" @click="openEditPostModal(post)">Redigera</button>
                </div>
            </div>

            <!-- Accordion för att visa alla replies under posten -->
            <div v-if="post.replies && post.replies.length > 0" class="accordion" id="accordionReplies">
                <div class="accordion-item">
                    <h2 class="accordion-header" :id="'heading' + post.postId">
                        <button class="accordion-button collapsed"
                                type="button"
                                data-bs-toggle="collapse"
                                :data-bs-target="'#collapse' + post.postId"
                                aria-expanded="false"
                                :aria-controls="'collapse' + post.postId">
                            Svar
                        </button>
                    </h2>
                    <div :id="'collapse' + post.postId"
                         class="accordion-collapse collapse"
                         :aria-labelledby="'heading' + post.postId"
                         data-bs-parent="#accordionReplies">
                        <div class="accordion-body">
                            <div v-for="reply in post.replies" :key="reply.replyId" class="reply">
                                <p>{{ formatDate(reply.createdAt) }}</p>
                                <p>{{ reply.content }} - <em>{{ reply.author }}</em> ({{ formatDate(reply.updatedAt) }})</p>
                                <p>Gillningar: {{ reply.likes }}</p>
                                <!-- Visa bilden om den finns -->
                                <div v-if="reply.imageBase64">
                                    <img :src="'data:image/jpeg;base64,' + reply.imageBase64" alt="Reply Image" class="img-fluid post-image" />
                                </div>
                                <!-- Svara-knapp för reply -->
                                <button class="btn btn-secondary" @click="openCreateReplyModal(post.postId, reply.replyId)">
                                    Svara på detta svar
                                </button>
                                <button class="btn btn-danger" @click="openDeleteReplyModal(reply.replyId)">Radera</button>
                                <!-- Redigera-knapp för reply -->
                                <button class="btn btn-warning" @click="openEditReplyModal(reply)">Redigera</button>
                                <button class="btn btn-success" @click="likeReply(reply.replyId)">Gilla</button>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <!-- Pagination controls -->
        <div class="pagination-controls">
            <button @click="prevPage" :disabled="currentPage === 1" class="btn btn-secondary">Föregående</button>
            <span>Sida {{ currentPage }} av {{ totalPages }}</span>
            <button @click="nextPage" :disabled="currentPage === totalPages" class="btn btn-secondary">Nästa</button>
        </div>

        <!-- Edit Post Modal -->
        <EditPostModal v-if="isEditPostModalVisible"
                       :isVisible="isEditPostModalVisible"
                       :post="selectedPost"
                       @close="closeEditPostModal"
                       @postUpdated="fetchPosts" />

        <!-- Modal för att skapa svar -->
        <CreateReplyModal :isVisible="isCreateReplyModalVisible"
                          :postId="selectedPostId"
                          :parentReplyId="selectedParentReplyId"
                          @close="closeCreateReplyModal" />

        <!-- Edit Reply Modal -->
        <EditReplyModal v-if="isEditReplyModalVisible"
                        :isVisible="isEditReplyModalVisible"
                        :reply="selectedReply"
                        @close="closeEditReplyModal"
                        @replyUpdated="fetchPosts" />

        <!-- Modal för att bekräfta borttagning av inlägg -->
        <ConfirmDeletePostModal v-if="isConfirmDeleteModalVisible && selectedPostIdToDelete !== null"
                                :isVisible="isConfirmDeleteModalVisible"
                                :postId="selectedPostIdToDelete"
                                @confirm="fetchPosts"
                                @cancel="closeConfirmDeleteModal" />
    </div>
</template>

<script setup lang="ts">
    import { ref, computed, onMounted } from 'vue';
    import axios from 'axios';
    import ConfirmDeletePostModal from './ConfirmDeletePostModal.vue';
    import ConfirmDeleteReplyModal from './ConfirmDeleteReplyModal.vue';
    import CreateReplyModal from './CreateReplyModal.vue';
    import EditPostModal from './EditPostModal.vue';
    import EditReplyModal from './EditReplyModal.vue'

    // Typdefinition för Post, Reply och Category
    interface Post {
        postId: number;
        title: string;
        content: string;
        createdAt: string;
        updatedAt: string;
        author: string;
        likes: number;
        categoryId: number;
        categoryName?: string;
        imageBase64?: string;
        replies?: Reply[];
    }

    interface Reply {
        replyId: number;
        content: string;
        author: string;
        createdAt: string;
        postId: number;
        likes: number;
        imageBase64?: string;
    }

    interface Category {
        categoryId: number;
        name: string;
    }

    // Reaktiva variabler
    const posts = ref<Post[]>([]);
    const allPosts = ref<Post[]>([]); // Kopia för att hålla alla inlägg
    const categories = ref<Category[]>([]);
    const currentPage = ref(1);
    const postsPerPage = 20;

    // Paginering
    const totalPages = computed(() => Math.ceil(posts.value.length / postsPerPage));

    const paginatedPosts = computed(() => {
        const start = (currentPage.value - 1) * postsPerPage;
        const end = start + postsPerPage;
        return posts.value.slice(start, end);
    });

    // Datumformatfunktion som returnerar YYYY-MM-DD
    const formatDate = (dateString: string) => {
        return dateString.split('T')[0];
    };

    const prevPage = () => {
        if (currentPage.value > 1) {
            currentPage.value--;
        }
    };

    const nextPage = () => {
        if (currentPage.value < totalPages.value) {
            currentPage.value++;
        }
    };

    // Modal- och likes-relaterade variabler och funktioner
    const isConfirmDeleteModalVisible = ref(false);
    const isConfirmDeleteReplyModalVisible = ref(false);
    const isCreateReplyModalVisible = ref(false);
    const selectedPostIdToDelete = ref<number | null>(null);
    const selectedReplyIdToDelete = ref<number | null>(null);
    const selectedPostId = ref<number | null>(null);
    const selectedParentReplyId = ref<number | null>(null);
    const isEditPostModalVisible = ref(false);
    const selectedPost = ref(null);
    const isEditReplyModalVisible = ref(false);
    const selectedReply = ref(null);

    // Funktion för att öppna EditPostModal
    const openEditPostModal = (post) => {
        selectedPost.value = post;
        isEditPostModalVisible.value = true;
    };

    // Funktion för att stänga EditPostModal
    const closeEditPostModal = () => {
        isEditPostModalVisible.value = false;
    };

    // Funktion för att öppna CreateReplyModal för att svara på en post eller ett svar
    const openCreateReplyModal = (postId: number, parentReplyId: number | null) => {
        selectedPostId.value = postId;
        selectedParentReplyId.value = parentReplyId;
        isCreateReplyModalVisible.value = true;
    };

    // Funktion för att stänga CreateReplyModal
    const closeCreateReplyModal = () => {
        isCreateReplyModalVisible.value = false;
        selectedPostId.value = null;
        selectedParentReplyId.value = null;
    };

    // Funktion för att öppna EditReplyModal
    const openEditReplyModal = (reply) => {
        selectedReply.value = reply;
        isEditReplyModalVisible.value = true;
    };

    // Funktion för att stänga EditReplyModal
    const closeEditReplyModal = () => {
        isEditReplyModalVisible.value = false;
    };

    // Funktion för att sortera inlägg
    const sortPosts = (sortBy: string) => {
        posts.value = [...allPosts.value]; // Återställ till alla inlägg
        switch (sortBy) {
            case 'likes':
                posts.value.sort((a, b) => b.likes - a.likes);
                break;
            case 'latest':
                posts.value.sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime());
                break;
            case 'oldest':
                posts.value.sort((a, b) => new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime());
                break;
        }
        currentPage.value = 1; // Återställ till första sidan efter sortering
    };

    // Sortera efter kategori
    const sortPostsByCategory = (categoryId: number) => {
        posts.value = allPosts.value.filter(post => post.categoryId === categoryId);
        currentPage.value = 1; // Återställ till första sidan efter sortering
    };

    // Återställ alla inlägg
    const resetPosts = () => {
        posts.value = [...allPosts.value];
        currentPage.value = 1; // Återställ till första sidan
    };

    // Funktion för att hämta alla kategorier
    const fetchCategories = async () => {
        try {
            const response = await axios.get<Category[]>('https://localhost:7147/api/Category');
            categories.value = response.data;
        } catch (error) {
            console.error('Fel vid hämtning av kategorier:', error);
        }
    };

    // Funktion för att hämta alla inlägg och deras replies
    const fetchPosts = async () => {
        try {
            const response = await axios.get<Post[]>('https://localhost:7147/api/Post');
            const postsWithReplies = await Promise.all(
                response.data.map(async (post) => {
                    const replies = await fetchRepliesForPost(post.postId);
                    const category = categories.value.find(cat => cat.categoryId === post.categoryId);

                    // Om API:t returnerar base64-sträng för bilden, inkludera den
                    return {
                        ...post,
                        replies,
                        categoryName: category ? category.name : 'Okänd kategori',
                        imageBase64: post.imageBase64 // Lägg till base64-strängen för bilden
                    };
                })
            );
            posts.value = postsWithReplies;
            allPosts.value = postsWithReplies; // Spara en kopia av alla inlägg
        } catch (error) {
            console.error('Fel vid hämtning av inlägg:', error);
        }
    };


    // Funktion för att hämta alla replies för ett specifikt postId
    const fetchRepliesForPost = async (postId: number) => {
        try {
            const response = await axios.get<Reply[]>(`https://localhost:7147/api/replies/post/${postId}`);

            // Om API:t returnerar Base64-sträng för bilden, inkludera den i varje reply
            return response.data.map(reply => ({
                ...reply,
                imageBase64: reply.imageBase64 || null // Om Base64-strängen finns, inkludera den
            }));
        } catch (error) {
            console.error('Fel vid hämtning av replies:', error);
            return [];
        }
    };


    // Lifecycle hook för att hämta alla inlägg och kategorier när komponenten monteras
    onMounted(async () => {
        await fetchCategories();
        await fetchPosts();
    });
</script>

<style scoped>
    .pagination-controls {
        margin-top: 20px;
        text-align: center;
    }

    .post-image {
        max-width: 300px; /* Maximal bredd */
        max-height: 300px; /* Maximal höjd */
        object-fit: contain; /* Behåll bildens proportioner */
    }


    .post, .reply {
        border: 1px solid #ddd;
        border-radius: 0.375rem;
        padding: 1rem;
        margin-bottom: 1rem;
        background-color: #fff;
    }
    /*Text inside posts*/

    .postcontainer {
        display: flex;
        justify-content: space-between;
        align-items: flex-start;
        height: auto;
        padding: 0.5rem;
        max-height: 4rem;
    }



        .postcontainer > div {
            display: flex;
            flex-direction: column;
        }

    .leftsectioncontainer {
        border: 1px solid black;
        border-radius: 10px;
        padding: 0.20rem;
    }

    .left-section {
        display: flex;
        flex-direction: column;
        gap: 0.5rem;
    }

    .middlesectioncontainer {
        border: 1px solid black;
        border-radius: 10px;
        padding: 0.20rem;
    }

    .middle-section {
        display: flex;
        flex-direction: column;
        gap: 0.5rem;
    }

    .rightsectioncontainer {
        border: 1px solid black;
        border-radius: 10px;
        padding: 0.20rem;
    }

    .right-section {
        display: flex;
        flex-direction: column;
        gap: 0.5rem;
    }

    /* Reset margins for items */
    .postcategory, .postcreatedat, .postlastupdate, .postauthor {
        margin: 0;
    }

    .posttitle {
        background-color: #6ECB63;
        font-weight: bold;
        border-radius: 10px;
    }

    .postcontent{
        margin-top:1rem;
        border:1px solid black;
        border-radius:5px;
    }



    /* Button Styles */

    .buttoncontainer {
        margin-bottom: 10px;
        display: flex;
        justify-content: flex-start;
        padding: 1rem;
    }

    .btnlike {
        background-color: #6ECB63;
        color: black;
        border: 1px solid transparent;
        align-content: flex-start;
    }

        .btnlike:hover {
            background-color: #218838;
            border-color: #1e7e34;
            color: black;
        }

    .btnanswer {
        background-color: #6ECB63;
        color: black;
        border: 1px solid transparent;
        align-content: flex-start;
    }

        .btnanswer:hover {
            background-color: #218838;
            border-color: #1e7e34;
            color: black;
        }

    .btn {
        margin-right: 0.5rem;
    }

    /* Sorting Dropdown */
    .dropdown-menu {
        min-width: 200px;
    }

    .dropdown-item {
        padding: 0.5rem 1rem;
    }

    /* Accordion Styles */
    .accordion-button {
        border-radius: 0.375rem;
    }

    .accordion-body {
        padding: 1rem;
    }

    /* Pagination Controls */
    .pagination-controls {
        margin-top: 1rem;
        text-align: center;
    }

        .pagination-controls .btn {
            margin: 0 0.5rem;
        }

    /* Modal Styles */
    .modal-dialog {
        max-width: 90%;
    }

    .modal-content {
        border-radius: 0.375rem;
    }

    /* Responsive Design */
    @media (max-width: 768px) {
        .btn {
            display: block;
            margin-bottom: 0.5rem;
        }

        .pagination-controls {
            display: block;
        }
    }
</style>
