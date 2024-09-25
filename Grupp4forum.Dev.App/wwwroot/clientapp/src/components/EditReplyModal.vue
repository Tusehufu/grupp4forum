<template>
    <!-- Modalen visas endast när `isVisible` är sant -->
    <div v-if="isVisible" class="modal show" style="display: block;">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title">Redigera Svar</h5>
                    <!-- Stäng modal-knappen -->
                    <button type="button" class="btn-close" @click="closeModal"></button>
                </div>
                <div class="modal-body">
                    <form @submit.prevent="submitForm">
                        <div class="mb-3">
                            <label for="content" class="form-label">Innehåll</label>
                            <textarea id="content" v-model="content" class="form-control" required></textarea>
                        </div>
                        <button type="submit" class="btn btn-primary">Spara</button>
                    </form>
                </div>
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue';
import axios from 'axios';

// Props för modalen
const props = defineProps({
    isVisible: {
        type: Boolean,
        required: true,
    },
    reply: {
        type: Object,
        required: true, // Förväntar sig ett reply-objekt med befintliga värden
    },
});

// Emit för att stänga modalen och uppdatera svaret
const emit = defineEmits(['close', 'replyUpdated']);

// Reaktiva data för innehåll
const content = ref(props.reply.content); // Fyll i med befintligt svarsinnehåll

// Funktion för att stänga modalen
const closeModal = () => {
    emit('close');
};

// Funktion för att hantera formulärinlämning och uppdatera svaret
const submitForm = async () => {
    try {
        const formData = new FormData();
        formData.append('Content', content.value);
        const token = localStorage.getItem('jwtToken');
        if (!token) {
            console.error('Ingen JWT-token hittades i localStorage.');
            return;
        }
        await axios.put(`https://grupp4forumdevapp20240923094105.azurewebsites.net/api/Replies/${props.reply.replyId}`, formData, {
            headers: {
                'Content-Type': 'multipart/form-data',
                'Authorization': `Bearer ${token}`
            },
        });
      
        // Emitera en händelse för att uppdatera listan av svar
        emit('replyUpdated');

        // Stäng modalen efter uppdateringen
        closeModal();
    } catch (error) {
        console.error('Ett fel uppstod vid uppdateringen av svaret:', error);
    }
};

// Uppdatera reaktiva data om svaret ändras (vid öppning av nytt svar)
watch(() => props.reply, (newReply) => {
    content.value = newReply.content;
});
</script>

<style scoped>
    .modal {
        background-color: rgba(0, 0, 0, 0.5);
    }
</style>
