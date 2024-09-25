<template>
    <div class="container mt-3">
        <div class="mb-3">
            <label for="username" class="form-label">Användarnamn</label>
            <input type="text" id="username" v-model="username" class="form-control" placeholder="Skriv användarnamn">
        </div>
        <button class="btn btn-danger" @click="deleteUser">Radera användare</button>
        <p v-if="errorMessage" class="text-danger">{{ errorMessage }}</p>
        <p v-if="successMessage" class="text-success">{{ successMessage }}</p>
    </div>
</template>

<script setup lang="ts">
    import { ref } from 'vue';
    import axios from 'axios';

    const username = ref('');
    const errorMessage = ref('');
    const successMessage = ref('');

    // Här kan du sätta din JWT-token (hämtad från autentisering eller lagrad i localStorage)
    const token = localStorage.getItem('jwtToken') || ''; // Hämta JWT-token från localStorage

    const deleteUser = async () => {
        if (!username.value) {
            errorMessage.value = 'Användarnamn kan inte vara tomt';
            return;
        }

        try {
            const response = await axios.delete(`https://grupp4forumdevapp20240923094105.azurewebsites.net/api/Users/by-username/${username.value}`, {
                headers: {
                    'Authorization': `Bearer ${token}` // Skicka JWT-token i Authorization-headern
                }
            });

            if (response.status === 204) { // No Content
                successMessage.value = `Användare ${username.value} raderad.`;
                username.value = ''; // Återställ inputfältet
                errorMessage.value = '';
            } else if (response.status === 404) {
                errorMessage.value = 'Användaren kunde inte hittas.';
                successMessage.value = '';
            } else if (response.status === 401) {
                errorMessage.value = 'Obehörig. Du måste vara inloggad för att radera användaren.';
                successMessage.value = '';
            } else {
                errorMessage.value = 'Ett fel inträffade. Försök igen.';
                successMessage.value = '';
            }
        } catch (error) {
            errorMessage.value = 'Kunde inte ansluta till servern.';
            successMessage.value = '';
        }
    };
</script>

<style scoped>
    .container {
        max-width: 400px;
    }
</style>
