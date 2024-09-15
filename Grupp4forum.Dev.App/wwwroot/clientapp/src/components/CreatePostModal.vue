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
                        <button type="submit" class="btn btn-primary">Spara</button>
                    </form>
                </div>
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
    import { ref } from 'vue';

    // Använd defineProps för att definiera props
    const props = defineProps({
        isVisible: {
            type: Boolean,
            required: true,
        },
    });

    // Använd defineEmits för att skicka händelser
    const emit = defineEmits(['close']);

    // Reaktiva data för formuläret
    const title = ref('');
    const content = ref('');

    // Funktion för att stänga modalen
    const closeModal = () => {
        emit('close');
    };

    // Funktion för att hantera formulärinlämning
    const submitForm = () => {
        console.log('Nytt inlägg:', title.value, content.value);
        // Logik för att hantera inlämnat inlägg, t.ex. skicka det till API
        title.value = '';
        content.value = '';
        // Stäng modalen när inlägget har skapats
        closeModal();
    };
</script>

<style scoped>
    .modal {
        background-color: rgba(0, 0, 0, 0.5);
    }
</style>
