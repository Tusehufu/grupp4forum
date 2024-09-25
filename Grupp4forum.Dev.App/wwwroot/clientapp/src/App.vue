<template>
    <div id="app">
        <img src="@/assets/flowertopleft.png" alt="Left Image" class="corner-image left-image" />
        <img src="@/assets/flowertopright.png" alt="Right Image" class="corner-image right-image" />
        <img src="@/assets/bottomright.png" alt="right img leaves" class="bottomcorner-image bottomright-image" />
        <img src="@/assets/bottomleft.png" alt="left img leaves" class="bottomcorner-image bottomleft-image" />
        <nav>
            <router-link to="/" class="btn custom-primary">Hem</router-link>
            <router-link v-if="!isLoggedIn" to="/register" class="btn custom-primary">Registrera dig</router-link>
            <router-link to="/login" class="btn custom-primary">Logga in</router-link>
            <router-link to="/forum" class="btn custom-primary">Forum</router-link>
            <router-link to="/profile" class="btn custom-primary">Mina sidor</router-link>

            <!-- Visa endast länken om användaren är admin eller moderator -->
            <router-link v-if="isAdminOrModerator" to="/Admin" class="btn custom-primary">Användarhantering</router-link>
        </nav>
        <router-view />
    </div>
</template>

<script setup lang="ts">
    import { ref, onMounted } from 'vue';
    import axios from 'axios';

    // Reaktiva variabler för att hantera inloggnings- och adminstatus
    const isAdminOrModerator = ref(false);
    const isLoggedIn = ref(false); 

    // Kontrollera om användaren är inloggad genom att kontrollera om JWT-token finns i localStorage
    const checkLoginStatus = () => {
        const token = localStorage.getItem('jwtToken');
        isLoggedIn.value = !!token; // Sätt true om token finns, annars false
    };

    onMounted(async () => {
        // Kontrollera inloggningsstatus
        checkLoginStatus();

        // Om användaren inte är inloggad behöver vi inte kolla adminstatus
        if (!isLoggedIn.value) {
            return;
        }

        try {
            const token = localStorage.getItem('jwtToken');
            if (!token) {
                console.error('Ingen JWT-token hittades i localStorage.');
                return;
            }

            // Kolla om användaren är admin eller moderator
            const response = await axios.get('https://grupp4forumdevapp20240923094105.azurewebsites.net/api/Users/is-admin-or-moderator', {
                headers: {
                    'Authorization': `Bearer ${token}`
                }
            });

            // Uppdatera admin/moderatorstatus om API-svaret visar det
            if (response.data.isAdminOrModerator) {
                isAdminOrModerator.value = true;
            }
        } catch (error) {
            console.error('Error fetching admin/moderator status:', error);
        }
    });
</script>


<style>
    #app {
        font-family: Avenir, Helvetica, Arial, sans-serif;
        -webkit-font-smoothing: antialiased;
        -moz-osx-font-smoothing: grayscale;
        text-align: center;
        color: #2c3e50;
    }

    nav {
        background-color: #F0F9E7;
        padding: 30px;
    }




    /* Custom button styles */
    .btn.custom-primary {
        background-color: #6ECB63;
        color: black;
        padding: 10px;
        border-radius: 5px;
        text-decoration: none;
        display: inline-block;
        margin: 10px;
        border-color: black;
    }

        .btn.custom-primary:hover {
            background-color: #5CA44A;
        }

    /*image*/

    .corner-image {
        position: fixed;
        height: auto;
    }

    .left-image {
        top: 0;
        left: 0;
    }

    .right-image {
        top: 0;
        right: 0;
    }

    .bottomcorner-image {
        position: fixed;
        height: auto;
    }

    .bottomleft-image {
        bottom: 0;
        left: 0;
    }

    .bottomright-image {
        bottom: 0;
        right: 0;
    }
</style>
