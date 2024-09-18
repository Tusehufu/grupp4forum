<template>
    <div class="register-user">
        <h1 class="text-center">Registrera ny användare</h1>
        <form @submit.prevent="submitForm">
            <div class="mb-3">
                <label for="username" class="form-label">Användarnamn</label>
                <input type="text" id="username" v-model="newUser.username" class="form-control" required />
            </div>
            <div class="mb-3">
                <label for="email" class="form-label">E-post</label>
                <input type="email" id="email" v-model="newUser.email" class="form-control" required />
            </div>
            <div class="mb-3">
                <label for="password" class="form-label">Lösenord</label>
                <input type="password" id="password" v-model="newUser.password" class="form-control" required />
            </div>
            <button type="submit" class="btn btn-primary">Registrera</button>
        </form>

        <!-- Visa meddelandet när användaren har registrerats -->
        <div v-if="successMessage" class="alert alert-success mt-3">
            {{ successMessage }}
        </div>
        <!-- Visa felmeddelandet om något gick fel -->
        <div v-if="errorMessage" class="alert alert-danger mt-3">
            {{ errorMessage }}
        </div>
    </div>
</template>

<script lang="ts" setup>
    import { ref } from 'vue';
    import axios from 'axios';

    // Definiera ett objekt för att lagra information om den nya användaren
    const newUser = ref({
        username: '',
        email: '',
        password: '',
    });

    // Skapa en ref för att lagra framgångsmeddelandet
    const successMessage = ref('');
    // Skapa en ref för att lagra felmeddelandet
    const errorMessage = ref('');

    // Funktion för att skicka formuläret
    const submitForm = async () => {
        try {
            // Skicka POST-förfrågan till API:et för att registrera en ny användare
            await axios.post('https://localhost:7147/api/Users', newUser.value);

            // Visa meddelandet om framgång
            successMessage.value = 'Användaren har registrerats framgångsrikt!';

            // Rensa formuläret
            newUser.value = {
                username: '',
                email: '',
                password: '',
            };

            // Töm eventuella felmeddelanden
            errorMessage.value = '';
        } catch (error) {
            console.error('Fel vid registrering av användare:', error);
            // Hantera felmeddelanden
            errorMessage.value = 'Ett fel uppstod vid registrering av användare.';
            successMessage.value = '';
        }
    };
</script>

<style scoped>
    /* Lägg till din CSS här om du vill */
</style>
