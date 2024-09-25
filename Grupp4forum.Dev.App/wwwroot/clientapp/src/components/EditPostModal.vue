<template>
    <!-- Modalen visas endast när `isVisible` är sant -->
    <div v-if="isVisible" class="modal show" style="display: block;">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title">Redigera inlägg</h5>
                    <!-- När man klickar på knappen, emit 'close' eventet för att stänga modalen -->
                    <button type="button" class="btn-close" @click="closeModal"></button>
                </div>
                <div class="modal-body">
                    <form @submit.prevent="submitForm">
                        <div class="mb-3">
                            <label for="title" class="form-label">Titel</label>
                            <input type="text" id="title" v-model="title" class="form-control" required />
                        </div>
                        <div class="mb-3">
                            <label for="content" class="form-label">Innehåll</label>
                            <textarea id="content" v-model="content" class="form-control" required></textarea>
                        </div>
                        <div class="mb-3">
                            <label for="category" class="form-label">Kategori</label>
                            <!-- Dropdown-lista för att välja kategori -->
                            <select id="category" v-model="selectedCategory" class="form-select" required>
                                <option v-for="category in categories" :key="category.categoryId" :value="category.categoryId">
                                    {{ category.name }}
                                </option>
                            </select>
                        </div>
                        <button type="submit" class="btn btn-primary">Spara</button>
                    </form>
                </div>
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import { ref, onMounted, watch } from 'vue';
import axios from 'axios';

// Props för modalen
const props = defineProps({
    isVisible: {
        type: Boolean,
        required: true,
    },
    post: {
        type: Object,
        required: true, // Förväntar sig ett post-objekt med befintliga värden
    },
});

// Emit för att stänga modalen och uppdatera inlägg
const emit = defineEmits(['close', 'postUpdated']);

// Reaktiva data för titel, innehåll och kategori
const title = ref(props.post.title); // Fyll i med postens titel
const content = ref(props.post.content); // Fyll i med postens innehåll
const selectedCategory = ref(props.post.categoryId); // Fyll i med postens kategori
const categories = ref([]); // Alla kategorier

// Funktion för att stänga modalen
const closeModal = () => {
    emit('close');
};

// Funktion för att hämta alla kategorier från API
const fetchCategories = async () => {
    try {
        const response = await axios.get('https://grupp4forumdevapp20240923094105.azurewebsites.net/api/Category'); // Hämta från API
        categories.value = response.data; // Spara kategorierna
    } catch (error) {
        console.error('Misslyckades med att hämta kategorier:', error);
    }
};

    // Funktion för att hantera formulärinlämning och uppdatera inlägget
    const submitForm = async () => {

        try {

            const formData = new FormData();
            formData.append('Title', title.value);
            formData.append('Content', content.value);
            formData.append('categoryId', String(selectedCategory.value));  // Konvertera till sträng om det är ett nummer

            // Logga serverns svar för att se vad API:et returnerar
            //console.log('Serverns svar:', response);
            const token = localStorage.getItem('jwtToken');
            if (!token) {
                console.error('Ingen JWT-token hittades i localStorage.');
                return;
            }
            await axios.put(`https://grupp4forumdevapp20240923094105.azurewebsites.net/api/Post/${props.post.postId}`, formData, {
                headers: {
                    'Content-Type': 'multipart/form-data',
                    'Authorization': `Bearer ${token}`
                },
            });
            // Emitera en händelse för att uppdatera listan av inlägg
            emit('postUpdated');

            // Stäng modalen efter uppdateringen
            closeModal();
        } catch (error) {
            // Logga detaljerat felmeddelande om något går fel
            console.error('Ett fel uppstod vid uppdateringen av inlägget:', error);

            // Logga ut eventuella detaljer från serverns felmeddelande
            if (error.response) {
                console.error('Server fel:', error.response.data);
            }
        }
    };




// Uppdatera reaktiva data om posten ändras (vid öppning av ny post)
watch(() => props.post, (newPost) => {
    Title.value = newPost.Title;
    Content.value = newPost.Content;
    selectedCategory.value = newPost.categoryId;
});

// Hämta kategorier när komponenten monteras
onMounted(() => {
    fetchCategories();
});
</script>

<style scoped>
    .modal {
        background-color: rgba(0, 0, 0, 0.5);
    }
</style>
