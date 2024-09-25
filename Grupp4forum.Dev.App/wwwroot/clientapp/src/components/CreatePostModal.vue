<template>
    <!-- Modalen visas endast när `isVisible` är sant -->
    <div v-if="isVisible" class="modal show" style="display: block;">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title">Skapa nytt inlägg</h5>
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
                        <!-- Nytt bilduppladdningsfält -->
                        <div class="mb-3">
                            <label for="image" class="form-label">Lägg till bild</label>
                            <input type="file" id="image" @change="handleImageUpload" class="form-control" />
                        </div>
                        <button type="submit" class="btn btn-primary">Spara</button>
                    </form>
                </div>
            </div>
        </div>
    </div>
</template>


<script setup lang="ts">
    import { ref, onMounted } from 'vue';
    import axios from 'axios';

    // Props för modalen
    const props = defineProps({
        isVisible: {
            type: Boolean,
            required: true,
        },
    });

    // Emit för att stänga modalen och uppdatera inlägg
    const emit = defineEmits(['close', 'postCreated']);

    // Reaktiva data
    const title = ref('');
    const content = ref('');
    const selectedCategory = ref('');  // Kategorin som användaren väljer
    const categories = ref([]);  // Alla kategorier
    const image = ref<File | null>(null); // För att hålla den valda bilden

// Funktion för att hantera när en fil väljs i bilduppladdningen
const handleImageUpload = (event: Event) => {
    const file = (event.target as HTMLInputElement).files?.[0];
    if (file) {
        image.value = file; // Spara den valda filen i image ref
    }
};

    // Funktion för att stänga modalen
    const closeModal = () => {
        emit('close');
    };

    // Funktion för att hämta alla kategorier från API
   const fetchCategories = async () => {
    try {
        const response = await axios.get('https://grupp4forumdevapp20240923094105.azurewebsites.net/api/Category');  // Din API-endpoint
        categories.value = response.data;
    } catch (error) {
        console.error('Fel vid hämtning av kategorier:', error);
    }
};


    // Hämta kategorier när komponenten monteras
    onMounted(() => {
        fetchCategories();
    });

    // Funktion för att hantera formulärinlämning och skapa ett nytt inlägg
const submitForm = async () => {
    try {
        
        const formData = new FormData();
        formData.append('title', title.value);
        formData.append('content', content.value);
        formData.append('categoryId', String(selectedCategory.value));  // Konvertera till sträng om det är ett nummer

        if (image.value) {
            formData.append('image', image.value);
        }
        const token = localStorage.getItem('jwtToken');
        // Kontrollera om token finns
        if (!token) {
            console.error('Ingen JWT-token hittades i localStorage.');
            return;
        }
        await axios.post('https://grupp4forumdevapp20240923094105.azurewebsites.net/api/Post', formData, {
            headers: {
                'Content-Type': 'multipart/form-data',
                'Authorization': `Bearer ${token}`
            },
        });

        emit('postCreated');

        title.value = '';
        content.value = '';
        selectedCategory.value = '';
        image.value = null;
        closeModal();
    } catch (error) {
        console.error('Ett fel uppstod vid skapandet av inlägget:', error);
    }
};



</script>

<style scoped>
    .modal {
        background-color: rgba(0, 0, 0, 0.5);
    }
</style>
