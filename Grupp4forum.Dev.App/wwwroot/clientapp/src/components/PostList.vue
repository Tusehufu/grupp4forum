﻿
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
                    <button v-if="isLoggedIn" class="btn"
                            :class="post.userHasLiked ? 'btn-danger' : 'btn-success'"
                            @click="post.userHasLiked ? unlikePost(post.postId) : likePost(post.postId)">
                        {{ post.userHasLiked ? 'Sluta gilla' : 'Gilla' }}
                    </button>
                    <!-- Svara-knapp för post -->
                    <button v-if="isLoggedIn" class="btn btn-primary" @click="openCreateReplyModal(post.postId, null)">Svara</button>

                    <!-- Gul redigera-knapp -->
                    <button v-if="post.canEdit" class="btn btn-warning" @click="openEditPostModal(post)">Redigera</button>

                    <!-- Radera-knapp -->
                    <button v-if="post.canEdit" class="btn btn-danger" @click="showConfirmDeleteModal(post.postId)">Radera</button>

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
                                <div class="replytextbox">
                                    <p>{{ reply.content }} /Skrivet av:<em>{{ reply.author }}</em></p>
                                </div>
                                <p>{{ formatDate(reply.createdAt) }}</p>
                                <p>Gillningar: {{ reply.likes }}</p>
                                <!-- Visa bilden om den finns -->
                                <div v-if="reply.imageBase64">
                                    <img :src="'data:image/jpeg;base64,' + reply.imageBase64" alt="Reply Image" class="img-fluid post-image" />
                                </div>
                                <!-- Svara-knapp för reply -->
                                <button v-if="isLoggedIn" class="btn btn-secondary" @click="openCreateReplyModal(post.postId, reply.replyId)">
                                    Svara på detta svar
                                </button>
                                <button v-if="isLoggedIn" class="btn"
                                        :class="reply.userHasLiked ? 'btn-danger' : 'btn-success'"
                                        @click="reply.userHasLiked ? unlikeReply(reply.replyId) : likeReply(reply.replyId)">
                                    {{ reply.userHasLiked ? 'Sluta gilla' : 'Gilla' }}
                                </button>
                                <!-- Redigera-knapp för reply -->
                                <button v-if="reply.canEdit" class="btn btn-danger" @click="showConfirmDeleteReplyModal(reply.replyId)">Radera</button>
                                <button v-if="reply.canEdit" class="btn btn-warning" @click="openEditReplyModal(reply)">Redigera</button>
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
    import EditReplyModal from './EditReplyModal.vue';

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
        userHasLiked: boolean;
        canEdit: boolean;
    }

    interface Reply {
        replyId: number;
        content: string;
        author: string;
        createdAt: string;
        postId: number;
        likes: number;
        imageBase64?: string;
        userHasLiked: boolean;
        canEdit: boolean;
    }

    interface Category {
        categoryId: number;
        name: string;
    }

    // Reaktiva variabler
    const posts = ref<Post[]>([]);
    const allPosts = ref<Post[]>([]); // Kopia för alla inlägg
    const categories = ref<Category[]>([]);
    const currentPage = ref(1);
    const postsPerPage = 10;
    const isLoggedIn = ref(false);

    // Paginering
    const totalPages = computed(() => Math.ceil(posts.value.length / postsPerPage));

    const paginatedPosts = computed(() => {
        const start = (currentPage.value - 1) * postsPerPage;
        const end = start + postsPerPage;
        return posts.value.slice(start, end);
    });

    const formatDate = (dateString: string) => dateString.split('T')[0];

    // Modal- och likes-relaterade variabler och funktioner
    const isConfirmDeleteModalVisible = ref(false);
    const isConfirmDeleteReplyModalVisible = ref(false);
    const isCreateReplyModalVisible = ref(false);
    const selectedPostIdToDelete = ref<number | null>(null);
    const selectedReplyIdToDelete = ref<number | null>(null);
    const selectedPostId = ref<number | null>(null);
    const selectedParentReplyId = ref<number | null>(null);
    const isEditPostModalVisible = ref(false);
    const selectedPost = ref<Post | null>(null);
    const isEditReplyModalVisible = ref(false);
    const selectedReply = ref<Reply | null>(null);

    // Kontrollera om användaren är inloggad genom att kontrollera om JWT-token finns i localStorage
    const checkLoginStatus = () => {
        const token = localStorage.getItem('jwtToken');
        isLoggedIn.value = !!token;  // Sätt true om token finns, annars false
    };


    // Funktion för att öppna EditPostModal
    const openEditPostModal = (post: Post) => {
        selectedPost.value = post;
        isEditPostModalVisible.value = true;
    };

    // Funktion för att stänga EditPostModal
    const closeEditPostModal = () => {
        isEditPostModalVisible.value = false;
    };

    // Funktion för att öppna CreateReplyModal
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
    const openEditReplyModal = (reply: Reply) => {
        selectedReply.value = reply;
        isEditReplyModalVisible.value = true;
    };

    // Funktion för att stänga EditReplyModal
    const closeEditReplyModal = () => {
        isEditReplyModalVisible.value = false;
    };

    // Funktion för att öppna borttagningsmodalen för inlägg
    const showConfirmDeleteModal = (postId: number) => {
        selectedPostIdToDelete.value = postId;
        isConfirmDeleteModalVisible.value = true;
    };

    // Funktion för att stänga borttagningsmodalen för inlägg
    const closeConfirmDeleteModal = () => {
        isConfirmDeleteModalVisible.value = false;
        selectedPostIdToDelete.value = null;
    };

    // Funktion för att öppna borttagningsmodalen för svar
    const showConfirmDeleteReplyModal = (replyId: number) => {
        selectedReplyIdToDelete.value = replyId;
        isConfirmDeleteReplyModalVisible.value = true;
    };

    // Funktion för att stänga borttagningsmodalen för svar
    const closeConfirmDeleteReplyModal = () => {
        isConfirmDeleteReplyModalVisible.value = false;
        selectedReplyIdToDelete.value = null;
    };

    // Paginering
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

    // Funktion för att hämta alla inlägg
    const fetchPosts = async () => {
        try {
            const response = await axios.get<Post[]>('https://localhost:7147/api/Post');
            const postsWithReplies = await Promise.all(
                response.data.map(async (post) => {
                    const replies = await fetchRepliesForPost(post.postId);
                    const category = categories.value.find(cat => cat.categoryId === post.categoryId);
                    const userHasLiked = await HasUserLikedPost(post.postId);

                    // Kontrollera om användaren kan redigera/radera posten
                    const canEdit = await checkEditPermissions(post.postId);

                    return {
                        ...post,
                        replies,
                        categoryName: category ? category.name : 'Okänd kategori',
                        imageBase64: post.imageBase64,
                        userHasLiked,
                        canEdit,
                    };
                })
            );
            posts.value = postsWithReplies;
            allPosts.value = postsWithReplies;
        } catch (error) {
            console.error('Fel vid hämtning av inlägg:', error);
        }
    };



    // Hämta replies för ett inlägg
    const fetchRepliesForPost = async (postId: number) => {
        try {
            const response = await axios.get<Reply[]>(`https://localhost:7147/api/Replies/post/${postId}`);
            const repliesWithAdditionalInfo = await Promise.all(
                response.data.map(async (reply) => {
                    const userHasLiked = await HasUserLikedReply(reply.replyId);
                    const canEdit = await checkEditPermissionsForReply(reply.replyId);

                    console.log(`Reply ${reply.replyId} - canEdit: ${canEdit}`); // Lägg till debug-logg här också

                    return {
                        ...reply,
                        imageBase64: reply.imageBase64 || null,
                        userHasLiked,
                        canEdit,  // Lägg till canEdit flaggan
                    };
                })
            );
            return repliesWithAdditionalInfo;
        } catch (error) {
            console.error('Fel vid hämtning av replies:', error);
            return [];
        }
    };




    // Funktion för att hämta kategorier
    const fetchCategories = async () => {
        try {
            const response = await axios.get<Category[]>('https://localhost:7147/api/Category');
            categories.value = response.data;
        } catch (error) {
            console.error('Fel vid hämtning av kategorier:', error);
        }
    };

    // Sortera inlägg
    const sortPosts = (sortBy: string) => {
        posts.value = [...allPosts.value];
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
        currentPage.value = 1;
    };

    const likePost = async (postId: number) => {
        try {
            // Hämta JWT-token från localStorage (justera detta baserat på var du lagrar token)
            const token = localStorage.getItem('jwtToken');

            // Kontrollera att token existerar
            if (!token) {
                throw new Error('Ingen JWT-token tillgänglig, användaren är inte inloggad.');
            }

            // Anropa backend-API för att gilla posten med Bearer-token (JWT)
            const response = await axios.post(
                `https://localhost:7147/api/Post/${postId}/like`,
                {}, // Tom body för POST-förfrågan
                {
                    headers: {
                        Authorization: `Bearer ${token}` // Skicka JWT-token i Authorization-headern
                    }
                }
            );

            const message = response.data.message;

            // Uppdatera lokalt antal likes och userHasLiked om anropet lyckas
            const post = posts.value.find(p => p.postId === postId);
            if (post) {
                post.likes += 1;
                post.userHasLiked = true;  // Uppdatera flaggan direkt för att reflektera att användaren har gillat
            }

            console.log(message); // Feedback från API:t
        } catch (error: any) {
            if (error.response && error.response.data.message) {
                // Specifika felmeddelanden från API:t
                console.error(error.response.data.message);
            } else {
                // Annat fel (nätverksproblem eller oväntat problem)
                console.error('Ett fel uppstod vid gillningen:', error);
            }
        }
    };

    const likeReply = async (replyId: number) => {
        try {
            // Hämta JWT-token från localStorage (justera detta baserat på var du lagrar token)
            const token = localStorage.getItem('jwtToken');

            // Kontrollera att token existerar
            if (!token) {
                throw new Error('Ingen JWT-token tillgänglig, användaren är inte inloggad.');
            }

            // Anropa backend-API för att gilla reply med Bearer-token (JWT)
            const response = await axios.post(
                `https://localhost:7147/api/Replies/${replyId}/like`,
                {}, // Tom body för POST-förfrågan
                {
                    headers: {
                        Authorization: `Bearer ${token}` // Skicka JWT-token i Authorization-headern
                    }
                }
            );

            const message = response.data.message;

            // Uppdatera lokalt antal likes och userHasLiked om anropet lyckas
            for (const post of posts.value) {
                const reply = post.replies?.find(r => r.replyId === replyId);
                if (reply) {
                    reply.likes += 1;
                    reply.userHasLiked = true;  // Uppdatera flaggan direkt för att reflektera att användaren har gillat
                    break; // Avbryt loopen när vi har hittat och uppdaterat rätt reply
                }
            }

            console.log(message); // Feedback från API:t
        } catch (error: any) {
            if (error.response && error.response.data.message) {
                // Specifika felmeddelanden från API:t
                console.error(error.response.data.message);
            } else {
                // Annat fel (nätverksproblem eller oväntat problem)
                console.error('Ett fel uppstod vid gillningen av reply:', error);
            }
        }
    };


    const unlikePost = async (postId: number) => {
        try {
            const token = localStorage.getItem('jwtToken');
            if (!token) {
                throw new Error('Ingen JWT-token tillgänglig, användaren är inte inloggad.');
            }

            const response = await axios.post(
                `https://localhost:7147/api/Post/${postId}/unlike`,  // Endpoint för att sluta gilla
                {},
                {
                    headers: {
                        Authorization: `Bearer ${token}`, // Skicka JWT-token i Authorization-headern
                    },
                }
            );

            const post = posts.value.find(p => p.postId === postId);
            if (post) {
                post.likes -= 1;  // Minska likes lokalt
                post.userHasLiked = false;  // Markera att användaren inte längre har gillat posten
            }

            console.log(response.data.message);
        } catch (error: any) {
            console.error('Fel vid att sluta gilla posten:', error);
        }
    };

    const HasUserLikedPost = async (postId: number): Promise<boolean> => {
        try {
            const token = localStorage.getItem('jwtToken');
            if (!token) {
                throw new Error('Ingen JWT-token tillgänglig, användaren är inte inloggad.');
            }

            const response = await axios.get<boolean>(
                `https://localhost:7147/api/Post/${postId}/hasLiked`,
                {
                    headers: {
                        Authorization: `Bearer ${token}`,  // Skicka JWT-token i Authorization-headern
                    },
                }
            );
            return response.data.hasLiked;  // Returnera true eller false baserat på backend-svaret
        } catch (error) {
            console.error('Fel vid kontroll av om användaren har gillat posten:', error);
            return false;  // Vid fel, returnera false som standard
        }
    };

    const unlikeReply = async (replyId: number) => {
        try {
            // Hämta JWT-token från localStorage (justera detta baserat på var du lagrar token)
            const token = localStorage.getItem('jwtToken');

            if (!token) {
                throw new Error('Ingen JWT-token tillgänglig, användaren är inte inloggad.');
            }

            // Optimistisk uppdatering - hitta rätt post och reply
            for (const post of posts.value) {
                const reply = post.replies?.find(r => r.replyId === replyId);
                if (reply) {
                    // Minska likes lokalt och markera reply som "ogillad"
                    reply.likes -= 1;
                    reply.userHasLiked = false;

                    // Anropa backend för att sluta gilla reply
                    const response = await axios.post(
                        `https://localhost:7147/api/Replies/${replyId}/unlike`,
                        {},
                        {
                            headers: {
                                Authorization: `Bearer ${token}`, // Skicka JWT-token i Authorization-headern
                            },
                        }
                    );

                    console.log(response.data.message); // Skriv ut svaret från backend
                    break; // Avbryt loopen när rätt reply har uppdaterats
                }
            }
        } catch (error) {
            console.error('Fel vid att sluta gilla replyn:', error);
        }
    };

    const HasUserLikedReply = async (replyId: number): Promise<boolean> => {
        try {
            // Hämta JWT-token från localStorage (justera detta baserat på var du lagrar token)
            const token = localStorage.getItem('jwtToken');
            if (!token) {
                throw new Error('Ingen JWT-token tillgänglig, användaren är inte inloggad.');
            }

            // Anropa backend för att kontrollera om användaren har gillat reply
            const response = await axios.get<boolean>(
                `https://localhost:7147/api/Replies/${replyId}/hasLiked`,
                {
                    headers: {
                        Authorization: `Bearer ${token}`,  // Skicka JWT-token i Authorization-headern
                    },
                }
            );
            console.log("hej", response);
            return response.data;  // Returnera true eller false baserat på backend-svaret
        } catch (error) {
            console.error('Fel vid kontroll av om användaren har gillat replyn:', error);
            return false;  // Vid fel, returnera false som standard
        }
    };

    const checkEditPermissions = async (postId: number): Promise<boolean> => {
        try {
            const token = localStorage.getItem('jwtToken');
            if (!token) {
                throw new Error('Ingen JWT-token tillgänglig, användaren är inte inloggad.');
            }

            const response = await axios.get(`https://localhost:7147/api/Post/${postId}/can-edit`, {
                headers: {
                    Authorization: `Bearer ${token}`,
                },
            });

            return response.data.canEdit;
        } catch (error) {
            console.error('Fel vid kontroll av redigeringsbehörighet:', error);
            return false;
        }
    };

    const checkEditPermissionsForReply = async (replyId: number): Promise<boolean> => {
        try {
            const token = localStorage.getItem('jwtToken');
            if (!token) {
                throw new Error('Ingen JWT-token tillgänglig, användaren är inte inloggad.');
            }

            const response = await axios.get(`https://localhost:7147/api/Replies/${replyId}/can-edit`, {
                headers: {
                    Authorization: `Bearer ${token}`,
                },
            });

            console.log(`canEdit for reply ${replyId}:`, response.data.canEdit); // Debug
            return response.data.canEdit;
        } catch (error) {
            console.error('Fel vid kontroll av redigeringsbehörighet för reply:', error);
            return false;
        }
    };



    // Sortera efter kategori
    const sortPostsByCategory = (categoryId: number) => {
        posts.value = allPosts.value.filter(post => post.categoryId === categoryId);
        currentPage.value = 1;
    };

    // Återställ inlägg
    const resetPosts = () => {
        posts.value = [...allPosts.value];
        currentPage.value = 1;
    };

    // Lifecycle hook för att hämta data
    onMounted(async () => {
        await fetchCategories();
        await fetchPosts();
        checkLoginStatus();
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
        border: 1px solid black;
    }



        .postcontainer > div {
            display: flex;
            flex-direction: column;
        }

    .leftsectioncontainer {
        padding: 0.20rem;
    }

    .left-section {
        display: flex;
        flex-direction: column;
        gap: 0.5rem;
    }

    .middlesectioncontainer {
        padding: 0.20rem;
    }

    .middle-section {
        display: flex;
        flex-direction: column;
        gap: 0.5rem;
    }

    .rightsectioncontainer {
        padding: 0.20rem;
    }

    .right-section {
        display: flex;
        flex-direction: column;
        gap: 0.5rem;
    }

    .replytextbox {
        border: 1px solid black;
        border-radius: 10px;
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

    .postcontent {
        margin-top: 1rem;
        border: 1px solid black;
        border-radius: 5px;
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
    /*@media (max-width: 768px) {
        .btn {
            display: block;
            margin-bottom: 0.5rem;
        }

        .pagination-controls {
            display: block;
        }
    }*/
</style>
