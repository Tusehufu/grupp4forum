<template>
    <div class="col-md-12">
        <div class="card card-container">
            <!-- Visa inloggningsformulär om användaren inte är inloggad -->
            <Form v-if="!isLoggedIn" @submit="handleLogin" :validation-schema="schema">
                <div class="form-group">
                    <label for="username">Användarnamn</label>
                    <Field name="username" v-model="userCredentials.username" type="text" class="form-control" />
                    <ErrorMessage name="username" class="error-feedback" />
                </div>
                <div class="form-group">
                    <label for="password">Lösenord</label>
                    <Field name="password" v-model="userCredentials.password" type="password" class="form-control" />
                    <ErrorMessage name="password" class="error-feedback" />
                </div>

                <div class="form-group">
                    <button class="btn btn-primary btn-block" :disabled="loading">
                        <span v-show="loading" class="spinner-border spinner-border-sm"></span>
                        <span>Logga in</span>
                    </button>
                </div>

                <div class="form-group">
                    <div v-if="errorMessage" class="alert alert-danger" role="alert">
                        {{ errorMessage }}
                    </div>
                </div>
            </Form>

            <!-- Visa inloggad text och knappar om användaren är inloggad -->
            <div v-else>
                <p>Du är inloggad</p>
                <button @click="handleLogout" class="btn btn-danger">Logga ut</button>
                <!-- Visa bekräftelsemeddelande om användaren har loggats ut -->
                <p v-if="loggedOut" class="text-success">Du har loggats ut</p>
            </div>
        </div>
    </div>
</template>

<script lang="ts" setup>
    import { ref, computed, onMounted } from 'vue';
    import axios from 'axios';
    import { Form, Field, ErrorMessage } from 'vee-validate';
    import * as yup from 'yup';

    const userCredentials = ref({
        username: '',
        password: '',
    });

    const loading = ref(false);
    const errorMessage = ref('');
    const loggedOut = ref(false);
    const isLoggedIn = ref(false);  // Reaktiv variabel för inloggningsstatus

    const schema = yup.object().shape({
        username: yup.string().required('Användarnamn krävs'),
        password: yup.string().required('Lösenord krävs'),
    });

    const handleLogin = async () => {
        try {
            loading.value = true;
            errorMessage.value = '';

            const loginResponse = await axios.post('https://grupp4forumdevapp20240923094105.azurewebsites.net/api/auth/login', userCredentials.value);

            if (loginResponse.status === 200) {
                const token = loginResponse.data.token;
                localStorage.setItem('jwtToken', token);

                const username = userCredentials.value.username;
                const userIdResponse = await axios.get(`https://grupp4forumdevapp20240923094105.azurewebsites.net/api/users/id?username=${username}`);

                if (userIdResponse.status === 200) {
                    const userId = userIdResponse.data;
                    localStorage.setItem('userId', userId);
                }

                window.location.reload();
            } else {
                throw new Error('Inloggning misslyckades');
            }
        } catch (error) {
            errorMessage.value = 'Fel vid inloggning. Kontrollera dina inloggningsuppgifter och försök igen.';
        } finally {
            loading.value = false;
        }
    };

    const handleLogout = () => {
        // Ta bort både JWT-token och userId från localStorage för att logga ut användaren
        localStorage.removeItem('jwtToken');
        localStorage.removeItem('userId');
        loggedOut.value = true;
        userCredentials.value = {
            username: '',
            password: '',
        };
        window.location.reload();
    };

    // Funktion för att extrahera expiration-time från JWT-token
    const getTokenExpiration = (token: string) => {
        const payloadBase64 = token.split('.')[1];
        const decodedPayload = JSON.parse(atob(payloadBase64));
        return decodedPayload.exp * 1000; // Konvertera till millisekunder
    };

    // Funktion för att kontrollera och logga ut användaren när token har gått ut
    const handleTokenExpiration = () => {
        const token = localStorage.getItem('jwtToken');
        if (token) {
            const expirationTime = getTokenExpiration(token);
            const currentTime = Date.now();

            if (currentTime >= expirationTime) {
                handleLogout(); // Token har gått ut, logga ut användaren
            } else {
                const timeUntilExpiration = expirationTime - currentTime;
                setTimeout(handleLogout, timeUntilExpiration); // Logga ut användaren när tiden har gått ut
            }
        }
    };

    // Funktion för att logga ut användaren efter 30 minuter
    const setAutoLogoutTimer = () => {
        const thirtyMinutes = 1800000; // 30 minuter i millisekunder
        setTimeout(handleLogout, thirtyMinutes); // Logga ut användaren efter 30 minuter
    };

    // Funktion för att kontrollera inloggningsstatus baserat på om både JWT-token och userId finns i localStorage
    const checkLoginStatus = () => {
        const token = localStorage.getItem('jwtToken');
        const userId = localStorage.getItem('userId');
        isLoggedIn.value = !!token && !!userId; // Sätter isLoggedIn till true om båda finns, annars false
    };

    // Kontrollera inloggningsstatus när sidan laddas
    onMounted(() => {
        checkLoginStatus(); // Kontrollera om användaren är inloggad
        handleTokenExpiration(); // Kontrollera om JWT-token har gått ut och sätt timer
        setAutoLogoutTimer(); // Sätt en timer för automatisk utloggning efter 30 minuter
    });
</script>



<style scoped>
    .error-feedback {
        color: red;
    }

    .error-feedback {
        color: red;
    }

    .card-container {
        max-width: 400px;
        margin: 50px auto;
        padding: 20px;
        border-radius: 8px;
        box-shadow: 0px 4px 8px rgba(0, 0, 0, 0.1);
        background-color: #fff;
    }

    .form-group {
        margin-bottom: 15px;
    }

    label {
        font-weight: bold;
        margin-bottom: 5px;
        display: block;
    }

    .form-control {
        border: 1px solid #ced4da;
        border-radius: 4px;
        padding: 10px;
        font-size: 14px;
        transition: border-color 0.3s;
    }

        .form-control:focus {
            border-color: #80bdff;
            outline: none;
            box-shadow: 0 0 0 0.2rem rgba(0, 123, 255, 0.25);
        }

    .btn-block {
        width: 100%;
        padding: 10px;
        font-size: 16px;
    }

    .btn-primary {
        background-color: #007bff;
        border: none;
    }

        .btn-primary:hover {
            background-color: #0056b3;
        }

    .btn-danger {
        background-color: #dc3545;
        border: none;
    }

        .btn-danger:hover {
            background-color: #c82333;
        }

    .spinner-border {
        margin-right: 8px;
    }

    .error-feedback {
        color: red;
        font-size: 13px;
    }

    .alert {
        margin-top: 10px;
    }

    .text-success {
        color: green;
        font-weight: bold;
    }

    @media (max-width: 576px) {
        .card-container {
            padding: 15px;
            max-width: 100%;
        }
    }
</style>
