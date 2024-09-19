import { createRouter, createWebHashHistory } from 'vue-router'
import HomeView from '../views/HomeView.vue'
import ForumView from '../views/ForumView.vue'
import Login from '../components/Login.vue';
import Register from '../components/Register.vue';

const routes = [
    {
        path: '/',
        name: 'home',
        component: HomeView
    },

    {
        path: '/forum',
        name: 'forum',
        component: ForumView
    },
    {
        path: '/login',
        name: 'login',
        component: Login
    },
     {
        path: '/register',
        name: 'register',
        component: Register
    }
]

const router = createRouter({
    history: createWebHashHistory(),
    routes
})

export default router