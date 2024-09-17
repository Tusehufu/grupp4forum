<template>
    <div class="modal show" style="display: block;" v-if="isVisible">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title">Svara</h5>
                    <button type="button" class="btn-close" @click="closeModal"></button>
                </div>
                <div class="modal-body">
                    <form @submit.prevent="submitForm">
                        <div class="mb-3">
                            <label for="content" class="form-label">Svar</label>
                            <textarea id="content" v-model="content" class="form-control" required></textarea>
                        </div>
                        <button type="submit" class="btn btn-primary">Skicka</button>
                    </form>
                </div>
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
    import { ref } from 'vue';
    import axios from 'axios';

    // Props för modalen
    const props = defineProps({
        isVisible: {
            type: Boolean,
            required: true,
        },
        postId: {
            type: Number,
            required: true,
        },
        parentReplyId: {
            type: [Number, null],  // Tillåter både Number och null
            default: null,
        }
    });


    // Emit för att stänga modalen och avisera att ett nytt svar har skapats
    const emit = defineEmits(['close', 'replyCreated']);

    // Reaktiv variabel för svaret
    const content = ref('');

    const submitForm = async () => {
        try {
            const userId = 1;  // Hårdkodat användar-ID

            // Skapa reply-data som skickas till backend
            const replyData = {
                content: content.value,
                userId
            };

            // Skicka POST-förfrågan till API med postId och parentReplyId som query-parametrar
            let url = `https://localhost:7147/api/Replies?postId=${props.postId}`;

            // Lägg till parentReplyId till URL:en endast om det inte är null
            if (props.parentReplyId !== null) {
                url += `&parentReplyId=${props.parentReplyId}`;
            }

            const response = await axios.post(url, replyData);

            console.log('Svar skapat:', response.data);

            emit('replyCreated');  // Emitera event för att meddela att ett svar har skapats

            // Återställ formuläret
            content.value = '';

            // Stäng modalen
            closeModal();
        } catch (error) {
            console.error('Fel vid skapandet av svar:', error);
        }
    };

    // Funktion för att stänga modalen
    const closeModal = () => {
        emit('close');
    };
</script>

<style scoped>
    .modal {
        background-color: rgba(0, 0, 0, 0.5);
    }
</style>
