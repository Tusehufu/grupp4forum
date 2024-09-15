<template>
    <div class="modal show" style="display: block;" v-if="isVisible">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title">Bekräfta borttagning</h5>
                    <button type="button" class="btn-close" @click="closeModal"></button>
                </div>
                <div class="modal-body">
                    <p>Är du säker på att du vill radera detta inlägg?</p>
                </div>
                <div class="modal-footer">
                    <button class="btn btn-danger" @click="confirmDelete">Radera</button>
                    <button class="btn btn-secondary" @click="closeModal">Avbryt</button>
                </div>
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
    import { defineProps, defineEmits } from 'vue';
    import axios from 'axios';

    // Definiera props
    const props = defineProps({
        isVisible: {
            type: Boolean,
            required: true,
        },
        postId: {
            type: Number,
            required: true,
        },
    });

    // Emitter för att skicka eventen 'confirm' och 'cancel'
    const emit = defineEmits(['confirm', 'cancel']);

    // Funktion för att bekräfta borttagning
    const confirmDelete = async () => {
        // Hämta JWT-token från lokal lagring eller autentiseringskälla
        const token = localStorage.getItem('jwtToken');
        try {
            await axios.delete(`https://localhost:7056/api/Post/${props.postId}`, {
                headers: {
                    Authorization: `Bearer ${token}`, // Skicka JWT-token som en del av Authorization-headers
                },
            });
            emit('confirm', props.postId);
            window.location.reload();
        } catch (error) {
            console.error('Det uppstod ett fel vid borttagning av inlägget:', error);
        }
    };

    // Funktion för att stänga modalen
    const closeModal = () => {
        emit('cancel');
    };
</script>

<style scoped>
</style>
