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