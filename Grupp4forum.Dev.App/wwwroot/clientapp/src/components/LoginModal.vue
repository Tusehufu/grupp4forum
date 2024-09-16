<template>
    <div class="modal fade" id="loginModalDialog" tabindex="-1" aria-labelledby="loginModalLabel" aria-hidden="true">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title" id="loginModalLabel">Login</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>
                <div class="modal-body">
                    <form @submit.prevemt="handleLogin">
                        <div class="mb-3">
                            <label for="loginEmail" class="form-label">Email address</label>
                            <input type="email" class="form-control" id="loginEmail" required>
                        </div>
                        <div class="mb-3">
                            <label for="loginPassword" class="form-label">Password</label>
                            <input type="password" class="form-control" id="loginPassword" required>
                        </div>
                        <div v-if="errorMessage" class="alert alert-danger" role="alert">
                            {{ errorMessage }}
                        </div>
                        <button @click="handleLogin" class="btn btn-primary">Login</button>
                        
                    </form>
                </div>
            </div>
        </div>
    </div>
</template>

<script lang="ts">
    import { defineComponent, ref } from 'vue';
    import { login } from '../services/authService';
    import { Modal } from 'bootstrap';


    export default defineComponent({
        name: 'LoginModal',

        setup() {

            const email = ref('');
            const password = ref('');
            const errorMessage = ref('');

            const handleLogin = async () => {
                try {

                    errorMessage.value = '';//clear prev errors
                    const response = await login(email.value, password.value);

                    console.log("login succesful:", response);

                    const modalElement = document.getElementById('loginModalDialog');

                    if (modalElement) {
                        const modal = Modal.getInstance(modalElement) || new Modal(modalElement);
                        if (modal) {
                            modal.hide();

                        }
                    }
                } catch (error) {
                    if (error instanceof Error) {
                        errorMessage.value = error.message;
                    } else {
                        errorMessage.value = 'An unknown error occurred';
                    }
                }
            };

            return { email, password, errorMessage, handleLogin };
        }
        });


</script>

<style scoped>
    
</style>