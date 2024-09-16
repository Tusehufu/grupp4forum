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

<script lang="ts" setup>
import { ref, defineProps, defineEmits } from 'vue';
import axios from 'axios';

// Props
const props = defineProps<{
  isVisible: boolean;
  postId: number | null;
  parentReplyId: number | null;
}>();

const emit = defineEmits(['close']);

// Reaktiv variabel för svaret
const content = ref('');

// Funktion för att skicka formulär
const submitForm = async () => {
  try {
    const token = localStorage.getItem('jwtToken');
    const author = 'Användarnamn eller ID för inloggad användare';

    const replyData = {
      content: content.value,
      author,
      postId: props.postId,
      parentReplyId: props.parentReplyId,
    };

    const response = await axios.post('https://localhost:7056/api/replies', replyData, {
      headers: {
        Authorization: `Bearer ${token}`,
      },
    });

    console.log('Svar skapat:', response.data);
    content.value = '';
    emit('close');
  } catch (error) {
    console.error('Fel vid skapandet av svar:', error);
  }
};

const closeModal = () => {
  emit('close');
};
</script>

<style scoped>
    /* Din CSS här */
</style>
