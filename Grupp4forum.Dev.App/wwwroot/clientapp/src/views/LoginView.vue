<template>
    <div id="register">
        <h1 class="text-center">Registrera</h1>
        <button type="button" class="btn btn-primary" data-bs-toggle="modal" data-bs-target="#registerModalDialog">
            Register
        </button>
      
        <button type="button" class="btn btn-secondary" data-bs-toggle="modal" data-bs-target="#loginModalDialog">
            Login
        </button>
        <RegisterModal />
        <LoginModal />
    </div>
</template>

<script lang="ts">
    import { defineComponent } from 'vue';
    import RegisterModal from '../components/RegisterModal.vue';
    import LoginModal from '../components/LoginModal.vue';

    export default defineComponent({
        name: 'App',
        components: {
            RegisterModal,
            LoginModal
        }
    });
</script>

<style>
    /* Globala stilar kan läggas här om det behövs */
</style>