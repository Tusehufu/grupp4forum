<template>
    <div class="profile">
        <h1>Profil</h1>
        <div v-if="user">
            <p><strong>Användarnamn:</strong> {{ user.username }}</p>
            <p><strong>Email:</strong> {{ user.email }}</p>
            <p><strong>Skapades:</strong> {{ formatDate(user.createdAt) }}</p>
            <p><strong>Uppdaterades senast:</strong> {{ formatDate(user.updatedAt) }}</p>
        </div>
        <div v-else>
            <p>Loading profile data...</p>
        </div>
    </div>
</template>

<script setup lang="ts">
    import { ref, onMounted } from 'vue';
    import axios from 'axios';

    // Define state variables
    const user = ref(null);
    const errorMessage = ref('');
    const token = localStorage.getItem('jwtToken');
    const userId = localStorage.getItem('userId');
    const formatDate = (dateString: string) => dateString.split('T')[0];
    // Fetch user profile when the component is mounted
    const fetchUserProfile = async () => {
        try {
            const response = await axios.get(`https://localhost:7147/api/Users/${userId}`, {
                headers: {
                    Authorization: `Bearer ${token}` // Lägg till JWT-token i Authorization-headern
                }
            });

            user.value = response.data;
        } catch (error) {
            errorMessage.value = 'Error fetching profile data';
            console.error(error);
        }
    };

    // Call the fetchUserProfile on component mount
    onMounted(() => {
        fetchUserProfile();
    });
</script>

<style scoped>
    .profile {
        margin: 20px;
    }
</style>
