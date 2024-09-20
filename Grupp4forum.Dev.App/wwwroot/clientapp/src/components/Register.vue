﻿
<template>
    <div class="register-user-container">
        <h1 class="register-title text-center">Registrering</h1>
        <form class="register-form" @submit.prevent="submitForm">
            <div class="form-group">
                <label for="username" class="form-label">Användarnamn</label>
                <input type="text" id="username" v-model="newUser.username" class="form-control" required />
            </div>
            <div class="form-group">
                <label for="email" class="form-label">E-post</label>
                <input type="email" id="email" v-model="newUser.email" class="form-control" required />
            </div>
            <div class="form-group">
                <label for="password" class="form-label">Lösenord</label>
                <input type="password" id="password" v-model="newUser.password" class="form-control" required />
            </div>
            <button type="submit" class="btn btn-submit">Registrera</button>
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
    .register-user-container {
        max-width: 400px;
        margin: 0 auto;
        padding: 20px;
        background-color: #f9f9f9;
        border-radius: 8px;
        box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
    }

    .register-title {
        font-family: 'Arial', sans-serif;
        color: #333;
        margin-bottom: 20px;
        font-weight: bold;
    }

    .register-form .form-group {
        margin-bottom: 15px;
    }

    .form-label {
        font-size: 14px;
        color: #555;
        margin-bottom: 5px;
        display: block;
    }

    .form-input {
        width: 100%;
        padding: 10px;
        border: 1px solid #ccc;
        border-radius: 4px;
        font-size: 16px;
        color: #333;
        transition: border-color 0.2s ease;
    }

        .form-input:focus {
            border-color: #007bff;
            outline: none;
        }

    .btn-submit {
        width: 100%;
        padding: 12px;
        background-color: #007bff;
        color: #fff;
        border: none;
        border-radius: 4px;
        font-size: 16px;
        cursor: pointer;
        transition: background-color 0.2s ease;
    }

        .btn-submit:hover {
            background-color: #0056b3;
        }

    .alert {
        margin-top: 20px;
        padding: 15px;
        border-radius: 5px;
        font-size: 14px;
    }

    .alert-success {
        background-color: #d4edda;
        color: #155724;
    }

    .alert-danger {
        background-color: #f8d7da;
        color: #721c24;
    }
</style>
