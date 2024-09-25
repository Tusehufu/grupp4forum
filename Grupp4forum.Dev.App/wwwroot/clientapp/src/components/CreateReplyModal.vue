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
                        <div class="mb-3">
                            <label for="image" class="form-label">Ladda upp en bild (valfritt)</label>
                            <input type="file" id="image" @change="onFileChange" class="form-control" accept="image/*">
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

    // Reaktiva variabler för svaret och bilden
    const content = ref('');
    const replyImage = ref<File | null>(null);  // Hanterar den uppladdade bilden

    // Hantera bildfilen som laddas upp
    const onFileChange = (event: Event) => {
        const target = event.target as HTMLInputElement;
        if (target.files && target.files.length > 0) {
            replyImage.value = target.files[0];  // Lagra den valda filen
        }
    };

    // Skicka formulärdata till backend
    const submitForm = async () => {
        try {
            // Hämta JWT-token från localStorage (justera detta baserat på var du lagrar token)
            const token = localStorage.getItem('jwtToken');

            // Kontrollera att token existerar
            if (!token) {
                throw new Error('Ingen JWT-token tillgänglig, användaren är inte inloggad.');
            }

            // Skapa FormData för att inkludera både text och bild
            const formData = new FormData();
            formData.append('Content', content.value);  // Lägg till svaret

            // Om en bild är vald, lägg till den i FormData
            if (replyImage.value) {
                formData.append('Image', replyImage.value);  // Lägg till bilden
            }

            // Skicka POST-förfrågan till API med postId och parentReplyId som query-parametrar
            let url = `https://grupp4forumdevapp20240923094105.azurewebsites.net/api/Replies?postId=${props.postId}`;

            // Lägg till parentReplyId till URL:en endast om det inte är null
            if (props.parentReplyId !== null) {
                url += `&parentReplyId=${props.parentReplyId}`;
            }

            const response = await axios.post(url, formData, {
                headers: {
                    'Content-Type': 'multipart/form-data',  // Viktigt att specificera detta när vi skickar FormData
                    Authorization: `Bearer ${token}`,  // Skicka JWT-token i Authorization-headern
                },
            });


            emit('replyCreated');  // Emitera event för att meddela att ett svar har skapats

            // Återställ formuläret
            content.value = '';
            replyImage.value = null;

            // Stäng modalen
            closeModal();
        } catch (error: any) {
            console.error('Fel vid skapandet av svar:', error);
            if (error.response && error.response.status === 401) {
                console.error('Obehörig: Ingen giltig JWT-token eller inloggning krävs.');
            }
        }
    };


    // Funktion för att stänga modalen
    const closeModal = () => {
        emit('close');
    };
</script>
