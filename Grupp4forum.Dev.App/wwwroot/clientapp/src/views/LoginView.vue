<template>
  <div>
    <h2>Login</h2>
    <form @submit.prevent="handleLogin">
      <div class="form-group">
        <label for="email">Användarnamn</label>
        <input type="email" id="email" v-model="email" required />
      </div>
      <div class="form-group">
        <label for="password">Lösenord</label>
        <input type="password" id="password" v-model="password" required />
      </div>
      <button type="submit">Login</button>
    </form>
  </div>
</template>

<script lang="ts">
import { defineComponent } from 'vue';

export default defineComponent({
  data() {
    return {
      email: '',
      password: ''
    };
  },
  methods: {
    handleLogin() {
      console.log('Login attempt with', this.email, this.password);
    }
  }
});
</script>

<style scoped>
/* Adjusting the form */
.form-group {
  margin-bottom: 20px;
}

label {
  display: block;
  margin-bottom: 5px; /* This moves the label above the input */
  font-weight: bold; /* Optional: makes the label text bold */
}

input {
  width: 350px;
  padding: 8px;
  box-sizing: border-box; /* Ensures padding doesn't break layout */
  margin-top: 5px;
}

button {
  padding: 10px;
  background-color: #42b983;
  color: white;
  border: none;
  cursor: pointer;
  width: 23%;
}

button:hover {
  background-color: #38a172;
}
</style>
