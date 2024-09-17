import { createRouter, createWebHashHistory } from 'vue-router'
import HomeView from '../views/HomeView.vue'
import ForumView from '../views/ForumView.vue'
import LoginView from '../views/LoginView.vue';

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
        component: LoginView  
    }
]

const router = createRouter({
  history: createWebHashHistory(),
  routes
})

export default router
